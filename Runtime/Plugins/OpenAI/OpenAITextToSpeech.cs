using OpenAI;
using OpenAI.Audio;
using OpenAI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
        //    [FunctionProperty("The voice to use when generating the audio.", true, "alloy", new object[] { "echo", "fable", "onyx", "nova", "shimmer" })]

    [XrAiProvider(PROVIDER_NAME)]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "OpenAI API key for authentication")]
    [XrAiOption("speed", XrAiOptionScope.Global, isRequired: false, defaultValue: "1.0", description: "The speed of the generated audio - a value from 0.25 to 4.0. 1.0 is the default")]
    [XrAiOption("voice", XrAiOptionScope.Global, isRequired: false, defaultValue: "alloy", description: "The voice to use for text-to-speech generation (e.g., 'alloy', 'echo', 'fable', 'onyx', 'nova', 'shimmer')")]
    public class OpenAITextToSpeech : IXrAiTextToSpeech
    {
        private const string PROVIDER_NAME = "OpenAI";
        private XrAiOptionsHelper _optionsHelper;

        private OpenAIClient _openAIClient;

        public Task Initialize(Dictionary<string, string> globalOptions)
        {
            _optionsHelper = new XrAiOptionsHelper(this, globalOptions);
            _openAIClient = new OpenAIClient(_optionsHelper.GetOption("apiKey"));
            return Task.CompletedTask;
        }

        public async Task Execute(string text, Dictionary<string, string> workflowOptions, Action<XrAiResult<AudioClip>> callback)
        {
            try
            {
                SpeechRequest request = new(
                    text,
                    Model.TTS_1,
                    GetVoice(_optionsHelper.GetOption("voice", workflowOptions)),
                    null,
                    SpeechResponseFormat.MP3,
                    float.Parse(_optionsHelper.GetOption("speed", workflowOptions))
                );
                SpeechClip result = await _openAIClient.AudioEndpoint.GetSpeechAsync(request);

                callback?.Invoke(result != null
                    ? XrAiResult.Success(result.AudioClip)
                    : XrAiResult.Failure<AudioClip>("No audio clip returned from OpenAI Text-to-Speech."));
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<AudioClip>($"Exception in OpenAITextToSpeech: {ex.Message}")
                );
            }
        }

        private Voice GetVoice(string voiceName)
        {
            return voiceName.ToLower() switch
            {
                "echo" => Voice.Echo,
                "fable" => Voice.Fable,
                "onyx" => Voice.Onyx,
                "nova" => Voice.Nova,
                "shimmer" => Voice.Shimmer,
                _ => Voice.Alloy,
            };
        }
    }
}
