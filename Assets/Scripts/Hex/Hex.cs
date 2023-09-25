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
