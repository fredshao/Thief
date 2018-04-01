using System;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve {

    /// <summary>
    /// 获取个条曲线
    /// </summary>
    /// <param name="_point0">起始点</param>
    /// <param name="_point1">中间点</param>
    /// <param name="_point2">终点</param>
    /// <param name="_startValue">增量起始值</param>
    /// <param name="_maxValue">最终值，最大为1</param>
    /// <param name="_growthFactor">增长因子，影响点的密度,值越小两点之间距离越近</param>
    /// <returns></returns>
    public static List<Vector3> GetCurve(Vector3 _point0, Vector3 _point1, Vector3 _point2, float _startValue = 0.0f, float _maxValue = 1.0f, float _growthFactor = 0.06f) {
        _startValue = Mathf.Clamp(_startValue, 0.0f, 1.0f);
        _maxValue = Mathf.Clamp(_maxValue, 0.0f, 1.0f);
        List<Vector3> curve = new List<Vector3>();
        float t = _startValue;
        while (t <= _maxValue) {
            float x = ((1 - t) * (1 - t)) * _point0.x + 2 * t * (1 - t) * _point1.x + t * t * _point2.x;
            float y = ((1 - t) * (1 - t)) * _point0.y + 2 * t * (1 - t) * _point1.y + t * t * _point2.y;
            float z = ((1 - t) * (1 - t)) * _point0.z + 2 * t * (1 - t) * _point1.z + t * t * _point2.z;
            Vector3 pos = new Vector3(x, y, z);
            curve.Add(pos);
            t += _growthFactor;
        }

        return curve;
    }

    /// <summary>
    /// 返回曲线在某一时间t上的点
    /// </summary>
    /// <param name="_point0">起始点</param>
    /// <param name="_point1">中间点</param>
    /// <param name="_point2">终止点</param>
    /// <param name="t">当前时间t</param>
    /// <returns></returns>
    public static Vector3 GetCurvePoint(Vector3 _point0, Vector3 _point1, Vector3 _point2, float t) {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = ((1 - t) * (1 - t)) * _point0.x + 2 * t * (1 - t) * _point1.x + t * t * _point2.x;
        float y = ((1 - t) * (1 - t)) * _point0.y + 2 * t * (1 - t) * _point1.y + t * t * _point2.y;
        float z = ((1 - t) * (1 - t)) * _point0.z + 2 * t * (1 - t) * _point1.z + t * t * _point2.z;
        Vector3 pos = new Vector3(x, y, z);
        return pos;
    }

}
