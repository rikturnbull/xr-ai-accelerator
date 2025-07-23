using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace XrAiAccelerator
{

    #region Main Class
    public class GoogleObjectDetector : IXrAiObjectDetector
    {
        private static readonly string _PROMPT = "Detect the all of the prominent items in the image. The box_2d should be [ymin, xmin, ymax, xmax] normalized to 0-1000.";
        private HttpClient _httpClient = new();
        private Dictionary<string, string> _globalOptions = new();


        public GoogleObjectDetector(Dictionary<string, string> options)
        {
            _globalOptions = options;
        }

        public async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
        {
            try {
                string apiKey = GetOption("apiKey", options);
                string url = GetOption("url", options);

                return await Execute(texture, apiKey, url);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, string apiKey, string url)
        {
            try
            {
                // Encode the texture to JPEG format and convert to Base64
                byte[] imageBytes = texture.EncodeToJPG();
                string base64Image = Convert.ToBase64String(imageBytes);

                // Prepare the JSON request body
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
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                },
                                new
                                {
                                    text = _PROMPT
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        response_mime_type = "application/json"
                    }
                };
                string jsonData = JsonConvert.SerializeObject(requestData);
                StringContent content = new(jsonData, Encoding.UTF8, "application/json");

                // Create the HTTP request
                using HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = content;

                // Send the request and get the response
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<XrAiBoundingBox[]>($"Error in Google Gemini Vision request: {response.StatusCode} - {errorContent}");
                }

                // Read the response as a string
                string responseText = await response.Content.ReadAsStringAsync();

                // Parse the response JSON
                GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);

                // Check if we have candidates and parts in the response
                string resultContent = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;
                if (string.IsNullOrEmpty(resultContent))
                {
                    return XrAiResult.Failure<XrAiBoundingBox[]>("No content received from Google Gemini API");
                }

                // Parse the object detection results
                List<XrAiBoundingBox> boundingBoxes = ParseGeminiObjectDetectionResponse(resultContent, texture.width, texture.height);

                return XrAiResult.Success(boundingBoxes.ToArray());
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private List<XrAiBoundingBox> ParseGeminiObjectDetectionResponse(string responseText, float imageWidth, float imageHeight)
        {
            List<XrAiBoundingBox> boundingBoxes = new List<XrAiBoundingBox>();

            try
            {
                // Try to deserialize as direct array first
                GoogleDetectedObject[] detectedObjects = null;
                
                try
                {
                    detectedObjects = JsonConvert.DeserializeObject<GoogleDetectedObject[]>(responseText);
                }
                catch
                {
                    // If that fails, try the wrapper format
                    GoogleObjectDetectionResponse detectionResponse = JsonConvert.DeserializeObject<GoogleObjectDetectionResponse>(responseText);
                    detectedObjects = detectionResponse?.objects;
                }
                
                if (detectedObjects != null)
                {
                    foreach (var obj in detectedObjects)
                    {
                        // Handle both "box" and "bbox" formats
                        float[] coordinates = obj.box_2d;
                        
                        if (coordinates != null && coordinates.Length == 4)
                        {
                            // Convert from [ymin, xmin, ymax, xmax] (0-1000) to center coordinates
                            float ymin = coordinates[0] / 1000f * imageHeight;
                            float xmin = coordinates[1] / 1000f * imageWidth;
                            float ymax = coordinates[2] / 1000f * imageHeight;
                            float xmax = coordinates[3] / 1000f * imageWidth;

                            float centerX = (xmin + xmax) / 2f;
                            float centerY = (ymin + ymax) / 2f;
                            float width = xmax - xmin;
                            float height = ymax - ymin;

                            boundingBoxes.Add(new XrAiBoundingBox
                            {
                                CenterX = centerX,
                                CenterY = centerY,
                                Width = width,
                                Height = height,
                                ClassName = obj.label ?? "unknown"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing Gemini object detection response: {ex.Message}");
            }

            return boundingBoxes;
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

        [Serializable]
        private class GoogleObjectDetectionResponse
        {
            public GoogleDetectedObject[] objects;
        }

        [Serializable]
        private class GoogleDetectedObject
        {
            public string label;
            public float[] box_2d;
        }
        #endregion
    }
    #endregion

}
