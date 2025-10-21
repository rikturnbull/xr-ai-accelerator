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
    [XrAiOption("language", XrAiOptionScope.Global, isRequired: false, defaultValue: "en", description: "The language of the audio (e.g., 'en' for English)")]
    [XrAiOption("model", XrAiOptionScope.Global, isRequired: false, defaultValue: "whisper-1", description: "The model to use for speech-to-text conversion")]
    [XrAiOption("prompt", XrAiOptionScope.Global, isRequired: false, defaultValue: "", description: "An optional text prompt to guide the transcription")]
    [XrAiOption("temperature", XrAiOptionScope.Global, defaultValue: "0.0", isRequired: false, description: "Temperature for the API request")]
    public class OpenAISpeechToText : IXrAiSpeechToText
    {
        private XrAiOptionsHelper _optionsHelper;

        private OpenAIClient _openAIClient;

        public Task Initialize(Dictionary<string, string> options = null)
        {
            _optionsHelper = new XrAiOptionsHelper(this, options);
            _openAIClient = new OpenAIClient(_optionsHelper.GetOption("apiKey"));
            return Task.CompletedTask;
        }

        public async Task Execute(byte[] audioData, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            try
            {
                string language = _optionsHelper.GetOption("language", options);
                string model = _optionsHelper.GetOption("model", options);
                string prompt = _optionsHelper.GetOption("prompt", options);
                float temperature = _optionsHelper.GetFloatOption("temperature", options);

                Stream stream = new MemoryStream(audioData);
                AudioTranscriptionRequest request = new(stream, "name.wav", model, null, null, language, prompt, AudioResponseFormat.Text, temperature);
                string result = await _openAIClient.AudioEndpoint.CreateTranscriptionTextAsync(request);

                callback?.Invoke(!string.IsNullOrEmpty(result)
                    ? XrAiResult.Success(result)
                    : XrAiResult.Failure<string>("No transcription result returned."));
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<string>($"Exception in OpenAISpeechToText: {ex.Message}")
                );
            }
        }
    }
}