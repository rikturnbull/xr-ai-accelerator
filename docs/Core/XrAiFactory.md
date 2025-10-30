# XrAiFactory

The `XrAiFactory` class serves as the central factory for creating instances of various AI workflow pipelines in the XR AI Library. It provides static methods to load different workflow providers by name.

## Class Declaration

```csharp
public class XrAiFactory
```

## Methods

### LoadImageTo3d

Creates an instance of an Image-to-3D workflow.

```csharp
public static IXrAImageTo3d LoadImageTo3d(string name)
```

**Parameters:**
- `name` (string): The name of the Image-to-3D provider to load (e.g., "StabilityAi")

**Returns:**
- `IXrAImageTo3d`: An instance of the requested Image-to-3D workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadObjectDetector

Creates an instance of an object detection workflow.

```csharp
public static IXrAiObjectDetector LoadObjectDetector(string name)
```

**Parameters:**
- `name` (string): The name of the object detector model to load (e.g., "Google", "Yolo", "Roboflow", "RoboflowLocal")

**Returns:**
- `IXrAiObjectDetector`: An instance of the requested object detector workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadImageToText

Creates an instance of an Image-to-Text workflow.

```csharp
public static IXrAiImageToText LoadImageToText(string name)
```

**Parameters:**
- `name` (string): The name of the Image-to-Text provider to load (e.g., "Groq", "Google", "Nvidia")

**Returns:**
- `IXrAiImageToText`: An instance of the requested Image-to-Text workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadImageToImage

Creates an instance of an Image-to-Image workflow.

```csharp
public static IXrAiImageToImage LoadImageToImage(string name)
```

**Parameters:**
- `name` (string): The name of the Image-to-Image provider to load (e.g., "OpenAI")

**Returns:**
- `IXrAiImageToImage`: An instance of the requested Image-to-Image workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadTextToImage

Creates an instance of a Text-to-Image workflow.

```csharp
public static IXrAiTextToImage LoadTextToImage(string name)
```

**Parameters:**
- `name` (string): The name of the Text-to-Image provider to load (e.g., "OpenAI")

**Returns:**
- `IXrAiTextToImage`: An instance of the requested Text-to-Image workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadSpeechToText

Creates an instance of a Speech-to-Text workflow.

```csharp
public static IXrAiSpeechToText LoadSpeechToText(string name)
```

**Parameters:**
- `name` (string): The name of the Speech-to-Text provider to load (e.g., "OpenAI")

**Returns:**
- `IXrAiSpeechToText`: An instance of the requested Speech-to-Text workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported

### LoadTextToSpeech

Creates an instance of a Text-to-Speech workflow.

```csharp
public static IXrAiTextToSpeech LoadTextToSpeech(string name)
```

**Parameters:**
- `name` (string): The name of the Text-to-Speech provider to load
- `properties` (Dictionary<string, string>, optional): Configuration properties for the model

**Returns:**
- `IXrAiTextToSpeech`: An instance of the requested Text-to-Speech workflow

**Throws:**
- `NotSupportedException`: When the specified provider is not supported
