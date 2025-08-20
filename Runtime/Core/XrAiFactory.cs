using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XrAiAccelerator
{
    public class XrAiFactory
    {
        private static readonly Dictionary<Type, Dictionary<string, Type>> _cachedImplementations = new();

        // Public constants for interface type names
        public const string WORKFLOW_IMAGE_TO_3D = "ImageTo3d";
        public const string WORKFLOW_OBJECT_DETECTOR = "ObjectDetector";
        public const string WORKFLOW_IMAGE_TO_TEXT = "ImageToText";
        public const string WORKFLOW_IMAGE_TO_IMAGE = "ImageToImage";
        public const string WORKFLOW_TEXT_TO_IMAGE = "TextToImage";
        public const string WORKFLOW_TEXT_TO_TEXT = "TextToText";
        public const string WORKFLOW_SPEECH_TO_TEXT = "SpeechToText";
        public const string WORKFLOW_TEXT_TO_SPEECH = "TextToSpeech";

        public static readonly Dictionary<string, Type> XrAiInterfaces = new()
        {
            { WORKFLOW_IMAGE_TO_3D, typeof(IXrAiImageTo3d) },
            { WORKFLOW_OBJECT_DETECTOR, typeof(IXrAiObjectDetector) },
            { WORKFLOW_IMAGE_TO_TEXT, typeof(IXrAiImageToText) },
            { WORKFLOW_IMAGE_TO_IMAGE, typeof(IXrAiImageToImage) },
            { WORKFLOW_TEXT_TO_IMAGE, typeof(IXrAiTextToImage) },
            { WORKFLOW_TEXT_TO_TEXT, typeof(IXrAiTextToText) },
            { WORKFLOW_SPEECH_TO_TEXT, typeof(IXrAiSpeechToText) },
            { WORKFLOW_TEXT_TO_SPEECH, typeof(IXrAiTextToSpeech) }
        };

        static XrAiFactory()
        {
            DiscoverAllInterfaces();
        }

        private static void DiscoverAllInterfaces()
        {
            try
            {
                // Pre-populate cache for each interface
                foreach (var interfaceType in XrAiInterfaces.Values)
                {
                    GetImplementationsForType(interfaceType);
                }

                UnityEngine.Debug.Log($"XrAiFactory initialized with {XrAiInterfaces.Count} interfaces and {_cachedImplementations.Values.Sum(d => d.Count)} implementations");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize XR AI interfaces: {ex.Message}");
            }
        }

        private static Dictionary<string, Type> GetImplementationsForType(Type interfaceType)
        {
            if (_cachedImplementations.TryGetValue(interfaceType, out var cached))
            {
                return cached;
            }

            var implementations = new Dictionary<string, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

                    foreach (var type in types)
                    {
                        var attribute = type.GetCustomAttribute<XrAiProviderAttribute>();
                        if (attribute != null)
                        {
                            implementations[attribute.ProviderName] = type;
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.LogWarning($"Could not load some types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            _cachedImplementations[interfaceType] = implementations;
            return implementations;
        }

        private static Dictionary<string, Type> GetImplementations<T>()
        {
            return GetImplementationsForType(typeof(T));
        }

        private static T CreateInstance<T>(Type implementationType)
        {
            try
            {
                return (T)Activator.CreateInstance(implementationType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of {implementationType.Name}: {ex.Message}", ex);
            }
        }

        public static IXrAiImageTo3d LoadImageTo3d(string name) => LoadProvider<IXrAiImageTo3d>(name);
        public static IXrAiObjectDetector LoadObjectDetector(string name) => LoadProvider<IXrAiObjectDetector>(name);
        public static IXrAiImageToText LoadImageToText(string name) => LoadProvider<IXrAiImageToText>(name);
        public static IXrAiImageToImage LoadImageToImage(string name) => LoadProvider<IXrAiImageToImage>(name);
        public static IXrAiTextToImage LoadTextToImage(string name) => LoadProvider<IXrAiTextToImage>(name);
        public static IXrAiTextToText LoadTextToText(string name) => LoadProvider<IXrAiTextToText>(name);
        public static IXrAiSpeechToText LoadSpeechToText(string name) => LoadProvider<IXrAiSpeechToText>(name);
        public static IXrAiTextToSpeech LoadTextToSpeech(string name) => LoadProvider<IXrAiTextToSpeech>(name);

        private static T LoadProvider<T>(string name)
        {
            var implementations = GetImplementations<T>();

            if (implementations.TryGetValue(name, out var implementationType))
            {
                return CreateInstance<T>(implementationType);
            }

            throw new NotSupportedException($"Model '{name}' is not supported. Available: {string.Join(", ", implementations.Keys)}");
        }

        public static List<string> GetProviderNames<T>()
        {
            var implementations = GetImplementations<T>();
            return implementations.Keys.ToList();
}

        public static List<XrAiOptionAttribute> GetProviderOptions<T>(string providerName)
        {
            var implementations = GetImplementations<T>();

            if (implementations.TryGetValue(providerName, out var implementationType))
            {
                return GetOptionAttributes(implementationType);
            }

            return new List<XrAiOptionAttribute>();
        }

        public static Dictionary<string, List<XrAiOptionAttribute>> GetAllProviderOptions<T>()
        {
            var implementations = GetImplementations<T>();
            var result = new Dictionary<string, List<XrAiOptionAttribute>>();
            
            foreach (var kvp in implementations)
            {
                result[kvp.Key] = GetOptionAttributes(kvp.Value);
            }
            
            return result;
        }

        public static Dictionary<string, List<XrAiOptionAttribute>> GetAllProviderOptions(Type interfaceType)
        {
            var implementations = GetImplementationsForType(interfaceType);
            var result = new Dictionary<string, List<XrAiOptionAttribute>>();
            
            foreach (var kvp in implementations)
            {
                result[kvp.Key] = GetOptionAttributes(kvp.Value);
            }
            
            return result;
        }

        private static List<XrAiOptionAttribute> GetOptionAttributes(Type type)
        {
            return type.GetCustomAttributes<XrAiOptionAttribute>().ToList();
        }
    }
}