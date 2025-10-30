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

        public async Task Initialize(Dictionary<string, string> options = null)
        {
            _yoloExecutor = new YoloExecutor();
            await _yoloExecutor.LoadModel();
        }

        public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
        {
            await _yoloExecutor.Execute(texture, callback);
        }
    }
}