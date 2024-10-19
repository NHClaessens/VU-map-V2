using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using TMPro;

public class APsInRangePlotter : EditorWindow
{
    private string filePath = "Assets/yourfile.csv";
    private Dictionary<Vector3, List<int>> locationDataDict = new Dictionary<Vector3, List<int>>();

    [MenuItem("Tools/APs in Range Plotter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(APsInRangePlotter));
    }

    private void OnGUI()
    {
        GUILayout.Label("APs in Range Plotter Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("Select CSV File"))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                ParseCSV(filePath);
                PlotLocations();
            }
        }

        GUILayout.Label("CSV File Path:", EditorStyles.label);
        GUILayout.TextField(filePath);

        if (GUILayout.Button("Plot Locations"))
        {
            PlotLocations();
        }
    }

    private void ParseCSV(string path)
    {
        var lines = File.ReadAllLines(path);
        var headers = lines[0].Split(',');

        var apColumns = headers.Skip(3).Where(h => !h.Contains("distance") && !h.Contains("_x") && !h.Contains("_y") && !h.Contains("_z") && !h.Contains("obstacle")).ToArray();

        locationDataDict.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            float x = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
            float y = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            float z = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);

            Vector3 location = new Vector3(x, y, z);

            if (!locationDataDict.ContainsKey(location))
            {
                locationDataDict[location] = new List<int>();
            }

            int count = 0;
            foreach (var apColumn in apColumns)
            {
                int apIndex = Array.IndexOf(headers, apColumn);
                if (values[apIndex] != "1")
                {
                    count++;
                }
            }
            locationDataDict[location].Add(count);
        }
    }

    private void PlotLocations()
    {
        GameObject parent = GameObject.Find("APs in range");

        if (parent)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in parent.transform)
                children.Add(child);

            foreach (Transform child in children)
                DestroyImmediate(child.gameObject);
        }
        else
        {
            parent = new GameObject("APs in range");
        }

        Material sphereMaterial = new Material(Shader.Find("Custom/AlwaysOnTop")) {
            renderQueue = 3000
        };
        Material textMaterial = new Material(Shader.Find("Custom/AlwaysOnTop")) {
            renderQueue = 3000
        };


        foreach (var entry in locationDataDict)
        {
            Vector3 location = entry.Key;
            List<int> apsInRange = entry.Value;

            int minAPs = apsInRange.Count == 0 ? 0 : apsInRange.Min();
            int maxAPs = apsInRange.Count == 0 ? 0 : apsInRange.Max();
            float avgAPs = apsInRange.Count == 0 ? 0f : (float) apsInRange.Average();

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = location;

            float scale = 2f;
            sphere.transform.localScale = new Vector3(scale, scale, scale);
            sphere.transform.SetParent(parent.transform);
            sphere.GetComponent<MeshRenderer>().sharedMaterial = sphereMaterial;

            GameObject text = new GameObject("LocationText");
            text.transform.localScale = new Vector3(1 / scale, 1 / scale, 1 / scale);
            text.transform.eulerAngles = new Vector3(90, 0, 0);
            // text.transform.position = sphere.transform.position + Vector3.up * 0.6f;
            TextMeshPro textMesh = text.AddComponent<TextMeshPro>();
            Debug.Log($"textmesh {textMesh}");
            textMesh.text = $"Min: {minAPs}\nMax: {maxAPs}\nAvg: {avgAPs:F2}";
            textMesh.fontSize = 10;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;

            Renderer textRenderer = textMesh.GetComponent<Renderer>();
            textRenderer.sortingLayerName = "UI";  // Use an existing sorting layer name or create a new one
            textRenderer.sortingOrder = 10;  // Set a high value to ensure it renders on top
            textRenderer.sharedMaterial.renderQueue = 4000;

            text.transform.SetParent(sphere.transform, false);


        }
    }
}

public class LocationData
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }
    public int MinAPs { get; private set; }
    public int MaxAPs { get; private set; }
    public float AvgAPs { get; private set; }

    public LocationData(float x, float y, float z, int minAPs, int maxAPs, float avgAPs)
    {
        X = x;
        Y = y;
        Z = z;
        MinAPs = minAPs;
        MaxAPs = maxAPs;
        AvgAPs = avgAPs;
    }
}
