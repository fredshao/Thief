using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : MonoBehaviour {

    public float minAngle = 0.0f;
    public float maxAngle = 0.0f;
    public float rotateSpeed = 10.0f;

    private int dir = 1;

    private void Update() {
        transform.eulerAngles += new Vector3(0, dir * rotateSpeed * Time.deltaTime, 0);

        if (dir > 0) {
            if (transform.eulerAngles.y >= maxAngle) {
                dir = -1;
            }
        } else {
            if(transform.eulerAngles.y <= minAngle) {
                dir = 1;
            }
        }
    }
}
