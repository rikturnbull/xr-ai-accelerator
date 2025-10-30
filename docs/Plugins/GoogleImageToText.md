# GoogleImageToText

The `GoogleImageToText` class provides image-to-text conversion capabilities using Google's Gemini Vision API.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Google API key for authentication with the Gemini API
- **Example**: `"AIzaSyC-your-api-key-here"`

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

#### prompt
- **Type**: `string`
- **Required**: No
- **Default**: `"Describe the image"`
- **Description**: The text prompt that guides the AI's analysis of the image

#### url
- **Type**: `string`
- **Required**: No
- **Default**: `"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"`
- **Description**: The Google Gemini API endpoint URL

## Methods

### Initialize

Initializes the GoogleImageToText instance with the provided configuration options.

```csharp
public Task Initialize(Dictionary<string, string> options = null)
```

**Parameters:**
- `options` (Dictionary<string, string>, optional): Configuration options for the Google AI service

**Returns:**
- `Task`: A task that represents the asynchronous initialization operation

**Description:**
- Must be called once before using the Execute method

### Execute

Processes an image and generates a text description using Google's Gemini Vision API.

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
3. Sends the image and prompt to Google's Gemini API
4. Returns the generated text description through the callback

**Error Handling:**
- Returns failure result if API call fails
- Handles exceptions and network errors gracefully
- Validates API response content

## Usage Example

```csharp
var googleImageToText = XrAiFactory.LoadImageToText("Google");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-google-api-key",
    ["prompt"] = "What objects do you see in this image?"
};

// Initialize must be called once before Execute
await googleImageToText.Initialize(options);

await googleImageToText.Execute(inputTexture, options, result =>
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
