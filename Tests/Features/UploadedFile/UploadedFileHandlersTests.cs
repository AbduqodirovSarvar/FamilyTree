using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using Application.Features.UploadedFile.Commands.Delete;
using Application.Features.UploadedFile.Commands.Update;
using Application.Features.UploadedFile.Queries.CheckExist;
using Application.Features.UploadedFile.Queries.GetLIst;
using Application.Features.UploadedFile.Queries.GetOne;
using Microsoft.AspNetCore.Http;
using UploadedFileEntity = Domain.Entities.UploadedFile;

namespace FamilyTree.Tests.Features.UploadedFile;

public class UploadedFileHandlersTests
{
    private readonly Mock<IUploadedFileService> _service = new();
    private readonly Mock<IFileService> _files = new();

    [Fact]
    public async Task Create_NullFile_Throws()
    {
        var sut = new CreateUploadedFileCommandHandler(_service.Object);

        var act = () => sut.Handle(new CreateUploadedFileCommand { File = null! }, default);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Create_HappyPath_ReturnsOk()
    {
        var vm = new UploadedFileViewModel { Id = Guid.NewGuid() };
        var formFile = new Mock<IFormFile>().Object;
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUploadedFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new CreateUploadedFileCommandHandler(_service.Object);

        var response = await sut.Handle(new CreateUploadedFileCommand { File = formFile }, default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeSameAs(vm);
    }

    [Fact]
    public async Task Update_HappyPath_ReturnsOk()
    {
        var vm = new UploadedFileViewModel { Id = Guid.NewGuid() };
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUploadedFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        var sut = new UpdateUploadedFileCommandHandler(_service.Object);

        var response = await sut.Handle(new UpdateUploadedFileCommand { Id = vm.Id }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Success_ReturnsOk()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteUploadedFileCommandHandler(_service.Object);

        var response = await sut.Handle(new DeleteUploadedFileCommand { Id = Guid.NewGuid() }, default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetOne_FileMissing_ThrowsFileNotFound()
    {
        var vm = new UploadedFileViewModel { Id = Guid.NewGuid(), Name = "missing.png" };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        _files.Setup(f => f.GetFileAsync("missing.png")).ReturnsAsync((byte[]?)null);
        var sut = new GetUploadedFileQueryHandler(_files.Object, _service.Object);

        var act = () => sut.Handle(new GetUploadedFileQuery { Id = vm.Id }, default);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task GetOne_HappyPath_ReturnsBytes()
    {
        var vm = new UploadedFileViewModel { Id = Guid.NewGuid(), Name = "ok.png" };
        _service.Setup(s => s.GetByIdAsync(vm.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var bytes = new byte[] { 1, 2, 3 };
        _files.Setup(f => f.GetFileAsync("ok.png")).ReturnsAsync(bytes);
        var sut = new GetUploadedFileQueryHandler(_files.Object, _service.Object);

        var result = await sut.Handle(new GetUploadedFileQuery { Id = vm.Id }, default);

        result.Should().BeSameAs(bytes);
    }

    [Fact]
    public async Task GetList_PassesPagingThrough()
    {
        var vms = new List<UploadedFileViewModel> { new() { Id = Guid.NewGuid() } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<UploadedFileEntity, bool>>>(),
                                          It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response<List<UploadedFileViewModel>>.Ok(vms, 0, 20, 1));
        var sut = new GetUploadedFileListQueryHandler(_service.Object);

        var response = await sut.Handle(new GetUploadedFileListQuery(), default);

        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckExist_AllFiltersEmpty_ReturnsFail()
    {
        var sut = new CheckUploadedFileExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUploadedFileExistQuery(), default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExist_WithUrl_ReturnsExistsResult()
    {
        _service.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<UploadedFileEntity, bool>>>(),
                                          It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new CheckUploadedFileExistQueryHandler(_service.Object);

        var response = await sut.Handle(new CheckUploadedFileExistQuery { Url = "/x.png" }, default);

        response.Data.Should().BeTrue();
    }
}
