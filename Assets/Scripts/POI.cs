using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI : MonoBehaviour
{
    public string title;
    public string description;
    public string[] tags;
    public int floor;
    public float maxZoomLevel;

    private bool forceVisible;

    public void setZoomVisibility() {
        if(forceVisible) return;
        if(Camera.main.orthographicSize > maxZoomLevel) {
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
        }
    }

    public void setVisibility(bool visible) {
        forceVisible = visible;
        gameObject.SetActive(visible);
        if(!visible) setZoomVisibility();
    }
}
