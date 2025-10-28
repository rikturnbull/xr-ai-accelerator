using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XrAiAccelerator
{
    public class XrAiWorkflowInspector : MonoBehaviour
    {
        public Dictionary<string, string> GetWorkflowOptions(string workflowId, string providerName)
        {
            Dictionary<string, string> options = new();

            string interfaceName = XrAiFactory.XrAiInterfaces[workflowId].Name;
            Transform typeTransform = transform.Find(interfaceName);
            if (typeTransform == null)
            {
                Debug.LogError($"Type {interfaceName} not found under WorkflowsManager.");
                return options;
            }

            Transform providerTransform = typeTransform.Find(providerName);
            if (providerTransform == null)
            {
                Debug.LogError($"Provider {providerName} not found under type {interfaceName}.");
                return options;
            }

            XrAiWorkflowInspectorPanel workflowInspectorPanel = providerTransform.GetComponent<XrAiWorkflowInspectorPanel>();
            if (workflowInspectorPanel != null)
            {
                List<XrAiWorkflowInspectorOption> workflowInspectorOptions = workflowInspectorPanel.GetWorkflowInspectorOptions();
                foreach (var workflowInspectorOption in workflowInspectorOptions)
                {
                    options[workflowInspectorOption.Key] = workflowInspectorOption.Value;
                }
            }

            return options;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(XrAiWorkflowInspector))]
    public class XrAiWorkflowInspectorEditor : Editor
    {
        private void OnEnable()
        {
            XrAiWorkflowInspector inspector = (XrAiWorkflowInspector)target;
            DiscoverAndCreateProperties(inspector);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            XrAiWorkflowInspector inspector = (XrAiWorkflowInspector)target;

            if (GUILayout.Button("Refresh Workflow Options"))
            {
                DiscoverAndCreateProperties(inspector);
            }
        }

        private static void DiscoverAndCreateProperties(XrAiWorkflowInspector inspector)
        {
            List<Type> types = XrAiFactory.XrAiInterfaces.Values.ToList();
            types = types.OrderBy(t => t.Name).ToList();

            foreach (Type type in types)
            {
                GameObject typeGameObject = FindOrCreateTypeGameObject(inspector, type.Name);
                Dictionary<string, Type> providers = XrAiFactory.GetImplementationsForType(type);
                foreach (var provider in providers)
                {
                    bool reinitialize = false;
                    if (!TryGetProviderGameObject(inspector, typeGameObject.transform, provider.Key, out GameObject providerGameObject))
                    {
                        providerGameObject = new GameObject($"{provider.Key}");
                        providerGameObject.transform.parent = typeGameObject.transform;
                        reinitialize = true;
                    }

                    if (!providerGameObject.TryGetComponent(out XrAiWorkflowInspectorPanel workflowInspectorPanel))
                    {
                        workflowInspectorPanel = providerGameObject.AddComponent<XrAiWorkflowInspectorPanel>();
                        reinitialize = true;
                    }

                    if (reinitialize)
                    {
                        List<XrAiOptionAttribute> properties = XrAiFactory.GetProviderOptions<XrAiOptionAttribute>(provider.Key, provider.Value);
                        workflowInspectorPanel.Init(provider.Key, properties);
                        EditorUtility.SetDirty(inspector);
                        // UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(inspector.gameObject.scene);

                    }

                    // Transform providerTransform = typeGameObject.transform.Find(provider.Key);
                    // GameObject providerGameObject;
                    // XrAiWorkflowInspectorPanel workflowInspectorPanel;

                    // if (providerTransform == null)
                    // {
                    //     providerGameObject = new GameObject($"{provider.Key}");
                    //     providerGameObject.transform.parent = typeGameObject.transform;
                    //     workflowInspectorPanel = providerGameObject.AddComponent<XrAiWorkflowInspectorPanel>();
                    //     List<XrAiOptionAttribute> properties = XrAiFactory.GetProviderOptions<XrAiOptionAttribute>(provider.Key, provider.Value);
                    //     workflowInspectorPanel.Init(provider.Key, properties);
                    // }
                    // else
                    // {
                    //     providerGameObject = providerTransform.gameObject;
                    //     workflowInspectorPanel = providerGameObject.GetComponent<XrAiWorkflowInspectorPanel>();
                    //     if (workflowInspectorPanel == null)
                    //     {
                    //         workflowInspectorPanel = providerGameObject.AddComponent<XrAiWorkflowInspectorPanel>();
                    //         List<XrAiOptionAttribute> properties = XrAiFactory.GetProviderOptions<XrAiOptionAttribute>(provider.Key, provider.Value);
                    //         workflowInspectorPanel.Init(provider.Key, properties);
                    //     }
                    // }
                }
            }
        }

        private static GameObject FindOrCreateTypeGameObject(XrAiWorkflowInspector inspector, string typeName)
        {
            Transform typeTransform = inspector.transform.Find(typeName);
            if (typeTransform == null)
            {
                GameObject typeGameObject = new GameObject(typeName);
                typeGameObject.transform.parent = inspector.transform;
                return typeGameObject;
            }
            else
            {
                return typeTransform.gameObject;
            }
        }

        private static bool TryGetProviderGameObject(XrAiWorkflowInspector inspector, Transform parent, string providerName, out GameObject providerGameObject)
        {
            Transform providerTransform = parent.Find(providerName);
            if (providerTransform == null)
            {
                providerGameObject = null;
                return false;
            }
            else
            {
                providerGameObject = providerTransform.gameObject;
                return true;
            }
        }

        // private static GameObject CreateProviderGameObject(XrAiWorkflowInspector inspector, Transform parent, string providerName, Type providerType)
        // {
        //     Transform providerTransform = parent.Find(providerName);
        //     GameObject providerGameObject;
        //     XrAiWorkflowInspectorPanel workflowInspectorPanel;

        //     if (providerTransform == null)
        //     {
        //         providerGameObject = new GameObject($"{providerName}");
        //         providerGameObject.transform.parent = parent;
        //         workflowInspectorPanel = providerGameObject.AddComponent<XrAiWorkflowInspectorPanel>();
        //         List<XrAiOptionAttribute> properties = XrAiFactory.GetProviderOptions<XrAiOptionAttribute>(providerName, providerType);
        //         workflowInspectorPanel.Init(providerName, properties);
        //     }
        //     else
        //     {
        //         providerGameObject = providerTransform.gameObject;
        //         workflowInspectorPanel = providerGameObject.GetComponent<XrAiWorkflowInspectorPanel>();
        //         if (workflowInspectorPanel == null)
        //         {
        //             workflowInspectorPanel = providerGameObject.AddComponent<XrAiWorkflowInspectorPanel>();
        //             List<XrAiOptionAttribute> properties = XrAiFactory.GetProviderOptions<XrAiOptionAttribute>(providerName, providerType);
        //             workflowInspectorPanel.Init(providerName, properties);
        //         }
        //     }
        // }
    }
    #endif
}