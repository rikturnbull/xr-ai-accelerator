using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Text;

namespace XrAiAccelerator
{
    #region Main Class
    public class NvidiaObjectDetector : IXrAiObjectDetector
    {
        private static readonly string _VLM_URL = "https://ai.api.nvidia.com/v1/vlm/microsoft/florence-2";
        private static readonly string _ASSET_URL = "https://api.nvcf.nvidia.com/v2/nvcf/assets";
        private static readonly string _OD_TASK = "<OD>";

        private HttpClient _client = new HttpClient();
        Dictionary<string, string> _globalOptions = new();

        public NvidiaObjectDetector(Dictionary<string, string> options = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
        }

        public async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
        {
            try
            {
                string apiKey = GetOption("apiKey", options);
                return await Execute(texture, apiKey);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, string apiKey)
        {
            try
            {
                // Upload asset to NVIDIA API
                string assetId = await UploadAsset(texture, apiKey);
                if (string.IsNullOrEmpty(assetId))
                {
                    return XrAiResult.Failure<XrAiBoundingBox[]>("Failed to upload asset to NVIDIA API");
                }

                // Generate content with OD task
                string content = GenerateContent(assetId);

                // Prepare the request
                var requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = content
                        }
                    }
                };

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Set headers
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                _client.DefaultRequestHeaders.Add("NVCF-INPUT-ASSET-REFERENCES", assetId);
                _client.DefaultRequestHeaders.Add("NVCF-FUNCTION-ASSET-IDS", assetId);
                _client.DefaultRequestHeaders.Add("Accept", "application/json");

                // Send request
                HttpResponseMessage response = await _client.PostAsync(_VLM_URL, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<XrAiBoundingBox[]>($"Request failed with status code {response.StatusCode}: {errorContent}");
                }

                // Parse response
                string result = await response.Content.ReadAsStringAsync();
                NvidiaObjectDetectorResponse nvidiaResult = JsonConvert.DeserializeObject<NvidiaObjectDetectorResponse>(result);
                
                return ConvertToXrAiObjectDetectorResult(nvidiaResult, texture.width, texture.height);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private async Task<string> UploadAsset(Texture2D texture, string apiKey)
        {
            try
            {
                // Step 1: Authorize asset upload
                var authorizeBody = new
                {
                    contentType = "image/jpeg",
                    description = "Object Detection Image"
                };

                var authorizeContent = new StringContent(
                    JsonConvert.SerializeObject(authorizeBody),
                    Encoding.UTF8,
                    "application/json"
                );

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                _client.DefaultRequestHeaders.Add("Accept", "application/json");

                HttpResponseMessage authorizeResponse = await _client.PostAsync(_ASSET_URL, authorizeContent);
                
                if (!authorizeResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                string authorizeResult = await authorizeResponse.Content.ReadAsStringAsync();
                var authorizeData = JsonConvert.DeserializeObject<AssetAuthorizeResponse>(authorizeResult);

                // Step 2: Upload the actual image
                byte[] imageBytes = texture.EncodeToJPG();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                imageContent.Headers.Add("x-amz-meta-nvcf-asset-description", "Object Detection Image");

                HttpResponseMessage uploadResponse = await _client.PutAsync(authorizeData.uploadUrl, imageContent);
                
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                return authorizeData.assetId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GenerateContent(string assetId)
        {
            return $"{_OD_TASK}<img src=\"data:image/jpeg;asset_id,{assetId}\" />";
        }

        private XrAiResult<XrAiBoundingBox[]> ConvertToXrAiObjectDetectorResult(NvidiaObjectDetectorResponse nvidiaResult, int imageWidth, int imageHeight)
        {
            List<XrAiBoundingBox> boundingBoxes = new List<XrAiBoundingBox>();
            
            if (nvidiaResult?.choices != null && nvidiaResult.choices.Length > 0)
            {
                var message = nvidiaResult.choices[0].message;
                if (message?.content != null)
                {
                    // Parse the OD response format from Florence-2
                    // The response typically contains bounding box coordinates and labels
                    // This is a simplified parser - you may need to adjust based on actual response format
                    try
                    {
                        var parsedData = JsonConvert.DeserializeObject<Florence2ODResponse>(message.content);
                        if (parsedData?.bboxes != null && parsedData.labels != null)
                        {
                            for (int i = 0; i < Math.Min(parsedData.bboxes.Length, parsedData.labels.Length); i++)
                            {
                                var bbox = parsedData.bboxes[i];
                                if (bbox.Length >= 4)
                                {
                                    // Convert from absolute coordinates to center + width/height
                                    float x1 = bbox[0];
                                    float y1 = bbox[1];
                                    float x2 = bbox[2];
                                    float y2 = bbox[3];
                                    
                                    boundingBoxes.Add(new XrAiBoundingBox
                                    {
                                        CenterX = (x1 + x2) / 2,
                                        CenterY = (y1 + y2) / 2,
                                        Width = x2 - x1,
                                        Height = y2 - y1,
                                        ClassName = parsedData.labels[i]
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // If parsing fails, return empty result
                        Debug.LogWarning("Failed to parse NVIDIA Florence-2 object detection response");
                    }
                }
            }
            
            return XrAiResult.Success(boundingBoxes.ToArray());
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
        private class AssetAuthorizeResponse
        {
            public string assetId;
            public string uploadUrl;
        }

        [Serializable]
        private class NvidiaObjectDetectorResponse
        {
            public Choice[] choices;
        }

        [Serializable]
        private class Choice
        {
            public Message message;
        }

        [Serializable]
        private class Message
        {
            public string content;
        }

        [Serializable]
        private class Florence2ODResponse
        {
            public float[][] bboxes;
            public string[] labels;
        }
        #endregion
    }
    #endregion
}
