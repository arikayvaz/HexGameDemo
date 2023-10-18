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

    private Dictionary<Hex, HexTile> placedHexTileDict = null;
    public Dictionary<Hex, HexTile> PlacedHexTileDict => placedHexTileDict;

    private Dictionary<Hex, HexTile> placeableHexTileDict = null;
    public Dictionary<Hex, HexTile> PlaceableHexTileDict => placeableHexTileDict;

    public HexSettings HexSettings => hexSettings;

    public float HexSize => hexSize;

    private void Awake()
    {
        Instance = this;
        //InitHexSettings();
        //SetHexes();
        //SpawnHexes();
    }

    private void Start()
    {
        //HexNodeManager.Instance.SetHexNodes();
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

                if (hexDict != null && hexDict.Count > 0 && placedHexTileDict != null && placedHexTileDict.Count > 1)
                {
                    foreach (Hex hex in hexDict.Values)
                    {
                        bool isActive = hex.axialCoord == coord;

                        if (isActive)
                            Debug.Log("Found! " + coord);

                        HexTile hexTile;

                        placedHexTileDict.TryGetValue(hex, out hexTile);

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
            if (hexDict != null && hexDict.Count > 0 && placedHexTileDict != null && placedHexTileDict.Count > 1)
            {
                foreach (Hex hex in hexDict.Values)
                {
                    HexTile hexTile;

                    placedHexTileDict.TryGetValue(hex, out hexTile);

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
        placedHexTileDict.TryGetValue(neighbourHex, out neighbourHexTile);

        return neighbourHexTile;
    }

    public Hex GetNeighbourHex(Hex hex, Directions direction) 
    {
        if (hexDict == null || hexDict.Count < 1)
            return null;

        if (placedHexTileDict == null || placedHexTileDict.Count < 1)
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

        if (placedHexTileDict == null || placedHexTileDict.Count < 1)
            return null;

        CubeCoordinate[] coords = HexUtils.GetAllCubeCoordinateNeighbours(parentHex.cubeCoord);
        HexTile[] hexTiles = new HexTile[coords.Length];

        for (int i = 0; i < coords.Length; i++)
        {
            if (!hexDict.ContainsKey(coords[i]))
                continue;

            Hex hex;
            hexDict.TryGetValue(coords[i], out hex);

            if (!placedHexTileDict.ContainsKey(hex))
                continue;

            placedHexTileDict.TryGetValue(hex, out hexTiles[i]);
        }

        return hexTiles;
    }

    public void InitHexSettings()
    {
        hexSettings = new HexSettings(hexSize, mapSize);
    }

    public void SetHexes()
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

        placedHexTileDict = new Dictionary<Hex, HexTile>();
        placeableHexTileDict = new Dictionary<Hex, HexTile>();
    }

    public void SpawnCenterHex()
    {
        if (hexSettings == null)
            return;

        if (hexDict == null || hexDict.Count < 1)
            return;

        if (placedHexTileDict == null)
            return;

        CubeCoordinate centerHexCoord = new CubeCoordinate(0, 0, 0);
        Hex centerHex;
        hexDict.TryGetValue(centerHexCoord, out centerHex);
        Vector3 centerHexPos = HexUtils.GetPositionFromAxialCoordinate(centerHex.axialCoord, hexSettings.width, hexSettings.height);
        HexTileModel centerHexTileModel = HexUtils.GetRandomLandscapeHexTileModel(centerHex);

        HexTile centerHexTile = Instantiate<HexTile>(hexTilePrefab);
        centerHexTile.InitTileAsPlaced(centerHexTileModel, centerHex, centerHexPos);
        placedHexTileDict.Add(centerHex, centerHexTile);

        CubeCoordinate[] neighbourCoordinates = HexUtils.GetAllCubeCoordinateNeighbours(centerHexCoord);
        Hex[] neighbourHexes = new Hex[neighbourCoordinates.Length];
        for (int i = 0; i < neighbourCoordinates.Length; i++)
            hexDict.TryGetValue(neighbourCoordinates[i], out neighbourHexes[i]);

        foreach (Hex neighbourHex in neighbourHexes)
        {
            if(!hexDict.ContainsValue(neighbourHex))
                continue;

            Vector3 pos = HexUtils.GetPositionFromAxialCoordinate(neighbourHex.axialCoord, hexSettings.width, hexSettings.height);
            HexTile hexTile = Instantiate<HexTile>(hexTilePrefab);
            hexTile.InitTileAsPlaceable(neighbourHex, pos);
            placeableHexTileDict.Add(neighbourHex, hexTile);
        }
    }

    public Hex GetHex(AxialCoordinate coord) 
    {
        CubeCoordinate cubeCoord = HexUtils.AxialToCubeCoordinate(coord);
        Hex hex = new Hex(int.MinValue, int.MinValue);

        if (hexDict != null && hexDict.Count > 0)
            hexDict.TryGetValue(cubeCoord, out hex);

        return hex;
    }

    private void DrawHexGizmos() 
    {
        if (placedHexTileDict == null || placedHexTileDict.Count < 1)
            return;

        int index = 0;

        foreach (HexTile tile in placedHexTileDict.Values)
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
}
