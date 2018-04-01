using System;
using System.Collections.Generic;
using UnityEngine;
using _UnityTicker_Internal;

/// <summary>
/// Unity 时间和计时器相关
/// </summary>
public class UnityTicker : BaseModule {
    private event Listener onUpdate;
    private event Listener onFixedUpdate;

    private ulong tickerIdIndex = 0;

    private _TimeTicker timeTicker = new _TimeTicker();

    public override void Initialize() {
        base.Initialize();

        // Time Ticker
        AddUpdateCallback(timeTicker.OnUpdate);
    }


    public void AddUpdateCallback(Listener _callback) {
        onUpdate += _callback;
    }

    public void RemoveUpdateCallback(Listener _callback) {
        onUpdate -= _callback;
    }

    public void AddFixedUpdateCallback(Listener _callback) {
        onFixedUpdate += _callback;
    }

    public void RemoveFixedUpdateCallback(Listener _callback) {
        onFixedUpdate -= _callback;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (onUpdate != null) {
            onUpdate();
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (onFixedUpdate != null) {
            onFixedUpdate();
        }
    }


    private ulong GetTickerId() {
        return ++tickerIdIndex;
    }

    // -------------------- Extension --------------------

    /// <summary>
    /// 停止并移除一个 Ticker
    /// </summary>
    /// <param name="_tickerId"></param>
    public void StopTicker(ulong _tickerId) {
        timeTicker.TryRemoveTicker(_tickerId);
    }

    /// <summary>
    /// 时间迭代，从 _from 到 _to，按步长 Time.deltaTime 迭代
    /// </summary>
    /// <param name="_from">起始值</param>
    /// <param name="_to">终止值</param>
    /// <param name="_callback">每次迭代的回调</param>
    public ulong AddTicker(float _from, float _to, Action<float> _callback) {
        ulong tickerId = GetTickerId();
        timeTicker.AddTicker(tickerId,_from, _to, _callback);
        return tickerId;
    }

    /// <summary>
    /// 时间迭代，从 _from 到 _to，按步长 Time.deltaTime 迭代
    /// </summary>
    /// <param name="_from">起始值</param>
    /// <param name="_to">终止值</param>
    /// <param name="_callback">每次迭代的回调</param>
    /// <param name="_delayTime">延迟 _delayTime 秒后才开始这个Ticker</param>
    public ulong AddTicker(float _from, float _to, Action<float> _callback, float _delayTime) {
        ulong tickerId = GetTickerId();
        //timeTicker.AddTicker(tickerId, _from, _to, _callback);
        // TODO:
        return tickerId;
    }

    /// <summary>
    /// 时间迭代，从 _from 到 _to，按步长 Time.deltaTime 迭代
    /// </summary>
    /// <param name="_from">起始值</param>
    /// <param name="_to">终止值</param>
    /// <param name="_callback">每次迭代的回调</param>
    public ulong AddTicker(int _from, int _to, Action<int> _callback) {
        ulong tickerId = GetTickerId();
        timeTicker.AddTicker(tickerId , _from, _to, _callback);
        return tickerId;
    }

    /// <summary>
    /// 时间迭代，从 _from 到 _to，按步长 Time.deltaTime 迭代
    /// </summary>
    /// <param name="_from">起始值</param>
    /// <param name="_to">终止值</param>
    /// <param name="_callback">每次迭代的回调</param>
    /// <param name="_delayTime"> 延迟 _delayTime 秒后才开始这个Ticker </param>
    public ulong AddTicker(int _from, int _to, Action<int> _callback, float _delayTime) {
        ulong tickerId = GetTickerId();
        //timeTicker.AddTicker(tickerId, _from, _to, _callback);
        // TODO:
        return tickerId;
    }

    /// <summary>
    /// 每隔 _callRate 秒调用一次，一共调用 _callCount 次，
    /// </summary>
    /// <param name="_callRate">每次调用的间隔秒数</param>
    /// <param name="_callCount">一共调用多少次</param>
    /// <param name="_callback">每次的调用的回调</param>
    /// <returns></returns>
    public ulong AddTicker(float _callRate, int _callCount, Action _callback) {
        ulong tickerId = GetTickerId();
        // TODO: 

        return tickerId;
    }

    /// <summary>
    /// 每隔 _callRate 秒调用一次，一共调用 _callCount 次，
    /// </summary>
    /// <param name="_callRate">每次调用的间隔秒数</param>
    /// <param name="_callCount">一共调用多少次</param>
    /// <param name="_callback">每次的调用的回调</param>
    /// <param name="_delayTime">延迟 _delayTime 秒后再开始这个Ticker </param>
    /// <returns></returns>
    public ulong AddTicker(float _callRate, int _callCount, Action _callback, float _delayTime) {
        ulong tickerId = GetTickerId();
        // TODO: 

        return tickerId;
    }

    /// <summary>
    /// 每隔 _callRate 秒调用一次，一直调用下去，直到手动 StopTicker
    /// </summary>
    /// <param name="_callRate">每次调用间隔的秒数</param>
    /// <param name="_callback">每次的调用回调</param>
    /// <returns></returns>
    public ulong AddTicker(float _callRate, Action _callback) {
        ulong tickerId = GetTickerId();
        // TODO: 

        return tickerId;
    }

    /// <summary>
    /// 每隔 _callRate 秒调用一次，一直调用下去，直到手动 StopTicker
    /// </summary>
    /// <param name="_callRate">每次调用间隔的秒数</param>
    /// <param name="_callback">每次的调用回调</param>
    /// <param name="_delayTime">延迟 _delayTime 秒后再开始这个Ticker </param>
    /// <returns></returns>
    public ulong AddTicker(float _callRate, Action _callback, float _delayTime) {
        ulong tickerId = GetTickerId();
        // TODO: 

        return tickerId;
    }

    /// <summary>
    /// 延迟 _delayTime 秒调用
    /// </summary>
    /// <param name="_callback">回调</param>
    /// <param name="_delayTime">延迟调用的秒数</param>
    /// <returns></returns>
    public ulong AddTicker(Action _callback, float _delayTime) {
        ulong tickerId = GetTickerId();
        // TODO: 

        return tickerId;
    }

}
