using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public enum RotationDirections 
{
    Clockwise = 1,
    Anticlockwise = -1
}

public static class HexUtils
{
    public static readonly Color[] GizmosColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.magenta,
        Color.cyan,
        Color.yellow,
        Color.black
    };


    /// <summary>
    /// Cube coordinate directions. Starts from north and goes clockwise
    /// </summary>
    public static readonly CubeCoordinate[] CubeDirectionCoordinates = {
        new CubeCoordinate(0, 1, -1), new CubeCoordinate(1, 0 , -1), new CubeCoordinate(1, -1, 0),
        new CubeCoordinate(0, -1, 1), new CubeCoordinate(-1, 0, 1), new CubeCoordinate(-1, 1, 0)
    };

    /// <summary>
    /// Axial coordinate directions. Starts from north and goes clockwise
    /// </summary>
    public static readonly AxialCoordinate[] AxialDirectionCoordinates = {
        new AxialCoordinate(0, 1), new AxialCoordinate(1, 0), new AxialCoordinate(1, -1),
        new AxialCoordinate(0, -1), new AxialCoordinate(-1, 0), new AxialCoordinate(-1, 1)
    };

    /// <summary>
    /// Direction Array. Starts from north and goes clockwise
    /// </summary>
    public static readonly Directions[] DirectionArray  = { Directions.North, Directions.NorthEast, Directions.SouthEast
            , Directions.South, Directions.SouthWest, Directions.NorthWest };

    public static CubeCoordinate GetCubeDirectionCoordinate(Directions direction) 
    {
        return CubeDirectionCoordinates[(int)direction];
    }

    public static AxialCoordinate GetAxialDirectionCoordinate(Directions direction)
    {
        return AxialDirectionCoordinates[(int)direction];
    }

    public static CubeCoordinate[] GetAllCubeCoordinateNeighbours(CubeCoordinate parentCoord) 
    {
        CubeCoordinate[] neighbours = new CubeCoordinate[CubeDirectionCoordinates.Length];

        for (int i = 0; i < neighbours.Length; i++)
            neighbours[i] = parentCoord.Add(CubeDirectionCoordinates[i]);

        return neighbours;
    }

    public static CubeCoordinate GetCubeCoordinateNeighbour(CubeCoordinate parentCoord, Directions direction) 
    {
        return parentCoord.Add(GetCubeDirectionCoordinate(direction));
    }

    public static AxialCoordinate[] GetAllAxialCoordinateNeighbours(AxialCoordinate parentCoord) 
    {
        AxialCoordinate[] neighbours = new AxialCoordinate[AxialDirectionCoordinates.Length];

        for (int i = 0; i < neighbours.Length; i++)
            neighbours[i] = parentCoord.Add(AxialDirectionCoordinates[i]);

        return neighbours;
    }

    public static AxialCoordinate GetAxialCoordinateNeighbour(AxialCoordinate parentCoord, Directions direction) 
    {
        return parentCoord.Add(GetAxialDirectionCoordinate(direction));
    }

    public static Directions GetNeighbourDirection(Directions direction) 
    {
        return direction == Directions.None ? Directions.None : (Directions)((3 + (int)direction) % 6);
    }

    const int DIRECTION_COUNT = 6;
    public static Directions GetSideDirection(Directions direction) 
    {
        if (direction == Directions.None)
            return Directions.None;

        int dirVal = (int)direction;

        return (Directions)((dirVal + 1) % DIRECTION_COUNT);
    }

    public static Directions RotateDirectionClockwise(Directions direction) 
    {
        if (direction == Directions.None)
            return Directions.None;

        int dirVal = (int)direction;
        int rotatedDirVal = dirVal + 1;

        return (Directions)(rotatedDirVal % DIRECTION_COUNT);
    }

    public static Directions RotateDirectionAnticlockwise(Directions direction) 
    {
        if (direction == Directions.None)
            return Directions.None;

        int dirVal = (int)direction;
        int rotatedDirVal = dirVal + 5;

        return (Directions)(rotatedDirVal % DIRECTION_COUNT);
    }

    public static Directions RotateDirection(Directions direction, RotationDirections rotation) 
    {
        switch (rotation)
        {
            case RotationDirections.Clockwise:
                return RotateDirectionClockwise(direction);
            case RotationDirections.Anticlockwise:
                return RotateDirectionAnticlockwise(direction);
            default:
                return direction;
        }
    }

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

    public static LandscapeTypes GetRandomLandscape() 
    {
        const int LANDSCAPE_VALUE_MIN = 1;
        const int LANDSCAPE_VALUE_MAX = 2;
        const int LANDSCAPE_DELTA = 100;

        return (LandscapeTypes)(Random.Range(LANDSCAPE_VALUE_MIN, LANDSCAPE_VALUE_MAX + 1) * LANDSCAPE_DELTA);
    }

    public static HexTileModel GetRandomLandscapeHexTileModel(Hex hex) 
    {
        LandscapeModel[] landscapeModels = new LandscapeModel[6];

        for (int i = 0; i < DirectionArray.Length; i++)
            landscapeModels[i] = new LandscapeModel(GetRandomLandscape(), DirectionArray[i]);

        return new HexTileModel(hex, landscapeModels, null);
    }

    public static Vector3 GetLandscapePosition(Vector3 hexTilePosition, Directions direction, float height) 
    {
        Vector3 position = hexTilePosition;
        position.y += 0.2f;

        if (direction == Directions.None)
            return position;

        int dirIndex = (int)direction;

        float angle_deg = 60f - (60f * dirIndex);
        float angle_rad = Mathf.PI / 180f * (angle_deg + 30f);

        float halfHeight = height * 0.5f;

        const float HEIGHT_OFFSET_MULT = 0.8f;

        position.x += (halfHeight * HEIGHT_OFFSET_MULT) * Mathf.Cos(angle_rad);
        position.z += (halfHeight * HEIGHT_OFFSET_MULT) * Mathf.Sin(angle_rad);
        

        return position;
    }

    public static Quaternion GetLandscapeRotation(Directions direction) 
    {
        if (direction == Directions.None)
            return Quaternion.identity;

        int dirIndex = (int)direction;

        float angle_deg = 60f * dirIndex;

        return Quaternion.Euler(0f, angle_deg, 0f);
    }
}
