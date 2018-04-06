using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathConst {
    public static string UIPath(string _name) {
        return "Assets/Prefab/UI/" + _name;
    }

    public static string MapPath(string _name) {
        return "Assets/Prefab/Data/MapData/" + _name;
    }

    public static string MapItemPath(string _name) {
        return "Assets/Prefab/Map/" + _name;
    }
}
