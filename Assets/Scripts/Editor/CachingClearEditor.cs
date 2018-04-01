using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CachingClearEditor : Editor {
    //[MenuItem("BunnyTools/清除缓存的Bundle")]
    public static void ClearCachingBundles() {
        Caching.ClearCache();
        Ulog.Log("缓存Bundle已清除");
    }
}
