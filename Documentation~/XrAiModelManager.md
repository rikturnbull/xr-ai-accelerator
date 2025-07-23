# XrAiModelManager

The `XrAiModelManager` class is a MonoBehaviour component that manages AI model configurations, API keys, and workflow-specific properties. It provides a centralized system for storing and retrieving model settings across different AI providers and workflows.

## Class Declaration

```csharp
public class XrAiModelManager : MonoBehaviour
```

## Data Structures

### XrAiProperty

Represents a key-value pair for configuration properties.

```csharp
[System.Serializable]
public class XrAiProperty
{
    public string key;
    public string value;
}
```

### XrAiSection

Groups related properties under a section name.

```csharp
[System.Serializable]
public class XrAiSection
{
    public string sectionName;
    public List<XrAiProperty> properties = new List<XrAiProperty>();
}
```

### XrAiModelData

Container for all configuration sections.

```csharp
[System.Serializable]
public class XrAiModelData
{
    public List<XrAiSection> sections = new List<XrAiSection>();
}
```

## Constants

### File Paths
```csharp
private const string CONFIG_FILE_PATH = "XrAiModelConfig";
private const string API_KEYS_FILE_PATH = "XrAiApiKeys";
```

### Workflow Types
```csharp
public const string WORKFLOW_IMAGE_TO_IMAGE = "ImageToImage";
public const string WORKFLOW_IMAGE_TO_TEXT = "ImageToText";
public const string WORKFLOW_TEXT_TO_IMAGE = "TextToImage";
public const string WORKFLOW_SPEECH_TO_TEXT = "SpeechToText";
public const string WORKFLOW_IMAGE_TO_3D = "ImageTo3d";
public const string WORKFLOW_OBJECT_DETECTOR = "ObjectDetector";
public const string WORKFLOW_TEXT_TO_SPEECH = "TextToSpeech";
```

## Properties

### ModelData

Gets the main configuration data, loading from file if necessary.

```csharp
public XrAiModelData ModelData { get; }
```

### ApiKeysData

Gets the API keys data, separated from main configuration for security.

```csharp
public XrAiModelData ApiKeysData { get; }
```

## Methods

### LoadFromFile

Loads configuration data from Resources folder files.

```csharp
public void LoadFromFile()
```

### SaveToFile

Saves configuration data to Resources folder files.

```csharp
public void SaveToFile()
```

### GetGlobalProperties

Retrieves all global properties for a section, including API keys.

```csharp
public Dictionary<string, string> GetGlobalProperties(string sectionName)
```

### GetWorkflowProperties

Retrieves workflow-specific properties for a section.

```csharp
public Dictionary<string, string> GetWorkflowProperties(string sectionName, string workflowName)
```

### GetGlobalProperty

Retrieves a single global property value.

```csharp
public string GetGlobalProperty(string sectionName, string key, string defaultValue = "")
```

### GetWorkflowProperty

Retrieves a single workflow-specific property value.

```csharp
public string GetWorkflowProperty(string sectionName, string workflowName, string key, string defaultValue = "")
```

## Usage Examples

### Basic Setup

```csharp
public class AIConfigurationManager : MonoBehaviour
{
    private XrAiModelManager modelManager;
    
    void Start()
    {
        modelManager = GetComponent<XrAiModelManager>();
        ConfigureAIModels();
    }
    
    private void ConfigureAIModels()
    {
        // Get OpenAI configuration
        var openAIConfig = modelManager.GetGlobalProperties("OpenAI");
        string apiKey = openAIConfig.GetValueOrDefault("apiKey", "");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("OpenAI API key not configured");
        }
        
        // Get workflow-specific settings
        string imageToTextModel = modelManager.GetWorkflowProperty(
            "OpenAI", 
            XrAiModelManager.WORKFLOW_IMAGE_TO_TEXT, 
            "model", 
            "gpt-4-vision-preview"
        );
    }
}
```

### Dynamic Configuration

```csharp
public class RuntimeConfiguration : MonoBehaviour
{
    private XrAiModelManager modelManager;
    
    public void SetupProviderConfiguration(string provider, Dictionary<string, string> config)
    {
        modelManager = GetComponent<XrAiModelManager>();
        
        // Update configuration at runtime
        foreach (var setting in config)
        {
            SetGlobalProperty(provider, setting.Key, setting.Value);
        }
        
        // Save changes
        modelManager.SaveToFile();
    }
    
    private void SetGlobalProperty(string section, string key, string value)
    {
        // This would require extending the manager to support runtime updates
        // or directly modifying the ModelData structure
    }
}
```

### Workflow-Specific Configuration

```csharp
public class WorkflowConfigurationExample : MonoBehaviour
{
    private XrAiModelManager modelManager;
    
    public async Task<string> ProcessImageWithGroq(Texture2D image)
    {
        modelManager = GetComponent<XrAiModelManager>();
        
        // Get Groq configuration for Image-to-Text workflow
        var groqConfig = modelManager.GetGlobalProperties("Groq");
        var workflowConfig = modelManager.GetWorkflowProperties("Groq", XrAiModelManager.WORKFLOW_IMAGE_TO_TEXT);
        
        // Load the model with configuration
        var imageToText = XrAiFactory.LoadImageToText("Groq", groqConfig);
        
        // Execute with workflow-specific options
        byte[] imageData = XrAiImageHelper.EncodeTexture(image, "image/jpeg");
        var result = await imageToText.Execute(imageData, "image/jpeg", workflowConfig);
        
        return result.IsSuccess ? result.Data : null;
    }
}
```

## Configuration File Structure

### Main Configuration (XrAiModelConfig.txt)

```json
{
  "sections": [
    {
      "sectionName": "Groq",
      "properties": []
    },
    {
      "sectionName": "Groq.ImageToText",
      "properties": [
        {
          "key": "model",
          "value": "llama-vision-free"
        },
        {
          "key": "prompt",
          "value": "Describe what you see in this image."
        }
      ]
    },
    {
      "sectionName": "OpenAI.SpeechToText",
      "properties": [
        {
          "key": "model",
          "value": "whisper-1"
        }
      ]
    }
  ]
}
```

### API Keys Configuration (XrAiApiKeys.txt)

```json
{
  "sections": [
    {
      "sectionName": "Groq",
      "properties": [
        {
          "key": "apiKey",
          "value": "your-groq-api-key"
        }
      ]
    },
    {
      "sectionName": "OpenAI",
      "properties": [
        {
          "key": "apiKey",
          "value": "your-openai-api-key"
        }
      ]
    }
  ]
}
```

## Predefined Sections and Properties

The manager automatically initializes configuration for known providers:

### Supported Providers
- **Nvidia**: Image-to-Text
- **OpenAI**: Speech-to-Text, Text-to-Image, Image-to-Image, Text-to-Speech
- **StabilityAi**: Image-to-3D
- **Google**: Image-to-Text, Object Detection
- **Groq**: Image-to-Text
- **Roboflow**: Object Detection
- **RoboflowLocal**: Object Detection

### Auto-Initialized Properties

Each provider section automatically includes:
- API key property (stored separately)
- Workflow-specific properties based on capabilities

## Security Considerations

### API Key Separation
- API keys are stored in a separate file (`XrAiApiKeys.txt`)
- Main configuration can be shared without exposing sensitive data
- Consider encrypting the API keys file for production

### File Location
- Configuration files are stored in the `Resources` folder
- Files are included in builds but accessible at runtime
- Consider using persistent data path for user-specific configurations

## Integration with AI Factory

```csharp
public class FactoryIntegration : MonoBehaviour
{
    private XrAiModelManager modelManager;
    
    public IXrAiImageToText CreateConfiguredImageToText(string provider)
    {
        modelManager = GetComponent<XrAiModelManager>();
        
        // Get provider configuration
        var config = modelManager.GetGlobalProperties(provider);
        
        // Load model with configuration
        return XrAiFactory.LoadImageToText(provider, config);
    }
    
    public async Task<XrAiResult<string>> ExecuteWithWorkflowConfig(
        IXrAiImageToText model, 
        byte[] imageData, 
        string provider)
    {
        // Get workflow-specific options
        var workflowOptions = modelManager.GetWorkflowProperties(
            provider, 
            XrAiModelManager.WORKFLOW_IMAGE_TO_TEXT
        );
        
        return await model.Execute(imageData, "image/jpeg", workflowOptions);
    }
}
```

## Editor Integration

The manager supports Unity Editor integration:

```csharp
#if UNITY_EDITOR
// Auto-refresh assets when saving configuration
UnityEditor.AssetDatabase.Refresh();
#endif
```

## Best Practices

1. **Separate Concerns**: Keep API keys separate from workflow configuration
2. **Default Values**: Always provide sensible defaults for configuration properties
3. **Validation**: Validate configuration before using with AI models
4. **Environment-Specific**: Consider different configurations for development/production
5. **Documentation**: Document all configuration properties and their purposes

## Implementation Notes

- Inherits from MonoBehaviour for Unity integration
- Uses JSON serialization for configuration persistence
- Automatically initializes predefined provider configurations
- Separates API keys from general configuration for security
- Supports both global and workflow-specific property management
- Integrates with Unity's Resources system for easy asset management
