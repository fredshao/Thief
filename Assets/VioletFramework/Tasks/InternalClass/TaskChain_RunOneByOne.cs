using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public class TaskChain_RunOneByOne : TaskChain_Queue {
        private int currIndex = 0;

        ~TaskChain_RunOneByOne() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
        }


        public override void StartTask() {
            base.StartTask();

            currIndex = 0;

            if (this.taskList.Count > 0) {
                IVTask task = taskList[currIndex];
                task.AddOnEnd(OnTaskEnd);
                task.StartTask();
            } else {
                this.FireOnEnd(true);
            }
        }


        public override void StopTask() {
            base.StopTask();
            foreach (IVTask task in this.taskList) {
                task.RemoveOnEnd(OnTaskEnd);
                task.StopTask();
            }
        }

        /// <summary>
        /// 从失败的地方重新继续任务链
        /// </summary>
        public override void RetryTask() {
            if(this.currIndex < this.taskList.Count) {
                IVTask task = taskList[currIndex];
                task.AddOnEnd(OnTaskEnd);
                task.StartTask();
            } else {
                this.FireOnEnd(true);
            }
        }


        private void OnTaskEnd(IVTask _task) {
            _task.RemoveOnEnd(OnTaskEnd);
            if (_task.isSuccess) {
                ++this.currIndex;
                if (this.currIndex < this.taskList.Count) {
                    IVTask task = taskList[currIndex];
                    task.AddOnEnd(OnTaskEnd);
                    task.StartTask();
                } else {
                    this._isSuccess = true;
                    this.FireOnEnd(true);
                }
            } else {
                this._isSuccess = false;
                this.FireOnEnd(false);
            }
        }
    }
}
