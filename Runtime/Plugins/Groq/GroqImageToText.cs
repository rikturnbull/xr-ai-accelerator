using GroqApiLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("Groq")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "Groq API key for authentication")]
    [XrAiOption("model", XrAiOptionScope.Both, isRequired: true, defaultValue: "llama-3.2-11b-vision-preview", description: "The vision model to use")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: true, description: "The prompt to use for image analysis")]
    public class GroqImageToText : IXrAiImageToText
    {
        private GroqApiClient _groqApi;
        private Dictionary<string, string> _globalOptions = null;

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
            _groqApi = new GroqApiClient(GetOption("apiKey"));
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            byte[] imageBytes = texture.EncodeToPNG();

            XrAiResult<string> result = await ExecuteGroqRequest(imageBytes, options);
            if (result.IsSuccess)
            {
                callback?.Invoke(result);
            }
            else
            {
                callback?.Invoke(
                    XrAiResult.Failure<string>(result.ErrorMessage)
                );
            }
        }

        private async Task<XrAiResult<string>> ExecuteGroqRequest(byte[] imageBytes, Dictionary<string, string> options = null)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new ArgumentException("Texture is empty or not valid.");
                }

                string model = GetOption("model", options);
                string prompt = GetOption("prompt", options);
                string base64Image = Convert.ToBase64String(imageBytes);

                JObject response = await _groqApi.CreateVisionCompletionWithTempBase64ImageAsync(base64Image, prompt, model);

                string result = response?["choices"]?[0]?["message"]?["content"]?.ToString();
                return XrAiResult.Success(result);
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