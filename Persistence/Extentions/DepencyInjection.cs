using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Repositories.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.Data.Interceptors;
using Persistence.Models;
using Persistence.Services;
using Persistence.Services.Repositories;
using Persistence.Services.Repositories.Common;
using StackExchange.Redis;
using System.Text;

namespace Persistence.Extentions
{
    public static class DepencyInjection
    {
        public static IServiceCollection AddInfrastructureDepencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<IFamilyViewRepository, FamilyViewRepository>();
            // Singleton — holds the in-memory dedup buffer that
            // FamilyViewFlushService drains every few minutes. Must outlive
            // every request scope so refresh storms within a single page-load
            // collapse into one row.
            services.AddSingleton<IFamilyViewRecorder, FamilyViewRecorder>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserRolePermissionRepository, UserRolePermissionRepository>();

            services.AddSingleton<IHashService, HashService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IFileService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                if (!Directory.Exists(webRootPath))
                    Directory.CreateDirectory(webRootPath);

                return new FileService(webRootPath);
            });

            services.AddScoped<IEmailService, EmailService>();

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));
            services.AddScoped<IRedisService, RedisService>();

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
                options.UseNpgsql(configuration.GetConnectionString("PostgreSqlConnection"))
                       .AddInterceptors(interceptor);
            });

            services.Configure<JWTConfiguration>(configuration.GetSection("JWTConfiguration"));

            // Database backup — pg_dump runner. Configurable so dev boxes
            // (Windows) can point at the full pg_dump.exe path while the
            // Docker image leaves it as the default ($PATH lookup).
            services.Configure<DatabaseBackupConfiguration>(
                configuration.GetSection(DatabaseBackupConfiguration.SectionName));
            services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();

            // Notification gateway — typed HttpClient.
            // Lives in Persistence (not Application) because the HTTP transport
            // is an infrastructure concern; Application code depends only on
            // the INotificationService abstraction.
            services.Configure<NotificationGatewayConfiguration>(
                configuration.GetSection(NotificationGatewayConfiguration.SectionName));
            services.AddHttpClient<INotificationService, TelegramNotificationService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<NotificationGatewayConfiguration>>().Value;
                if (!string.IsNullOrEmpty(opts.BaseUrl))
                    client.BaseAddress = new Uri(opts.BaseUrl);
                if (!string.IsNullOrEmpty(opts.ApiKey))
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.ApiKey}");
                // Tight timeout — notification calls run inside business
                // request threads; long hangs would bubble up as slow
                // sign-up/family-create even though the call is async.
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            var secretWord = configuration["JWTConfiguration:Secret"] ?? "JWTConfiguration:Secret";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = configuration["JWTConfiguration:ValidAudience"],
                        ValidIssuer = configuration["JWTConfiguration:ValidIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretWord))
                    };
                });
            services.AddAuthorization();
            return services;
        }
    }
}
