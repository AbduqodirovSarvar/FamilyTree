using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using FamilyTree.Tests.Helpers;
using Persistence.Services;

namespace FamilyTree.Tests.Services;

public class PermissionServiceTests
{
    private readonly Mock<IUserRolePermissionRepository> _repo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private PermissionService CreateSut() => new(_repo.Object, _currentUser.Object);

    // ─── CheckPermission ──────────────────────────────────────────

    [Fact]
    public async Task CheckPermission_NoCurrentUserAndNoneProvided_Throws()
    {
        // Recent NEW_USER bug: anonymous-looking calls used to surface as 500
        // because this throws by design when no user can be resolved.
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var sut = CreateSut();

        var act = () => sut.CheckPermission("Family", OperationType.GET);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task CheckPermission_WithUser_QueriesByRoleAndPermission()
    {
        var user = TestData.User(roleId: Guid.NewGuid());
        _repo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRolePermission, bool>>>(),
                                    It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();

        var ok = await sut.CheckPermission("Family", OperationType.GET, user);

        ok.Should().BeTrue();
        _repo.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<UserRolePermission, bool>>>(),
                                     It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckPermission_RoleLacksPermission_ReturnsFalse()
    {
        var user = TestData.User(roleId: Guid.NewGuid());
        _repo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRolePermission, bool>>>(),
                                    It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = CreateSut();

        var ok = await sut.CheckPermission("Family", OperationType.GET, user);

        ok.Should().BeFalse();
    }

    // ─── GetPermission mapping ────────────────────────────────────

    public static IEnumerable<object[]> EntityOperationCases() => new[]
    {
        new object[] { "Family", OperationType.GET, Permission.GET_FAMILY },
        new object[] { "FAMILY", OperationType.CREATE, Permission.CREATE_FAMILY },
        new object[] { "family", OperationType.UPDATE, Permission.UPDATE_FAMILY },
        new object[] { "Family", OperationType.DELETE, Permission.DELETE_FAMILY },

        new object[] { "Member", OperationType.GET, Permission.GET_MEMBER },
        new object[] { "Member", OperationType.CREATE, Permission.CREATE_MEMBER },
        new object[] { "Member", OperationType.UPDATE, Permission.UPDATE_MEMBER },
        new object[] { "Member", OperationType.DELETE, Permission.DELETE_MEMBER },

        new object[] { "User", OperationType.GET, Permission.GET_USER },
        new object[] { "User", OperationType.DELETE, Permission.DELETE_USER },

        new object[] { "UserRole", OperationType.GET, Permission.GET_ROLE },
        new object[] { "UserRolePermission", OperationType.CREATE, Permission.CREATE_ROLE_PERMISSION },
        new object[] { "UploadedFile", OperationType.UPDATE, Permission.UPDATE_FILE },
    };

    [Theory]
    [MemberData(nameof(EntityOperationCases))]
    public void GetPermission_ResolvesCanonicalEntityName(string entity, OperationType op, Permission expected)
    {
        var sut = CreateSut();

        var p = sut.GetPermission(entity, op);

        p.Should().Be(expected);
    }

    [Fact]
    public void GetPermission_UnknownEntity_Throws()
    {
        var sut = CreateSut();

        var act = () => sut.GetPermission("UnknownEntity", OperationType.GET);

        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void GetPermission_UnsupportedOperationForEntity_Throws()
    {
        var sut = CreateSut();

        // OperationType.NONE has no mapping in the switch.
        var act = () => sut.GetPermission("Family", OperationType.NONE);

        act.Should().Throw<NotImplementedException>();
    }
}
