using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using GroqApiLibrary;

namespace XrAiAccelerator
{
    public class GroqImageToText : IXrAiImageToText
    {
        private GroqApiClient _groqApi;
        private Dictionary<string, string> _globalOptions = new();

        public GroqImageToText(Dictionary<string, string> options)
        {
            _globalOptions = options;
            _groqApi = new GroqApiClient(GetOption("apiKey"));
        }

        public async Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, Dictionary<string, string> options = null)
        {
            try
            {
                string model = GetOption("model", options);
                string prompt = GetOption("prompt", options);
                string result = await Execute(imageBytes, imageFormat, prompt, model);
                return XrAiResult.Success(result);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }

        private async Task<string> Execute(byte[] imageBytes, string imageFormat, string prompt, string model)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("Texture is empty or not valid.");
            }
            string base64Image = Convert.ToBase64String(imageBytes);

            JsonObject response = await _groqApi.CreateVisionCompletionWithTempBase64ImageAsync(base64Image, prompt, model);

            return response?["choices"]?[0]?["message"]?["content"]?.ToString();
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