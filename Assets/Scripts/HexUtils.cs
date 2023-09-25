using Unity.VisualScripting;
using UnityEngine;

public static class HexUtils
{
    public static (float, float) CalculateHexWidthAndHeight(float hexSize)
    {
        float width = hexSize * 1.5f;
        float height = HexSettings.SQRT_3 * hexSize;

        return (width, height);
    }

    public static AxialCoordinate CubeToAxialCoordinate(CubeCoordinate cube) 
    {
        return new AxialCoordinate(cube.q, cube.r);
    }

    public static CubeCoordinate AxialToCubeCoordinate(AxialCoordinate axial) 
    {
        return new CubeCoordinate(axial.q, axial.r, -axial.q - axial.r);
    }

    public static Vector3 GetPositionFromAxialCoordinate(AxialCoordinate axial, float width, float height) 
    {
        Vector3 pos = Vector3.zero;

        //hexSize * 3.0f / 2.0f * q;
        pos.x = width * axial.q;
        //hexSize * Mathf.Sqrt(3.0f) * (r + q / 2.0f);
        pos.z = height * (axial.r + axial.q / 2.0f);

        return pos;
    }

    public static AxialCoordinate RoundToAxialCoordinate(float qFrac, float rFrac)
    {
        float sFrac = -qFrac - rFrac;
        return CubeToAxialCoordinate(RoundToCubeCoordinate(qFrac, rFrac, sFrac));
    }

    public static CubeCoordinate RoundToCubeCoordinate(float qFrac, float rFrac, float sFrac)
    {
        float qRound = Mathf.Round(qFrac);
        float rRound = Mathf.Round(rFrac);
        float sRound = Mathf.Round(sFrac);

        float qDiff = Mathf.Abs(qRound - qFrac);
        float rDiff = Mathf.Abs(rRound - rFrac);
        float sDiff = Mathf.Abs(sRound - sFrac);

        if (qDiff > rDiff && qDiff > sDiff)
            qRound = -rRound - sRound;
        else if (rDiff > sDiff)
            rRound = -qRound - sRound;
        else
            sRound = -qRound - rRound;

        return new CubeCoordinate((int)qRound, (int)rRound, (int)sRound);
    }

    public static AxialCoordinate GetAxialCoordinateFromWorldPosition(Vector3 position, float hexSize) 
    {
        float q = (position.x * (2.0f / 3.0f)) / hexSize;
        float r = ((-position.x / 3.0f) + ((HexSettings.SQRT_3 / 3.0f) * position.z)) / hexSize;

        return RoundToAxialCoordinate(q, r);
    }
}
