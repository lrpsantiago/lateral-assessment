using System.Text.Json;

namespace LateralCms.Api.Contracts.Requests;

public sealed record class PostCmsEventRequest(
    string Type,
    string Id,
    JsonElement? Payload,
    int? Version,
    DateTime? Timestamp);
