using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace XrAiGoogle
{

    #region Main Class
    public class GoogleImageToText : IXrAiImageToText
    {
        private HttpClient _httpClient = new();
        private Dictionary<string, string> _globalOptions = new();

        public GoogleImageToText(Dictionary<string, string> options)
        {
            _globalOptions = options;
        }

        public async Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, Dictionary<string, string> options = null)
        {
            try {
                string apiKey = GetOption("apiKey", options);
                string prompt = GetOption("prompt", options);
                string url = GetOption("url", options);

                return await Execute(imageBytes, imageFormat, apiKey, url, prompt);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }


        private async Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, string apiKey, string url, string prompt)
        {
            string base64Image = Convert.ToBase64String(imageBytes);

            object requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                inline_data = new
                                {
                                    // mime_type = "image/jpeg",
                                    mime_type = imageFormat,
                                    data = base64Image
                                }
                            },
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };
            string jsonData = JsonConvert.SerializeObject(requestData);
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using HttpRequestMessage request = new(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", apiKey);
            request.Content = content;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                return XrAiResult.Failure<string>($"Error in Google Gemini Vision request: {response.StatusCode} - {errorContent}");
            }

            string responseText = await response.Content.ReadAsStringAsync();

            GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);

            string resultContent = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;
            if (string.IsNullOrEmpty(resultContent))
            {
                return XrAiResult.Failure<string>("No content received from Google Gemini API");
            }

            return XrAiResult.Success(resultContent);
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

        #region JSON Classes
        [Serializable]
        private class GeminiResponse
        {
            public GeminiCandidate[] candidates;
        }

        [Serializable]
        private class GeminiCandidate
        {
            public GeminiContent content;
        }

        [Serializable]
        private class GeminiContent
        {
            public GeminiPart[] parts;
        }

        [Serializable]
        private class GeminiPart
        {
            public string text;
        }
        #endregion
    }
    #endregion

}
