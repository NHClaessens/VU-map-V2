using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class WorldPos : Editor
{
    public override void OnInspectorGUI()
    {
        Transform transform = (Transform)target;

        DrawDefaultInspector();

        // Display world position
        EditorGUILayout.BeginHorizontal();
        transform.position = EditorGUILayout.Vector3Field("World Pos", transform.position);
        EditorGUILayout.EndHorizontal();

        // Calculate the combined bounding box of the object and its children
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);
        bool hasRenderer = false;

        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                if (!hasRenderer)
                {
                    combinedBounds = renderer.bounds;
                    hasRenderer = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }

        if (hasRenderer)
        {
            EditorGUILayout.LabelField("Bounding Box (Including Children)", EditorStyles.boldLabel);
            EditorGUILayout.Vector3Field("Center", combinedBounds.center);
            EditorGUILayout.Vector3Field("Size", combinedBounds.size);
            EditorGUILayout.Vector3Field("Min", combinedBounds.min);
            EditorGUILayout.Vector3Field("Max", combinedBounds.max);
        }
        else
        {
            EditorGUILayout.LabelField("No Renderer components found.");
        }
    }
}
