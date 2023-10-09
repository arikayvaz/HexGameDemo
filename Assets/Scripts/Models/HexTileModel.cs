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
}
