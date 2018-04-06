using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;

public enum En_GameState {
    UpdateRes,
    LoadRes,
    Error,
}

public class Game : MonoBehaviour {

    public const string BundleUrl = "http://192.168.3.24/Thief/Bundles/";
    public const string BundleSizeInfo = "BundleSize.info";
    public static En_GameState gameState = En_GameState.UpdateRes;
    public static string gameStateErrorInfo = "";


    public static MapManager mapMgr;



    private void Awake() {
        BadBox.Init();
        DontDestroyOnLoad(gameObject);
    }

    public void Start() {
        var startGame = StartGame();

        // 初始化各种游戏管理器模块
        mapMgr = gameObject.AddComponent<MapManager>();
    }


    /// <summary>
    /// 游戏启动流程
    /// </summary>
    private async Task StartGame() {
        gameState = En_GameState.UpdateRes;
        await BadBox.mBundle.LoadBundlesAsync(Game.BundleUrl, Game.BundleSizeInfo, null);
        await 0.2f;
        gameState = En_GameState.LoadRes;

        // 加载地图数据
        TextAsset ta = BadBox.mBundle.GetAsset<TextAsset>(PathConst.MapPath(GameConst.mapConfigFile));
        string[] mapNameList = JsonConvert.DeserializeObject<string[]>(ta.text);
        if(mapNameList == null || mapNameList.Length == 0) {
            Ulog.LogError("地图数据出错！");
            gameState = En_GameState.Error;
            gameStateErrorInfo = "地图数据加载出错！";
            return;
        }

        // 依次加载每一个地图数据
        foreach(string mapName in mapNameList) {
            string mapPath = PathConst.MapPath(mapName);
            TextAsset mta = BadBox.mBundle.GetAsset<TextAsset>(mapPath);
            MapDataConfig mdc = new MapDataConfig(mapName, mta.text);
            MapConfig.AddMap(mdc);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        UIManager.Inst.OpenUI(GameConst.UILevel);
    }
    
}
