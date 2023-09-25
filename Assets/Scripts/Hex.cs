using UnityEngine;
using UnityEngine.UIElements;

public class Hex
{
    public float size;
    public float width;
    public float height;

    public AxialCoordinate axialCoord;
    public CubeCoordinate cubeCoord;

    public Vector3 Position 
    {
        get 
        {
            return HexUtils.GetPositionFromAxialCoordinate(axialCoord, width, height);
        }
    }

    public Hex(float size, int column, int row)
    {
        this.size = size;
        width = size * 2;
        height = Mathf.Sqrt(3) * size;

        axialCoord = new AxialCoordinate(column, row);
        cubeCoord = new CubeCoordinate(column, row, -column - row);
    }
}

public struct CubeCoordinate 
{
    public int q;//Column
    public int r;//Row
    public int s; // q + r + s = 0

    public CubeCoordinate(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }
}

public struct AxialCoordinate 
{
    public int q;//Column
    public int r;//Row

    public AxialCoordinate(int q, int r)
    {
        this.q = q;
        this.r = r;
    }
}
