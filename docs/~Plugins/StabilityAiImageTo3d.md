# StabilityAiImageTo3d

The `StabilityAiImageTo3d` class provides image-to-3D model generation capabilities using Stability AI's Stable Fast 3D API.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Stability AI API key for authentication
- **Example**: `"sk-your-stability-ai-api-key"`

### Optional Options

#### foregroundRatio
- **Type**: `float`
- **Required**: No
- **Default**: `"0.85"`
- **Range**: 0.1-1.0
- **Description**: Amount of padding around the object to be processed within the frame

#### textureResolution
- **Type**: `int`
- **Required**: No
- **Default**: `"1024"`
- **Options**: 512, 1024, 2048
- **Description**: Resolution of the generated 3D model texture

#### url
- **Type**: `string`
- **Required**: No
- **Default**: `"https://api.stability.ai/v2beta/3d/stable-fast-3d"`
- **Description**: Stability AI API endpoint URL

## Methods

### Initialize

Initializes the StabilityAiImageTo3d instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the Stability AI service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Converts a 2D image to a 3D model using Stability AI's Stable Fast 3D API.

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to convert to 3D
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<byte[]>>): Callback function to receive the generated 3D model data

**Process Flow:**
1. Encodes the input texture to PNG format
2. Creates a multipart form request with image and parameters
3. Sends the request to Stability AI's API with Bearer token authentication
4. Returns the generated 3D model as byte array through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates API response status and content

## Usage Example

```csharp
var stabilityAiImageTo3d = XrAiFactory.LoadImageTo3d("StabilityAi");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-stability-ai-api-key",
    ["foregroundRatio"] = "0.9",
    ["textureResolution"] = "1024"
};

// Initialize must be called once before Execute
await stabilityAiImageTo3d.Initialize(options);

await stabilityAiImageTo3d.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        byte[] modelData = result.Data;
        GameObject go = Siccity.GLTFUtility.Importer.LoadFromBytes(result.Data);
        Debug.Log("3D model generated successfully");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```
