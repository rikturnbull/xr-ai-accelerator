using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Audio;
using System;
using System.IO;

namespace XrAiAccelerator
{
    public class OpenAISpeechToText : IXrAiSpeechToText
    {
        private Dictionary<string, string> _globalOptions = new();

        private OpenAIClient _openAIClient;

        public OpenAISpeechToText(Dictionary<string, string> options)
        {
            _globalOptions = options;
            _openAIClient = new OpenAIClient(GetOption("apiKey"));
        }

        public async Task<XrAiResult<string>> Execute(byte[] audioData, Dictionary<string, string> options = null)
        {
            try 
            {
                string model = GetOption("model", options);

                return await Execute(audioData, model);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }

        private async Task<XrAiResult<string>> Execute(byte[] audioData, string model)
        {
            try
            {
                Stream stream = new MemoryStream(audioData);
                AudioTranscriptionRequest request = new(stream, "name.wav", model, null, null, "en", null, AudioResponseFormat.Text);
                string transcription = await _openAIClient.AudioEndpoint.CreateTranscriptionTextAsync(request);
                return XrAiResult.Success(transcription);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
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