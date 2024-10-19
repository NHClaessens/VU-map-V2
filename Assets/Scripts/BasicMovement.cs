using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BasicMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    void Update()
    {
        // if(IsMouseOverUIElement()) return;
        
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        if(movement.magnitude > 0) {
            LocationController.location += movement * speed * Time.deltaTime;
        }
    }

    bool IsMouseOverUIElement()
    {
        if (Input.touchCount > 0 || EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if(result.gameObject.name == "PanelSettings") {

                    IPanel panel = result.gameObject.GetComponent<PanelEventHandler>().panel;
                    Vector2 screenPos;
                    if(Application.platform == RuntimePlatform.Android) {
                        screenPos = Input.GetTouch(0).position;
                    } else {
                        screenPos = Input.mousePosition;
                    }

                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                    Vector2 coords = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPos, Camera.main);
                    VisualElement el = panel.Pick(coords);
                    return el.name != "safe-area-content-container";
                }
            }
        }
        return false; // Mouse is not over a UI Toolkit element
    }
}
