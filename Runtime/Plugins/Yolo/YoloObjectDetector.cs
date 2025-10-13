using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    [XrAiProvider("Yolo")]
    public class YoloObjectDetector : IXrAiObjectDetector
    {
        private YoloExecutor _yoloExecutor;

        public async Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            Debug.Log("Yolo: Initializing...");
            YoloAssets yoloAssets = assets as YoloAssets;
            if (yoloAssets == null)
            {
                Debug.LogError("YoloObjectDetector requires YoloAssets for initialization.");
                return;
            }
            Debug.Log("Yolo: Loading model...");
            _yoloExecutor = new YoloExecutor();
            Debug.Log("Yolo: Model loading...");
            await _yoloExecutor.LoadModel(yoloAssets);
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
        {
            await _yoloExecutor.Execute(texture, callback);
        }
    }
}