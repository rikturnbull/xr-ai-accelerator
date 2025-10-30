# YoloObjectDetector

The `YoloObjectDetector` class provides local object detection capabilities using YOLO (You Only Look Once) models running on Unity's Sentis inference engine. This implementation runs entirely offline without requiring external API calls.

## Configuration Options

### No Configuration Required

The YoloObjectDetector requires no configuration options as it runs locally using pre-trained YOLO models included with the package.

## Methods

### Initialize

Initializes the YoloObjectDetector instance and loads the YOLO model.

```csharp
public async Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Not used for local YOLO detection

**Returns:**
- `Task`: A task that represents the asynchronous initialization and model loading operation

**Description:**
- Must be called once before using the Execute method
- Loads the YOLO model and labels from StreamingAssets

### Execute

Processes an image and detects objects using the local YOLO model.

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to analyze for object detection
- `options` (Dictionary<string, string>): Not used for local YOLO detection
- `callback` (Action<XrAiResult<XrAiBoundingBox[]>>): Callback function to receive the detected objects

**Process Flow:**
1. Preprocesses the input texture for YOLO model input requirements
2. Runs inference using Unity's Sentis engine on the local YOLO model
3. Post-processes the model output to extract bounding boxes
4. Applies Non-Maximum Suppression to filter overlapping detections
5. Returns the detected objects through the callback

**Error Handling:**
- Returns failure result if model loading fails
- Validates input texture format and dimensions

## Model Details

### Default Model
- **Model**: YOLOv11n (nano) with segmentation
- **File**: `yolo11n-seg.sentis` (located in StreamingAssets)
- **Labels**: `yolo11n-labels.txt` (COCO dataset classes)
- **Backend**: Unity Sentis (CPU by default)

### Supported Objects
The model can detect 80 different object classes from the COCO dataset, including:
- People, vehicles, animals
- Furniture, electronics, kitchenware
- Sports equipment, food items
- And many more common objects

## Usage Example

```csharp
var yoloObjectDetector = XrAiFactory.LoadObjectDetector("Yolo");

// Initialize the model (no options needed)
await yoloObjectDetector.Initialize();

await yoloObjectDetector.Execute(inputTexture, null, result =>
{
    if (result.IsSuccess)
    {
        XrAiBoundingBox[] detectedObjects = result.Data;
        Debug.Log($"Detected {detectedObjects.Length} objects locally");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```

## Notes

- The YoloExecutor handles all model management internally
- Model files are automatically copied to StreamingAssets during development
