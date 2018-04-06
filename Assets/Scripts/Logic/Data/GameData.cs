using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#region 地图相关
/// <summary>
/// 一张地图的配置数据
/// </summary>
public class MapDataConfig {
    public string mapName;
    public Dictionary<int, GridNode> gridNodes = null;

    public MapDataConfig(string _mapName, string _mapJsonData) {
        mapName = _mapName;
        gridNodes = JsonConvert.DeserializeObject<Dictionary<int, GridNode>>(_mapJsonData);
        // 处理一下, 找到整个地图的左下起点，然后计算出偏移，将所有的点往0，0点偏移

        int minX = -1;
        int minZ = -1;

        var enumer = gridNodes.GetEnumerator();
        while (enumer.MoveNext()) {
            GridNode node = enumer.Current.Value;
            if (minX < 0 || node.x < minX) {
                minX = node.x;
            }

            if (minZ < 0 || node.z < minZ) {
                minZ = node.z;
            }
        }

        enumer = gridNodes.GetEnumerator();

        while (enumer.MoveNext()) {
            GridNode node = enumer.Current.Value;
            node.x -= minX;
            node.z -= minZ;
        }
    }

}

public class MapConfig {
    public static List<MapDataConfig> mapList = new List<MapDataConfig>();
    
    public static void AddMap(MapDataConfig _map) {
        mapList.Add(_map);
    }

    public static MapDataConfig GetMapData(string _mapName) {
        for(int i = 0; i <mapList.Count; ++i) {
            if(mapList[i].mapName == _mapName) {
                return mapList[i];
            }
        }
        return null;
    }
}

#endregion
