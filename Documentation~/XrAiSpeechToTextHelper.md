# XrAiSpeechToTextHelper

The `XrAiSpeechToTextHelper` class is a MonoBehaviour component that simplifies audio recording and conversion for speech-to-text operations. It handles microphone input, audio encoding, and provides callbacks for processing recorded audio data.

## Class Declaration

```csharp
public class XrAiSpeechToTextHelper : MonoBehaviour
```

## Methods

### StartRecording

Begins recording audio from the specified microphone device.

```csharp
public void StartRecording(string device, Action<byte[]> onRecordingComplete = null, int recordingMax = 5)
```

**Parameters:**
- `device` (string): The microphone device name to record from
- `onRecordingComplete` (Action<byte[]>, optional): Callback invoked when recording completes with audio data
- `recordingMax` (int, optional): Maximum recording duration in seconds (default: 5)

### StopRecording

Manually stops the current recording session.

```csharp
public void StopRecording()
```

## Usage Example

```csharp
public class VoiceCommandSystem : MonoBehaviour
{
    private XrAiSpeechToTextHelper recorder;
    private IXrAiSpeechToText speechToText;
    
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
        // Get the default microphone
        string defaultMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        
        if (defaultMic != null)
        {
            recorder.StartRecording(defaultMic, OnAudioRecorded, 10);
            Debug.Log("Started recording...");
        }
        else
        {
            Debug.LogError("No microphone devices found");
        }
    }
    
    private async void OnAudioRecorded(byte[] audioData)
    {
        Debug.Log($"Recording complete. Audio data size: {audioData.Length} bytes");
        
        // Process the audio with speech-to-text
        var result = await speechToText.Execute(audioData, new Dictionary<string, string>
        {
            { "model", "whisper-1" },
            { "language", "en" }
        });
        
        if (result.IsSuccess)
        {
            Debug.Log($"Transcribed: {result.Data}");
            ProcessVoiceCommand(result.Data);
        }
        else
        {
            Debug.LogError($"Speech recognition failed: {result.ErrorMessage}");
        }
    }
    
    private void ProcessVoiceCommand(string command)
    {
        // Handle the voice command
        Debug.Log($"Processing command: {command}");
    }
}
```

## Microphone Device Selection

```csharp
public class MicrophoneManager : MonoBehaviour
{
    private XrAiSpeechToTextHelper recorder;
    
    void Start()
    {
        recorder = GetComponent<XrAiSpeechToTextHelper>();
        ListAvailableMicrophones();
    }
    
    private void ListAvailableMicrophones()
    {
        Debug.Log($"Available microphones: {Microphone.devices.Length}");
        
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            string deviceName = Microphone.devices[i];
            Debug.Log($"{i}: {deviceName}");
            
            // Get device capabilities
            Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);
            Debug.Log($"  Frequency range: {minFreq}Hz - {maxFreq}Hz");
        }
    }
    
    public void StartRecordingWithDevice(int deviceIndex)
    {
        if (deviceIndex >= 0 && deviceIndex < Microphone.devices.Length)
        {
            string deviceName = Microphone.devices[deviceIndex];
            recorder.StartRecording(deviceName, OnRecordingComplete, 8);
        }
    }
}
```

## Real-time Voice Recognition

```csharp
public class ContinuousVoiceRecognition : MonoBehaviour
{
    private XrAiSpeechToTextHelper recorder;
    private IXrAiSpeechToText speechToText;
    private bool isListening = false;
    private Queue<string> recentCommands = new Queue<string>();
    
    public void StartContinuousListening()
    {
        if (!isListening)
        {
            isListening = true;
            StartNextRecording();
        }
    }
    
    public void StopContinuousListening()
    {
        isListening = false;
        recorder.StopRecording();
    }
    
    private void StartNextRecording()
    {
        if (isListening)
        {
            recorder.StartRecording(Microphone.devices[0], OnContinuousRecording, 3);
        }
    }
    
    private async void OnContinuousRecording(byte[] audioData)
    {
        if (isListening && audioData.Length > 1000) // Ignore very short recordings
        {
            var result = await speechToText.Execute(audioData);
            
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data.Trim()))
            {
                string command = result.Data.Trim();
                recentCommands.Enqueue(command);
                
                // Keep only recent commands
                if (recentCommands.Count > 10)
                {
                    recentCommands.Dequeue();
                }
                
                ProcessCommand(command);
            }
            
            // Continue listening
            StartNextRecording();
        }
    }
}
```

## Audio Quality Configuration

```csharp
public class AudioQualitySettings
{
    public static class Presets
    {
        public static (int sampleRate, int recordingLength) LowQuality => (22050, 3);
        public static (int sampleRate, int recordingLength) StandardQuality => (44100, 5);
        public static (int sampleRate, int recordingLength) HighQuality => (48000, 10);
    }
}

public void StartHighQualityRecording()
{
    var settings = AudioQualitySettings.Presets.HighQuality;
    recorder.StartRecording(
        Microphone.devices[0], 
        OnHighQualityRecording, 
        settings.recordingLength
    );
}
```

## Integration with UI

```csharp
public class VoiceInputUI : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private Image recordingIndicator;
    [SerializeField] private Text statusText;
    
    private XrAiSpeechToTextHelper recorder;
    private bool isRecording = false;
    
    void Start()
    {
        recorder = GetComponent<XrAiSpeechToTextHelper>();
        recordButton.onClick.AddListener(ToggleRecording);
    }
    
    private void ToggleRecording()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }
    
    private void StartRecording()
    {
        isRecording = true;
        recordingIndicator.color = Color.red;
        statusText.text = "Recording...";
        
        recorder.StartRecording(Microphone.devices[0], OnRecordingComplete, 10);
    }
    
    private void StopRecording()
    {
        isRecording = false;
        recordingIndicator.color = Color.gray;
        statusText.text = "Processing...";
        
        recorder.StopRecording();
    }
    
    private async void OnRecordingComplete(byte[] audioData)
    {
        statusText.text = "Converting to text...";
        
        // Process audio data
        // Update UI with results
        
        isRecording = false;
        recordingIndicator.color = Color.gray;
        statusText.text = "Ready";
    }
}
```

## Audio Data Processing

The helper automatically converts Unity AudioClip to WAV format:

### WAV Format Specifications
- **Sample Rate**: 44100 Hz
- **Channels**: Mono
- **Bit Depth**: 16-bit
- **Format**: PCM WAV with proper header

### Manual Audio Conversion
```csharp
// If you need to process AudioClip manually
private byte[] ConvertAudioClipToWav(AudioClip clip)
{
    // The helper handles this internally, but you can access
    // the conversion logic if needed for custom processing
    return XrAiSpeechToTextHelper.Convert(clip); // This method would need to be made public
}
```

## Performance Considerations

- Recording automatically stops after the specified duration
- Audio data is converted to compressed WAV format
- Memory usage scales with recording duration
- Consider implementing audio compression for longer recordings

## Error Handling

```csharp
private void OnRecordingComplete(byte[] audioData)
{
    if (audioData == null || audioData.Length == 0)
    {
        Debug.LogError("Recording failed or produced no audio data");
        return;
    }
    
    if (audioData.Length < 1000) // Very short recording
    {
        Debug.LogWarning("Recording may be too short for accurate transcription");
    }
    
    // Process audio data...
}
```

## Implementation Notes

- Inherits from MonoBehaviour for Unity integration
- Automatically handles microphone permissions
- Converts audio to standard WAV format for compatibility
- Provides automatic recording timeout functionality
- Supports callback-based asynchronous operation
- Manages Unity AudioClip lifecycle automatically
