using System;
using System.Collections.Generic;
using System.Linq;

namespace XrAiAccelerator
{
    public class XrAiOptionsHelper
    {
        private Dictionary<string, string> _globalOptions = null;
        private Type _type;

        public XrAiOptionsHelper(object owner, Dictionary<string, string> globalOptions = null)
        {
            _globalOptions = globalOptions;
            _type = owner.GetType();
        }

        public string GetOption(string key, Dictionary<string, string> options = null)
        {
            if (options != null && options.TryGetValue(key, out string value))
            {
                if(!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            if (_globalOptions != null && _globalOptions.TryGetValue(key, out value))
            {
                if(!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            string defaultValue = GetOptionAttribute(key)?.DefaultValue;
            if (defaultValue != null)
            {
                return defaultValue;
            }

            throw new KeyNotFoundException($"Option '{key}' not found.");
        }

        public int GetIntOption(string key, Dictionary<string, string> options = null)
        {
            string valueStr = GetOption(key, options);
            if (int.TryParse(valueStr, out int value))
            {
                return value;
            }
            throw new FormatException($"Option '{key}' with value '{valueStr}' is not a valid integer.");
        }

        public float GetFloatOption(string key, Dictionary<string, string> options = null)
        {
            string valueStr = GetOption(key, options);
            if (float.TryParse(valueStr, out float value))
            {
                return value;
            }
            throw new FormatException($"Option '{key}' with value '{valueStr}' is not a valid float.");
        }

        private XrAiOptionAttribute GetOptionAttribute(string key)
        {
            var attributes = _type.GetCustomAttributes(typeof(XrAiOptionAttribute), true);
            return attributes.Cast<XrAiOptionAttribute>().FirstOrDefault(attr => attr.Key == key);
        }
    }
}