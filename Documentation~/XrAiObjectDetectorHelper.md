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

## Usage Example

```csharp
public class ObjectDetectionVisualizer : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private RawImage imageDisplay;
    
    public async void DetectAndVisualize(Texture2D inputImage)
    {
        // Clear previous detections
        XrAiObjectDetectorHelper.ClearBoxes(canvasTransform);
        
        // Perform object detection
        var result = await objectDetector.Execute(inputImage);
        
        if (result.IsSuccess)
        {
            // Get image and display dimensions
            Vector2 imageSize = new Vector2(inputImage.width, inputImage.height);
            Vector2 displaySize = new Vector2(imageDisplay.rectTransform.rect.width, 
                                              imageDisplay.rectTransform.rect.height);
            
            // Draw bounding boxes
            XrAiObjectDetectorHelper.DrawBoxes(
                parent: canvasTransform,
                boundingBoxes: result.Data,
                scale: imageSize,
                dimensions: displaySize
            );
        }
    }
}
```

## Visual Styling

The helper automatically applies different colors to distinguish between detected objects:

### Color Palette
- Red, Green, Blue, Yellow
- Cyan, Magenta, Orange, Purple
- Spring Green, Hot Pink

Colors cycle through this palette based on detection order.

### Box Styling
- **Line Width**: 0.001f (thin lines)
- **Material**: Sprites/Default shader
- **Z-Position**: -0.01f (in front of image)
- **Label Position**: Above the bounding box

## Advanced Usage

### Custom Scaling

```csharp
// For normalized coordinates (0-1) to pixel coordinates
Vector2 imageScale = new Vector2(1920, 1080); // Image resolution
Vector2 displayDimensions = new Vector2(800, 600); // Display size

XrAiObjectDetectorHelper.DrawBoxes(
    canvasTransform, 
    detections, 
    imageScale, 
    displayDimensions
);
```

### Filtered Visualization

```csharp
// Only show high-confidence or specific class detections
var filteredDetections = detections
    .Where(d => d.ClassName == "person" || d.ClassName == "car")
    .ToArray();

XrAiObjectDetectorHelper.DrawBoxes(canvasTransform, filteredDetections, scale, dimensions);
```

### Real-time Visualization

```csharp
public class RealtimeDetection : MonoBehaviour
{
    private void Update()
    {
        if (newDetectionsAvailable)
        {
            // Clear old boxes
            XrAiObjectDetectorHelper.ClearBoxes(canvasTransform);
            
            // Draw new detections
            XrAiObjectDetectorHelper.DrawBoxes(
                canvasTransform, 
                latestDetections, 
                imageScale, 
                displayDimensions
            );
            
            newDetectionsAvailable = false;
        }
    }
}
```

## Coordinate System Handling

The helper handles different coordinate systems:

### Normalized Coordinates (0.0 to 1.0)
```csharp
// When detections use normalized coordinates
Vector2 scale = Vector2.one; // No scaling needed
Vector2 dimensions = displaySize; // Use actual display size
```

### Pixel Coordinates
```csharp
// When detections use pixel coordinates
Vector2 scale = Vector2.one; // Use actual coordinates
Vector2 dimensions = new Vector2(imageWidth, imageHeight); // Image dimensions
```

## Generated GameObject Structure

Each bounding box creates a GameObject hierarchy:

```
ObjectBox (GameObject)
├── LineRenderer (Component) - Draws the box outline
└── ObjectLabel (Child GameObject)
    └── TextMeshPro (Component) - Displays class name
```

## Customization

For custom styling, you can extend the helper:

```csharp
public static class CustomObjectDetectorHelper
{
    public static void DrawCustomBoxes(Transform parent, XrAiBoundingBox[] boxes, 
                                      Color boxColor, float lineWidth = 0.002f)
    {
        foreach (var box in boxes)
        {
            var boxObject = CreateCustomBox(parent, boxColor, lineWidth);
            // Custom positioning and styling logic
        }
    }
    
    private static GameObject CreateCustomBox(Transform parent, Color color, float width)
    {
        // Custom box creation with different styling
        // Implementation details...
    }
}
```

## Performance Considerations

- **Object Pooling**: For frequent updates, consider implementing object pooling
- **Batch Operations**: Clear and redraw all boxes together rather than individually
- **LOD System**: Use different detail levels based on box size or distance

## Integration with UI Systems

### Canvas Overlay
```csharp
// For Canvas set to "Screen Space - Overlay"
XrAiObjectDetectorHelper.DrawBoxes(
    canvasTransform, 
    detections, 
    new Vector2(Screen.width, Screen.height), 
    new Vector2(Screen.width, Screen.height)
);
```

### World Space Canvas
```csharp
// For Canvas set to "World Space"
Vector2 worldScale = new Vector2(canvasWorldWidth, canvasWorldHeight);
XrAiObjectDetectorHelper.DrawBoxes(canvasTransform, detections, worldScale, worldScale);
```

## Implementation Notes

- Uses Unity's LineRenderer for drawing box outlines
- Employs TextMeshPro for crisp text labels
- Automatically handles box positioning and sizing
- Provides visual feedback for object detection results
- Supports both 2D and 3D UI integration
- Labels are positioned above bounding boxes for clarity
