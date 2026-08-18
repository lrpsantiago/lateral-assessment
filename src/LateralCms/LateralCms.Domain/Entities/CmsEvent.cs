using LateralCms.Domain.Enumerations;

namespace LateralCms.Domain.Entities;

public class CmsEvent
{
    public Guid Id { get; set; }

    public Guid BatchId { get; set; }

    public string? EntityId { get; set; }

    public string? Type { get; set; }

    public string? Payload { get; set; }

    public int? Version { get; set; }

    public DateTime ReceivedAt { get; set; }

    public DateTime? ProcessStart { get; set; }

    public DateTime? ProcessEnd { get; set; }

    public EventStatus Status { get; set; }

    public string? LastErrorMessage { get; set; }
}
