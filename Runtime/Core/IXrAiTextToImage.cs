using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    public interface IXrAiTextToImage
    {
        public Task Initialize(Dictionary<string, string> options = null);
        public Task Execute(string prompt, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback);
    }
}