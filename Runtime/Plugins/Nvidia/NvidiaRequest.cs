using System;

namespace XrAiAccelerator
{
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
        public object content;
    }

    [Serializable]
    public class NvidiaContentPart
    {
        public string type;
        public string text;
        public NvidiaImageUrl image_url;
    }

    [Serializable]
    public class NvidiaImageUrl
    {
        public string url;
    }
}
