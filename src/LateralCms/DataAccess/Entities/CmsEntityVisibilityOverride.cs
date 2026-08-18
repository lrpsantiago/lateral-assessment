namespace LateralCms.Domain.Entities;

public class CmsEntityVisibilityOverride
{
    public string? CmsEntityId { get; set; }

    public bool IsVisible { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
