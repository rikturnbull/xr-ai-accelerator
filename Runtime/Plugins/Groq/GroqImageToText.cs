using GroqApiLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace XrAiAccelerator
{
    [XrAiProvider("Groq")]
    [XrAiOption("apiKey", XrAiOptionScope.Global, isRequired: true, description: "Groq API key for authentication")]
    [XrAiOption("imageQuality", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "100", description: "JPEG quality (1-100, lower = smaller file)")]
    [XrAiOption("model", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "meta-llama/llama-4-scout-17b-16e-instruct", description: "The vision model to use: meta-llama/llama-4-scout-17b-16e-instruct or meta-llama/llama-4-maverick-17b-128e-instruct")]
    [XrAiOption("prompt", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "Describe the image", description: "The prompt to use for image analysis")]
    [XrAiOption("maxImageDimension", XrAiOptionScope.Workflow, isRequired: false, defaultValue: "512", description: "Maximum width or height for the image before encoding (e.g., 512, 1024, 1536)")]
    public class GroqImageToText : IXrAiImageToText
    {
        private GroqApiClient _groqApi;
        private XrAiOptionsHelper _optionsHelper;

        public Task Initialize(Dictionary<string, string> options = null)
        {
            _optionsHelper = new XrAiOptionsHelper(this, options);
            _groqApi = new GroqApiClient(_optionsHelper.GetOption("apiKey"));
            return Task.CompletedTask;
        }

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

                XrAiResult<string> result = await ExecuteGroqRequest(imageBytes, options);
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
                    XrAiResult.Failure<string>($"Exception in GroqImageToText: {ex.Message}")
                );
            }
        }

        private async Task<XrAiResult<string>> ExecuteGroqRequest(byte[] imageBytes, Dictionary<string, string> options = null)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new ArgumentException("Texture is empty or not valid.");
                }

                string model = _optionsHelper.GetOption("model", options);
                string prompt = _optionsHelper.GetOption("prompt", options);
                string base64Image = Convert.ToBase64String(imageBytes);

                JObject response = await _groqApi.CreateVisionCompletionWithTempBase64ImageAsync(base64Image, prompt, model);

                string result = response?["choices"]?[0]?["message"]?["content"]?.ToString();
                return XrAiResult.Success(result);
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<string>(ex.Message);
            }
        }
    }

}