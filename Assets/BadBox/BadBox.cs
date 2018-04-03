using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBox : MonoBehaviour {

    public static BundleSystem mBundle = null;

    public static void Init() {
        GameObject obj = new GameObject("BadBox");
        obj.AddComponent<BadBox>();
        DontDestroyOnLoad(obj);
        InitSubModules();
    }

    private static void InitSubModules() {
        mBundle = new BundleSystem();
    }



	
}
