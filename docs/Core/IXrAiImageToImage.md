# IXrAiImageToImage

The `IXrAiImageToImage` interface defines the contract for AI models that transform images into other images.
This interface should be implemented by providers for image-to-image transformation tasks.

## Interface Declaration

```csharp
public interface IXrAiImageToImage
```

## Methods

### Initialize

Initializes the workflow with provider-specific options asynchronously.

```csharp
public Task Initialize(Dictionary<string, string> options = null);
```

**Parameters:**
- `options` (Dictionary<string, string>): Model-specific options

### Execute

Processes an input image and generates a transformed image asynchronously.

```csharp
public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback);
```

**Parameters:**
- `texture` (Texture2D): The input image data as a Unity texture
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;Texture2D>>): The callback when inference is complete
