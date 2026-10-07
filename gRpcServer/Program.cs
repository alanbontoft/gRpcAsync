using Grpc.Core;
using RpcAsync;

using RpcAsync.Services;


class Program
{
    static async Task Main(string[] args)
    {
        const int Port = 50051;

        var random = new Random();
        
        var subscriptions = new SubscriptionManager();

        var server = new Server
        {
            Services =
            {
                AutomationService.BindService( new AutomationServiceImpl(subscriptions))
            },
            Ports =
            {
                new ServerPort("localhost", Port, ServerCredentials.Insecure)
            }
        };

        server.Start();

        Console.WriteLine("Server started");

        // start task to create async events

        _ = Task.Run(async () =>
        {
            int count = 0;

            while (true)
            {
                // wait for a connection
                await subscriptions.ConnectionSemaphore.WaitAsync();

                while (subscriptions.Connected)
                {
                    var delay = random.Next(1000, 10000);

                    Console.WriteLine($"Delay: {delay}");

                    await Task.Delay(delay);

                    await subscriptions.BroadcastAsync(
                        new EventMessage
                        {
                            Delay = delay,
                            EventType = "CycleComplete",
                            Message = $"Part {++count} finished"
                        });
                }
            }
        });

        Console.ReadLine();

        await server.ShutdownAsync();
    }
}
