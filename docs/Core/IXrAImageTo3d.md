# IXrAImageTo3d

The `IXrAImageTo3d` interface defines the contract for AI models that convert 2D images into 3D model data.
This interface should be implemented by providers for image-to-3D conversion tasks.

## Interface Declaration

```csharp
public interface IXrAImageTo3d
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

Processes an input image and generates 3D model data asynchronously.

```csharp
public Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback);
```

**Parameters:**
- `texture` (Texture2D): The input image data as a Unity texture
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;byte[]>>): The callback when inference is complete
