using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public GameObject cube;

    public void GenerateMap(Dictionary<int, GridNode> _map) {
        GameObject obj = new GameObject("Map");
        obj.transform.position = Vector3.zero;
        var enumer = _map.GetEnumerator();
        while (enumer.MoveNext()) {
            GridNode node = enumer.Current.Value;
            GameObject clone = Instantiate(cube);
            clone.transform.SetParent(obj.transform);
            clone.transform.position = new Vector3(node.x, 0, node.z);
        }
    }
}
