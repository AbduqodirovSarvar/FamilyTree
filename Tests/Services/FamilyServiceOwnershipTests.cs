using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FamilyTree.Tests.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FamilyEntity = Domain.Entities.Family;

namespace FamilyTree.Tests.Services;

/// <summary>
/// Pins the family-scoping rules added per product spec:
///   - List: admins see everything; everyone else only sees their own.
///   - Update / Delete: owner-only, even for admins.
/// Reflection is used to construct the internal class because all of its
/// dependencies are interfaces and the class itself isn't public.
/// </summary>
public class FamilyServiceOwnershipTests
{
    private readonly Mock<IFamilyRepository> _families = new();
    private readonly Mock<IUserRoleRepository> _roles = new();
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMapper> _mapper = new();

    private object CreateSut()
    {
        // FamilyService is `internal` — instantiate via the same trick the DI
        // container uses (ActivatorUtilities resolves the public ctor).
        var sp = new ServiceCollection().BuildServiceProvider();
        var ftype = typeof(FamilyService);
        var ctor = ftype.GetConstructors()[0];
        return ctor.Invoke(new object[]
        {
            _families.Object,
            _roles.Object,
            _permissions.Object,
            _userService.Object,
            _mediator.Object,
            _mapper.Object
        });
    }

    private static IFamilyService AsService(object sut) => (IFamilyService)sut;

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
        _permissions.Setup(p => p.CheckPermission("Family", OperationType.GET, It.IsAny<User?>())).ReturnsAsync(true);
        _families.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<FamilyEntity, bool>>?>(), 0, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<FamilyEntity>(), 0));
        _mapper.Setup(m => m.Map<List<FamilyViewModel>>(It.IsAny<List<FamilyEntity>>()))
            .Returns(new List<FamilyViewModel>());
        var sut = AsService(CreateSut());

        await sut.GetAllAsync(null, 0, 10);

        // Admin path leaves the predicate untouched (null since the caller passed null).
        _families.Verify(r => r.GetPaginatedAsync(
            null!, 0, 10, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_AsNewUser_FiltersByOwnerId()
    {
        var user = TestData.User();
        GivenNewUser(user);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _permissions.Setup(p => p.CheckPermission("Family", OperationType.GET, It.IsAny<User?>())).ReturnsAsync(true);

        Expression<Func<FamilyEntity, bool>>? capturedPredicate = null;
        _families.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<FamilyEntity, bool>>?>(), 0, 10, null, It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<FamilyEntity, bool>>?, int, int, Func<IQueryable<FamilyEntity>, IOrderedQueryable<FamilyEntity>>?, CancellationToken>(
                (p, _, _, _, _) => capturedPredicate = p)
            .ReturnsAsync((new List<FamilyEntity>(), 0));
        _mapper.Setup(m => m.Map<List<FamilyViewModel>>(It.IsAny<List<FamilyEntity>>()))
            .Returns(new List<FamilyViewModel>());
        var sut = AsService(CreateSut());

        await sut.GetAllAsync(null, 0, 10);

        capturedPredicate.Should().NotBeNull();
        // Verify the predicate matches owned family and rejects others.
        var compiled = capturedPredicate!.Compile();
        compiled(TestData.Family(ownerId: user.Id)).Should().BeTrue();
        compiled(TestData.Family(ownerId: Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public async Task Update_AsAdminButNotOwner_StillRejected()
    {
        // Per spec: admin status does NOT bypass the owner check on writes.
        var admin = TestData.User();
        GivenAdmin(admin);
        var someoneElsesFamily = TestData.Family(ownerId: Guid.NewGuid());
        _permissions.Setup(p => p.CheckPermission("Family", OperationType.UPDATE, It.IsAny<User?>())).ReturnsAsync(true);
        _families.Setup(r => r.GetByIdAsync(someoneElsesFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(someoneElsesFamily);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        var sut = AsService(CreateSut());

        var act = () => sut.UpdateAsync(new UpdateFamilyCommandLikeDto { Id = someoneElsesFamily.Id });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_AsOwner_Allowed()
    {
        var owner = TestData.User();
        GivenNewUser(owner);
        var ownFamily = TestData.Family(ownerId: owner.Id);
        _permissions.Setup(p => p.CheckPermission("Family", OperationType.DELETE, It.IsAny<User?>())).ReturnsAsync(true);
        _families.Setup(r => r.GetByIdAsync(ownFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownFamily);
        _families.Setup(r => r.DeleteAsync(ownFamily, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        var sut = AsService(CreateSut());

        var ok = await sut.DeleteAsync(ownFamily.Id);

        ok.Should().BeTrue();
        _families.Verify(r => r.DeleteAsync(ownFamily, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsBeforeRepoDelete()
    {
        var user = TestData.User();
        GivenNewUser(user);
        var notMine = TestData.Family(ownerId: Guid.NewGuid());
        _permissions.Setup(p => p.CheckPermission("Family", OperationType.DELETE, It.IsAny<User?>())).ReturnsAsync(true);
        _families.Setup(r => r.GetByIdAsync(notMine.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notMine);
        _userService.Setup(u => u.GetCurrentUser(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = AsService(CreateSut());

        var act = () => sut.DeleteAsync(notMine.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _families.Verify(r => r.DeleteAsync(It.IsAny<FamilyEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Concrete subclass of UpdateFamilyDto so the abstract base can be
    /// instantiated for the Update test.
    /// </summary>
    private sealed record UpdateFamilyCommandLikeDto : UpdateFamilyDto;
}
