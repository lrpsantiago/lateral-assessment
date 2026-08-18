using LateralCms.Application.Interfaces.Queue;
using System.Threading.Channels;

namespace LateralCms.Infrastructure.Queue;

public sealed class CmsEventQueue : ICmsEventQueue
{
    private readonly Channel<Guid> _channel;

    public CmsEventQueue()
    {
        var options = new BoundedChannelOptions(1_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<Guid>(options);
    }

    public ValueTask EnqueueAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(eventId, cancellationToken);
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}