#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(XrAiModelManager))]
public class XrAiModelManagerEditor : Editor
{
    private XrAiModelManager _manager;

    void OnEnable()
    {
        _manager = (XrAiModelManager)target;
        _manager.LoadFromFile();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("XR AI Model Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Draw properties organized by sections
        foreach (var section in XrAiModelManager.ARTIFACT_SECTIONS)
        {
            EditorGUILayout.LabelField(GetSectionDisplayName(section.Key), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // Draw global properties
            EditorGUILayout.LabelField("Global Settings", EditorStyles.miniBoldLabel);
            foreach (var key in section.Value)
            {
                DrawPropertyByKey(section.Key, key);
            }
            
            // Draw workflow-specific properties
            if (XrAiModelManager.WORKFLOW_PROPERTIES.ContainsKey(section.Key))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Workflow Settings", EditorStyles.miniBoldLabel);
                
                foreach (var workflow in XrAiModelManager.WORKFLOW_PROPERTIES[section.Key])
                {
                    DrawWorkflowSection(section.Key, workflow.Key, workflow.Value);
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Save and load buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save to File"))
        {
            EnsureResourcesFolderExists();
            _manager.SaveToFile();
            EditorUtility.DisplayDialog("Saved", "Configuration saved to Resources/XrAiModelConfig.txt and API keys saved to Resources/XrAiApiKeys.txt", "OK");
        }

        if (GUILayout.Button("Load from File"))
        {
            _manager.LoadFromFile();
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_manager);
        }
    }

    private string GetSectionDisplayName(string sectionKey)
    {
        switch (sectionKey)
        {
            case "OpenAI": return "OpenAI";
            case "StabilityAi": return "Stability AI";
            case "Google": return "Google";
            case "Groq": return "Groq";
            case "Roboflow": return "Roboflow";
            default: return sectionKey;
        }
    }
    
    private void DrawWorkflowSection(string sectionName, string workflowName, string[] properties)
    {
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField(GetWorkflowDisplayName(workflowName), EditorStyles.boldLabel);
        
        string workflowSectionName = $"{sectionName}.{workflowName}";
        foreach (var key in properties)
        {
            DrawPropertyByKey(workflowSectionName, key);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
    
    private string GetWorkflowDisplayName(string workflowKey)
    {
        switch (workflowKey)
        {
            case "ImageToImage": return "Image to Image";
            case "ImageToText": return "Image to Text";
            case "TextToImage": return "Text to Image";
            case "SpeechToText": return "Speech to Text";
            case "ImageTo3d": return "Image to 3D";
            default: return workflowKey;
        }
    }

    private void DrawPropertyByKey(string sectionName, string key)
    {
        XrAiSection section;
        
        // Check if it's an API key - use API keys data
        if (key == "apiKey")
        {
            section = _manager.ApiKeysData.sections.FirstOrDefault(s => s.sectionName == sectionName);
        }
        else
        {
            section = _manager.ModelData.sections.FirstOrDefault(s => s.sectionName == sectionName);
        }
        
        if (section == null)
            return;
            
        var property = section.properties.FirstOrDefault(p => p.key == key);
        if (property == null) return;
        
        EditorGUILayout.BeginHorizontal();
        
        // Special handling for API keys (password field)
        if (key == "apiKey")
        {
            property.value = EditorGUILayout.PasswordField(key, property.value);
        }
        else
        {
            property.value = EditorGUILayout.TextField(key, property.value);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void EnsureResourcesFolderExists()
    {
        string resourcesPath = Application.dataPath + "/Resources";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }
    }
}
#endif
