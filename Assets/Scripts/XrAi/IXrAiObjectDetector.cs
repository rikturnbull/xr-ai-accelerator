using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAiObjectDetector
{
    public Task<XrAiObjectDetectorResult> Execute(Texture2D texture, Dictionary<string, string> options = null);
}