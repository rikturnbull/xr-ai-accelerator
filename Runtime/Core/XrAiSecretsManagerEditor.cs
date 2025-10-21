#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace XrAiAccelerator
{
    [CustomEditor(typeof(XrAiSecretsManager))]
    public class XrAiSecretsManagerEditor : Editor
    {
        private XrAiSecretsManager _manager;
        private string _newSecretName = "";
        private string _lastFocusedControl = "";

        void OnEnable()
        {
            _manager = (XrAiSecretsManager)target;
            _manager.LoadFromFile();
        }

        public override void OnInspectorGUI()
        {
            Dictionary<string, string> secrets = _manager.GetSecrets();

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S && (e.control || e.command))
            {
                SaveToFile();
            }

            EditorGUILayout.LabelField("Secrets", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var secretsToRemove = new List<string>();
            foreach (var secretName in secrets.Keys.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                
                string controlName = "secret_" + secretName;
                GUI.SetNextControlName(controlName);
                
                string newValue = EditorGUILayout.PasswordField(secretName, secrets[secretName]);
                secrets[secretName] = newValue;
                
                if (_lastFocusedControl == controlName && GUI.GetNameOfFocusedControl() != controlName)
                {
                    SaveToFile();   
                }
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    secretsToRemove.Add(secretName);
                }
                EditorGUILayout.EndHorizontal();
            }
            _lastFocusedControl = GUI.GetNameOfFocusedControl();
            
            foreach (var secretName in secretsToRemove)
            {
                _manager.RemoveSecret(secretName);
                SaveToFile();
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add New Secret:", GUILayout.Width(100));
            _newSecretName = EditorGUILayout.TextField(_newSecretName);
            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                if (!string.IsNullOrEmpty(_newSecretName))
                {
                    _manager.AddSecret(_newSecretName, "");
                    _newSecretName = "";
                    SaveToFile();
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
        }

        private void SaveToFile()
        {
            string resourcesPath = Application.dataPath + "/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
                AssetDatabase.Refresh();
            }
            _manager.SaveToFile();
            EditorUtility.SetDirty(_manager);
        }
    }
}
#endif
