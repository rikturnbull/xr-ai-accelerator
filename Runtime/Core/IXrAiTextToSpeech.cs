using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public interface IXrAiTextToSpeech
{
    public Task<XrAiResult<AudioClip>> Execute(string text, Dictionary<string, string> options = null);
}