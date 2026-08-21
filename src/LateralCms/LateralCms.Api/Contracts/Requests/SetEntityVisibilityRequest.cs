namespace LateralCms.Api.Contracts.Requests;

public class SetEntityVisibilityRequest
{
    public string? Id { get; set; }

    public bool IsVisible { get; set; }
}
