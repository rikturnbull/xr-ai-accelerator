using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAiTextToImage
{
    public Task<XrAiResult<Texture2D>> Execute(Dictionary<string, string> options = null);
}