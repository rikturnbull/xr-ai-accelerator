using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("Nvidia")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "NVIDIA API key for authentication")]
    [XrAiOption("frequencyPenalty", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "0.0", description: "Frequency penalty for the response")]
    [XrAiOption("imageQuality", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "100", description: "JPEG quality (1-100, lower = smaller file)")]
    [XrAiOption("maxImageDimension", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "512", description: "Maximum width or height for the image before encoding (e.g., 128, 256, 384)")]
    [XrAiOption("maxTokens", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "512", description: "Maximum number of tokens in the response")]
    [XrAiOption("model", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "meta/llama-3.2-90b-vision-instruct", description: "The model to use for image-to-text conversion")]
    [XrAiOption("presencePenalty", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "0.0", description: "Presence penalty for the response")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "Describe the image", description: "The prompt to use for image-to-text conversion")]
    [XrAiOption("temperature", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "1.0", description: "Sampling temperature for the response")]
    [XrAiOption("topP", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "1.0", description: "Top-p sampling value for the response")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "https://integrate.api.nvidia.com/v1/chat/completions", description: "The URL for the NVIDIA API endpoint")]
    public class NvidiaImageToText : IXrAiImageToText
    {
        private HttpClient _httpClient = new();
        private XrAiOptionsHelper _optionsHelper;

        public Task Initialize(Dictionary<string, string> options = null)
        {
            _optionsHelper = new XrAiOptionsHelper(this, options);
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            try
            {
                string url = _optionsHelper.GetOption("url", options);
                string apiKey = _optionsHelper.GetOption("apiKey", options);
                float frequencyPenalty = _optionsHelper.GetFloatOption("frequencyPenalty", options);
                int imageQuality = _optionsHelper.GetIntOption("imageQuality", options);
                int maxDimension = _optionsHelper.GetIntOption("maxImageDimension", options);
                int maxTokens = _optionsHelper.GetIntOption("maxTokens", options);
                float temperature = _optionsHelper.GetFloatOption("temperature", options);
                float topP = _optionsHelper.GetFloatOption("topP", options);
                float presencePenalty = _optionsHelper.GetFloatOption("presencePenalty", options);
                string prompt = _optionsHelper.GetOption("prompt", options);

                Texture2D scaledTexture = XrAiImageHelper.ScaleTexture(texture, maxDimension);
                
                await Task.Yield();
                byte[] imageBytes = scaledTexture.EncodeToJPG(imageQuality);
                
                UnityEngine.Object.Destroy(scaledTexture);
                
                await Task.Yield();

                string model = _optionsHelper.GetOption("model", options);

                string base64Image = Convert.ToBase64String(imageBytes);
                string mimeType = XrAiImageHelper.DetectImageFormat(imageBytes);

                NvidiaRequest requestData = CreateJsonRequest(
                    model,
                    prompt,
                    mimeType,
                    base64Image,
                    maxTokens,
                    frequencyPenalty,
                    presencePenalty,
                    temperature,
                    topP
                );

                string jsonData = JsonConvert.SerializeObject(requestData);
                Debug.Log($"NVIDIA Request JSON: {jsonData}");
                StringContent content = new(jsonData, Encoding.UTF8, "application/json");

                using HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;
                request.Content.Headers.ContentType.CharSet = null;

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    callback?.Invoke(
                        XrAiResult.Failure<string>($"Error in NVIDIA API request: {response.StatusCode} - {errorContent}")
                    );
                    return;
                }

                string responseText = await response.Content.ReadAsStringAsync();
                NvidiaResponse nvidiaResponse = JsonConvert.DeserializeObject<NvidiaResponse>(responseText);

                string resultContent = nvidiaResponse?.choices?[0]?.message?.content;
                if (string.IsNullOrEmpty(resultContent))
                {
                    callback?.Invoke(XrAiResult.Failure<string>("No content received from NVIDIA API"));
                    return;
                }

                callback?.Invoke(XrAiResult.Success(resultContent));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                callback?.Invoke(XrAiResult.Failure<string>(ex.Message));
            }
        }

        private NvidiaRequest CreateJsonRequest(string model, string prompt, string mimeType,
            string base64Image, int maxTokens, float frequencyPenalty, float presencePenalty, float temperature, float topP)
        {
            // Use OpenAI-compatible vision format with content array
            var contentParts = new NvidiaContentPart[]
            {
                new NvidiaContentPart
                {
                    type = "text",
                    text = prompt
                },
                new NvidiaContentPart
                {
                    type = "image_url",
                    image_url = new NvidiaImageUrl
                    {
                        url = $"data:{mimeType};base64,{base64Image}"
                    }
                }
            };

            return new NvidiaRequest
            {
                model = model,
                messages = new[]
                {
                    new NvidiaMessage
                    {
                        role = "user",
                        content = contentParts
                    }
                },
                max_tokens = maxTokens,
                temperature = temperature,
                top_p = topP,
                frequency_penalty = frequencyPenalty,
                presence_penalty = presencePenalty,
                stream = false
            };
        }
    }
}
