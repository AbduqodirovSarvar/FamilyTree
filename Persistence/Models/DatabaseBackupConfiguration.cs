namespace Persistence.Models
{
    /// <summary>
    /// Bound from the <c>DatabaseBackup</c> section of appsettings.json.
    /// Defaults assume the FamilyTree backend container has
    /// <c>postgresql-client</c> installed (Dockerfile updates pg_dump path).
    /// </summary>
    public sealed class DatabaseBackupConfiguration
    {
        public const string SectionName = "DatabaseBackup";

        /// <summary>
        /// Path to the <c>pg_dump</c> executable. <c>pg_dump</c> alone
        /// works when it's on $PATH (Linux/Docker). On Windows dev boxes
        /// override to e.g. <c>C:\Program Files\PostgreSQL\16\bin\pg_dump.exe</c>.
        /// </summary>
        public string PgDumpPath { get; set; } = "pg_dump";

        /// <summary>
        /// Directory used for the temporary dump file before it's streamed
        /// out and deleted. Should be writable by the app process; default
        /// works in both Linux (<c>/tmp</c>) and Windows containers.
        /// </summary>
        public string TempDirectory { get; set; } = "/tmp";

        /// <summary>
        /// How long to wait for pg_dump to finish before killing the
        /// process. 5 minutes covers a few-hundred-MB DB; bump for larger
        /// datasets.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;
    }
}
