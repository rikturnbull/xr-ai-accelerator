using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace XrAiAccelerator
{
    public abstract class GoogleAiBase
    {
        protected HttpClient _httpClient = new();
        protected Dictionary<string, string> _globalOptions = null;

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
            return Task.CompletedTask;
        }

        protected string GetOption(string key, Dictionary<string, string> options = null)
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

        protected GeminiRequest CreateJsonRequest(string base64Image, string imageFormat, string prompt, GeminiGenerationConfig generationConfig = null)
        {
            return new GeminiRequest
            {
                contents = new[]
                {
                    new GeminiRequestContent
                    {
                        parts = new GeminiRequestPart[]
                        {
                            new()
                            {
                                inline_data = new GeminiInlineData
                                {
                                    mime_type = imageFormat,
                                    data = base64Image
                                }
                            },
                            new()
                            {
                                text = prompt
                            }
                        }
                    }
                },
                generationConfig = generationConfig
            };
        }

        protected async Task<XrAiResult<string>> ExecuteGeminiRequest(byte[] imageBytes, string prompt, Dictionary<string, string> options = null, GeminiGenerationConfig generationConfig = null)
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Debug.Log($"Google:Gemini:1:{stopwatch.ElapsedMilliseconds}ms - Starting Execute");
                string apiKey = GetOption("apiKey", options);
                string url = GetOption("url", options);
                
                Debug.Log($"Google:Gemini:2:{stopwatch.ElapsedMilliseconds}ms - Converting");
                string imageFormat = XrAiImageHelper.DetectImageFormat(imageBytes);
                string base64Image = Convert.ToBase64String(imageBytes);

                Debug.Log($"Google:Gemini:3:{stopwatch.ElapsedMilliseconds}ms - Creating Request");
                GeminiRequest requestData = CreateJsonRequest(base64Image, imageFormat, prompt, generationConfig);
                string jsonData = JsonConvert.SerializeObject(requestData);
                StringContent content = new(jsonData, Encoding.UTF8, "application/json");

                Debug.Log($"Google:Gemini:4:{stopwatch.ElapsedMilliseconds}ms - Sending Request");

                using HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = content;
                Debug.Log($"Google:Gemini:5:{stopwatch.ElapsedMilliseconds}ms - Request Sent");

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<string>($"Error in Google Gemini Vision request: {response.StatusCode} - {errorContent}");
                }
                Debug.Log($"Google:Gemini:6:{stopwatch.ElapsedMilliseconds}ms - Request Completed");

                string responseText = await response.Content.ReadAsStringAsync();
                Debug.Log($"Google:Gemini:7:{stopwatch.ElapsedMilliseconds}ms - Response Received");

                GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                Debug.Log($"Google:Gemini:8:{stopwatch.ElapsedMilliseconds}ms - Response Parsed");

                string resultContent = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;
                if (string.IsNullOrEmpty(resultContent))
                {
                    return XrAiResult.Failure<string>("No content received from Google Gemini API");
                }

                return XrAiResult.Success(resultContent);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }
    }
}
