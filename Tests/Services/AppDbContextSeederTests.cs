using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.Data.DefaultData;
using Persistence.Data.DefaultData.Services;
using Persistence.Data.Interceptors;

namespace FamilyTree.Tests.Services;

/// <summary>
/// The seeder has regressed twice: first it skipped the whole permissions
/// block when any rows existed, then it tried to insert NEW_USER permissions
/// using a fresh-per-process role GUID that didn't match the row already in
/// the DB. These tests pin both regressions plus the empty-DB happy path.
/// </summary>
public class AppDbContextSeederTests
{
    private static AppDbContext NewInMemoryContext()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var interceptor = new AuditableEntitySaveChangesInterceptor(services);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"seeder-{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options, interceptor);
    }

    private static IHashService MockHash()
    {
        var m = new Mock<IHashService>();
        m.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"H({p})");
        return m.Object;
    }

    [Fact]
    public async Task SeedAsync_EmptyDb_CreatesRolesPermissionsAndAdmin()
    {
        await using var ctx = NewInMemoryContext();

        await AppDbContextSeeder.SeedAsync(ctx, MockHash());

        ctx.UserRoles.Should().HaveCount(2);
        ctx.UserRoles.Select(r => r.DesignedName).Should().BeEquivalentTo("ADMIN", "NEW_USER");
        ctx.UserRolePermissions.Should().HaveCount(
            DefaultUserRolePermissions.AdminPermissions.Count +
            DefaultUserRolePermissions.NewUserPermissions.Count);
        ctx.Users.Should().HaveCount(1);
        ctx.Users.Single().UserName.Should().Be("admin");
    }

    [Fact]
    public async Task SeedAsync_AdminPermissionsExist_TopsUpNewUserPermissions()
    {
        // Production state pre-fix: rows existed for admin, none for NEW_USER.
        await using var ctx = NewInMemoryContext();
        ctx.UserRoles.AddRange(DefaultUserRole.Instance);
        await ctx.SaveChangesAsync();
        var adminRoleId = ctx.UserRoles.Single(r => r.DesignedName == "ADMIN").Id;
        ctx.UserRolePermissions.AddRange(
            Enum.GetValues<Permission>().Select(p => new UserRolePermission
            {
                Id = Guid.NewGuid(),
                UserRoleId = adminRoleId,
                Permission = p
            }));
        await ctx.SaveChangesAsync();

        await AppDbContextSeeder.SeedAsync(ctx, MockHash());

        var newUserRoleId = ctx.UserRoles.Single(r => r.DesignedName == "NEW_USER").Id;
        var newUserPerms = await ctx.UserRolePermissions
            .Where(p => p.UserRoleId == newUserRoleId)
            .Select(p => p.Permission)
            .ToListAsync();

        newUserPerms.Should().Contain(Permission.GET_MEMBER, "NEW_USER must be able to read members");
        newUserPerms.Should().Contain(Permission.CREATE_FAMILY);
        newUserPerms.Should().NotContain(Permission.CREATE_ROLE, "non-admins must not manage roles");
    }

    [Fact]
    public async Task SeedAsync_RoleGuidDifferentFromStaticInstance_StillTopsUp()
    {
        // Reproduces the FK-violation bug: pre-existing roles in the DB have
        // GUIDs unrelated to whatever the static `DefaultUserRole.Instance`
        // generated this process. Seeder must resolve role IDs from the DB
        // by DesignedName, not blindly trust the static field.
        await using var ctx = NewInMemoryContext();
        ctx.UserRoles.AddRange(
            new UserRole { Id = Guid.NewGuid(), Name = "Admin",    DesignedName = "ADMIN" },
            new UserRole { Id = Guid.NewGuid(), Name = "New user", DesignedName = "NEW_USER" }
        );
        await ctx.SaveChangesAsync();
        var dbAdminId = ctx.UserRoles.Single(r => r.DesignedName == "ADMIN").Id;
        var dbNewUserId = ctx.UserRoles.Single(r => r.DesignedName == "NEW_USER").Id;
        // Confirm the test's pre-condition: DB GUIDs differ from the static
        // instance the seeder used to rely on.
        dbAdminId.Should().NotBe(DefaultUserRole.Instance[0].Id);
        dbNewUserId.Should().NotBe(DefaultUserRole.Instance[1].Id);

        // Should not throw FK violation.
        await AppDbContextSeeder.SeedAsync(ctx, MockHash());

        // Permissions land under the *DB* role IDs, not the static ones.
        (await ctx.UserRolePermissions.CountAsync(p => p.UserRoleId == dbAdminId))
            .Should().Be(DefaultUserRolePermissions.AdminPermissions.Count);
        (await ctx.UserRolePermissions.CountAsync(p => p.UserRoleId == dbNewUserId))
            .Should().Be(DefaultUserRolePermissions.NewUserPermissions.Count);
        // Admin user references the real ADMIN row, not the stale static one.
        ctx.Users.Single().RoleId.Should().Be(dbAdminId);
    }

    [Fact]
    public async Task SeedAsync_RunsTwice_NoDuplicateRows()
    {
        // Idempotency — the seeder runs on every container start.
        await using var ctx = NewInMemoryContext();
        await AppDbContextSeeder.SeedAsync(ctx, MockHash());
        var firstCount = await ctx.UserRolePermissions.CountAsync();

        await AppDbContextSeeder.SeedAsync(ctx, MockHash());
        var secondCount = await ctx.UserRolePermissions.CountAsync();

        secondCount.Should().Be(firstCount);
        (await ctx.Users.CountAsync()).Should().Be(1);
        (await ctx.UserRoles.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task SeedAsync_AdminUserGetsHashedPassword()
    {
        await using var ctx = NewInMemoryContext();

        await AppDbContextSeeder.SeedAsync(ctx, MockHash());

        var admin = ctx.Users.Single();
        admin.PasswordHash.Should().Be($"H({DefaultUser.DefaultPassword})");
    }
}
