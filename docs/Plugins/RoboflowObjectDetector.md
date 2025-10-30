# RoboflowObjectDetector

The `RoboflowObjectDetector` class provides object detection capabilities using Roboflow's inference API. It supports both serverless and inference endpoints with automatic detection of endpoint type.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Roboflow API key for authentication
- **Example**: `"your-roboflow-api-key"`

### Optional Options

#### url
- **Type**: `string`
- **Required**: No
- **Default**: `"https://serverless.roboflow.com/infer/workflows/xr-shu81/yolov11-object-detection"`
- **Description**: The URL for the Roboflow API endpoint (supports both serverless and inference endpoints)

#### modelId
- **Type**: `string`
- **Required**: No
- **Default**: `""`
- **Description**: The model ID to use for object detection (required for inference endpoints, not used for serverless)

## Methods

### Initialize

Initializes the RoboflowObjectDetector instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the Roboflow service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Processes an image and detects objects using Roboflow's API.

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to analyze for object detection
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<XrAiBoundingBox[]>>): Callback function to receive the detected objects

**Process Flow:**
1. Encodes the input texture to JPEG format
2. Converts the image to base64 format
3. Creates appropriate request format based on endpoint type (serverless vs inference)
4. Sends the request to Roboflow's API
5. Parses the response to extract bounding boxes and keypoints
6. Returns the detected objects through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates API response content and format

## Usage Example

```csharp
var roboflowObjectDetector = XrAiFactory.LoadObjectDetector("Roboflow");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-roboflow-api-key",
    ["url"] = "https://serverless.roboflow.com/infer/workflows/your-workflow/model"
};

// Initialize must be called once before Execute
await roboflowObjectDetector.Initialize(options);

await roboflowObjectDetector.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        XrAiBoundingBox[] detectedObjects = result.Data;
        Debug.Log($"Detected {detectedObjects.Length} objects");
        
        foreach (var obj in detectedObjects)
        {
            Debug.Log($"Object: {obj.ClassName} at ({obj.CenterX}, {obj.CenterY})");
        }
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```

## Notes

- The class automatically detects endpoint type based on URL pattern
- Keypoint detection is supported when available in the model response
- Images are automatically encoded as JPEG base64 for transmission
- Both serverless workflows and direct model inference are supported