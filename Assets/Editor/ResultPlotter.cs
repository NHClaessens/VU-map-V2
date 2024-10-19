using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ResultPlotterWindow : EditorWindow
{
    private string csvFilePath;
    private TextAsset file;
    private Dictionary<Vector3, List<Vector3>> dataDictionary = new Dictionary<Vector3, List<Vector3>>();
    private string[] options = new string[]{
        "Arrows",
        "Circles"
    };
    private int selectedOption = 0;
    private bool update = false;
    private float heightOffset = 0;
    private Dictionary<string, bool> previouslyEnabled = new Dictionary<string, bool>();
    private float colorDifferenceThreshold = 30;
    private float proximityThreshold = 30;

    private string ssName;


    // Options for circles
    private float actualSize = 1;
    private float predictedSize = 0.5f;
    private bool circleOutline = false;
    private int numSegments = 100;
    private float outlineThickness = 0.3f;

    // Options for arrows
    private float arrowThickness = 0.3f;



    [MenuItem("Tools/Result Plotter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ResultPlotterWindow));
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Live update");
        update = EditorGUILayout.Toggle(update);
        GUILayout.Label("CSV File Path", EditorStyles.boldLabel);
        if(csvFilePath != "")
            GUILayout.Label(csvFilePath);
        if(GUILayout.Button("Pick file"))
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV", "Assets/Resources/Data", "csv");

        GUILayout.Label("Select visualization method");
        selectedOption = EditorGUILayout.Popup(selectedOption, options);

        if(GUILayout.Button("Visualize results")) {
            VisualizeData();
        }

        if (GUILayout.Button("Clear")) {
            DestroyImmediate(GetParent());
        }

        GUILayout.Label("Screenshot name");
        ssName = GUILayout.TextField(ssName);
        if (GUILayout.Button("Take screenshot")) {
            Utilities.TakeScreenshot(ssName);
        }

        EditorGUILayout.Space(32);

        GUILayout.Label("Actual location radius");
        actualSize = EditorGUILayout.FloatField(actualSize);
        GUILayout.Label("Predicted location radius");
        predictedSize = EditorGUILayout.FloatField(predictedSize);
        GUILayout.Label("Distance threshold");
        proximityThreshold = EditorGUILayout.FloatField(proximityThreshold);
        GUILayout.Label("Color threshold");
        colorDifferenceThreshold = EditorGUILayout.FloatField(colorDifferenceThreshold);
        GUILayout.Label("Outline thickness");
        outlineThickness = EditorGUILayout.FloatField(outlineThickness);
        GUILayout.Label("Height offset");
        heightOffset = EditorGUILayout.FloatField(heightOffset);

        EditorGUILayout.Space(32);

        if(options[selectedOption] == "Circles") {
            GUILayout.Label("Circle outline");
            circleOutline = EditorGUILayout.Toggle(circleOutline);
            if(circleOutline) {
                GUILayout.Label("Outline segments");
                numSegments = EditorGUILayout.IntField(numSegments);
            }
        }

        if(options[selectedOption] == "Arrows") {
            GUILayout.Label("Arrow thickness");
            arrowThickness = EditorGUILayout.FloatField(arrowThickness);
        }







        if(EditorGUI.EndChangeCheck()) {
            if(update) {
                GetPreviouslyEnabled();
                VisualizeData();
                SetPreviouslyEnabled();
            }
        }
    }

    void LoadCSVData(string filePath)
    {
        dataDictionary.Clear();

        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at path: " + filePath);
            return;
        }

        StreamReader reader = new StreamReader(filePath);
        reader.ReadLine(); //Skip headers
        while (!reader.EndOfStream)
        {
            string[] line = reader.ReadLine().Split(',');
            if (line.Length >= 6)
            {
                float x = float.Parse(line[0], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(line[1], System.Globalization.CultureInfo.InvariantCulture) + heightOffset;
                float z = float.Parse(line[2], System.Globalization.CultureInfo.InvariantCulture);
                float predictedX = float.Parse(line[3], System.Globalization.CultureInfo.InvariantCulture);
                float predictedY = float.Parse(line[4], System.Globalization.CultureInfo.InvariantCulture) + heightOffset;
                float predictedZ = float.Parse(line[5], System.Globalization.CultureInfo.InvariantCulture);

                Vector3 realLocation = new Vector3(x, y, z);
                Vector3 predictedLocation = new Vector3(predictedX, predictedY, predictedZ);

                if (!dataDictionary.ContainsKey(realLocation))
                {
                    dataDictionary.Add(realLocation, new List<Vector3>());
                }
                dataDictionary[realLocation].Add(predictedLocation);
            }
        }
        reader.Close();
    }

    void VisualizeData() {
        LoadCSVData(csvFilePath);

        Debug.Log($"Selected option {selectedOption}");

        switch(selectedOption) {
            case 0:
                VisualizeWithArrows();
                break;
            case 1:
                VisualizeWithCircles();
                break;
        }
    }

    void VisualizeWithCircles()
    {
        GameObject parent = GetParent();

        foreach (KeyValuePair<Vector3, List<Vector3>> entry in dataDictionary)
        {
            Vector3 realLocation = entry.Key;
            List<Vector3> predictedLocations = entry.Value;

            Material pointMaterial = new Material(Shader.Find("Custom/AlwaysOnTop"));

            // Create a point for the real location
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = realLocation;
            point.transform.localScale = new Vector3(actualSize, actualSize, actualSize);
            point.GetComponent<Renderer>().sharedMaterial = pointMaterial;
            AddToFloor(point);
            // point.transform.parent = parent.transform;

            Material material = new Material(Shader.Find("Custom/AlwaysOnTop"));

            // Create a larger sphere for the predicted locations
            float maxDistance = predictedLocations.Max(loc => Vector3.Distance(loc, realLocation)) * 2 * (1 / point.transform.localScale.x);
            if(circleOutline) {
                GameObject outline = new GameObject("Outline");
                outline.transform.parent = point.transform;
                outline.transform.position = point.transform.position;
                LineRenderer lineRenderer = outline.AddComponent<LineRenderer>();
                lineRenderer.positionCount = numSegments + 1;
                lineRenderer.startWidth = outlineThickness;
                lineRenderer.endWidth = outlineThickness;
                lineRenderer.material = pointMaterial;
                float deltaTheta = 2f * Mathf.PI / (numSegments - 1);
                float theta = 0f;

                for (int i = 0; i < numSegments + 1; i++)
                {
                    float x = outline.transform.position.x + maxDistance * Mathf.Cos(theta);
                    float z = outline.transform.position.z + maxDistance * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, outline.transform.position.y, z);
                    lineRenderer.SetPosition(i, pos);
                    theta += deltaTheta;
                }


            } else {
                GameObject largerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                largerSphere.transform.position = point.transform.position;
                largerSphere.transform.localScale = Vector3.one;
                largerSphere.GetComponent<Renderer>().sharedMaterial = material;
                largerSphere.GetComponent<Renderer>().sharedMaterial.SetFloat("_Mode", 3); // Set transparency mode
                largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_ZWrite", 0);
                largerSphere.GetComponent<Renderer>().sharedMaterial.DisableKeyword("_ALPHATEST_ON");
                largerSphere.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_ALPHABLEND_ON");
                largerSphere.GetComponent<Renderer>().sharedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                largerSphere.GetComponent<Renderer>().sharedMaterial.renderQueue = 3000;
                largerSphere.transform.parent = point.transform;

                // Adjust the size of the larger sphere to contain all predicted points
                largerSphere.transform.localScale = new Vector3(maxDistance, 0.01f, maxDistance);
            }

            foreach(Vector3 pred in predictedLocations) {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.transform.position = pred;
                p.transform.localScale = new Vector3(predictedSize, predictedSize, predictedSize);
                p.transform.parent = point.transform;
                p.GetComponent<Renderer>().sharedMaterial = pointMaterial;
            }



        }

        foreach(Transform child in parent.transform) {
            List<Transform> children = new List<Transform>();

            foreach(Transform c in child) {
                children.Add(c);
            }

            AssignColors(children);
        }
    }


    void VisualizeWithArrows() {
        GameObject parent = GetParent();

        foreach (KeyValuePair<Vector3, List<Vector3>> entry in dataDictionary)
        {
            Vector3 realLocation = entry.Key;
            List<Vector3> predictedLocations = entry.Value;

            Material lineMaterial = new Material(Shader.Find("Custom/AlwaysOnTop")) {
                renderQueue = 3000
            };
            Material alwaysOnTop = new Material(Shader.Find("Custom/AlwaysOnTop")) {
                renderQueue = 3004
            };
            Material alwaysOnTop2 = new Material(Shader.Find("Custom/AlwaysOnTop")){
                renderQueue = 3002
            };


            // Create a point for the real location
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = realLocation;
            point.transform.localScale = new Vector3(actualSize, actualSize, actualSize);
            point.GetComponent<Renderer>().material = alwaysOnTop;
            AddToFloor(point);
            AddOutline(point, actualSize + outlineThickness, 3003);


            foreach(Vector3 pred in predictedLocations) {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.transform.position = pred;
                p.transform.localScale = new Vector3(predictedSize, predictedSize, predictedSize);
                p.GetComponent<Renderer>().material = alwaysOnTop2;
                p.transform.parent = point.transform;

                AddOutline(p, predictedSize + outlineThickness, 3001);            

                LineRenderer renderer = p.AddComponent<LineRenderer>();
                renderer.positionCount = 2;
                renderer.material = lineMaterial;
                renderer.SetPositions(new Vector3[]{
                    point.transform.position,
                    p.transform.position,
                });
                renderer.startWidth = arrowThickness;
                renderer.endWidth = arrowThickness;
            }
        }

        foreach(Transform child in parent.transform) {
            List<Transform> children = new List<Transform>();

            foreach(Transform c in child) {
                children.Add(c);
            }

            AssignColors(children);
        }
    }
    void AddToFloor(GameObject obj) {
        int floor = Utilities.HeightToFloor(obj.transform.position.y);

        GameObject parent = GameObject.Find($"Results/F{floor}");

        if(!parent) {
            parent = new GameObject($"F{floor}");
            parent.transform.parent = GameObject.Find("Results").transform;
        }

        obj.transform.parent = parent.transform;
    }


    void AddOutline(GameObject obj, float thickness, int outlineQueue = 3000) {
        Material outlineShader = new Material(Shader.Find("Custom/AlwaysOnTop")){
            color = Color.white,
            renderQueue = outlineQueue
        };
        
        GameObject outline = Instantiate(obj);
        outline.transform.localScale = new Vector3(thickness, thickness, thickness);
        outline.transform.parent = obj.transform;
        outline.transform.localPosition = Vector3.zero;
        outline.GetComponent<MeshRenderer>().material = outlineShader;
    }

    GameObject GetParent() {
        GameObject parent = GameObject.Find("Results");

        if(parent) {
            List<Transform> children = new List<Transform>();
            foreach(Transform child in parent.transform)
                children.Add(child);
            
            foreach(Transform child in children)
                DestroyImmediate(child.gameObject);
        } else {
            parent = new GameObject("Results");
        }
        return parent;
    }

    void GetPreviouslyEnabled() {
        GameObject parent = GameObject.Find("Results");

        foreach(Transform child in parent.transform) {
            if(previouslyEnabled.ContainsKey(child.name)) {
                previouslyEnabled[child.name] = child.gameObject.activeSelf;
            }else {
                previouslyEnabled.Add(child.name, child.gameObject.activeSelf);
            }
        }
    }

    void SetPreviouslyEnabled() {
        GameObject parent = GameObject.Find("Results");

        foreach(Transform child in parent.transform) {
            if(previouslyEnabled.ContainsKey(child.name))
                child.gameObject.SetActive(previouslyEnabled[child.name]);
        }
    }


    void AssignColors(List<Transform> objects)
    {
        Dictionary<Transform, Color> objectColors = new Dictionary<Transform, Color>();
        
        foreach (Transform obj in objects)
        {
            Color newColor;
            bool isColorAcceptable;

            int tries = 0;
            do
            {
                newColor = Random.ColorHSV(0f, 1f, 0.2f, 1f, 0.5f, 1f);
                isColorAcceptable = true;

                foreach (Transform otherObj in objects)
                {
                    if (obj == otherObj) continue;

                    float distance = Vector3.Distance(obj.position, otherObj.position);
                    if (distance < proximityThreshold) // Adjust the proximity threshold as needed
                    {
                        Color otherColor;
                        if (objectColors.TryGetValue(otherObj, out otherColor))
                        {
                            float colorDifference = GetColorDifference(newColor, otherColor);
                            if (colorDifference < colorDifferenceThreshold)
                            {
                                isColorAcceptable = false;
                                break;
                            }
                        }
                    }
                }
                tries++;
            } while (!isColorAcceptable && tries < 10);

            objectColors[obj] = newColor;

            foreach(MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>()) {
                if(renderer.gameObject.name != "Sphere(Clone)")
                    renderer.sharedMaterial.color = newColor;
            }

            foreach(LineRenderer renderer in obj.GetComponentsInChildren<LineRenderer>()) {
                renderer.sharedMaterial.color = newColor;
                renderer.startColor = newColor;
                renderer.endColor = newColor;
            }
        }
    }

    float GetColorDifference(Color color1, Color color2)
    {
        // Simple Euclidean distance in RGB space
        return Mathf.Sqrt(
            Mathf.Pow(color1.r - color2.r, 2) +
            Mathf.Pow(color1.g - color2.g, 2) +
            Mathf.Pow(color1.b - color2.b, 2)
        );
    }


}
