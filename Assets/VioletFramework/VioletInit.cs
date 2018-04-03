using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Threading.Tasks;


/// <summary>
/// 用户继承框架指定的类，并Override指定方法，实现框架启动完成后，会调用到用户逻辑启动
/// </summary>
public class VioletInit {
    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod() {
        OnBefore();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoadRuntimeMethod() {
        OnAfter();
    }

    private static void OnBefore() {
        BeforeAwake();
    }

    private static void OnAfter() {
        AfterStart();
    }

    private static void BeforeAwake() {
        //if (!InitVioletConfig()) {
        //    return;
        //}
        QualitySettings.SetQualityLevel(5);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        V.Instance.Initialize();
    }

    private static void AfterStart() {
        //VioletAbstractGameIniter gameIniter = GetUserGameIniter();
        //if (gameIniter != null) {
        //    gameIniter.InitGame();
        //}
    }


   
    /// <summary>
    /// 初始化框架Const
    /// </summary>
    /// <returns></returns>
    private static bool InitVioletConfig() {
        Type configType = typeof(VioletAbstractUserConfig);
        Assembly assem = Assembly.GetAssembly(configType);
        Type userConfigType = null;
        foreach (Type child in assem.GetTypes()) {
            if (child.BaseType == configType) {
                if (userConfigType == null) {
                    userConfigType = child;
                } else {
                    Ulog.LogError("ERROR:", "用户配置冲突，只能有一份实现AbstractUserConfig接口的用户配置! - ", child.Name);
                }
            }
        }

        if (userConfigType != null) {
            VioletConfig.userConfig = (VioletAbstractUserConfig)Activator.CreateInstance(userConfigType);
            return true;
        } else {
            Ulog.LogError("ERROR:", "没用找到用户配置文件，请实现AbstractUserConfig接口，并在属性 get 中返回值");
            return false;
        }
    }

    /// <summary>
    /// 获取用户游戏Initer
    /// </summary>
    /// <returns></returns>
    private static VioletAbstractGameIniter GetUserGameIniter() {
        Type initerType = typeof(VioletAbstractGameIniter);
        Assembly assem = Assembly.GetAssembly(initerType);
        Type userIniterType = null;
        foreach (Type child in assem.GetTypes()) {
            if (child.BaseType == initerType) {
                if (userIniterType == null) {
                    userIniterType = child;
                } else {
                    Ulog.LogError("ERROR:", "GameIniter 冲突，发现多个继承 VioletGameIniter 的类");
                }
            }
        }

        if (userIniterType != null) {
            VioletAbstractGameIniter gameIniter = (VioletAbstractGameIniter)Activator.CreateInstance(userIniterType);
            return gameIniter;
        } else {
            Ulog.LogError("ERROR:", "没有找到用户 GameIniter，无法调用游戏逻辑初始化");
            return null;
        }
    }
    */
}
