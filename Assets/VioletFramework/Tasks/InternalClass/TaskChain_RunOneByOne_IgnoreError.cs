using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public class TaskChain_RunOneByOne_IgnoreError : TaskChain_Queue {
        private int currIndex = 0;

        private bool hasSuccess = false;

        ~TaskChain_RunOneByOne_IgnoreError() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
        }


        public override void StartTask() {
            base.StartTask();

            currIndex = 0;
            hasSuccess = false;

            if (this.taskList.Count > 0) {
                IVTask task = taskList[currIndex];
                task.AddOnEnd(OnTaskEnd);
                task.StartTask();
            } else {
                this._isSuccess = false;
                this.FireOnEnd(false);
            }
        }


        public override void StopTask() {
            base.StopTask();
            foreach (IVTask task in this.taskList) {
                task.RemoveOnEnd(OnTaskEnd);
                task.StopTask();
            }
        }


        private void OnTaskEnd(IVTask _task) {
            ++this.currIndex;

            _task.RemoveOnEnd(OnTaskEnd);

            if (_task.isSuccess) {
                hasSuccess = true;
            }

            if (this.currIndex >= this.taskList.Count) {
                if (hasSuccess) {
                    this._isSuccess = true;
                    this.FireOnEnd(true);
                } else {
                    this._isSuccess = false;
                    this.FireOnEnd(false);
                }
            }
        }
    }
}
