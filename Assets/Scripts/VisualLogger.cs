using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualLogger : MonoBehaviour
{
    private static GameObject self;
    private static GameObject content;
    private static ScrollRect rect;
    private static List<string> cache = new List<string>();

    public void Awake() {
        self = gameObject;
        content = gameObject.transform.Find("Viewport/Content").gameObject;
        rect = gameObject.GetComponent<ScrollRect>();

        Debug.Log("Logger awake");
    }

    public static void Log(string message) {
        string currentTime = System.DateTime.Now.ToString("HH:mm:ss");

        if(rect == null || content == null){
            cache.Append($"{currentTime} : {message}");
            return;
        }

        if(cache.Count > 0) {
            foreach(string msg in cache) {
                AddLog(msg);
            }
        }

        bool bottom = rect.verticalNormalizedPosition <= 0.1f;
        
        AddLog($"{currentTime} : {message}");
        Debug.Log($"{currentTime} : {message}");

        if(bottom) rect.verticalNormalizedPosition = 0;
    }

    private static void AddLog(string message) {
        
        GameObject temp = new GameObject("Log message");
        temp.transform.SetParent(content.transform);

        TextMeshProUGUI text = temp.AddComponent<TextMeshProUGUI>();

        

        text.text = message;
    }
}
