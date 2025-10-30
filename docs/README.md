# XR AI Accelerator Library - Documentation

This documentation is organized into two main sections covering the Core framework and AI provider Plugins.

## Overview

The XR AI Accelerator Library provides a simplified, unified API for integrating various AI capabilities into Unity XR applications. The library follows a modular architecture with standardized interfaces and a factory pattern for easy provider swapping and configuration management.

### Supported AI Workflows

- **Image-to-Text**: Generate descriptive text from images
- **Image-to-3D**: Convert 2D images to 3D models
- **Object Detection**: Detect and locate objects in images
- **Text-to-Image**: Generate images from text descriptions
- **Text-to-Speech**: Convert text to natural speech audio
- **Speech-to-Text**: Transcribe audio to text
- **Image-to-Image**: Transform and edit images with AI

### Key Features

- **Unified Interface**: Consistent API across all AI providers
- **Factory Pattern**: Easy provider instantiation and management
- **Async Operations**: Non-blocking AI operations for smooth XR experiences
- **Result Pattern**: Consistent error handling across all operations
- **Local & Cloud**: Support for both cloud-based APIs and local inference
- **Configuration Management**: Centralized API key and parameter management

## Documentation

### [Core Framework](~Core/README.md)

The core framework provides the foundational interfaces, utilities, and management classes that enable the AI provider ecosystem.

#### Core Interfaces
- **[IXrAiImageToText](~Core/IXrAiImageToText.md)** - Image analysis and description
- **[IXrAImageTo3d](~Core/IXrAImageTo3d.md)** - 2D to 3D model conversion
- **[IXrAiObjectDetector](~Core/IXrAiObjectDetector.md)** - Object detection and localization
- **[IXrAiTextToSpeech](~Core/IXrAiTextToSpeech.md)** - Text to audio conversion
- **[IXrAiSpeechToText](~Core/IXrAiSpeechToText.md)** - Audio transcription
- **[IXrAiTextToImage](~Core/IXrAiTextToImage.md)** - Text-based image generation
- **[IXrAiImageToImage](~Core/IXrAiImageToImage.md)** - Image transformation and editing

#### Core Classes
- **[XrAiFactory](~Core/XrAiFactory.md)** - Central factory for AI provider instantiation
- **[XrAiResult](~Core/XrAiResult.md)** - Unified result type for consistent error handling
- **[XrAiBoundingBox](~Core/XrAiBoundingBox.md)** - Object detection result structure
- **[XrAiImageHelper](~Core/XrAiImageHelper.md)** - Texture encoding utilities
- **[XrAiObjectDetectorHelper](~Core/XrAiObjectDetectorHelper.md)** - Visualization helpers
- **[XrAiSpeechToTextHelper](~Core/XrAiSpeechToTextHelper.md)** - Audio recording utilities

### [Plugin Classes](~Plugins/README.md)

The example AI provider plugins implement the core interfaces with specific cloud services and local inference engines.

#### Cloud-Based Providers

**Google Services**
- **[GoogleImageToText](~Plugins/GoogleImageToText.md)** - Gemini Vision for image analysis
- **[GoogleObjectDetector](~Plugins/GoogleObjectDetector.md)** - Gemini Vision for object detection

**OpenAI Services**
- **[OpenAITextToImage](~Plugins/OpenAITextToImage.md)** - DALL-E 3 image generation
- **[OpenAITextToSpeech](~Plugins/OpenAITextToSpeech.md)** - TTS-1 speech synthesis
- **[OpenAISpeechToText](~Plugins/OpenAISpeechToText.md)** - Whisper speech recognition
- **[OpenAIImageToImage](~Plugins/OpenAIImageToImage.md)** - Image editing and completion

**Specialized Services**
- **[GroqImageToText](~Plugins/GroqImageToText.md)** - Fast LLaMA vision inference
- **[RoboflowObjectDetector](~Plugins/RoboflowObjectDetector.md)** - Custom computer vision models
- **[StabilityAiImageTo3d](~Plugins/StabilityAiImageTo3d.md)** - 2D to 3D model generation

#### Local Inference
- **[YoloObjectDetector](~Plugins/YoloObjectDetector.md)** - Local object detection with Unity Sentis

## Basic Usage Pattern

```csharp
// Load a provider using the factory
var imageToText = XrAiFactory.LoadImageToText("Groq");

// Configure with API keys and options
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-api-key",
};

// Initialize the provider
await imageToText.Initialize(options);

// Execute AI operation
await imageToText.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        Debug.Log($"Result: {result.Data}");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```
