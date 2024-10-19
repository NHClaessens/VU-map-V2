using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CSVImporter: EditorWindow
{
    private CSVImporterSettings settings;
    private CSVImporterSettings items;
    private Vector2 scrollPosition;
    private Material newMat;
    private float rotationAngle;

    [MenuItem("Tools/CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CSVImporter), false, "CSV Importer");
    }

    private void OnEnable()
    {
        settings = AssetDatabase.LoadAssetAtPath<CSVImporterSettings>("Assets/Editor/CSVImporterSettings.asset");
        if (settings == null)
        {
            settings = CreateInstance<CSVImporterSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Editor/CSVImporterSettings.asset");
            AssetDatabase.SaveAssets();
        }
        newMat = new Material(Shader.Find("Standard"));
        newMat.color = Color.yellow;
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        SerializedObject serializedSettings = new SerializedObject(settings);

        // if(GUILayout.Button("Add Item")) {
        //     settings.items.Add(new CSVImportItem());
        // }

        EditorGUILayout.PropertyField(serializedSettings.FindProperty("items"), new GUIContent("Items"), true);

        
        serializedSettings.ApplyModifiedProperties();

        if (GUILayout.Button("Create Points"))
        {
            CreatePointsFromCSVs();
        }

        EditorGUILayout.EndScrollView();
    }

    private void CreatePointsFromCSVs()
    {
        foreach(CSVImportItem item in settings.items) {
        
            GameObject parentObject = GameObject.Find(item.parentObjectName);


            if (parentObject != null)
            {
                // Create a list to hold all children temporarily
                List<GameObject> children = new List<GameObject>();

                // Populate the list with all child game objects
                foreach (Transform child in parentObject.transform)
                {
                    children.Add(child.gameObject);
                }

                // Destroy all child game objects collected in the list
                foreach (GameObject child in children)
                {
                    DestroyImmediate(child);  // Using DestroyImmediate because you mentioned it, typical for editor scripts
                }
            }
            else
            {
                parentObject = new GameObject(item.parentObjectName);
            }

            foreach (TextAsset csvFile in item.csvFiles)
            {
                if (csvFile != null)
                    CreatePointsFromCSV(csvFile.text, csvFile.name, item);
            }
        }
        RotatePoints();
    }

    private void CreatePointsFromCSV(string csvText, string fileName, CSVImportItem item)
    {
        string[] lines = csvText.Split('\n');
        GameObject parentObject = GameObject.Find(item.parentObjectName);

        Material material = new Material(Shader.Find("Standard"))
        {
            color = item.color
        };

        // Find column indices based on headers
        string[] headers = lines[0].Split(',');
        int xColumn = FindColumnIndex(headers, item.xColumnName);
        int yColumn = FindColumnIndex(headers, item.yColumnName);
        int zColumn = FindColumnIndex(headers, item.zColumnName);
        int nameColumn = FindColumnIndex(headers, item.nameColumnName);

        Debug.Log(xColumn + " " + yColumn + " " +  zColumn + " " + nameColumn);

        Dictionary<float, float> hrep = new Dictionary<float, float>();

        foreach(string s in item.heightReplacements) {
            string[] parts = s.Split(",");
            hrep.Add(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
            Debug.Log(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture) + " " + float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
        }

        for (int i = 1; i < lines.Length; i++) // Skip the header row
        {
            string[] tokens = lines[i].Split(',');
            if (tokens.Length > Mathf.Max(xColumn, yColumn, zColumn) && xColumn != -1 && yColumn != -1 && zColumn != -1)
            {
                float rawX = float.Parse(tokens[xColumn]);
                float rawY = float.Parse(tokens[yColumn]);
                float rawZ = float.Parse(tokens[zColumn]);

                if(hrep.ContainsKey(rawY)) rawY = hrep.GetValueOrDefault(rawY);

                float x = rawX * item.xMultiplier + item.xAddition;
                float y = rawY * item.yMultiplier + item.yAddition;
                float z = rawZ * item.zMultiplier + item.zAddition;

                
                Vector3 position = new Vector3(x, y, z);
                GameObject sphere;
                if(item.nameColumnName != null && item.nameColumnName.Length > 0)
                    sphere = CreatePoint(position, parentObject.transform, material, tokens[nameColumn]);
                else
                    sphere = CreatePoint(position, parentObject.transform, material, "");

                PositionData data = sphere.AddComponent<PositionData>();
                data.x = rawX;
                data.y = rawY;
                data.z = rawZ;

                SerializableDictionary<string, string> dict = new SerializableDictionary<string, string>();
                for(int j = 0; j < tokens.Length; j++) {
                    if(headers[j] == "x" || headers[j] == "y" || headers[j] == "z") continue;
                    dict.Add(headers[j], tokens[j]);
                }

                data.rssi = dict;
            }
        }
    }

    private int FindColumnIndex(string[] headers, string columnName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1; // Return -1 if column name not found
    }

    private GameObject CreatePoint(Vector3 position, Transform parent, Material material, string name)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.SetParent(parent, false);

        if(name != null && name.Length > 0) sphere.name = name;

        
        sphere.GetComponent<MeshRenderer>().material = material;
        DestroyImmediate(sphere.GetComponent<SphereCollider>());
        return sphere;
    }

    private void RotatePoints()
    {
        // if(rotationAngle == 0) return;
        // if (!settings.csvFiles.Length.Equals(0))
        // {
        //     GameObject parentObject = GameObject.Find(settings.parentObjectName);
        //     if (parentObject != null)
        //     {
        //         Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
        //         RotateAroundCenter(parentObject, rotation);
        //         // AlignWithAxes(parentObject);
        //     }
        //     else
        //     {
        //         EditorUtility.DisplayDialog("Error", "No parent object found with the name '" + settings.parentObjectName + "'.", "OK");
        //     }
        // }
        // else
        // {
        //     EditorUtility.DisplayDialog("Error", "No CSV files are loaded to define points.", "OK");
        // }
    }

    private void RotateAroundCenter(GameObject parentObject, Quaternion rotation)
    {
        Vector3 center = CalculateCentroid(parentObject);
        foreach (Transform child in parentObject.transform)
        {
            Vector3 direction = child.position - center;
            child.position = center + rotation * direction;
            child.rotation = rotation * child.rotation;
        }
    }

    private void AlignWithAxes(GameObject parentObject)
    {
        if (parentObject.transform.childCount == 0)
            return;

        float minX = float.MaxValue;
        float minZ = float.MaxValue;

        // Find minimum x and z
        foreach (Transform child in parentObject.transform)
        {
            if (child.position.x < minX)
                minX = child.position.x;
            if (child.position.z < minZ)
                minZ = child.position.z;
        }

        Vector3 offset = new Vector3(-minX, 0, -minZ);

        // Apply offset to all children to align bottom and leftmost to the axis
        foreach (Transform child in parentObject.transform)
        {
            child.position += offset;
        }
    }

    private Vector3 CalculateCentroid(GameObject parentObject)
    {
        Vector3 centroid = Vector3.zero;
        int count = parentObject.transform.childCount;
        foreach (Transform child in parentObject.transform)
        {
            centroid += child.position;
        }
        if (count > 0) centroid /= count;
        return centroid;
    }
}
