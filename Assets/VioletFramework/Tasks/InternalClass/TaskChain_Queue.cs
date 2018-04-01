using System;
using System.Collections.Generic;
using UnityEngine;

namespace Violet.Tasks {
    public class TaskChain_Queue : VTask {
        protected List<IVTask> taskList;

        public TaskChain_Queue() {
            taskList = new List<IVTask>();
        }

        ~TaskChain_Queue() {
            this.Dispose();
        }

        public override void Dispose() {
            base.Dispose();
            if (this.taskList != null) {
                for (int i = 0; i < taskList.Count; ++i) {
                    taskList[i].Dispose();
                }
                taskList.Clear();
            }

            taskList = null;
        }

        public void AddTask(IVTask _task) {
            taskList.Add(_task);
        }

        public void RemoveTask(IVTask _task) {
            if (taskList.Contains(_task)) {
                taskList.Remove(_task);
            }
        }
    }
}
