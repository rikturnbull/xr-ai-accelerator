using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace XrAiAccelerator
{
    public abstract class GoogleAiBase
    {
        protected HttpClient _httpClient = new();
        protected XrAiOptionsHelper _optionsHelper;


        public Task Initialize(Dictionary<string, string> options = null)
        {
            _optionsHelper = new XrAiOptionsHelper(this, options);
            return Task.CompletedTask;
        }

        protected GeminiRequest CreateJsonRequest(string base64Image, string imageFormat, string prompt, GeminiGenerationConfig generationConfig = null)
        {
            GeminiRequest request = new GeminiRequest
            {
                contents = new[]
                {
                    new GeminiRequestContent
                    {
                        parts = new GeminiRequestPart[]{ }
                    }
                },
                generationConfig = generationConfig
            };

            List<GeminiRequestPart> partsList = new();
            if (base64Image != null)
            {
                partsList.Add(new GeminiRequestPart
                {
                    inline_data = new GeminiInlineData
                    {
                        mime_type = imageFormat,
                        data = base64Image
                    }
                });
            }
            if (prompt != null)
            {
                partsList.Add(new GeminiRequestPart
                {
                    text = prompt
                });
            }
            request.contents[0].parts = partsList.ToArray();

            return request;
        }

        protected async Task<XrAiResult<string>> ExecuteObjectDetectionRequest(byte[] imageBytes, string prompt, Dictionary<string, string> options = null, GeminiGenerationConfig generationConfig = null)
        {
            return await ExecuteRequest<string>(imageBytes, prompt, options, generationConfig);
        }

        protected async Task<XrAiResult<string>> ExecuteImageToTextRequest(byte[] imageBytes, string prompt, Dictionary<string, string> options = null, GeminiGenerationConfig generationConfig = null)
        {
            return await ExecuteRequest<string>(imageBytes, prompt, options, generationConfig);
        }

        protected async Task<XrAiResult<Texture2D>> ExecuteTextToImageRequest(string prompt, Dictionary<string, string> options = null, GeminiGenerationConfig generationConfig = null)
        {
            return await ExecuteRequest<Texture2D>(null, prompt, options, generationConfig);
        }

        protected async Task<XrAiResult<T>> ExecuteRequest<T>(byte[] imageBytes, string prompt, Dictionary<string, string> options = null, GeminiGenerationConfig generationConfig = null)
        {
            try
            {
                string apiKey = _optionsHelper.GetOption("apiKey", options);
                string url = _optionsHelper.GetOption("url", options);

                string base64Image = null;
                string imageFormat = null;

                if (imageBytes != null)
                {
                    imageFormat = XrAiImageHelper.DetectImageFormat(imageBytes);
                    base64Image = Convert.ToBase64String(imageBytes);
                }

                GeminiRequest requestData = CreateJsonRequest(base64Image, imageFormat, prompt, generationConfig);
                string jsonData = JsonConvert.SerializeObject(requestData);
                StringContent content = new(jsonData, Encoding.UTF8, "application/json");

                using HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = content;

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<T>($"Error in Google Gemini Vision request: {response.StatusCode} - {errorContent}");
                }

                string responseText = await response.Content.ReadAsStringAsync();

                GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);

                GeminiPart part = geminiResponse?.candidates?[0]?.content?.parts?.FirstOrDefault();
                if (part == null)
                {
                    return XrAiResult.Failure<T>("No content received from Google Gemini API");
                }

                if (typeof(T) == typeof(string) && part.text != null)
                {
                    return XrAiResult.Success((T)(object)part.text);
                }
                else if (typeof(T) == typeof(Texture2D) && part.inlineData != null)
                {
                    byte[] resultBytes = Convert.FromBase64String(part.inlineData.data);
                    Texture2D resultTexture = new(2, 2);
                    if (resultTexture.LoadImage(resultBytes))
                    {
                        return XrAiResult.Success((T)(object)resultTexture);
                    }
                    else
                    {
                        return XrAiResult.Failure<T>("Failed to load image data into Texture2D");
                    }
                }
                else
                {
                    return XrAiResult.Failure<T>($"Empty response or unsupported return type: {typeof(T)}");
                }
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<T>(ex.Message);
            }
        }
    }
}
