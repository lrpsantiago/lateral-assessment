namespace LateralCms.Domain.Entities;

public class CmsEntity
{
    public string? Id { get; set; }

    public int? LatestVersionId { get; set; }

    public int? PublishedVersionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CmsEntityVersion? LatestVersion { get; set; }

    public virtual CmsEntityVersion? PublishedVersion { get; set; }

    public virtual IEnumerable<CmsEntityVersion>? Versions { get; set; }

    //public virtual IEnumerable<CmsEvent>? Events { get; set; }
}
