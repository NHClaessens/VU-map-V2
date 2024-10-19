using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class LocationController : MonoBehaviour
{

    public static string selectedEndpoint = "predict/default";
    public static UnityEvent<Vector3> locationChanged = new UnityEvent<Vector3>();
    public static Vector3 location;

    private Coroutine scanning;

    void Start()
    {
        // if(!WifiManager.available) return;
        WifiManager.scanComplete.AddListener(onScanComplete);
        location = new Vector3(30, 1, 30);
        scanning = StartCoroutine(WifiScanning());
    }

    void OnDestroy() {
        if(scanning != null)
            StopCoroutine(scanning);
    }


    private async void onScanComplete(JToken result) {
        Debug.Log($"Wifi scan result {result}");

        JToken res = await API.Post(selectedEndpoint, result);

        Debug.Log($"Server prediction {res}");

        Vector3 position = new Vector3(float.Parse(res["x"].ToString()), float.Parse(res["y"].ToString()), float.Parse(res["z"].ToString()));

        NavMeshHit adjusted;
        NavMesh.SamplePosition(position, out adjusted, 30, NavMesh.AllAreas);
        position = adjusted.position;

        Debug.Log($"Adjusted prediction {position}");
        transform.position = position;
        location = position;
        locationChanged.Invoke(position);

        FloorController.SelectFloor(Utilities.HeightToFloor(position.y));
    }

    private IEnumerator WifiScanning() {
        while(true) {
            WifiManager.startScan();

            yield return new WaitForSeconds(3);
        }
    }
}
