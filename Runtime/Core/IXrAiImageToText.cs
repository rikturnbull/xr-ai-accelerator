using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    public interface IXrAiImageToText
    {
        public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null);
        public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
    }
}