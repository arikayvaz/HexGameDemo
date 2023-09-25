using UnityEngine;

public class HexSettings
{
    public const float SQRT_3 = 1.73205080757f;

    public readonly float hexSize;
    public readonly Vector2Int mapSize;
    public readonly int mapLength;

    public readonly float width;
    public readonly float height;

    public HexSettings(float hexSize, Vector2Int mapSize)
    {
        this.hexSize = hexSize;

        (float, float) hexWidthAndHeight = HexUtils.CalculateHexWidthAndHeight(hexSize);
        width = hexWidthAndHeight.Item1;
        height = hexWidthAndHeight.Item2;

        this.mapSize = mapSize;
        mapLength = Mathf.Max(mapSize.x, mapSize.y);
;    }
}
