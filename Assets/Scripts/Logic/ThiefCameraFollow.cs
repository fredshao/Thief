using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefCameraFollow : MonoBehaviour {

    public Transform target;

    public Vector3 stayPoint;
    public float damping = 0.05f;

    private void FixedUpdate() {
        Vector3 nextPos = target.position + stayPoint;
        transform.position = Vector3.Lerp(transform.position, nextPos, damping);
    }


}
