using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static SerializableDictionary<(Vector3, Vector3), RaycastHit[]> raycastCache = new SerializableDictionary<(Vector3, Vector3), RaycastHit[]>();
    public static RaycastHit[] RaycastAll(Vector3 start, Vector3 direction, float distance, int layerMask) {
        RaycastHit[] hits = null;
        raycastCache.TryGetValue((start, direction), out hits);

        if(hits == null) {
            // Debug.Log($"Cache miss {start} {direction} {distance}");
            hits = Physics.RaycastAll(start, direction, distance, layerMask);
            raycastCache.Add((start, direction), hits);
        }

        return hits;
    }

    public static List<float> ApObstacles(List<GameObject> APs, Vector3 from) {
        List<float> results = new List<float>();
        foreach(GameObject AP in APs) {
            results.Add(CalculateObstacleThickness(from, AP.transform.position));
            // Debug.Log("Completed AP " + AP.name);
        }

        return results;
    }

        

    public static List<GameObject> GetAllBottomLevelGameObjects(GameObject root)
    {
        List<GameObject> bottomLevelGameObjects = new List<GameObject>();
        FindBottomLevelGameObjects(root, bottomLevelGameObjects);
        return bottomLevelGameObjects;
    }
    private static void FindBottomLevelGameObjects(GameObject current, List<GameObject> bottomLevelGameObjects)
    {
        if (current.transform.childCount == 0)
        {
            bottomLevelGameObjects.Add(current);
        }
        else
        {
            foreach (Transform child in current.transform)
            {
                FindBottomLevelGameObjects(child.gameObject, bottomLevelGameObjects);
            }
        }
    }

    public static float CalculateObstacleThickness(Vector3 start, RaycastHit[] forwardHits, RaycastHit[] reverseHits) {
        List<(RaycastHit, string)> allHits = new List<(RaycastHit, string)>();
        // forwardHits.CopyTo(allHits, 0);
        // reverseHits.CopyTo(allHits, forwardHits.Length);
        foreach (var hit in forwardHits)
        {
            allHits.Add((hit, "forward"));
        }
        foreach (var hit in reverseHits)
        {
            allHits.Add((hit, "reverse"));
        }

        (RaycastHit, string)[] arr = allHits.ToArray();

        System.Array.Sort(arr, (hit1, hit2) => Vector3.Distance(start, hit1.Item1.point).CompareTo(Vector3.Distance(start, hit2.Item1.point)));

        float totalThickness = 0f;
        int enterCount = 0;
        Vector3 firstEnter = Vector3.zero;

        // Calculate cumulative thickness based on entry and exit points
        for(int i = 0; i < arr.Length; i++) {
            var item = arr[i];
            RaycastHit hit = item.Item1;
            if(item.Item2 == "forward") {
                if(enterCount == 0) {
                    firstEnter = hit.point;
                }
                enterCount++;
            } else {
                enterCount--;

                if(enterCount == 0) {
                    totalThickness += Vector3.Distance(firstEnter, hit.point);
                }
            }

        }

        return totalThickness;
    }

    public static float CalculateObstacleThickness(Vector3 start, Vector3 end, string layers = "")
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // Setup the layer mask
                int layerMask = 0;
        if (!string.IsNullOrEmpty(layers))
        {
            string[] layerNames = layers.Split(',');
            foreach (string layerName in layerNames)
            {
                int layer = LayerMask.NameToLayer(layerName.Trim());
                if (layer != -1)  // Layer exists
                {
                    layerMask |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"Layer '{layerName}' does not exist.");
                }
            }
        }
        else
        {
            layerMask = Physics.DefaultRaycastLayers;
        }

        // Perform raycast in the forward direction
        RaycastHit[] forwardHits = RaycastAll(start, direction, distance, layerMask);
        // Perform raycast in the reverse direction
        RaycastHit[] reverseHits = RaycastAll(end, -direction, distance, layerMask);

        // Combine and sort hits by distance
        List<(RaycastHit, string)> allHits = new List<(RaycastHit, string)>();
        // forwardHits.CopyTo(allHits, 0);
        // reverseHits.CopyTo(allHits, forwardHits.Length);
        foreach (var hit in forwardHits)
        {
            allHits.Add((hit, "forward"));
        }
        foreach (var hit in reverseHits)
        {
            allHits.Add((hit, "reverse"));
        }

        (RaycastHit, string)[] arr = allHits.ToArray();

        System.Array.Sort(arr, (hit1, hit2) => Vector3.Distance(start, hit1.Item1.point).CompareTo(Vector3.Distance(start, hit2.Item1.point)));

        float totalThickness = 0f;
        int enterCount = 0;
        Vector3 firstEnter = Vector3.zero;

        // Calculate cumulative thickness based on entry and exit points
        for(int i = 0; i < arr.Length; i++) {
            var item = arr[i];
            RaycastHit hit = item.Item1;
            if(item.Item2 == "forward") {
                if(enterCount == 0) {
                    firstEnter = hit.point;
                }
                enterCount++;
            } else {
                enterCount--;

                if(enterCount == 0) {
                    totalThickness += Vector3.Distance(firstEnter, hit.point);
                }
            }

        }

        return totalThickness;
    }

    public static int CountObstacles(Vector3 start, Vector3 end, string layers = "")
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // Setup the layer mask
                int layerMask = 0;
        if (!string.IsNullOrEmpty(layers))
        {
            string[] layerNames = layers.Split(',');
            foreach (string layerName in layerNames)
            {
                int layer = LayerMask.NameToLayer(layerName.Trim());
                if (layer != -1)  // Layer exists
                {
                    layerMask |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"Layer '{layerName}' does not exist.");
                }
            }
        }
        else
        {
            layerMask = Physics.DefaultRaycastLayers;
        }

        // Perform raycast in the forward direction
        RaycastHit[] forwardHits = RaycastAll(start, direction, distance, layerMask);
        return forwardHits.Length;
    }

    public static int namesToLayerMask(string[] layerNames) {
        int layerMask = 0;
        foreach (string layerName in layerNames)
        {
            int layer = LayerMask.NameToLayer(layerName.Trim());
            if (layer != -1)  // Layer exists
            {
                layerMask |= (1 << layer);
            }
            else
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist.");
            }
        }

        return layerMask;
    }

    public static bool ObstacleOnPath(Vector3 start, Vector3 end, string layers = "") {
        return CountObstacles(start, end, layers) > 0;
    }

    public static void SaveToFile(string content, string fileName) {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, content);
        print($"Saved {fileName} to {path}");
    }
    public static void SaveToFile(List<string> content, string fileName) {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllLines(path, content);
        print($"Saved {fileName} to {path}");
    }

    public static string ConvertDictionaryToString(Dictionary<string, SerializableList<string>> dictionary)
    {
        StringBuilder csvBuilder = new StringBuilder();

        // Add headers
        csvBuilder.Append(string.Join(',', dictionary.Keys));
        csvBuilder.AppendLine();
        // Determine the maximum number of rows in the dictionary
        int maxRowCount = 0;
        foreach (var list in dictionary.Values)
        {
            maxRowCount = Mathf.Max(maxRowCount, list.Count);
        }

        // Add rows
        for (int i = 0; i < maxRowCount; i++)
        {
            for(int j = 0; j < dictionary.Keys.Count; j++) {
                var header = dictionary.Keys.ElementAt(j);

                if (dictionary[header].Count > i)
                {
                    csvBuilder.Append(EscapeString(dictionary[header][i]));
                    
                }
                if(j < dictionary.Keys.Count - 1)
                    csvBuilder.Append(',');
            }
           
            csvBuilder.AppendLine();
        }

        return csvBuilder.ToString();
    }

    public static List<string> ConvertDictionaryToCsvLines(Dictionary<string, SerializableList<string>> dictionary)
    {
        List<string> csvLines = new List<string>();

        // Add headers
        string headerLine = string.Join(",", dictionary.Keys);
        csvLines.Add(headerLine);

        // Determine the maximum number of rows in the dictionary
        int maxRowCount = dictionary.Values.Max(list => list.Count);

        // Add rows
        for (int i = 0; i < maxRowCount; i++)
        {
            List<string> row = new List<string>();
            foreach (var header in dictionary.Keys)
            {
                if (dictionary[header].Count > i)
                {
                    row.Add(EscapeString(dictionary[header][i]));
                }
                else
                {
                    row.Add(""); // Adding an empty string for missing values
                }
            }

            string line = string.Join(",", row);
            csvLines.Add(line);
        }

        return csvLines;
    }


    public static void WriteCsvToFile(string filePath, List<string> csvLines)
    {
        try
        {
            File.WriteAllLines(filePath, csvLines);
            Debug.Log("CSV file saved successfully.");
        }
        catch (IOException e)
        {
            Debug.Log("An error occurred while writing to the file:");
            Debug.Log(e.Message);
        }
    }

    private static string EscapeString(string input)
    {
        // If the input string contains a comma, double quotes, or newline characters, enclose it in double quotes and double any existing double quotes
        if (input.Contains(",") || input.Contains("\"") || input.Contains("\n") || input.Contains("\r"))
        {
            return "\"" + input.Replace("\"", "\"\"") + "\"";
        }
        else
        {
            return input;
        }
    }

    public static void SelectFloor(string floor) {
        GameObject models = GameObject.Find("3D models");

        foreach(Transform child in models.transform) {
            if(child.name == floor) child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }
    }

    public static void SelectFloor(int floor) {
        SelectFloor("F"+floor);
    }

        // Takes a list of GameObjects and sorts them based on the minimal walking distance using a simple heuristic
    public static List<GameObject> SortGameObjects(List<GameObject> gameObjects)
    {
        if (gameObjects == null || gameObjects.Count <= 1)
        {
            return gameObjects;
        }

        List<GameObject> sortedList = new List<GameObject>();
        GameObject current = gameObjects[0];
        sortedList.Add(current);
        gameObjects.RemoveAt(0);

        // Nearest neighbor heuristic
        while (gameObjects.Count > 0)
        {
            GameObject nearest = FindNearestGameObject(current, gameObjects);
            sortedList.Add(nearest);
            gameObjects.Remove(nearest);
            current = nearest;
        }

        return sortedList;
    }

    // Finds the nearest GameObject from the current one
    public static GameObject FindNearestGameObject(GameObject current, List<GameObject> gameObjects)
    {
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in gameObjects)
        {
            float dist = Vector3.Distance(current.transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = obj;
            }
        }

        return nearest;
    }

    public static List<GameObject> GetAllChildren(GameObject obj)
    {
        Transform transform = obj.transform;
        List<GameObject> children = new List<GameObject>();

        // Loop through all child Transforms and add their GameObjects to the list
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        return children;
    }

    public static List<GameObject> FindTopLevelGameObjectsByPattern(string pattern)
    {
        Regex regex = new Regex(pattern);
        List<GameObject> matchingObjects = new List<GameObject>();

        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (regex.IsMatch(obj.name))
            {
                matchingObjects.Add(obj);
            }
        }

        return matchingObjects;
    }

    public static List<string> FindTopLevelGameObjectNamesByPattern(string pattern)
    {
        Regex regex = new Regex(pattern);
        List<string> matchingObjects = new List<string>();

        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (regex.IsMatch(obj.name))
            {
                matchingObjects.Add(obj.name);
            }
        }

        return matchingObjects;
    }

    public static void DisableTopLevelMatchingPattern(string pattern)
    {
        Regex regex = new Regex(pattern);

        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (regex.IsMatch(obj.name))
            {
                obj.SetActive(false);
            }
        }

    }

    public static List<List<T>> GetAllCombinations<T>(List<T> list)
    {
        List<List<T>> result = new List<List<T>>();
        int combinationCount = 1 << list.Count; // 2^n combinations

        for (int i = 1; i < combinationCount; i++) // Start at 1 to avoid the empty set
        {
            List<T> combination = new List<T>();
            for (int j = 0; j < list.Count; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    combination.Add(list[j]);
                }
            }
            result.Add(combination);
        }

        return result;
    }

    public static void DeleteAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public static int HeightToFloor(float height) {
        if(height > 46.8) return 12;
        else if(height > 43.15) return 11;
        else if(height > 39.4) return 10;
        else if(height > 35.7) return 9;
        else if(height > 32) return 8;
        else if(height > 27.7) return 7;
        else if(height > 24.6) return 6;
        else if(height > 20.7) return 5;
        else if(height > 16.8) return 4;
        else if(height > 13) return 3;
        else if(height > 9) return 2;
        else if(height > 4.6) return 1;
        else return 0;
    }

    public static void TakeScreenshot(string name)
    {
        // Set up the RenderTexture
        Camera screenshotCamera = GameObject.Find("Screenshot Camera").GetComponent<Camera>();
        int width = 7300;//6500;
        int height = 7300;
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        screenshotCamera.targetTexture = renderTexture;
        screenshotCamera.clearFlags = CameraClearFlags.SolidColor;
        screenshotCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background

        // Render the camera
        screenshotCamera.Render();

        // Read pixels from the RenderTexture
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Reset the RenderTexture and active texture
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Save the texture to a PNG file
        byte[] bytes = screenshot.EncodeToPNG();


        File.WriteAllBytes(Application.dataPath + $"/../{name}".ToString() + ".png", bytes);

        Debug.Log($"Screenshot saved");
    }

}
