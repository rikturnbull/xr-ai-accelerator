public struct XrAiBoundingBox
{
    public float CenterX;
    public float CenterY;
    public float Width;
    public float Height;
    public string ClassName;
}

public class XrAiObjectDetectorResult
{
    public XrAiBoundingBox[] BoundingBoxes;
}