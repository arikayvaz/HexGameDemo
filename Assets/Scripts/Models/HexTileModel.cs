using UnityEngine;

public class HexTileModel
{
    public Hex hex;
    public LandscapeModel[] landscapes;
    public SubTileModel subTile;

    public HexTileModel(Hex hex, LandscapeModel[] landscapes, SubTileModel subTile)
    {
        this.hex = hex;
        this.landscapes = landscapes;
        this.subTile = subTile;
    }

    public void SetHex(Hex hex) 
    {
        this.hex = hex;
    }

    public void RotateLandscapes(Vector3 centerPos, RotationDirections rotation) 
    {
        if (landscapes == null || landscapes.Length < 1)
            return;

        foreach (LandscapeModel landscape in landscapes)
        {
            landscape.Rotate(rotation);

            Vector3 position = HexUtils.GetLandscapePosition(centerPos, landscape.direction, HexGridManager.Instance.HexSettings.height);
            landscape.UpdatePosition(position);
        }
    }

    public void OnModelUpdated() 
    {
        UpdateLandscapeModels();
    }

    private void UpdateLandscapeModels() 
    {
        if (landscapes == null || landscapes.Length < 1)
            return;

        foreach (LandscapeModel landscape in landscapes)
        {
            landscape.OnModelUpdated();
        }
    }
}
