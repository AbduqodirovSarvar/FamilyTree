using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.Member.Commands.Create;
using Application.Features.Member.Commands.Delete;
using Application.Features.Member.Commands.Update;
using Application.Features.Member.Queries.CheckExist;
using Application.Features.Member.Queries.GetList;
using Application.Features.Member.Queries.GetOne;
using MemberEntity = Domain.Entities.Member;

namespace FamilyTree.Tests.Features.Member;

public class MemberHandlersTests
{
    private readonly Mock<IMemberService> _service = new();

    [Fact]
    public async Task Create_HappyPath_ReturnsOk()
    {
        var vm = new MemberViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new CreateMemberCommandHandler(_service.Object);

        var response = await sut.Handle(new CreateMemberCommand(), default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_Throws()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberViewModel?)null!);
        var sut = new CreateMemberCommandHandler(_service.Object);

        var act = () => sut.Handle(new CreateMemberCommand(), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new MemberViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateMemberCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateMemberCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteMemberCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteMemberCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Failure_ReturnsFail()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new DeleteMemberCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteMemberCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetOne_NotFound_Throws()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberViewModel?)null);
        var sut = new GetMemberQueryHandler(_service.Object);

        var act = () => sut.Handle(new GetMemberQuery { Id = Guid.NewGuid() }, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetOne_HappyPath_ReturnsOk()
    {
        var vm = new MemberViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var sut = new GetMemberQueryHandler(_service.Object);

        var response = await sut.Handle(new GetMemberQuery { Id = vm.Id }, default);

        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<MemberViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<MemberEntity, bool>>>(),
                                          0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<MemberViewModel>>.Ok(vms, 0, 20, 1));
        var sut = new GetMemberListQueryHandler(_service.Object);

        var response = await sut.Handle(new GetMemberListQuery(), default);

        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckExist_AllFiltersEmpty_ReturnsFail()
    {
        var sut = new CheckMemberExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckMemberExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithId_QueriesService()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<MemberEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckMemberExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckMemberExistQuery { Id = Guid.NewGuid() }, default);

        response.Data.Should().BeTrue();
    }
}
