using System;
using System.Collections.Generic;

public class VSingleton<T> : IVSingleton where T : VSingleton<T> {

    private static T instance = null;
    public static T Instance {
        get {
            if(instance == null) {
                instance = default(T);
                instance.OnInit();
            }
            return instance;
        }
    }

    public virtual void Initialize() {

    }

    public virtual void Dispose() {

    }

    public virtual void OnInit() {

    }
}
