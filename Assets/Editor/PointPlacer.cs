using UnityEngine;
using UnityEditor;
using System;

public class PointPlacer : EditorWindow
{
    string parentObjectName = "ParentObject";
    Vector3 startLocation = Vector3.zero;
    float angle = 0f;
    float distance = 1f;
    GameObject parentObject;
    Vector3 currentLocation;

    [MenuItem("Tools/Point Placer")]
    public static void ShowWindow()
    {
        GetWindow<PointPlacer>("Point Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        parentObjectName = EditorGUILayout.TextField("Parent Object Name", parentObjectName);
        startLocation = EditorGUILayout.Vector3Field("Start Location", startLocation);

        if (GUILayout.Button("Initialize Parent"))
        {
            InitializeParent();
        }

        GUILayout.Space(10);

        GUILayout.Label("Point Placement", EditorStyles.boldLabel);
        angle = EditorGUILayout.FloatField("Angle (degrees)", angle);
        distance = EditorGUILayout.FloatField("Distance", distance);

        if (GUILayout.Button("Place Point"))
        {
            PlacePoint();
        }

        if (GUILayout.Button("Remove Last Point"))
        {
            RemoveLastPoint();
        }
    }

    private void InitializeParent()
    {
        GameObject existingParent = GameObject.Find(parentObjectName);
        if (existingParent != null)
        {
            DestroyImmediate(existingParent);
        }
        parentObject = new GameObject(parentObjectName);
        parentObject.transform.position = startLocation;
        currentLocation = startLocation;

        // Create the first point at the start location
        GameObject firstPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        firstPoint.name = "Point 0";
        firstPoint.transform.position = startLocation;
        firstPoint.transform.parent = parentObject.transform;
        firstPoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    private void PlacePoint()
    {
        if (parentObject == null)
        {
            Debug.LogError("Parent object not initialized.");
            return;
        }

        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 newPointLocation = currentLocation + direction * distance;
        GameObject newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newPoint.name = $"Point {parentObject.transform.childCount}";
        newPoint.transform.position = newPointLocation;
        newPoint.transform.parent = parentObject.transform;
        newPoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        currentLocation = newPointLocation;

        SceneView.RepaintAll();
    }

    private void RemoveLastPoint()
    {
        if (parentObject == null || parentObject.transform.childCount == 0)
        {
            Debug.LogError("No points to remove.");
            return;
        }

        int lastIndex = parentObject.transform.childCount - 1;
        GameObject lastPoint = parentObject.transform.GetChild(lastIndex).gameObject;
        if (lastIndex > 0)
        {
            currentLocation = parentObject.transform.GetChild(lastIndex - 1).position;
        }
        else
        {
            currentLocation = parentObject.transform.position;
        }
        DestroyImmediate(lastPoint);
        SceneView.RepaintAll();
    }

    void OnFocus()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (parentObject != null && parentObject.transform.childCount > 0)
        {
            Handles.color = Color.blue;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 previewLocation = currentLocation + direction * distance;
            Handles.DrawLine(currentLocation, previewLocation);
            Handles.SphereHandleCap(0, previewLocation, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.SphereHandleCap(0, currentLocation, Quaternion.identity, 0.1f, EventType.Repaint);
        }
    }
}
