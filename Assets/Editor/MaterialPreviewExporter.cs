using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialPreviewExporter : MonoBehaviour
{
    [MenuItem("Tools/Export Material Preview")]
    private static void ExportMaterialPreview()
    {
        if (Selection.activeObject is Material material)
        {
            Texture2D previewTexture = AssetPreview.GetAssetPreview(material);
            if (previewTexture != null)
            {
                byte[] bytes = previewTexture.EncodeToPNG();
                string path = Path.Combine(Application.dataPath, material.name + "_Preview.png");
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                Debug.Log("Material preview exported to " + path);
            }
            else
            {
                Debug.LogError("Failed to get material preview.");
            }
        }
    }
}