using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Auth.Queries.GetMyPermissions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.Data.Interceptors;
using Persistence.Services.Repositories;
using FamilyTree.Tests.Helpers;
using UserEntity = Domain.Entities.User;
using UserRolePermissionEntity = Domain.Entities.UserRolePermission;

namespace FamilyTree.Tests.Features.Auth;

/// <summary>
/// Drives the handler through a real in-memory AppDbContext + repository
/// because <c>EntityFrameworkQueryableExtensions.ToListAsync</c> on the
/// IQueryable returned by `Query()` requires EF's async provider — a plain
/// Moq stub of IUserRolePermissionRepository doesn't satisfy it.
/// </summary>
public class GetMyPermissionsQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static (AppDbContext, IUserRolePermissionRepository) NewContext()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var interceptor = new AuditableEntitySaveChangesInterceptor(services);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"perm-handler-{Guid.NewGuid()}")
            .Options;
        var ctx = new AppDbContext(options, interceptor);
        var redisStub = new Mock<IRedisService>().Object;
        return (ctx, new UserRolePermissionRepository(ctx, redisStub));
    }

    [Fact]
    public async Task Handle_NotAuthenticated_Throws()
    {
        var (ctx, repo) = NewContext();
        await using var _ = ctx;
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var sut = new GetMyPermissionsQueryHandler(_currentUser.Object, repo);

        var act = () => sut.Handle(new GetMyPermissionsQuery(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ReturnsPermissionsForUsersRole()
    {
        var (ctx, repo) = NewContext();
        await using var _ = ctx;

        var roleId = Guid.NewGuid();
        var otherRoleId = Guid.NewGuid();
        var user = TestData.User(roleId: roleId);

        ctx.UserRolePermissions.AddRange(
            new UserRolePermissionEntity { Id = Guid.NewGuid(), UserRoleId = roleId,      Permission = Permission.GET_FAMILY },
            new UserRolePermissionEntity { Id = Guid.NewGuid(), UserRoleId = roleId,      Permission = Permission.CREATE_MEMBER },
            // Permission for a different role — must not leak into the response.
            new UserRolePermissionEntity { Id = Guid.NewGuid(), UserRoleId = otherRoleId, Permission = Permission.DELETE_USER }
        );
        await ctx.SaveChangesAsync();

        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = new GetMyPermissionsQueryHandler(_currentUser.Object, repo);

        var response = await sut.Handle(new GetMyPermissionsQuery(), default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(new[] { "GET_FAMILY", "CREATE_MEMBER" });
    }

    [Fact]
    public async Task Handle_RoleHasNoPermissions_ReturnsEmptyList()
    {
        var (ctx, repo) = NewContext();
        await using var _ = ctx;
        var user = TestData.User(roleId: Guid.NewGuid());
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = new GetMyPermissionsQueryHandler(_currentUser.Object, repo);

        var response = await sut.Handle(new GetMyPermissionsQuery(), default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PermissionStringsMatchEnumNames()
    {
        // Wire-format guarantee — the SPA matches against names like "GET_USER".
        // If the projection ever switched to ints, the frontend would silently
        // hide everything. This test pins the representation.
        var (ctx, repo) = NewContext();
        await using var _ = ctx;
        var roleId = Guid.NewGuid();
        var user = TestData.User(roleId: roleId);
        ctx.UserRolePermissions.Add(
            new UserRolePermissionEntity { Id = Guid.NewGuid(), UserRoleId = roleId, Permission = Permission.GET_USER });
        await ctx.SaveChangesAsync();
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = new GetMyPermissionsQueryHandler(_currentUser.Object, repo);

        var response = await sut.Handle(new GetMyPermissionsQuery(), default);

        response.Data.Should().ContainSingle().Which.Should().Be("GET_USER");
    }
}
