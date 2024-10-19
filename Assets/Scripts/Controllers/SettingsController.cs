using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    private TMP_InputField baseInput;

    private TMP_Dropdown endpointSelector;
    private JToken endpoints;

    async void Start()
    {
        baseInput = transform.Find("Base URL").GetComponentInChildren<TMP_InputField>();
        baseInput.text = API.baseUrl;

        endpoints = await API.Get("predict/options");
        Debug.Log(endpoints);

        endpointSelector = transform.Find("Method").gameObject.GetComponent<TMP_Dropdown>();
        List<string> options = new List<string>();

        foreach(JToken element in endpoints) {
            options.Add(element["title"].ToString());
        }

        endpointSelector.AddOptions(options);
    }

    public void ApplyBaseUrl() {
        API.baseUrl = baseInput.text;
    }

    public void SelectEndpoint(int index) {
        LocationController.selectedEndpoint = endpoints[index]["endpoint"].ToString();
    }

    public void GoToSampling() {
        SceneManager.LoadScene("Sampling");
    }
}