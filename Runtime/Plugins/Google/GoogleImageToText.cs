using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace XrAiAccelerator
{
    [XrAiProvider("Google")]
    [XrAiOption("apiKey", XrAiOptionScope.Workflow, isRequired: true, description: "Google API key for authentication")]
    [XrAiOption("imageQuality", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "100", description: "JPEG quality (1-100, lower = smaller file)")]
    [XrAiOption("maxImageDimension", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "512", description: "Maximum width or height for the image before encoding (e.g., 512, 1024, 1536)")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "Describe the image", description: "The prompt to use for image-to-text conversion")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent", description: "The URL for the Google API endpoint")]
    public class GoogleImageToText : GoogleAiBase, IXrAiImageToText
    {
        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            try
            {
                await Task.Yield();

                int maxDimension = _optionsHelper.GetIntOption("maxImageDimension", options);
                int imageQuality = _optionsHelper.GetIntOption("imageQuality", options);
                Texture2D scaledTexture = XrAiImageHelper.ScaleTexture(texture, maxDimension);

                await Task.Yield();
                byte[] imageBytes = scaledTexture.EncodeToJPG(imageQuality);

                UnityEngine.Object.Destroy(scaledTexture);

                await Task.Yield();

                string prompt = _optionsHelper.GetOption("prompt", options);

                XrAiResult<string> result = await ExecuteGeminiRequest(imageBytes, prompt, options);
                if (result.IsSuccess)
                {
                    callback?.Invoke(result);
                }
                else
                {
                    callback?.Invoke(
                        XrAiResult.Failure<string>(result.ErrorMessage)
                    );
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<string>($"Exception in GoogleImageToText: {ex.Message}")
                );
            }
        }
    }
}
