using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class Enricher : MonoBehaviour {

    public SerializableDictionary<string, bool> options = new SerializableDictionary<string, bool>() {
        {"YesNo", false},
        {"Thickness", false},
        {"ObstacleCount", false},
        {"APCount", false},
        {"APDistance", false},
    };
    public SerializableDictionary<string, SerializableList<string>> results = new SerializableDictionary<string, SerializableList<string>>();
    public TextAsset file;
    public string fileName;
    public string[] layerNames;
    public List<string> APs = new List<string>();
    public List<Vector3> sampleLocations = new List<Vector3>();

    CultureInfo culture = CultureInfo.InvariantCulture;

    public (List<string> columnNames, List<Vector3> locations) ParseCsv()
    {
        string csvContent = file.text;

        APs.Clear();
        sampleLocations.Clear();
        results.Clear();
        using (StringReader reader = new StringReader(csvContent))
        {
            // Read the first line to get column headers
            string line = reader.ReadLine();
            if (line != null)
            {
                APs.AddRange(line.Split(','));
                APs.RemoveAll(x => x == "Location X" || x == "Location Y" || x == "Location Z" || x == "x" || x == "y" || x == "z" || x == "X" || x == "Y" || x == "Z");

                // Read the subsequent lines to extract locations
                while ((line = reader.ReadLine()) != null)
                {
                    string[] entries = line.Split(',');
                    if (entries.Length >= 3)
                    {
                        NumberStyles style = NumberStyles.AllowDecimalPoint;
                        // Attempt to parse the first three entries as float values for Vector3
                        if (float.TryParse(entries[0], style, culture, out float x) && 
                            float.TryParse(entries[1], style, culture, out float y) && 
                            float.TryParse(entries[2], style, culture, out float z))
                        {
                            Vector3 location = new Vector3(x, y, z);
                            sampleLocations.Add(location);

                            AddToResults("x", x.ToString(culture));
                            AddToResults("y", y.ToString(culture));
                            AddToResults("z", z.ToString(culture));
                            
                        }

                        // Add RSSI values
                        for(int i = 3; i < entries.Length; i++) {
                            AddToResults(APs[i-3], entries[i].Trim());
                        }
                    }
                }
            }
            Debug.Log($"Found {sampleLocations.Count} samples from {APs.Count} APs");
            return (APs, sampleLocations);
        }
    }

    public void Enrich() {
        ParseCsv();
        CastRays();
        Save();
    }
    private SerializableDictionary<(Vector3, Vector3), RaycastHit[]> rays = new SerializableDictionary<(Vector3, Vector3), RaycastHit[]>();

    private (Ray, float) createRay(Vector3 start, Vector3 end) {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        return (new Ray(start, direction), distance);
    }

    public void CastRays() {
        List<(string, (Ray, float), (Ray, float))> rayList = new List<(string, (Ray, float), (Ray, float))>();

        for(int i = 0; i < sampleLocations.Count; i += 60) {
            Vector3 loc = sampleLocations[i];

            for(int j = 0; j < APs.Count; j++) {
                string AP = APs[j];

                GameObject ap = GameObject.Find(AP);

                if(ap == null) continue;

                // raySources.Append((loc, ap.transform.position));
                var forward = createRay(loc, ap.transform.position);
                var backward = createRay(ap.transform.position, loc);

                rayList.Add((AP, forward, backward));
            }
        }

        foreach((string AP, (Ray forward, float forwardDistance), (Ray backward, float backwardDistance)) in rayList) {
            // RaycastHit[] forwardHits = Physics.RaycastAll(forward.origin, forward.direction, forwardDistance);
            // RaycastHit[] backwardHits = Physics.RaycastAll(backward.origin, backward.direction, backwardDistance);

            // bool obstaclePresent = forwardHits.Length > 0;
            // int obstacleCount = forwardHits.Length;
            // float obstacleThickness = Utilities.CalculateObstacleThickness(forward.origin, forwardHits, backwardHits);
            float distance = Vector3.Distance(forward.origin, backward.origin);

            // Debug.Log($"From {forward.origin} to {backward.origin} = {AP}, present: {obstaclePresent}, count: {obstacleCount}, thick: {obstacleThickness}");

            for(int i = 0; i < 60; i++) {
                // AddToResults(AP+"_obstacle_present", obstaclePresent ? "1" : "0");
                // AddToResults(AP+"_obstacle_count", obstacleCount.ToString());
                // AddToResults(AP+"_obstacle_thickness", obstacleThickness.ToString(culture));
                AddToResults(AP+"_distance", distance.ToString(culture));
                AddToResults(AP+"_x", backward.origin.x.ToString(culture));
                AddToResults(AP+"_y", backward.origin.y.ToString(culture));
                AddToResults(AP+"_z", backward.origin.z.ToString(culture));
            }

        }

    }

    private void AddToResults(string key, string value) {
        if(results.ContainsKey(key)) {
            results[key].Add(value);
        } else {
            results.Add(key, new SerializableList<string>(){value});
        }
    }
 
    private void APCount() {
        Debug.Log("Enrich in range count");

        for(int i = 0; i < sampleLocations.Count; i++) {
            int count = 0;
            foreach(string AP in APs) {
                if(results.GetValueOrDefault(AP)[i] != "1") count++;
            }
            AddToResults("APs_in_range", count.ToString());
        }
    }

    private Dictionary<(Vector3, Vector3), float> distanceCache = new Dictionary<(Vector3, Vector3), float>();
    private void APDistance() {
        Debug.Log("Enrich distance");

        foreach(Vector3 loc in sampleLocations){
            foreach(string AP in APs) {
                GameObject ap = GameObject.Find(AP);

                if(ap == null) continue;

                if(distanceCache.ContainsKey((loc, ap.transform.position))) {
                    AddToResults(AP + "_distance", distanceCache.GetValueOrDefault((loc, ap.transform.position)).ToString(culture));
                    continue;
                }

                float distance = Vector3.Distance(loc, ap.transform.position);
                distanceCache.Add((loc, ap.transform.position), distance);

                AddToResults(AP+"_distance", distance.ToString(culture));
            }
        }
    }

    public void Save() {
        Debug.Log("Saving...");
        List<string> csv = Utilities.ConvertDictionaryToCsvLines(results);
        // string formattedDateTime = System.DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");

        string opt = "-ENRICHED";

        foreach((string key, bool value) in options) {
            if(value) opt += "-" + key;
        }

        Utilities.SaveToFile(csv, fileName ?? $"{file.name}{opt}.csv");
    }

}