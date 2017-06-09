using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//填充的类型
public enum PadType { 
    NORMAL,                     //连接主体
    LINE,                       //连接线
    OBSTACLE,                   //障碍
    MAX
};

//填充对象
public class Cube
{
    public int id;              //对象编号
    public int row;             //填充到哪一行
    public int col;             //填充到哪一列
    public PadType padType;     //填充类型
}

//地图单元格
public class MapCell
{
    public int rowIndex;
    public int colIndex;
    public bool canPadding;     //是否允许填充
    public Cube cube;           //已填充的对象(可以替换) 

    public MapCell(int _rowIndex, int _colIndex, bool _canPadding, Cube _cube)
    {
        rowIndex = _rowIndex;
        colIndex = _colIndex;
        canPadding = _canPadding;
        cube = _cube;
    }
}
