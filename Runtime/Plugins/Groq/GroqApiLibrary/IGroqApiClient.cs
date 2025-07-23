
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GroqApiLibrary
{
    public interface IGroqApiClient
    {
        Task<JObject> CreateChatCompletionAsync(JObject request);
        IAsyncEnumerable<JObject> CreateChatCompletionStreamAsync(JObject request);
        Task<JObject> CreateTranscriptionAsync(Stream audioFile, string fileName, string model,
            string prompt = null, string responseFormat = "json", string language = null, float? temperature = null);
        Task<JObject> CreateTranslationAsync(Stream audioFile, string fileName, string model,
            string prompt = null, string responseFormat = "json", float? temperature = null);
        Task<JObject> ListModelsAsync();
        Task<string> RunConversationWithToolsAsync(string userPrompt, List<Tool> tools, string model, string systemMessage);
    }
}