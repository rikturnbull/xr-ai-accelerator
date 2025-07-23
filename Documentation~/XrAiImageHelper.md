# XrAiImageHelper

The `XrAiImageHelper` class provides utility methods for encoding Unity textures into standard image formats. This helper simplifies the process of converting `Texture2D` objects into byte arrays suitable for AI model processing.

## Class Declaration

```csharp
public class XrAiImageHelper
```

## Methods

### EncodeTexture

Converts a Unity Texture2D to a byte array in the specified image format.

```csharp
public static byte[] EncodeTexture(Texture2D texture, string imageFormat)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to encode
- `imageFormat` (string): The desired output format ("image/jpeg" or "image/png")

**Returns:**
- `byte[]`: The encoded image data as a byte array

**Throws:**
- `ArgumentNullException`: When texture is null
- `NotSupportedException`: When the specified image format is not supported

## Supported Formats

### JPEG Format
- **Format String**: `"image/jpeg"`
- **Use Case**: Photographs, images with complex color gradients
- **Compression**: Lossy compression, smaller file sizes
- **Transparency**: Not supported

### PNG Format
- **Format String**: `"image/png"`
- **Use Case**: Images with transparency, simple graphics, screenshots
- **Compression**: Lossless compression, larger file sizes
- **Transparency**: Supported

## Usage Examples

### Basic Usage

```csharp
public class ImageProcessor : MonoBehaviour
{
    [SerializeField] private Texture2D sourceTexture;
    
    public async void ProcessImage()
    {
        try
        {
            // Convert texture to JPEG bytes
            byte[] jpegData = XrAiImageHelper.EncodeTexture(sourceTexture, "image/jpeg");
            
            // Send to AI model
            var result = await imageToTextModel.Execute(jpegData, "image/jpeg");
            
            if (result.IsSuccess)
            {
                Debug.Log($"Image analysis result: {result.Data}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Image processing failed: {ex.Message}");
        }
    }
}
```

### Format Selection

```csharp
public byte[] EncodeForAI(Texture2D texture, bool preserveTransparency = false)
{
    string format = preserveTransparency ? "image/png" : "image/jpeg";
    return XrAiImageHelper.EncodeTexture(texture, format);
}

public void ProcessScreenshot()
{
    // Capture screenshot
    Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
    
    try
    {
        // Encode as PNG to preserve quality
        byte[] imageData = XrAiImageHelper.EncodeTexture(screenshot, "image/png");
        
        // Process with AI
        ProcessImageWithAI(imageData, "image/png");
    }
    finally
    {
        // Clean up
        Destroy(screenshot);
    }
}
```

### Batch Processing

```csharp
public async Task ProcessImageBatch(Texture2D[] textures)
{
    var encodingTasks = textures.Select(texture => 
        Task.Run(() => XrAiImageHelper.EncodeTexture(texture, "image/jpeg"))
    ).ToArray();
    
    byte[][] encodedImages = await Task.WhenAll(encodingTasks);
    
    for (int i = 0; i < encodedImages.Length; i++)
    {
        await ProcessEncodedImage(encodedImages[i], "image/jpeg");
    }
}
```

## Integration with AI Models

### Image-to-Text Processing

```csharp
public async Task<string> AnalyzeImage(Texture2D image, string prompt)
{
    byte[] imageData = XrAiImageHelper.EncodeTexture(image, "image/jpeg");
    
    var result = await imageToText.Execute(imageData, "image/jpeg", new Dictionary<string, string>
    {
        { "prompt", prompt },
        { "model", "llama-vision-free" }
    });
    
    return result.IsSuccess ? result.Data : null;
}
```

### Object Detection

```csharp
public async Task<XrAiBoundingBox[]> DetectObjects(Texture2D image)
{
    byte[] imageData = XrAiImageHelper.EncodeTexture(image, "image/png");
    
    // Note: Some object detection models work directly with Texture2D
    // But if you need bytes, this helper provides the conversion
    var result = await objectDetector.Execute(image);
    
    return result.IsSuccess ? result.Data : null;
}
```

## Format Optimization

### Quality vs. Size Trade-offs

```csharp
public class ImageFormatOptimizer
{
    public static byte[] OptimizeForAI(Texture2D texture, AIModelType modelType)
    {
        return modelType switch
        {
            AIModelType.ImageToText => 
                // JPEG is usually sufficient for text analysis
                XrAiImageHelper.EncodeTexture(texture, "image/jpeg"),
                
            AIModelType.ObjectDetection => 
                // PNG may preserve important details for detection
                XrAiImageHelper.EncodeTexture(texture, "image/png"),
                
            AIModelType.ImageGeneration => 
                // High quality input for generation models
                XrAiImageHelper.EncodeTexture(texture, "image/png"),
                
            _ => XrAiImageHelper.EncodeTexture(texture, "image/jpeg")
        };
    }
}
```

### Size Validation

```csharp
public bool ValidateImageSize(Texture2D texture, string format)
{
    byte[] encoded = XrAiImageHelper.EncodeTexture(texture, format);
    
    // Check if image is within API limits (e.g., 20MB for OpenAI)
    const int maxSizeBytes = 20 * 1024 * 1024; // 20MB
    
    if (encoded.Length > maxSizeBytes)
    {
        Debug.LogWarning($"Image size ({encoded.Length} bytes) exceeds API limit");
        return false;
    }
    
    return true;
}
```

## Error Handling

### Robust Encoding

```csharp
public static byte[] SafeEncodeTexture(Texture2D texture, string format, bool fallbackToPNG = true)
{
    try
    {
        return XrAiImageHelper.EncodeTexture(texture, format);
    }
    catch (ArgumentNullException)
    {
        Debug.LogError("Cannot encode null texture");
        return null;
    }
    catch (NotSupportedException ex)
    {
        Debug.LogWarning($"Format {format} not supported: {ex.Message}");
        
        if (fallbackToPNG && format != "image/png")
        {
            Debug.Log("Falling back to PNG format");
            return XrAiImageHelper.EncodeTexture(texture, "image/png");
        }
        
        return null;
    }
}
```

### Input Validation

```csharp
private bool ValidateTexture(Texture2D texture)
{
    if (texture == null)
    {
        Debug.LogError("Texture is null");
        return false;
    }
    
    if (texture.width <= 0 || texture.height <= 0)
    {
        Debug.LogError("Texture has invalid dimensions");
        return false;
    }
    
    if (!texture.isReadable)
    {
        Debug.LogError("Texture is not readable. Enable 'Read/Write' in import settings.");
        return false;
    }
    
    return true;
}
```

## Performance Considerations

### Memory Management

```csharp
public void ProcessLargeImageBatch(Texture2D[] textures)
{
    foreach (var texture in textures)
    {
        // Process one at a time to manage memory
        byte[] encoded = XrAiImageHelper.EncodeTexture(texture, "image/jpeg");
        ProcessEncodedImage(encoded);
        
        // Force garbage collection for large batches
        if (textures.Length > 100)
        {
            System.GC.Collect();
        }
    }
}
```

### Async Encoding

```csharp
public async Task<byte[]> EncodeTextureAsync(Texture2D texture, string format)
{
    return await Task.Run(() => XrAiImageHelper.EncodeTexture(texture, format));
}
```

## Implementation Notes

- Uses Unity's built-in `EncodeToJPG()` and `EncodeToPNG()` methods
- Validates input parameters before processing
- Throws descriptive exceptions for error handling
- Supports standard MIME type format strings
- Optimized for AI model integration workflows
- Thread-safe for concurrent usage
