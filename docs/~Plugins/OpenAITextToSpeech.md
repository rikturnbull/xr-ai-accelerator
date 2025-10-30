# OpenAITextToSpeech

The `OpenAITextToSpeech` class provides text-to-speech conversion capabilities using OpenAI's TTS-1 model.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: OpenAI API key for authentication
- **Example**: `"sk-your-api-key-here"`

### Optional Options

#### speed
- **Type**: `float`
- **Required**: No
- **Default**: `"1.0"`
- **Range**: 0.25-4.0
- **Description**: The speed of the generated audio where 1.0 is normal speed

#### voice
- **Type**: `string`
- **Required**: No
- **Default**: `"alloy"`
- **Description**: The voice to use for text-to-speech generation
- **Available Voices**: `alloy`, `echo`, `fable`, `onyx`, `nova`, `shimmer`

## Methods

### Initialize

Initializes the OpenAITextToSpeech instance with the provided configuration options.

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

Converts text to speech using OpenAI's TTS-1 model.

```csharp
public async Task Execute(string text, Dictionary<string, string> options, Action<XrAiResult<AudioClip>> callback)
```

**Parameters:**
- `text` (string): The text to convert to speech
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<AudioClip>>): Callback function to receive the generated audio

**Process Flow:**
1. Creates a speech request with the specified text, voice, and speed
2. Sends the request to OpenAI's Audio API using TTS-1 model
3. Returns the generated AudioClip through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates API response content

## Usage Example

```csharp
var openAITextToSpeech = XrAiFactory.LoadTextToSpeech("OpenAI");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-openai-api-key",
    ["voice"] = "nova",
    ["speed"] = "1.2"
};

// Initialize must be called once before Execute
await openAITextToSpeech.Initialize(options);

await openAITextToSpeech.Execute("Hello, world!", options, result =>
{
    if (result.IsSuccess)
    {
        AudioClip audioClip = result.Data;
        Debug.Log("Audio generated successfully");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```