using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GroqApiLibrary
{
    public class GroqApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.groq.com/openai/v1";
        private const string ChatCompletionsEndpoint = "/chat/completions";
        private const string TranscriptionsEndpoint = "/audio/transcriptions";
        private const string TranslationsEndpoint = "/audio/translations";

        private const string VisionModels = "meta-llama/llama-4-scout-17b-16e-instruct,meta-llama/llama-4-maverick-17b-128e-instruct";
        private const int MAX_IMAGE_SIZE_MB = 20;
        private const int MAX_BASE64_SIZE_MB = 4;

        public GroqApiClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public GroqApiClient(string apiKey, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        private async Task<string> ConvertImageToBase64(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException($"Image file not found: {imagePath}");

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        public async Task<JObject> CreateChatCompletionAsync(JObject request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl + ChatCompletionsEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}. Response content: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(responseContent);
        }

        public async IAsyncEnumerable<JObject> CreateChatCompletionStreamAsync(JObject request)
        {
            request["stream"] = true;
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, BaseUrl + ChatCompletionsEndpoint) { Content = content };
            using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line["data: ".Length..];
                    if (data != "[DONE]")
                    {
                        yield return JsonConvert.DeserializeObject<JObject>(data);
                    }
                }
            }
        }

        public async Task<JObject> CreateTranscriptionAsync(Stream audioFile, string fileName, string model,
            string prompt = null, string responseFormat = "json", string language = null, float? temperature = null)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(audioFile), "file", fileName);
            content.Add(new StringContent(model), "model");

            if (!string.IsNullOrEmpty(prompt))
                content.Add(new StringContent(prompt), "prompt");

            content.Add(new StringContent(responseFormat), "response_format");

            if (!string.IsNullOrEmpty(language))
                content.Add(new StringContent(language), "language");

            if (temperature.HasValue)
                content.Add(new StringContent(temperature.Value.ToString()), "temperature");

            var response = await _httpClient.PostAsync(BaseUrl + TranscriptionsEndpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(responseContent);
        }

        public async Task<JObject> CreateTranslationAsync(Stream audioFile, string fileName, string model,
            string prompt = null, string responseFormat = "json", float? temperature = null)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(audioFile), "file", fileName);
            content.Add(new StringContent(model), "model");

            if (!string.IsNullOrEmpty(prompt))
                content.Add(new StringContent(prompt), "prompt");

            content.Add(new StringContent(responseFormat), "response_format");

            if (temperature.HasValue)
                content.Add(new StringContent(temperature.Value.ToString()), "temperature");

            var response = await _httpClient.PostAsync(BaseUrl + TranslationsEndpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(responseContent);
        }

        public async Task<JObject> CreateVisionCompletionAsync(JObject request)
        {
            ValidateVisionModel(request);
            return await CreateChatCompletionAsync(request);
        }

        public async Task<JObject> CreateVisionCompletionWithImageUrlAsync(
            string imageUrl,
            string prompt,
            string model = "llama-3.2-90b-vision-preview",
            float? temperature = null)
        {
            ValidateImageUrl(imageUrl);

            var request = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = prompt
                            },
                            new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = imageUrl
                                }
                            }
                        }
                    }
                }
            };

            if (temperature.HasValue)
            {
                request["temperature"] = temperature.Value;
            }

            return await CreateVisionCompletionAsync(request);
        }

        public async Task<JObject> CreateVisionCompletionWithBase64ImageAsync(
            string imagePath,
            string prompt,
            string model = "llama-3.2-90b-vision-preview",
            float? temperature = null)
        {
            var base64Image = await ConvertImageToBase64(imagePath);
            ValidateBase64Size(base64Image);

            var request = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = prompt
                            },
                            new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                }
            };

            if (temperature.HasValue)
            {
                request["temperature"] = temperature.Value;
            }

            return await CreateVisionCompletionAsync(request);
        }

        public async Task<JObject> CreateVisionCompletionWithTempBase64ImageAsync(
            string base64Image,
            string prompt,
            string model = "llama-3.2-90b-vision-preview",
            float? temperature = null)
        {
            ValidateBase64Size(base64Image);

            var request = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = prompt
                            },
                            new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                }
            };

            if (temperature.HasValue)
            {
                request["temperature"] = temperature.Value;
            }

            return await CreateVisionCompletionAsync(request);
        }

        public async Task<JObject> CreateVisionCompletionWithToolsAsync(
            string imageUrl,
            string prompt,
            List<Tool> tools,
            string model = "llama-3.2-90b-vision-preview")
        {
            ValidateImageUrl(imageUrl);

            var request = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = prompt
                            },
                            new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = imageUrl
                                }
                            }
                        }
                    }
                },
                ["tools"] = JArray.FromObject(tools.Select(t => new
                {
                    type = t.Type,
                    function = new
                    {
                        name = t.Function.Name,
                        description = t.Function.Description,
                        parameters = t.Function.Parameters
                    }
                })),
                ["tool_choice"] = "auto"
            };

            return await CreateVisionCompletionAsync(request);
        }

        public async Task<JObject> CreateVisionCompletionWithJsonModeAsync(
            string imageUrl,
            string prompt,
            string model = "llama-3.2-90b-vision-preview")
        {
            ValidateImageUrl(imageUrl);

            var request = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = prompt
                            },
                            new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = imageUrl
                                }
                            }
                        }
                    }
                },
                ["response_format"] = new JObject { ["type"] = "json_object" }
            };

            return await CreateVisionCompletionAsync(request);
        }

        public async Task<JObject> ListModelsAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{BaseUrl}/models");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            JObject responseJson = JsonConvert.DeserializeObject<JObject>(responseString);

            return responseJson;
        }

        public async Task<string> RunConversationWithToolsAsync(string userPrompt, List<Tool> tools, string model, string systemMessage)
        {
            try
            {
                var messages = new List<JObject>
                {
                    new JObject
                    {
                        ["role"] = "system",
                        ["content"] = systemMessage
                    },
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = userPrompt
                    }
                };

                var request = new JObject
                {
                    ["model"] = model,
                    ["messages"] = JArray.FromObject(messages),
                    ["tools"] = JArray.FromObject(tools.Select(t => new
                    {
                        type = t.Type,
                        function = new
                        {
                            name = t.Function.Name,
                            description = t.Function.Description,
                            parameters = t.Function.Parameters
                        }
                    })),
                    ["tool_choice"] = "auto"
                };

                Console.WriteLine($"Sending request to API: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

                var response = await CreateChatCompletionAsync(request);
                var responseMessage = response?["choices"]?[0]?["message"] as JObject;
                var toolCalls = responseMessage?["tool_calls"] as JArray;

                if (toolCalls != null && toolCalls.Count > 0)
                {
                    messages.Add(responseMessage);
                    foreach (var toolCall in toolCalls)
                    {
                        var functionName = toolCall?["function"]?["name"]?.Value<string>();
                        var functionArgs = toolCall?["function"]?["arguments"]?.Value<string>();
                        var toolCallId = toolCall?["id"]?.Value<string>();

                        if (!string.IsNullOrEmpty(functionName) && !string.IsNullOrEmpty(functionArgs))
                        {
                            var tool = tools.Find(t => t.Function.Name == functionName);
                            if (tool != null)
                            {
                                var functionResponse = await tool.Function.ExecuteAsync(functionArgs);
                                messages.Add(new JObject
                                {
                                    ["tool_call_id"] = toolCallId,
                                    ["role"] = "tool",
                                    ["name"] = functionName,
                                    ["content"] = functionResponse
                                });
                            }
                        }
                    }

                    request["messages"] = JArray.FromObject(messages);
                    var secondResponse = await CreateChatCompletionAsync(request);
                    return secondResponse?["choices"]?[0]?["message"]?["content"]?.Value<string>() ?? string.Empty;
                }

                return responseMessage?["content"]?.Value<string>() ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        private void ValidateVisionModel(JObject request)
        {
            var model = request["model"]?.Value<string>();
            if (string.IsNullOrEmpty(model) || !VisionModels.Contains(model))
            {
                throw new ArgumentException($"Invalid vision model. Must be one of: {VisionModels}");
            }
        }

        private void ValidateBase64Size(string base64String)
        {
            double sizeInMB = (base64String.Length * 3.0 / 4.0) / (1024 * 1024);
            if (sizeInMB > MAX_BASE64_SIZE_MB)
                throw new ArgumentException($"Base64 encoded image exceeds maximum size of {MAX_BASE64_SIZE_MB}MB");
        }

        private void ValidateImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("Image URL cannot be null or empty");

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid image URL format");
        }



        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }


    public class Tool
    {
        public string Type { get; set; } = "function";
        public Function Function { get; set; }
    }

    public class Function
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JObject Parameters { get; set; }
        public Func<string, Task<string>> ExecuteAsync { get; set; }
    }
}