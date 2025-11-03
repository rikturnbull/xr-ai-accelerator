# GoogleTextToImage

The `GoogleTextToImage` class provides text-to-image conversion capabilities using Google's Gemini Vision API.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: Google API key for authentication with the Gemini API
- **Example**: `"AIzaSyC-your-api-key-here"`

### Optional Options

#### url
- **Type**: `string`
- **Required**: No
- **Default**: `"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent"`
- **Description**: The Google Gemini API endpoint URL

## Methods

### Initialize

Initializes the GoogleTextToImage instance with the provided configuration options.

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

Generates an image from the prompt using Google's Gemini Vision API.

```csharp
public async Task Execute(string prompt, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
```

**Parameters:**
- `prompt` (string): The prompt for image generation
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<Texture2D>>): Callback function to receive the result

**Process Flow:**
1. Sends the prompt to Google's Gemini API
2. Decodes the base64 response into a Texture2D
4. Returns the generated text description through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates API response content

## Usage Example

```csharp
var googleTextToImage = XrAiFactory.LoadImageToText("Google");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-google-api-key",
};

// Initialize must be called once before Execute
await googleImageToText.Initialize(options);

await googleImageToText.Execute(prompt, options, result =>
{
    if (result.IsSuccess)
    {
        Debug.Log($"Texture2D: {result.Data}");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```
