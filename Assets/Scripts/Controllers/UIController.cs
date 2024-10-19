using Unity.VisualScripting;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public string[] options;
    public static void SetUIState(string newState) {
        foreach(Transform child in GameObject.Find("UI").transform) {
            if(child.gameObject.name == newState) {
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }
        }
    }
    
    public void SetUIState(int newState) {
        SetUIState(options[newState]);
    }


}