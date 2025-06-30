using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAiImageToText
{
    public Task<XrAiTaskResult> Execute(Texture2D texture, Dictionary<string, string> options = null);
}