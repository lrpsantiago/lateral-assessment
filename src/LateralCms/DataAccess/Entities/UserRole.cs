namespace LateralCms.Domain.Entities;

public class UserRole
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual IEnumerable<User>? Users { get; set; }
}
