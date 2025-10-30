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
