# Unity XR AI Accelerator Library

This is an API for various AI Model pipelines for use in Unity XR prototypes. Plugin providers can implement this API to expose their models via a standard interface.

The intention is to provide a simplified API for building XR prototypes in Unity.

This initial proof of concept has a selection of AI Model pipelines:

* ImageToText
* ImageTo3d
* ObjectDetection
* ImageToImage
* TextToImage
* SpeechToText
* TextToSpeech

Note that these are one-shot model pipelines - not conversational LLMs.

The following plugins are provided:

* ImageToText
    * Groq
    * Google
    * Nvidia
* ImageTo3d
    * StabilityAI
* ObjectDetection
    * Google
    * YOLO
    * Roboflow
    * RoboflowLocal
* ImageToImage
    * OpenAI
* TextToImage
    * OpenAI
* SpeechToText
    * OpenAI
* TextToSpeech
    * OpenAI

## Installation

1. **Add Scoped Registries** - In Unity Editor:
   - Go to **Edit > Project Settings**
   - Select **Package Manager** from the left panel
   - Under **Scoped Registries**, click the **+** button to add new registries
   - Add the first registry:
     - **Name**: `package.openupm.com (nuget)`
     - **URL**: `https://package.openupm.com`
     - **Scope(s)**: `com.github-glitchenzo.nugetforunity`
   - Click **+** again and add the second registry:
     - **Name**: `package.openupm.com (openai)`
     - **URL**: `https://package.openupm.com`
     - **Scope(s)**: `com.openai` and `com.utilities` (add each scope separately)
   - Click **Save**

2. **Install from Git URL** - In Unity Editor:
   - Go to **Window > Package Manager**
   - Click the **+** button in the top-left corner
   - Select **Add package from git URL...**
   - Enter: `https://github.com/siccity/gltfutility.git`
   - Click **Add**

3. **Copy Configuration File** - **Important**:
   - Copy `XrAiModelConfig.txt` from the package's `Resources` folder
   - Paste it into your project's `Assets/Resources` folder
   - Create the `Resources` folder if it doesn't exist
   - This file is required for default model configuration

## Security Considerations

**Important**: If you store API keys in a `Resources/XrAiApiKeys.txt` file, you must add this file to your `.gitignore` to prevent accidentally committing sensitive API keys to version control.

Add the following line to your `.gitignore` file:
```
Assets/Resources/XrAiApiKeys.txt
```

This ensures that your API keys remain private and are not shared when you commit your project to a repository.

## Example

The `XrAiFactory` class manages plugins and fetches models by name.

For example, to fetch the Groq ImageToText model, load it by name '`Groq`' and pass your `apiKey` inside the `options`:

```
IXrAiImageToText imageToText = 
  XrAiFactory.LoadImageToText("Groq", new 
    System.Collections.Generic.Dictionary<string, string> {
        { "apiKey", _apiKey }
    }
);
```

Now execute the pipeline, passing in any model specific options (Groq requires a model and a prompt):

```
_task = imageToText.Execute(_rawImage.texture as Texture2D,
    new System.Collections.Generic.Dictionary<string, string>
    {
        { "model", "llama-4-scout-17b-16e-instruct" },
        { "prompt", "What's in this image?" }
    }
);
```

This is an asynchronous call that runs in the background. Check its progress in the `Update()` method:

```
if (_task != null)
{
    if (!_task.IsCompleted) return;

    if (_task.IsFaulted)
    {
        Debug.LogError("Task failed: " + _task.Exception);
        return;
    }

    XrAiTaskResult result = _task.Result;
    Debug.Log($"Answer: {result.StringResult}");
}
```

The `XrAiTaskResult` class supports multiple pipeline outputs - such as `string` for text results and `byte[]` for image results.

**To switch to another model plugin - change the name and the model specific options. The rest remains the same.**
