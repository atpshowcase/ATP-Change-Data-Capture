using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CDC_Azure.Config;

namespace CDC_Azure.Helpers
{
    public static class KestraMonitor
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task MonitorExecutionAsync(string execId, CancellationToken token)
        {
            var url = $"{KestraConfig.Host}/api/v1/executions/{execId}";

            while (!token.IsCancellationRequested)
            {
                var response = await _httpClient.GetAsync(url, token);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Failed to monitor: {response.StatusCode} - {json}");
                    break;
                }

                var doc = JsonDocument.Parse(json);
                var state = doc.RootElement.GetProperty("state").GetString();

                Console.WriteLine($"📈 Execution State: {state}");

                if (state == "SUCCESS" || state == "FAILED")
                {
                    Console.WriteLine(state == "SUCCESS" ? "✅ Success" : "❌ Failed");
                    break;
                }

                await Task.Delay(3000, token);
            }
        }
    }
}
