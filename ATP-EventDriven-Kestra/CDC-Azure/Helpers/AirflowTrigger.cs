using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CDC_Azure.Config;

namespace CDC_Azure.Helpers
{
    public class AirflowTrigger
    {
        private readonly HttpClient _httpClient;
        private readonly string _airflowHost;
        private readonly string _username;
        private readonly string _password;

        public AirflowTrigger()
        {
            _httpClient = new HttpClient();
            _airflowHost = AirflowConfig.Host;
            _username = AirflowConfig.Username;
            _password = AirflowConfig.Password;

            var credentials = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        }

        public async Task TriggerDagAsync(string dagId, object payload)
        {
            var url = $"{_airflowHost}/api/v1/dags/{dagId}/dagRuns";

            var dagRunId = $"dagrun_{Guid.NewGuid()}";

            var requestBody = new
            {
                conf = payload,
                dag_run_id = dagRunId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Failed to trigger DAG '{dagId}'. Status: {response.StatusCode}, Body: {body}");
                    return;
                }

                Console.WriteLine($"✅ DAG '{dagId}' triggered successfully with run id '{dagRunId}'.");

                var statusUrl = $"{_airflowHost}/api/v1/dags/{dagId}/dagRuns/{dagRunId}";
                bool isCompleted = false;
                while (!isCompleted)
                {
                    var statusResponse = await _httpClient.GetAsync(statusUrl);
                    if (!statusResponse.IsSuccessStatusCode)
                    {
                        var errorBody = await statusResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Failed to get DAG run status. Status: {statusResponse.StatusCode}, Body: {errorBody}");
                        break;
                    }

                    var json = await statusResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var state = doc.RootElement.GetProperty("state").GetString();

                    switch (state)
                    {
                        case "queued":
                            OnQueued();
                            break;
                        case "running":
                            OnRunning();
                            break;
                        case "success":
                            OnSuccess();
                            isCompleted = true;
                            break;
                        case "failed":
                        case "upstream_failed":
                            OnFailed();
                            isCompleted = true;
                            break;
                        default:
                            Console.WriteLine($"ℹ️ DAG run in state: {state}");
                            break;
                    }

                    if (!isCompleted)
                    {
                        await Task.Delay(3000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Error triggering DAG '{dagId}': {ex.Message}");
            }
        }

        private void OnQueued()
        {
            Console.WriteLine("🚦 DAG run is queued...");
        }

        private void OnRunning()
        {
            Console.WriteLine("🏃 DAG run is running...");
        }

        private void OnSuccess()
        {
            Console.WriteLine("✅ DAG run succeeded!");
        }

        private void OnFailed()
        {
            Console.WriteLine("❌ DAG run failed!");
        }


    }
}
