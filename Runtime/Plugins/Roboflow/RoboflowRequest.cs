using System;

namespace XrAiAccelerator
{
    #region Roboflow Request JSON Classes
    [Serializable]
    public class RoboflowRequest  { }

    [Serializable]
    public class RoboflowInferenceRequest : RoboflowRequest
    {
        public string api_key;
        public string model_id;
        public RoboflowRequestImage image;
    }

    public class RoboflowServerlessRequest : RoboflowRequest
    {
        public string api_key;
        public RoboflowRequestInputs inputs;
    }

    [Serializable]
    public class RoboflowRequestInputs
    {
        public RoboflowRequestImage image;
    }

    [Serializable]
    public class RoboflowRequestImage
    {
        public string type;
        public string value;
    }
    #endregion
}
