using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LocationMarkers : MonoBehaviour
{
    [System.Serializable]
    public class Marker
    {
        public Transform targetTransform;
        public Sprite iconSprite;
    }

    public Marker[] locations;
    public GameObject iconPrefab;
    public Transform canvasParent;

    private Image[] markerIcons;
    private Camera mainCam;
    private bool isVisible = false;

    public static LocationMarkers Instance { get; private set; }

    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        mainCam = Camera.main;
        markerIcons = new Image[locations.Length];

        for (int i = 0; i < locations.Length; i++)
        {
            GameObject newMarker = Instantiate(iconPrefab, canvasParent);

            newMarker.name = $"{locations[i].targetTransform.gameObject.name}_Icon";

            markerIcons[i] = newMarker.GetComponent<Image>();

            // Apply the specific icon if one is assigned
            if (locations[i].iconSprite != null)
            {
                markerIcons[i].sprite = locations[i].iconSprite;
            }

            newMarker.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle icons with the 'M' key
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            isVisible = !isVisible;
            foreach (var img in markerIcons) img.gameObject.SetActive(isVisible);
        }

        if (isVisible) UpdateMarkerPositions();
    }

    void UpdateMarkerPositions()
    {
        for (int i = 0; i < locations.Length; i++)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(locations[i].targetTransform.position);

            // Hide the UI icon if the location is behind the player's camera
            if (screenPos.z < 0)
            {
                markerIcons[i].gameObject.SetActive(false);
            }
            else
            {
                markerIcons[i].gameObject.SetActive(true);
                markerIcons[i].transform.position = screenPos;
            }
        }
    }
    public static string GetTopTwoClosestLocations(Transform target)
    {
        if (Instance == null || Instance.locations == null) return "Locations unavailable.";

        // Sort locations by distance to the target
        var sorted = new List<Marker>(Instance.locations);
        sorted.Sort((a, b) =>
            Vector3.Distance(target.position, a.targetTransform.position)
            .CompareTo(Vector3.Distance(target.position, b.targetTransform.position)));

        string result = "Closest Locations: ";
        for (int i = 0; i < Mathf.Min(2, sorted.Count); i++)
        {
            float dist = Vector3.Distance(target.position, sorted[i].targetTransform.position);
            result += $"{sorted[i].targetTransform.gameObject.name} ({dist:F1}m)";
            if (i == 0) result += ", ";
        }

        return result;
    }
}