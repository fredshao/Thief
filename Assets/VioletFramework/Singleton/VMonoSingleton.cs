using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class VMonoSingleton<T> : MonoBehaviour, IVSingleton where T : VMonoSingleton<T> {

    private static T instance = null;

    public static T Instance {
        get { 
            if(instance == null) {
                GameObject obj = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<T>();
                instance.OnInit();
            }
            return instance;
        }
    }

    /// <summary>
    /// call by manually
    /// </summary>
    public virtual void Initialize() {

    }

    public virtual void Dispose() {

    }

    /// <summary>
    /// call by auto on singleton created
    /// </summary>
    public virtual void OnInit() {

    }
}
