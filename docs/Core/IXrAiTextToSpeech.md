# IXrAiTextToSpeech

The `IXrAiTextToSpeech` interface defines the contract for AI models that generate speech audio from text input.
This interface should be implemented by providers for text-to-speech conversion.

## Interface Declaration

```csharp
public interface IXrAiTextToSpeech
```

## Methods

### Initialize

Initializes the workflow with provider-specific options asynchronously.

```csharp
public Task Initialize(Dictionary<string, string> globalOptions);
```

**Parameters:**
- `globalOptions` (Dictionary<string, string>): Model-specific global options

### Execute

Processes text input and generates speech audio asynchronously.

```csharp
public Task Execute(string text, Dictionary<string, string> workflowOptions, Action<XrAiResult<AudioClip>> callback);
```

**Parameters:**
- `text` (string): The text to convert to speech
- `workflowOptions` (Dictionary<string, string>): Model-specific workflow options and parameters
- `callback` (Action<XrAiResult&lt;AudioClip>>): The callback when inference is complete
