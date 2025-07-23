# IXrAiSpeechToText

The `IXrAiSpeechToText` interface defines the contract for AI models that convert spoken audio into text. This interface processes audio data and returns transcribed text.

## Interface Declaration

```csharp
public interface IXrAiSpeechToText
```

## Methods

### Execute

Converts audio data to text asynchronously.

```csharp
public Task<XrAiResult<string>> Execute(byte[] audioData, Dictionary<string, string> options = null)
```

**Parameters:**
- `audioData` (byte[]): The audio data as a byte array (typically WAV format)
- `options` (Dictionary<string, string>, optional): Model-specific options and parameters

**Returns:**
- `Task<XrAiResult<string>>`: A task that resolves to a result containing the transcribed text

## Usage Example

```csharp
// Load the model
IXrAiSpeechToText speechToText = XrAiFactory.LoadSpeechToText("OpenAI", new Dictionary<string, string>
{
    { "apiKey", "your-openai-api-key" }
});

// Record audio using helper
XrAiSpeechToTextHelper recorder = GetComponent<XrAiSpeechToTextHelper>();
recorder.StartRecording(Microphone.devices[0], OnRecordingComplete, 5);

// Process recorded audio
private async void OnRecordingComplete(byte[] audioData)
{
    var result = await speechToText.Execute(audioData, new Dictionary<string, string>
    {
        { "model", "whisper-1" },
        { "language", "en" }
    });

    if (result.IsSuccess)
    {
        string transcribedText = result.Data;
        Debug.Log($"Transcribed: {transcribedText}");
        
        // Use the transcribed text
        ProcessTranscription(transcribedText);
    }
    else
    {
        Debug.LogError($"Speech recognition failed: {result.ErrorMessage}");
    }
}
```

## Audio Recording Helper

Use the `XrAiSpeechToTextHelper` component to simplify audio recording:

```csharp
// Start recording from default microphone
speechToTextHelper.StartRecording(
    device: Microphone.devices[0],
    onRecordingComplete: ProcessAudioData,
    recordingMax: 10 // seconds
);

// Stop recording manually
speechToTextHelper.StopRecording();
```

## Model-Specific Options

Different providers support different configuration options:

### OpenAI (Whisper)
- `model`: Model to use (e.g., "whisper-1")
- `language`: Source language code (e.g., "en", "es", "fr")
- `prompt`: Optional text to guide the model's style
- `response_format`: Response format (e.g., "json", "text", "srt", "vtt")
- `temperature`: Sampling temperature (0 to 1)

## Audio Format Requirements

Most providers expect audio in specific formats:
- **Format**: WAV (recommended)
- **Sample Rate**: 44100 Hz (standard)
- **Channels**: Mono or Stereo
- **Bit Depth**: 16-bit

The `XrAiSpeechToTextHelper` automatically converts Unity `AudioClip` to the appropriate WAV format.

## Real-time Usage

For real-time speech recognition applications:

```csharp
public class RealtimeSpeechRecognition : MonoBehaviour
{
    private XrAiSpeechToTextHelper recorder;
    private IXrAiSpeechToText speechToText;
    private bool isListening = false;

    void Start()
    {
        recorder = GetComponent<XrAiSpeechToTextHelper>();
        speechToText = XrAiFactory.LoadSpeechToText("OpenAI", new Dictionary<string, string>
        {
            { "apiKey", "your-api-key" }
        });
    }

    public void StartListening()
    {
        if (!isListening)
        {
            isListening = true;
            recorder.StartRecording(Microphone.devices[0], OnSpeechRecorded, 3);
        }
    }

    private async void OnSpeechRecorded(byte[] audioData)
    {
        if (isListening)
        {
            var result = await speechToText.Execute(audioData);
            
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                ProcessCommand(result.Data);
            }
            
            // Continue listening
            recorder.StartRecording(Microphone.devices[0], OnSpeechRecorded, 3);
        }
    }
}
```

## Implementation Notes

- All operations are asynchronous and return a `Task`
- Results are wrapped in `XrAiResult<string>` for consistent error handling
- Audio data should be provided as byte arrays in WAV format
- Recording quality affects transcription accuracy
- Network connectivity is required for cloud-based services

## Error Handling

```csharp
if (!result.IsSuccess)
{
    Debug.LogError($"Speech transcription failed: {result.ErrorMessage}");
    // Handle specific error cases:
    // - Invalid API key
    // - Unsupported audio format
    // - Audio too short/long
    // - Network issues
    // - Service rate limits
}
```

## Best Practices

- Use appropriate recording duration (2-10 seconds for most use cases)
- Ensure good audio quality with minimal background noise
- Handle empty or unclear audio gracefully
- Implement timeout handling for long recordings
- Consider local fallback methods when network is unavailable
- Test with different accents and speaking speeds
