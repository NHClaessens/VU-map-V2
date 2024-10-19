using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MaterialAssignerEditor : EditorWindow
{
    private const string PrefKey = "MaterialAssignerData";
    private List<string> keys = new List<string>();
    private List<Color> colors = new List<Color>();

    private List<Material> createdMaterials = new List<Material>();

    [MenuItem("Tools/Material Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<MaterialAssignerEditor>("Material Assigner");
        window.LoadData();
    }

    void OnGUI()
    {
        GUILayout.Label("Material Color Assignments", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Color Assignment"))
        {
            keys.Add("");
            colors.Add(Color.white);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            keys[i] = EditorGUILayout.TextField("Key", keys[i]);
            colors[i] = EditorGUILayout.ColorField("Color", colors[i]);

            if (GUILayout.Button("Remove"))
            {
                keys.RemoveAt(i);
                colors.RemoveAt(i);
                // break;  // Exit loop to avoid invalid index access
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Assign Materials"))
        {
            AssignMaterials();
        }

        if (GUI.changed)
        {
            SaveData();
        }
    }

    private void AssignMaterials()
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            // Check if the parent object's name starts with 'F'
            if (obj.name.StartsWith("F"))
            {
                foreach (Transform child in obj.transform)
                {
                    ApplyMaterialToGameObject(child.gameObject);
                }
            }
        }
    }

    private void ApplyMaterialToGameObject(GameObject gameObject)
    {
        
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i].Trim();
            if (string.IsNullOrEmpty(key)) continue;  // Ignore empty keys

            string[] subKeys = key.Split(',');

            Color color = colors[i];
            foreach(string subKey in subKeys) {
                string trimmedSubKey = subKey.Trim();
                if (string.IsNullOrEmpty(trimmedSubKey)) continue;  // Ignore empty sub-keys

                if (gameObject.name.Contains(trimmedSubKey))
                {
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material newMat = new Material(Shader.Find("Standard"));
                        newMat.color = color;
                        
                        // Set material to transparent mode if alpha is less than 1
                        if (color.a < 1.0f)
                        {
                            newMat.SetFloat("_Mode", 3);  // Transparent mode
                            newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            newMat.SetInt("_ZWrite", 0);
                            newMat.DisableKeyword("_ALPHATEST_ON");
                            newMat.DisableKeyword("_ALPHABLEND_ON");
                            newMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            newMat.renderQueue = 3000;
                        }

                        // Apply material to all elements
                        Material[] newMats = new Material[renderer.sharedMaterials.Length];
                        for (int j = 0; j < newMats.Length; j++)
                        {
                            newMats[j] = newMat;
                        }
                        renderer.sharedMaterials = newMats;

                        createdMaterials.Add(newMat); // Store the new material
                    }
                    break;
                }
            }
        }
    }

    void OnDisable()
    {
        // Clean up the created materials
        // foreach (Material mat in createdMaterials)
        // {
        //     if (mat != null)
        //     {
        //         DestroyImmediate(mat);
        //     }
        // }
        // createdMaterials.Clear();
    }

    private void LoadData()
    {
        if (EditorPrefs.HasKey(PrefKey))
        {
            string data = EditorPrefs.GetString(PrefKey);
            EditorData editorData = JsonUtility.FromJson<EditorData>(data);

            keys = editorData.keys.ToList();
            colors = editorData.colors.Select(c => new Color(c.r, c.g, c.b, c.a)).ToList();
        }
    }

    private void SaveData()
    {
        EditorData editorData = new EditorData
        {
            keys = keys.ToArray(),
            colors = colors.Select(c => new SerializableColor(c)).ToArray()
        };

        string data = JsonUtility.ToJson(editorData);
        EditorPrefs.SetString(PrefKey, data);
    }

    [System.Serializable]
    private class EditorData
    {
        public string[] keys;
        public SerializableColor[] colors;
    }

    [System.Serializable]
    private struct SerializableColor
    {
        public float r, g, b, a;

        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public static implicit operator Color(SerializableColor sColor)
        {
            return new Color(sColor.r, sColor.g, sColor.b, sColor.a);
        }

        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color);
        }
    }
}
