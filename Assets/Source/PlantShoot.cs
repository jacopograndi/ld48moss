using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantShoot : MonoBehaviour {

    PlantManagement pm;
    EnemyManagement em;

    public GameObject ShotPrefab;

    public float shotCooldown;
    float shotTimer;
    public float damage;
    public float range = 2f;

    EnemyLogic target;

    void Start() {
        em = FindObjectOfType<EnemyManagement>();
        pm = FindObjectOfType<PlantManagement>();
        shotTimer = Time.time + shotCooldown;
    }

    void Scan() {
        float dist = float.PositiveInfinity;
        foreach (EnemyLogic en in em.enemies) {
            Vector2 diff = en.transform.position - transform.position;
            float r = range * pm.Bonuses().range;
            if (diff.magnitude < r) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position,
                    diff.normalized, diff.magnitude);
                if (hit) {
                    if (hit.collider.gameObject.tag == "Enemy") {
                        if (dist > diff.magnitude) {
                            target = en;
                            dist = diff.magnitude;
                        }
                    }
                }
            }
        } 
    }

    void Shoot() {
        Vector2 diff = target.transform.position - transform.position;
        Quaternion dir = Quaternion.identity;
        GameObject obj = Instantiate(ShotPrefab, transform.position + new Vector3(0, 0, -1), dir);
        float shotSpeed = 300;
        obj.GetComponent<Rigidbody2D>().AddForce(diff.normalized * shotSpeed);
        obj.GetComponent<ShotLogic>().damage = damage * pm.Bonuses().damage;
    }

    void Update() {
        if (shotTimer < Time.time) {
            shotTimer = Time.time + shotCooldown * pm.Bonuses().firerate;
            Scan();
            if (target) {
                Shoot();
            } 
        }
    }
}
