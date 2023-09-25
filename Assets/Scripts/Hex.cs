using UnityEngine;
using UnityEngine.UIElements;

public class Hex
{
    public AxialCoordinate axialCoord;
    public CubeCoordinate cubeCoord;

    public Hex(int column, int row)
    {
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

    public override string ToString()
    {
        return $"q:{q} r:{r} s{s}";
    }

    public static bool operator ==(CubeCoordinate a, CubeCoordinate b)
    {
        return a.q == b.q && a.r == b.r && a.s == b.s;
    }

    public static bool operator !=(CubeCoordinate a, CubeCoordinate b)
    {
        return a.q == b.q || a.r == b.r || a.s == b.s;
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

    public override string ToString()
    {
        return $"q:{q} r:{r}";
    }

    public static bool operator ==(AxialCoordinate a, AxialCoordinate b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(AxialCoordinate a, AxialCoordinate b)
    {
        return a.q == b.q || a.r == b.r;
    }

    public override bool Equals(object obj)
    {
        return (obj is AxialCoordinate) && this == (AxialCoordinate)obj;
    }
}
