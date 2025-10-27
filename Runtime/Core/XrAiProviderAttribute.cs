using System;

namespace XrAiAccelerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XrAiProviderAttribute : Attribute
    {
        public string ProviderName { get; }
        public string WorkflowName { get; }

        public XrAiProviderAttribute(string providerName)
        {
            ProviderName = providerName;
        }

        public XrAiProviderAttribute(string providerName, string workflowName)
        {
            ProviderName = providerName;
            WorkflowName = workflowName;
        }
    }
}
