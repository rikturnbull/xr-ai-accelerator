# XrAiBoundingBox

The `XrAiBoundingBox` struct represents a detected object's bounding box information returned by object detection AI models. It contains spatial coordinates, dimensions, classification data, and optional keypoint information.

## Struct Declaration

```csharp
public struct XrAiBoundingBox
```

## Properties

### CenterX

The X coordinate of the bounding box center.

```csharp
public float CenterX
```

**Type:** `float`  
**Description:** Horizontal center position of the detected object, typically in normalized coordinates (0.0 to 1.0) or pixel coordinates depending on the AI provider.

### CenterY

The Y coordinate of the bounding box center.

```csharp
public float CenterY
```

**Type:** `float`  
**Description:** Vertical center position of the detected object, typically in normalized coordinates (0.0 to 1.0) or pixel coordinates depending on the AI provider.

### Width

The width of the bounding box.

```csharp
public float Width
```

**Type:** `float`  
**Description:** Horizontal dimension of the bounding box, in the same coordinate system as CenterX.

### Height

The height of the bounding box.

```csharp
public float Height
```

**Type:** `float`  
**Description:** Vertical dimension of the bounding box, in the same coordinate system as CenterY.

### ClassName

The classification label for the detected object.

```csharp
public string ClassName
```

**Type:** `string`  
**Description:** Human-readable name of the detected object class (e.g., "person", "car", "dog").

### Keypoints

Array of keypoints associated with the detected object.

```csharp
public XrAiKeypoint[] Keypoints
```

**Type:** `XrAiKeypoint[]`  
**Description:** Optional array of keypoints for pose estimation or feature detection. May be null if the detection model doesn't provide keypoint data.
