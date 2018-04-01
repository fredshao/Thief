using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public class TaskChain_RunParallel : TaskChain_Queue {

        private int taskCount = 0;
        private bool hasError = false;

        ~TaskChain_RunParallel() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
        }

        public override void StartTask() {
            taskCount = 0;
            hasError = false;

            foreach (IVTask task in this.taskList) {
                task.AddOnEnd(OnTaskEnd);
                task.StartTask();
            }

        }

        public override void StopTask() {
            foreach (IVTask task in this.taskList) {
                task.RemoveOnEnd(OnTaskEnd);
                task.StopTask();
            }
        }

        private void OnTaskEnd(IVTask _task) {
            ++taskCount;
            if (_task.isSuccess == false) {
                hasError = true;
            }

            if (taskCount == this.taskList.Count) {
                if (hasError) {
                    this._isSuccess = false;
                    this.FireOnEnd(false);
                } else {
                    this._isSuccess = true;
                    this.FireOnEnd(true);
                }
            }
        }
    }
}
