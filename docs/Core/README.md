# XR AI Library - Runtime Core API Documentation

This documentation covers the core API classes and interfaces in the XR AI Library Runtime/Core module. These components provide the foundational interfaces and utilities for AI model integration in Unity XR applications.

## Core Interfaces

The following interfaces define the contracts for different AI model pipelines:

### [IXrAiImageToText](IXrAiImageToText.md)
Interface for AI models that generate text descriptions from images. Supports providers like Groq, Google, and Nvidia.

### [IXrAImageTo3d](IXrAImageTo3d.md)
Interface for AI models that generate 3D models from 2D images. Currently supports StabilityAI.

### [IXrAiObjectDetector](IXrAiObjectDetector.md)
Interface for AI models that detect and locate objects within images. Supports Google, YOLO, and Roboflow providers.

### [IXrAiTextToSpeech](IXrAiTextToSpeech.md)
Interface for AI models that convert text into spoken audio. Generates Unity AudioClip objects from text input.

### [IXrAiSpeechToText](IXrAiSpeechToText.md)
Interface for AI models that convert spoken audio into text. Processes audio data and returns transcribed text.

### [IXrAiTextToImage](IXrAiTextToImage.md)
Interface for AI models that generate images from text descriptions. Creates visual content based on textual prompts.

### [IXrAiImageToImage](IXrAiImageToImage.md)
Interface for AI models that transform or modify images based on text prompts. Enables image-to-image translation and style transfer.

## Core Classes

### [XrAiFactory](XrAiFactory.md)
Central factory class for creating instances of various AI model pipelines. Provides static methods to load different types of AI models by name.

### [XrAiResult](XrAiResult.md)
Unified result type for all AI operations. Encapsulates both success and error states using a result pattern for consistent error handling.

### [XrAiModelManager](XrAiModelManager.md)
MonoBehaviour component that manages AI model configurations, API keys, and workflow-specific properties. Provides centralized configuration management.

### [XrAiAssets](XrAiAssets.md)
MonoBehaviour component that manages AI model assets required by local inference models. Serves as a container for model files and configuration data.

## Data Structures

### [XrAiBoundingBox](XrAiBoundingBox.md)
Struct representing detected object location and classification information in object detection results. Provides spatial boundaries and identification of detected objects.

## Helper Classes

### [XrAiObjectDetectorHelper](XrAiObjectDetectorHelper.md)
Utility class for visualizing object detection results in Unity. Creates visual bounding boxes and labels to display detected objects on screen.

### [XrAiSpeechToTextHelper](XrAiSpeechToTextHelper.md)
MonoBehaviour component that simplifies audio recording and conversion for speech-to-text operations. Handles microphone input and audio encoding.

### [XrAiImageHelper](XrAiImageHelper.md)
Utility class for encoding Unity textures into standard image formats. Simplifies conversion of Texture2D objects into byte arrays for AI model processing.

## Quick Start Guide

### Basic Usage Pattern

1. **Load a Model**: Use `XrAiFactory` to load an AI model by provider name
2. **Prepare Input**: Convert Unity objects (textures, audio) to appropriate formats
3. **Execute Model**: Call the model's `Execute` method with input data and options
4. **Handle Results**: Check `XrAiResult.IsSuccess` and process the data or error

### Example Implementation

```csharp
// Load an image-to-text model
IXrAiImageToText imageToText = XrAiFactory.LoadImageToText("Groq", new Dictionary<string, string>
{
    { "apiKey", "your-api-key" }
});

// Convert texture to bytes
byte[] imageData = XrAiImageHelper.EncodeTexture(texture, "image/jpeg");

// Execute the model
var result = await imageToText.Execute(imageData, "image/jpeg", new Dictionary<string, string>
{
    { "model", "llama-vision-free" },
    { "prompt", "Describe this image" }
});

// Handle the result
if (result.IsSuccess)
{
    Debug.Log($"Description: {result.Data}");
}
else
{
    Debug.LogError($"Error: {result.ErrorMessage}");
}
```

## Configuration Management

The library supports centralized configuration through `XrAiModelManager`:

1. **Global Properties**: Provider-level settings like API keys
2. **Workflow Properties**: Specific settings for different AI workflows
3. **Separation of Concerns**: API keys stored separately from general configuration

## Architecture Overview

The core architecture follows these principles:

- **Interface-Based Design**: All AI models implement standard interfaces
- **Factory Pattern**: Centralized model creation and configuration
- **Result Pattern**: Consistent error handling across all operations
- **Helper Classes**: Utilities for common Unity integration tasks
- **Configuration Management**: Centralized settings and API key management

## Thread Safety

- All AI operations are asynchronous using `Task<XrAiResult<T>>`
- Helper classes support concurrent usage
- Configuration management is thread-safe for read operations

## Error Handling

The library uses a result pattern rather than exceptions for expected failure cases:

- Check `XrAiResult.IsSuccess` before accessing data
- `ErrorMessage` provides descriptive error information
- Exceptions are reserved for programming errors (null arguments, etc.)

## Extension Points

The modular design allows for easy extension:

- Implement interfaces to add new AI providers
- Extend helper classes for custom visualization or processing
- Add new workflow types to the configuration system
