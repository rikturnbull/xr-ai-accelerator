# XrAiResult

The `XrAiResult` class provides a unified result type for all AI operations in the XR AI Library. It encapsulates both success and error states, following a result pattern for error handling.

## Class Declaration

```csharp
public abstract class XrAiResult
```

## Properties

### IsSuccess

Indicates whether the operation completed successfully.

```csharp
public bool IsSuccess { get; protected set; }
```

### ErrorMessage

Contains the error message if the operation failed.

```csharp
public string ErrorMessage { get; protected set; }
```

## Constructor

Creates a new instance of `XrAiResult`.

```csharp
protected XrAiResult(bool isSuccess, string errorMessage = null)
```

**Parameters:**
- `isSuccess` (bool): Whether the operation was successful
- `errorMessage` (string, optional): Error message if the operation failed

## Static Methods

### Success<T>

Creates a successful result with data.

```csharp
public static XrAiResult<T> Success<T>(T data)
```

**Parameters:**
- `data` (T): The result data

**Returns:**
- `XrAiResult<T>`: A successful result containing the data

### Failure<T>

Creates a failed result with an error message.

```csharp
public static XrAiResult<T> Failure<T>(string errorMessage)
```

**Parameters:**
- `errorMessage` (string): Description of the error

**Returns:**
- `XrAiResult<T>`: A failed result with the error message

---

# XrAiResult<T>

The generic `XrAiResult<T>` class extends `XrAiResult` to include typed data for successful operations.

## Class Declaration

```csharp
public class XrAiResult<T> : XrAiResult
```

## Properties

### Data

Contains the result data for successful operations.

```csharp
public T Data { get; private set; }
```

## Constructor

Creates a new instance of `XrAiResult<T>`.

```csharp
internal XrAiResult(T data, bool isSuccess, string errorMessage = null)
```

**Parameters:**
- `data` (T): The result data
- `isSuccess` (bool): Whether the operation was successful
- `errorMessage` (string, optional): Error message if the operation failed

## Usage Examples

### Checking Results

```csharp
XrAiResult<string> result = await imageToText.Execute(imageBytes, "image/jpeg");

if (result.IsSuccess)
{
    Debug.Log($"Generated text: {result.Data}");
}
else
{
    Debug.LogError($"Error: {result.ErrorMessage}");
}
```

### Creating Results

```csharp
// Creating a successful result
var successResult = XrAiResult.Success("Generated text content");

// Creating a failed result
var failureResult = XrAiResult.Failure<string>("API request failed");
```

## Common Usage Pattern

```csharp
public async Task ProcessImage(Texture2D image)
{
    var result = await imageToTextModel.Execute(imageBytes, "image/jpeg");
    
    if (!result.IsSuccess)
    {
        HandleError(result.ErrorMessage);
        return;
    }
    
    ProcessTextResult(result.Data);
}
```

## Notes

- The result pattern helps avoid exceptions for expected failure cases
- Always check `IsSuccess` before accessing `Data`
- `ErrorMessage` is only meaningful when `IsSuccess` is false
- The generic type parameter `T` matches the expected return type of the AI operation
