using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace XrAiAccelerator
{
    #region Main Class
    public class NvidiaImageToText : IXrAiImageToText
    {
        private HttpClient _httpClient = new();
        private Dictionary<string, string> _globalOptions = new();

        public NvidiaImageToText(Dictionary<string, string> options)
        {
            _globalOptions = options;
        }

        public async Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, Dictionary<string, string> options = null)
        {
            try 
            {
                string apiKey = GetOption("apiKey", options);
                string prompt = GetOption("prompt", options);
                string url = GetOption("url", options);
                string model = GetOption("model", options);
                
                return await Execute(imageBytes, imageFormat, apiKey, url, prompt, model);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }

        private async Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, string apiKey, string url, string prompt, string model)
        {
            string base64Image = Convert.ToBase64String(imageBytes);
            string mimeType = GetMimeType(imageFormat);

            object requestData = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"{prompt} <img src=\"data:{mimeType};base64,{base64Image}\" />"
                    }
                },
                max_tokens = 512,
                temperature = 1.00,
                top_p = 1.00,
                frequency_penalty = 0.00,
                presence_penalty = 0.00,
                stream = false
            };

            string jsonData = JsonConvert.SerializeObject(requestData);
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

        private string GetOption(string key, Dictionary<string, string> options = null, string defaultValue = null)
        {
            if (options != null && options.TryGetValue(key, out string value))
            {
                return value;
            }
            else if (_globalOptions.TryGetValue(key, out value))
            {
                return value;
            }
            
            if (defaultValue != null)
            {
                return defaultValue;
            }
            
            throw new KeyNotFoundException($"Option '{key}' not found.");
        }

        private string GetMimeType(string imageFormat)
        {
            return imageFormat switch
            {
                "image/jpeg" => "image/jpeg",
                "image/jpg" => "image/jpeg",
                "image/png" => "image/png",
                "image/gif" => "image/gif",
                "image/webp" => "image/webp",
                _ => "image/png"
            };
        }

        #region JSON Classes
        [Serializable]
        private class NvidiaResponse
        {
            public NvidiaChoice[] choices;
        }

        [Serializable]
        private class NvidiaChoice
        {
            public NvidiaMessage message;
        }

        [Serializable]
        private class NvidiaMessage
        {
            public string content;
        }
        #endregion
    }
    #endregion
}
