using Grpc.Core;

namespace RpcAsync.Services;
public class AutomationServiceImpl : AutomationService.AutomationServiceBase
{
    private readonly SubscriptionManager _subscriptions;

    private readonly Random _random = new();

    public AutomationServiceImpl(SubscriptionManager subscriptions)
    {
        _subscriptions = subscriptions;
    }

    public override Task<StatusReply> GetStatus(StatusRequest request, ServerCallContext context)
    {
        Console.WriteLine($"Status Request: {request.MachineId}");

        var random = new Random();

        var datasize = random.Next(1, 10);

        var reply = new StatusReply
        {
            Running = true,
            PartCount = _random.Next(0, 100)
        };

        // add array data to message
        for (int i = 0; i < datasize; i++) { reply.Data.Add(_random.Next(1, 100)); }

        // show data sent
        var msg = "Data sent: ";

        for (int i = 0; i < datasize; i++) { msg += reply.Data[i].ToString() + " "; }

        Console.WriteLine(msg);

        // return StatusReply object
        return Task.FromResult(reply);
    }

    public override async Task Subscribe( EventSubscription request, IServerStreamWriter<EventMessage> responseStream, ServerCallContext context)
    {
        var id = _subscriptions.AddSubscriber(request.ClientId, responseStream);

        Console.WriteLine( $"Connected: {request.ClientId}");

        _subscriptions.ConnectionSemaphore.Release();

        try
        {
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _subscriptions.RemoveSubscriber(id);

            Console.WriteLine($"Disconnected: {request.ClientId}");
        }
    }
}