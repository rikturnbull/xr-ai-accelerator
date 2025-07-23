# IXrAiTextToImage

The `IXrAiTextToImage` interface defines the contract for AI models that generate images from text descriptions. This interface creates visual content based on textual prompts.

## Interface Declaration

```csharp
public interface IXrAiTextToImage
```

## Methods

### Execute

Generates an image from text description asynchronously.

```csharp
public Task<XrAiResult<Texture2D>> Execute(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Model-specific options including the text prompt

**Returns:**
- `Task<XrAiResult<Texture2D>>`: A task that resolves to a result containing the generated image as a Unity Texture2D

## Usage Example

```csharp
// Load the model
IXrAiTextToImage textToImage = XrAiFactory.LoadTextToImage("OpenAI", new Dictionary<string, string>
{
    { "apiKey", "your-openai-api-key" }
});

// Generate an image
var result = await textToImage.Execute(new Dictionary<string, string>
{
    { "prompt", "A futuristic city with flying cars at sunset" },
    { "size", "1024x1024" },
    { "quality", "standard" },
    { "model", "dall-e-3" }
});

// Handle the result
if (result.IsSuccess)
{
    Texture2D generatedImage = result.Data;
    
    // Use the generated image
    imageRenderer.material.mainTexture = generatedImage;
    
    // Or apply to UI element
    rawImage.texture = generatedImage;
}
else
{
    Debug.LogError($"Image generation failed: {result.ErrorMessage}");
}
```

## Model-Specific Options

Different providers support different configuration options:

### OpenAI (DALL-E)
- `prompt`: Text description of the desired image (required)
- `model`: Model version (e.g., "dall-e-2", "dall-e-3")
- `size`: Image dimensions (e.g., "256x256", "512x512", "1024x1024", "1024x1792", "1792x1024")
- `quality`: Image quality ("standard" or "hd")
- `style`: Image style ("vivid" or "natural")
- `n`: Number of images to generate (1-10 for DALL-E 2, 1 for DALL-E 3)

## Prompt Engineering

Effective prompts can significantly improve image quality:

```csharp
// Good prompts are specific and descriptive
string prompt = "A serene mountain landscape with snow-capped peaks, " +
                "reflected in a crystal-clear alpine lake, " +
                "surrounded by evergreen forests, " +
                "photographed during golden hour with soft lighting";

// Include style descriptors
string stylePrompt = "A cyberpunk street scene in the style of Blade Runner, " +
                     "neon lights, rain-soaked streets, digital art, " +
                     "highly detailed, atmospheric lighting";
```

## Image Integration

The returned `Texture2D` can be used throughout Unity:

```csharp
// Apply to 3D object material
Renderer renderer = gameObject.GetComponent<Renderer>();
renderer.material.mainTexture = generatedImage;

// Use in UI system
RawImage uiImage = GetComponent<RawImage>();
uiImage.texture = generatedImage;

// Save to file
byte[] imageData = generatedImage.EncodeToPNG();
File.WriteAllBytes(Application.persistentDataPath + "/generated_image.png", imageData);

// Create sprite for 2D games
Sprite sprite = Sprite.Create(
    generatedImage, 
    new Rect(0, 0, generatedImage.width, generatedImage.height), 
    new Vector2(0.5f, 0.5f)
);
```

## Performance Considerations

Image generation can be resource-intensive:

```csharp
// Show loading indicator
loadingIndicator.SetActive(true);

try
{
    var result = await textToImage.Execute(options);
    // Process result...
}
finally
{
    loadingIndicator.SetActive(false);
}

// Consider caching generated images
private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

public async Task<Texture2D> GetOrGenerateImage(string prompt)
{
    if (imageCache.ContainsKey(prompt))
    {
        return imageCache[prompt];
    }
    
    var result = await textToImage.Execute(new Dictionary<string, string>
    {
        { "prompt", prompt }
    });
    
    if (result.IsSuccess)
    {
        imageCache[prompt] = result.Data;
        return result.Data;
    }
    
    return null;
}
```

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<Texture2D>` for consistent error handling
- Generated images are ready for immediate use in Unity
- Prompt quality significantly affects output quality
- Generation time varies based on complexity and provider

## Error Handling

```csharp
if (!result.IsSuccess)
{
    Debug.LogError($"Image generation failed: {result.ErrorMessage}");
    // Handle specific error cases:
    // - Invalid API key
    // - Prompt content policy violations
    // - Unsupported image dimensions
    // - Service rate limits
    // - Network connectivity issues
}
```

## Best Practices

- Use descriptive, specific prompts for better results
- Cache generated images to reduce API calls and costs
- Implement proper loading states for user feedback
- Consider image dimensions based on intended use
- Test prompts to understand model capabilities and limitations
- Handle content policy violations gracefully
- Implement retry logic for network failures
