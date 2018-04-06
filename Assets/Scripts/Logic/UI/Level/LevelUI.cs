using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : AbstractUI {

    public RectTransform R_RTrans_Container;

    public override void OpenUI() {
        for(int i = 0; i < MapConfig.mapList.Count; ++i) {
            string mapName = MapConfig.mapList[i].mapName;
            GameObject itemObj = BadBox.mBundle.GetAsset<GameObject>(PathConst.UIPath(GameConst.UILevelItem));
            LevelItemUI code = itemObj.GetComponent<LevelItemUI>();
            itemObj.transform.SetParent(R_RTrans_Container);
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;
            code.Init(mapName, OnSelectMap);
        }
    }

    public override void CloseUI() {

    }

    private void OnSelectMap(string _mapName) {
        if (Game.mapMgr.EnterMap(_mapName)) {
            UIManager.Inst.CloseUI(GameConst.UILevel);
        }
    }
}
