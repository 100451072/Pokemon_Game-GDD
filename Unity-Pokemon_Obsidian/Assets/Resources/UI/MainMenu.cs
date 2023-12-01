using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject battleSystem;
    public GameObject panelMiniMap;

    void Start()
    {
        if (panelMiniMap != null)
            panelMiniMap.SetActive(false);
    }

    void Update()
    {
        if (battleSystem != null)
            TogglePanel();
    }

    private void TogglePanel()
    {
        if (battleSystem != null)
            panelMiniMap.gameObject.SetActive(!battleSystem.gameObject.activeSelf);
    }
}
