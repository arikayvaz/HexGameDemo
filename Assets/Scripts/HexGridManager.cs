using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public Vector2Int mapSize = Vector2Int.zero;
    public GameObject goHexPrefab = null;
    [Min(1)]
    public int hexSize = 1;
    public Transform trHexParent = null;

    private List<Hex> hexList = null;

    private void Awake()
    {
        //InitHexGrid();
        //SpawnHexes();
        InitHexagonalGrid();
    }

    private void OnDrawGizmos()
    {
        //DrawHexGizmos();
    }

    private void InitHexGrid() 
    {
        hexList = new List<Hex>(mapSize.x * mapSize.y);

        for (int i = 0; i < mapSize.x; i++)
        {
            //Column
            for (int j = 0; j < mapSize.y; j++)
            {
                //Row
                Hex hex = new Hex(hexSize, i, j);
                hexList.Add(hex);
            }
        }
    }

    private void SpawnHexes() 
    {
        if (hexList == null || hexList.Count < 1)
            return;

        foreach (Hex hex in hexList)
        {
            GameObject goHex = Instantiate(goHexPrefab, trHexParent);
            goHex.name = $"Hex ({hex.axialCoord.q},{hex.axialCoord.r})";
            goHex.transform.position = hex.Position;
            goHex.transform.SetParent(trHexParent);
        }
    }

    private void InitHexagonalGrid() 
    {
        int mapLength = Mathf.Max(mapSize.x, mapSize.y);
        Vector3 pos = Vector3.zero;

        for (int q = -mapLength; q <= mapLength; q++)
        {
            int r1 = Mathf.Max(-mapLength, -q-mapLength);
            int r2 = Mathf.Min(mapLength, -q+mapLength);

            for (int r = r1; r <= r2; r++)
            {
                pos.x = hexSize * 3.0f / 2.0f * q;
                pos.z = hexSize * Mathf.Sqrt(3.0f) * (r + q / 2.0f);

                GameObject goHex = Instantiate(goHexPrefab);
                goHex.name = $"Hex({q},{r})";
                goHex.transform.position = pos;
            }
        }
    }

    private void DrawHexGizmos() 
    {
        if (hexList == null || hexList.Count < 1)
            return;

        AxialCoordinate centerHexCoord = HexUtils.GetCenterHexAxialCoordinate(mapSize.x, mapSize.y);

        foreach (Hex hex in hexList)
        {
            bool centerHex = hex.axialCoord.q == centerHexCoord.q && hex.axialCoord.r == centerHexCoord.r;
            Handles.color = centerHex ? Color.blue : Color.white;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = centerHex ? Color.blue : Color.white;
            Handles.Label(hex.Position, $"{hex.axialCoord.q}, {hex.axialCoord.r}", style);
        }
    }
}
