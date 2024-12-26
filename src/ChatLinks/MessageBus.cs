using System.Collections.Concurrent;

namespace SL.ChatLinks;

public static class MessageBus
{
    private static readonly ConcurrentDictionary<string, Action<string>> Destinations = [];

    public static void Send(string address, string message)
    {
        if (Destinations.TryGetValue(address, out var destination))
        {
            destination?.Invoke(message);
        }
    }

    public static bool Register(string address, Action<string> destination)
    {
        return Destinations.TryAdd(address, destination);
    }

    public static bool Unregister(string address)
    {
        return Destinations.TryRemove(address, out _);
    }
}