using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FamilyTree.Tests.Helpers;
using MediatR;
using FamilyEntity = Domain.Entities.Family;
using MemberEntity = Domain.Entities.Member;

namespace FamilyTree.Tests.Services;

/// <summary>
/// Pins MemberService scoping rules — analogous to the family-side rules:
///   - List: admin sees every member; everyone else only sees members of
///     families they own (via `m.Family.OwnerId == currentUser.Id`).
///   - Create / Update / Delete: caller must own the target family,
///     regardless of role.
/// </summary>
public class MemberServiceOwnershipTests
{
    private readonly Mock<IMemberRepository> _members = new();
    private readonly Mock<IFamilyRepository> _families = new();
    private readonly Mock<IUserRoleRepository> _roles = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMapper> _mapper = new();

    /// <summary>
    /// MemberService is `internal`; instantiate via reflection so the test
    /// project doesn't have to change visibility on the production class.
    /// </summary>
    private IMemberService CreateSut()
    {
        var ctor = typeof(MemberService).GetConstructors()[0];
        return (IMemberService)ctor.Invoke(new object[]
        {
            _members.Object,
            _families.Object,
            _roles.Object,
            _userService.Object,
            _permissions.Object,
            _mediator.Object,
            _mapper.Object
        });
    }

    private void GivenAdmin(User user)
    {
        _roles.Setup(r => r.GetByIdAsync(user.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.UserRole(designedName: "ADMIN"));
    }

    private void GivenNewUser(User user)
    {
        _roles.Setup(r => r.GetByIdAsync(user.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.UserRole(designedName: "NEW_USER"));
    }

    [Fact]
    public async Task GetAll_AsAdmin_DoesNotInjectOwnershipPredicate()
    {
        var admin = TestData.User();
        GivenAdmin(admin);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _permissions.Setup(p => p.CheckPermission("Member", OperationType.GET, It.IsAny<User?>())).ReturnsAsync(true);
        _members.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<MemberEntity, bool>>?>(), 0, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<MemberEntity>(), 0));
        _mapper.Setup(m => m.Map<List<MemberViewModel>>(It.IsAny<List<MemberEntity>>()))
            .Returns(new List<MemberViewModel>());
        var sut = CreateSut();

        await sut.GetAllAsync(null, 0, 10);

        _members.Verify(r => r.GetPaginatedAsync(
            null!, 0, 10, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_AsNewUser_FiltersByFamilyOwnerId()
    {
        var user = TestData.User();
        GivenNewUser(user);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _permissions.Setup(p => p.CheckPermission("Member", OperationType.GET, It.IsAny<User?>())).ReturnsAsync(true);

        Expression<Func<MemberEntity, bool>>? captured = null;
        _members.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<MemberEntity, bool>>?>(), 0, 10, null, It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<MemberEntity, bool>>?, int, int, Func<IQueryable<MemberEntity>, IOrderedQueryable<MemberEntity>>?, CancellationToken>(
                (p, _, _, _, _) => captured = p)
            .ReturnsAsync((new List<MemberEntity>(), 0));
        _mapper.Setup(m => m.Map<List<MemberViewModel>>(It.IsAny<List<MemberEntity>>()))
            .Returns(new List<MemberViewModel>());
        var sut = CreateSut();

        await sut.GetAllAsync(null, 0, 10);

        captured.Should().NotBeNull();
        // Ownership predicate accepts members whose Family.OwnerId matches and rejects others.
        var ownFamily = TestData.Family(ownerId: user.Id);
        var otherFamily = TestData.Family(ownerId: Guid.NewGuid());
        var mine  = TestData.Member();  mine.Family  = ownFamily;
        var other = TestData.Member();  other.Family = otherFamily;
        var noFamily = TestData.Member(); noFamily.Family = null;

        var compiled = captured!.Compile();
        compiled(mine).Should().BeTrue();
        compiled(other).Should().BeFalse();
        compiled(noFamily).Should().BeFalse(); // null Family treated as not-owned.
    }

    [Fact]
    public async Task Create_TargetFamilyNotOwned_Throws()
    {
        var user = TestData.User();
        GivenNewUser(user);
        var someoneElsesFamily = TestData.Family(ownerId: Guid.NewGuid());
        _permissions.Setup(p => p.CheckPermission("Member", OperationType.CREATE, It.IsAny<User?>())).ReturnsAsync(true);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _families.Setup(r => r.GetByIdAsync(someoneElsesFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(someoneElsesFamily);
        var sut = CreateSut();

        var act = () => sut.CreateAsync(new CreateMemberDto { FamilyId = someoneElsesFamily.Id });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _members.Verify(r => r.CreateAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_AsAdminButTargetFamilyNotOwned_StillRejected()
    {
        // Admin status doesn't bypass the owner check on writes — same rule as family.
        var admin = TestData.User();
        GivenAdmin(admin);
        var notMineFamily = TestData.Family(ownerId: Guid.NewGuid());
        var member = TestData.Member(familyId: notMineFamily.Id);
        _permissions.Setup(p => p.CheckPermission("Member", OperationType.UPDATE, It.IsAny<User?>())).ReturnsAsync(true);
        _members.Setup(r => r.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _families.Setup(r => r.GetByIdAsync(notMineFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notMineFamily);
        var sut = CreateSut();

        var act = () => sut.UpdateAsync(new UpdateMemberDtoStub { Id = member.Id });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsBeforeRepoDelete()
    {
        var user = TestData.User();
        GivenNewUser(user);
        var notMine = TestData.Family(ownerId: Guid.NewGuid());
        var member = TestData.Member(familyId: notMine.Id);
        _permissions.Setup(p => p.CheckPermission("Member", OperationType.DELETE, It.IsAny<User?>())).ReturnsAsync(true);
        _members.Setup(r => r.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _families.Setup(r => r.GetByIdAsync(notMine.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notMine);
        var sut = CreateSut();

        var act = () => sut.DeleteAsync(member.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _members.Verify(r => r.DeleteAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>UpdateMemberDto is abstract — minimal concrete subclass for the test.</summary>
    private sealed record UpdateMemberDtoStub : UpdateMemberDto;
}
