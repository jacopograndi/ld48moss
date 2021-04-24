using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint {
    public Vector3Int pt;
    public Room room;
}

public class EnemyManagement : MonoBehaviour {

    GroundGeneration gg;

    public GameObject enemyPrefab;

    GameObject EnemyFather;
    public List<EnemyLogic> enemies = new List<EnemyLogic>();

    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public float spawnCooldown;
    public float spawnTimer;

    void Start() {
        gg = FindObjectOfType<GroundGeneration>();
        EnemyFather = GameObject.Find("Enemies");
    }

    void Spawn () {
        List<SpawnPoint> filterMoss = new List<SpawnPoint>();
        foreach(SpawnPoint sp in spawnPoints) {
            if (sp.room.hasMoss) filterMoss.Add(sp);
        }
        Vector3Int pt = filterMoss[Random.Range(0, filterMoss.Count-1)].pt;
        Vector3 worldpt = gg.groundtilemap.CellToWorld(pt);
        worldpt += new Vector3(0.25f / 2.0f, 0.25f / 2.0f, -1);
        GameObject obj = Instantiate(enemyPrefab, worldpt, Quaternion.identity);
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
        spawnCooldown = Mathf.Max(0.5f, spawnCooldown);

        List<EnemyLogic> delList = new List<EnemyLogic>();
        foreach (EnemyLogic en in enemies) {
            if (en.hp <= 0) {
                delList.Add(en);
            }
        }
        foreach (EnemyLogic en in delList) {
            enemies.Remove(en);
            Destroy(en.gameObject);
        }
    }
}
