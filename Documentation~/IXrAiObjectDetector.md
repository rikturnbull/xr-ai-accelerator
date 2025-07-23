# IXrAiObjectDetector

The `IXrAiObjectDetector` interface defines the contract for AI models that detect and locate objects within images. This interface returns bounding box information for detected objects.

## Interface Declaration

```csharp
public interface IXrAiObjectDetector
```

## Methods

### Execute

Analyzes an image and detects objects, returning their locations and classifications.

```csharp
public Task<XrAiResult<XrAiBoundingBox[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
```

**Parameters:**
- `texture` (Texture2D): The input Unity texture containing the image to analyze
- `options` (Dictionary<string, string>, optional): Model-specific options and parameters

**Returns:**
- `Task<XrAiResult<XrAiBoundingBox[]>>`: A task that resolves to a result containing an array of detected object bounding boxes

## Usage Example

```csharp
// Load the model
IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector("Yolo", new Dictionary<string, string>
{
    { "confidence", "0.5" },
    { "threshold", "0.4" }
}, assets);

// Execute object detection
var result = await objectDetector.Execute(inputTexture, new Dictionary<string, string>
{
    { "maxDetections", "10" }
});

// Handle the result
if (result.IsSuccess)
{
    XrAiBoundingBox[] detections = result.Data;
    
    foreach (var detection in detections)
    {
        Debug.Log($"Detected {detection.ClassName} at ({detection.CenterX}, {detection.CenterY}) " +
                  $"with size {detection.Width}x{detection.Height}");
    }
    
    // Draw bounding boxes on UI
    XrAiObjectDetectorHelper.DrawBoxes(parentTransform, detections, scale, dimensions);
}
else
{
    Debug.LogError($"Object detection failed: {result.ErrorMessage}");
}
```

## Supported Providers

### Google
Cloud-based object detection service.

**Required Options:**
- `apiKey`: Your Google Cloud API key

**Optional Parameters:**
- `url`: Custom API endpoint URL

### YOLO (Local)
Local inference using Sentis and YOLO models.

**Required:**
- `assets`: XrAiAssets component with model references

**Optional Parameters:**
- Model-specific configuration options

### Roboflow
Cloud-based object detection with custom models.

**Required Options:**
- `apiKey`: Your Roboflow API key

**Optional Parameters:**
- `url`: API endpoint URL

### RoboflowLocal
Local inference using Roboflow-trained models.

**Required Options:**
- `apiKey`: Authentication key

**Optional Parameters:**
- `url`: Local service endpoint

## Bounding Box Format

The `XrAiBoundingBox` structure contains:
- `CenterX`: X coordinate of the bounding box center
- `CenterY`: Y coordinate of the bounding box center  
- `Width`: Width of the bounding box
- `Height`: Height of the bounding box
- `ClassName`: The detected object class/category name

Coordinates are typically normalized (0.0 to 1.0) relative to the image dimensions.

## Visualization

Use the `XrAiObjectDetectorHelper` class to visualize detection results:

```csharp
// Clear previous boxes
XrAiObjectDetectorHelper.ClearBoxes(parentTransform);

// Draw new detection boxes
XrAiObjectDetectorHelper.DrawBoxes(
    parentTransform, 
    detections, 
    new Vector2(imageWidth, imageHeight), // scale
    new Vector2(displayWidth, displayHeight) // dimensions
);
```

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<XrAiBoundingBox[]>` for consistent error handling
- Bounding box coordinates may need scaling based on display requirements
- Different providers may detect different object categories
- Local models (YOLO) require appropriate model assets to be configured

## Common Options

- `confidence`: Minimum confidence threshold for detections (0.0 to 1.0)
- `threshold`: Non-maximum suppression threshold
- `maxDetections`: Maximum number of objects to detect
- `classes`: Specific object classes to detect (provider-dependent)
