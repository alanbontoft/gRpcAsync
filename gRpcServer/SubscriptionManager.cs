using System.Collections.Concurrent;
using Grpc.Core;
using RpcAsync;


public class Subscriber
{
    public string ClientId { get; set; } = "";

    public IServerStreamWriter<EventMessage> Stream
    {
        get;
        set;
    } = null!;
}

public class SubscriptionManager
{
    private readonly ConcurrentDictionary<Guid, Subscriber> _clients = new();

    public readonly SemaphoreSlim ConnectionSemaphore = new SemaphoreSlim(0, 1);

    public bool Connected { get; private set; } = false;

    public Guid AddSubscriber(string clientId, IServerStreamWriter<EventMessage> stream)
    {
        var id = Guid.NewGuid();

        _clients[id] = new Subscriber
        {
            ClientId = clientId,
            Stream = stream
        };

        return id;
    }

    public void RemoveSubscriber(Guid id)
    {
        _clients.TryRemove(id, out _);
    }

    public async Task BroadcastAsync(EventMessage evt)
    {
        foreach (var c in _clients.Values)
        {
            try
            {
                await c.Stream.WriteAsync(evt);
            }
            catch
            {
                // disconnected
            }
        }
    }

    public void SetConnected(bool connected)
    {
        Connected = connected;

        if (Connected)
        {
            ConnectionSemaphore.Release();
        }
    }
}