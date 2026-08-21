namespace LateralCms.Application.Services.Contracts;

public record CmsEventParameters
{
    public string? EntityId { get; set; }

    public string? Payload { get; set; }

    public int? Version { get; set; }

    public DateTime? Timestamp { get; set; }
}
