namespace LateralCms.Domain.Entities;

public class CmsEntityVersion
{
    public string? EntityId { get; set; }

    public int Version { get; set; }

    public string? Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CmsEntity? Entity { get; set; }
}