using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace XrAiAccelerator
{
    public class XrAiWorkflowInspectorPanel : MonoBehaviour
    {

        [SerializeField]
        private List<XrAiWorkflowInspectorOption> _workflowInspectorOptions = new();

        [SerializeField]
        private string _providerName;

        public void Init(string providerName, List<XrAiOptionAttribute> options)
        {
            _providerName = providerName;

            _workflowInspectorOptions.Clear();
            foreach (var option in options)
            {
                if (option.Key == "apiKey") continue;
                _workflowInspectorOptions.Add(new XrAiWorkflowInspectorOption
                {
                    Key = option.Key,
                    Scope = option.Scope,
                    IsRequired = option.IsRequired,
                    DefaultValue = option.DefaultValue,
                    Description = option.Description,
                    Value = option.DefaultValue
                });
            }
        }

        public string GetProviderName()
        {
            return _providerName;
        }

        public List<XrAiWorkflowInspectorOption> GetWorkflowInspectorOptions()
        {
            return _workflowInspectorOptions;
        }

        public void SetOptionValue(string key, string value)
        {
            foreach (var option in _workflowInspectorOptions)
            {
                if (option.Key == key)
                {
                    option.Value = value;
                    break;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(XrAiWorkflowInspectorPanel))]
    public class XrAiWorkflowInspectorPanelEditor : Editor
    {
        private XrAiWorkflowInspectorPanel _workflowInspectorPanel;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _workflowInspectorPanel = (XrAiWorkflowInspectorPanel)target;

            GUIStyle wrappedLabelStyle = new(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Provider Name", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_workflowInspectorPanel.GetProviderName() ?? "<not initialized>");
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(5);

            List<XrAiWorkflowInspectorOption> workflowInspectorOptions = _workflowInspectorPanel.GetWorkflowInspectorOptions();
            if (workflowInspectorOptions == null || workflowInspectorOptions.Count == 0)
            {
                EditorGUILayout.HelpBox("No properties available.", MessageType.Info);
                return;
            }

            IEnumerable<XrAiWorkflowInspectorOption> globalOptions = workflowInspectorOptions.Where(opt => opt.Scope == XrAiOptionScope.Global);
            IEnumerable<XrAiWorkflowInspectorOption> workflowOptions = workflowInspectorOptions.Where(attr => attr.Scope == XrAiOptionScope.Workflow);

            DrawOptionTable(globalOptions.ToList(), "Global Properties", wrappedLabelStyle);
            DrawOptionTable(workflowOptions.ToList(), "Workflow Properties", wrappedLabelStyle);

            // serializedObject.ApplyModifiedProperties();
        }

        private void DrawHorizontalRule()
        {
            EditorGUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(5);
        }

        private void DrawOptionTable(List<XrAiWorkflowInspectorOption> options, string title, GUIStyle wrappedLabelStyle)
        {
            if (options == null || options.Count == 0) return;

            DrawHorizontalRule();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField("Default", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            foreach (var option in options)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(option.Key, EditorStyles.boldLabel, GUILayout.Width(150));
                EditorGUI.BeginChangeCheck();
                string currentValue = option.Value ?? "";
                string newValue = EditorGUILayout.TextField(currentValue, GUILayout.Width(180));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_workflowInspectorPanel, "Change Option Value");
                    _workflowInspectorPanel.SetOptionValue(option.Key, newValue);
                    EditorUtility.SetDirty(_workflowInspectorPanel);
                }

                EditorGUILayout.LabelField(option.Description?.ToString(), wrappedLabelStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }
    }
#endif
}