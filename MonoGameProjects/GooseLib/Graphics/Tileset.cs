namespace GooseLib.Graphics;

public class Tileset
{
    private readonly TextureRegion[] _tiles;
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public int Columns { get; private set; }
    public int Rows { get; private set; }
    public int TileCount { get; private set; }

    public Tileset(TextureRegion textureRegion, int tileWidth, int tileHeight)
    {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Columns = textureRegion.Width / tileWidth;
        Rows = textureRegion.Height / tileHeight;
        TileCount = Columns * Rows;

        _tiles = new TextureRegion[TileCount];

        for (int i = 0; i < TileCount; i++)
        {
            int x = (i % Columns) * tileWidth;
            int y = (i / Columns) * tileHeight;
            _tiles[i] = new TextureRegion(textureRegion.Texture, textureRegion.SourceRectangle.X + x, textureRegion.SourceRectangle.Y + y, tileWidth, tileHeight);
        }
    }

    public TextureRegion getTile(int index) => _tiles[index];
    public TextureRegion getTile(int column, int row)
    {
        int index = row * Columns + column;
        return getTile(index);
    }
}