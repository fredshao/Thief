using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelItemUI : MonoBehaviour {
    public Text R_Text_MapName;

    private string mapName;
    private Action<string> onSelectMap;

    private void Start() {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            onSelectMap?.Invoke(mapName);
        });
    }

    public void Init(string _mapName, Action<string> _onSelectCallback) {
        mapName = _mapName;
        R_Text_MapName.text = _mapName;
        onSelectMap = _onSelectCallback;
    }

    


	
}
