using UnityEngine;

public class LandscapeModel
{
    public LandscapeTypes landscapeType;
    public Directions direction;
    public Vector3 Position { get; private set; }
    public GameObject goLandscape;

    public NodeGroup group = null;
    public bool HasGroup => group != null;

    public LandscapeModel(LandscapeTypes landscapeType, Directions direction)
    {
        this.landscapeType = landscapeType;
        this.direction = direction;
    }

    public void Rotate(RotationDirections rotation) 
    {
        direction = HexUtils.RotateDirection(direction, rotation);
    }

    public void UpdatePosition(Vector3 newPosition) 
    {
        Position = newPosition;
        UpdateGoLandscapePositionAndRotation();
    }

    public void OnModelUpdated() 
    {
        UpdateGoLandscapePositionAndRotation();
    }

    private void UpdateGoLandscapePositionAndRotation() 
    {
        if (goLandscape == null)
            return;

        goLandscape.name = $"LS_{direction}";
        goLandscape.transform.position = Position;
        goLandscape.transform.rotation = HexUtils.GetLandscapeRotation(direction);
    }
}
