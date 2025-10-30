using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    public interface IXrAiObjectDetector
    {
        public Task Initialize(Dictionary<string, string> options = null);
        public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback);
    }
}