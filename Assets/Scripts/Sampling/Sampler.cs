using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Linq;

public class Sampler : MonoBehaviour
{
    public WifiManager wifiManager;
    public SamplingMethod samplingMethod;
    public int randomSampleAmount;
    public int wifiSampleAmount;
    public bool directionalScans = true;
    public float gridSize = 1f;
    public GameObject customSampleSet;
    public FloorHeight[] floorHeights;
    public List<Sample> samples = new List<Sample>();
    public GameObject sampleUI;
    public CameraController cameraController;


    private Bounds bounds;
    private Vector3 currentPosition;


    public List<Vector3> samplePoints = new List<Vector3>();
    public List<Vector3> missedPoints = new List<Vector3>();


    private TMP_Dropdown dropdown;
    private GameObject startProcess;


    private GameObject cannot;
    private GameObject later;
    private GameObject start;
    private GameObject end;
    public GameObject indicator;
    private TMP_Text location;
    private TMP_Text progress;
    private TMP_Text compass;
    private int coveredLocations = 0;
    private int totalLocations = 0;
    private string fileName;
    private List<string> possibleSets = new List<string>();

    public void Start() {
        dropdown = sampleUI.transform.Find("Setup/Dropdown").GetComponent<TMP_Dropdown>();
        
        possibleSets = Utilities.FindTopLevelGameObjectNamesByPattern("SampleSet.*");
        dropdown.ClearOptions();
        dropdown.AddOptions(possibleSets);
        dropdown.onValueChanged.AddListener(SelectSet);
        


        startProcess = sampleUI.transform.Find("Setup/start").gameObject;
        startProcess.GetComponent<Button>().onClick.AddListener(StartSampling);

        start = sampleUI.transform.Find("Panel/start").gameObject;
        start.GetComponent<Button>().onClick.AddListener(takeMeasurement);

        later = sampleUI.transform.Find("Panel/later").gameObject;
        later.GetComponent<Button>().onClick.AddListener(addToEnd);

        cannot = sampleUI.transform.Find("Panel/cannot").gameObject;
        cannot.GetComponent<Button>().onClick.AddListener(cannotReach);

        sampleUI.transform.Find("Panel/recenter").gameObject.GetComponent<Button>().onClick.AddListener(delegate () {
            cameraController.moveTo(samplePoints[0], 0.5f);
        });


        location = sampleUI.transform.Find("Panel/location").GetComponent<TMP_Text>();
        progress = sampleUI.transform.Find("Panel/progress").GetComponent<TMP_Text>();
        compass = sampleUI.transform.Find("Panel/compass").GetComponent<TMP_Text>();
    }

    private void SelectSet(int index) {
        VisualLogger.Log($"Select set {possibleSets[index]}");
        customSampleSet = GameObject.Find(possibleSets[index]);
    }

    public void StartSampling() {
        VisualLogger.Log($"Start sampling for set {customSampleSet}");
        sampleUI.transform.Find("Setup").gameObject.SetActive(false);
        sampleUI.transform.Find("Panel").gameObject.SetActive(true);
        WifiManager.scanComplete.AddListener(onScanComplete);

        fileName = $"{System.DateTime.Now:yyyyMMdd_HHmmss}-wifi{wifiSampleAmount}";
        switch(samplingMethod) {
            case SamplingMethod.Grid:
                fileName += $"-grid{gridSize}.csv";
                break;
            case SamplingMethod.Random:
                fileName += $"-random{randomSampleAmount}.csv";
                break;
            case SamplingMethod.Custom:
                fileName += $"-custom{customSampleSet.name}.csv";
                break;
            default:
                fileName += ".csv";
                break;
        }


        bounds = new Bounds(transform.position, Vector3.zero);
        Renderer[] renderers = GameObject.Find("3D models").GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        generateSampleLocations();
        totalLocations = samplePoints.Count;

        preprareMeasurement(samplePoints[0]);
    }

    private void preprareMeasurement(Vector3 pos) {
        cameraController.moveTo(pos, 10, 0.5f);
        sampleUI.SetActive(true);
        indicator.SetActive(true);
        indicator.transform.position = new Vector3(pos.x, 990, pos.z);
        currentPosition = pos;

        foreach(FloorHeight floor in floorHeights) {
            if(pos.y > floor.height) {
                Utilities.SelectFloor(floor.name);
            }
        }

        coveredLocations++;

        location.text = "Location" + coveredLocations + "/" + totalLocations;
        progress.text = "Sample 0/" + wifiSampleAmount*4;
    }

    private void cannotReach() {
        samplePoints.RemoveAt(0);
        coveredLocations--;
        totalLocations--;
        preprareMeasurement(samplePoints[0]);
        VisualLogger.Log("Removed point");
    }

    private void addToEnd() {
        Vector3 temp = samplePoints[0];
        samplePoints.RemoveAt(0);
        samplePoints.Add(temp);
        coveredLocations--;
        preprareMeasurement(samplePoints[0]);
        VisualLogger.Log("Moved point to end");
    }

    public void OnDrawGizmos() {



        // foreach(Vector3 pos in samplePoints) {
        //     Gizmos.DrawSphere(pos, 1);
        // }
        
        // Gizmos.color = Color.red;

        // foreach(Vector3 pos in missedPoints) {
        //     Gizmos.DrawSphere(pos, 1);
        // }

        // Gizmos.color = Color.green; // Set the color of the Gizmos
        // Gizmos.DrawWireCube(bounds.center, bounds.size);

        for(int i = 0; i < samplePoints.Count-1; i++) {
            Gizmos.DrawLine(samplePoints[i], samplePoints[i+1]);
        }
    }

    public void generateSampleLocations(){
        Utilities.DisableTopLevelMatchingPattern("SampleSet.*");
        switch(samplingMethod){
            case SamplingMethod.Grid:
                sampleGrid();
                break;
            case SamplingMethod.Random:
                sampleRandom();
                break;
            case SamplingMethod.Custom:
                sampleCustom();
                break;
        }
    }

    public List<string> intermediate = new List<string>();

    public void takeMeasurement() {
        VisualLogger.Log("Started wifi scan");
        Handheld.Vibrate();
        start.GetComponentInChildren<TMP_Text>().text = "Scanning...";


        WifiManager.startScan();
    }

    public void onScanComplete(JToken result) {
        VisualLogger.Log("Wifi scan complete");
        intermediate.Add(result.ToString());

        sampleUI.transform.Find("Panel/progress").GetComponent<TMP_Text>().text = "Sample " + intermediate.Count + "/" + wifiSampleAmount*4;

        if(intermediate.Count % wifiSampleAmount == 0) {
            Handheld.Vibrate();
        }

        if(intermediate.Count == wifiSampleAmount) {
            compass.text = "Orient to East";
            start.GetComponentInChildren<TMP_Text>().text = "Resume scan";
        }
        else if(intermediate.Count == wifiSampleAmount * 2) {
            compass.text = "Orient to South";
            start.GetComponentInChildren<TMP_Text>().text = "Resume scan";
        }
        else if(intermediate.Count == wifiSampleAmount * 3) {
            compass.text = "Orient to West";
            start.GetComponentInChildren<TMP_Text>().text = "Resume scan";
        }
        else if(intermediate.Count >= wifiSampleAmount * 4) {
            compass.text = "Orient to North";
            start.GetComponentInChildren<TMP_Text>().text = "Start";
            processMeasurements();
        } else {
            WifiManager.startScan();
        }
    }

    private void processMeasurements() {
        // Result: [{"SSID":"","MAC":"e6:75:dc:e9:cd:d9","signalStrength":-83},{"SSID":"EEMN Network","MAC":"18:e8:29:9b:42:27","signalStrength":-26},{"SSID":"boomland","MAC":"bc:df:58:ca:64:d5","signalStrength":-84},{"SSID":"EEMN Network","MAC":"18:e8:29:9a:42:27","signalStrength":-32},{"SSID":"Ziggo-ap-4098a8b","MAC":"c4:71:54:09:8a:8b","signalStrength":-81},{"SSID":"Gasten","MAC":"ce:ce:1e:2c:a1:70","signalStrength":-80},{"SSID":"KPN69DF26","MAC":"e4:75:dc:1d:cc:63","signalStrength":-82},{"SSID":"REMOTE32usxw","MAC":"00:1d:c9:07:e9:7d","signalStrength":-80},{"SSID":"VR46","MAC":"e4:75:dc:e9:cd:d9","signalStrength":-83},{"SSID":"boomland","MAC":"bc:df:58:c7:70:f9","signalStrength":-82},{"SSID":"","MAC":"e6:75:dc:2d:cc:63","signalStrength":-80},{"SSID":"FRITZ!Box 5490 DC","MAC":"cc:ce:1e:2c:a1:70","signalStrength":-83}]
        
        foreach(string res in intermediate) {
            string wrappedJson = "{\"measurements\":" + res + "}";
            print("Result: " + wrappedJson);
            
            Sample sample = JsonUtility.FromJson<Sample>(wrappedJson);
            sample.location = currentPosition;
            
            samples.Add(sample);
        }

        intermediate.Clear();

        if(samplePoints.Count > 2) {
            GameObject past = Instantiate(indicator);
            past.GetComponent<MeshRenderer>().material.color = Color.green;
            
            samplePoints.RemoveAt(0);
            preprareMeasurement(samplePoints[0]);
        } else {
            samplePoints.Clear();
            start.GetComponent<Button>().enabled = false;
        }

        int length = saveToCSV(samples, fileName);
        VisualLogger.Log($"Saved {length} characters to {fileName}");
    }

    private void sampleGrid() {
        samplePoints.Clear();
        foreach(FloorHeight floorHeight in floorHeights) {
            for (float x = bounds.min.x; x < bounds.max.x; x += gridSize)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += gridSize)
                {
                    // Check if the grid point is inside the floor shape
                    Vector3 gridPoint = new Vector3(x, floorHeight.height, z);

                    if (isPointOnNavMesh(gridPoint))
                    {
                        samplePoints.Add(gridPoint);
                        print(gridPoint + " on Mesh");
                    } else {
                        missedPoints.Add(gridPoint);
                    }
                    print(gridPoint + " NOT ON MESH");
                }
            }
        }
    }

    private void sampleRandom() {
        samplePoints.Clear();
        for(int i = 0; i < randomSampleAmount; i++) {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                0,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            bool onMesh = isPointOnNavMesh(randomPoint);
            if(onMesh) {
                samplePoints.Add(randomPoint);
            } else {
                i--;
            }
        }
    }

    private void sampleCustom() {
        var sorted = Utilities.SortGameObjects(Utilities.GetAllChildren(customSampleSet));
        foreach(GameObject item in sorted) {
            samplePoints.Add(item.transform.position);
        }
    }

    private bool isPointOnNavMesh(Vector3 point) {
        NavMeshHit hit;
        return NavMesh.SamplePosition(point, out hit,0.1f, NavMesh.AllAreas);
    }

    public static int saveToCSV(List<Sample> samples, string fileName) {
        string outputCsv = ProcessSamples("Data/AP-MAC", samples);

        string path = Path.Combine(Application.persistentDataPath, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, outputCsv);

        return outputCsv.Length;
    }



    public static string ProcessSamples(string apCsvContent, List<Sample> samples)
    {
        // Parse the AP CSV content
        var aps = ParseAPCsv(apCsvContent);
        
        // Prepare CSV header
        StringBuilder csvBuilder = new StringBuilder();
        csvBuilder.Append("Location X,Location Y,Location Z");
        foreach (var ap in aps)
        {
            csvBuilder.Append($",{ap.Key}"); // AP Name as header
        }
        csvBuilder.AppendLine();

        // Process each sample
        foreach (var sample in samples)
        {
            csvBuilder.Append($"{sample.location.x.ToString(CultureInfo.InvariantCulture)},{sample.location.y.ToString(CultureInfo.InvariantCulture)},{sample.location.z.ToString(CultureInfo.InvariantCulture)}");

            Dictionary<string, float> macSignalMap = new Dictionary<string, float>();
            foreach (var measurement in sample.measurements)
            {
                macSignalMap[measurement.MAC] = measurement.signalStrength;
            }

            foreach (var ap in aps)
            {
                float signalStrength = 1; // Default signal strength
                if (macSignalMap.ContainsKey(ap.Value))
                {
                    signalStrength = macSignalMap[ap.Value];
                }
                csvBuilder.Append($",{signalStrength}");
            }
            csvBuilder.AppendLine();
        }

        return csvBuilder.ToString();
    }

    
    public static Dictionary<string, string> ParseAPCsv(string path)
    {
        string csvContent = Resources.Load<TextAsset>(path).text;
        Dictionary<string, string> aps = new Dictionary<string, string>();
        using (System.IO.StringReader reader = new System.IO.StringReader(csvContent))
        {
            string line = reader.ReadLine(); // Skip header
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                if (items.Length >= 3)
                {
                    string apName = items[0];
                    string macAddress = items[1];
                    aps[apName] = macAddress;
                }
            }
        }
        print(aps);
        return aps;
    }
}

public enum SamplingMethod {
    Random,
    Grid,
    Custom
}

[System.Serializable]
public class Sample {
    public Vector3 location;
    public List<Measurement> measurements = new List<Measurement>();

    public override string ToString()
    {
        return $"Location: {location}, Measurement nr.: {measurements.Count}";
    }

    public string toCSV() {
        StringBuilder csv = new StringBuilder();

        return csv.ToString();
    }
}

[System.Serializable]
public class Measurement {

    public Measurement(string SSID, string MAC, float signalStrength) {
        this.SSID = SSID;
        this.MAC = MAC;
        this.signalStrength = signalStrength;
    }
    public Measurement(string MAC, float signalStrength, Dictionary<string, float> obstacles) {
        this.MAC = MAC;
        this.signalStrength = signalStrength;
        this.obstacles = obstacles;
    }

    override public string ToString() {
        return $"SSID: {SSID}, MAC: {MAC}, RSSI: {signalStrength}";
    }

    public string SSID;
    public string MAC;
    public float signalStrength;
    public Dictionary<string, float> obstacles;
}

[System.Serializable]
public class FloorHeight {
    public string name;
    public float height;
}
