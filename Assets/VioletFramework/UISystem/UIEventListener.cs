using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventListener : UnityEngine.EventSystems.EventTrigger {

    public delegate void VoidDelegate(GameObject _go);
    public delegate void PointerDelegate(PointerEventData _eventData);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public PointerDelegate onDrag;

    public static UIEventListener Get(GameObject _go) {
        UIEventListener listener = _go.GetComponent<UIEventListener>();
        if (listener == null) {
            listener = _go.AddComponent<UIEventListener>();
        }
        return listener;
    }
    public static UIEventListener Get(Transform _tran) {
        return Get(_tran.gameObject);
    }
    public override void OnPointerClick(PointerEventData _eventData) {
        if (onClick != null) onClick(gameObject);
    }
    public override void OnPointerDown(PointerEventData _eventData) {
        if (onDown != null) onDown(gameObject);
    }
    public override void OnPointerEnter(PointerEventData _eventData) {
        if (onEnter != null) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData _eventData) {
        if (onExit != null) onExit(gameObject);
    }
    public override void OnPointerUp(PointerEventData _eventData) {
        if (onUp != null) onUp(gameObject);
    }
    public override void OnSelect(BaseEventData _eventData) {
        if (onSelect != null) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData _eventData) {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }
    public override void OnDrag(PointerEventData _eventData) {
        if (onDrag != null) onDrag(_eventData);
    }
}
