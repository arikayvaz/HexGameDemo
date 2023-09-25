using UnityEngine;

public static class HexUtils
{
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
        bool shouldOffset = axial.q % 2 == 0;

        float horizontalDistance = width * (3f/4f);
        float verticalDistance = height;

        float offset = shouldOffset ? height / 2f : 0f;

        float xPosition = axial.q * horizontalDistance;
        float zPosition = axial.r * verticalDistance - offset;

        return new Vector3(xPosition, 0f, zPosition);
    }

    public static AxialCoordinate GetCenterHexAxialCoordinate(int mapSizeX, int mapSizeY) 
    {
        int xOffset = mapSizeX % 2 == 0 ? 0 : 1;
        int yOffset = mapSizeY % 2 == 0 ? 0 : 1;

        int q = (mapSizeX - xOffset) / 2;
        int r = (mapSizeY - yOffset) / 2;

        return new AxialCoordinate(q, r);
    }
}
