using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAImageTo3d
{
    public Task<XrAiResult<byte[]>> Execute(Texture2D texture, Dictionary<string, string> options = null);
}