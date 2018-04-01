using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public enum ENModuleState {
    Stoped,         // ��;ǿ����ֹ
    Running,        // ��������
    Finished,       // �������
    Error,          // ���г���
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
