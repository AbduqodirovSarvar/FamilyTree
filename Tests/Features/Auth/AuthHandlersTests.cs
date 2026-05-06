using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.Auth.Commands.ChangePassword;
using Application.Features.Auth.Commands.Confirmation;
using Application.Features.Auth.Commands.Reset;
using Application.Features.Auth.Commands.SignIn;
using Application.Features.Auth.Commands.SignUp;
using Application.Features.Auth.Commands.UpdateProfile;
using Application.Features.Auth.Queries.GetMe;
using Application.Features.UploadedFile.Commands.Create;
using AutoMapper;
// Alias to dodge the FamilyTree.Tests.Features.User namespace which shadows the entity.
using UserEntity = Domain.Entities.User;
using FamilyTree.Tests.Helpers;
using MediatR;

namespace FamilyTree.Tests.Features.Auth;

public class AuthHandlersTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IHashService> _hash = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<INotificationService> _notifications = new();

    // ─── SignIn ──────────────────────────────────────────────────

    [Fact]
    public async Task SignIn_HappyPath_ReturnsTokenWithSuccess()
    {
        var token = new TokenViewModel { AccessToken = "tok" };
        _auth.Setup(a => a.SignInAsync(It.IsAny<SignInCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var sut = new SignInCommandHandler(_auth.Object);

        var response = await sut.Handle(new SignInCommand { Login = "u", Password = "p" }, default);

        response.Success.Should().BeTrue();
        response.Data!.AccessToken.Should().Be("tok");
    }

    [Fact]
    public async Task SignIn_ServiceReturnsNull_Throws()
    {
        _auth.Setup(a => a.SignInAsync(It.IsAny<SignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenViewModel)null!);
        var sut = new SignInCommandHandler(_auth.Object);

        var act = () => sut.Handle(new SignInCommand { Login = "u", Password = "p" }, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── SignUp ──────────────────────────────────────────────────

    [Fact]
    public async Task SignUp_DelegatesToService_AndWrapsInResponse()
    {
        _auth.Setup(a => a.SignUpAsync(It.IsAny<SignUpCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new SignUpCommandHandler(_auth.Object, _notifications.Object);

        var response = await sut.Handle(new SignUpCommand { Password = "p", ConfirmPassword = "p" }, default);

        response.Success.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    // ─── ResetSignIn ─────────────────────────────────────────────

    [Fact]
    public async Task Reset_ServiceReturnsTrue_ReturnsOk()
    {
        _auth.Setup(a => a.ResetAsync(It.IsAny<ResetSignInCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new ResetSignInCommandHandler(_auth.Object);

        var response = await sut.Handle(new ResetSignInCommand(), default);

        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Reset_ServiceReturnsFalse_ReturnsFail()
    {
        _auth.Setup(a => a.ResetAsync(It.IsAny<ResetSignInCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new ResetSignInCommandHandler(_auth.Object);

        var response = await sut.Handle(new ResetSignInCommand(), default);

        response.Success.Should().BeFalse();
    }

    // ─── SendConfirmationCode ────────────────────────────────────

    [Fact]
    public async Task SendConfirmation_DelegatesToServiceAndReturnsOk()
    {
        _auth.Setup(a => a.SendEmailAsync("u@e.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new SendConfirmationCodeCommandHandler(_auth.Object);

        var response = await sut.Handle(new SendConfirmationCodeCommand { Email = "u@e.com" }, default);

        response.Success.Should().BeTrue();
    }

    // ─── ChangePassword ──────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_NewMismatchesConfirm_Fails()
    {
        var sut = new ChangePasswordCommandHandler(_currentUser.Object, _users.Object, _hash.Object);

        var response = await sut.Handle(new ChangePasswordCommand
        {
            OldPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "different"
        }, default);

        response.Success.Should().BeFalse();
        response.Message.Should().Contain("match");
    }

    [Fact]
    public async Task ChangePassword_NewSameAsOld_Fails()
    {
        var sut = new ChangePasswordCommandHandler(_currentUser.Object, _users.Object, _hash.Object);

        var response = await sut.Handle(new ChangePasswordCommand
        {
            OldPassword = "samePass",
            NewPassword = "samePass",
            ConfirmPassword = "samePass"
        }, default);

        response.Success.Should().BeFalse();
        response.Message.Should().Contain("differ");
    }

    [Fact]
    public async Task ChangePassword_NotAuthenticated_Throws()
    {
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var sut = new ChangePasswordCommandHandler(_currentUser.Object, _users.Object, _hash.Object);

        var act = () => sut.Handle(new ChangePasswordCommand
        {
            OldPassword = "a", NewPassword = "b", ConfirmPassword = "b"
        }, default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ChangePassword_OldPasswordWrong_Fails()
    {
        var user = TestData.User(passwordHash: "stored");
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hash.Setup(h => h.Verify("wrong", "stored")).Returns(false);
        var sut = new ChangePasswordCommandHandler(_currentUser.Object, _users.Object, _hash.Object);

        var response = await sut.Handle(new ChangePasswordCommand
        {
            OldPassword = "wrong", NewPassword = "newpass", ConfirmPassword = "newpass"
        }, default);

        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePassword_HappyPath_HashesAndUpdates()
    {
        var user = TestData.User(passwordHash: "stored");
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hash.Setup(h => h.Verify("right", "stored")).Returns(true);
        _hash.Setup(h => h.Hash("newpass")).Returns("HASHED");
        _users.Setup(r => r.UpdateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity u, CancellationToken _) => u);
        var sut = new ChangePasswordCommandHandler(_currentUser.Object, _users.Object, _hash.Object);

        var response = await sut.Handle(new ChangePasswordCommand
        {
            OldPassword = "right", NewPassword = "newpass", ConfirmPassword = "newpass"
        }, default);

        response.Success.Should().BeTrue();
        _users.Verify(r => r.UpdateAsync(It.Is<UserEntity>(u => u.PasswordHash == "HASHED"),
                                          It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── UpdateProfile ───────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_NotAuthenticated_Throws()
    {
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var sut = new UpdateProfileCommandHandler(_currentUser.Object, _users.Object, _mediator.Object, _mapper.Object);

        var act = () => sut.Handle(new UpdateProfileCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateProfile_OnlyUpdatesProvidedFields()
    {
        var user = TestData.User(firstName: "Old", lastName: "Last");
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity u, CancellationToken _) => u);
        _mapper.Setup(m => m.Map<UserViewModel>(It.IsAny<UserEntity>()))
            .Returns(new UserViewModel { FirstName = "New", LastName = "Last" });
        var sut = new UpdateProfileCommandHandler(_currentUser.Object, _users.Object, _mediator.Object, _mapper.Object);

        var response = await sut.Handle(new UpdateProfileCommand
        {
            FirstName = "New" // LastName left null — should keep "Last".
        }, default);

        response.Success.Should().BeTrue();
        // The handler mutates the entity in-place; the captured argument has "New" / "Last".
        _users.Verify(r => r.UpdateAsync(
            It.Is<UserEntity>(u => u.FirstName == "New" && u.LastName == "Last"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── GetMe ───────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_NotAuthenticated_Throws()
    {
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var sut = new GetMeQueryHandler(_currentUser.Object, _mapper.Object);

        var act = () => sut.Handle(new GetMeQuery(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetMe_HappyPath_MapsAndReturns()
    {
        var user = TestData.User();
        _currentUser.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var vm = new UserViewModel { Id = user.Id };
        _mapper.Setup(m => m.Map<UserViewModel>(user)).Returns(vm);
        var sut = new GetMeQueryHandler(_currentUser.Object, _mapper.Object);

        var response = await sut.Handle(new GetMeQuery(), default);

        response.Data.Should().BeSameAs(vm);
    }
}
