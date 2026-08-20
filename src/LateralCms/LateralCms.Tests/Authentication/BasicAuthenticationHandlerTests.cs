using LateralCms.Api.Authentication;
using LateralCms.Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace LateralCms.Tests.Authentication;

public sealed class BasicAuthenticationHandlerTests
{
    private readonly Mock<IUserCredentialValidator> _credentialValidatorMock;
    private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _optionsMonitorMock;
    private readonly BasicAuthenticationHandler _sut;

    public BasicAuthenticationHandlerTests()
    {
        _credentialValidatorMock = new Mock<IUserCredentialValidator>(MockBehavior.Strict);
        _optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>(MockBehavior.Strict);

        _optionsMonitorMock
            .Setup(options => options.Get(It.IsAny<string?>()))
            .Returns(new AuthenticationSchemeOptions());

        _sut = new BasicAuthenticationHandler(
            _optionsMonitorMock.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            _credentialValidatorMock.Object);
    }

    [Theory]
    [InlineData("alice", "alice-password", 42, "admin")]
    [InlineData("cms-service", "service-password", 84, "cms")]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthenticatedPrincipal(string username,
        string password, int expectedUserId, string expectedRole)
    {
        var expectedUser = new AuthenticatedUser(expectedUserId, username, expectedRole);

        _credentialValidatorMock
            .Setup(validator => validator.ValidateAsync(username, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        await InitializeSutAsync(username, password);

        var result = await _sut.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.Equal(BasicAuthenticationHandler.DefaultScheme, result.Ticket?.AuthenticationScheme);
        Assert.Equal(expectedUserId.ToString(), result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(username, result.Principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(expectedRole, result.Principal.FindFirst(ClaimTypes.Role)?.Value);

        _credentialValidatorMock.Verify(
            validator => validator.ValidateAsync(username, password, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("alice", "wrong-password")]
    [InlineData("alice", "service-password")]
    [InlineData("cms-service", "alice-password")]
    [InlineData("unknown-user", "alice-password")]
    [InlineData("unknown-user", "wrong-password")]
    public async Task AuthenticateAsync_WithInvalidCredentialCombination_ReturnsFailure(string username,
        string password)
    {
        _credentialValidatorMock
            .Setup(validator => validator.ValidateAsync(username, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        await InitializeSutAsync(username, password);

        var result = await _sut.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Invalid credentials.", result.Failure.Message);
        Assert.Null(result.Principal);

        _credentialValidatorMock.Verify(
            validator => validator.ValidateAsync(username, password, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task InitializeSutAsync(string username, string password)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = CreateBasicHeader(username, password);

        var scheme = new AuthenticationScheme(
            BasicAuthenticationHandler.DefaultScheme,
            BasicAuthenticationHandler.DefaultScheme,
            typeof(BasicAuthenticationHandler));

        await _sut.InitializeAsync(scheme, context);
    }

    private static string CreateBasicHeader(string username, string password)
    {
        var byreArray = Encoding.UTF8.GetBytes($"{username}:{password}");
        var credentials = Convert.ToBase64String(byreArray);
        var header = new AuthenticationHeaderValue(BasicAuthenticationHandler.DefaultScheme, credentials);

        return header.ToString();
    }
}