using LateralCms.Application.Authentication;
using LateralCms.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LateralCms.Infrastructure.Authentication;

public sealed class DatabaseUserCredentialValidator : IUserCredentialValidator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _passwordHashService;

    public DatabaseUserCredentialValidator(IUnitOfWork unitOfWork, IPasswordHashService passwordHashService)
    {
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
    }

    public async Task<AuthenticatedUser?> ValidateAsync(string username, string password,
        CancellationToken cancellationToken = default)
    {
        Domain.Entities.User? user = await _unitOfWork.Users
            .Query()
            .Include(candidate => candidate.Role)
            .SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        PasswordHashVerificationResult verificationResult = _passwordHashService.VerifyPassword(user.PasswordHash, password);

        if (verificationResult == PasswordHashVerificationResult.Failed)
        {
            return null;
        }

        if (verificationResult == PasswordHashVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHashService.HashPassword(password);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new AuthenticatedUser(user.Id, user.Username!, user.Role?.Name);
    }
}
