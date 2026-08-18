namespace LateralCms.Api.Contracts.Responses;

public record ResponseWrapper
{
    public bool Success { get; init; }

    public int HttpStatusCode { get; init; }

    public string? HttpStatus { get; init; }

    public string? Message { get; init; }

    public object? Data { get; init; }
}
