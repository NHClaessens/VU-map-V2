using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIController : MonoBehaviour
{
    public CameraController cameraController;
    
    void OnEnable() {
        cameraController.onZoom.AddListener(setZoomVisibility);
        setZoomVisibility();
    }
    void setZoomVisibility() {
        foreach(Transform child in transform) {
            child.gameObject.GetComponent<POI>().setZoomVisibility();
        }
    }
}
