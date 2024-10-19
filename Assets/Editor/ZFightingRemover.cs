using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ZFightingRemover : MonoBehaviour
{
    [MenuItem("Tools/Remove Z-Fighting Objects")]
    static void RemoveZFightingObjectsMenu()
    {
        // Get the selected GameObject
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogWarning("No GameObject selected. Please select a root GameObject.");
            return;
        }

        RemoveZFightingObjects(root);
    }

    static void RemoveZFightingObjects(GameObject root)
    {
        // Get all MeshRenderers in the children of the root
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
        List<MeshRenderer> toRemove = new List<MeshRenderer>();

        // Check each renderer against every other renderer
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = i + 1; j < renderers.Length; j++)
            {
                // Check for exact overlap
                if (MeshesAreExactlyOverlapping(renderers[i].GetComponent<MeshFilter>().sharedMesh, renderers[j].GetComponent<MeshFilter>().sharedMesh, renderers[i].transform, renderers[j].transform))
                {
                    toRemove.Add(renderers[j]);
                }
            }
        }

        // Remove the duplicates
        foreach (MeshRenderer renderer in toRemove)
        {
            DestroyImmediate(renderer.gameObject);
        }

        Debug.Log("Z-Fighting objects removed: " + toRemove.Count);
    }

    static bool MeshesAreExactlyOverlapping(Mesh meshA, Mesh meshB, Transform transformA, Transform transformB)
    {
        // Get vertices
        Vector3[] verticesA = meshA.vertices;
        Vector3[] verticesB = meshB.vertices;

        // Check if the vertex count is different
        if (verticesA.Length != verticesB.Length)
            return false;

        // Transform vertices to world space
        for (int i = 0; i < verticesA.Length; i++)
        {
            verticesA[i] = transformA.TransformPoint(verticesA[i]);
            verticesB[i] = transformB.TransformPoint(verticesB[i]);
        }

        // Sort vertices for comparison
        System.Array.Sort(verticesA, CompareVector3);
        System.Array.Sort(verticesB, CompareVector3);

        // Compare vertices
        for (int i = 0; i < verticesA.Length; i++)
        {
            if (Vector3.Distance(verticesA[i], verticesB[i]) > 0.0001f) // Allow for a small tolerance
                return false;
        }

        return true;
    }

    static int CompareVector3(Vector3 a, Vector3 b)
    {
        if (a.x != b.x)
            return a.x.CompareTo(b.x);
        if (a.y != b.y)
            return a.y.CompareTo(b.y);
        return a.z.CompareTo(b.z);
    }
}
