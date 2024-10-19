using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    static int selectedFloor = 0;

    public void Start() {
        SelectFloor(0);
    }

    public static void SelectFloor(int num) {
        if(num < 0) num = 0;
        if(num > 12) num = 12;
        
        GameObject model = GameObject.Find("3D models");

        foreach(Transform floor in model.transform) {
            floor.gameObject.SetActive(floor.name == "F"+num);
        }

        selectedFloor = num;
    }

    public static void ChangeFloor(int change) {
        SelectFloor(selectedFloor + change);
    }
}
