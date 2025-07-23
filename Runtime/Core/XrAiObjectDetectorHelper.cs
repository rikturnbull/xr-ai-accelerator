using UnityEngine;
using TMPro;

namespace XrAiAccelerator
{
    public class XrAiObjectDetectorHelper
    {
        public static void ClearBoxes(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.name == "ObjectBox")
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        public static void DrawBoxes(Transform parent, XrAiBoundingBox[] boundingBoxes, Vector2 scale = default, Vector2 dimensions = default)
        {
            scale = scale == default ? new Vector2(1.0f, 1.0f) : scale;
            dimensions = dimensions == default ? new Vector2(1.0f, 1.0f) : dimensions;
            for (int i = 0; i < boundingBoxes.Length; i++)
            {
                DrawBox(parent, boundingBoxes[i], scale, dimensions, i);
            }
        }

        private static void DrawBox(Transform parent, XrAiBoundingBox box, Vector2 scale, Vector2 dimensions, int colorIndex)
        {
            Color boxColor = GetBoxColor(colorIndex);
            GameObject lineObject = CreateNewBox(parent, boxColor);

            float scaledWidth = box.Width * scale.x;
            float scaledHeight = box.Height * scale.y;
            float scaledCenterX = box.CenterX * scale.x;
            float scaledCenterY = dimensions.y - (box.CenterY * scale.y);

            float halfWidth = dimensions.x / 2;
            float halfHeight = dimensions.y / 2;

            // Calculate box corners
            float left = scaledCenterX - scaledWidth / 2 - halfWidth;
            float right = scaledCenterX + scaledWidth / 2 - halfWidth;
            float top = scaledCenterY + scaledHeight / 2 - halfHeight;
            float bottom = scaledCenterY - scaledHeight / 2 - halfHeight;

            // Set line positions to form a rectangle
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 5; // 5 points to close the rectangle
            lineRenderer.SetPosition(0, new Vector3(left, bottom, -0.01f)); // Bottom-left
            lineRenderer.SetPosition(1, new Vector3(right, bottom, -0.01f)); // Bottom-right
            lineRenderer.SetPosition(2, new Vector3(right, top, -0.01f)); // Top-right
            lineRenderer.SetPosition(3, new Vector3(left, top, -0.01f)); // Top-left
            lineRenderer.SetPosition(4, new Vector3(left, bottom, -0.01f)); // Close the rectangle

            // Set label text
            TextMeshPro label = lineObject.GetComponentInChildren<TextMeshPro>();
            label.text = box.ClassName;
            
            // Position label above the box center
            label.transform.localPosition = new Vector3(scaledCenterX - halfWidth, top + 0.005f, -0.01f);
        }

        private static Color GetBoxColor(int index)
        {
            Color[] colors = new Color[]
            {
                Color.red,
                Color.green,
                Color.blue,
                Color.yellow,
                Color.cyan,
                Color.magenta,
                new Color(1f, 0.5f, 0f), // Orange
                new Color(0.5f, 0f, 1f), // Purple
                new Color(0f, 1f, 0.5f), // Spring green
                new Color(1f, 0f, 0.5f)  // Hot pink
            };
            
            return colors[index % colors.Length];
        }

        private static GameObject CreateNewBox(Transform parent, Color color)
        {
            // Create the box with LineRenderer
            GameObject lineObject = new("ObjectBox");
            lineObject.transform.SetParent(parent, false);

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.001f;
            lineRenderer.endWidth = 0.001f;
            lineRenderer.useWorldSpace = false;

            // Create the label
            GameObject textGameObject = new("ObjectLabel");
            textGameObject.transform.SetParent(lineObject.transform, false);

            TextMeshPro textMesh = textGameObject.AddComponent<TextMeshPro>();
            textMesh.color = color;
            textMesh.fontSize = 0.05f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.characterSpacing = 0;

            return lineObject;
        }
    }
}
