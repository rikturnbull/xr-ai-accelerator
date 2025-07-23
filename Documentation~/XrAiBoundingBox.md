# XrAiBoundingBox

The `XrAiBoundingBox` struct represents a detected object's location and classification information in object detection results. It provides the spatial boundaries and identification of objects found within an image.

## Struct Declaration

```csharp
public struct XrAiBoundingBox
```

## Properties

### CenterX

The X coordinate of the bounding box center.

```csharp
public float CenterX;
```

**Value Range:** Typically 0.0 to 1.0 (normalized) or pixel coordinates depending on the provider

### CenterY

The Y coordinate of the bounding box center.

```csharp
public float CenterY;
```

**Value Range:** Typically 0.0 to 1.0 (normalized) or pixel coordinates depending on the provider

### Width

The width of the bounding box.

```csharp
public float Width;
```

**Value Range:** Typically 0.0 to 1.0 (normalized) or pixel dimensions depending on the provider

### Height

The height of the bounding box.

```csharp
public float Height;
```

**Value Range:** Typically 0.0 to 1.0 (normalized) or pixel dimensions depending on the provider

### ClassName

The detected object's class or category name.

```csharp
public string ClassName;
```

**Examples:** "person", "car", "dog", "bicycle", "apple", etc.

## Usage Example

```csharp
// Process detection results
XrAiBoundingBox[] detections = objectDetectionResult.Data;

foreach (var detection in detections)
{
    Debug.Log($"Detected {detection.ClassName}:");
    Debug.Log($"  Center: ({detection.CenterX:F3}, {detection.CenterY:F3})");
    Debug.Log($"  Size: {detection.Width:F3} x {detection.Height:F3}");
    
    // Calculate bounding box corners
    float left = detection.CenterX - detection.Width / 2;
    float right = detection.CenterX + detection.Width / 2;
    float top = detection.CenterY + detection.Height / 2;
    float bottom = detection.CenterY - detection.Height / 2;
    
    Debug.Log($"  Bounds: ({left:F3}, {bottom:F3}) to ({right:F3}, {top:F3})");
}
```

## Coordinate System

The coordinate system used depends on the detection provider:

### Normalized Coordinates (0.0 to 1.0)
Most providers use normalized coordinates where:
- `(0, 0)` represents the top-left corner of the image
- `(1, 1)` represents the bottom-right corner of the image
- Values are relative to image dimensions

### Pixel Coordinates
Some providers may return actual pixel coordinates:
- Values represent actual pixel positions within the image
- Coordinate origin may vary by provider

## Converting to Screen Coordinates

To display bounding boxes on UI elements:

```csharp
public Vector2 ConvertToScreenCoordinates(XrAiBoundingBox box, Vector2 imageSize, Vector2 screenSize)
{
    // Convert normalized coordinates to screen coordinates
    float screenX = box.CenterX * screenSize.x;
    float screenY = box.CenterY * screenSize.y;
    
    return new Vector2(screenX, screenY);
}

public Rect GetScreenRect(XrAiBoundingBox box, Vector2 imageSize, Vector2 screenSize)
{
    float screenWidth = box.Width * screenSize.x;
    float screenHeight = box.Height * screenSize.y;
    float screenX = (box.CenterX * screenSize.x) - (screenWidth / 2);
    float screenY = (box.CenterY * screenSize.y) - (screenHeight / 2);
    
    return new Rect(screenX, screenY, screenWidth, screenHeight);
}
```

## Filtering and Processing

Common operations with bounding box arrays:

```csharp
// Filter by class name
var people = detections.Where(d => d.ClassName == "person").ToArray();

// Filter by size (remove very small detections)
var significantDetections = detections.Where(d => d.Width > 0.05f && d.Height > 0.05f).ToArray();

// Sort by detection size (largest first)
var sortedBySize = detections.OrderByDescending(d => d.Width * d.Height).ToArray();

// Group by class
var groupedDetections = detections.GroupBy(d => d.ClassName).ToArray();

// Find center detection (closest to image center)
var centerDetection = detections.OrderBy(d => 
    Math.Sqrt(Math.Pow(d.CenterX - 0.5, 2) + Math.Pow(d.CenterY - 0.5, 2))
).FirstOrDefault();
```

## Visualization Helper

The `XrAiObjectDetectorHelper` class provides methods to visualize bounding boxes:

```csharp
// Draw all detections
XrAiObjectDetectorHelper.DrawBoxes(
    parentTransform: canvas.transform,
    boundingBoxes: detections,
    scale: new Vector2(imageWidth, imageHeight),
    dimensions: new Vector2(canvasWidth, canvasHeight)
);

// Clear previous visualizations
XrAiObjectDetectorHelper.ClearBoxes(canvas.transform);
```

## Working with Confidence Scores

Some providers may include confidence information in custom implementations:

```csharp
// Extended bounding box with confidence (custom implementation)
public struct ExtendedBoundingBox
{
    public XrAiBoundingBox BoundingBox;
    public float Confidence;
    
    public bool IsHighConfidence => Confidence > 0.8f;
}

// Filter by confidence
var highConfidenceDetections = extendedDetections
    .Where(d => d.Confidence > 0.5f)
    .Select(d => d.BoundingBox)
    .ToArray();
```

## Implementation Notes

- All coordinate values are `float` type for precision
- `ClassName` should be checked for null or empty values
- Coordinate systems may vary between different detection providers
- Always validate bounding box values are within expected ranges
- Consider the image's aspect ratio when converting between coordinate systems
