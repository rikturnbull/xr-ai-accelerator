# XrAiSecretsManager

The `XrAiSecretsManager` class is a ScriptableObject that provides secure storage and management of API keys, tokens, and other sensitive configuration data required by AI providers. It handles loading, saving, and accessing secrets through a centralized system.

## Methods

### GetSecrets

Returns the complete secrets dictionary.

```csharp
public Dictionary<string, string> GetSecrets()
```

**Returns:**
- `Dictionary<string, string>`: The complete collection of stored secrets

### GetSecret

Retrieves a specific secret by name.

```csharp
public string GetSecret(string name)
```

**Parameters:**
- `name` (string): The identifier of the secret to retrieve

**Returns:**
- `string`: The secret value, or null if not found

### GetSecretsManager (Static)

Gets the singleton instance of the XrAiSecretsManager.

```csharp
public static XrAiSecretsManager GetSecretsManager()
```

**Returns:**
- `XrAiSecretsManager`: The singleton instance loaded from Resources

**Notes:**
- Automatically loads the instance from `Resources/XrAiSecretsManager`
- Logs an error if the manager asset is not found
