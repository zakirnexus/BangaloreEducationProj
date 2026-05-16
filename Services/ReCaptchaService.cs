using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SchoolProject.Services
{
    public class ReCaptchaService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReCaptchaService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsValidAsync(string token, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secretKey = _config["ReCaptcha:SecretKey"];
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey!),
                    new KeyValuePair<string, string>("response", token),
                    new KeyValuePair<string, string>("remoteip", ipAddress)
                })
            );

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json).RootElement;

            bool success = result.GetProperty("success").GetBoolean();
            double score = result.TryGetProperty("score", out var s) ? s.GetDouble() : 0;

            // Score: 1.0 = human, 0.0 = bot. Threshold 0.5 is standard
            return success && score >= 0.5;
        }
    }
}