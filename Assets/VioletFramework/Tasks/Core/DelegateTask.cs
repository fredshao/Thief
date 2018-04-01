using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public class DelegateTask : VTask {
        public delegate void Func(DelegateTask _task);

        public Func func;
        public object[] args;

        public DelegateTask(Func _func, object[] _args = null) {
            this.func = _func;
            this.args = _args;
        }

        ~DelegateTask() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
            this.func = null;
            this.args = null;
        }

        public override void StartTask() {
            base.StartTask();
            if (this.func != null) {
                this.func(this);
            } else {
                this.FireOnEnd(true);
            }
        }

        /// <summary>
        /// 手动调用Task结束
        /// </summary>
        /// <param name="_isSuccess"></param>
        public void OnCallback(bool _isSuccess) {
            this.FireOnEnd(_isSuccess);
        }


        void Test() {
            

            TaskChain taskChain = new TaskChain(TaskChainType.RunOneByOne);
            taskChain.AddTask(new DelegateTask(delegate (DelegateTask _task) {
                //_task.OnCallback(true);
                //_task.OnCallback(false);
            }));
            
        }
    }
}
