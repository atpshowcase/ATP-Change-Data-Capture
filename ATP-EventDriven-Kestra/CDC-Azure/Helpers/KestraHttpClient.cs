using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CDC_Azure.Config;

namespace CDC_Azure.Helpers
{
    public static class KestraHttpClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static KestraHttpClient()
        {
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{KestraConfig.Email}:{KestraConfig.Password}")
            );

            _httpClient.BaseAddress = new Uri(KestraConfig.Host);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);
        }

        public static HttpClient Client => _httpClient;
    }
}
