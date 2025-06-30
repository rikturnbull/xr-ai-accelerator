using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAiModelImageTo3d
{
    public Task<XrAiTaskResult> Execute(Texture2D texture, Dictionary<string, string> options = null);
}