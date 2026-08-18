namespace LateralCms.Application.Authentication;

public interface IPasswordHashService
{
    string HashPassword(string password);

    PasswordHashVerificationResult VerifyPassword(string passwordHash, string providedPassword);
}

public enum PasswordHashVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}
