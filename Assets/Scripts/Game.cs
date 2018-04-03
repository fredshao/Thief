using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Game : MonoBehaviour {

    public const string BundleUrl = "http://192.168.3.24/Thief/Bundles/";
    public const string BundleSizeInfo = "BundleSize.info";

    private void Awake() {
        BadBox.Init();
    }

    public void Start() {
        var startGame = StartGame();
    }


    /// <summary>
    /// 游戏启动流程
    /// </summary>
    private async Task StartGame() {
        await BadBox.mBundle.LoadBundlesAsync(Game.BundleUrl, Game.BundleSizeInfo, null);
        await 2.0f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
    
}
