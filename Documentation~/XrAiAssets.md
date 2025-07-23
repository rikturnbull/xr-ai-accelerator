# XrAiAssets

The `XrAiAssets` class is a MonoBehaviour component that manages AI model assets required by local inference models. This class serves as a container for model files, configuration data, and other resources needed by AI pipelines.

## Class Declaration

```csharp
public class XrAiAssets : MonoBehaviour
```

## Overview

The `XrAiAssets` component is designed to hold references to model assets that are used by local AI implementations, particularly for models that run inference directly within Unity using frameworks like Sentis.

## Usage Example

```csharp
// Attach XrAiAssets component to a GameObject
public class AIModelSetup : MonoBehaviour
{
    [SerializeField] private XrAiAssets aiAssets;
    
    void Start()
    {
        // Load object detector with assets
        IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector(
            "Yolo", 
            new Dictionary<string, string>
            {
                { "confidence", "0.5" }
            }, 
            aiAssets // Pass the assets component
        );
    }
}
```

## Integration with Factory

The `XrAiAssets` component is passed to factory methods that require local model assets:

```csharp
// Object detection with local models
IXrAiObjectDetector detector = XrAiFactory.LoadObjectDetector(
    name: "Yolo",
    options: detectionOptions,
    assets: aiAssetsComponent
);
```

## Asset Management

While the current implementation is minimal, the class is designed to be extended for specific asset management needs:

```csharp
// Example of how XrAiAssets might be extended
public class XrAiAssets : MonoBehaviour
{
    [Header("YOLO Model Assets")]
    [SerializeField] private TextAsset yoloModel;
    [SerializeField] private TextAsset yoloLabels;
    
    [Header("Custom Model Assets")]
    [SerializeField] private TextAsset[] customModels;
    
    // Properties to access assets
    public TextAsset YoloModel => yoloModel;
    public TextAsset YoloLabels => yoloLabels;
    public TextAsset[] CustomModels => customModels;
}
```

## Setup in Unity Editor

1. **Create Assets GameObject:**
   ```csharp
   // Create a GameObject in your scene
   GameObject assetsObject = new GameObject("AI Assets");
   XrAiAssets assets = assetsObject.AddComponent<XrAiAssets>();
   ```

2. **Configure in Inspector:**
   - Add the `XrAiAssets` component to a GameObject
   - Assign model files and configuration assets in the Inspector
   - Reference the component in your AI setup scripts

## Model Types

The assets component can be extended to support various model types:

### Local Inference Models
- **YOLO**: Object detection models for Sentis
- **Custom ONNX**: User-trained models
- **TensorFlow Lite**: Mobile-optimized models

### Configuration Files
- Model labels and class definitions
- Preprocessing parameters
- Post-processing configurations

## Best Practices

### Asset Organization
```csharp
[System.Serializable]
public class ModelAssetGroup
{
    public string modelName;
    public TextAsset modelFile;
    public TextAsset labelFile;
    public TextAsset configFile;
}

public class XrAiAssets : MonoBehaviour
{
    [SerializeField] private ModelAssetGroup[] modelGroups;
    
    public ModelAssetGroup GetModelGroup(string modelName)
    {
        return modelGroups.FirstOrDefault(g => g.modelName == modelName);
    }
}
```

### Resource Management
```csharp
public class XrAiAssets : MonoBehaviour
{
    private Dictionary<string, byte[]> loadedAssets = new Dictionary<string, byte[]>();
    
    public byte[] GetModelData(string modelName)
    {
        if (!loadedAssets.ContainsKey(modelName))
        {
            // Load and cache model data
            var modelAsset = GetModelAsset(modelName);
            loadedAssets[modelName] = modelAsset.bytes;
        }
        
        return loadedAssets[modelName];
    }
    
    private void OnDestroy()
    {
        // Clean up cached assets
        loadedAssets.Clear();
    }
}
```

## Validation

Implement validation to ensure required assets are properly configured:

```csharp
public class XrAiAssets : MonoBehaviour
{
    public bool ValidateAssets()
    {
        // Check if required assets are assigned
        bool isValid = true;
        
        if (yoloModel == null)
        {
            Debug.LogError("YOLO model asset is not assigned");
            isValid = false;
        }
        
        if (yoloLabels == null)
        {
            Debug.LogError("YOLO labels asset is not assigned");
            isValid = false;
        }
        
        return isValid;
    }
}
```

## Performance Considerations

- Assets should be loaded once and cached for reuse
- Large model files can impact memory usage
- Consider streaming assets for very large models
- Validate asset integrity before use

## Implementation Notes

- The component inherits from `MonoBehaviour` to leverage Unity's asset system
- Assets are typically assigned through the Unity Inspector
- The component serves as a bridge between Unity's asset system and AI model implementations
- Local model implementations can access assets through this component
- Consider implementing lazy loading for large assets to improve startup performance
