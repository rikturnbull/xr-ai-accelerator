using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    public interface IXrAiImageTo3d
    {
       public Task Initialize(Dictionary<string, string> options = null, XrAiAssets assets = null);
       public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback);
    }
}