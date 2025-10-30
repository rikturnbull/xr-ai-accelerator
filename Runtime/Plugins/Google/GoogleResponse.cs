using System;

namespace XrAiAccelerator
{
    [Serializable]
    public class GeminiResponse
    {
        public GeminiCandidate[] candidates;
    }

    [Serializable]
    public class GeminiCandidate
    {
        public GeminiContent content;
    }

    [Serializable]
    public class GeminiContent
    {
        public GeminiPart[] parts;
    }

    [Serializable]
    public class GeminiPart
    {
        public string text;
    }

    [Serializable]
    public class GoogleObjectDetectionResponse
    {
        public GoogleDetectedObject[] objects;
    }

    [Serializable]
    public class GoogleDetectedObject
    {
        public string label;
        public float[] box_2d;
    }
}
