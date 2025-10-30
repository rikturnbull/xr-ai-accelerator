# GroqImageToText

The `GroqImageToText` class provides image-to-text conversion capabilities using Groq's vision models.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Groq API key for authentication
- **Example**: `"gsk_your-api-key-here"`

### Optional Options

#### imageQuality
- **Type**: `int`
- **Required**: No
- **Default**: `"100"`
- **Range**: 1-100
- **Description**: JPEG compression quality where lower values produce smaller files but reduced image quality

#### maxImageDimension
- **Type**: `int`
- **Required**: No
- **Default**: `"512"`
- **Description**: Maximum width or height for image scaling before processing
- **Common Values**: 512, 1024, 1536

#### model
- **Type**: `string`
- **Required**: No
- **Default**: `"meta-llama/llama-4-scout-17b-16e-instruct"`
- **Description**: The vision model to use
- **Available Models**: 
  - `meta-llama/llama-4-scout-17b-16e-instruct`
  - `meta-llama/llama-4-maverick-17b-128e-instruct`

#### prompt
- **Type**: `string`
- **Required**: No
- **Default**: `"Describe the image"`
- **Description**: The text prompt that guides the AI's analysis of the image

## Methods

### Initialize

Initializes the GroqImageToText instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the Groq service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Processes an image and generates a text description using Groq's vision models.

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to analyze
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<string>>): Callback function to receive the result

**Process Flow:**
1. Scales the input texture to the specified maximum dimension
2. Encodes the scaled texture to JPEG with the specified quality
3. Converts the image to base64 format
4. Sends the image and prompt to Groq's vision API
5. Returns the generated text description through the callback

**Error Handling:**
- Returns failure result if API call fails
- Handles exceptions and network errors gracefully
- Validates input texture and API response

## Usage Example

```csharp
var groqImageToText = XrAiFactory.LoadImageToText("Groq");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-groq-api-key",
    ["model"] = "meta-llama/llama-4-scout-17b-16e-instruct",
    ["prompt"] = "What objects do you see in this image?"
};

// Initialize must be called once before Execute
await groqImageToText.Initialize(options);

await groqImageToText.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        Debug.Log($"Image description: {result.Data}");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```