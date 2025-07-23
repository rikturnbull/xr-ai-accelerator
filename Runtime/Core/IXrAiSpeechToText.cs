using System.Threading.Tasks;
using System.Collections.Generic;

namespace XrAiAccelerator
{
    public interface IXrAiSpeechToText
    {
        public Task<XrAiResult<string>> Execute(byte[] audioData, Dictionary<string, string> options = null);
    }
}