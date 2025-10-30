# IXrAiTextToImage

The `IXrAiTextToImage` interface defines the contract for AI models that generate images from text descriptions.
This interface should be implemented by providers for text-to-image generation.

## Interface Declaration

```csharp
public interface IXrAiTextToImage
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

Processes text input and generates an image asynchronously.

```csharp
public Task Execute(Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback);
```

**Parameters:**
- `options` (Dictionary<string, string>): Model-specific options and parameters including the text prompt
- `callback` (Action<XrAiResult&lt;Texture2D>>): The callback when inference is complete
