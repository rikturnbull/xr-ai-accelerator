using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XrAiAccelerator
{
    [System.Serializable]
    public class XrAiProperty
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class XrAiSection
    {
        public string sectionName;
        public List<XrAiProperty> properties = new List<XrAiProperty>();
    }

    [System.Serializable]
    public class XrAiModelData
    {
        public List<XrAiSection> sections = new List<XrAiSection>();
    }

    [CreateAssetMenu(fileName = "XrAiModelManager", menuName = "XR AI Accelerator/Model Manager")]
    public class XrAiModelManager : ScriptableObject
    {
        private const string CONFIG_FILE_PATH = "XrAiModelConfig";
        private const string CONFIG_FILE_PATH_TEMPLATE = "XrAiModelConfig.default";
        private const string API_KEYS_FILE_PATH = "XrAiApiKeys";
        private XrAiModelData _modelData;
        private XrAiModelData _apiKeysData;

        public static Dictionary<string, XrAiOptionAttribute[]> GLOBAL_SECTION = InitializeGlobalProperties();

        private static XrAiModelManager _instance;

        public static Dictionary<string, Dictionary<string, XrAiOptionAttribute[]>> WORKFLOW_PROPERTIES = InitializeWorkflowProperties();

        public static void ReInitialize()
        {
            GLOBAL_SECTION = InitializeGlobalProperties();
            WORKFLOW_PROPERTIES = InitializeWorkflowProperties();
        }

        private static Dictionary<string, Dictionary<string, XrAiOptionAttribute[]>> InitializeWorkflowProperties()
        {
            var workflowProperties = new Dictionary<string, Dictionary<string, XrAiOptionAttribute[]>>();

            foreach (var interfaceKvp in XrAiFactory.XrAiInterfaces)
            {
                var interfaceName = interfaceKvp.Key;
                var interfaceType = interfaceKvp.Value;

                var allProviderOptions = XrAiFactory.GetAllProviderOptions(interfaceType);

                foreach (var providerKvp in allProviderOptions)
                {
                    var providerName = providerKvp.Key;
                    var options = providerKvp.Value;

                    var workflowOptions = options
                        .Where(opt => opt.Scope == XrAiOptionScope.Workflow || opt.Scope == XrAiOptionScope.Both)
                        .Select(opt => opt)
                        .ToArray();

                    if (workflowOptions.Length > 0)
                    {
                        if (!workflowProperties.ContainsKey(providerName))
                        {
                            workflowProperties[providerName] = new Dictionary<string, XrAiOptionAttribute[]>();
                        }

                        workflowProperties[providerName][interfaceName] = workflowOptions;
                    }
                }
            }

            return workflowProperties;
        }

        private static Dictionary<string, XrAiOptionAttribute[]> InitializeGlobalProperties()
        {
            var globalProperties = new Dictionary<string, XrAiOptionAttribute[]>();
            var allProviders = new HashSet<string>();

            foreach (var interfaceType in XrAiFactory.XrAiInterfaces.Values)
            {
                var providerOptions = XrAiFactory.GetAllProviderOptions(interfaceType);
                foreach (var providerName in providerOptions.Keys)
                {
                    allProviders.Add(providerName);
                }
            }

            foreach (var providerName in allProviders)
            {
                var globalOptions = new HashSet<XrAiOptionAttribute>();

                foreach (var interfaceType in XrAiFactory.XrAiInterfaces.Values)
                {
                    var providerOptions = XrAiFactory.GetAllProviderOptions(interfaceType);
                    if (providerOptions.TryGetValue(providerName, out var options))
                    {
                        var relevantOptions = options
                            .Where(opt => opt.Scope == XrAiOptionScope.Global || opt.Scope == XrAiOptionScope.Both)
                            .Select(opt => opt);
                        
                        foreach (var option in relevantOptions)
                        {
                            globalOptions.Add(option);
                        }
                    }
                }

                if (globalOptions.Count > 0)
                {
                    globalProperties[providerName] = globalOptions.ToArray();
                }
            }

            return globalProperties;
        }

        public XrAiModelData ModelData
        {
            get
            {
                if (_modelData == null)
                    LoadFromFile();
                return _modelData;
            }
        }

        public XrAiModelData ApiKeysData
        {
            get
            {
                if (_apiKeysData == null)
                    LoadFromFile();
                return _apiKeysData;
            }
        }

        void Awake()
        {
            LoadFromFile();
        }

        public void LoadFromFile()
        {
            TextAsset configFile = Resources.Load<TextAsset>(CONFIG_FILE_PATH);
            if (configFile == null)
            {
                configFile = Resources.Load<TextAsset>(CONFIG_FILE_PATH_TEMPLATE);
            }
            if (configFile != null)
            {
                try
                {
                    _modelData = JsonUtility.FromJson<XrAiModelData>(configFile.text);
                    if (_modelData.sections == null)
                        _modelData.sections = new List<XrAiSection>();
                }
                catch
                {
                    _modelData = new XrAiModelData();
                    _modelData.sections = new List<XrAiSection>();
                }
            }
            else
            {
                _modelData = new XrAiModelData();
                _modelData.sections = new List<XrAiSection>();
            }

            TextAsset apiKeysFile = Resources.Load<TextAsset>(API_KEYS_FILE_PATH);
            if (apiKeysFile != null)
            {
                try
                {
                    _apiKeysData = JsonUtility.FromJson<XrAiModelData>(apiKeysFile.text);
                    if (_apiKeysData.sections == null)
                        _apiKeysData.sections = new List<XrAiSection>();
                }
                catch
                {
                    _apiKeysData = new XrAiModelData();
                    _apiKeysData.sections = new List<XrAiSection>();
                }
            }
            else
            {
                _apiKeysData = new XrAiModelData();
                _apiKeysData.sections = new List<XrAiSection>();
            }

            InitializePredefinedProperties();
        }

        private void InitializePredefinedProperties()
        {
            foreach (var sectionDef in GLOBAL_SECTION)
            {
                var apiSection = _apiKeysData.sections.FirstOrDefault(s => s.sectionName == sectionDef.Key);
                if (apiSection == null)
                {
                    apiSection = new XrAiSection { sectionName = sectionDef.Key };
                    _apiKeysData.sections.Add(apiSection);
                }

                foreach (var key in sectionDef.Value)
                {
                    if (key.Key == "apiKey" && !apiSection.properties.Any(p => p.key == key.Key))
                    {
                        apiSection.properties.Add(new XrAiProperty()
                        {
                            key = key.Key,
                            value = ""
                        });
                    }
                }

                var section = _modelData.sections.FirstOrDefault(s => s.sectionName == sectionDef.Key);
                if (section == null)
                {
                    section = new XrAiSection { sectionName = sectionDef.Key };
                    _modelData.sections.Add(section);
                }

                if (WORKFLOW_PROPERTIES.ContainsKey(sectionDef.Key))
                {
                    foreach (var workflow in WORKFLOW_PROPERTIES[sectionDef.Key])
                    {
                        var workflowSectionName = $"{sectionDef.Key}.{workflow.Key}";
                        var workflowSection = _modelData.sections.FirstOrDefault(s => s.sectionName == workflowSectionName);
                        if (workflowSection == null)
                        {
                            workflowSection = new XrAiSection { sectionName = workflowSectionName };
                            _modelData.sections.Add(workflowSection);
                        }

                        foreach (var key in workflow.Value)
                        {
                            if (!workflowSection.properties.Any(p => p.key == key.Key))
                            {
                                workflowSection.properties.Add(new XrAiProperty()
                                {
                                    key = key.Key,
                                    value = ""
                                });
                            }
                        }
                    }
                }
            }
        }

        public void SaveToFile()
        {
            string json = JsonUtility.ToJson(_modelData, true);
            string filePath = Application.dataPath + "/Resources/" + CONFIG_FILE_PATH + ".txt";
            System.IO.File.WriteAllText(filePath, json);

            string apiKeysJson = JsonUtility.ToJson(_apiKeysData, true);
            string apiKeysFilePath = Application.dataPath + "/Resources/" + API_KEYS_FILE_PATH + ".txt";
            System.IO.File.WriteAllText(apiKeysFilePath, apiKeysJson);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private Dictionary<string, string> GetSectionProperties(string sectionName)
        {
            var result = new Dictionary<string, string>();
            var section = _modelData.sections.FirstOrDefault(s => s.sectionName == sectionName);
            if (section != null)
            {
                foreach (var property in section.properties)
                {
                    result[property.key] = property.value;
                }
            }
            return result;
        }

        public Dictionary<string, string> GetGlobalProperties(string sectionName)
        {
            var result = GetSectionProperties(sectionName);

            var apiSection = _apiKeysData.sections.FirstOrDefault(s => s.sectionName == sectionName);
            if (apiSection != null)
            {
                var apiKeyProperty = apiSection.properties.FirstOrDefault(p => p.key == "apiKey");
                if (apiKeyProperty != null)
                {
                    result["apiKey"] = apiKeyProperty.value;
                }
            }

            return result;
        }

        public Dictionary<string, string> GetWorkflowProperties(string sectionName, string workflowName)
        {
            string workflowSectionName = $"{sectionName}.{workflowName}";
            return GetSectionProperties(workflowSectionName);
        }

        public string GetGlobalProperty(string sectionName, string key, string defaultValue = "")
        {
            if (key == "apiKey")
            {
                var apiSection = _apiKeysData.sections.FirstOrDefault(s => s.sectionName == sectionName);
                if (apiSection != null)
                {
                    var property = apiSection.properties.Find(p => p.key == key);
                    return property?.value ?? defaultValue;
                }
                return defaultValue;
            }

            var section = _modelData.sections.FirstOrDefault(s => s.sectionName == sectionName);
            if (section != null)
            {
                var property = section.properties.Find(p => p.key == key);
                return property?.value ?? defaultValue;
            }
            return defaultValue;
        }

        public string GetWorkflowProperty(string sectionName, string workflowName, string key, string defaultValue = "")
        {
            string workflowSectionName = $"{sectionName}.{workflowName}";
            var section = _modelData.sections.FirstOrDefault(s => s.sectionName == workflowSectionName);
            if (section != null)
            {
                var property = section.properties.Find(p => p.key == key);
                return property?.value ?? defaultValue;
            }
            return defaultValue;
        }
        
        public static XrAiModelManager GetModelManager()
        {
            if (_instance == null)
            {
                _instance = Resources.Load<XrAiModelManager>("XrAiModelManager");
                if (_instance == null)
                {
                    Debug.LogError("XrAiModelManager not found in Resources.");
                }
            }
            return _instance;
        }
    }
}
