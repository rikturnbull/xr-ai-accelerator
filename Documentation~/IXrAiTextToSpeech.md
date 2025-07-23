# IXrAiTextToSpeech

The `IXrAiTextToSpeech` interface defines the contract for AI models that convert text into spoken audio. This interface generates audio clips from text input.

## Interface Declaration

```csharp
public interface IXrAiTextToSpeech
```

## Methods

### Execute

Converts text to speech and generates an audio clip asynchronously.

```csharp
public Task<XrAiResult<AudioClip>> Execute(string text, Dictionary<string, string> options = null)
```

**Parameters:**
- `text` (string): The text to convert to speech
- `options` (Dictionary<string, string>, optional): Model-specific options and parameters

**Returns:**
- `Task<XrAiResult<AudioClip>>`: A task that resolves to a result containing the generated audio clip

## Usage Example

```csharp
// Load the model
IXrAiTextToSpeech textToSpeech = XrAiFactory.LoadTextToSpeech("OpenAI", new Dictionary<string, string>
{
    { "apiKey", "your-openai-api-key" }
});

// Convert text to speech
string textToSpeak = "Hello, welcome to the XR AI Library!";
var result = await textToSpeech.Execute(textToSpeak, new Dictionary<string, string>
{
    { "voice", "alloy" },
    { "model", "tts-1" },
    { "speed", "1.0" }
});

// Handle the result
if (result.IsSuccess)
{
    AudioClip audioClip = result.Data;
    
    // Play the audio clip
    AudioSource audioSource = GetComponent<AudioSource>();
    audioSource.clip = audioClip;
    audioSource.Play();
}
else
{
    Debug.LogError($"Text-to-speech failed: {result.ErrorMessage}");
}
```

## Model-Specific Options

Different providers support different configuration options:

### OpenAI
- `voice`: Voice selection (e.g., "alloy", "echo", "fable", "onyx", "nova", "shimmer")
- `model`: TTS model to use (e.g., "tts-1", "tts-1-hd")
- `speed`: Speech speed (0.25 to 4.0, default 1.0)
- `response_format`: Audio format (e.g., "mp3", "opus", "aac", "flac")

## Audio Integration

The returned `AudioClip` can be used with Unity's audio system:

```csharp
// Play immediately
AudioSource.PlayClipAtPoint(audioClip, transform.position);

// Assign to AudioSource component
audioSource.clip = audioClip;
audioSource.volume = 0.8f;
audioSource.pitch = 1.0f;
audioSource.Play();

// Use with 3D spatial audio
audioSource.spatialBlend = 1.0f; // Full 3D
audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
```

## Quality Considerations

- Higher quality models may have longer processing times
- Consider caching generated audio for frequently used text
- Different voices may be more suitable for different content types
- Audio quality affects file size and memory usage

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<AudioClip>` for consistent error handling
- Generated audio clips are ready for immediate playback in Unity
- Text length may affect generation time and API costs
- Some providers may have character limits per request

## Error Handling

```csharp
if (!result.IsSuccess)
{
    Debug.LogError($"TTS generation failed: {result.ErrorMessage}");
    // Handle specific error cases:
    // - Invalid API key
    // - Text too long
    // - Unsupported voice
    // - Network connectivity issues
    // - Service rate limits
}
```

## Best Practices

- Keep text segments reasonably short for better responsiveness
- Cache commonly used audio clips to reduce API calls
- Consider using lower quality settings for background audio
- Implement fallback text display when audio generation fails
- Test different voices to find the best fit for your application
