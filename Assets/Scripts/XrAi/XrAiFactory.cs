using System.Collections.Generic;

public class XrAiFactory
{
    public static IXrAiModelImageTo3d LoadModelImageTo3d(string name, Dictionary<string, string> options = null)
    {
        if (name == "TripoAi")
        {
            return new TripoAi.TripXrAiModelImageTo3d(options);
        }
        else if (name == "StableAi")
        {
            return new StableAi.StableAiImageTo3d(options);
        }
        return null;
    }

    public static IXrAiObjectDetector LoadObjectDetector(string name, Dictionary<string, string> options = null)
    {
        if (name == "Yolo11")
        {
            return new Yolo.Yolo11ObjectDetector(options);
        }
        return null;
    }
    
    public static IXrAiImageToText LoadImageToText(string name, Dictionary<string, string> options = null)
    {
        if (name == "Groq")
        {
            return new Groq.GroqImageToText(options);
        }
        return null;
    }
}