using System;
using System.Collections.Generic;
using UnityEngine;

public enum BundleLoadState {
    Success,
    Faild,
}

public class BundleLoadResult {
    public BundleLoadState state;
    public AssetBundle bundle = null;
    public List<AssetBundle> bundleList = null;
}
