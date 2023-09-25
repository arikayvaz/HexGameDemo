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

    public static AxialCoordinate GetAxialCoordinateFromPosition(Vector3 pos, float size)
    {
        float qFrac = ((2.0f / 3.0f) * pos.x) / size;
        float rFrac = ((-1.0f/3.0f) * pos.x + (HexSettings.SQRT_3 / 2.0f) * pos.z) / size;

        return RoundToAxialCoordinate(qFrac, rFrac);
    }

    public static CubeCoordinate GetCubeCoordinateFromPosition(Vector3 pos, float size)
    {
        float qFrac = ((2.0f / 3.0f) * pos.x) / size;
        float rFrac = ((-1.0f / 3.0f) * pos.x + (HexSettings.SQRT_3 / 2.0f) * pos.z) / size;
        float sFrac = -qFrac - rFrac;

        return RoundToCubeCoordinate(qFrac, rFrac, sFrac);
    }

    public static AxialCoordinate RoundToAxialCoordinate(float qFrac, float rFrac)
    {
        float sFrac = -qFrac - rFrac;
        return CubeToAxialCoordinate(RoundToCubeCoordinate(qFrac, rFrac, sFrac));
    }

    public static CubeCoordinate RoundToCubeCoordinate(float qFrac, float rFrac, float sFrac)
    {
        int q = Mathf.RoundToInt(qFrac);
        int r = Mathf.RoundToInt(rFrac);
        int s = Mathf.RoundToInt(sFrac);

        float qDiff = Mathf.Abs(q - qFrac);
        float rDiff = Mathf.Abs(r - rFrac);
        float sDiff = Mathf.Abs(s - sFrac);

        if (qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if (rDiff > sDiff)
            r = q - s;
        else
            s = -q - r;

        return new CubeCoordinate(q, r, s);
    }
}
