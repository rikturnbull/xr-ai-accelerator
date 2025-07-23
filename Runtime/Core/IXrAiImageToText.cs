using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IXrAiImageToText
{
    public Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, Dictionary<string, string> options = null);
}