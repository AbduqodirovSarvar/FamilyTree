using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.DefaultData.Services;

namespace WebApi.Extentions
{
    public static class WebAppExtention
    {
        public static async Task AddWebAppExtention(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyTree API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AddCors");
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
