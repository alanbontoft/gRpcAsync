using Grpc.Core;
using RpcAsync;


class Program
{
    static async Task Main(string[] args)
    {
        var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);

        var client = new AutomationService.AutomationServiceClient(channel);

        _ = Task.Run(async () =>
        {
            using var call =
                client.Subscribe(
                    new EventSubscription
                    {
                        ClientId = "Operator1"
                    });

            await foreach (var evt in
                call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(
                    $"EVENT:({evt.Delay}) {evt.EventType} : {evt.Message}");
            }
        });

        while (true)
        {
            var status = await client.GetStatusAsync(
                    new StatusRequest
                    {
                        MachineId = "M01"
                    });

            var msg =$"Status: Running={status.Running} Count={status.PartCount} Data=";

            for (int i=0; i < status.Data.Count; i++) { msg += status.Data[i].ToString() + " "; }

            Console.WriteLine(msg);
                

            await Task.Delay(3000);
        }
    }

}
