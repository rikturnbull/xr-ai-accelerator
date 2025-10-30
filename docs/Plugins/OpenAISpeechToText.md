# OpenAISpeechToText

The `OpenAISpeechToText` class provides speech-to-text transcription capabilities using OpenAI's Whisper model.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: OpenAI API key for authentication
- **Example**: `"sk-your-api-key-here"`

### Optional Options

#### language
- **Type**: `string`
- **Required**: No
- **Default**: `"en"`
- **Description**: The language of the audio for transcription
- **Example**: `"en"` for English, `"es"` for Spanish

#### model
- **Type**: `string`
- **Required**: No
- **Default**: `"whisper-1"`
- **Description**: The Whisper model to use for speech-to-text conversion

#### prompt
- **Type**: `string`
- **Required**: No
- **Default**: `""`
- **Description**: An optional text prompt to guide the transcription context

#### temperature
- **Type**: `float`
- **Required**: No
- **Default**: `"0.0"`
- **Description**: Controls randomness in the transcription output (0.0 = deterministic)

## Methods

### Initialize

Initializes the OpenAISpeechToText instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the OpenAI service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Transcribes audio data to text using OpenAI's Whisper model.

```csharp
public async Task Execute(byte[] audioData, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
```

**Parameters:**
- `audioData` (byte[]): The audio data to transcribe
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<string>>): Callback function to receive the transcribed text

**Process Flow:**
1. Creates a memory stream from the audio data
2. Creates an audio transcription request with specified parameters
3. Sends the request to OpenAI's Audio API
4. Returns the transcribed text through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates audio data and API response

## Usage Example

```csharp
var openAISpeechToText = XrAiFactory.LoadSpeechToText("OpenAI");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-openai-api-key",
    ["language"] = "en",
    ["prompt"] = "This is a conversation about technology"
};

// Initialize must be called once before Execute
await openAISpeechToText.Initialize(options);

await openAISpeechToText.Execute(audioBytes, options, result =>
{
    if (result.IsSuccess)
    {
        string transcribedText = result.Data;
        Debug.Log($"Transcribed text: {transcribedText}");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```