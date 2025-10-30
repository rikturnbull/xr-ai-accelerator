# GoogleObjectDetector

The `GoogleObjectDetector` class provides object detection capabilities using Google's Gemini Vision API. It inherits from `GoogleAiBase` and implements the `IXrAiObjectDetector` interface to enable integration with the XR AI Accelerator framework.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Google API key for authentication with the Gemini API
- **Example**: `"AIzaSyC-your-api-key-here"`

### Optional Options

#### imageQuality
- **Type**: `int`
- **Required**: No
- **Default**: `"100"`
- **Range**: 1-100
- **Description**: JPEG compression quality where lower values produce smaller files but reduced image quality

#### maxImageDimension
- **Type**: `int`
- **Required**: No
- **Default**: `"512"`
- **Description**: Maximum width or height for image scaling before processing
- **Common Values**: 512, 1024, 1536

#### url
- **Type**: `string`
- **Required**: No
- **Default**: `"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"`
- **Description**: The Google Gemini API endpoint URL

## Methods

### Initialize

Initializes the GoogleObjectDetector instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the Google AI service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Processes an image and detects objects using Google's Gemini Vision API.

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to analyze for object detection
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<XrAiBoundingBox[]>>): Callback function to receive the detected objects

**Process Flow:**
1. Scales the input texture to the specified maximum dimension
2. Encodes the scaled texture to JPEG with the specified quality
3. Sends the image to Google's Gemini API with object detection prompt
4. Parses the JSON response to extract bounding boxes
5. Converts coordinates from normalized format to pixel coordinates
6. Returns the detected objects through the callback

**Error Handling:**
- Returns failure result if API call fails

## Usage Example

```csharp
var googleObjectDetector = XrAiFactory.LoadObjectDetector("Google");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-google-api-key"
};

// Initialize must be called once before Execute
await googleObjectDetector.Initialize(options);

await googleObjectDetector.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        XrAiBoundingBox[] detectedObjects = result.Data;
        Debug.Log($"Detected {detectedObjects.Length} objects");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```

## API Integration

### Google Gemini Vision API
- **Model**: gemini-2.0-flash (default)
- **Authentication**: API key via `x-goog-api-key` header
- **Input Format**: Base64-encoded JPEG images
- **Response Format**: JSON with object detection data
- **Coordinate System**: Normalized 0-1000 format converted to pixel coordinates

### Detection Prompt
The class uses a predefined prompt for object detection:
```
"Detect the all of the prominent items in the image. The box_2d should be [ymin, xmin, ymax, xmax] normalized to 0-1000."
```
