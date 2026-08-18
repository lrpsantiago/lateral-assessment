namespace LateralCms.Application.Services.Contracts;

public class EventReceivalOutput
{
    public Guid BatchId { get; set; }
    public IEnumerable<Guid>? EventsIds { get; set; }
}
