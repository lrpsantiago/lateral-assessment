namespace LateralCms.Api.Contracts.Requests;

public sealed record class PostCmsEventRequest(
    string Type,
    string Id,
    string? Payload,
    int? Version,
    DateTime? Timestamp);
