using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

    public float speed = 4f;
    Vector3 mousePosLast;

    void Start() {
        mousePosLast = Input.mousePosition;
    }

    void LateUpdate() {
        Vector3 delta = mousePosLast - Input.mousePosition;
        if (Input.GetMouseButton(1)) {
            transform.position += delta * speed;
        }
        if(transform.position.y > 0) {
            Vector3 pos = transform.position;
            pos.y = 0;
            transform.position = pos;
        }
        if (transform.position.y < -30) {
            Vector3 pos = transform.position;
            pos.y = -30;
            transform.position = pos;
        }
        if (transform.position.x > 10) {
            Vector3 pos = transform.position;
            pos.x = 10;
            transform.position = pos;
        }
        if (transform.position.x < -10) {
            Vector3 pos = transform.position;
            pos.x = -10;
            transform.position = pos;
        }
        mousePosLast = Input.mousePosition;
    }
}
