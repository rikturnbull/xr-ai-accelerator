using OpenAI;
using OpenAI.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    [XrAiProvider("OpenAI")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "OpenAI API key for authentication")]
    public class OpenAISpeechToText : IXrAiSpeechToText
    {
        private Dictionary<string, string> _globalOptions = new();

        private OpenAIClient _openAIClient;

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
            _openAIClient = new OpenAIClient(GetOption("apiKey"));
            return Task.CompletedTask;
        }

        public async Task Execute(byte[] audioData, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            string model = GetOption("model", options);

            Stream stream = new MemoryStream(audioData);
            AudioTranscriptionRequest request = new(stream, "name.wav", model, null, null, "en", null, AudioResponseFormat.Text);
            string result = await _openAIClient.AudioEndpoint.CreateTranscriptionTextAsync(request);

            callback?.Invoke(!string.IsNullOrEmpty(result)
                ? XrAiResult.Success(result)
                : XrAiResult.Failure<string>("No transcription result returned."));
        }

        private string GetOption(string key, Dictionary<string, string> options = null)
        {
            if (options != null && options.TryGetValue(key, out string value))
            {
                return value;
            }
            else if (_globalOptions.TryGetValue(key, out value))
            {
                return value;
            }
            throw new KeyNotFoundException($"Option '{key}' not found.");
        }
    }
}