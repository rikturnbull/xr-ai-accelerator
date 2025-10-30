# IXrAiTextToText

The `IXrAiTextToText` interface defines the contract for AI models that generate text responses from text input.
This interface should be implemented by providers for tasks like text completion, chat, translation, and text processing.

## Interface Declaration

```csharp
public interface IXrAiTextToText
```

## Methods

### Initialize

Initializes the workflow with provider-specific options asynchronously.

```csharp
public Task Initialize(Dictionary<string, string> options = null);
```

**Parameters:**
- `options` (Dictionary<string, string>): Model-specific options

### Execute

Processes text input and generates a text response asynchronously.

```csharp
public Task Execute(string inputText, Dictionary<string, string> options, Action<XrAiResult<string>> callback);
```

**Parameters:**
- `inputText` (string): The input text to process
- `options` (Dictionary<string, string>): Model-specific options and parameters
- `callback` (Action<XrAiResult&lt;string>>): The callback when inference is complete
