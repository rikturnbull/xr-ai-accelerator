using System;

namespace XrAiAccelerator
{
    #region Nvidia Request JSON Classes
    [Serializable]
    public class NvidiaRequest
    {
        public string model;
        public NvidiaMessage[] messages;
        public int max_tokens;
        public float temperature;
        public float top_p;
        public float frequency_penalty;
        public float presence_penalty;
        public bool stream;
    }

    [Serializable]
    public class NvidiaMessage
    {
        public string role;
        public string content;
    }
    #endregion
}
