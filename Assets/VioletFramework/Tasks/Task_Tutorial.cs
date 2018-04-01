using System;
using System.Collections.Generic;
using Violet.Tasks;
public class Task_Tutorial {
    void Test() {
        
        // Delegate Task
        new DelegateTask(delegate (DelegateTask _task) {
            _task.OnCallback(false);
            _task.OnCallback(true);
        }).StartTask();


        // TaskChain Tutorial
        TaskChain taskChain = new TaskChain(TaskChainType.RunOneByOne);
        taskChain.AddTask(new DelegateTask(delegate (DelegateTask _task) {
            _task.OnCallback(true);
        }));

        VTask task = new VTask();
        taskChain.AddTask(task);
        taskChain.StartTask();

        // Normal Task
        WalkTask walkTask = new WalkTask();
        walkTask.StartTask();

        taskChain.AddTask(walkTask);
    }
}


public class WalkTask : VTask {

}