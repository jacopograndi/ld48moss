using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantLogic : MonoBehaviour {

    public int production = 1;
    public float prodCooldown = 2f;
    float prodTimer = 2f;

    public Vector3Int cell;

    PlantManagement pm;

    public float maxHp = 100;
    public float hp;

    Transform hpBar;

    void Start() {
        pm = FindObjectOfType<PlantManagement>();
        prodTimer = Time.time + prodCooldown;

        hpBar = transform.Find("hpbar").Find("pivot");
    }

    void Produce () {
        pm.reslight += production;
    }

    void Update() {
        if (prodTimer < Time.time) {
            Produce();
            prodTimer = Time.time + prodCooldown;
        }

        hpBar.localScale = new Vector3(hp / maxHp, 1, 1);
    }
}
