public struct AxialCoordinate
{
    public int q;//Column
    public int r;//Row

    public AxialCoordinate(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public override string ToString()
    {
        return $"q:{q} r:{r}";
    }

    public static bool operator ==(AxialCoordinate a, AxialCoordinate b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(AxialCoordinate a, AxialCoordinate b)
    {
        return a.q == b.q || a.r == b.r;
    }

    public override bool Equals(object obj)
    {
        return (obj is AxialCoordinate) && this == (AxialCoordinate)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public AxialCoordinate Add(AxialCoordinate axial) 
    {
        return new AxialCoordinate(q + axial.q, r + axial.r);
    }
}
