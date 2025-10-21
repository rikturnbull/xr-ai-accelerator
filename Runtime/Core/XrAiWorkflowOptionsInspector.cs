using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using XrAiAccelerator;

public class XrAiWorkflowOptionsInspector : MonoBehaviour
{
    private Dictionary<string, List<XrAiOptionAttribute>> _options;
    private string _providerName;

    public void Init(string providerName, Dictionary<string, List<XrAiOptionAttribute>> options)
    {
        _providerName = providerName;
        _options = options;
    }

    public Dictionary<string, List<XrAiOptionAttribute>> GetProperties()
    {
        return _options;
    }

    public string GetProviderName()
    {
        return _providerName;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(XrAiWorkflowOptionsInspector))]
public class XrAiWorkflowOptionsInspectorEditor : Editor
{
    private XrAiWorkflowOptionsInspector _workflowOptions;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        _workflowOptions = (XrAiWorkflowOptionsInspector)target;

        GUIStyle wrappedLabelStyle = new(EditorStyles.label)
        {
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Provider Name", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(_workflowOptions.GetProviderName() ?? "<not initialized>");
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(5);

        Dictionary<string, List<XrAiOptionAttribute>> options = _workflowOptions.GetProperties();
        
        if (options == null || options.Count == 0)
        {
            EditorGUILayout.HelpBox("No properties available. This component needs to be initialized.", MessageType.Info);
            return;
        }

        foreach (var option in options)
        {
            IEnumerable<XrAiOptionAttribute> globalAttributes = option.Value.Where(attr => attr.Scope == XrAiOptionScope.Global);
            IEnumerable<XrAiOptionAttribute> workflowAttributes = option.Value.Where(attr => attr.Scope == XrAiOptionScope.Workflow);

            DrawOptionTable(globalAttributes.ToList(), "Global Properties", wrappedLabelStyle);
            DrawOptionTable(workflowAttributes.ToList(), "Workflow Properties", wrappedLabelStyle);
        }
    }

    private void DrawHorizontalRule()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        EditorGUILayout.Space(5);
    }

    private void DrawOptionTable(List<XrAiOptionAttribute> attributes, string title, GUIStyle wrappedLabelStyle)
    {
        if (attributes == null || attributes.Count == 0) return;

        DrawHorizontalRule();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(150));
        // EditorGUILayout.LabelField("Required", EditorStyles.boldLabel, GUILayout.Width(60));
        EditorGUILayout.LabelField("Default", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        foreach (var attribute in attributes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(attribute.Key, EditorStyles.boldLabel, GUILayout.Width(150));
            // GUILayout.Space(20);
            // EditorGUILayout.Toggle(attribute.IsRequired, GUILayout.Width(20));
            // GUILayout.Space(20);
            EditorGUILayout.LabelField(attribute.DefaultValue ?? "<empty>", wrappedLabelStyle, GUILayout.Width(180));
            EditorGUILayout.LabelField(attribute.Description?.ToString(), wrappedLabelStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndVertical();
    }
}
#endif