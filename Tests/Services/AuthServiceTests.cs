using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using Application.Services.EntityServices.Auths;
using AutoMapper;
using Domain.Entities;
using FamilyTree.Tests.Helpers;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace FamilyTree.Tests.Services;

/// <summary>
/// AuthService is where the recent reset/sign-up/email bugs lived.
/// Each test below corresponds to a specific code path that has misbehaved
/// at least once.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserRoleRepository> _roles = new();
    private readonly Mock<ITokenService> _token = new();
    private readonly Mock<IHashService> _hash = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<IRedisService> _redis = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IMediator> _mediator = new();

    /// <summary>In-memory IConfiguration used by tests — the email confirmation
    /// flow needs `App:FrontendBaseUrl` to build the link sent in the welcome
    /// email; tests don't actually fire mail so any value works.</summary>
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["App:FrontendBaseUrl"] = "https://test.local"
        })
        .Build();

    private AuthService CreateSut() => new(
        _users.Object, _roles.Object, _token.Object, _hash.Object,
        _email.Object, _redis.Object, _mapper.Object, _mediator.Object, _configuration);

    // ─── SignUpAsync ──────────────────────────────────────────────

    [Fact]
    public async Task SignUp_PasswordsMismatch_Throws()
    {
        var sut = CreateSut();
        var dto = new CreateUserDto { Password = "abc123", ConfirmPassword = "different" };

        var act = () => sut.SignUpAsync(dto, default);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*do not match*");
    }

    [Fact]
    public async Task SignUp_NewUserRoleMissing_Throws()
    {
        _roles.Setup(r => r.GetAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole?)null);
        var sut = CreateSut();

        var act = () => sut.SignUpAsync(new CreateUserDto { Password = "x", ConfirmPassword = "x" }, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SignUp_HappyPath_HashesPasswordAndCreatesUser()
    {
        var role = TestData.UserRole(designedName: "NEW_USER");
        _roles.Setup(r => r.GetAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _hash.Setup(h => h.Hash("secret")).Returns("HASHED");
        _mapper.Setup(m => m.Map<User>(It.IsAny<CreateUserDto>())).Returns(TestData.User());
        _users.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        var sut = CreateSut();

        var result = await sut.SignUpAsync(
            new CreateUserDto { Password = "secret", ConfirmPassword = "secret" }, default);

        result.Should().BeTrue();
        _users.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.RoleId == role.Id && u.PasswordHash == "HASHED"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── SignInAsync ──────────────────────────────────────────────

    [Fact]
    public async Task SignIn_UserNotFound_ThrowsUnauthorized()
    {
        _users.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var sut = CreateSut();

        var act = () => sut.SignInAsync(new ConcreteSignInDto { Login = "x", Password = "y" }, default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SignIn_WrongPassword_ThrowsUnauthorized()
    {
        var user = TestData.User(passwordHash: "stored");
        _users.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hash.Setup(h => h.Verify("wrong", "stored")).Returns(false);
        var sut = CreateSut();

        var act = () => sut.SignInAsync(new ConcreteSignInDto { Login = "u", Password = "wrong" }, default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SignIn_HappyPath_ReturnsToken()
    {
        var user = TestData.User(passwordHash: "stored");
        _users.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hash.Setup(h => h.Verify("right", "stored")).Returns(true);
        var expected = new TokenViewModel { AccessToken = "tok" };
        _token.Setup(t => t.GenerateToken(It.IsAny<System.Security.Claims.Claim[]>())).Returns(expected);
        var sut = CreateSut();

        var token = await sut.SignInAsync(new ConcreteSignInDto { Login = "u", Password = "right" }, default);

        token.AccessToken.Should().Be("tok");
    }

    // ─── ResetAsync ───────────────────────────────────────────────

    [Fact]
    public async Task Reset_PasswordsMismatch_Throws()
    {
        var sut = CreateSut();
        var dto = new ConcreteResetDto
        {
            Email = "u@e.com",
            Password = "a",
            ConfirmPassword = "b",
            ConfirmationCode = "1"
        };

        var act = () => sut.ResetAsync(dto, default);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Reset_InvalidConfirmationCode_ThrowsUnauthorized()
    {
        _redis.Setup(r => r.GetAsync<string>(It.IsAny<string>())).ReturnsAsync("999999");
        var sut = CreateSut();
        var dto = new ConcreteResetDto
        {
            Email = "u@e.com",
            Password = "x", ConfirmPassword = "x",
            ConfirmationCode = "111111"
        };

        var act = () => sut.ResetAsync(dto, default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Reset_LooksUpUserByEmailNotUsername()
    {
        // Regression for the bug where the handler called GetByUsernameAsync
        // with the email value, querying the wrong column and 404-ing every
        // valid reset. Verify GetByEmailAsync is hit and GetByUsernameAsync isn't.
        const string email = "u@e.com", code = "123456";
        _redis.Setup(r => r.GetAsync<string>($"confirmation-code-for-{email}")).ReturnsAsync(code);
        _users.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.User());
        _users.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _hash.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        var sut = CreateSut();
        var dto = new ConcreteResetDto
        {
            Email = email, Password = "p", ConfirmPassword = "p", ConfirmationCode = code
        };

        var ok = await sut.ResetAsync(dto, default);

        ok.Should().BeTrue();
        _users.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reset_UserMissing_ThrowsKeyNotFound()
    {
        const string email = "u@e.com", code = "111";
        _redis.Setup(r => r.GetAsync<string>($"confirmation-code-for-{email}")).ReturnsAsync(code);
        _users.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var sut = CreateSut();

        var act = () => sut.ResetAsync(new ConcreteResetDto
        {
            Email = email, Password = "p", ConfirmPassword = "p", ConfirmationCode = code
        }, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── SendEmailAsync ───────────────────────────────────────────

    [Fact]
    public async Task SendEmail_UserMissing_Throws()
    {
        _users.Setup(r => r.GetByEmailAsync("missing@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var sut = CreateSut();

        var act = () => sut.SendEmailAsync("missing@e.com", default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SendEmail_HappyPath_StoresCodeAndDispatches()
    {
        const string email = "u@e.com";
        _users.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.User(email: email));
        _email.Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var sut = CreateSut();

        var ok = await sut.SendEmailAsync(email, default);

        ok.Should().BeTrue();
        _redis.Verify(r => r.SetAsync($"confirmation-code-for-{email}",
            It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    // ─── ConfirmEmailAsync ────────────────────────────────────────

    [Fact]
    public async Task ConfirmEmail_BlankToken_Throws()
    {
        var sut = CreateSut();
        var act = () => sut.ConfirmEmailAsync("", default);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConfirmEmail_TokenNotInDb_Throws()
    {
        // No user matches the hash → reject without leaking which case it was.
        _users.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var sut = CreateSut();

        var act = () => sut.ConfirmEmailAsync("not-a-real-token", default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ConfirmEmail_AlreadyConfirmed_IsIdempotent()
    {
        // Re-clicking the link after success must not regress — it just no-ops.
        var user = TestData.User(emailConfirmed: true);
        _users.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var sut = CreateSut();

        var ok = await sut.ConfirmEmailAsync("any-token", default);

        ok.Should().BeTrue();
        // No write — already confirmed.
        _users.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ExpiredToken_Throws()
    {
        var user = TestData.User(emailConfirmed: false);
        user.EmailConfirmationTokenHash = "stub";
        user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        _users.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var sut = CreateSut();

        var act = () => sut.ConfirmEmailAsync("token", default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ConfirmEmail_HappyPath_SetsFlagAndClearsToken()
    {
        var user = TestData.User(emailConfirmed: false);
        user.EmailConfirmationTokenHash = "stub";
        user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        _users.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = CreateSut();

        var ok = await sut.ConfirmEmailAsync("token", default);

        ok.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.EmailConfirmedAt.Should().NotBeNull();
        // Single-use guarantee — the link can't be replayed.
        user.EmailConfirmationTokenHash.Should().BeNull();
        user.EmailConfirmationTokenExpiresAt.Should().BeNull();
    }

    // ─── ResendConfirmationAsync ──────────────────────────────────

    [Fact]
    public async Task Resend_AlreadyConfirmed_Throws()
    {
        var user = TestData.User(email: "u@e.com", emailConfirmed: true);
        _users.Setup(r => r.GetByEmailAsync("u@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var sut = CreateSut();

        var act = () => sut.ResendConfirmationAsync("u@e.com", default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Resend_HappyPath_RotatesTokenAndSendsEmail()
    {
        var user = TestData.User(email: "u@e.com", emailConfirmed: false);
        _users.Setup(r => r.GetByEmailAsync("u@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _email.Setup(e => e.SendEmailAsync("u@e.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var sut = CreateSut();

        var ok = await sut.ResendConfirmationAsync("u@e.com", default);

        ok.Should().BeTrue();
        user.EmailConfirmationTokenHash.Should().NotBeNullOrEmpty();
        user.EmailConfirmationTokenExpiresAt.Should().NotBeNull();
        _email.Verify(e => e.SendEmailAsync("u@e.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ─── SendEmail / Reset block on unconfirmed email ─────────────

    [Fact]
    public async Task SendEmail_UnconfirmedUser_ThrowsInvalidOperation()
    {
        // Reset-password mail must NOT go out for an account whose owner
        // hasn't proved they control the inbox — otherwise we become a
        // free relay for sending mail to arbitrary addresses.
        var user = TestData.User(email: "u@e.com", emailConfirmed: false);
        _users.Setup(r => r.GetByEmailAsync("u@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var sut = CreateSut();

        var act = () => sut.SendEmailAsync("u@e.com", default);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _email.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ResetSignInDto and SignInDto are abstract; concrete subclasses let tests build instances.
    private sealed record ConcreteResetDto : ResetSignInDto;
    private sealed record ConcreteSignInDto : SignInDto;
}
