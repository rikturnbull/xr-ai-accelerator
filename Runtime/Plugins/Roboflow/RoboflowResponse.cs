using System;
using System.Collections.Generic;

namespace XrAiAccelerator
{
    #region Roboflow Response JSON Classes

    [Serializable]
    public class RoboflowInferenceResponse
    {
        public RoboflowImageInfo image;
        public List<RoboflowDetection> predictions;
    }

    [Serializable]
    public class RoboflowServerlessResponse
    {
        public List<RoboflowOutput> outputs;
    }

    [Serializable]
    public class RoboflowOutput
    {
        public RoboflowPredictions model_predictions;
    }

    [Serializable]
    public class RoboflowPredictions
    {
        public RoboflowImageInfo image;
        public List<RoboflowDetection> predictions;
    }

    [Serializable]
    public class RoboflowImageInfo
    {
        public float width;
        public float height;
    }

    [Serializable]
    public class RoboflowDetection
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public string @class;
        public float confidence;
        public int class_id;
        public string detection_id;
        public string parent_id;
        public List<RoboflowKeypoint> keypoints;
    }

    [Serializable]
    public class RoboflowKeypoint
    {
        public float x;
        public float y;
        public float confidence;
        public int class_id;
        public string @class;
    }
    #endregion
}
