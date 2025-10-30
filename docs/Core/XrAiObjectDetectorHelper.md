# XrAiObjectDetectorHelper

The `XrAiObjectDetectorHelper` class provides utility methods for visualizing object detection results in Unity. It creates visual bounding boxes and labels to display detected objects on screen.

## Class Declaration

```csharp
public class XrAiObjectDetectorHelper
```

## Methods

### ClearBoxes

Removes all previously drawn bounding boxes from a parent transform.

```csharp
public static void ClearBoxes(Transform parent)
```

**Parameters:**
- `parent` (Transform): The parent transform containing the bounding box GameObjects to clear

### DrawBoxes

Draws bounding boxes for all detected objects.

```csharp
public static void DrawBoxes(Transform parent, XrAiBoundingBox[] boundingBoxes, Vector2 scale = default, Vector2 dimensions = default)
```

**Parameters:**
- `parent` (Transform): The parent transform to attach the bounding box GameObjects
- `boundingBoxes` (XrAiBoundingBox[]): Array of detected objects to visualize
- `scale` (Vector2, optional): Scale factor for converting normalized coordinates (default: 1,1)
- `dimensions` (Vector2, optional): Display dimensions for positioning (default: 1,1)

### DrawKeypoints

Draws keypoints for all detected objects that contain keypoint data.

```csharp
public static void DrawKeypoints(Transform parent, XrAiBoundingBox[] boundingBoxes, Vector2 imageDimensions = default, Vector2 canvasDimensions = default)
```

**Parameters:**
- `parent` (Transform): The parent transform to attach the keypoint GameObjects
- `boundingBoxes` (XrAiBoundingBox[]): Array of detected objects that may contain keypoints
- `imageDimensions` (Vector2, optional): Original image dimensions for coordinate conversion (default: 1,1)
- `canvasDimensions` (Vector2, optional): Canvas dimensions for positioning (default: 1,1)
