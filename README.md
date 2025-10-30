# Unity XR AI Accelerator Library

Documentation https://rikturnbull.github.io/xr-ai-accelerator/.

This is an API for various AI Model pipelines for use in Unity XR prototypes. Plugin providers can implement this API to expose their models via a standard interface.

The intention is to provide a simplified API for testing AI providers and models for XR projects in Unity.

This initial proof of concept has a selection of AI Model pipelines:

* ImageTo3d
* ImageToImage
* ImageToText
* ObjectDetection
* SpeechToText
* TextToImage
* TextToSpeech

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
   - Click **+** to add a registry:
     - **Name**: `package.openupm.com (openai)`
     - **URL**: `https://package.openupm.com`
     - **Scope(s)**: `com.openai` and `com.utilities` (add each scope separately)
   - Click **Save**

2. **Add Package via Git URL** - In Unity Editor:
> **⚠️ Warning:** Ensure to use the version tag in the GIT url - this is work in progress and there may be future breaking changes.

   - Go to **Window > Package Manager**
   - Click the **+** button in the top-left corner
   - Select **Add package from git URL...**
   - Enter: `https://github.com/rikturnbull/xr-ai-accelerator.git#0.0.1`
   - Click **Add**

## Secrets Manager

You can use `XrAiSecretsManager` to store secrets:

1. Navigate to your `Assets/Resources` folder.
2. Right-click `Create -> XrAiAccelerator -> Secrets Manager`.
3. Click on the `XrAiSecretsManager` object and view the inspector.

**Add** a secret: enter the secret and click `Add`.

**Delete** a secret: click the `X` next to the secret.

**Edit** a secret: delete the secret and recreate it.

To fetch a secret in code:

```csharp
// Load secrets manager
XrAiSecretsManager secretsManager = XrAiSecretsManager.GetSecretsManager()

// Fetch a secret
string apiKey = secretsManager.GetSecret("apiKey");
```

> **⚠️ Important:** : XrAiSecretsManager stores secrets in `Resources/XrAiSecretsManager.txt` file, you must add this file to your `.gitignore` to prevent accidentally committing sensitive API keys to version control.

Add the following line to your `.gitignore` file:
```
Assets/Resources/XrAiSecretsManager.txt
```

This ensures that your secrets remain private and are not shared when you commit your project to a repository.

## Usage Example

```csharp
// Load a provider implementation
private IXrAiTextToImage textToImage;
private XrAiSecretsManager secretsManager;

public void LoadInference()
{
    secretsManager = XrAiSecretsManager.GetSecretsManager();
    textToImage = XrAiFactory.LoadTextToImage("OpenAI");
}

// Initialize inference
public void InitializeInference()
{
    StartCoroutine(InitializeInferenceCoroutine());
}

// Initialize inference coroutine
private IEnumerator InitializeInferenceCoroutine()
{
    Task task = textToImage.Initialize(new Dictionary<string, string>
    {
        // Model specific initialization options
        { "apiKey", secretsManager.GetSecret("OpenAI") }
    });
    yield return new WaitUntil(() => task.IsCompleted);
}

// Run inference
public void RunInference()
{
    StartCoroutine(RunInferenceCoroutine());
}

// Run inference coroutine
private IEnumerator RunInferenceCoroutine()
{
    Task task = textToImage.Execute(
        // Model specific execution options
        new Dictionary<string, string>
        {
            { "prompt", "A futuristic city in VR with flying cars" }
        },
        OnResult
    );
    yield return new WaitUntil(() => task.IsCompleted);
}

// Process the result
private void OnResult(XrAiResult<Texture2D> result)
{
    if (result.IsSuccess)
    {
        Texture2D generatedImage = result.Data;
        // Use texture
        ...
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
```

To switch to another provider plugin - change the name and the provider specific options. The rest remains the same.
