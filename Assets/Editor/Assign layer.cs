using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LayerAssignerEditor : EditorWindow
{
    private const string PrefKey = "LayerAssignerData";
    private List<string> keys = new List<string>();
    private List<int> layers = new List<int>();
    private List<bool> addColliderFlags = new List<bool>(); // New list for collider flags


    [MenuItem("Tools/Layer Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<LayerAssignerEditor>("Layer Assigner");
        window.LoadData();
    }

    void OnGUI()
    {
        GUILayout.Label("Layer Assignments", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Layer Assignment"))
        {
            keys.Add("");
            layers.Add(0); // Default layer
        }

        for (int i = 0; i < keys.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            keys[i] = EditorGUILayout.TextField("Key", keys[i]);
            layers[i] = EditorGUILayout.LayerField("Layer", layers[i]);

            if (addColliderFlags.Count > i) // Check for existing flag
            {
                addColliderFlags[i] = EditorGUILayout.Toggle("Add Collider", addColliderFlags[i]);
            }
            else
            {
                addColliderFlags.Add(false); // Add default value if not existing
            }

            if (GUILayout.Button("Remove"))
            {
                keys.RemoveAt(i);
                layers.RemoveAt(i);
                addColliderFlags.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Assign Layers"))
        {
            AssignLayers();
        }

        if (GUI.changed)
        {
            SaveData();
        }
    }

    private void AssignLayers()
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                string[] subKeys = key.Split(',');
                foreach(string subKey in subKeys) {
                    string trimmedSubKey = subKey.Trim();
                    if (string.IsNullOrEmpty(trimmedSubKey)) continue;  // Ignore empty sub-keys

                    if (obj.name.Contains(trimmedSubKey))
                    {
                        obj.layer = layers[i];
                        // Add Mesh Collider if the flag is set
                        if (addColliderFlags[i])
                        {
                            if (obj.GetComponent<MeshCollider>() == null) // Add only if not already present
                            {
                                obj.AddComponent<MeshCollider>();
                            }
                        } else if(obj.GetComponent<MeshCollider>() != null) {
                            DestroyImmediate(obj.GetComponent<MeshCollider>());
                        }
                    }
                }

            }
        }
    }

    private void LoadData()
    {
        if (EditorPrefs.HasKey(PrefKey))
        {
            string data = EditorPrefs.GetString(PrefKey);
            LayerAssignerData layerAssignerData = JsonUtility.FromJson<LayerAssignerData>(data);

            keys = new List<string>(layerAssignerData.keys);
            layers = new List<int>(layerAssignerData.layers);
            addColliderFlags = new List<bool>(layerAssignerData.addColliderFlags);
        }
    }

    private void SaveData()
    {
        LayerAssignerData layerAssignerData = new LayerAssignerData
        {
            keys = keys.ToArray(),
            layers = layers.ToArray(),
            addColliderFlags = addColliderFlags.ToArray(),
        };

        string data = JsonUtility.ToJson(layerAssignerData);
        EditorPrefs.SetString(PrefKey, data);
    }

    [System.Serializable]
    private class LayerAssignerData
    {
        public string[] keys;
        public int[] layers;
        public bool[] addColliderFlags;
    }
}