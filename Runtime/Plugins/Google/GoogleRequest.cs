using System;

namespace XrAiAccelerator
{
    [Serializable]
    public class GeminiRequest
    {
        public GeminiRequestContent[] contents;
        public GeminiGenerationConfig generationConfig;
    }

    [Serializable]
    public class GeminiRequestContent
    {
        public GeminiRequestPart[] parts;
    }

    [Serializable]
    public class GeminiRequestPart
    {
        public GeminiInlineData inline_data;
        public string text;
    }

    [Serializable]
    public class GeminiInlineData
    {
        public string mime_type;
        public string data;
    }

    [Serializable]
    public class GeminiGenerationConfig
    {
        public string response_mime_type;
    }
}
