using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BadBoxJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    public enum AxisOption {
        Both,
        OnlyHorizontal,
        OnlyVertical
    }

    public int MovementRadius = 100;
    public AxisOption axesToUse = AxisOption.Both;

    private Vector3 m_StartPos;
    private bool m_UseX;
    private bool m_UseY;

    private bool useKeyboard = true;

    private void OnEnable() {
        CreateVirtualAxes();
    }

    void Start() {
        m_StartPos = transform.position;
    }

    private void CreateVirtualAxes() {
        m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
        m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);
    }

    public void OnPointerUp(PointerEventData data) {
        useKeyboard = true;
        transform.position = m_StartPos;
        BadBoxCrossPlatformInput.UpdateDirection(Vector3.zero);
        BadBoxCrossPlatformInput.UpdateMovedPercentage(0);
    }

    public void OnPointerDown(PointerEventData data) {
        useKeyboard = false;
    }

    public void OnDrag(PointerEventData data) {
        Vector3 currPos = new Vector3(data.position.x, data.position.y, 0);

        if (!m_UseX) {
            currPos.x = m_StartPos.x;
        }

        if (!m_UseY) {
            currPos.y = m_StartPos.y;
        }

        Vector3 moveDir = (currPos - m_StartPos).normalized;
        float movedMagnitude = (currPos - m_StartPos).magnitude;
        movedMagnitude = Mathf.Clamp(movedMagnitude, 0, MovementRadius);
        transform.position = m_StartPos + moveDir * movedMagnitude;

        BadBoxCrossPlatformInput.UpdateDirection(moveDir);
        BadBoxCrossPlatformInput.UpdateMovedPercentage(movedMagnitude / MovementRadius);
    }

    void Update() {

        if (!useKeyboard) {
            return;
        }

        float x = 0;
        float y = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            x = -1;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            x = 1;
        } else {
            x = 0;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            y = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            y = -1;
        } else {
            y = 0;
        }

        float speedPercentage = (x != 0 || y != 0) ? 1 : 0;

        BadBoxCrossPlatformInput.UpdateDirection(new Vector3(x, y, 0));
        BadBoxCrossPlatformInput.UpdateMovedPercentage(speedPercentage);
    }
    
}
