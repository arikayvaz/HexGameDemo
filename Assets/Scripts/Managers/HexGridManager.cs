using System.Collections.Generic;
using UnityEditor;
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
    private Dictionary<CubeCoordinate, Hex> hexDict = null;
    private Dictionary<Hex, HexTile> hexTileDict = null;

    public Dictionary<Hex, HexTile> HextTileDict => hexTileDict;
    public Dictionary<CubeCoordinate, Hex> HexDict => hexDict;

    public HexSettings HexSettings => hexSettings;

    private void Awake()
    {
        Instance = this;
        InitHexSettings();
        SetHexes();
        SpawnHexes();
    }

    private void Start()
    {
        HexNodeManager.Instance.SetHexNodes();
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

                if (hexDict != null && hexDict.Count > 0 && hexTileDict != null && hexTileDict.Count > 1)
                {
                    foreach (Hex hex in hexDict.Values)
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
            if (hexDict != null && hexDict.Count > 0 && hexTileDict != null && hexTileDict.Count > 1)
            {
                foreach (Hex hex in hexDict.Values)
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
        DrawHexGizmos();
    }

    public GameObject GetLandscapeGameObject(LandscapeTypes landscape, Transform trParent) 
    {
        switch (landscape)
        {
            case LandscapeTypes.Empty:
                return Instantiate(goLandscapeEmpty, trParent);
            case LandscapeTypes.Forest:
                return Instantiate(goLandscapeForest, trParent);
            case LandscapeTypes.Field:
                return Instantiate(goLandscapeField, trParent);
            default:
                return null;
        }
    }

    public LandscapeModel GetNeighbourHexLandscape(Hex parentHex, Directions landscapeDirection) 
    {
        /*
        Hex neighbourHex = GetNeighbourHex(parentHex, landscapeDirection);

        if (!hexDict.ContainsValue(neighbourHex))
            return null;

        */
        HexTile neighbourHexTile = HetNeighbourHexTile(parentHex, landscapeDirection);
        //hexTileDict.TryGetValue(neighbourHex, out neighbourHexTile);

        if (neighbourHexTile == null)
            return null;

        return neighbourHexTile.GetNeighbourLandscape(landscapeDirection);
    }

    public HexTile HetNeighbourHexTile(Hex hex, Directions direction) 
    {
        Hex neighbourHex = GetNeighbourHex(hex, direction);

        if (!hexDict.ContainsValue(neighbourHex))
            return null;

        HexTile neighbourHexTile = null;
        hexTileDict.TryGetValue(neighbourHex, out neighbourHexTile);

        return neighbourHexTile;
    }

    public Hex GetNeighbourHex(Hex hex, Directions direction) 
    {
        if (hexDict == null || hexDict.Count < 1)
            return null;

        if (hexTileDict == null || hexTileDict.Count < 1)
            return null;

        CubeCoordinate neighbourCoord = HexUtils.GetCubeCoordinateNeighbour(hex.cubeCoord, direction);

        if (!hexDict.ContainsKey(neighbourCoord))
            return null;

        Hex neighbour = null;
        hexDict.TryGetValue(neighbourCoord, out neighbour);

        return neighbour;
    }

    public HexTile[] GetAllNeighbourHexTiles(Hex parentHex) 
    {
        if (hexDict == null || hexDict.Count < 1)
            return null;

        if (hexTileDict == null || hexTileDict.Count < 1)
            return null;

        CubeCoordinate[] coords = HexUtils.GetAllCubeCoordinateNeighbours(parentHex.cubeCoord);
        HexTile[] hexTiles = new HexTile[coords.Length];

        for (int i = 0; i < coords.Length; i++)
        {
            if (!hexDict.ContainsKey(coords[i]))
                continue;

            Hex hex;
            hexDict.TryGetValue(coords[i], out hex);

            if (!hexTileDict.ContainsKey(hex))
                continue;

            hexTileDict.TryGetValue(hex, out hexTiles[i]);
        }

        return hexTiles;
    }

    private void InitHexSettings()
    {
        hexSettings = new HexSettings(hexSize, mapSize);
    }

    private void SetHexes()
    {
        if (hexSettings == null)
            return;

        hexDict = new Dictionary<CubeCoordinate, Hex>();

        int mapLength = hexSettings.mapLength;

        for (int q = -mapLength; q <= mapLength; q++)
        {
            int r1 = Mathf.Max(-mapLength, -q - mapLength);
            int r2 = Mathf.Min(mapLength, -q + mapLength);

            for (int r = r1; r <= r2; r++)
            {
                Hex hex = new Hex(q, r);
                hexDict.Add(hex.cubeCoord, hex);
            }//for (int r = r1; r <= r2; r++)
        }//for (int q = -mapLength; q <= mapLength; q++)
    }

    private void SpawnHexes()
    {
        if (hexSettings == null)
            return;

        if (hexDict == null || hexDict.Count < 1)
            return;

        hexTileDict = new Dictionary<Hex, HexTile>();

        foreach (Hex hex in hexDict.Values)
        {
            Vector3 pos = HexUtils.GetPositionFromAxialCoordinate(hex.axialCoord, hexSettings.width, hexSettings.height);

            HexTile hexTile = Instantiate<HexTile>(hexTilePrefab);

            hexTile.InitTile(HexUtils.GetRandomLandscapeHexTileModel(hex), hex);

            hexTile.transform.position = pos;

            hexTile.SpawnLandscapes();

            hexTileDict.Add(hex, hexTile);
        }
    }

    private void DrawHexGizmos() 
    {
        if (hexTileDict == null || hexTileDict.Count < 1)
            return;

        int index = 0;

        foreach (HexTile tile in hexTileDict.Values)
        {
            Vector3 pos = tile.transform.position;

            pos.y += 0.1f;

            GUIStyle style = EditorStyles.boldLabel;
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;

            Handles.Label(pos, index.ToString(), style);

            index++;
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
