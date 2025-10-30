using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XrAiAccelerator
{
    public interface IXrAiTextToText
    {
        public Task Initialize(Dictionary<string, string> options = null);
        public Task Execute(string inputText, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
    }
}