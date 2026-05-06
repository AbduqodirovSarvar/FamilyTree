using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.DefaultData.Services;
using WebApi.Middleware;

namespace WebApi.Extentions
{
    public static class WebAppExtention
    {
        public static async Task AddWebAppExtention(this WebApplication app)
        {
            // Global exception → Telegram bridge runs FIRST so it can catch
            // failures from any later middleware (auth, controllers, etc.).
            // Any exception that bubbles past UseStaticFiles / UseRouting /
            // UseAuthorization lands here and gets dispatched to the bugs
            // topic before the JSON error envelope is returned.
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyTree API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
            // Serve files from wwwroot (e.g. /uploads/<guid>.png) so the SPA can render avatars.
            app.UseStaticFiles();
            app.UseRouting();
            // CORS must run before Authentication so preflight (OPTIONS) requests
            // get the Access-Control-Allow-* headers attached even when the route
            // is [Authorize]. Wrong order is the classic cause of "blocked by CORS
            // policy: No 'Access-Control-Allow-Origin' header" errors.
            app.UseCors("AddCors");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            await app.UpdateMigration();
            await app.Seed();
        }

        private static async Task UpdateMigration(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }


        private static async Task Seed(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
            await AppDbContextSeeder.SeedAsync(context, hashService);
        }

    }
}
