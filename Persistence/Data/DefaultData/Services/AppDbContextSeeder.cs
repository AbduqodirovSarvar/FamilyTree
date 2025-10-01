using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.DefaultData.Services
{
    public static class AppDbContextSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IHashService hashService)
        {
            if (!await context.UserRoles.AnyAsync())
            {
                var adminRole = DefaultUserRole.Instance;
                await context.UserRoles.AddRangeAsync(adminRole);
            }

            if(!await context.UserRolePermissions.AnyAsync())
            {
                var adminRolePermissions = DefaultUserRolePermissions.Instances;
                await context.UserRolePermissions.AddRangeAsync(adminRolePermissions);
            }

            if (!await context.Users.AnyAsync())
            {
                var admin = DefaultUser.Instance;
                admin.PasswordHash = hashService.Hash("Admin123!");
                await context.Users.AddAsync(admin);
            }

            await context.SaveChangesAsync();
        }

        public static void Seed(AppDbContext context, IHashService hashService)
        {
            if (!context.UserRoles.Any())
            {
                var adminRole = DefaultUserRole.Instance;
                context.UserRoles.AddRange(adminRole);
            }

            if (!context.UserRolePermissions.Any())
            {
                var adminRolePermissions = DefaultUserRolePermissions.Instances;
                context.UserRolePermissions.AddRange(adminRolePermissions);
            }

            if (!context.Users.Any())
            {
                var admin = DefaultUser.Instance;
                admin.PasswordHash = hashService.Hash("Admin123!");
                context.Users.Add(admin);
            }

            context.SaveChanges();
        }
    }
}
