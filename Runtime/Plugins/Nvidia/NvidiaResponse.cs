using System;

namespace XrAiAccelerator
{
    #region Nvidia Response JSON Classes
    [Serializable]
    public class NvidiaResponse
    {
        public NvidiaChoice[] choices;
    }

    [Serializable]
    public class NvidiaChoice
    {
        public NvidiaResponseMessage message;
    }

    [Serializable]
    public class NvidiaResponseMessage
    {
        public string content;
    }
    #endregion
}
