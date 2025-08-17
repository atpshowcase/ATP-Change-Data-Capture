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

            var requestBody = new
            {
                conf = payload,
                dag_run_id = $"dagrun_{Guid.NewGuid()}"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ DAG '{dagId}' triggered successfully.");
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Failed to trigger DAG '{dagId}'. Status: {response.StatusCode}, Body: {body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Error triggering DAG '{dagId}': {ex.Message}");
            }
        }
    }
}
