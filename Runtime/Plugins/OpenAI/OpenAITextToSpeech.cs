using OpenAI;
using OpenAI.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider(PROVIDER_NAME)]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "OpenAI API key for authentication")]
    public class OpenAITextToSpeech : IXrAiTextToSpeech
    {
        private const string PROVIDER_NAME = "OpenAI";
        private Dictionary<string, string> _globalOptions = new();

        private OpenAIClient _openAIClient;

        public Task Initialize(Dictionary<string, string> globalOptions)
        {
            _globalOptions = globalOptions;
            _openAIClient = new OpenAIClient(GetOption("apiKey"));
            return Task.CompletedTask;
        }

        public async Task Execute(string text, Dictionary<string, string> workflowOptions, Action<XrAiResult<AudioClip>> callback)
        {
            SpeechRequest request = new SpeechRequest(text);
            SpeechClip result = await _openAIClient.AudioEndpoint.GetSpeechAsync(request);

            callback?.Invoke(result != null
                ? XrAiResult.Success(result.AudioClip)
                : XrAiResult.Failure<AudioClip>("No audio clip returned from OpenAI Text-to-Speech."));
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
