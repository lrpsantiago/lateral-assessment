using System.Text.Json;

namespace LateralCms.Application.Services.Contracts;

public class CmsEntityOutput
{
    public string? Id { get; set; }

    public int Version { get; set; }

    public JsonElement? Payload { get; set; }

    public DateTime EntityCreatedAt { get; set; }

    public DateTime VersionCreatedAt { get; set; }

    public bool IsPublished { get; set; }

    public bool HasVisibilityAllowed { get; set; }
}
