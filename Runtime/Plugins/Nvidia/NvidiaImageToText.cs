using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    #region Main Class
    [XrAiProvider("Nvidia")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "NVIDIA API key for authentication")]
    [XrAiOption("model", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "meta/llama-3.2-90b-vision-instruct", description: "The model to use for image-to-text conversion")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "Describe the image", description: "The prompt to use for image-to-text conversion")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "https://api.nvidia.com/v1/generate", description: "The URL for the NVIDIA API endpoint")]
    public class NvidiaImageToText : IXrAiImageToText
    {
        private HttpClient _httpClient = new();
        private Dictionary<string, string> _globalOptions = new();

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options;
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            byte[] imageBytes = texture.EncodeToPNG();
            string prompt = GetOption("prompt", options);

            XrAiResult<string> result = await ExecuteNvidiaRequest(imageBytes, options);
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

        private async Task<XrAiResult<string>> ExecuteNvidiaRequest(byte[] imageBytes, Dictionary<string, string> options = null)
        {
            try
            {
                string apiKey = GetOption("apiKey", options);
                string prompt = GetOption("prompt", options);
                string url = GetOption("url", options);
                string model = GetOption("model", options);

                string base64Image = Convert.ToBase64String(imageBytes);
                string mimeType = XrAiImageHelper.DetectImageFormat(imageBytes);

                NvidiaRequest requestData = CreateJsonRequest(model, prompt, mimeType, base64Image);

                string jsonData = JsonConvert.SerializeObject(requestData);
                Debug.Log($"NVIDIA API Request: {jsonData}");
                StringContent content = new(jsonData, Encoding.UTF8, "application/json");

                using HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;
                request.Content.Headers.ContentType.CharSet = null;

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<string>($"Error in NVIDIA API request: {response.StatusCode} - {errorContent}");
                }

                string responseText = await response.Content.ReadAsStringAsync();
                NvidiaResponse nvidiaResponse = JsonConvert.DeserializeObject<NvidiaResponse>(responseText);

                string resultContent = nvidiaResponse?.choices?[0]?.message?.content;
                if (string.IsNullOrEmpty(resultContent))
                {
                    return XrAiResult.Failure<string>("No content received from NVIDIA API");
                }

                return XrAiResult.Success(resultContent);
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

        private NvidiaRequest CreateJsonRequest(string model, string prompt, string mimeType, string base64Image)
        {
            return new NvidiaRequest
            {
                model = model,
                messages = new[]
                {
                    new NvidiaMessage
                    {
                        role = "user",
                        content = $"{prompt} <img src=\"data:{mimeType};base64,{base64Image}\" />"
                    }
                },
                max_tokens = 512,
                temperature = 1.00f,
                top_p = 1.00f,
                frequency_penalty = 0.00f,
                presence_penalty = 0.00f,
                stream = false
            };
        }
    }
    #endregion
}
