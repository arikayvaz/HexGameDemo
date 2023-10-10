using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexTile : MonoBehaviour
{

    private HexTileModel model;
    public Hex hex;

    public void InitTile(HexTileModel model, Hex hex) 
    {
        this.model = model;
        this.hex = hex;
    }

    public LandscapeModel[] Lanscapes => model.landscapes;

    public void SpawnLandscapes() 
    {
        if (model == null || model.landscapes == null || model.landscapes.Length < 1)
            return;

        foreach (LandscapeModel model in model.landscapes)
        {
            GameObject goLandscape = HexGridManager.Instance.GetLandscapeGameObject(model.landscapeType, transform);

            goLandscape.name = $"LS_{model.direction}";
            goLandscape.transform.position = HexUtils.GetLandscapePosition(transform.position, model.direction, HexGridManager.Instance.HexSettings.height);
            goLandscape.transform.rotation = HexUtils.GetLandscapeRotation(model.direction);

            goLandscape.SetActive(model.landscapeType != LandscapeTypes.Empty);

            model.position = goLandscape.transform.position;
        }
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

    private void OnDrawGizmosSelected()
    {
        //DrawCorners();
        //DrawEdges();
    }

    private void DrawCorners() 
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

    private void DrawEdges() 
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

}
