using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace XrAiAccelerator
{
    public interface IXrAiObjectDetector
    {
        public Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null);
    }
}