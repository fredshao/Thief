using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {

    public float moveSpeed;
    public Transform playerMesh;
    public float maxRotSpeed = 20.0f;
    public float minRotSpeed = 5.0f;
    private float currRotSpeed = 20.0f;

    private Vector3 move;
    private float x;
    private float y;
    private float currMoveSpeed = 0;
	
	void Update () {

        x = BadBoxCrossPlatformInput.GetXAxis();
        y = BadBoxCrossPlatformInput.GetYAxis();

        Vector3 moveVector = (transform.right * x + transform.forward * y).normalized;

        float speedPercentage = BadBoxCrossPlatformInput.GetSpeedPercentage();
        currMoveSpeed = moveSpeed * speedPercentage;

        move = moveVector * currMoveSpeed * Time.deltaTime;
        transform.Translate(move);

        currRotSpeed = maxRotSpeed * speedPercentage;
        currRotSpeed = Mathf.Clamp(currRotSpeed, minRotSpeed, maxRotSpeed);
        playerMesh.Rotate(new Vector3(0, currRotSpeed, 0));
    }

    
}
