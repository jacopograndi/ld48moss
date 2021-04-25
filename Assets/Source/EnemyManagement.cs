using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint {
    public Vector3Int pt;
    public Room room;
}

public class EnemyManagement : MonoBehaviour {

    PlantManagement pm;
    GroundGeneration gg;

    public List<GameObject> enemyPrefab = new List<GameObject>();
    public List<GameObject> enemyPool = new List<GameObject>();

    GameObject EnemyFather;
    public List<EnemyLogic> enemies = new List<EnemyLogic>();

    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public float spawnCooldown;
    public float spawnTimer;
    public int phase = 0;

    void Start() {
        pm = FindObjectOfType<PlantManagement>();
        gg = FindObjectOfType<GroundGeneration>();
        EnemyFather = GameObject.Find("Enemies");
        enemyPool.Add(enemyPrefab[0]);
    }

    void Spawn () {/*
        List<SpawnPoint> filterMoss = new List<SpawnPoint>();
        foreach(SpawnPoint sp in spawnPoints) {
            if (sp.room.hasMoss) filterMoss.Add(sp);
        }*/
        Vector3Int pt = spawnPoints[0].pt;
        Vector3 worldpt = gg.groundtilemap.CellToWorld(pt);
        worldpt += new Vector3(0.25f / 2.0f, 0.25f / 2.0f, -1);
        GameObject pref = enemyPool[Random.Range(0, enemyPool.Count)];
        GameObject obj = Instantiate(pref, worldpt, Quaternion.identity);
        obj.transform.SetParent(EnemyFather.transform);
        EnemyLogic el = obj.GetComponent<EnemyLogic>();
        enemies.Add(el);
    }

    void Update() {
        if (spawnTimer < Time.time) {
            spawnTimer = Time.time + spawnCooldown;
            Spawn();
        }

        spawnCooldown -= Time.deltaTime * 0.01f;
        spawnCooldown = Mathf.Max(0.2f, spawnCooldown);

        if (phase == 0 && spawnCooldown < 1.7f) {
            enemyPool.Add(enemyPrefab[1]);
            phase = 1;
        }
        if (phase == 1 && spawnCooldown < 1f) {
            enemyPool.Add(enemyPrefab[2]);
            phase = 2;
        }
        if (phase == 2 && spawnCooldown < 0.5f) {
            enemyPool.Add(enemyPrefab[2]);
            phase = 3;
        }

        List<EnemyLogic> delList = new List<EnemyLogic>();
        foreach (EnemyLogic en in enemies) {
            if (en.hp <= 0) {
                delList.Add(en);
            }
        }
        foreach (EnemyLogic en in delList) {
            enemies.Remove(en);
            Destroy(en.gameObject);
            pm.kills++;
        }
    }
}
