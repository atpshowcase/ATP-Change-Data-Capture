using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CDC_Azure.Config;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CDC_Azure.Helpers
{
    public static class KestraTrigger
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static async Task<string> CreateOrUpdateFlowAsync(string flowYaml)
        {
            var url = $"{KestraConfig.Host}/api/v1/flows";
            var content = new StringContent(flowYaml);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-yaml");

            Console.WriteLine($"[KestraTrigger] Mengirim permintaan POST untuk membuat/memperbarui flow ke: {url}");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var flowId = "";

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[KestraTrigger] Pembuatan/pembaruan flow Kestra gagal: {response.StatusCode} - {responseContent}");
                //throw new Exception($"Kestra flow creation/update failed: {response.StatusCode} - {responseContent}");
            }
            else
            {
                using var doc = JsonDocument.Parse(responseContent);
                flowId = doc.RootElement.GetProperty("id").GetString();
                Console.WriteLine($"[KestraTrigger] Flow Kestra berhasil dibuat/diperbarui! Flow ID: {flowId}");

            }

            return flowId!;
        }

        public static async Task<string> TriggerFlowMultipartAsync(string flowYaml, Dictionary<string, object> inputs)
        {
            await CreateOrUpdateFlowAsync(flowYaml);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // Sesuaikan jika YAML Anda menggunakan snake_case atau lainnya
                .Build();
            var flowData = deserializer.Deserialize<Dictionary<object, object>>(flowYaml);

            string? strNameSpace = flowData["namespace"]?.ToString();
            string? strFlowID = flowData["id"]?.ToString();

            var url = $"{KestraConfig.Host}/api/v1/main/executions/{strNameSpace}/{strFlowID}?labels=key:mstOrder3";

            using var form = new MultipartFormDataContent();

            var jsonOptions = new JsonSerializerOptions { WriteIndented = false };

            foreach (var kv in inputs)
            {
                string value;

                if (kv.Value is string str)
                    value = str;
                else
                    value = JsonSerializer.Serialize(kv.Value, jsonOptions);

                var content = new StringContent(value);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = $"\"{kv.Key}\""
                };

                form.Add(content);
            }

            var response = await _httpClient.PostAsync(url, form);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Kestra trigger failed: {response.StatusCode} - {json}");

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString()!;
        }
    }
}
