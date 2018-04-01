using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public enum ENModuleState {
    Stoped,         // 中途强行终止
    Running,        // 正在运行
    Finished,       // 运行完成
    Error,          // 运行出错
}

public class BaseModule
{
    public ENModuleState moduleState = ENModuleState.Stoped;
    public string ERROR_MSG = string.Empty;

    public virtual void Initialize(){
        
    }

    public virtual void OnUpdate(){
        
    }

    public virtual void OnFixedUpdate(){
        
    }

    public virtual void OnLateUpdate() {

    }
}
