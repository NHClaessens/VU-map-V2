using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CSVPlotterSettings", menuName = "CSV Plotter/Settings", order = 1)]
public class CSVImporterSettings : ScriptableObject
{
    public List<CSVImportItem> items = new List<CSVImportItem>();
}

[Serializable]
public class CSVImportItem {
    public TextAsset[] csvFiles;
    public Color color;
    public string parentObjectName = "PlottedPoints";
    public string xColumnName = "X";
    public string yColumnName = "Y";
    public string zColumnName = "Z";
    public string nameColumnName = "";
    public float xAddition = 0;
    public float yAddition = 0;
    public float zAddition = 0;
    public float xMultiplier = 1;
    public float yMultiplier = 1;
    public float zMultiplier = 1;
    public string[] heightReplacements;
}
