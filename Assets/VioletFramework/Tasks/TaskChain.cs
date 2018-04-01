using System;
using System.Collections.Generic;

namespace Violet.Tasks {
    public class TaskChain : VTask {
        private TaskChainType type = TaskChainType.RunOneByOne;

        private TaskChain_Queue taskQueue = null;

        public TaskChain(TaskChainType _chainType) {
            type = _chainType;

            switch (type) {
                case TaskChainType.RunOneByOne: {
                        taskQueue = new TaskChain_RunOneByOne();
                    }
                    break;
                case TaskChainType.RunOneByOneIgnoreError: {
                        taskQueue = new TaskChain_RunOneByOne_IgnoreError();
                    }
                    break;
                case TaskChainType.RunParallel: {
                        taskQueue = new TaskChain_RunParallel();
                    }
                    break;
                case TaskChainType.RunParallelIgnoreError: {
                        taskQueue = new TaskChain_RunParallel_IgnoreError();
                    }
                    break;
            }
        }

        ~TaskChain() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
            taskQueue.RemoveOnEnd(OnTaskChainEnd);
            taskQueue.Dispose();
            taskQueue = null;
        }

        public override void StartTask() {
            if (this.running) {
                Ulog.LogError("Task Chain is Running, Can't Start!");
                return;
            }
            base.StartTask();
            taskQueue.StartTask();
        }

        public override void StopTask() {
            base.StartTask();
            taskQueue.RemoveOnEnd(OnTaskChainEnd);
            taskQueue.StopTask();
        }

        public void AddTask(IVTask _task) {
            taskQueue.AddTask(_task);
        }

        public void RemoveTask(IVTask _task) {
            taskQueue.RemoveTask(_task);
        }

        public override void AddOnEnd(Listener<IVTask> _onEnd) {
            base.AddOnEnd(_onEnd);
            taskQueue.AddOnEnd(OnTaskChainEnd);
        }

        public override void RetryTask() {
            if (this.running) {
                Ulog.LogError("Task Chain is Running, Can't Retry");
            }
            base.RetryTask();
            taskQueue.RetryTask();
        }

        private void OnTaskChainEnd(IVTask _task) {
            _task.RemoveOnEnd(OnTaskChainEnd);
            this.FireOnEnd(_task.isSuccess);
        }
    }
}
