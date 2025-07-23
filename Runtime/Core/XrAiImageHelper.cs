using System;
using UnityEngine;

public class XrAiImageHelper
{
    public static byte[] EncodeTexture(Texture2D texture, string imageFormat)
    {
        if (texture == null)
        {
            throw new ArgumentNullException(nameof(texture), "Texture cannot be null");
        }

        return imageFormat.ToLower() switch
        {
            "image/jpeg" => texture.EncodeToJPG(),
            "image/png" => texture.EncodeToPNG(),
            _ => throw new NotSupportedException($"Image format '{imageFormat}' is not supported"),
        };
    }
}