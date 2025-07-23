# IXrAImageTo3d

The `IXrAImageTo3d` interface defines the contract for AI models that generate 3D models from 2D images. This interface enables conversion of flat images into three-dimensional representations.

## Interface Declaration

```csharp
public interface IXrAImageTo3d
```

## Methods

### Execute

Processes a 2D image and generates a 3D model asynchronously.

```csharp
public Task<XrAiResult<byte[]>> Execute(Texture2D texture, Dictionary<string, string> options = null)
```

**Parameters:**
- `texture` (Texture2D): The input Unity texture containing the 2D image
- `options` (Dictionary<string, string>, optional): Model-specific options and parameters

**Returns:**
- `Task<XrAiResult<byte[]>>`: A task that resolves to a result containing the 3D model data as bytes

## Usage Example

```csharp
// Load the model
IXrAImageTo3d imageTo3d = XrAiFactory.LoadImageTo3d("StabilityAi", new Dictionary<string, string>
{
    { "apiKey", "your-stability-api-key" }
});

// Execute the model
var result = await imageTo3d.Execute(inputTexture, new Dictionary<string, string>
{
    { "format", "obj" },
    { "quality", "high" }
});

// Handle the result
if (result.IsSuccess)
{
    // Process the 3D model data
    byte[] modelData = result.Data;
    
    // Convert to GameObject using appropriate helper
    // For OBJ files: XrAiOBJHelper.ConvertToGameObject(modelData)
    // For GLTF files: XrAiGLTFHelper.ConvertToGameObject(modelData)
}
else
{
    Debug.LogError($"Error generating 3D model: {result.ErrorMessage}");
}
```

## Supported Providers

### StabilityAI
Currently supported provider for Image-to-3D generation.

**Required Options:**
- `apiKey`: Your Stability AI API key

**Optional Parameters:**
- Model-specific parameters depending on the service capabilities

## 3D Model Formats

The returned byte array typically contains 3D model data in formats such as:
- **OBJ**: Wavefront OBJ format
- **GLTF**: GL Transmission Format
- **PLY**: Polygon File Format

The specific format depends on the provider and model configuration.

## Helper Classes

Use the appropriate helper classes to convert the byte array result into Unity GameObjects:

- `XrAiOBJHelper`: For OBJ format models
- `XrAiGLTFHelper`: For GLTF format models

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<byte[]>` for consistent error handling
- The input must be a Unity `Texture2D` object
- Generated 3D models may require post-processing for optimal Unity integration
- Different providers may have varying quality, speed, and format capabilities

## Error Handling

```csharp
if (!result.IsSuccess)
{
    Debug.LogError($"3D generation failed: {result.ErrorMessage}");
    // Handle specific error cases
    // - Invalid API key
    // - Unsupported image format
    // - Service unavailable
    // - Processing timeout
}
```
