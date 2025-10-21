using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using XrAiAccelerator;

public class XrAiWorkflowInspector : MonoBehaviour
{

}

#if UNITY_EDITOR
[CustomEditor(typeof(XrAiWorkflowInspector))]
public class XrAiWorkflowInspectorEditor : Editor
{
    [InitializeOnLoadMethod]
    private static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += () =>
        {
            XrAiWorkflowInspector[] inspectors = FindObjectsByType<XrAiWorkflowInspector>(FindObjectsSortMode.None);
            foreach (var inspector in inspectors)
            {
                RefreshInspector(inspector);
            }
        };
    }

    private static void RefreshInspector(XrAiWorkflowInspector manager)
    {
        if (manager == null) return;
        
        ClearChildProperties(manager);
        DiscoverAndCreateProperties(manager);
    }

    private void OnEnable()
    {
        XrAiWorkflowInspector manager = (XrAiWorkflowInspector)target;
        RefreshInspector(manager);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        XrAiWorkflowInspector manager = (XrAiWorkflowInspector)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Workflow Properties"))
        {
            ClearChildProperties(manager);
            DiscoverAndCreateProperties(manager);
        }
    }

    private static void DiscoverAndCreateProperties(XrAiWorkflowInspector manager)
    {        
        List<Type> types = XrAiFactory.XrAiInterfaces.Values.ToList();
        types = types.OrderBy(t => t.Name).ToList();
        
        foreach (Type type in types)
        {
            Debug.Log($"Discovered type: {type.FullName}");
            GameObject typeGameObject = new GameObject(type.Name);
            typeGameObject.transform.parent = manager.transform;
            Dictionary<string, Type> providers = XrAiFactory.GetImplementationsForType(type);
            foreach (var provider in providers)
            {
                Debug.Log($"Provider: {provider.Key}, Type: {provider.Value.FullName}");
                Dictionary<string, List<XrAiOptionAttribute>> properties = XrAiFactory.GetAllProviderOptions(provider.Value);
                
                GameObject go = new GameObject($"{provider.Key}");
                go.transform.parent = typeGameObject.transform;
                
                XrAiWorkflowOptionsInspector workflowProperties = go.AddComponent<XrAiWorkflowOptionsInspector>();
                workflowProperties.Init(provider.Key, properties);
                
                Undo.RegisterCreatedObjectUndo(go, "Create Workflow Properties");
            }
        }
        
        EditorUtility.SetDirty(manager);
    }

    private static void ClearChildProperties(XrAiWorkflowInspector manager)
    {
        int childCount = manager.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(manager.transform.GetChild(i).gameObject);
        }
        Debug.Log($"Cleared {childCount} child properties.");
        EditorUtility.SetDirty(manager);
    }
}
#endif