# XrAiSpeechToTextHelper

The `XrAiSpeechToTextHelper` class is a MonoBehaviour component that simplifies audio recording and conversion for speech-to-text operations. It handles microphone input, audio encoding, and provides callbacks for processing recorded audio data.

## Class Declaration

```csharp
public class XrAiSpeechToTextHelper : MonoBehaviour
```

## Methods

### StartRecording

Begins recording audio from the microphone device.

```csharp
public void StartRecording(Action<byte[]> onRecordingComplete = null, string device = null, string encoding = null, int? recordingMax = null, int? frequency = null)
```

**Parameters:**
- `onRecordingComplete` (Action<byte[]>, optional): Callback invoked when recording completes with audio data
- `device` (string, optional): The microphone device name to record from (auto-selects if null)
- `encoding` (string, optional): Audio encoding format - "wav" or "pcm" (default: "wav")
- `recordingMax` (int?, optional): Maximum recording duration in seconds (default: 5)
- `frequency` (int?, optional): Recording sample rate in Hz (default: 16000)

**Notes:**
- Automatically selects Oculus microphone if available, otherwise uses first available device
- Prevents starting a new recording if one is already in progress

### StopRecording

Manually stops the current recording session.

```csharp
public void StopRecording()
```

**Notes:**
- Converts recorded audio to the specified encoding format
- Invokes the completion callback with the encoded audio data
- Automatically called when maximum recording duration is reached

### ConvertToRawPCM

Converts an AudioClip to raw PCM audio data as a byte array.

```csharp
public static byte[] ConvertToRawPCM(AudioClip clip)
```

**Parameters:**
- `clip` (AudioClip): The Unity AudioClip to convert

**Returns:**
- `byte[]`: Raw PCM audio data as 16-bit signed integers

**Notes:**
- Static utility method for audio conversion
- Returns null if the AudioClip is null
