using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeasureDistance : MonoBehaviour
{
    public void Measure(String APname) {
        GameObject ap = GameObject.Find(APname);
        List<RaycastHit> hits = new List<RaycastHit>();
        Vector3 origin = transform.position;
        Vector3 destination = ap.transform.position;

        float maxDistance = Vector3.Distance(origin, destination);
        float totalDistance = 0;


        Color[] colors = new Color[]{Color.green, Color.red};

        Physics.queriesHitBackfaces = true;
        while (totalDistance < maxDistance)
        {
            RaycastHit hit;
            Ray ray = new Ray(origin, destination - origin);
            
            if (Physics.Raycast(ray, out hit))
            {
                totalDistance += Vector3.Distance(origin, hit.point);

                origin = hit.point + ray.direction.normalized / 100.0f;

                hits.Add(hit);
            }
        }

        float totalWallThickness = 0;
        for(int i = 0; i < hits.Count; i+=2) {
            if(i + 1 < hits.Count) {
                Debug.DrawLine(hits[i].point, hits[i+1].point, colors[i % 2], 999);
                totalWallThickness += Vector3.Distance(hits[i].point, hits[i+1].point);
            }
        }

        Debug.Log("Total wall thickness: " + totalWallThickness);
    }
}
