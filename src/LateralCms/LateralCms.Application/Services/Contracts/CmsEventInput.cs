namespace LateralCms.Application.Services.Contracts;

public record CmsEventInput : CmsEventParameters
{
    public string? Type { get; set; }
}
