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
    [XrAiOption("foregroundRatio", XrAiOptionScope.Workflow, isRequired: false, description: "Amount of padding around the object to be processed within the frame: 0.1..1.0", defaultValue: "0.85")]
    [XrAiOption("textureResolution", XrAiOptionScope.Workflow, isRequired: false, description: "Texture resolution: 512, 1024, 2048", defaultValue: "1024")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: false, description: "Stability AI API URL", defaultValue: "https://api.stability.ai/v2beta/3d/stable-fast-3d")]
    public class StabilityAiImageTo3d : IXrAiImageTo3d
    {
        private XrAiOptionsHelper _optionsHelper;
        private HttpClient _client = new HttpClient();

        public Task Initialize(Dictionary<string, string> options = null)
        {
            _optionsHelper = new XrAiOptionsHelper(this, options);
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback)
        {
            try
            {
                byte[] imageBytes = texture.EncodeToPNG();
                string apiKey = _optionsHelper.GetOption("apiKey", options);
                string foregroundRatio = _optionsHelper.GetOption("foregroundRatio", options);
                string textureResolution = _optionsHelper.GetOption("textureResolution", options);
                string url = _optionsHelper.GetOption("url", options);
                XrAiResult<byte[]> result = await Execute(imageBytes, apiKey, foregroundRatio, textureResolution, url);

                callback?.Invoke(result);
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<byte[]>($"Exception in StabilityAiImageTo3d: {ex.Message}")
                );
            }
        }

        private async Task<XrAiResult<byte[]>> Execute(
            byte[] imageBytes,
            string apiKey,
            string foregroundRatio,
            string textureResolution,
            string url
        )
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                using var form = new MultipartFormDataContent
                {
                    ImageFormField("image", imageBytes, "image/png", "image.png"),
                    StringFormField("texture_resolution", textureResolution),
                    StringFormField("foreground_ratio", foregroundRatio)
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