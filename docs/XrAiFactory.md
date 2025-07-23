# XrAiFactory

The `XrAiFactory` class serves as the central factory for creating instances of various AI model pipelines in the XR AI Library. It provides static methods to load different types of AI models by name.

## Class Declaration

```csharp
public class XrAiFactory
```

## Methods

### LoadImageTo3d

Creates an instance of an Image-to-3D model pipeline.

```csharp
public static IXrAImageTo3d LoadImageTo3d(string name, Dictionary<string, string> options = null)
```

**Parameters:**
- `name` (string): The name of the Image-to-3D model to load (e.g., "StabilityAi")
- `options` (Dictionary<string, string>, optional): Configuration options for the model

**Returns:**
- `IXrAImageTo3d`: An instance of the requested Image-to-3D model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadObjectDetector

Creates an instance of an object detection model pipeline.

```csharp
public static IXrAiObjectDetector LoadObjectDetector(string name, Dictionary<string, string> options = null, XrAiAssets assets = null)
```

**Parameters:**
- `name` (string): The name of the object detector model to load (e.g., "Google", "Yolo", "Roboflow", "RoboflowLocal")
- `options` (Dictionary<string, string>, optional): Configuration options for the model
- `assets` (XrAiAssets, optional): Asset references required by some models

**Returns:**
- `IXrAiObjectDetector`: An instance of the requested object detector model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadImageToText

Creates an instance of an Image-to-Text model pipeline.

```csharp
public static IXrAiImageToText LoadImageToText(string name, Dictionary<string, string> properties = null)
```

**Parameters:**
- `name` (string): The name of the Image-to-Text model to load (e.g., "Groq", "Google", "Nvidia")
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiImageToText`: An instance of the requested Image-to-Text model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadImageToImage

Creates an instance of an Image-to-Image model pipeline.

```csharp
public static IXrAiImageToImage LoadImageToImage(string name, Dictionary<string, string> properties = null)
```

**Parameters:**
- `name` (string): The name of the Image-to-Image model to load (e.g., "OpenAI")
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiImageToImage`: An instance of the requested Image-to-Image model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadTextToImage

Creates an instance of a Text-to-Image model pipeline.

```csharp
public static IXrAiTextToImage LoadTextToImage(string name, Dictionary<string, string> properties = null)
```

**Parameters:**
- `name` (string): The name of the Text-to-Image model to load (e.g., "OpenAI")
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiTextToImage`: An instance of the requested Text-to-Image model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadSpeechToText

Creates an instance of a Speech-to-Text model pipeline.

```csharp
public static IXrAiSpeechToText LoadSpeechToText(string name, Dictionary<string, string> properties = null)
```

**Parameters:**
- `name` (string): The name of the Speech-to-Text model to load (e.g., "OpenAI")
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiSpeechToText`: An instance of the requested Speech-to-Text model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

### LoadTextToSpeech

Creates an instance of a Text-to-Speech model pipeline.

```csharp
public static IXrAiTextToSpeech LoadTextToSpeech(string name, Dictionary<string, string> properties = null)
```

**Parameters:**
- `name` (string): The name of the Text-to-Speech model to load
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiTextToSpeech`: An instance of the requested Text-to-Speech model

**Throws:**
- `NotSupportedException`: When the specified model name is not supported

## Usage Example

```csharp
// Load a Groq Image-to-Text model
IXrAiImageToText imageToText = XrAiFactory.LoadImageToText("Groq", new Dictionary<string, string> 
{
    { "apiKey", "your-api-key" }
});

// Load a YOLO object detector
IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector("Yolo", null, assets);
```

## Notes

- The factory uses the provider name to determine which specific implementation to instantiate
- Options and properties dictionaries are passed through to the underlying model implementations
- Each model type has its own set of supported providers
- The factory throws `NotSupportedException` for unsupported model names
