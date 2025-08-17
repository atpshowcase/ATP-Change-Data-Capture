using Microsoft.Extensions.Logging;
using CDC_Azure.Consumers;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<OrderConsumer>();

        var consumer = new OrderConsumer(logger);
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("🛑 Stopping...");
            cts.Cancel();
        };

        await consumer.Start(cts.Token);
    }
}
