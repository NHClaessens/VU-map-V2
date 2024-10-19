using UnityEditor;
using UnityEngine;

public class EnhancedSceneCamera : EditorWindow
{
    private Quaternion savedRotation;
    private Vector3 savedPosition;
    private RaycastHit lastHit;

    [MenuItem("Tools/Enhanced Scene Camera")]
    public static void ShowWindow()
    {
        GetWindow<EnhancedSceneCamera>("Enhanced Camera");
    }

    private void OnGUI()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        float degrees = EditorGUILayout.FloatField(90);
        if (GUILayout.Button("Rotate Camera Around Look Axis"))
        {
            RotateCameraAroundLookAxis(degrees); // Rotate by 10 degrees
        }
        sceneView.pivot = EditorGUILayout.Vector3Field("Position", sceneView.pivot);

        
        GUILayout.BeginHorizontal();
        sceneView.rotation = Quaternion.Euler(
            EditorGUILayout.FloatField("X", sceneView.rotation.eulerAngles.x),
            EditorGUILayout.FloatField("Y", sceneView.rotation.eulerAngles.y),
            EditorGUILayout.FloatField("Z", sceneView.rotation.eulerAngles.z)
        );

        GUILayout.EndHorizontal();


        GUILayout.Label($"Saved rotation: {savedRotation}, position: {savedPosition}");
            if(GUILayout.Button("Save Camera Position")) {
            if(sceneView != null) {
                savedRotation = sceneView.rotation;
                savedPosition = sceneView.pivot;
            }
        }
        if(GUILayout.Button("Restore Camera Position")) {
            if(sceneView != null) {
                if(savedRotation != null)
                sceneView.rotation = savedRotation;
                sceneView.pivot = savedPosition;
            }
        }

        GUILayout.Space(32f);

        if(GUILayout.Button("Cast Ray")) {
            if(Physics.Raycast(sceneView.camera.transform.position, sceneView.rotation * Vector3.forward, out lastHit)) {
                Debug.Log($"HIT x: {lastHit.point.x}, y: {lastHit.point.y}, z: {lastHit.point.z}");

            }

            Debug.DrawRay(sceneView.camera.transform.position, sceneView.rotation * Vector3.forward * 100, Color.red, 100f);
        }

        
    }

    // Rotates the scene view camera around its look-at axis
    private void RotateCameraAroundLookAxis(float angle)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            // Get the current rotation and position of the scene camera
            Quaternion currentRotation = sceneView.rotation;
            Vector3 currentPosition = sceneView.pivot;

            // Perform the rotation around the look-at axis
            Quaternion rotation = Quaternion.AngleAxis(angle, sceneView.camera.transform.forward);
            sceneView.rotation = rotation * currentRotation;
            
            // Repaint the scene view to update the camera change
            sceneView.Repaint();
        }
    }
}