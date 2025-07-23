using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Audio;
using System;
using UnityEngine;

public class OpenAITextToSpeech : IXrAiTextToSpeech
{
    private Dictionary<string, string> _globalOptions = new();

    private OpenAIClient _openAIClient;

    public OpenAITextToSpeech(Dictionary<string, string> options)
    {
        _globalOptions = options;
        _openAIClient = new OpenAIClient(GetOption("apiKey"));
    }

    public async Task<XrAiResult<AudioClip>> Execute(string text, Dictionary<string, string> options = null)
    {
        return await Execute(text);
    }

    private async Task<XrAiResult<AudioClip>> Execute(string text)
    {
        try
        {
            SpeechRequest request = new SpeechRequest(text);
            AudioClip speechClip = await _openAIClient.AudioEndpoint.GetSpeechAsync(request);
            return XrAiResult.Success(speechClip);
        }
        catch (Exception ex)
        {
            return XrAiResult.Failure<AudioClip>(ex.Message);
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
