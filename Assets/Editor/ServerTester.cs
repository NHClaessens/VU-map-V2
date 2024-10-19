using UnityEngine;
using UnityEditor;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq;

public class RSSIRequestWindow : EditorWindow
{
    public string rssiJson = "{\"rssi\": {\"AP1\": -70, \"AP2\": -75}}";
    public string endpoint = "/predict/default";
    public Vector3 actualLocation;
    private static HttpClient client = new HttpClient();
    private bool drawLines = false;
    private bool drawCircle = false;
    private bool drawDifference = false;

    private TcpClient socketClient;
    private static NetworkStream stream;
    private Thread receiveThread;
    private bool socketConnected = false;
    private string outgoing = "";
    private static List<GameObject> APs = new List<GameObject>();
    private static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();



    [MenuItem("Tools/Multi Layer Server")]
    public static void ShowWindow()
    {
        GetWindow(typeof(RSSIRequestWindow), false, "RSSI Data Sender");
        APs = Utilities.GetAllBottomLevelGameObjects(GameObject.Find("APs"));
    }

    async void OnGUI()
    {
        endpoint = EditorGUILayout.TextField(endpoint);
        GUILayout.Label("Enter RSSI Data as JSON", EditorStyles.boldLabel);
        rssiJson = EditorGUILayout.TextArea(rssiJson);

        actualLocation = EditorGUILayout.Vector3Field("Actual location", actualLocation);

        drawLines = GUILayout.Toggle(drawLines, "Draw lines");
        drawCircle = GUILayout.Toggle(drawCircle, "Draw circle");
        drawDifference = GUILayout.Toggle(drawDifference, "Draw difference");

        if (GUILayout.Button("Send RSSI Data"))
        {
            // SendDataToServer(rssiJson);
            JObject res = await API.Post(endpoint, rssiJson);
            HandleResponse(res);
        }
        if (GUILayout.Button("Clear lines")) {
            distances.Clear();
            SceneView.RepaintAll();
        }

        GUILayout.Space(64);

        EditorGUILayout.LabelField("Socket connected: " + socketConnected);
        EditorGUILayout.LabelField("APs found: " + APs.Count);
        if (GUILayout.Button("Connect to socket")) {
            connectToSocket();
        }
        if (GUILayout.Button("Disconnect socket")) {
            stream.Close();
            socketClient.Close();
        }
        outgoing = EditorGUILayout.TextArea(outgoing);
        if (GUILayout.Button("Send data")) {
            SendMessage(outgoing);
            outgoing = "";
        }
    }

    private void connectToSocket() {
        try {
            socketClient = new TcpClient("127.0.0.1", 65432);
            stream = socketClient.GetStream();
            socketConnected = true;
            EditorApplication.update += Update;

            APs = Utilities.GetAllBottomLevelGameObjects(GameObject.Find("APs"));

            Debug.Log("Connected to socket");

            receiveThread = new Thread(new ThreadStart(ReceiveSocket));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        } catch (Exception e) {
            Debug.LogError("Could not connect to socket: " + e.Message);
        }
    }

    static void SendMessage(string message)
    {
        if (stream == null)
        {
            Debug.LogError("No connection to the server.");
            return;
        }

        try
        {
            // Convert message to bytes and send
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data: " + e.Message);
        }
    }

    private void ReceiveSocket() {
        // try
        // {
            byte[] data = new byte[2048];
            while (socketConnected)
            {
                int bytes = stream.Read(data, 0, data.Length);
                if (bytes > 0)
                {
                    string message = Encoding.ASCII.GetString(data, 0, bytes);
                    Debug.Log("Received: " + message);
                    ExecuteCommand(JObject.Parse(message));
                }
            }
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError("Error receiving data: " + e.Message);
        // }
    }

    private void ExecuteCommand(JObject json) {
        Debug.Log("Command received: " + json["type"] + " data: " + json["data"]);

        if(json["type"].ToString() == "obstacles") {
            messageQueue.Enqueue(json.ToString());
        }
    }

    static void Update() {
        while (messageQueue.TryDequeue(out string message))
        {
            JObject json = JObject.Parse(message);

            if(json["type"].ToString() == "obstacles") {
                Vector3 from = new Vector3(float.Parse(json["data"]["x"].ToString()), float.Parse(json["data"]["y"].ToString()), float.Parse(json["data"]["z"].ToString()));

                string result = JsonConvert.SerializeObject(Utilities.ApObstacles(APs, from));
                json["data"]["obstacle_thickness"] = result;
            }
            SendMessage(json.ToString());
        }
    }


    public List<DistanceData> distances = new List<DistanceData>();
    public Vector3 guess;
    public Vector3 adjustedGguess;
    void HandleResponse(JObject res) {
        // Temporary dictionary to hold positions by AP names
        Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();


        foreach (var item in res["distances"])
        {
            string APname = item[0].ToString().Split("_")[0];
            float distance = (float)item[1];

            distances.Add(new DistanceData(
                GameObject.Find(APname).transform.position,
                distance
            ));
        }

        guess = EstimatePosition(distances);
        adjustedGguess = AdjustToNavMesh(guess, 2);
        Debug.Log(guess);
        Debug.Log(adjustedGguess);
        SceneView.RepaintAll();
    }

    Vector3 EstimatePosition(List<DistanceData> estimations) {
        Vector3 weightedPosition = Vector3.zero;
        float totalWeight = 0f;

        foreach (var estimation in estimations) {
            float weight = 1f / (estimation.distance * estimation.distance);
            weightedPosition += estimation.position * weight;
            totalWeight += weight;
        }

        return weightedPosition / totalWeight;
    }

    Vector3 AdjustToNavMesh(Vector3 estimatedPosition, float maxDistance) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(estimatedPosition, out hit, maxDistance, NavMesh.AllAreas)) {
            return new Vector3(hit.position.x, hit.position.y + 1.3f, hit.position.z);
        } else {
            // No valid NavMesh position found within maxDistance; handle accordingly
            return estimatedPosition; // Or some fallback logic
        }
    }

    void OnFocus()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void OnSceneGUI(SceneView sceneView) {
        if(drawLines) {
            foreach(DistanceData data in distances) {
                Vector3 end = Vector3.MoveTowards(data.position, guess, data.distance);
                Handles.DrawDottedLine(data.position, end, 10);
            }
        }

        if(drawCircle) {
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, guess, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.yellow;
            Handles.SphereHandleCap(0, adjustedGguess, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.white;
        }

        if(drawDifference) {
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, actualLocation, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.red;
            Handles.DrawLine(guess, actualLocation);
            Handles.color = Color.yellow;
            Handles.DrawLine(adjustedGguess, actualLocation);
            Handles.color = Color.white;
        }
    }

    public struct DistanceData {
        public DistanceData(Vector3 pos, float dist){
            position = pos;
            distance = dist;
        }
        
        public Vector3 position;
        public float distance;
    }
}