using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UnityTicker_Internal {
    public class _TimeTicker {
        private List<ulong> taskIdList = new List<ulong>();
        private Dictionary<ulong, _TimeTickerTask> taskDict = new Dictionary<ulong, _TimeTickerTask>();

        public void OnUpdate() {
            for(int i = taskIdList.Count - 1; i >= 0; --i) {
                ulong taskId = taskIdList[i];
                _TimeTickerTask task = taskDict[taskId];
                task.Ticker(Time.deltaTime);
            }
        }

        private void OnTaskFinish(ulong _taskId) {
            taskIdList.Remove(_taskId);
            taskDict.Remove(_taskId);
        }

        public void AddTicker(ulong _id, float _from, float _to, Action<float> _callback) {
            _TimeTickerTask task = new _TimeTickerTask(_from, _to, _callback);
            task.taskId = _id;
            task.finishCallback = OnTaskFinish;
            taskDict.Add(_id, task);
            taskIdList.Add(_id);
        }

        public void AddTicker(ulong _id, int _from, int _to, Action<int> _callback) {
            _TimeTickerTask task = new _TimeTickerTask(_from, _to, _callback);
            task.taskId = _id;
            task.finishCallback = OnTaskFinish;
            taskDict.Add(_id, task);
            taskIdList.Add(_id);
        }

        public void TryRemoveTicker(ulong _id) {
            if (taskDict.ContainsKey(_id)) {
                taskIdList.Remove(_id);
                taskDict.Remove(_id);
            }
        }
    }



    public enum _TimeTickerTaskType {
        Int,
        Float,
    }

    public class _TimeTickerTask {
        public ulong taskId;
        public Action<ulong> finishCallback;

        public float from;
        public float to;
        public float currValue;
        public Action<float> floatCallback;
        public Action<int> intCallback;
        public _TimeTickerTaskType taskType;


        private int reverse = 1;

        public _TimeTickerTask(float _from, float _to, Action<float> _callback) {
            taskType = _TimeTickerTaskType.Float;
            from = _from;
            to = _to;
            floatCallback = _callback;
            currValue = from;

            if(from > to) {
                reverse = -1;
            }
        }

        public _TimeTickerTask(int _from, int _to, Action<int> _callback) {
            taskType = _TimeTickerTaskType.Int;
            from = _from;
            to = _to;
            intCallback = _callback;
            currValue = from;

            if(from > to) {
                reverse = -1;
            }
        }

        public void Ticker(float _step) {
            if(taskType == _TimeTickerTaskType.Float) {
                currValue += _step * reverse;

                if(reverse < 0) {
                    if(currValue <= to) {
                        currValue = to;
                        if(floatCallback != null) {
                            floatCallback(currValue);
                        }
                        this.finishCallback(this.taskId);
                    } else {
                        if(floatCallback != null) {
                            floatCallback(currValue);
                        }
                    }
                } else {
                    if(currValue >= to) {
                        currValue = to;
                        if(floatCallback != null) {
                            floatCallback(currValue);
                        }
                        this.finishCallback(this.taskId);
                    } else {
                        if(floatCallback != null) {
                            floatCallback(currValue);
                        }
                    }
                }
            }
            else if(taskType == _TimeTickerTaskType.Int) {
                currValue += _step * reverse;
                if(reverse < 0) {
                    if((int)currValue <= (int)to) {
                        if(intCallback != null) {
                            intCallback((int)to);
                        }
                        this.finishCallback(this.taskId);
                    } else {
                        if(intCallback != null) {
                            intCallback((int)currValue);
                        }
                    }
                } else {
                    if((int)currValue >= (int)to) {
                        if(intCallback != null) {
                            intCallback((int)to);
                        }
                        this.finishCallback(this.taskId);
                    } else {
                        if(intCallback != null) {
                            intCallback((int)currValue);
                        }
                    }
                }
            }
        }
    }

}