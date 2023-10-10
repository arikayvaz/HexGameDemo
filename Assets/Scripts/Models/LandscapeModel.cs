using UnityEngine;

public class LandscapeModel
{
    public LandscapeTypes landscapeType;
    public Directions direction;
    public Vector3 position;

    public NodeGroup group = null;
    public bool HasGroup => group != null;

    public LandscapeModel(LandscapeTypes landscapeType, Directions direction)
    {
        this.landscapeType = landscapeType;
        this.direction = direction;
    }
}
