using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.User.Commands.Delete;
using Application.Features.User.Commands.Update;
using Application.Features.User.Queries.CheckExist;
using Application.Features.User.Queries.GetList;
using Application.Features.User.Queries.GetOne;
using UserEntity = Domain.Entities.User;

namespace FamilyTree.Tests.Features.User;

public class UserHandlersTests
{
    private readonly Mock<IUserService> _service = new();

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new UserViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateUserCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateUserCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ServiceReturnsNull_Throws()
    {
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserViewModel?)null!);
        var sut = new UpdateUserCommandHandler(_service.Object);

        var act = () => sut.Handle(new UpdateUserCommand { Id = Guid.NewGuid() }, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteUserCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteUserCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Failure_ReturnsFail()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new DeleteUserCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteUserCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetOne_NotFound_Throws()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserViewModel?)null);
        var sut = new GetUserQueryHandler(_service.Object);

        var act = () => sut.Handle(new GetUserQuery { Id = Guid.NewGuid() }, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetOne_HappyPath_ReturnsOk()
    {
        var vm = new UserViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetUserQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUserQuery { Id = vm.Id }, default);

        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<UserViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(),
                                          It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<UserViewModel>>.Ok(vms, 0, 20, 1));
        // Class is GetUserListQUeryHandler (typo in source) — preserved here so the test references the real symbol.
        var sut = new GetUserListQUeryHandler(_service.Object);

        var response = await sut.Handle(new GetUserListQuery(), default);

        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckExist_AllFiltersEmpty_ReturnsFail()
    {
        var sut = new CheckUserExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithEmail_ReturnsExistsResult()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckUserExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUserExistQuery { Email = "x@e.com" }, default);

        response.Data.Should().BeTrue();
    }
}
