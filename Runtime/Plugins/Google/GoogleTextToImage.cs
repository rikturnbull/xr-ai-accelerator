using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("Google")]
    [XrAiOption("apiKey", XrAiOptionScope.Workflow, isRequired: true, description: "Google API key for authentication")]
    [XrAiOption("url", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent", description: "The URL for the Google API endpoint")]
    public class GoogleTextToImage : GoogleAiBase, IXrAiTextToImage
    {
        public async Task Execute(string prompt, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
        {
            try
            {
                XrAiResult<Texture2D> result = await ExecuteTextToImageRequest(prompt, options);
                if (result.IsSuccess)
                {
                    callback?.Invoke(result);
                }
                else
                {
                    callback?.Invoke(
                        XrAiResult.Failure<Texture2D>(result.ErrorMessage)
                    );
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(
                    XrAiResult.Failure<Texture2D>($"Exception in GoogleImageToText: {ex.Message}")
                );
            }
        }
    }
}
