using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace XrAiAccelerator
{
    #region Main Class
    public class RoboflowObjectDetector : IXrAiObjectDetector
    {
        private HttpClient _client = new HttpClient();
        Dictionary<string, string> _globalOptions = new();

        public RoboflowObjectDetector(Dictionary<string, string> options = null)
        {
            _globalOptions = options;
        }

        public async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
        {
            try
            {
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
                byte[] imageBytes = texture.EncodeToJPG();
                string base64Image = Convert.ToBase64String(imageBytes);

                var requestBody = new
                {
                    api_key = apiKey,
                    inputs = new
                    {
                        image = new
                        {
                            type = "base64",
                            value = base64Image
                        }
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return XrAiResult.Failure<XrAiBoundingBox[]>($"Request failed with status code {response.StatusCode}: {errorContent}");
                }

                string result = await response.Content.ReadAsStringAsync();
                RoboflowObjectDetectorResults roboflowResult = JsonConvert.DeserializeObject<RoboflowObjectDetectorResults>(result);
                return ConvertToXrAiObjectDetectorResult(roboflowResult);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }

        private XrAiResult<XrAiBoundingBox[]> ConvertToXrAiObjectDetectorResult(RoboflowObjectDetectorResults roboflowResult)
        {
            List<XrAiBoundingBox> boundingBoxes = new List<XrAiBoundingBox>();
            foreach (var prediction in roboflowResult.outputs[0].model_predictions.predictions)
            {
                boundingBoxes.Add(new XrAiBoundingBox
                {
                    CenterX = prediction.x,
                    CenterY = prediction.y,
                    Width = prediction.width,
                    Height = prediction.height,
                    ClassName = prediction.@class
                });
            }
            return XrAiResult.Success(boundingBoxes.ToArray());;
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
        private class RoboflowObjectDetectorResults
        {
            public List<RoboflowObjectDetectorOutput> outputs;
        }

        [Serializable]
        private class RoboflowObjectDetectorOutput
        {
            public RoboflowObjectDetectorModelPredictions model_predictions;
        }

        [Serializable]
        private class RoboflowObjectDetectorModelPredictions
        {
            public RoboflowObjectDetectorImage image;
            public List<RoboflowObjectDetectorPredictions> predictions;
        }

        [Serializable]
        private class RoboflowObjectDetectorImage
        {
            public float width;
            public float height;
        }

        [Serializable]
        private class RoboflowObjectDetectorPredictions
        {
            public float x;
            public float y;
            public float width;
            public float height;
            public string @class;
            public float confidence;
            public int class_id;
            public string detection_id;
            public string parent_id;
        }
        #endregion
    }
    #endregion
}