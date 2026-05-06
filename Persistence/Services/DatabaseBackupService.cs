using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence.Models;
using System.Diagnostics;
using System.IO.Compression;

namespace Persistence.Services
{
    /// <summary>
    /// Runs <c>pg_dump</c> against the FamilyTree PostgreSQL database,
    /// compresses the dump with gzip, and streams it to the notification
    /// gateway. Used by both the scheduled daily backup and the
    /// admin-triggered "send now" endpoint.
    ///
    /// <para><b>Why pg_dump and not Npgsql.</b> A logical backup needs the
    /// full custom format that pg_dump produces (proper FK ordering,
    /// extension-aware), which would be hundreds of lines to reproduce
    /// in C#. Shelling out is the boring, well-tested path.</para>
    ///
    /// <para><b>Cleanup.</b> The temp file is deleted in <c>finally</c> so
    /// a failed upload doesn't leave dumps accumulating in <c>/tmp</c>.</para>
    /// </summary>
    public sealed class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly DatabaseBackupConfiguration _options;
        private readonly INotificationService _notifications;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseBackupService> _logger;

        public DatabaseBackupService(
            IOptions<DatabaseBackupConfiguration> options,
            INotificationService notifications,
            IConfiguration configuration,
            ILogger<DatabaseBackupService> logger)
        {
            _options = options.Value;
            _notifications = notifications;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RunAndSendAsync(CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("PostgreSqlConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:PostgreSqlConnection is not configured.");

            // Parse via Npgsql to extract host/port/db/user/password without
            // string-splitting the connection string ourselves — handles
            // quoting, semicolons in passwords, etc.
            var csb = new NpgsqlConnectionStringBuilder(connectionString);

            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmm");
            var dbName = csb.Database ?? "familytree";
            var dumpFileName = $"{dbName}-{stamp}.sql.gz";
            var dumpPath = Path.Combine(_options.TempDirectory, dumpFileName);

            try
            {
                _logger.LogInformation("Starting DB backup to {Path}.", dumpPath);
                await RunPgDumpAsync(csb, dumpPath, cancellationToken);

                var size = new FileInfo(dumpPath).Length;
                _logger.LogInformation("Backup complete: {Path} ({Size} bytes). Uploading...", dumpPath, size);

                var caption = FormatCaption(dbName, stamp, size);

                // Open with FileShare.Read so a concurrent investigation
                // (`tail -c 1k file` from another shell) can't deadlock us.
                await using var stream = new FileStream(
                    dumpPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                await _notifications.SendDocumentAsync(
                    "familytree.dev.dbarxiv",
                    stream,
                    dumpFileName,
                    caption,
                    cancellationToken);

                _logger.LogInformation("Backup uploaded: {FileName}.", dumpFileName);
            }
            finally
            {
                // Cleanup runs in finally so a failed upload doesn't strand
                // the dump on disk. Errors here are non-fatal — just log.
                if (File.Exists(dumpPath))
                {
                    try { File.Delete(dumpPath); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete backup file {Path}.", dumpPath);
                    }
                }
            }
        }

        private async Task RunPgDumpAsync(
            NpgsqlConnectionStringBuilder csb,
            string outputPath,
            CancellationToken cancellationToken)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _options.PgDumpPath,
                // -Fc = custom format (compact, restorable with pg_restore).
                // We pipe stdout through GZipStream so the Telegram payload
                // is a single .gz file the user can extract with `gunzip`.
                // Using -Fp (plain SQL) instead would also work but makes
                // it harder to restore selectively.
                ArgumentList =
                {
                    "-h", csb.Host ?? "localhost",
                    "-p", (csb.Port == 0 ? 5432 : csb.Port).ToString(),
                    "-U", csb.Username ?? "",
                    "-d", csb.Database ?? "",
                    "--no-owner",
                    "--no-privileges",
                    "-Fp",                  // plain SQL — easiest to inspect/restore
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // pg_dump reads the password from the PGPASSWORD env var when
            // running non-interactively. Setting it on the child process
            // avoids both a leaky `-W` prompt and a .pgpass file on disk.
            psi.Environment["PGPASSWORD"] = csb.Password ?? "";

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException(
                    $"Failed to start pg_dump at '{_options.PgDumpPath}'.");

            // Compress on-the-fly: pg_dump.stdout → GZipStream → file.
            // Saves memory (no full dump in RAM) and saves disk space
            // (gzip typically ~5x smaller than plain SQL).
            await using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            await using (var gz = new GZipStream(fs, CompressionLevel.Optimal))
            {
                await process.StandardOutput.BaseStream.CopyToAsync(gz, cancellationToken);
            }

            // Read stderr only after stdout is consumed — readers must drain
            // both pipes to avoid the OS pipe buffer filling up and
            // deadlocking the child process.
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            await process.WaitForExitAsync(linked.Token);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"pg_dump exited with code {process.ExitCode}. stderr: {stderr}");
            }
        }

        private static string FormatCaption(string dbName, string stamp, long sizeBytes)
        {
            // Caption has a 1024-char Telegram cap; ours is well under.
            // No HTML escaping needed since we don't pass parse_mode in the
            // gateway's sendDocument flow today (kept simple — body is plain
            // text). All fields here are server-controlled, not user input.
            var sizeMb = sizeBytes / 1024.0 / 1024.0;
            return $"📦 DB backup {dbName} — {stamp} UTC ({sizeMb:F2} MB, gzip)";
        }
    }
}
