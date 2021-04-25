using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class EnemyLogic : MonoBehaviour {

    // move to nearest flower
    // stop and eat whatever is in the way

    public GameObject debug;

    GroundGeneration gg;
    PlantManagement pm;

    List<Vector3Int> path = new List<Vector3Int>();
    float pathTimer;
    public float pathCooldown;

    public float speed = 10f;

    public float maxHp = 10;
    public float hp;
    public float damage = 3f;

    public float range = 0.3f;

    public float attackCooldown;
    float attackTimer;

    Transform hpBar;

    Rigidbody2D rb;

    Vector3Int[] dirToVec = new Vector3Int[4] {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, -1, 0)
    };

    bool ListTupleContains (List<Tuple<Vector3Int, Vector3Int>> l, Vector3Int n) {
        foreach(Tuple<Vector3Int, Vector3Int> t in l) {
            if (t.Item2 == n) return true;
        }
        return false;
    }

    Vector3Int GetFromFirst (List<Tuple<Vector3Int, Vector3Int>> l, Vector3Int n) {
        foreach (Tuple<Vector3Int, Vector3Int> t in l) {
            if (t.Item1 == n) return t.Item2;
        }
        return new Vector3Int();
    }

    bool CheckMove (Vector3Int start) {
        if (start.y <= 0) {
            TileBase tile = gg.wallstilemap.GetTile(start);
            if (!tile) { return true; }
        }
        return false;
    }

    List<Vector3Int> GetPath () {
        Vector3 mp = transform.position; mp.z = 0;
        Vector3Int pos = gg.groundtilemap.WorldToCell(mp);

        bool finish = false;
        Vector3Int goal = new Vector3Int();
        List<Tuple<Vector3Int, Vector3Int>> backref = new List<Tuple<Vector3Int, Vector3Int>>();
        List<Vector3Int> visited = new List<Vector3Int>();
        List<Vector3Int> frontier = new List<Vector3Int>();
        frontier.Add(pos);
        int iter = 0;
        for (; iter < 500; iter++) {
            Vector3Int node = frontier[0];
            for (int d=0; d<4; d++) {
                Vector3Int neigh = node + dirToVec[d]; neigh.z = 0;
                if (neigh.y <= 0 
                    && !visited.Contains(neigh)
                    && !frontier.Contains(neigh)) {
                    TileBase tile = gg.wallstilemap.GetTile(neigh);
                    if (!tile) {
                        backref.Add(new Tuple<Vector3Int, Vector3Int>(neigh, node));
                        frontier.Add(neigh);
                        PlantLogic plant = pm.PlantFromVec(neigh);
                        if (plant) {
                            goal = neigh;
                            finish = true;
                        }
                    }
                }
            }
            if (finish) break;
            visited.Add(node);
            frontier.RemoveAt(0);
            if (frontier.Count == 0) break;
        }

        List<Vector3Int> path = new List<Vector3Int>();

        if (!finish) {
            int randDir = UnityEngine.Random.Range(0, 4);
            Vector3Int rpos = pos + dirToVec[randDir];
            if (CheckMove(rpos)) path.Add(rpos);
            return path;
        }


        iter = 0;
        for (; iter < 100; iter++) {
            path.Add(goal);
            goal = GetFromFirst(backref, goal);
            if (goal == pos) break;
        }
        if (iter >= 100) {
            int randDir = UnityEngine.Random.Range(0, 4);
            Vector3Int rpos = pos + dirToVec[randDir];
            if (CheckMove(rpos)) path.Add(rpos);
            return path;
        }
        path.Reverse();
        return path;
    }

    void Start() {
        gg = FindObjectOfType<GroundGeneration>();
        pm = FindObjectOfType<PlantManagement>();
        rb = GetComponent<Rigidbody2D>();

        attackTimer = Time.time + attackCooldown;

        hpBar = transform.Find("hpbar").Find("pivot");
    }

    void Update() {
        if (attackTimer < Time.time) {
            foreach (PlantLogic pl in pm.plants) {
                Vector2 diff = pl.transform.position - transform.position;
                if (diff.SqrMagnitude() <= range * range) {
                    attackTimer = Time.time + attackCooldown;
                    pl.hp -= damage;
                    break;
                }
            }
        }

        if (path.Count == 0 && pathTimer < Time.time) {
            path = GetPath();
            pathTimer = Time.time + pathCooldown;
        }

        if (path.Count > 0 && attackTimer < Time.time) {
            /*
            foreach (Transform t in transform) Destroy(t.gameObject);
            foreach (Vector3Int p in path) {
                Vector3 pp = gg.groundtilemap.CellToWorld(p);
                pp.x += 0.25f/2;
                pp.y += 0.25f/2;
                pp.z = -1;
                var obj = Instantiate(debug, pp, Quaternion.identity);
                obj.transform.SetParent(transform);
            }*/
            Vector3 pathpos = gg.groundtilemap.CellToWorld(path[0]);
            pathpos.x += 0.25f / 2;
            pathpos.y += 0.25f / 2;
            pathpos.z = -1;

            transform.position = Vector3.MoveTowards(
                transform.position, pathpos, 0.001f * speed);
            if (Vector3.Distance(pathpos, transform.position) < 0.11f) {
                path.RemoveAt(0);
            }
        }

        hpBar.localScale = new Vector3(hp/maxHp, 1, 1);
    }
}
