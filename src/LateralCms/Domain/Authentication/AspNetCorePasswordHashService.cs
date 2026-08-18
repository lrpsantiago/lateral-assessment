using LateralCms.Application.Authentication;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace LateralCms.Infrastructure.Authentication;

public sealed class AspNetCorePasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new User(), password);
    }

    public PasswordHashVerificationResult VerifyPassword(string passwordHash, string providedPassword)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            new User(),
            passwordHash,
            providedPassword);

        if (result == PasswordVerificationResult.Success)
        {
            return PasswordHashVerificationResult.Success;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded
            || VerifyLegacyPassword(passwordHash, providedPassword))
        {
            return PasswordHashVerificationResult.SuccessRehashNeeded;
        }

        return PasswordHashVerificationResult.Failed;
    }

    private static bool VerifyLegacyPassword(string storedHash, string password)
    {
        var suppliedHashBytes = Encoding.UTF8.GetBytes(password.Encrypt());
        var storedHashBytes = Encoding.UTF8.GetBytes(storedHash);

        return suppliedHashBytes.Length == storedHashBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedHashBytes, storedHashBytes);
    }
}
