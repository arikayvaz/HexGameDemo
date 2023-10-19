using System.Collections;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System;

public class HexTile : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer = null;
    [SerializeField] Material matPlaced = null;
    [SerializeField] Material matPlaceable = null;
    [SerializeField] GameObject goVisualRoot = null;
    [SerializeField] GameObject goLandscapeRoot = null;

    private HexTileModel model;
    public Hex hex;

    public enum PlaceStates { None, Placeable, Placed, Preview }
    public PlaceStates PlaceState { get; private set; } = PlaceStates.None;

    public bool IsPlaceable => PlaceState == PlaceStates.Placeable;
    public bool IsPlaced => PlaceState == PlaceStates.Placed;

    private GameObject[] goLandscapes = new GameObject[6];

    public Quaternion VisualRotation => goVisualRoot.transform.rotation;

    public void InitTileAsPlaced(HexTileModel model, Hex hex, Vector3 position) 
    {
        this.model = model;
        this.hex = hex;
        transform.position = position;
        ChangePlaceState(PlaceStates.Placed);
    }

    public void InitTileAsPlaceable(Hex hex, Vector3 position) 
    {
        this.hex = hex;
        model = null;
        transform.position = position;
        ChangePlaceState(PlaceStates.Placeable);
    }

    public void InitTileAsPreview(HexTileModel model) 
    {
        this.model = model;
        ChangePlaceState(PlaceStates.Preview);
    }

    public void OnPreviewEnd() 
    {
        ChangePlaceState(PlaceStates.None);
    }

    public void OnTilePlaced(HexTileModel model) 
    {
        this.model = model;
        ChangePlaceState(PlaceStates.Placed);
    }

    #region Place State Machine

    private void ChangePlaceState(PlaceStates stateNew) 
    {
        switch (PlaceState)
        {
            case PlaceStates.Preview:
                ExitStatePreview();
                break;
        }

        switch (stateNew)
        {
            case PlaceStates.None:
                EnterStateNone();
                break;
            case PlaceStates.Placeable:
                EnterStatePlaceable();
                break;
            case PlaceStates.Placed:
                EnterStatePlaced();
                break;
            case PlaceStates.Preview:
                EnterStatePreview();
                break;
        }

        PlaceState = stateNew;
    }

    #region States

    #region State None

    private void EnterStateNone() 
    {
        ResetLandscapeRootRotation();
        SetVisualRotation(Quaternion.identity);
    }

    #endregion

    #region State Placeable

    private void EnterStatePlaceable()
    {
        SetPlaceableHexVisual();
    }

    #endregion

    #region State Placed

    private void EnterStatePlaced() 
    {
        ResetLandscapeRootRotation();
        SetPlacedHexVisual();
        SpawnLandscapes();
    }

    #endregion

    #region State Preview

    private void EnterStatePreview() 
    {
        SpawnLandscapes();
    }

    private void ExitStatePreview() 
    {
        DeleteLandscapes();
    }

    #endregion

    #endregion

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (model == null)
            return;

        if (hex == null)
            return;

        if (HexGridManager.Instance == null)
            return;

        //DrawNeighbourGizmos();
        //DrawNeighbourLandscapeGizmos();
        DrawNodeGizmos();
    }

    public LandscapeModel[] Lanscapes => model.landscapes;

    public void SpawnLandscapes() 
    {
        if (model == null || model.landscapes == null || model.landscapes.Length < 1)
            return;

        for (int i = 0; i < model.landscapes.Length; i++)
        {
            LandscapeModel landscape = model.landscapes[i];

            GameObject goLandscape = HexGridManager.Instance.GetLandscapeGameObject(landscape.landscapeType, goLandscapeRoot.transform);

            Vector3 position = HexUtils.GetLandscapePosition(transform.position, landscape.direction, HexGridManager.Instance.HexSettings.height);

            goLandscape.name = $"LS_{landscape.direction}";
            goLandscape.transform.position = HexUtils.GetLandscapePosition(transform.position, landscape.direction, HexGridManager.Instance.HexSettings.height);
            goLandscape.transform.rotation = HexUtils.GetLandscapeRotation(landscape.direction);

            goLandscape.SetActive(landscape.landscapeType != LandscapeTypes.Empty);

            //landscape.Position = goLandscape.transform.position;

            landscape.goLandscape = goLandscape;
            landscape.UpdatePosition(position);

            goLandscapes[i] = goLandscape;
        }
    }

    public void DeleteLandscapes() 
    {
        if (goLandscapes == null || goLandscapes.Length < 1)
            return;

        foreach (GameObject goLandscape in goLandscapes)
        {
            Destroy(goLandscape);
        }
    }

    public void ResetPreviewTile() 
    {
        ChangePlaceState(PlaceStates.None);
    }

    public LandscapeModel GetNeighbourLandscape(Directions direction) 
    {
        if (model == null || (model.landscapes == null || model.landscapes.Length < 1))
            return null;

        Directions neighbourDirection = HexUtils.GetNeighbourDirection(direction);

        for (int i = 0; i < model.landscapes.Length; i++)
        {
            if (model.landscapes[i].direction != neighbourDirection)
                continue;

            return model.landscapes[i];
        }

        return null;
    }

    public LandscapeModel GetSideLandscape(Directions direction) 
    {
        if (direction == Directions.None)
            return null;

        if (model == null || (model.landscapes == null || model.landscapes.Length < 1))
            return null;

        Directions sideDirection = HexUtils.GetSideDirection(direction);

        for (int i = 0; i < model.landscapes.Length; i++)
        {
            if (model.landscapes[i].direction != sideDirection)
                continue;

            return model.landscapes[i];
        }

        return null;
    }

    public bool HasAnyLandscape(LandscapeTypes type) 
    {
        if (model == null || (model.landscapes == null || model.landscapes.Length < 1))
            return false;

        foreach (LandscapeModel landscape in model.landscapes)
        {
            if (landscape.landscapeType == type)
                return true;
        }

        return false;
    }

    private void SetPlaceableHexVisual() 
    {
        UpdateHexVisual(matPlaceable);
    }

    private void SetPlacedHexVisual() 
    {
        UpdateHexVisual(matPlaced);
    }

    private void UpdateHexVisual(Material mat) 
    {
        meshRenderer.materials = new Material[] { mat, mat };
    }

    public bool IsRotating { get; private set; } = false;

    public void RotateVisual(RotationDirections rotation, Action onComplete) 
    {
        if (IsRotating)
            return;

        IsRotating = true;

        const float ROTATION_DELTA = 60f;
        const float ROTATION_ANIM_DURATION = 0.5f;

        Vector3 rotEuler = goVisualRoot.transform.rotation.eulerAngles;
        rotEuler.y += (int)rotation * ROTATION_DELTA;

        goLandscapeRoot.transform.DORotate(rotEuler, ROTATION_ANIM_DURATION);
        goVisualRoot.transform.DORotate(rotEuler, ROTATION_ANIM_DURATION).OnComplete(() => 
        {
            IsRotating = false;
            onComplete?.Invoke();
        });
    }

    public void SetVisualRotation(Quaternion rotation) 
    {
        goVisualRoot.transform.rotation = rotation;
    }

    public void OnModelUpdated() 
    {
        model.OnModelUpdated();
    }

    public void ResetLandscapeRootRotation() 
    {
        goLandscapeRoot.transform.rotation = Quaternion.identity;
    }

    #region Gizmos

    private void DrawCornerGizmos() 
    {
        for (int j = 0; j < 6; j++)
        {
            float angle_deg = 60 * j;
            float angle_rad = Mathf.PI / 180f * angle_deg;

            Vector3 pointPos = Vector3.zero;

            pointPos.x = transform.position.x + 1f * Mathf.Cos(angle_rad);
            pointPos.y = 0.25f;
            pointPos.z = transform.position.z + 1f * Mathf.Sin(angle_rad);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointPos, 0.1f);
        }
    }

    private void DrawEdgeGizmos() 
    {
        for (int j = 0; j < 6; j++)
        {
            float angle_deg = 60f - (60f * j);
            float angle_rad = Mathf.PI / 180f * (angle_deg + 30f);

            Vector3 pointPos = Vector3.zero;

            float size = 1f;
            float halfHeight = (size * Mathf.Sqrt(3f)) * 0.5f;

            pointPos.x = transform.position.x + halfHeight * Mathf.Cos(angle_rad);
            pointPos.y = 0.25f;
            pointPos.z = transform.position.z + halfHeight * Mathf.Sin(angle_rad);

            Gizmos.color = Color.blue;
            //Gizmos.DrawWireSphere(pointPos, 0.1f);

            Handles.Label(pointPos, $"{j}_{angle_deg}");
        }
    }

    private void DrawNeighbourGizmos()
    {
        Vector3 centerPos = transform.position;
        centerPos.y += 0.2f;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(centerPos, 0.1f);

        HexTile[] neighbours = HexGridManager.Instance.GetAllNeighbourHexTiles(hex);

        if (neighbours == null || neighbours.Length < 1)
            return;

        for (int i = 0; i < neighbours.Length; i++)
        {
            HexTile neighbour = neighbours[i];

            if (neighbour == null)
                continue;

            Vector3 pos = neighbour.transform.position;
            pos.y += 0.2f;

            Gizmos.color = HexUtils.GizmosColors[i];
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }

    private void DrawNeighbourLandscapeGizmos()
    {
        for (int i = 0; i < model.landscapes.Length; i++)
        {
            LandscapeModel current = model.landscapes[i];
            LandscapeModel neighbour = HexGridManager.Instance.GetNeighbourHexLandscape(hex, current.direction);

            Gizmos.color = HexUtils.GizmosColors[i];

            Vector3 cp = current.Position;
            cp.y += 0.2f;

            Gizmos.DrawSphere(cp, 0.1f);

            if (neighbour == null)
                continue;

            Vector3 np = neighbour.Position;
            np.y += 0.2f;

            Gizmos.DrawSphere(np, 0.1f);
        }
    }

    private void DrawNodeGizmos() 
    {
        for (int i = 0; i < model.landscapes.Length; i++)
        {
            LandscapeModel landscape = model.landscapes[i];

            if (!landscape.HasGroup)
                continue;

            Gizmos.color = HexUtils.GizmosColors[i];
            for (int j = 0; j < landscape.group.nodes.Count; j++)
            {
                LandscapeModel node = landscape.group.nodes[j];

                Vector3 pos = node.Position;
                pos.y += 0.2f + (i * 0.1f);

                Gizmos.DrawSphere(pos, 0.1f);

            }//for (int j = 0; j < landscape.group.nodes.Count; j++)


        }//for (int i = 0; i < model.landscapes.Length; i++)
    }

    #endregion

}
