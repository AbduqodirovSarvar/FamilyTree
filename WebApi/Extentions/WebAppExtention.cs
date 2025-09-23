using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.DefaultData.Services;

namespace WebApi.Extentions
{
    public static class WebAppExtention
    {
        public static WebApplication AddWebAppExtention(this WebApplication app)
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

            app.MapControllers();


            //app.UpdateMigration();
            //app.Seed();

            return app;
        }

        private static void UpdateMigration(this WebApplication app)
        {
            Console.WriteLine("Update MIgration boshlanmoqda...");
            using var scope = app.Services.CreateScope();
            Console.WriteLine("SCOPE...");
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Console.WriteLine("DBCONTEXT...");
                dbContext.Database.Migrate();
                Console.WriteLine("MIGRATE...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Migrationda xato: " + ex.Message);
                throw;
            }
        }

        private static void Seed(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
                AppDbContextSeeder.Seed(context, hashService);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Seedda xato: " + ex.Message);
                throw;
            }
        }

    }
}
