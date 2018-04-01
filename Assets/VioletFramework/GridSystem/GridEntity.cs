using System;
using System.Collections.Generic;

/// <summary>
/// entityWidth: 每一个实体描述都是nxn的格子，从左下到右上如果某一个格子占居，则在entityPattern对应的序列位置设置为0,其他位置设为1
/// </summary>
public class GridEntity {
    public int entityGridIndex;
    public int entityWidth;
    public int[] entityPattern;
}
