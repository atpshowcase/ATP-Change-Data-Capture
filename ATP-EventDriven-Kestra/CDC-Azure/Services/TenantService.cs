using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CDC_Azure.Config;
using CDC_Azure.Helpers;
using CDC_Azure.Models;

namespace CDC_Azure.Services
{
    public class TenantService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public TenantService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateTenant(string execId, CancellationToken token)
        {
            var url = $"{KestraConfig.Host}{KestraConfig.UrlAPI}{execId}";

            while (!token.IsCancellationRequested)
            {
                var response = await KestraHttpClient.Client.GetAsync(url, token);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to monitor: {response.StatusCode} - {json}");
                    break;
                }

                var doc = JsonDocument.Parse(json);
                var state = doc.RootElement.GetProperty("state").GetProperty("current").GetString();
                Console.WriteLine($"Execution State: {state}");

                if (state == "SUCCESS" || state == "FAILED")
                {
                    if (state == "SUCCESS")
                    {
                        try
                        {
                            var inputs = doc.RootElement
                                .GetProperty("inputs")
                                .GetProperty("after")
                                .GetRawText();

                            var data = JsonSerializer.Deserialize<mstTenant>(inputs);

                            if (data != null)
                            {
                                await UpdateTenantAsync(data, token);
                                Console.WriteLine("Tenant updated successfully in database");
                            }
                            else
                            {
                                Console.WriteLine("Data order kosong, update dibatalkan");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Gagal memproses data: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Execution failed");
                    }

                    break;
                }

                await Task.Delay(3000, token);
            }
        }

        private async Task UpdateTenantAsync(mstTenant order, CancellationToken token)
        {
            var query = @"
                UPDATE mstTenant
                SET
                    CustomerID = '2'";

            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(token);

            using var cmd = new SqlCommand(query, conn);

            await cmd.ExecuteNonQueryAsync(token);
        }
    }
}
