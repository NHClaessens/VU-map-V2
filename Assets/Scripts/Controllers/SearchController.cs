using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SearchController : MonoBehaviour
{
    public bool ignoreCapitals;
    public bool ignoreWhiteSpace;
    public bool sortByDistance;
    public bool moveCamera;
    public CameraController cameraController;
    public GameObject POIs;
    public GameObject SearchResults;
    public GameObject SearchResultTemplate;
    public GameObject SearchResultsContent;

    private List<GameObject> results = new List<GameObject>();

    public void Search(string query) {
        results.Clear();
        POIDisplay.SetActive(false);
        SearchResults.SetActive(query !="");
        if(query == "") {
            ClearResults();
            return;
        }

        query = clean(query);

        //TODO: only show results from current floor on map, show on floor selector how many results for each floor
        foreach(Transform child in POIs.transform) {
            POI poi = child.GetComponent<POI>();
            if(clean(poi.title).Contains(clean(query))) {
                results.Add(child.gameObject);
                continue;
            }
            foreach(string tag in poi.tags) {
                if(clean(tag).Contains(clean(query))) {
                    results.Add(poi.gameObject);
                    break;
                }
            }
        }

        if(sortByDistance && results.Count > 0) {
            results.Sort();
        }

        Debug.Log(results.Count);

        DisplayResults(results);
    }

    private void ClearResults() {
        Utilities.DeleteAllChildren(SearchResultsContent);
    }

    private void DisplayResults(List<GameObject> results) {
        ClearResults();

        int index = 0;
        foreach(GameObject result in results) {
            POI poi = result.GetComponent<POI>();

            GameObject newTemplate = Instantiate(SearchResultTemplate);
            newTemplate.transform.Find("Title").GetComponent<TMP_Text>().text = poi.title;
            newTemplate.transform.Find("Floor").GetComponent<TMP_Text>().text = $"F{poi.floor}";
            newTemplate.GetComponent<Index>().index = index;
            newTemplate.SetActive(true);

            newTemplate.transform.SetParent(SearchResultsContent.transform, false);
            index++;
        }
    }
    private string clean(string s) {
        s = s.Trim();
        if(ignoreCapitals) {
            s = s.ToLower();
        }

        if(ignoreWhiteSpace) {
            s = string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
        }

        return s;
    }

    public GameObject POIDisplay;
    private GameObject selectedPOI;
    public void SelectPOI(GameObject selected) {
        selectedPOI = results[selected.GetComponent<Index>().index];
        POI poi = selectedPOI.GetComponent<POI>();

        POIDisplay.transform.Find("Info/Title").gameObject.GetComponent<TMP_Text>().text = poi.title;
        POIDisplay.transform.Find("Info/Description").gameObject.GetComponent<TMP_Text>().text = poi.description;
        POIDisplay.SetActive(true);

        if(moveCamera) {
            cameraController.moveTo(selectedPOI.transform.position, 0.5f);
        }
    }

    public NavigationController navigationController;
    public void StartNavigation() {
        POIDisplay.SetActive(false);
        UIController.SetUIState("NavUI");

        navigationController.StartNavigation(selectedPOI.transform.position);

    }
}
