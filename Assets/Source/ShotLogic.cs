using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotLogic : MonoBehaviour {

    public GameObject FxBoom;
    public float damage;

    void Start() {
        Destroy(gameObject, 2f);
    }
    public void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            EnemyLogic el = collision.gameObject.GetComponent<EnemyLogic>();
            el.hp -= damage;
        }
        Destroy(gameObject);
        
        GameObject fx = Instantiate(FxBoom);
        fx.transform.position = transform.position;
        Destroy(fx, 0.5f);
    }
}
