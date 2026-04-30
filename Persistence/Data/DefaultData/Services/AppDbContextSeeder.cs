using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence.Data.DefaultData.Services
{
    public static class AppDbContextSeeder
    {
        // Stable identifier used to look up the seeded roles after they're
        // inserted, since DefaultUserRole.Instance[i].Id is regenerated each
        // process start and cannot be trusted across container restarts.
        private const string AdminRoleDesignedName = "ADMIN";
        private const string NewUserRoleDesignedName = "NEW_USER";

        public static async Task SeedAsync(AppDbContext context, IHashService hashService)
        {
            if (!await context.UserRoles.AnyAsync())
            {
                // Clone before adding — EF Core's change-tracker mutates entities
                // (populating navigation properties via fixup). Re-using the
                // static `DefaultUserRole.Instance` across processes/tests caused
                // those navs to leak into subsequent calls and re-insert ghost
                // related entities through graph discovery.
                await context.UserRoles.AddRangeAsync(DefaultUserRole.Instance.Select(CloneRole));
                await context.SaveChangesAsync();
            }

            // Permissions ─ top up per role using the actual DB-resident role
            // ids (resolved by DesignedName). Earlier this paired the static
            // `DefaultUserRole.Instance[i].Id` with permission rows, which was
            // unsafe across container restarts: a new process re-rolled the
            // GUIDs in the static fields, then attempted to insert
            // UserRolePermission rows pointing at GUIDs the existing roles
            // table didn't know about → FK violation, app crash on boot.
            await TopUpRolePermissionsAsync(context, AdminRoleDesignedName,   DefaultUserRolePermissions.AdminPermissions);
            await TopUpRolePermissionsAsync(context, NewUserRoleDesignedName, DefaultUserRolePermissions.NewUserPermissions);

            if (!await context.Users.AnyAsync())
            {
                var adminRoleId = await context.UserRoles
                    .Where(r => r.DesignedName == AdminRoleDesignedName)
                    .Select(r => r.Id)
                    .FirstAsync();
                await context.Users.AddAsync(BuildAdminUser(hashService, adminRoleId));
            }

            await context.SaveChangesAsync();
        }

        public static void Seed(AppDbContext context, IHashService hashService)
        {
            if (!context.UserRoles.Any())
            {
                context.UserRoles.AddRange(DefaultUserRole.Instance.Select(CloneRole));
                context.SaveChanges();
            }

            TopUpRolePermissions(context, AdminRoleDesignedName,   DefaultUserRolePermissions.AdminPermissions);
            TopUpRolePermissions(context, NewUserRoleDesignedName, DefaultUserRolePermissions.NewUserPermissions);

            if (!context.Users.Any())
            {
                var adminRoleId = context.UserRoles
                    .Where(r => r.DesignedName == AdminRoleDesignedName)
                    .Select(r => r.Id)
                    .First();
                context.Users.Add(BuildAdminUser(hashService, adminRoleId));
            }

            context.SaveChanges();
        }

        // ─── Fresh entity factories ──────────────────────────────────
        // The seeder never adds a static-field instance directly — fixup
        // would mutate that shared instance and leak nav properties into
        // subsequent invocations.

        private static UserRole CloneRole(UserRole template) => new()
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            DesignedName = template.DesignedName,
            FamilyId = template.FamilyId
        };

        private static User BuildAdminUser(IHashService hashService, Guid roleId)
        {
            var template = DefaultUser.Instance;
            return new User
            {
                Id = template.Id,
                FirstName = template.FirstName,
                LastName = template.LastName,
                UserName = template.UserName,
                Email = template.Email,
                Phone = template.Phone,
                RoleId = roleId,
                PasswordHash = hashService.Hash(DefaultUser.DefaultPassword)
            };
        }

        // ─── Per-role permission top-up ──────────────────────────────
        // Resolves the role's actual DB id by its DesignedName, then inserts
        // only the permissions that aren't already present. Idempotent.

        private static async Task TopUpRolePermissionsAsync(
            AppDbContext context,
            string roleDesignedName,
            IReadOnlyList<Permission> defaults)
        {
            if (defaults.Count == 0) return;

            var roleId = await context.UserRoles
                .Where(r => r.DesignedName == roleDesignedName)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
            if (roleId == Guid.Empty) return;

            var existing = await context.UserRolePermissions
                .Where(p => p.UserRoleId == roleId)
                .Select(p => p.Permission)
                .ToListAsync();

            var missing = defaults
                .Where(p => !existing.Contains(p))
                .Select(p => new UserRolePermission
                {
                    Id = Guid.NewGuid(),
                    UserRoleId = roleId,
                    Permission = p
                })
                .ToList();
            if (missing.Count > 0)
                await context.UserRolePermissions.AddRangeAsync(missing);
        }

        private static void TopUpRolePermissions(
            AppDbContext context,
            string roleDesignedName,
            IReadOnlyList<Permission> defaults)
        {
            if (defaults.Count == 0) return;

            var roleId = context.UserRoles
                .Where(r => r.DesignedName == roleDesignedName)
                .Select(r => r.Id)
                .FirstOrDefault();
            if (roleId == Guid.Empty) return;

            var existing = context.UserRolePermissions
                .Where(p => p.UserRoleId == roleId)
                .Select(p => p.Permission)
                .ToList();

            var missing = defaults
                .Where(p => !existing.Contains(p))
                .Select(p => new UserRolePermission
                {
                    Id = Guid.NewGuid(),
                    UserRoleId = roleId,
                    Permission = p
                })
                .ToList();
            if (missing.Count > 0)
                context.UserRolePermissions.AddRange(missing);
        }
    }
}
