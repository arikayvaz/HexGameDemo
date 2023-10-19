using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    public static TilePlacer Instance = null;

    private HexTileModel placeModel = null;

    [SerializeField] private HexTile previewTile = null;
    private HexTile lastPreviewPlaceableTile = null;

    private void Awake()
    {
        Instance = this;

        InputManager.OnHoveredHexChanged += OnHoveredHexChanged;
        InputManager.OnPlaceHexInputClicked += OnPlaceHexInputClicked;
        HexGridManager.OnHexPlaced += OnHexPlaced;
        InputManager.OnRotateInputClicked += OnRotateInputClicked;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        InputManager.OnHoveredHexChanged -= OnHoveredHexChanged;
        InputManager.OnPlaceHexInputClicked -= OnPlaceHexInputClicked;
        HexGridManager.OnHexPlaced -= OnHexPlaced;
        InputManager.OnRotateInputClicked -= OnRotateInputClicked;
    }

    public void SetNextPlaceableHexTileModel() 
    {
        placeModel = HexUtils.GetRandomLandscapeHexTileModel(null);

        previewTile.OnPreviewEnd();

        previewTile.InitTileAsPreview(placeModel);

        previewTile.gameObject.SetActive(false);
    }

    private void OnHoveredHexChanged(Hex hex) 
    {
        if (placeModel == null)
            return;

        HexTile pleaceableTile = HexGridManager.Instance.GetPlaceableHexTile(hex);

        if (pleaceableTile == null) 
        {
            if (lastPreviewPlaceableTile != null) 
            {
                lastPreviewPlaceableTile.gameObject.SetActive(true);
                lastPreviewPlaceableTile = null;
            }

            previewTile.gameObject.SetActive(false);
            return;
        }

        if (lastPreviewPlaceableTile != null)
            lastPreviewPlaceableTile.gameObject.SetActive(true);

        lastPreviewPlaceableTile = pleaceableTile;
        lastPreviewPlaceableTile.gameObject.SetActive(false);

        previewTile.transform.position = pleaceableTile.transform.position;
        previewTile.gameObject.SetActive(true);
    }

    private void OnPlaceHexInputClicked(Hex hex) 
    {
        if (placeModel == null)
            return;

        if (previewTile.IsRotating)
            return;

        HexTile pleaceableTile = HexGridManager.Instance.GetPlaceableHexTile(hex);

        if (pleaceableTile == null)
            return;

        if (lastPreviewPlaceableTile != null)
            lastPreviewPlaceableTile.gameObject.SetActive(true);

        lastPreviewPlaceableTile = null;

        previewTile.gameObject.SetActive(false);
        pleaceableTile.SetVisualRotation(previewTile.VisualRotation);

        HexGridManager.Instance.PlaceHexTile(pleaceableTile, placeModel);
    }

    private void OnHexPlaced(HexTile tile) 
    {
        SetNextPlaceableHexTileModel();

        previewTile.ResetPreviewTile();
        previewTile.InitTileAsPreview(placeModel);
    }

    private void OnRotateInputClicked(RotationDirections rotation) 
    {
        if (placeModel == null)
            return;

        if (previewTile.IsRotating)
            return;

        previewTile.RotateVisual(rotation, () => 
        {
            placeModel.RotateLandscapes(previewTile.transform.position, rotation);
            previewTile.OnModelUpdated();
        });
    }
}
