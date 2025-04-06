using System.Net;

using GuildWars2;
using GuildWars2.Mumble;

using SL.Common.Exploration;

namespace SL.ChatLinks;

public sealed class MumbleListener(GameLink link, IEventAggregator eventAggregator) : IObserver<GameTick>, IDisposable
{
    private IDisposable? _subscription;

    private IPEndPoint? _currentServer;

    public void Start()
    {
        _subscription?.Dispose();
        _subscription = link.Subscribe(this);

    }

    public void OnNext(GameTick value)
    {
        if (_currentServer?.Equals(value.Context.ServerAddress) != true)
        {
            _currentServer = value.Context.ServerAddress;
            eventAggregator.Publish(new MapChanged((int)value.Context.MapId));
        }
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
