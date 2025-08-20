using System;
using UnityEngine;

namespace XrAiAccelerator
{
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

        public static string DetectImageFormat(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4)
                return "image/jpeg"; // Default fallback

            // Check for common image format signatures
            // JPEG: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            {
                return "image/jpeg";
            }
            
            // PNG: 89 50 4E 47
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            {
                return "image/png";
            }
            
            // GIF: 47 49 46
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
            {
                return "image/gif";
            }
            
            // WebP: starts with "RIFF" and contains "WEBP"
            if (imageBytes.Length >= 12 &&
                imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
            {
                return "image/webp";
            }
            
            // BMP: 42 4D
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
            {
                return "image/bmp";
            }

            // Default to JPEG if unknown
            return "image/jpeg";
        }
    }
}