using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("StabilityAi")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "Stability AI API key for authentication")]
    [XrAiOption("url", XrAiOptionScope.Global, isRequired: true, description: "Stability AI API URL", defaultValue: "https://api.stability.ai/v2beta/3d/stable-fast-3d")]
    public class StabilityAiImageTo3d : IXrAiImageTo3d
    {
        private Dictionary<string, string> _globalOptions = new();
        private HttpClient _client = new HttpClient();

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback)
        {
            byte[] imageBytes = texture.EncodeToPNG();
            string apiKey = GetOption("apiKey", options);
            string url = GetOption("url", options);
            XrAiResult<byte[]> result = await Execute(imageBytes, apiKey, url);

            callback?.Invoke(result);
        }

        private async Task<XrAiResult<byte[]>> Execute(byte[] imageBytes, string apiKey, string url)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                using var form = new MultipartFormDataContent
                {
                    ImageFormField("image", imageBytes, "image/png", "image.png")
                };

                HttpResponseMessage response = await _client.PostAsync(url, form);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<byte[]>($"Error in StabilityAI request {response.StatusCode}: {errorContent}");
                }

                byte[] result = await response.Content.ReadAsByteArrayAsync();
                return XrAiResult.Success(result);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<byte[]>(ex.Message);
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

        private static StringContent StringFormField(string name, string value)
        {
            var formField = new StringContent(value);
            formField.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = $"\"{name}\"" };
            return formField;
        }

        private static ByteArrayContent ImageFormField(string name, byte[] imageBytes, string mimeType, string filename)
        {
            var formField = new ByteArrayContent(imageBytes);
            formField.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            formField.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = $"\"{name}\"", FileName = $"\"{filename}\"" };
            return formField;
        }
    }
}