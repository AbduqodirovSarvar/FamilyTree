using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.Family.Commands.Create;
using Application.Features.Family.Commands.Delete;
using Application.Features.Family.Commands.Update;
using Application.Features.Family.Queries.CheckExist;
using Application.Features.Family.Queries.GetList;
using Application.Features.Family.Queries.GetOne;
using FamilyEntity = Domain.Entities.Family;

namespace FamilyTree.Tests.Features.Family;

public class FamilyHandlersTests
{
    private readonly Mock<IFamilyService> _service = new();

    // ─── Create ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_HappyPath_ReturnsOk()
    {
        var vm = new FamilyViewModel { Id = Guid.NewGuid(), Name = "X", FamilyName = "X" };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateFamilyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new CreateFamilyCommandHandler(_service.Object);

        var response = await sut.Handle(new CreateFamilyCommand { Name = "X", FamilyName = "X" }, default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_Throws()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateFamilyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyViewModel?)null!);
        var sut = new CreateFamilyCommandHandler(_service.Object);

        var act = () => sut.Handle(new CreateFamilyCommand { Name = "X", FamilyName = "X" }, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── Update ──────────────────────────────────────────────────

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new FamilyViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateFamilyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateFamilyCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateFamilyCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    // ─── Delete ──────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteFamilyCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteFamilyCommand { Id = id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Failure_ReturnsFail()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new DeleteFamilyCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteFamilyCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeFalse();
    }

    // ─── GetOne (by Id and by FamilyName) ────────────────────────

    [Fact]
    public async Task GetOne_ById_DispatchesGetByIdAsync()
    {
        var id = Guid.NewGuid();
        var vm = new FamilyViewModel { Id = id };
        _service.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetFamilyQueryHandler(_service.Object);

        var response = await sut.Handle(new GetFamilyQuery { Id = id }, default);

        response.Data.Should().BeSameAs(vm);
        _service.Verify(s => s.GetAsync(It.IsAny<Expression<Func<FamilyEntity, bool>>>(),
                                        It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOne_ByFamilyName_DispatchesGetAsyncWithPredicate()
    {
        var vm = new FamilyViewModel { Id = Guid.NewGuid(), FamilyName = "Karimov" };
        _service.Setup(s => s.GetAsync(It.IsAny<Expression<Func<FamilyEntity, bool>>>(),
                                       It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetFamilyQueryHandler(_service.Object);

        var response = await sut.Handle(new GetFamilyQuery { Id = Guid.Empty, FamilyName = "Karimov" }, default);

        response.Data!.FamilyName.Should().Be("Karimov");
    }

    [Fact]
    public async Task GetOne_NotFound_Throws()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyViewModel?)null);
        var sut = new GetFamilyQueryHandler(_service.Object);

        var act = () => sut.Handle(new GetFamilyQuery { Id = Guid.NewGuid() }, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── GetList ─────────────────────────────────────────────────

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<FamilyViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<FamilyEntity, bool>>>(),
                                          0, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<FamilyViewModel>>.Ok(vms, 0, 5, 1));
        var sut = new GetFamilyListQueryHandler(_service.Object);

        var response = await sut.Handle(new GetFamilyListQuery { PageIndex = 0, PageSize = 5 }, default);

        response.Data.Should().HaveCount(1);
        response.PageSize.Should().Be(5);
    }

    // ─── CheckExist ──────────────────────────────────────────────

    [Fact]
    public async Task CheckExist_NoIdAndNoName_ReturnsFail()
    {
        var sut = new CheckFamilyExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckFamilyExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithFamilyName_QueriesByName()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<FamilyEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckFamilyExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckFamilyExistQuery { FamilyName = "Karimov" }, default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeTrue();
    }
}
