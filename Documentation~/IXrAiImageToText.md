# IXrAiImageToText

The `IXrAiImageToText` interface defines the contract for AI models that generate text descriptions from images. This interface is implemented by various providers like Groq, Google, and Nvidia.

## Interface Declaration

```csharp
public interface IXrAiImageToText
```

## Methods

### Execute

Processes an image and generates a text description asynchronously.

```csharp
public Task<XrAiResult<string>> Execute(byte[] imageBytes, string imageFormat, Dictionary<string, string> options = null)
```

**Parameters:**
- `imageBytes` (byte[]): The image data as a byte array
- `imageFormat` (string): The format of the image (e.g., "image/jpeg", "image/png")
- `options` (Dictionary<string, string>, optional): Model-specific options and parameters

**Returns:**
- `Task<XrAiResult<string>>`: A task that resolves to a result containing the generated text description

## Usage Example

```csharp
// Load the model
IXrAiImageToText imageToText = XrAiFactory.LoadImageToText("Groq", new Dictionary<string, string>
{
    { "apiKey", "your-groq-api-key" }
});

// Convert texture to bytes
byte[] imageBytes = texture.EncodeToJPG();

// Execute the model with options
var result = await imageToText.Execute(imageBytes, "image/jpeg", new Dictionary<string, string>
{
    { "model", "llama-vision-free" },
    { "prompt", "Describe what you see in this image in detail." }
});

// Handle the result
if (result.IsSuccess)
{
    Debug.Log($"Image description: {result.Data}");
}
else
{
    Debug.LogError($"Error: {result.ErrorMessage}");
}
```

## Model-Specific Options

Different providers support different options:

### Groq
- `model`: The model to use (e.g., "llama-vision-free")
- `prompt`: The prompt to guide the image description

### Google
- `prompt`: The prompt for image analysis
- `url`: API endpoint URL

### Nvidia
- `prompt`: The prompt for image description
- `model`: The model identifier
- `url`: API endpoint URL

## Image Format Support

The interface supports common image formats:
- `"image/jpeg"` - JPEG images
- `"image/png"` - PNG images

Use the `XrAiImageHelper.EncodeTexture()` method to convert Unity `Texture2D` objects to the appropriate byte array format.

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<string>` for consistent error handling
- Options dictionary allows for flexible, provider-specific configuration
- Image data should be provided as byte arrays in standard formats
