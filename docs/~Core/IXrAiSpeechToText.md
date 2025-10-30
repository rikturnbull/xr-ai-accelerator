# IXrAiSpeechToText

The `IXrAiSpeechToText` interface defines the contract for AI models that convert speech audio into text.
This interface should be implemented by providers for speech-to-text conversion.

## Interface Declaration

```csharp
public interface IXrAiSpeechToText
```

## Methods

### Initialize

Initializes the workflow with provider-specific options asynchronously.

```csharp
public Task Initialize(Dictionary<string, string> options = null);
```

**Parameters:**
- `options` (Dictionary<string, string>): Model-specific options

### Execute

Processes audio data and generates text transcription asynchronously.

```csharp
public Task Execute(byte[] audioData, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
```

**Parameters:**
- `audioData` (byte[]): The audio data to transcribe
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;string>>): The callback when inference is complete
