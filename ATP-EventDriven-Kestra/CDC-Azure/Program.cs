using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CDC_Azure.Consumers;
using CDC_Azure.Services;
using CDC_Azure.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        int maxRetry = 10;       // jumlah percobaan restart maksimal
        int delayBeforeRetry = 5000; // jeda sebelum restart (ms)
        int attempt = 0;

        while (attempt < maxRetry)
        {
            attempt++;

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(new SqlConnectionFactory(SqlConfig.SqlConnectionString));
                    services.AddSingleton<OrderService>();
                    services.AddSingleton<TenantService>();
                    services.AddSingleton<OrderConsumer>();
                    services.AddSingleton<TenantConsumer>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            var orderConsumer = host.Services.GetRequiredService<OrderConsumer>();
            var tenantConsumer = host.Services.GetRequiredService<TenantConsumer>();

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Stopping...");
                cts.Cancel();
            };

            var orderTask = Task.Run(() => orderConsumer.Start(cts.Token));
            var tenantTask = Task.Run(() => tenantConsumer.Start(cts.Token));

            try
            {
                Console.WriteLine($"[INFO] Menjalankan consumer (Percobaan ke-{attempt})...");
                await Task.WhenAll(orderTask, tenantTask);
                break; // Jika berhasil, keluar dari loop
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Terjadi error di consumer: {ex.Message}");
                Console.ResetColor();

                if (attempt >= maxRetry)
                {
                    Console.WriteLine("[FATAL] Batas restart tercapai. Aplikasi dihentikan.");
                    break;
                }

                Console.WriteLine($"[INFO] Restarting aplikasi dalam {delayBeforeRetry / 1000} detik...");
                await Task.Delay(delayBeforeRetry);
            }
        }
    }
}
