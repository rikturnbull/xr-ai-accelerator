using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace XrAiAccelerator
{
    [CreateAssetMenu(fileName = "XrAiSecretsManager", menuName = "XR AI Accelerator/Secrets Manager")]
    public class XrAiSecretsManager : ScriptableObject
    {
        private const string SECRETS_FILE_PATH = "XrAiSecrets";
        private Dictionary<string,string> _secrets = new();
        private static XrAiSecretsManager _instance;

        void Awake()
        {
            LoadFromFile();
        }

        public void LoadFromFile()
        {
            TextAsset configFile = Resources.Load<TextAsset>(SECRETS_FILE_PATH);
            if (configFile != null)
            {
                try
                {
                    _secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(configFile.text);
                    if (_secrets == null)
                    {
                        _secrets = new Dictionary<string, string>();
                    }
                }
                catch
                {
                    _secrets = new Dictionary<string, string>();
                }
            }
        }

        public void SaveToFile()
        {
            string secretsJson = JsonConvert.SerializeObject(_secrets, Formatting.Indented);
            string secretsFilePath = Application.dataPath + "/Resources/" + SECRETS_FILE_PATH + ".txt";
            System.IO.File.WriteAllText(secretsFilePath, secretsJson);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public Dictionary<string, string> GetSecrets()
        {
            return _secrets;
        }

        public void AddSecret(string name, string value = "")
        {
            if (!string.IsNullOrEmpty(name) && !_secrets.ContainsKey(name))
            {
                _secrets[name] = value;
            }
        }

        public string GetSecret(string name)
        {
            if (_secrets.ContainsKey(name))
            {
                return _secrets[name];
            }
            return null;
        }

        public void RemoveSecret(string name)
        {
            if (_secrets.ContainsKey(name))
            {
                _secrets.Remove(name);
            }
        }

        public static XrAiSecretsManager GetSecretsManager()
        {
            if (_instance == null)
            {
                _instance = Resources.Load<XrAiSecretsManager>("XrAiSecretsManager");
                if (_instance == null)
                {
                    Debug.LogError("XrAiSecretsManager not found in Resources.");
                }
                _instance.LoadFromFile();
            }
            return _instance;
        }
    }
}
