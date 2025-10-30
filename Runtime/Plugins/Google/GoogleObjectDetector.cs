using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("Google")]
    [XrAiOption("apiKey", XrAiOptionScope.Workflow, isRequired: true, description: "Google API key for authentication")]
    [XrAiOption("imageQuality", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "100", description: "JPEG quality (1-100, lower = smaller file)")]
    [XrAiOption("maxImageDimension", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "512", description: "Maximum width or height for the image before encoding (e.g., 512, 1024, 1536)")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent", description: "The URL for the Google API endpoint")]
    public class GoogleObjectDetector : GoogleAiBase, IXrAiObjectDetector
    {
        private static readonly string _PROMPT = "Detect the all of the prominent items in the image. The box_2d should be [ymin, xmin, ymax, xmax] normalized to 0-1000.";

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
        {
            try
            {
                int maxDimension = _optionsHelper.GetIntOption("maxImageDimension", options);
                int imageQuality = _optionsHelper.GetIntOption("imageQuality", options);
                Texture2D scaledTexture = XrAiImageHelper.ScaleTexture(texture, maxDimension);

                await Task.Yield();
                byte[] imageBytes = scaledTexture.EncodeToJPG(imageQuality);

                UnityEngine.Object.Destroy(scaledTexture);

                int imageWidth = texture.width;
                int imageHeight = texture.height;

                XrAiResult<string> result = await ExecuteGeminiRequest(imageBytes, _PROMPT, options, new GeminiGenerationConfig
                {
                    response_mime_type = "application/json"
                }
                );

                if (result.IsSuccess)
                {
                    callback?.Invoke(
                        XrAiResult.Success<XrAiBoundingBox[]>(
                            ParseGeminiObjectDetectionResponse(
                                result.Data,
                                imageWidth,
                                imageHeight
                            ).ToArray()
                        )
                    );
                }
                else
                {
                    callback?.Invoke(
                        XrAiResult.Failure<XrAiBoundingBox[]>(result.ErrorMessage)
                    );
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<XrAiBoundingBox[]>($"Exception in GoogleObjectDetector: {ex.Message}")
                );
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
    }
}
