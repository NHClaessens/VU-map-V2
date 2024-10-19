using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class CsvPlotterEditor : EditorWindow
{
    private string csvFilePath = "";
    private List<GameObject> plottedObjects = new List<GameObject>();

    [MenuItem("Tools/Csv Plotter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CsvPlotterEditor), false, "CSV Plotter");
    }

    void OnGUI()
    {
        GUILayout.Label("Plot Data from CSV File", EditorStyles.boldLabel);

        if (GUILayout.Button("Load CSV"))
        {
            LoadCsvData();
        }

        if (GUILayout.Button("Clear Points"))
        {
            ClearPlottedPoints();
        }

        csvFilePath = EditorGUILayout.TextField("CSV File Path", csvFilePath);
    }

    private void LoadCsvData()
    {
        string path = EditorUtility.OpenFilePanel("Load CSV File", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            csvFilePath = path;
            string[] lines = File.ReadAllLines(csvFilePath);

            // Assuming CSV format with headers: AP Name,Base Radio MAC Address,Location X,Location Y,Location Z
            string[] headers = lines[0].Split(',');
            int xIndex = System.Array.IndexOf(headers, "Location X");
            int yIndex = System.Array.IndexOf(headers, "Location Y");
            int zIndex = System.Array.IndexOf(headers, "Location Z");

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;

            for (int i = 1; i < lines.Length; i++) // Start at 1 to skip header
            {
                string[] tokens = lines[i].Split(',');
                if (tokens.Length > zIndex) // Ensuring there's enough columns
                {
                    Vector3 position = new Vector3(
                        float.Parse(tokens[xIndex], CultureInfo.InvariantCulture),
                        float.Parse(tokens[yIndex], CultureInfo.InvariantCulture),
                        float.Parse(tokens[zIndex], CultureInfo.InvariantCulture));
                    
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    point.transform.position = position;
                    point.transform.localScale = Vector3.one * 0.5f;  // Small size for the sphere
                    point.GetComponent<MeshRenderer>().material = mat;
                    plottedObjects.Add(point);
                }
            }
        }
    }

    private void ClearPlottedPoints()
    {
        foreach (var obj in plottedObjects)
        {
            DestroyImmediate(obj);
        }
        plottedObjects.Clear();

    }
}
