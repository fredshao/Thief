using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Inst = null;

    private Transform root = null;

    private void Awake() {
        Inst = this;
        root = transform.Find("Adaptive");
        DontDestroyOnLoad(gameObject);
    }

    


    private Dictionary<string, AbstractUI> uiDict = new Dictionary<string, AbstractUI>();

    public void OpenUI(string _uiName) {
        if (uiDict.ContainsKey(_uiName)) {
            return;
        }
        string uiPath = PathConst.UIPath(_uiName);
        string uiNameNoExt = System.IO.Path.GetFileNameWithoutExtension(_uiName);
        GameObject ui = BadBox.mBundle.GetAsset<GameObject>(uiPath);
        if(ui != null) {
            ui.SetActive(true);
            AbstractUI abstractUI = ui.GetComponent<AbstractUI>();
            ui.name = uiNameNoExt;
            ui.transform.SetParent(root);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;
            ui.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            ui.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            uiDict.Add(_uiName, abstractUI);
            abstractUI.OpenUI();
        } else {
            Ulog.LogError("打开UI失败:" + uiNameNoExt);
        }
    }

    public T OpenUI<T>(string _uiName) where T : AbstractUI {
        if (uiDict.ContainsKey(_uiName)) {
            return null;
        }
        string uiPath = PathConst.UIPath(_uiName);
        string uiNameNoExt = System.IO.Path.GetFileNameWithoutExtension(_uiName);
        GameObject ui = BadBox.mBundle.GetAsset<GameObject>(uiPath);
        if (ui != null) {
            ui.SetActive(true);
            AbstractUI abstractUI = ui.GetComponent<AbstractUI>();
            ui.name = uiNameNoExt;
            ui.transform.SetParent(root);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;
            ui.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            ui.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            uiDict.Add(_uiName, abstractUI);
            abstractUI.OpenUI();
            return (T)abstractUI;
        } else {
            Ulog.LogError("打开UI失败:" + uiNameNoExt);
        }
        return null;
    }


    public void OpenStaticUI(string _uiName) {
        if (uiDict.ContainsKey(_uiName)) {
            return;
        }
        string uiNameNoExt = System.IO.Path.GetFileNameWithoutExtension(_uiName);
        string uiPath = "UI/" + uiNameNoExt;
        GameObject ui = Instantiate(Resources.Load(uiPath)) as GameObject;
        if (ui != null) {
            AbstractUI abstractUI = ui.GetComponent<AbstractUI>();
            ui.name = uiNameNoExt;
            ui.transform.SetParent(root);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;
            ui.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            ui.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            uiDict.Add(_uiName, abstractUI);
            abstractUI.OpenUI();
        } else {
            Ulog.LogError("打开UI失败:" + uiNameNoExt);
        }
    }

    public void CloseUI(string _uiName) {
        uiDict.TryGetValue(_uiName, out AbstractUI ui);
        if(ui != null) {
            uiDict.Remove(_uiName);
            ui.CloseUI();
            Destroy(ui.gameObject);
        }
    }

    public void CloseAll() {
        var enumer = uiDict.GetEnumerator();
        while (enumer.MoveNext()) {
            enumer.Current.Value.CloseUI();
            Destroy(enumer.Current.Value.gameObject);
        }

        uiDict.Clear();
    }

}
