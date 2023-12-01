using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [Header("References")]
    public RectTransform minimapPoint_1;
    public RectTransform minimapPoint_2;
    public Transform worldPoint_1;
    public Transform worldPoint_2;

    [Header("References")]
    public RectTransform playerMinimap;
    public Transform playerWorld;

    public float xOffset;
    public float yOffset;

    private float minimapRatio;

    private void Awake()
    {
        CalculateMapRatio();
    }

    private void Update()
    {
        // Set the position of the little player in the map 
        playerMinimap.anchoredPosition = minimapPoint_1.anchoredPosition + new Vector2((playerWorld.position.x - worldPoint_1.position.x + xOffset) * minimapRatio,
            (playerWorld.position.y - worldPoint_1.position.y + yOffset) * minimapRatio);
    }

    public void CalculateMapRatio()
    {
        // Distance world ignoring Y axis
        Vector2 distanceWorldVector = worldPoint_1.position - worldPoint_2.position;
        float distanceWorld = distanceWorldVector.magnitude;

        // Distance minimap
        float distanceMinimap = Mathf.Sqrt(
            Mathf.Pow((minimapPoint_1.anchoredPosition.x - minimapPoint_2.anchoredPosition.x), 2) +
            Mathf.Pow((minimapPoint_1.anchoredPosition.y - minimapPoint_2.anchoredPosition.y), 2));

        minimapRatio = distanceMinimap / distanceWorld;
    }
}