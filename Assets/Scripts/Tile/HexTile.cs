using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class HexTile : MonoBehaviour
{

    private HexTileModel model;

    public void InitTile(HexTileModel model) 
    {
        this.model = model;
    }

    public void SpawnLandscapes() 
    {
        if (model == null || model.landscapes == null || model.landscapes.Length < 1)
            return;

        foreach (LandscapeModel model in model.landscapes)
        {
            GameObject goLandscape = HexGridManager.Instance.GetLandscapeGameObject(model.landscape, transform);

            goLandscape.name = $"LS_{model.direction}";
            goLandscape.transform.position = HexUtils.GetLandscapePosition(transform.position, model.direction, HexGridManager.Instance.HexSettings.height);
            goLandscape.transform.rotation = HexUtils.GetLandscapeRotation(model.direction);

            goLandscape.SetActive(model.landscape != Landscapes.Empty);
        }
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
