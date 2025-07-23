using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    public class StabilityAiImageTo3d : IXrAImageTo3d
    {
        private Dictionary<string, string> _globalOptions = new();
        private HttpClient _client = new HttpClient();

        public StabilityAiImageTo3d(Dictionary<string, string> options = null)
        {
            _globalOptions = options;
        }

        public async Task<XrAiResult<byte[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
        {
            try
            {
                string apiKey = GetOption("apiKey", options);
                return await Execute(texture, apiKey);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<byte[]>(ex.Message);                
            }
        }

        private async Task<XrAiResult<byte[]>> Execute(Texture2D texture, string apiKey)
        {
            try
            {
                byte[] imageBytes = texture.EncodeToPNG();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                using var form = new MultipartFormDataContent
                {
                    ImageFormField("image", imageBytes, "image/png", "image.png")
                };

                HttpResponseMessage response = await _client.PostAsync("https://api.stability.ai/v2beta/3d/stable-fast-3d", form);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<byte[]>($"Request failed with status code {response.StatusCode}: {errorContent}");
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