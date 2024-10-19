using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    public float minZoom;
    public float maxZoom;
    public Vector3 minPos;
    public Vector3 maxPos;
    public float zoomSensitivity;
    public UnityEvent onZoom = new UnityEvent();
    public bool stopMovement;
    Vector3 touchStart;
    Camera cam;

    private Coroutine moveRoutine;
    private Coroutine zoomRoutine;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y != 999) transform.position = new Vector3(transform.position.x, 999, transform.position.z);
        
        if(IsMouseOverUIElement()) {
            return;
        }
        if(Input.GetMouseButtonDown(0) && Input.touchCount < 2 && !IsMouseOverUIElement()) {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if(Input.touchCount == 2) {
            Touch zero = Input.GetTouch(0);
            Touch one = Input.GetTouch(1);

            Vector2 zeroPrev = zero.position - zero.deltaPosition;
            Vector2 onePrev = one.position - one.deltaPosition;

            float prevMag = (zeroPrev - onePrev).magnitude;
            float mag = (zero.position - one.position).magnitude;

            float diff = mag - prevMag;

            zoom(diff * zoomSensitivity);
        }

        if(Input.GetMouseButton(0) && Input.touchCount < 2) {
            Vector3 direction = touchStart - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += direction;

            cam.transform.position = new Vector3(
                Mathf.Clamp(cam.transform.position.x, minPos.x, maxPos.x),
                cam.transform.position.y,
                Mathf.Clamp(cam.transform.position.z, minPos.z, maxPos.z)
            );
        }
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
            zoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    void zoom(float increment) {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, minZoom, maxZoom);
        onZoom.Invoke();
    }

    public void moveTo(Vector3 pos, float duration) {
        Debug.Log($"Move camera to {pos}");
        if(moveRoutine != null) {
            StopCoroutine(moveRoutine);
        }
        moveRoutine = StartCoroutine(moveToRoutine(pos, duration));
    }

    public void moveTo(Vector3 pos, float orthoSize, float duration) {
        if(moveRoutine != null) {
            StopCoroutine(moveRoutine);
        }
        moveRoutine = StartCoroutine(moveToRoutine(pos, duration));
        if(zoomRoutine != null) {
            StopCoroutine(zoomRoutine);
        }
        zoomRoutine = StartCoroutine(setOrthoSize(orthoSize, duration));
    }

    public void cancelMoveTo() {
        StopAllCoroutines();
    }

    IEnumerator moveToRoutine(Vector3 pos, float duration) {
        pos = new Vector3(pos.x, 999, pos.z);
        float dist = Vector3.Distance(cam.transform.position, pos);
        while(cam.transform.position != pos) {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, pos, dist / (duration / Time.deltaTime));
            yield return 0;
        }
    }

    IEnumerator setOrthoSize(float size, float duration) {
        size = size / 2 + 10;

        float startTime = Time.time;
        float startSize = cam.orthographicSize;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration; // Calculate the interpolation factor
            cam.orthographicSize = Mathf.Lerp(startSize, size, t);
            yield return null;
        }

        cam.orthographicSize = size;
    }

    public void showAllElements(List<GameObject> gameObjects) {
        if (gameObjects == null || gameObjects.Count == 0)
            return;

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;

        // Calculate bounds
        foreach (GameObject obj in gameObjects)
        {
            Vector3 pos = obj.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.z < minZ) minZ = pos.z;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        Debug.DrawLine(
            new Vector3(minX, 1, minZ),
            new Vector3(maxX, 1, minZ),
            Color.green,
            99999
        );
        Debug.DrawLine(
            new Vector3(maxX, 1, minZ),
            new Vector3(maxX, 1, maxZ),
            Color.green,
            99999
        );
        Debug.DrawLine(
            new Vector3(maxX, 1, maxZ),
            new Vector3(minX, 1, maxZ),
            Color.green,
            99999
        );
        Debug.DrawLine(
            new Vector3(minX, 1, maxZ),
            new Vector3(minX, 1, minZ),
            Color.green,
            99999
        );

        Vector3 center = new Vector3((minX + maxX) / 2, cam.transform.position.y, (minZ + maxZ) / 2);

        // Adjust orthographic size

        float width = maxZ - minZ;
        float height = maxX - minX;
        float orthographicSize;

        if(width * Screen.height / Screen.width > height) {
            orthographicSize = width * Screen.height / Screen.width;
        } else {
            orthographicSize = height;
        }

        

        moveTo(center, orthographicSize, 1);
    }

    bool IsMouseOverUIElement()
    {
        if (stopMovement) return true;
        if (Input.touchCount > 0 || EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results.Count > 0;
        }
        return false; // Mouse is not over a UI Toolkit element
    }
}
