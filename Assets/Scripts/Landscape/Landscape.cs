using UnityEngine;

public abstract class Landscape : MonoBehaviour
{
    public MeshRenderer meshRenderer = null;

    public abstract LandscapeTypes LandScape { get; }

    protected Directions direction = Directions.None;
    public Directions Direction => direction;

    public virtual void InitLandscape(Directions direction) 
    {
        this.direction = direction;
    }
}
