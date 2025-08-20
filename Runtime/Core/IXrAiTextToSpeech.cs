using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XrAiAccelerator
{
    public interface IXrAiTextToSpeech
    {
        public Task Initialize(Dictionary<string, string> globalOptions);
        public Task Execute(string text, Dictionary<string, string> workflowOptions, Action<XrAiResult<AudioClip>> callback);
    }
}