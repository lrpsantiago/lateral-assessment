namespace LateralCms.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public int RoleId { get; set; }

    public virtual UserRole? Role { get; set; }
}
