using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvexHullXZ {

    private int startIndex = 0;
    List<int> borderPointsIndex = new List<int>();

    /// <summary>
    /// 在离散的点中获取构成凸包围的边界点
    /// NOTE: 2017.10.15 - 已经忘记这是要干嘛的了
    /// </summary>
    /// <param name="_pointSet"></param>
    /// <returns></returns>
    public List<Vector3> GetConvexPoint(List<Vector3> _pointSet) {
        
        if(_pointSet == null || _pointSet.Count == 1) {
            return _pointSet;
        }

        Vector3 prevNormDir = Vector3.right;
        int currIndex = GetMaxZAndMinXPointIndex(_pointSet);
        startIndex = currIndex;
        Vector3 currPointPos = _pointSet[currIndex];

        borderPointsIndex.Add(currIndex);

        int maxDotPointIndex = GetMaxDotPointIndexBetweenVectorAndFromCurrPointToSearchPoint(_pointSet,startIndex, currPointPos, prevNormDir);
        borderPointsIndex.Add(maxDotPointIndex);

        while (maxDotPointIndex != startIndex) {
            Vector3 selectedPos = _pointSet[maxDotPointIndex];
            prevNormDir = (selectedPos - currPointPos).normalized;
            currPointPos = selectedPos;
            maxDotPointIndex = GetMaxDotPointIndexBetweenVectorAndFromCurrPointToSearchPoint(_pointSet,maxDotPointIndex, currPointPos, prevNormDir);
            borderPointsIndex.Add(maxDotPointIndex);
        }

        List<Vector3> result = new List<Vector3>();
        for(int i = 0; i < borderPointsIndex.Count; ++i) {
            int index = borderPointsIndex[i];
            result.Add(_pointSet[index]);
        }
        return result;
    }


    /// <summary>
    /// 获取多边形的重心
    /// </summary>
    /// <param name="_points"></param>
    /// <returns></returns>
    public Vector3 GetConvexCenterPos(List<Vector3> _points) {
        if(_points.Count == 1) {
            return _points[0];
        }

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < _points.Count - 1; ++i) {
            Vector3 currPoint = _points[i];
            sum += currPoint;
        }

        Vector3 centerPos = sum / (_points.Count - 1);
        return centerPos;
    }

    /// <summary>
    /// 获取外围包围盒
    /// </summary>
    /// <param name="_points"></param>
    /// <param name="_distance"></param>
    /// <returns></returns>
    public List<Vector3> GetBoundOfConvex(List<Vector3> _points, float _distance) {
        List<Vector3> boundList = new List<Vector3>();
        Vector3 centerPos = GetConvexCenterPos(_points);
        for(int i = 0; i < _points.Count; ++i) {
            Vector3 currPos = _points[i];
            Vector3 normDir = (currPos - centerPos).normalized;
            float magnitude = (currPos - centerPos).magnitude;
            Vector3 boundPos = centerPos + (magnitude + _distance) * normDir;
            boundList.Add(boundPos);
        }

        return boundList;
    }


    /// <summary>
    /// 在z轴最大的情况下获取x轴最小的点作为起始点
    /// </summary>
    /// <param name="_pointSet"></param>
    /// <returns></returns>
    private int GetMaxZAndMinXPointIndex(List<Vector3> _pointSet) {
        int index = -1;
        float z = 0.0f;
        float x = 0.0f;

        for(int i = 0; i < _pointSet.Count; ++i) {
            if(index == -1) {
                index = i;
                z = _pointSet[i].z;
                x = _pointSet[i].x;
            } else {
                if(_pointSet[i].z > z) {
                    index = i;
                    z = _pointSet[i].z;
                    x = _pointSet[i].x;
                }
                else if(_pointSet[i].z == z) {
                    if(_pointSet[i].x < x) {
                        index = i;
                        z = _pointSet[i].z;
                        x = _pointSet[i].x;
                    }
                }
            }
        }

        return index;
    }

    /// <summary>
    /// 选择下一个点
    /// </summary>
    /// <param name="_pointSet"></param>
    /// <param name="_currIndex"></param>
    /// <param name="_currPointPos"></param>
    /// <param name="_normDir"></param>
    /// <returns></returns>
    private int GetMaxDotPointIndexBetweenVectorAndFromCurrPointToSearchPoint(List<Vector3> _pointSet,int _currIndex, Vector3 _currPointPos, Vector3 _normDir) {
        int resultIndex = -1;
        float dot = 0.0f;
        float magnitude = 0.0f;


        for(int i = 0; i < _pointSet.Count; ++i) {
            int checkIndex = i;
            // 禁止点连接自身
            if(_currIndex == checkIndex) {
                continue;
            }

            // 禁止连接不是起始点的已经连过的点
            if (borderPointsIndex.Contains(checkIndex) && checkIndex != startIndex) {
                continue;
            }

            Vector3 checkPos = _pointSet[i];
            Vector3 checkNormDir = (checkPos - _currPointPos).normalized;
            float checkMagnitude = (checkPos - _currPointPos).magnitude;
            // 计算两个单位单位向量的点积
            float checkDot = Vector3.Dot(_normDir, checkNormDir);

            if (resultIndex < 0) {
                resultIndex = checkIndex;
                dot = checkDot;
                magnitude = checkMagnitude;
            }
            else if(checkDot > dot) {
                resultIndex = checkIndex;
                dot = checkDot;
                magnitude = checkMagnitude;
            }
            else if(checkDot == dot) {
                if(checkMagnitude < magnitude) {
                    resultIndex = checkIndex;
                    dot = checkDot;
                    magnitude = checkMagnitude;
                }
            }
        }

        return resultIndex;
    }
    
}
