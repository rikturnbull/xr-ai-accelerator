using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiYolo
{
    public class YoloObjectDetector : IXrAiObjectDetector
    {
        private Dictionary<string, string> _globalOptions = new();
        private YoloExecutor _yoloExecutor;

        public YoloObjectDetector(XrAiAssets assets, Dictionary<string, string> options = null)
        {
            _globalOptions = options;

            YoloAssets yoloAssets = assets as YoloAssets;
            GameObject yoloExecutorObject = new("YoloExecutor");
            _yoloExecutor = yoloExecutorObject.AddComponent<YoloExecutor>();
            _yoloExecutor.LoadModel(yoloAssets);
        }

        public async Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
        {
            try
            {
                // Wait until _yoloExecutor is ready
                while (!_yoloExecutor.IsModelLoaded && _yoloExecutor.IsRunning())
                {
                    await Task.Yield();
                }

                // Run inference on the input texture
                _yoloExecutor.RunInference(texture);

                // Wait for the inference to complete
                while (_yoloExecutor.IsRunning())
                {
                    await Task.Yield();
                }

                // Get the results from the inference
                var result = _yoloExecutor.GetResult();

                return result;
            }
            catch (Exception ex)
            {
                return XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message);
            }
        }
    }
}