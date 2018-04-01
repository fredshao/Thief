using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class MapManager : MonoBehaviour {

    public MapGenerator mapGenerator;
    public string mapName = "";

    private string mapPath = "";

    private Dictionary<int, GridNode> map = null;

	// Use this for initialization
	void Start () {
        mapPath = "file://" + Application.streamingAssetsPath + "/" + mapName;
        StartCoroutine(LoadMap());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator LoadMap() {
        WWW www = new WWW(mapPath);
        yield return www;
        Debug.Log(www.text);
        map = ParseMap(www.text);
        mapGenerator.GenerateMap(map);
    }


    private Dictionary<int,GridNode> ParseMap(string _json) {
        Debug.Log("Json ################: " + _json);
        Dictionary<int, GridNode> gridNodes = JsonConvert.DeserializeObject<Dictionary<int, GridNode>>(_json);

        // 处理一下, 找到整个地图的左下起点，然后计算出偏移，将所有的点往0，0点偏移

        int minX = -1;
        int minZ = -1;

        var enumer = gridNodes.GetEnumerator();
        while(enumer.MoveNext()){
            GridNode node = enumer.Current.Value;
            if(minX < 0 || node.x < minX) {
                minX = node.x;
            }

            if(minZ < 0 || node.z < minZ) {
                minZ = node.z;
            }
        }

        enumer = gridNodes.GetEnumerator();


        Debug.Log(minX + "  " + minZ);

        while (enumer.MoveNext()) {
            GridNode node = enumer.Current.Value;
            node.x -= minX;
            node.z -= minZ;
        }

        return gridNodes;
    }

}
