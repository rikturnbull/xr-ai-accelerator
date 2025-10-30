using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    public interface IXrAiSpeechToText
    {
        public Task Initialize(Dictionary<string, string> options = null);
        public Task Execute(byte[] audioData, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
    }
}