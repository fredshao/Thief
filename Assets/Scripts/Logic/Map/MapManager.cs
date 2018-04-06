using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MapManager : MonoBehaviour {

    private MapDataConfig mapData = null;

    public bool EnterMap(string _mapName) {
        mapData = MapConfig.GetMapData(_mapName);
        if (mapData == null) {
            Ulog.LogError("获取地图数据失败：", _mapName);
            return false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Game");
        return true;
    }


    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GenerateMap(mapData.gridNodes);
    }


  

    


    public void GenerateMap(Dictionary<int, GridNode> _map) {
        GameObject wallObj = BadBox.mBundle.GetAsset(PathConst.MapItemPath(GameConst.MapItem_Wall)) as GameObject;

        GameObject obj = new GameObject("Map");
        obj.transform.position = Vector3.zero;
        var enumer = _map.GetEnumerator();
        while (enumer.MoveNext()) {
            GridNode node = enumer.Current.Value;
            GameObject clone = Instantiate(wallObj);
            clone.transform.SetParent(obj.transform);
            clone.transform.position = new Vector3(node.x, 0, node.z);
        }
    }
}
