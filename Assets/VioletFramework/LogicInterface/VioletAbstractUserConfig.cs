using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽象用户配置
/// </summary>
public abstract class VioletAbstractUserConfig {
    public abstract string IP { get; }
    public abstract int PORT { get; }
    public abstract string ConfigTableFilePath { get; }
    public abstract string AssetJsonPath { get; }
    public abstract string LocalResPath { get; }
    public abstract string URL_BUNDLE { get; }
    public abstract string URL_LOCAL_BUNDLE { get; }
    public abstract bool UseBundleOnEditor { get; }
}
