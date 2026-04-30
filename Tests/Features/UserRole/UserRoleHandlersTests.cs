using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UserRole.Commands.Create;
using Application.Features.UserRole.Commands.Delete;
using Application.Features.UserRole.Commands.Update;
using Application.Features.UserRole.Queries.CheckExist;
using Application.Features.UserRole.Queries.GetList;
using Application.Features.UserRole.Queries.GetOne;
using UserRoleEntity = Domain.Entities.UserRole;

namespace FamilyTree.Tests.Features.UserRole;

public class UserRoleHandlersTests
{
    private readonly Mock<IUserRoleService> _service = new();

    [Fact]
    public async Task Create_HappyPath_ReturnsOk()
    {
        var vm = new UserRoleViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new CreateUserRoleCommandHandler(_service.Object);

        var response = await sut.Handle(new CreateUserRoleCommand(), default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new UserRoleViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateUserRoleCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateUserRoleCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteUserRoleCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteUserRoleCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetOne_HappyPath_ReturnsVm()
    {
        var vm = new UserRoleViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetUserRoleQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUserRoleQuery { Id = vm.Id }, default);

        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<UserRoleViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<UserRoleEntity, bool>>>(),
                                          It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<UserRoleViewModel>>.Ok(vms, 0, 20, 1));
        var sut = new GetUserRoleListQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUserRoleListQuery(), default);

        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckExist_AllFiltersEmpty_ReturnsFail()
    {
        var sut = new CheckUserRoleExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserRoleExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithDesignedName_ReturnsExistsResult()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<UserRoleEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckUserRoleExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserRoleExistQuery { DesignedName = "ADMIN" }, default);

        response.Data.Should().BeTrue();
    }
}
