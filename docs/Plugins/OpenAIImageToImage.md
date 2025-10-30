# OpenAIImageToImage

The `OpenAIImageToImage` class provides image editing capabilities using OpenAI's image editing API.

## Configuration Options

### Required Options

#### apiKey
- **Type**: `string`
- **Required**: Yes
- **Description**: OpenAI API key for authentication
- **Example**: `"sk-your-api-key-here"`

## Methods

### Initialize

Initializes the OpenAIImageToImage instance with the provided configuration options.

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

Edits an image using OpenAI's image editing API with the prompt "Complete the image."

```csharp
public async Task Execute(Texture2D texture, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to edit (must be readable)
- `options` (Dictionary<string, string>): Configuration options for the API call
- `callback` (Action<XrAiResult<Texture2D>>): Callback function to receive the edited image

**Process Flow:**
1. Converts the input texture to RGBA32 format if needed
2. Creates an image edit request with the predefined prompt "Complete the image."
3. Sends the request to OpenAI's Images API
4. Cleans up temporary textures
5. Returns the edited texture through the callback

**Requirements:**
- Input texture must have "Read/Write Enabled" in import settings

**Error Handling:**
- Returns failure result if API call fails
- Handles exceptions and network errors gracefully
- Validates texture readability and API response

## Usage Example

```csharp
var openAIImageToImage = XrAiFactory.LoadImageToImage("OpenAI");
var options = new Dictionary<string, string>
{
    ["apiKey"] = "your-openai-api-key"
};

// Initialize must be called once before Execute
await openAIImageToImage.Initialize(options);

await openAIImageToImage.Execute(inputTexture, options, result =>
{
    if (result.IsSuccess)
    {
        Texture2D editedImage = result.Data;
        Debug.Log("Image edited successfully");
    }
    else
    {
        Debug.LogError($"Error: {result.ErrorMessage}");
    }
});
```

## Notes

- The class uses a fixed prompt "Complete the image." for all image editing operations
