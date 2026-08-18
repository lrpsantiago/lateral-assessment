using LateralCms.Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace LateralCms.Api.Authentication;

public sealed class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserCredentialValidator credentialValidator)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public static readonly string DefaultScheme = "Basic";
    public static readonly string DefaultRealm = "LateralCms";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerValue = Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        if (!TryReadCredentials(headerValue, out var username, out var password))
        {
            return AuthenticateResult.Fail("The Basic authorization header is malformed.");
        }

        var user = await credentialValidator.ValidateAsync(
            username,
            password,
            Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid credentials.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        var identity = new ClaimsIdentity(claims, DefaultScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DefaultScheme);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] =
            $"Basic realm=\"{DefaultRealm}\", charset=\"UTF-8\"";

        await base.HandleChallengeAsync(properties);
    }

    private static bool TryReadCredentials(string headerValue, out string username, out string password)
    {
        username = string.Empty;
        password = string.Empty;

        if (!AuthenticationHeaderValue.TryParse(headerValue, out var authorizationHeader)
            || !DefaultScheme.Equals(authorizationHeader.Scheme, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorizationHeader.Parameter))
        {
            return false;
        }

        try
        {
            var credentialBytes = Convert.FromBase64String(authorizationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes);
            var separatorIndex = credentials.IndexOf(':');

            if (separatorIndex <= 0)
            {
                return false;
            }

            username = credentials[..separatorIndex];
            password = credentials[(separatorIndex + 1)..];

            return !string.IsNullOrWhiteSpace(username);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
