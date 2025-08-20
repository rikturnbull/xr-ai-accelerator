using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{

    #region Main Class
    [XrAiProvider("Google")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "Google API key for authentication")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent", description: "The URL for the Google API endpoint")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: true, defaultValue: "Describe the image", description: "The prompt to use for image-to-text conversion")]
    public class GoogleImageToText : GoogleAiBase, IXrAiImageToText
    {
        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
        {
            await Task.Yield();
            byte[] imageBytes = texture.EncodeToPNG();
            await Task.Yield();

            string prompt = GetOption("prompt", options);

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
    }
    #endregion

}
