# IXrAiImageToImage

The `IXrAiImageToImage` interface defines the contract for AI models that transform or modify images based on text prompts. This interface enables image-to-image translation, style transfer, and image editing operations.

## Interface Declaration

```csharp
public interface IXrAiImageToImage
```

## Methods

### Execute

Transforms an input image based on provided options and prompts.

```csharp
public Task<XrAiResult<Texture2D>> Execute(Texture2D texture, Dictionary<string, string> options = null)
```

**Parameters:**
- `texture` (Texture2D): The input Unity texture to be transformed
- `options` (Dictionary<string, string>, optional): Model-specific options including transformation prompts

**Returns:**
- `Task<XrAiResult<Texture2D>>`: A task that resolves to a result containing the transformed image as a Unity Texture2D

## Usage Example

```csharp
// Load the model
IXrAiImageToImage imageToImage = XrAiFactory.LoadImageToImage("OpenAI", new Dictionary<string, string>
{
    { "apiKey", "your-openai-api-key" }
});

// Transform an image
var result = await imageToImage.Execute(inputTexture, new Dictionary<string, string>
{
    { "prompt", "Transform this photo into a watercolor painting" },
    { "size", "1024x1024" },
    { "model", "dall-e-2" },
    { "n", "1" }
});

// Handle the result
if (result.IsSuccess)
{
    Texture2D transformedImage = result.Data;
    
    // Display the transformed image
    resultImageRenderer.material.mainTexture = transformedImage;
    
    // Or update UI
    beforeAfterUI.SetAfterImage(transformedImage);
}
else
{
    Debug.LogError($"Image transformation failed: {result.ErrorMessage}");
}
```

## Model-Specific Options

Different providers support different transformation options:

### OpenAI (DALL-E)
- `prompt`: Description of how to transform the image (required)
- `model`: Model version (e.g., "dall-e-2")
- `size`: Output image dimensions (e.g., "256x256", "512x512", "1024x1024")
- `n`: Number of variations to generate (1-10)
- `mask`: Optional mask image for inpainting (byte array)

## Common Use Cases

### Style Transfer
```csharp
var options = new Dictionary<string, string>
{
    { "prompt", "Convert this photograph into an impressionist painting in the style of Monet" },
    { "size", "1024x1024" }
};
```

### Object Replacement
```csharp
var options = new Dictionary<string, string>
{
    { "prompt", "Replace the car in this image with a bicycle, keeping everything else the same" },
    { "size", "512x512" }
};
```

### Enhancement and Restoration
```csharp
var options = new Dictionary<string, string>
{
    { "prompt", "Enhance this image to be high resolution and remove any noise or artifacts" },
    { "size", "1024x1024" }
};
```

### Creative Transformations
```csharp
var options = new Dictionary<string, string>
{
    { "prompt", "Transform this daytime scene into a magical nighttime scene with stars and moonlight" },
    { "size", "1024x1024" }
};
```

## Before/After Comparison

Create compelling before/after visualizations:

```csharp
public class ImageTransformationDemo : MonoBehaviour
{
    [SerializeField] private RawImage beforeImage;
    [SerializeField] private RawImage afterImage;
    [SerializeField] private Slider comparisonSlider;
    
    public async void TransformImage(Texture2D original, string prompt)
    {
        // Show original
        beforeImage.texture = original;
        
        // Transform
        var result = await imageToImage.Execute(original, new Dictionary<string, string>
        {
            { "prompt", prompt }
        });
        
        if (result.IsSuccess)
        {
            afterImage.texture = result.Data;
            EnableComparison();
        }
    }
    
    private void EnableComparison()
    {
        comparisonSlider.onValueChanged.AddListener(OnComparisonChanged);
    }
    
    private void OnComparisonChanged(float value)
    {
        beforeImage.gameObject.SetActive(value < 0.5f);
        afterImage.gameObject.SetActive(value >= 0.5f);
    }
}
```

## Batch Processing

Process multiple images with the same transformation:

```csharp
public async Task TransformImageBatch(Texture2D[] images, string prompt)
{
    List<Task<XrAiResult<Texture2D>>> tasks = new List<Task<XrAiResult<Texture2D>>>();
    
    foreach (var image in images)
    {
        var task = imageToImage.Execute(image, new Dictionary<string, string>
        {
            { "prompt", prompt }
        });
        tasks.Add(task);
    }
    
    var results = await Task.WhenAll(tasks);
    
    for (int i = 0; i < results.Length; i++)
    {
        if (results[i].IsSuccess)
        {
            ProcessTransformedImage(results[i].Data, i);
        }
        else
        {
            Debug.LogError($"Failed to transform image {i}: {results[i].ErrorMessage}");
        }
    }
}
```

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<Texture2D>` for consistent error handling
- Input images should be in standard formats (PNG, JPEG)
- Transformation quality depends on prompt clarity and image content
- Processing time varies based on image complexity and desired transformation

## Error Handling

```csharp
if (!result.IsSuccess)
{
    Debug.LogError($"Image transformation failed: {result.ErrorMessage}");
    // Handle specific error cases:
    // - Invalid API key
    // - Unsupported image format
    // - Prompt content policy violations
    // - Image too large/small
    // - Service rate limits
    // - Network connectivity issues
}
```

## Best Practices

- Use clear, specific prompts describing the desired transformation
- Consider the relationship between input image content and transformation goal
- Test different prompt variations to achieve desired results
- Implement proper loading states during transformation
- Cache results when applying the same transformation to multiple images
- Handle edge cases where transformation may not be possible
- Provide fallback options when transformation fails
