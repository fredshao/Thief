using System;
using System.Collections.Generic;
using UnityEngine;

namespace Violet.Tasks {
    public class VTask : IVTask {
        public event Listener<IVTask> onEnd;

        protected bool _running = false;
        protected bool _isSuccess = false;

        public VTask() {
            V.vTicker.AddUpdateCallback(this.OnUpdate);
        }

        ~VTask() {
            this.Dispose();
        }

        public virtual void Dispose() {
            V.vTicker.RemoveUpdateCallback(this.OnUpdate);
            if (this.onEnd != null) {
                System.Delegate.RemoveAll(this.onEnd, this.onEnd);
            }
            this.onEnd = null;
        }

        public virtual void ClearOnEnd() {
            if (this.onEnd != null) {
                System.Delegate.RemoveAll(this.onEnd, this.onEnd);
            }
            this.onEnd = null;
        }

        public virtual void AddOnEnd(Listener<IVTask> _onEnd) {
            this.onEnd += _onEnd;
        }

        public virtual void RemoveOnEnd(Listener<IVTask> _onEnd) {
            this.onEnd -= _onEnd;
        }

        protected virtual void FireOnEnd(bool _isSuccess) {
            this._running = false;
            this._isSuccess = _isSuccess;
            if (this.onEnd != null) {
                this.onEnd(this);
            }

            ClearOnEnd();
        }

        public virtual void StartTask() {
            this._running = true;
        }

        public virtual void StopTask() {
            this._running = false;
        }


        public bool isSuccess {
            get { return this._isSuccess; }
        }

        public bool running {
            get { return this._running; }
        }

        public virtual void OnUpdate() {

        }

        public virtual void RetryTask() {
            this._running = true;
        }
    }
}
