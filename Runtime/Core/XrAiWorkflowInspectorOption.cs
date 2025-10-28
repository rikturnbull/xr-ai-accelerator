using System;

namespace XrAiAccelerator
{
    [Serializable]
    public class XrAiWorkflowInspectorOption
    {
        public string Key;
        public XrAiOptionScope Scope;
        public bool IsRequired;
        public string DefaultValue;
        public string Description;
        public string Value;
    }
}