using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour {

    protected Transform trans;

    private void Awake() {
        trans = transform;
        ReferenceComponent();
    }


    public virtual void ReferenceComponent() {

    }

    public virtual void Open(params object[] _data) {

    }

    public virtual void Close(params object[] _data) {
        
    }

	
}
