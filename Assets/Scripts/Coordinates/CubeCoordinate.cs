public struct CubeCoordinate
{
    public int q;//Column
    public int r;//Row
    public int s; // q + r + s = 0

    public CubeCoordinate(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public override string ToString()
    {
        return $"q:{q} r:{r} s{s}";
    }

    public static bool operator ==(CubeCoordinate a, CubeCoordinate b)
    {
        return a.q == b.q && a.r == b.r && a.s == b.s;
    }

    public static bool operator !=(CubeCoordinate a, CubeCoordinate b)
    {
        return a.q == b.q || a.r == b.r || a.s == b.s;
    }

    public override bool Equals(object obj)
    {
        return (obj is CubeCoordinate) && this == (CubeCoordinate)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public CubeCoordinate Add(CubeCoordinate cube) 
    {
        return new CubeCoordinate(q + cube.q, r + cube.r, s + cube.s);
    }
}
