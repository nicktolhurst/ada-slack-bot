using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ada.Services
{
    public class FactClient : HttpClient
    {
        public FactClient(string apiToken) : base()
        {
            BaseAddress = new Uri("https://api.api-ninjas.com/v1/");

            DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", apiToken);
        }

        public async Task<string> GetFactAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetAsync($"facts?limit=1", cancellationToken);

            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

            var facts = JsonSerializer.Deserialize<Fact[]>(responseData);

            return facts[0].Text;
        }

        private class Fact
        {
            [JsonPropertyName("fact")]
            public string Text { get; set; }
        }
    }
}