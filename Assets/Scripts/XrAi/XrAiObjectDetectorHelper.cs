using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XrAiObjectDetectorHelper
{
    public static void DrawBoxes(Transform location, XrAiBoundingBox[] boundingBoxes)
    {
        foreach (XrAiBoundingBox boundingBox in boundingBoxes)
        {
            DrawBox(location, boundingBox);
        }
    }

    private static void DrawBox(Transform location, XrAiBoundingBox box)
    {
        GameObject panel = CreateNewBox(location, Color.red);

        // Set box position
        panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY, 0.0f);

        // Set box size
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(box.Width, box.Height);

        // Set label text
        Text label = panel.GetComponentInChildren<Text>();
        label.text = box.ClassName;
    }

    private static GameObject CreateNewBox(Transform location, Color color)
    {
        // Create the box and set image
        GameObject panel = new("ObjectBox");
        panel.AddComponent<CanvasRenderer>();

        Image image = panel.AddComponent<Image>();
        image.color = color;
        // image.sprite = _boxTexture;
        image.type = Image.Type.Sliced;
        image.fillCenter = false;
        panel.transform.SetParent(location, false);

        // Create the label
        GameObject textGameObject = new("ObjectLabel");
        textGameObject.AddComponent<CanvasRenderer>();
        textGameObject.transform.SetParent(panel.transform, false);

        Text text = textGameObject.AddComponent<Text>();
        // text.font = _font;
        // text.color = _fontColor;
        // text.fontSize = _fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        RectTransform rectTransform = textGameObject.GetComponent<RectTransform>();
        rectTransform.offsetMin = new Vector2(20, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
        rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 30);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);

        return panel;
    }

}
