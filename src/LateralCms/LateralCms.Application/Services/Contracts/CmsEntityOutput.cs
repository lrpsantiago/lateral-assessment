namespace LateralCms.Application.Services.Contracts;

public class CmsEntityOutput
{
    public string? Id { get; set; }

    public int Version { get; set; }

    public string? Payload { get; set; }

    public DateTime EntityCreatedAt { get; set; }

    public DateTime VersionCreatedAt { get; set; }

    public bool IsPublished { get; set; }
}
