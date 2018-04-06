using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;


public class UpdateUI : AbstractUI {

    public RectTransform R_Img_Fill;
    public Text R_Text_Info;

    private float width = 0.0f;


    private void Start() {
        width = R_Img_Fill.sizeDelta.x;
        R_Img_Fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        OpenUI();
    }

    public override void OpenUI() {
        
    }

    public override void CloseUI() {

    }

    public void UpdateProgress(float _percentage, string _textInfo) {
        float currWidth = width * _percentage;
        R_Img_Fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currWidth);
        R_Text_Info.text = _textInfo;
    }


    private void Update() {
        if (Game.gameState == En_GameState.UpdateRes) {
            switch (BadBox.mBundle.bundleSystemState) {
                case EN_BundleSystemState.AllBundleCachedAndLoaded: {
                        string info = "本地资源加载完成!";
                        UpdateProgress(1.0f, info);
                    }
                    break;
                case EN_BundleSystemState.OnLoadingManifest:
                case EN_BundleSystemState.None: {
                        string info = string.Format("正在检查更新...");
                        UpdateProgress(1.0f, info);
                    }
                    break;
                case EN_BundleSystemState.OnLoadingRemoteBundle: {
                        double downloadedM = BadBox.mBundle.downloadedBytes / 1024.0f / 1024.0f;
                        double totalNeedDownload = BadBox.mBundle.totalNeedDownloadBytes / 1024.0f / 1024.0f;
                        downloadedM = System.Math.Round(downloadedM, 2);
                        totalNeedDownload = System.Math.Round(totalNeedDownload);
                        string info = string.Format("正在更新资源 {0}M / {1}M", downloadedM, totalNeedDownload);
                        UpdateProgress(BadBox.mBundle.progress / 100.0f, info);
                    }
                    break;
                case EN_BundleSystemState.AllRemoteBundleLoadSuccess: {
                        string info = "更新完成!";
                        UpdateProgress(1.0f, info);
                    }
                    break;
                case EN_BundleSystemState.Error_LoadManifest:
                case EN_BundleSystemState.Error_LoadRemoteBundle: {
                        string info = "更新失败!";
                        UpdateProgress(BadBox.mBundle.progress / 100.0f, info);
                    }
                    break;
                default: {
                        string info = "未知错误!";
                        UpdateProgress(BadBox.mBundle.progress / 100.0f, info);
                    }
                    break;
            }
        } else if(Game.gameState == En_GameState.LoadRes){
            R_Text_Info.text = "正在加载本地数据...";
        } else if(Game.gameState == En_GameState.Error) {
            R_Text_Info.text = Game.gameStateErrorInfo;
        }
    }

    private void OnGUI() {
        GUILayout.Label(BadBox.mBundle.bundleSystemState.ToString());
    }

}
