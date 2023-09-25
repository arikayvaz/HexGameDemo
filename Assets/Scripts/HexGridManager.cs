using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class HexGridManager : MonoBehaviour
{
    public Vector2Int mapSize = Vector2Int.zero;
    public GameObject goHexPrefab = null;
    [Min(1f)]
    public float hexSize = 1f;
    public Transform trHexParent = null;
    public LayerMask inputLayerMask;

    private HexSettings hexSettings = null;
    private List<Hex> hexList = null;
    private Dictionary<Hex, GameObject> goHexDict = null;

    private void Awake()
    {
        InitHexSettings();
        SetHexes();
        SpawnHexes();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out hit, 1000f, inputLayerMask))
            {
                Vector3 hitPos = hit.point;
                hitPos.y = 0f;

                AxialCoordinate coord = HexUtils.GetAxialCoordinateFromPosition(hitPos, hexSize);

                Debug.Log(coord);

                if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
                {
                    foreach (Hex hex in hexList)
                    {
                        bool isActive = hex.axialCoord == coord;

                        if (isActive)
                            Debug.Log("Found! " + coord);

                        GameObject goHex;

                        goHexDict.TryGetValue(hex, out goHex);

                        if (goHex == null)
                            continue;

                        goHex.SetActive(isActive);
                    }//foreach (Hex hex in hexList)
                }//if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
            }//if (Physics.Raycast(r, out hit, 1000f, inputLayerMask))
            return;
        }//if (Input.GetMouseButtonDown(0))

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
            {
                foreach (Hex hex in hexList)
                {
                    GameObject goHex;

                    goHexDict.TryGetValue(hex, out goHex);

                    if (goHex == null)
                        continue;

                    goHex.SetActive(true);
                }//foreach (Hex hex in hexList)
            }//if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
        }
    }

    private void InitHexSettings()
    {
        hexSettings = new HexSettings(hexSize, mapSize);
    }

    private void SetHexes()
    {
        if (hexSettings == null)
            return;

        hexList = new List<Hex>();

        int mapLength = hexSettings.mapLength;

        for (int q = -mapLength; q <= mapLength; q++)
        {
            int r1 = Mathf.Max(-mapLength, -q - mapLength);
            int r2 = Mathf.Min(mapLength, -q + mapLength);

            for (int r = r1; r <= r2; r++)
            {
                Hex hex = new Hex(q, r);
                hexList.Add(hex);
            }//for (int r = r1; r <= r2; r++)
        }//for (int q = -mapLength; q <= mapLength; q++)
    }

    private void SpawnHexes()
    {
        if (hexSettings == null)
            return;

        if (hexList == null || hexList.Count < 1)
            return;

        goHexDict = new Dictionary<Hex, GameObject>();

        foreach (Hex hex in hexList)
        {
            Vector3 pos = HexUtils.GetPositionFromAxialCoordinate(hex.axialCoord, hexSettings.width, hexSettings.height);

            GameObject goHex = Instantiate(goHexPrefab);
            goHex.name = $"Hex({hex.axialCoord.q},{hex.axialCoord.r})";
            goHex.transform.position = pos;

            goHexDict.Add(hex, goHex);
        }
    }

    /*
    private void InitHexagonalGrid() 
    {
        /*
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
        */
}
