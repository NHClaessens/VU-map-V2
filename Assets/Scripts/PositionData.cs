using System.Collections.Generic;
using UnityEngine;

public class PositionData : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public SerializableDictionary<string, string> rssi;
    public SerializableDictionary<string, float> obstacleThickness;
    public SerializableDictionary<string, int> obstacleCount;
    public SerializableDictionary<string, bool> obstaclePresent;
}