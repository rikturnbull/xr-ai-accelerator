using System;
using System.Collections.Generic;
using Unity.InferenceEngine;

namespace XrAiAccelerator
{
    public class XrAiFactory
    {
        public static IXrAImageTo3d LoadImageTo3d(string name, Dictionary<string, string> options = null)
        {
            if (name == "StabilityAi")
            {
                return new StabilityAiImageTo3d(options);
            }
            else
            {
                throw new NotSupportedException($"ImageTo3d model '{name}' is not supported.");
            }
        }

        public static IXrAiObjectDetector LoadObjectDetector(string name, Dictionary<string, string> options = null, XrAiAssets assets = null)
        {
            if (name == "Google")
            {
                return new GoogleObjectDetector(options);
            }
            if (name == "Yolo")
            {
                return new YoloObjectDetector(assets, options);
            }
            else if (name == "Roboflow")
            {
                return new RoboflowObjectDetector(options);
            }
            else if (name == "RoboflowLocal")
            {
                return new RoboflowObjectDetector(options);
            }
            else
            {
                throw new NotSupportedException($"ObjectDetector model '{name}' is not supported.");
            }
        }

        public static IXrAiImageToText LoadImageToText(string name, Dictionary<string, string> properties = null)
        {
            if (name == "Groq")
            {
                return new GroqImageToText(properties);
            }
            else if (name == "Google")
            {
                return new GoogleImageToText(properties);
            }
            else if (name == "Nvidia")
            {
                return new NvidiaImageToText(properties);
            }
            else
            {
                throw new NotSupportedException($"ImageToText model '{name}' is not supported.");
            }
        }

        public static IXrAiImageToImage LoadImageToImage(string name, Dictionary<string, string> properties = null)
        {
            if (name == "OpenAI")
            {
                return new OpenAIImageToImage(properties);
            }
            else
            {
                throw new NotSupportedException($"ImageToImage model '{name}' is not supported.");
            }
        }

        public static IXrAiTextToImage LoadTextToImage(string name, Dictionary<string, string> properties = null)
        {
            if (name == "OpenAI")
            {
                return new OpenAITextToImage(properties);
            }
            else
            {
                throw new NotSupportedException($"TextToImage model '{name}' is not supported.");
            }
        }

        public static IXrAiSpeechToText LoadSpeechToText(string name, Dictionary<string, string> properties = null)
        {
            if (name == "OpenAI")
            {
                return new OpenAISpeechToText(properties);
            }
            else
            {
                throw new NotSupportedException($"SpeechToText model '{name}' is not supported.");
            }
        }
        
        public static IXrAiTextToSpeech LoadTextToSpeech(string name, Dictionary<string, string> properties = null)
        {
            if (name == "OpenAI")
            {
                return new OpenAITextToSpeech(properties);
            }
            else
            {
                throw new NotSupportedException($"TextToSpeech model '{name}' is not supported.");
            }
        }
    }
}