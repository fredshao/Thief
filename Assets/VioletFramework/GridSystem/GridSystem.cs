//#define TEST
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 格子系统
/// 底层网格系统，为上层提供从任何一个格子到任何一个格子的路径
/// 从左下到右上增长
/// </summary>
public class GridSystem : BaseModule {
    public static int ROW = 1000;
    public static int COL = 1000;
    public static float gridSize = 1.0f;

    private Dictionary<int, _Point> gridDict = new Dictionary<int, _Point>();
    private Dictionary<int, _Point> openDict = new Dictionary<int, _Point>();
    private Dictionary<int, _Point> closeDict = new Dictionary<int, _Point>();

    private _Point startPoint;
    private _Point goalPoint;

    public override void Initialize() {
        base.Initialize();
        ConstructGridWorld();
    }

    public List<int> SearchPath(Vector3 _currPos, Vector3 _goalPos) {
        int startIndex = WorldPosToGridIndex(_currPos);
        int goalIndex = WorldPosToGridIndex(_goalPos);
        return SearchPath(startIndex, goalIndex);
    }

    public List<int> SearchPath(int _startIndex, int _goalIndex) {
        gridDict.TryGetValue(_startIndex, out startPoint);
        gridDict.TryGetValue(_goalIndex, out goalPoint);

#if TEST
        startPoint.walkable = 1;
        goalPoint.walkable = 1;
#endif

        if (startPoint == null) { Ulog.LogError(this, "StartPoint Is NULL:", _startIndex); return null;  }
        if (goalPoint == null) { Ulog.LogError(this, "GoalPoint is NULL:", _goalIndex); return null; }

        ResetPathFindingEnv();

        AddPointToOpenList(startPoint);

        while(!IsOpenListEmpty()) {
            _Point minFPoint = GetMinFPointFromOpenList();
            RemovePointFromOpenList(minFPoint);
            AddPointToCloseList(minFPoint);

            if(minFPoint == goalPoint) {
                //return null;
                return ConstructFinalPath(minFPoint);
            }

            _Point leftPoint = GetLeftPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, leftPoint, 10);

            _Point rightPoint = GetRightPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, rightPoint, 10);

            _Point upPoint = GetUpPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, upPoint, 10);

            _Point downPoint = GetDownPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, downPoint, 10);

            _Point leftUpPoint = GetLeftUpPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, leftUpPoint, 14);

            _Point leftDownPoint = GetLeftDownPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, leftDownPoint, 14);

            _Point rightUpPoint = GetRightUpPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, rightUpPoint, 14);

            _Point rightDownPoint = GetRightDownPoint(minFPoint);
            ProcessNeighborPoint(minFPoint, rightDownPoint, 14);
        }

        Ulog.LogError(this, "寻路失败！");

        return null;
    }

    public List<Vector3> SearchGridPath(Vector3 _currPos, Vector3 _goalPos) {
        List<int> indexPath = SearchPath(_currPos, _goalPos);
        if(indexPath == null) {
            return null;
        }

        List<Vector3> gridPath = new List<Vector3>();
        for(int i = 0; i < indexPath.Count; ++i) {
            Vector3 pos = GridIndexToWorldPos(indexPath[i]);
            gridPath.Add(pos);
        }

        return gridPath;
    }

    public List<Vector3> SearchCurvePath(Vector3 _currPos, Vector3 _goalPos) {
        List<Vector3> gridPath = SearchGridPath(_currPos, _goalPos);
        if(gridPath == null) {
            return null;
        }

        if(gridPath.Count < 3) {
            return gridPath;
        }

        List<Vector3> curvePath = new List<Vector3>();
        Vector3 point0 = gridPath[0];
        Vector3 point1;
        Vector3 point2;

        float maxValue = 0.5f;

        curvePath.Add(point0);

        int max = gridPath.Count - 1;

        gridPath[max] = _goalPos;

        for(int i = 1; i < max; ++i) {
            point1 = gridPath[i];
            point2 = gridPath[i + 1];

            if(i + 1 == max) {
                maxValue = 1.0f;
            }

            List<Vector3> curve = BezierCurve.GetCurve(point0, point1, point2, 0.0f, maxValue, 0.25f);
            for(int x = 1; x < curve.Count; ++x) {
                curvePath.Add(curve[x]);
            }

            point0 = curve[curve.Count - 1];
        }

        return curvePath;
    }


    #region About PathFinding
    private void ResetPathFindingEnv() {
        openDict.Clear();
        closeDict.Clear();
        startPoint.parent = null;
        goalPoint.parent = null;
    }

    private void ConstructGridWorld() {
        for (int i = 0; i < ROW; ++i) {
            for (int j = 0; j < COL; ++j) {
                _Point p = CreatePointByRowAndCol(i, j);
                gridDict.Add(p.index, p);
#if TEST
                if(UnityEngine.Random.value > 0.5f) {
                    p.walkable = 0;
                }
#endif
            }
        }
    }

    private bool IsOpenListEmpty() {
        return openDict.Count <= 0;
    }
    
    private void AddPointToOpenList(_Point _p) {
        openDict.Add(_p.index, _p);
    }

    private void RemovePointFromOpenList(_Point _p) {
        openDict.Remove(_p.index);
    }

    private bool IsPointInOpenList(_Point _p) {
        return openDict.ContainsKey(_p.index);
    }

    private void AddPointToCloseList(_Point _p) {
        closeDict.Add(_p.index, _p);
    }

    private void RemovePointFromCloseList(_Point _p) {
        closeDict.Remove(_p.index);
    }

    private bool IsPointInCloseList(_Point _p) {
        return closeDict.ContainsKey(_p.index);
    }

    private void ProcessNeighborPoint(_Point _currPoint, _Point _neighborPoint, int _cost) {
        if(_neighborPoint == null) {
            return;
        }

        if (IsPointInCloseList(_neighborPoint)) {
            return;
        }

        if (!IsPointInOpenList(_neighborPoint)) {
            _neighborPoint.g = _cost;
            _neighborPoint.CalculateH(goalPoint);
            _neighborPoint.CalculateF();
            _neighborPoint.parent = _currPoint;
            AddPointToOpenList(_neighborPoint);
        } else {
            int newG = _currPoint.g + _cost;
            if(newG < _neighborPoint.g) {
                _neighborPoint.g = _cost;
                _neighborPoint.CalculateF();
                _neighborPoint.parent = _currPoint;
            }
        }
    }

    private _Point GetMinFPointFromOpenList() {
        _Point point = null;
        var enumer = openDict.GetEnumerator();
        while (enumer.MoveNext()) {
            if(point == null) {
                point = enumer.Current.Value;
            } else {
                if(enumer.Current.Value.f < point.f) {
                    point = enumer.Current.Value;
                }
            }
        }
        return point;
    }

    private List<int> ConstructFinalPath(_Point _currPoint) {
        _Point walker = _currPoint;
        Stack<int> pathStack = new Stack<int>();
        while(walker != null) {
            pathStack.Push(walker.index);
            walker = walker.parent;
        }

        List<int> path = new List<int>();

        while(pathStack.Count > 0) {
            int index = pathStack.Pop();
            path.Add(index);
        }

        pathStack.Clear();
        pathStack = null;

        return path;
    }

    private _Point GetLeftPoint(_Point _p) {
        int index = _p.index - 1;
        int row = _p.row;
        int col = _p.col - 1;

        if(row >= 0 && row < ROW && col >= 0 && col < COL) {
            return gridDict[index];
        }

        return null;
    }

    private _Point GetRightPoint(_Point _p) {
        int index = _p.index + 1;
        int row = _p.row;
        int col = _p.col + 1;

        if (row >= 0 && row < ROW && col >= 0 && col < COL) {
            return gridDict[index];
        }

        return null;
    }

    private _Point GetUpPoint(_Point _p) {
        int index = _p.index + COL;
        int row = _p.row + 1;
        int col = _p.col;

        if (row >= 0 && row < ROW && col >= 0 && col < COL) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }

        return null;
    }

    private _Point GetDownPoint(_Point _p) {
        int index = _p.index - COL;
        int row = _p.row - 1;
        int col = _p.col;
        if (row >= 0 && row < ROW && col >= 0 && col < COL) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }
        return null;
    }

    private _Point GetLeftUpPoint(_Point _p) {
        int index = _p.index - 1 + COL;
        int row = _p.row + 1;
        int col = _p.col - 1;
        if (row >= 0 && row < ROW && col >= 0 && col < ROW) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }
        return null;
    }

    private _Point GetLeftDownPoint(_Point _p) {
        int index = _p.index - 1 - COL;
        int row = _p.row - 1;
        int col = _p.col - 1;
        if (row >= 0 && row < ROW && col >= 0 && col < ROW) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }
        return null;
    }

    private _Point GetRightDownPoint(_Point _p) {
        int index = _p.index + 1 - COL;
        int row = _p.row - 1;
        int col = _p.col + 1;
        if (row >= 0 && row < ROW && col >= 0 && col < ROW) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }
        return null;
    }

    private _Point GetRightUpPoint(_Point _p) {
        int index = _p.index + 1 + COL;
        int row = _p.row + 1;
        int col = _p.col + 1;
        if (row >= 0 && row < ROW && col >= 0 && col < ROW) {
            if (gridDict[index].walkable == 1) {
                return gridDict[index];
            }
        }
        return null;
    }

    #endregion

    #region Helper Functions 
    /// <summary>
    /// 世界坐标转为格子索引
    /// </summary>
    /// <param name="_worldPos"></param>
    /// <returns></returns>
    public int WorldPosToGridIndex(Vector3 _worldPos) {
        int x = Mathf.RoundToInt(_worldPos.x);
        int z = Mathf.RoundToInt(_worldPos.z);
        int index = GetIndexByRowAndCol(z, x);
        return index;
    }

    /// <summary>
    /// 格子索引转为世界坐标 
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public Vector3 GridIndexToWorldPos(int _index) {
        int row = GetRowByIndex(_index);
        int col = GetColByIndex(_index);
        return new Vector3(col * gridSize, 0, row * gridSize);
    }

    /// <summary>
    /// 设置一个实体
    /// </summary>
    /// <param name="_worldPos"></param>
    /// <param name="_entity"></param>
    /// <returns></returns>
    public bool SetEntity(Vector3 _worldPos ,GridEntity _entity) {
        if(CanSetEntity(_worldPos,_entity) == false) {
            return false;
        }

        int centerIndex = WorldPosToGridIndex(_worldPos);
        int subHalf = (_entity.entityWidth - 1) / 2;
        int centerRow = GetRowByIndex(centerIndex);
        int centerCol = GetColByIndex(centerIndex);
        int startRow = centerRow - subHalf;
        int startCol = centerCol - subHalf;

        int index = 0;
        for (int r = 0; r < _entity.entityWidth; ++r) {
            for (int c = 0; c < _entity.entityWidth; ++c) {
                int gridValue = _entity.entityPattern[index++];
                if (gridValue == 0) {
                    int currRow = startRow + r;
                    int currCol = startCol + c;
                    _Point p = GetPointFromGridDictByRowAndCol(currRow, currCol);
                    p.walkable = 0;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 是否可以设置一个实体
    /// </summary>
    /// <param name="_worldPos"></param>
    /// <param name="_entity"></param>
    /// <returns></returns>
    public bool CanSetEntity(Vector3 _worldPos, GridEntity _entity) {
        int centerIndex = WorldPosToGridIndex(_worldPos);
        int subHalf = (_entity.entityWidth - 1) / 2;
        int centerRow = GetRowByIndex(centerIndex);
        int centerCol = GetColByIndex(centerIndex);
        int startRow = centerRow - subHalf;
        int startCol = centerCol - subHalf;

        int index = 0;
        for(int r = 0; r < _entity.entityWidth; ++r) {
            for(int c = 0; c < _entity.entityWidth; ++c) {
                int gridValue = _entity.entityPattern[index++];
                if(gridValue == 0) {
                    int currRow = startRow + r;
                    int currCol = startCol + c;
                    _Point p = GetPointFromGridDictByRowAndCol(currRow, currCol);
                    if(p.walkable == 0) {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    #endregion

    #region Internal Functions
    private _Point CreatePointByRowAndCol(int _row, int _col) {
        int index = GetIndexByRowAndCol(_row, _col);
        return new _Point(index, _row, _col);
    }

    private int GetIndexByRowAndCol(int _row, int _col) {
        return _row * COL + _col;
    }

    private int GetRowByIndex(int _index) {
        return _index / COL;
    }
    
    private int GetColByIndex(int _index) {
        return _index % COL;
    }

    private _Point GetPointFromGridDictByRowAndCol(int _row, int _col) {
        int index = GetIndexByRowAndCol(_row, _col);
        return GetPointFromGridDictByIndex(index);
    }

    private _Point GetPointFromGridDictByIndex(int _index) {
        gridDict.TryGetValue(_index, out _Point p);
        return p;
    }

    #endregion 
}

public class _Point {
    public int index;
    public int row;
    public int col;
    public int f;
    public int g;
    public int h;
    public int walkable = 1;

    public _Point parent;

    public _Point(int _index, int _row, int _col) {
        this.index = _index;
        this.row = _row;
        this.col = _col;
    }

    public void CalculateH(_Point _goalPoint) {
        int goalRow = _goalPoint.row;
        int goalCol = _goalPoint.col;
        int rowCost = Mathf.Abs(goalRow - row);
        int colCost = Mathf.Abs(goalCol - col);
        h = (rowCost + colCost) * 10;
    }

    public void CalculateF() {
        f = g + h;
    }
}