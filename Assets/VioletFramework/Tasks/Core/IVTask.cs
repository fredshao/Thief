using System;
using System.Collections.Generic;
using UnityEngine;

namespace Violet.Tasks {
    public interface IVTask : IDisposable {

        bool isSuccess {
            get;
        }

        void StartTask();
        void StopTask();
        void AddOnEnd(Listener<IVTask> _onEnd);
        void RemoveOnEnd(Listener<IVTask> _onEnd);
        void ClearOnEnd();
        void OnUpdate();
        void RetryTask();

    }
}
