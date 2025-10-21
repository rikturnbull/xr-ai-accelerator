using System;
using UnityEngine;

namespace XrAiAccelerator
{
    public class XrAiImageHelper
    {
        public static Texture2D ScaleTexture(Texture2D source, int maxDimension)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source texture cannot be null");
            }

            if (source.width <= maxDimension && source.height <= maxDimension)
            {
                Texture2D copy = new(source.width, source.height, source.format, false);
                Graphics.CopyTexture(source, copy);
                return copy;
            }

            int newWidth, newHeight;
            if (source.width > source.height)
            {
                newWidth = maxDimension;
                newHeight = Mathf.RoundToInt((float)source.height / source.width * maxDimension);
            }
            else
            {
                newHeight = maxDimension;
                newWidth = Mathf.RoundToInt((float)source.width / source.height * maxDimension);
            }

            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;
            
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = rt;
            
            Graphics.Blit(source, rt);
            
            Texture2D scaledTexture = new(newWidth, newHeight, TextureFormat.RGBA32, false);
            scaledTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            scaledTexture.Apply();
            
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(rt);
            
            return scaledTexture;
        }

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