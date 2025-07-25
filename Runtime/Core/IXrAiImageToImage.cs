using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace XrAiAccelerator
{
    public interface IXrAiImageToImage
    {
        public Task<XrAiResult<Texture2D>> Execute(Texture2D texture, Dictionary<string, string> options = null);
    }
}