# XrAiImageHelper

The `XrAiImageHelper` class provides utility methods for encoding Unity textures into standard image formats. This helper simplifies the process of converting `Texture2D` objects into byte arrays suitable for AI model processing.

## Class Declaration

```csharp
public class XrAiImageHelper
```

## Methods

### EncodeTexture

Converts a Unity Texture2D to a byte array in the specified image format.

```csharp
public static byte[] EncodeTexture(Texture2D texture, string imageFormat)
```

**Parameters:**
- `texture` (Texture2D): The Unity texture to encode
- `imageFormat` (string): The desired output format ("image/jpeg" or "image/png")

**Returns:**
- `byte[]`: The encoded image data as a byte array

**Throws:**
- `ArgumentNullException`: When texture is null
- `NotSupportedException`: When the specified image format is not supported

### ScaleTexture

Scales a Unity Texture2D to fit within the specified maximum dimension while maintaining aspect ratio.

```csharp
public static Texture2D ScaleTexture(Texture2D source, int maxDimension)
```

**Parameters:**
- `source` (Texture2D): The source texture to scale
- `maxDimension` (int): The maximum width or height for the scaled texture

**Returns:**
- `Texture2D`: A new scaled texture that fits within the specified maximum dimension

**Throws:**
- `ArgumentNullException`: When source texture is null

**Notes:**
- If the source texture is already within the maximum dimension, a copy is returned
- Aspect ratio is preserved during scaling

### DetectImageFormat

Detects the image format of a byte array by examining the file header signatures.

```csharp
public static string DetectImageFormat(byte[] imageBytes)
```

**Parameters:**
- `imageBytes` (byte[]): The image data as a byte array

**Returns:**
- `string`: The detected image format MIME type

**Supported Detection:**
- JPEG: `"image/jpeg"`
- PNG: `"image/png"`
- GIF: `"image/gif"`
- WebP: `"image/webp"`
- BMP: `"image/bmp"`

**Notes:**
- Returns `"image/jpeg"` as default if format cannot be detected
- Requires at least 4 bytes for reliable detection
