using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UserRolePermission.Commands.Create;
using Application.Features.UserRolePermission.Commands.Delete;
using Application.Features.UserRolePermission.Commands.Update;
using Application.Features.UserRolePermission.Queries.CheckExist;
using Application.Features.UserRolePermission.Queries.GetList;
using Application.Features.UserRolePermission.Queries.GetOne;
using Domain.Enums;
using UserRolePermissionEntity = Domain.Entities.UserRolePermission;

namespace FamilyTree.Tests.Features.UserRolePermission;

public class UserRolePermissionHandlersTests
{
    private readonly Mock<IUserRolePermissionService> _service = new();

    [Fact]
    public async Task Create_HappyPath_ReturnsOk()
    {
        var vm = new UserRolePermissionViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserRolePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new CreateUserRolePermissionCommandHandler(_service.Object);

        var response = await sut.Handle(new CreateUserRolePermissionCommand(), default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new UserRolePermissionViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserRolePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateUserRolePermissionCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateUserRolePermissionCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteUserRolePermissionCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteUserRolePermissionCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetOne_HappyPath_ReturnsVm()
    {
        var vm = new UserRolePermissionViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetUserRolePermissionQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUserRolePermissionQuery { Id = vm.Id }, default);

        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<UserRolePermissionViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<UserRolePermissionEntity, bool>>>(),
                                          It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<UserRolePermissionViewModel>>.Ok(vms, 0, 20, 1));
        var sut = new GetUserRolePermissionListQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUserRolePermissionListQuery(), default);

        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckExist_AllFiltersEmpty_ReturnsFail()
    {
        var sut = new CheckUserRolePermissionExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserRolePermissionExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithUserRoleIdAndPermission_ReturnsExistsResult()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<UserRolePermissionEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckUserRolePermissionExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserRolePermissionExistQuery
        {
            UserRoleId = Guid.NewGuid(),
            Permission = Permission.GET_FAMILY
        }, default);

        response.Data.Should().BeTrue();
    }
}
