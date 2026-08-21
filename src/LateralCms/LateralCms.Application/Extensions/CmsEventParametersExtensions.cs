using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Exceptions;

namespace LateralCms.Application.Extensions;

public static class CmsEventParametersExtensions
{
    public static void Validate(this CmsEventParameters source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(source.EntityId))
        {
            throw new DomainException("The 'EntityId' is required.");
        }

        if (source.Version == null)
        {
            throw new DomainException("The 'Version' is required.");
        }

        if (source.Version <= 0)
        {
            throw new DomainException("The version must be greater than zero.");
        }

        if (source.Timestamp == null)
        {
            throw new DomainException("The 'Timestamp' is required.");
        }
    }
}
