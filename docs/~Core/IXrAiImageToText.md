# IXrAiImageToText

The `IXrAiImageToText` interface defines the contract for AI models that generate text descriptions from images.
This interface should be implemented by providers.

## Interface Declaration

```csharp
public interface IXrAiImageToText
```

## Methods

### Initialize

Initializes the workflow with provider-specific options ansynchronously.

```csharp
public Task Initialize(Dictionary<string, string> options = null);
```

**Parameters:**
- `options` (Dictionary<string, string>): Model-specific options

### Execute

Processes an image and generates a text description asynchronously.

```csharp
public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
```

**Parameters:**
- `texture` (Texture2D): The image data as a Unity texture
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;string>>): The callback when inference is complete
