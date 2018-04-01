using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// 框架与用户逻辑的连接器
/// </summary>
public abstract class VioletAbstractGameIniter {
    /// <summary>
    /// 逻辑层 InitGame 要做的事情(!!!! 非强制流程 !!!!)
    /// 0. 进入专用加载场景
    /// 1. 解压本地 AssetBundle
    /// 2. 更新远程 AssetBundle (UI上在Update中判断 BundleSystem 的状态，以显示UI)
    /// 3. 加载资源 Json 文件
    /// 4. 加载数据配置表
    /// 
    /// 这几步，可以做成一个任务连，任何一个任务失败，则可以从失败的地方继续重试
    /// </summary>
    public abstract void InitGame();
}
