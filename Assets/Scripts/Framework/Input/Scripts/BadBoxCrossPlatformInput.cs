using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBoxCrossPlatformInput {

    private static Vector3 joystickDir;
    private static float speedPercentage;

    public static void UpdateDirection(Vector3 _moveDirection) {
        joystickDir = _moveDirection;
    }

    public static void UpdateMovedPercentage(float _percentage) {
        speedPercentage = _percentage;
    }

    public static float GetXAxis() {
        return joystickDir.x;
    }

    public static float GetYAxis() {
        return joystickDir.y;
    }

    public static Vector3 GetAxis() {
        return joystickDir;
    }

    public static float GetSpeedPercentage() {
        return speedPercentage;
    }



	
}
