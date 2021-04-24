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
        mousePosLast = Input.mousePosition;
    }
}
