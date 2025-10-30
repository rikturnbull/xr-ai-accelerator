# OpenAITextToImage

The `OpenAITextToImage` class provides text-to-image generation capabilities using OpenAI's DALL-E 3 model.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: OpenAI API key for authentication
- **Example**: `"sk-your-api-key-here"`

#### prompt
- **Type**: `string`
- **Required**: Yes
- **Default**: `"A cat wearing a VR headset"`
- **Description**: The text prompt describing the image to generate

## Methods

### Initialize

Initializes the OpenAITextToImage instance with the provided configuration options.

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

Generates an image from text using OpenAI's DALL-E 3 model.

```csharp
public async Task Execute(string prompt, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
```

**Parameters:**
- `prompt` (string): Prompt for image generation
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<Texture2D>>): Callback function to receive the generated image

**Process Flow:**
1. Retrieves the prompt from options
2. Creates an image generation request using DALL-E 3
3. Sends the request to OpenAI's Images API
4. Returns the generated texture through the callback

**Error Handling:**
- Returns failure result if API call fails
- Validates API response content

## Usage Example

```csharp
var openAITextToImage = XrAiFactory.LoadTextToImage("OpenAI");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-openai-api-key"
};

// Initialize must be called once before Execute
await openAITextToImage.Initialize(options);

await openAITextToImage.Execute("A futuristic cityscape with flying cars", options, result =>
{
    if (result.IsSuccess)
    {
        Texture2D generatedImage = result.Data;
        Debug.Log("Image generated successfully");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```