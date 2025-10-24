using System;

namespace XrAiAccelerator
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class XrAiOptionAttribute : Attribute
    {
        public string Key { get; }
        public XrAiOptionScope Scope { get; }
        public bool IsRequired { get; }
        public string DefaultValue { get; }
        public string Description { get; }

        public XrAiOptionAttribute(string key, XrAiOptionScope scope, bool isRequired = false, string defaultValue = null, string description = null)
        {
            Key = key;
            Scope = scope;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            Description = description;
        }
    }

    [Serializable]
    public enum XrAiOptionScope
    {
        Global,
        Workflow,
        Both
    }
}