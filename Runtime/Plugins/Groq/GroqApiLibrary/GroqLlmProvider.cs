using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GroqApiLibrary
{
    public interface ILlmProvider
    {
        Task<string> GenerateAsync(string prompt);
    }


    public class GroqLlmProvider : ILlmProvider, IDisposable
    {
        private readonly GroqApiClient _client;
        private readonly string _model;

        public GroqLlmProvider(string apiKey, string model)
        {
            _client = new GroqApiClient(apiKey);
            _model = model;
        }

        public GroqLlmProvider(string apiKey, string model, HttpClient httpClient)
        {
            _client = new GroqApiClient(apiKey, httpClient);
            _model = model;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var request = new JObject
            {
                ["model"] = _model,
                ["messages"] = JArray.FromObject(new[]
                {
                new { role = "user", content = prompt }
            })
            };

            var response = await _client.CreateChatCompletionAsync(request);
            return response?["choices"]?[0]?["message"]?["content"]?.Value<string>() ?? string.Empty;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
