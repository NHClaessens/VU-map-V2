using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ManualSampler : MonoBehaviour {
    public int wifiSampleAmount;
    public WifiManager wifiManager;
    public GameObject UI;
    public GameObject indicator;
    public CameraController cameraController;
    public FloorHeight[] floorHeights;
    public float indicatorSpeed = 1;

    private string fileName;
    private List<Sample> samples = new List<Sample>();
    private GameObject progress;
    private Joystick joystick;
    private TMP_Dropdown floorSelector;

    public void Start() {
        progress = UI.transform.Find("Progress").gameObject;
        joystick = UI.transform.Find("Joystick").GetComponent<Joystick>();
        floorSelector = GameObject.Find("FloorSelector").GetComponent<TMP_Dropdown>();
    }

    public void Update() {
        Vector2 dir = joystick.Direction;
        if(dir.magnitude > 0) {
            cameraController.stopMovement = true;
            indicator.transform.position += new Vector3(-dir.y, 0, dir.x) * indicatorSpeed * Time.deltaTime;
        } else if(cameraController.stopMovement){
            cameraController.stopMovement = false;
        }
    }

    public void StartSampling() {
        UI.SetActive(true);
        indicator.SetActive(true);
        GameObject.Find("Initial").SetActive(false);

        fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}-wifi{wifiSampleAmount}-custom.csv";
        
        WifiManager.scanComplete.AddListener(ScanComplete);
    }

    public void TakeSample() {
        GameObject loc = Instantiate(indicator);
        loc.transform.position = new Vector3(loc.transform.position.x, floorHeights[floorSelector.value].height, loc.transform.position.z);
        loc.name = $"sample-{samples.Count / wifiSampleAmount}";
        loc.GetComponent<MeshRenderer>().material.color = Color.green;

        WifiManager.startScan();
    }

    private List<string> intermediate = new List<string>();
    public void ScanComplete(JToken result) {
        intermediate.Add(result.ToString());

        

        if(intermediate.Count >= wifiSampleAmount) {
            processMeasurements();
        } else {
            WifiManager.startScan();
        }
        progress.GetComponent<TMP_Text>().text = $"Locations: {samples.Count / wifiSampleAmount}\nMeasurements: {intermediate.Count}/{wifiSampleAmount}";
    }

    private void processMeasurements() {
        // Result: [{"SSID":"","MAC":"e6:75:dc:e9:cd:d9","signalStrength":-83},{"SSID":"EEMN Network","MAC":"18:e8:29:9b:42:27","signalStrength":-26},{"SSID":"boomland","MAC":"bc:df:58:ca:64:d5","signalStrength":-84},{"SSID":"EEMN Network","MAC":"18:e8:29:9a:42:27","signalStrength":-32},{"SSID":"Ziggo-ap-4098a8b","MAC":"c4:71:54:09:8a:8b","signalStrength":-81},{"SSID":"Gasten","MAC":"ce:ce:1e:2c:a1:70","signalStrength":-80},{"SSID":"KPN69DF26","MAC":"e4:75:dc:1d:cc:63","signalStrength":-82},{"SSID":"REMOTE32usxw","MAC":"00:1d:c9:07:e9:7d","signalStrength":-80},{"SSID":"VR46","MAC":"e4:75:dc:e9:cd:d9","signalStrength":-83},{"SSID":"boomland","MAC":"bc:df:58:c7:70:f9","signalStrength":-82},{"SSID":"","MAC":"e6:75:dc:2d:cc:63","signalStrength":-80},{"SSID":"FRITZ!Box 5490 DC","MAC":"cc:ce:1e:2c:a1:70","signalStrength":-83}]
        
        foreach(string res in intermediate) {
            string wrappedJson = "{\"measurements\":" + res + "}";
            print("Result: " + wrappedJson);
            
            Sample sample = JsonUtility.FromJson<Sample>(wrappedJson);
            sample.location = indicator.transform.position;
            sample.location.y = floorHeights[floorSelector.value].height;
            
            samples.Add(sample);
        }

        intermediate.Clear();

        
        Sampler.saveToCSV(samples, fileName);
    }
}