# XrAiKeypoint

The `XrAiKeypoint` struct represents a single keypoint detected by pose estimation or feature detection AI models. Keypoints are typically used for human pose detection, facial landmark detection, or object feature identification.

## Struct Declaration

```csharp
public struct XrAiKeypoint
```

## Properties

### x

The X coordinate of the keypoint.

```csharp
public float x
```

**Type:** `float`  
**Description:** Horizontal position of the keypoint, typically in pixel coordinates or normalized coordinates depending on the AI provider.

### y

The Y coordinate of the keypoint.

```csharp
public float y
```

**Type:** `float`  
**Description:** Vertical position of the keypoint, typically in pixel coordinates or normalized coordinates depending on the AI provider.

### confidence

The confidence score for the keypoint detection.

```csharp
public float confidence
```

**Type:** `float`  
**Description:** Confidence level of the keypoint detection, typically ranging from 0.0 (low confidence) to 1.0 (high confidence).

### class_id

The numeric identifier for the keypoint class.

```csharp
public int class_id
```

**Type:** `int`  
**Description:** Integer ID representing the type of keypoint (e.g., 0 for nose, 1 for left eye, etc. in pose estimation models).

### @class

The string name for the keypoint class.

```csharp
public string @class
```

**Type:** `string`  
**Description:** Human-readable name of the keypoint type (e.g., "nose", "left_wrist", "right_ankle"). Uses C# keyword escaping (@) for the reserved word "class".
