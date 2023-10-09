using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public static HexGridManager Instance { get; private set; } = null;

    public Vector2Int mapSize = Vector2Int.zero;
    public HexTile hexTilePrefab = null;
    [Min(1f)]
    public float hexSize = 1f;
    public Transform trHexParent = null;
    public LayerMask inputLayerMask;

    [Space]
    public GameObject goLandscapeEmpty = null;
    public GameObject goLandscapeForest = null;
    public GameObject goLandscapeField = null;

    private HexSettings hexSettings = null;
    private List<Hex> hexList = null;
    private Dictionary<Hex, HexTile> hexTileDict = null;

    public HexSettings HexSettings => hexSettings;

    private void Awake()
    {
        Instance = this;
        InitHexSettings();
        SetHexes();
        SpawnHexes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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

                AxialCoordinate coord = HexUtils.GetAxialCoordinateFromWorldPosition(hitPos, hexSize);

                if (hexList != null && hexList.Count > 0 && hexTileDict != null && hexTileDict.Count > 1)
                {
                    foreach (Hex hex in hexList)
                    {
                        bool isActive = hex.axialCoord == coord;

                        if (isActive)
                            Debug.Log("Found! " + coord);

                        HexTile hexTile;

                        hexTileDict.TryGetValue(hex, out hexTile);

                        if (hexTile == null)
                            continue;

                        hexTile.gameObject.SetActive(isActive);
                    }//foreach (Hex hex in hexList)
                }//if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
            }//if (Physics.Raycast(r, out hit, 1000f, inputLayerMask))
            return;
        }//if (Input.GetMouseButtonDown(0))

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (hexList != null && hexList.Count > 0 && hexTileDict != null && hexTileDict.Count > 1)
            {
                foreach (Hex hex in hexList)
                {
                    HexTile hexTile;

                    hexTileDict.TryGetValue(hex, out hexTile);

                    if (hexTile == null)
                        continue;

                    hexTile.gameObject.SetActive(true);
                }//foreach (Hex hex in hexList)
            }//if (hexList != null && hexList.Count > 0 && goHexDict != null && goHexDict.Count > 1)
        }
    }

    private void OnDrawGizmos()
    {
        //DrawHexGizmos();
    }

    public GameObject GetLandscapeGameObject(Landscapes landscape, Transform trParent) 
    {
        switch (landscape)
        {
            case Landscapes.Empty:
                return Instantiate(goLandscapeEmpty, trParent);
            case Landscapes.Forest:
                return Instantiate(goLandscapeForest, trParent);
            case Landscapes.Field:
                return Instantiate(goLandscapeField, trParent);
            default:
                return null;
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

        hexTileDict = new Dictionary<Hex, HexTile>();

        foreach (Hex hex in hexList)
        {
            Vector3 pos = HexUtils.GetPositionFromAxialCoordinate(hex.axialCoord, hexSettings.width, hexSettings.height);

            HexTile hexTile = Instantiate<HexTile>(hexTilePrefab);

            hexTile.InitTile(HexUtils.GetRandomLandscapeHexTileModel(hex));

            hexTile.transform.position = pos;

            hexTile.SpawnLandscapes();

            hexTileDict.Add(hex, hexTile);
        }
    }

    private void DrawHexGizmos() 
    {
        if (hexList == null || hexList.Count < 1)
            return;

        if (hexSettings == null)
            return;

        for (int i = 0; i < hexList.Count; i++)
        {
            Hex hex = hexList[i];

            Vector3 centerPos = HexUtils.GetPositionFromAxialCoordinate(hex.axialCoord, hexSettings.width, hexSettings.height);
            centerPos.y = 0.25f;

            Gizmos.DrawWireCube(centerPos, Vector3.one * 0.1f);

            for (int j = 0; j < 6; j++)
            {
                float angle_deg = 60 * j;
                float angle_rad = Mathf.PI / 180f * angle_deg;

                Vector3 pointPos = Vector3.zero;

                pointPos.x = centerPos.x + hexSettings.hexSize * Mathf.Cos(angle_rad);
                pointPos.y = 0.25f;
                pointPos.z = centerPos.z + hexSettings.hexSize * Mathf.Sin(angle_rad);

                Gizmos.DrawWireSphere(pointPos, 0.1f);
            }
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
