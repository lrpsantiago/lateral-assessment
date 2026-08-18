namespace LateralCms.Application.Authentication;

public sealed record AuthenticatedUser(
    int UserId,
    string Username,
    string? Role);
