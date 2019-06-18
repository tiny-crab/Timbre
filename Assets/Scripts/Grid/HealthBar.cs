using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar {

    public GameObject fullBar;
    public GameObject healthMarker0;
    public GameObject healthMarker1;
    public GameObject healthMarker2;
    public GameObject healthMarker3;
    public List<GameObject> healthMarkers = new List<GameObject>();

    public List<Color> healthTierList = new List<Color>() {
        Color.black,
        Color.red,
        Color.blue,
        Color.green
    };

    private int currentHealth;

    public HealthBar(GameObject healthBar, int initialHealth) {
        fullBar = healthBar;

        healthMarker0 = healthBar.transform.Find("0").gameObject;
        healthMarker1 = healthBar.transform.Find("1").gameObject;
        healthMarker2 = healthBar.transform.Find("2").gameObject;
        healthMarker3 = healthBar.transform.Find("3").gameObject;
        healthMarkers = new List<GameObject>() {
            healthMarker0, healthMarker1, healthMarker2, healthMarker3
        };

        healthMarkers.ForEach(marker => marker.GetComponent<SpriteRenderer>().color = healthTierList[0]);

        currentHealth = initialHealth;
        RecalculateHealth();
    }

    public void Update () {
        RecalculateHealth();
    }

    public void IncrementHealth(int number = 1) {
        
    }

    public void DecrementHealth(int number = 1) {

    }

    private void RecalculateHealth() {
        var healthTier = Mathf.FloorToInt(currentHealth / 4);
        var fill = currentHealth % 4;
        if (healthTier > healthTierList.Count) {
            healthTier = healthTierList.Count - 1;
            fill = 0;
        }
        
        healthMarkers.ForEach(marker => marker.GetComponent<SpriteRenderer>().color = healthTierList[healthTier]);
        for (int i = 0; i < fill; i++) {
            var marker = healthMarkers[i];
            marker.GetComponent<SpriteRenderer>().color = healthTierList[healthTier + 1];
        }
    }
}