using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;


public class WifiManager : MonoBehaviour
{
    public static bool available = Application.platform == RuntimePlatform.Android;
    public static UnityEvent<JToken> scanComplete = new UnityEvent<JToken>();
    private static AndroidJavaObject wifiManager;
    private static AndroidJavaObject instance;

    void Start() {
        if(Application.platform != RuntimePlatform.Android) {
            Debug.Log("Only works on android");
            return;
        }
        
        if(!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            Permission.RequestUserPermission(Permission.FineLocation);
        }


        Debug.Log("WIFIMANAGER ####");
        wifiManager = new AndroidJavaObject("com.nhclaessens.vumap.WifiManagerPlugin");
        instance = GetUnityPlayerActivity();
        wifiManager.Call("registerWifiScanReceiver", instance);
    }

    void OnDestroy() {
        if(wifiManager != null)
            wifiManager.Call("unRegisterReceiver", instance);
    }

    public static void startScan() {
        if(wifiManager != null) {
            Debug.Log("Manager starting scan");
            wifiManager.Call("startScan", instance);
        } else {
            scanComplete.Invoke(JToken.Parse("{}"));
        }
    }

    public void onScanComplete(string result) {
        scanComplete.Invoke(JToken.Parse(result));
    }

    public void onScanFailed(string result) {
        Debug.Log("Scan failed, you might have to manually turn of wifi throttling");
    }

    AndroidJavaObject GetUnityPlayerActivity()
    {
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
    }
}
