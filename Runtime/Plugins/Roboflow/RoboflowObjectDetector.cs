using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    #region Main Class
    [XrAiProvider("Roboflow")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "Roboflow API key for authentication")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "https://serverless.roboflow.com/infer/workflows/xr-shu81/yolov11-object-detection", description: "The URL for the Roboflow API endpoint")]  
    [XrAiOption("modelId", XrAiOptionScope.Workflow, isRequired: false, description: "The model id to use for object detection")]
    public class RoboflowObjectDetector : IXrAiObjectDetector
    {
        private HttpClient _httpClient = new HttpClient();
        private Dictionary<string, string> _globalOptions = new Dictionary<string, string>();

        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            _globalOptions = options ?? new Dictionary<string, string>();
            return Task.CompletedTask;
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
        {
            byte[] imageBytes = XrAiImageHelper.EncodeTexture(texture, "image/jpeg");
            int imageWidth = texture.width;
            int imageHeight = texture.height;

            XrAiResult<XrAiBoundingBox[]> result = await ExecuteRoboflowRequest(imageBytes, options);

            callback?.Invoke(
                result.IsSuccess
                    ? XrAiResult.Success(result.Data)
                    : XrAiResult.Failure<XrAiBoundingBox[]>(result.ErrorMessage)
            );
        }

        private async Task<XrAiResult<XrAiBoundingBox[]>> ExecuteRoboflowRequest(byte[] imageBytes, Dictionary<string, string> options = null)
        {
            try
            {
                string apiKey = GetOption("apiKey", options);
                string url = GetOption("url", options);
                string modelId = GetOption("modelId", options, "");

                string base64Image = Convert.ToBase64String(imageBytes);

                RoboflowRequest requestData = CreateRoboflowRequest(url, base64Image, apiKey, modelId);
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<XrAiBoundingBox[]>($"Request failed with status code {response.StatusCode}: {errorContent}");
                }

                string result = await response.Content.ReadAsStringAsync();

                return ParseRoboflowObjectDetectionResponse(url, result);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private RoboflowRequest CreateRoboflowRequest(string url, string base64Image, string apiKey, string modelId)
        {
            if (url.Contains("serverless.roboflow.com"))
            {
                return CreateRoboflowServerlessRequest(base64Image, apiKey);
            }
            else
            {
                return CreateRoboflowInferenceRequest(base64Image, apiKey, modelId);
            }
        }

        private RoboflowInferenceRequest CreateRoboflowInferenceRequest(string base64Image, string apiKey, string modelId)
        {
            return new RoboflowInferenceRequest
            {
                api_key = apiKey,
                model_id = modelId,
                image = new RoboflowRequestImage
                {
                    type = "base64",
                    value = base64Image
                }
            };
        }

        private RoboflowServerlessRequest CreateRoboflowServerlessRequest(string base64Image, string apiKey)
        {
            return new RoboflowServerlessRequest
            {
                api_key = apiKey,
                inputs = new RoboflowRequestInputs
                {
                    image = new RoboflowRequestImage
                    {
                        type = "base64",
                        value = base64Image
                    }
                }
            };
        }

        private XrAiResult<XrAiBoundingBox[]> ParseRoboflowObjectDetectionResponse(string url, string responseText)
        {
            List<XrAiBoundingBox> boundingBoxes = new List<XrAiBoundingBox>();

            Debug.Log($"Roboflow response: {responseText}");
            try
            {
                RoboflowPredictions roboflowResult;
                if (url.Contains("serverless.roboflow.com"))
                {
                    roboflowResult = JsonConvert.DeserializeObject<RoboflowServerlessResponse>(responseText).outputs[0].model_predictions;
                }
                else
                {
                    roboflowResult = JsonConvert.DeserializeObject<RoboflowPredictions>(responseText);
                }

                if (roboflowResult?.predictions != null && roboflowResult.predictions.Count > 0)
                {
                    foreach (RoboflowDetection prediction in roboflowResult.predictions)
                    {
                        List<XrAiKeypoint> keypointsList = new();

                        // Check if keypoints exist before iterating
                        if (prediction.keypoints != null)
                        {
                            foreach (RoboflowKeypoint keypoints in prediction.keypoints)
                            {
                                XrAiKeypoint keypoint = new()
                                {
                                    x = keypoints.x,
                                    y = keypoints.y,
                                    confidence = keypoints.confidence,
                                    class_id = keypoints.class_id,
                                    @class = keypoints.@class
                                };
                                keypointsList.Add(keypoint);
                            }
                        }
                        XrAiBoundingBox boundingBox = new XrAiBoundingBox
                        {
                            CenterX = prediction.x,
                            CenterY = prediction.y,
                            Width = prediction.width,
                            Height = prediction.height,
                            ClassName = prediction.@class ?? "unknown",
                            Keypoints = keypointsList.ToArray()
                        };
                        boundingBoxes.Add(boundingBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing Roboflow object detection response: {ex.Message}");
                return XrAiResult.Failure<XrAiBoundingBox[]>($"Failed to parse response: {ex.Message}");
            }

            return XrAiResult.Success(boundingBoxes.ToArray());
        }

        private string GetOption(string key, Dictionary<string, string> options = null, String defaultValue = null)
        {
            if (options != null && options.TryGetValue(key, out string value))
            {
                return value;
            }
            else if (_globalOptions.TryGetValue(key, out value))
            {
                return value;
            }
            if(defaultValue != null)
            {
                return defaultValue;
            }
            throw new KeyNotFoundException($"Option '{key}' not found.");
        }
    }
    #endregion
}