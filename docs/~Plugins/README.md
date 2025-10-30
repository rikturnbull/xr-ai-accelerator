# XR AI Library - Runtime Provider Documentation

This documentation covers the AI provider implementations in the XR AI Library Runtime/Plugins module. These components provide concrete implementations of the core interfaces for various AI service providers and local inference engines.

## Providers

The following providers offer cloud-based or local inference AI capabilities:

### Image-to-Text Providers

#### [GoogleImageToText](GoogleImageToText.md)
Google Gemini Vision API implementation for converting images to descriptive text. Uses Google's multimodal AI for accurate image analysis and description generation.

#### [GroqImageToText](GroqImageToText.md)
Groq uses Meta's LLaMA vision model implementation for fast image-to-text conversion. Supports multiple LLaMA-4 vision models with configurable inference parameters.

### Object Detection Providers

#### [GoogleObjectDetector](GoogleObjectDetector.md)
Google Gemini Vision API implementation for zero shot object detection. Provides structured object detection with bounding boxes and confidence scores.

#### [RoboflowObjectDetector](RoboflowObjectDetector.md)
Roboflow API implementation supporting both serverless workflows and direct model inference. Includes keypoint detection and supports custom trained models.

#### [YoloObjectDetector](YoloObjectDetector.md)
Local YOLO (You Only Look Once) implementation using Unity Sentis. Provides fast, offline object detection with no API requirements or data privacy concerns.

### Text-to-Image Providers

#### [OpenAITextToImage](OpenAITextToImage.md)
OpenAI DALL-E 3 implementation for generating high-quality images from text prompts. Creates detailed visual content based on natural language descriptions.

### Text-to-Speech Providers

#### [OpenAITextToSpeech](OpenAITextToSpeech.md)
OpenAI TTS-1 implementation for converting text to natural-sounding speech. Supports multiple voice options and configurable speech speed.

### Speech-to-Text Providers

#### [OpenAISpeechToText](OpenAISpeechToText.md)
OpenAI Whisper implementation for accurate speech transcription. Supports multiple languages and provides context-aware transcription capabilities.

### Image-to-Image Providers

#### [OpenAIImageToImage](OpenAIImageToImage.md)
OpenAI image editing implementation for modifying and completing images. Uses AI to enhance, edit, or complete partial images.

### Image-to-3D Providers

#### [StabilityAiImageTo3d](StabilityAiImageTo3d.md)
Stability AI Stable Fast 3D implementation for converting 2D images to 3D models. Generates textured 3D assets suitable for XR applications.

## Quick Start Guide

### Provider Selection

Choose providers based on your requirements:

```csharp
// For fast inference
var groqImageToText = XrAiFactory.LoadImageToText("Groq");

// For comprehensive features
var googleObjectDetector = XrAiFactory.LoadObjectDetector("Google");

// For local/offline operation
var yoloObjectDetector = XrAiFactory.LoadObjectDetector("Yolo");

// For creative content generation
var openAITextToImage = XrAiFactory.LoadTextToImage("OpenAI");
```

### Common Usage Pattern

Most providers follow this configuration pattern:

```csharp
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-api-key",
    ["model"] = "specific-model-name",  // Optional for most
    ["prompt"] = "custom-prompt"        // Optional for most
};

await provider.Initialize(options);
await provider.Execute(input, options, result => {
    if (result.IsSuccess) {
        // Process result.Data
    } else {
        Debug.LogError(result.ErrorMessage);
    }
});
```