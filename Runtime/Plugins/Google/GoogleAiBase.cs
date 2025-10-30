using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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
                string apiKey = _optionsHelper.GetOption("apiKey", options);
                string url = _optionsHelper.GetOption("url", options);

                string imageFormat = XrAiImageHelper.DetectImageFormat(imageBytes);
                string base64Image = Convert.ToBase64String(imageBytes);

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
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }
    }
}
