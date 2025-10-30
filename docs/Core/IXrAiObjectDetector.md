# IXrAiObjectDetector

The `IXrAiObjectDetector` interface defines the contract for AI models that detect and locate objects within images.
This interface should be implemented by providers for object detection tasks.

## Interface Declaration

```csharp
public interface IXrAiObjectDetector
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

Processes an image and detects objects within it asynchronously.

```csharp
public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback);
```

**Parameters:**
- `texture` (Texture2D): The image data as a Unity texture
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;XrAiBoundingBox[]>>): The callback when inference is complete
