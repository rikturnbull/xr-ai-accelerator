using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

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

public class XrAiModelManager : MonoBehaviour
{
    private const string CONFIG_FILE_PATH = "XrAiModelConfig";
    private const string API_KEYS_FILE_PATH = "XrAiApiKeys";
    private XrAiModelData _modelData;
    private XrAiModelData _apiKeysData;
    
    // Workflow constants
    public const string WORKFLOW_IMAGE_TO_IMAGE = "ImageToImage";
    public const string WORKFLOW_IMAGE_TO_TEXT = "ImageToText";
    public const string WORKFLOW_TEXT_TO_IMAGE = "TextToImage";
    public const string WORKFLOW_SPEECH_TO_TEXT = "SpeechToText";
    public const string WORKFLOW_IMAGE_TO_3D = "ImageTo3d";
    public const string WORKFLOW_OBJECT_DETECTOR = "ObjectDetector";
    public const string WORKFLOW_TEXT_TO_SPEECH = "TextToSpeech";
    
    // Predefined property keys organized by sections
    public static readonly Dictionary<string, string[]> ARTIFACT_SECTIONS = new Dictionary<string, string[]>
    {
        { "Nvidia", new[] { "apiKey" } },
        { "OpenAI", new[] { "apiKey" } },
        { "StabilityAi", new[] { "apiKey" } },
        { "Google", new[] { "apiKey" } },
        { "Groq", new[] { "apiKey" } },
        { "Roboflow", new[] { "apiKey" } },
        { "RoboflowLocal", new[] { "apiKey" } }
    };
    
    // Workflow-specific properties for each section
    public static readonly Dictionary<string, Dictionary<string, string[]>> WORKFLOW_PROPERTIES = new Dictionary<string, Dictionary<string, string[]>>
    {
        { "Google", new Dictionary<string, string[]>
            {
                { WORKFLOW_IMAGE_TO_TEXT, new[] { "prompt", "url" } },
                { WORKFLOW_OBJECT_DETECTOR, new[] { "url" } }
            }
        },
        { "Groq", new Dictionary<string, string[]>
            {
                { WORKFLOW_IMAGE_TO_TEXT, new[] { "model", "prompt" } },
        }
        },
        { "Nvidia", new Dictionary<string, string[]>
            {
                { WORKFLOW_IMAGE_TO_TEXT, new[] { "prompt", "model", "url" } },
            }
        },
        { "OpenAI", new Dictionary<string, string[]>
            {
                { WORKFLOW_SPEECH_TO_TEXT, new[] { "model" } },
            }
        },
        { "Roboflow", new Dictionary<string, string[]>
            {
                { WORKFLOW_OBJECT_DETECTOR, new[] { "url" } }
            }
        },
        { "RoboflowLocal", new Dictionary<string, string[]>
            {
                { WORKFLOW_OBJECT_DETECTOR, new[] { "url" } }
            }
        }
    };
    
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
        // Load main config
        TextAsset configFile = Resources.Load<TextAsset>(CONFIG_FILE_PATH);
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

        // Load API keys
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
        
        // Ensure all predefined properties exist
        InitializePredefinedProperties();
    }
    
    private void InitializePredefinedProperties()
    {
        foreach (var sectionDef in ARTIFACT_SECTIONS)
        {
            // Initialize API keys in separate data
            var apiSection = _apiKeysData.sections.FirstOrDefault(s => s.sectionName == sectionDef.Key);
            if (apiSection == null)
            {
                apiSection = new XrAiSection { sectionName = sectionDef.Key };
                _apiKeysData.sections.Add(apiSection);
            }
            
            // Add API key properties to API keys data
            foreach (var key in sectionDef.Value)
            {
                if (key == "apiKey" && !apiSection.properties.Any(p => p.key == key))
                {
                    apiSection.properties.Add(new XrAiProperty()
                    {
                        key = key,
                        value = ""
                    });
                }
            }

            // Initialize main config section (without API keys)
            var section = _modelData.sections.FirstOrDefault(s => s.sectionName == sectionDef.Key);
            if (section == null)
            {
                section = new XrAiSection { sectionName = sectionDef.Key };
                _modelData.sections.Add(section);
            }
            
            // Add workflow-specific properties to main config
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
                        if (!workflowSection.properties.Any(p => p.key == key))
                        {
                            workflowSection.properties.Add(new XrAiProperty()
                            {
                                key = key,
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
        // Save main config
        string json = JsonUtility.ToJson(_modelData, true);
        string filePath = Application.dataPath + "/Resources/" + CONFIG_FILE_PATH + ".txt";
        System.IO.File.WriteAllText(filePath, json);

        // Save API keys
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
        
        // Add API key from separate data
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
        // Check if it's an API key
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

        // Regular property from main config
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
}
