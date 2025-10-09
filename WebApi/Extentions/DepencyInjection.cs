using Application.Common.Interfaces;
using Application.Extentions;
using Microsoft.OpenApi.Models;
using Persistence.Extentions;
using Persistence.Services;

namespace WebApi.Extentions
{
    public static class DepencyInjection
    {
        public static void AdApiDepencies(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddApplicationDepencies();
            builder.Services.AddInfrastructureDepencies(configuration);
            builder.Services.AddSwagger();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
            }
            builder.Environment.WebRootPath = webRootPath;
        }

        private static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Family Tree website's Api",
                    Description = "An ASP.NET Core Web API for FamilyTree website Api items"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer Authentication",
                    Type = SecuritySchemeType.Http
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });

            services.AddCors(o => o.AddPolicy("AddCors", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            return services;
        }
    }
}
