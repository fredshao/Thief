using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISystem : BaseModule {

    private Dictionary<int, BaseUI> uiDict = new Dictionary<int, BaseUI>();

    private Canvas canvas;
    private RectTransform topLayer;
    private RectTransform middleLyaer;
    private RectTransform bottomLayer;

    public override void Initialize() {
        base.Initialize();

        //GameObject uiRoot = new GameObject("UIRoot");
        //uiRoot.transform.position = Vector3.zero;

        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        //canvasObj.transform.SetParent(uiRoot.transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventObj = new GameObject("EventSystem");
        eventObj.AddComponent<StandaloneInputModule>();
        //eventObj.transform.SetParent(uiRoot.transform);

        // Layers
        GameObject obj = new GameObject("BottomLayer");
        obj.transform.SetParent(canvasObj.transform);
        RectTransform rectTrans = obj.AddComponent<RectTransform>();
        bottomLayer = rectTrans;
        rectTrans.anchorMin = new Vector2(0, 0);
        rectTrans.anchorMax = new Vector2(1, 1);
        rectTrans.anchoredPosition = Vector3.zero;
        rectTrans.offsetMin = Vector2.zero;
        rectTrans.offsetMax = Vector3.zero;

        obj = new GameObject("MiddleLayer");
        obj.transform.SetParent(canvasObj.transform);
        rectTrans = obj.AddComponent<RectTransform>();
        middleLyaer = rectTrans;
        rectTrans.anchorMin = new Vector2(0, 0);
        rectTrans.anchorMax = new Vector2(1, 1);
        rectTrans.anchoredPosition = Vector3.zero;
        rectTrans.offsetMin = Vector2.zero;
        rectTrans.offsetMax = Vector3.zero;

        obj = new GameObject("TopLayer");
        obj.transform.SetParent(canvasObj.transform);
        rectTrans = obj.AddComponent<RectTransform>();
        topLayer = rectTrans;
        rectTrans.anchorMin = new Vector2(0, 0);
        rectTrans.anchorMax = new Vector2(1, 1);
        rectTrans.anchoredPosition = Vector3.zero;
        rectTrans.offsetMin = Vector2.zero;
        rectTrans.offsetMax = Vector3.zero;

        //GameObject.DontDestroyOnLoad(uiRoot);
        GameObject.DontDestroyOnLoad(canvasObj);
        GameObject.DontDestroyOnLoad(eventObj);
        
    }

    public void OpenUI<T>(int _uiId,params object[] _data) where T : BaseUI {
        if (uiDict.ContainsKey(_uiId)) {
            uiDict[_uiId].Open(_data);
            return;
        }

        GameObject uiObj = V.vResource.GetGameObject(_uiId);
        BaseUI baseUI = uiObj.AddComponent<T>();
        uiObj.transform.SetParent(middleLyaer);
        RectTransform uiRectTrans = uiObj.transform as RectTransform;
        uiRectTrans.anchoredPosition3D = Vector2.zero;
        uiRectTrans.localScale = Vector3.one;
        uiRectTrans.offsetMin = Vector2.zero;
        uiRectTrans.offsetMax = Vector2.zero;

        uiDict.Add(_uiId, baseUI);
        baseUI.Open(_data);
    }

    public void CloseUI(int _uiId, params object[] _data) {
        if (uiDict.ContainsKey(_uiId)) {
            BaseUI baseUI = uiDict[_uiId];
            uiDict.Remove(_uiId);
            baseUI.Close(_data);
            V.vResource.ForceReleaseGameObject(baseUI.gameObject);
        }
    }

    public void CloseAll() {

    }

}
