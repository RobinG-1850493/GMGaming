using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GooseLib.Graphics;

public class Tilemap
{
    private readonly Tileset _tileset;
    private readonly int[] _tiles;

    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public int Count { get; private set; }
    public Vector2 Scale { get; set; }
    public float TileWidth => _tileset.TileWidth * Scale.X;
    public float TileHeight => _tileset.TileHeight * Scale.Y;

    public Tilemap(Tileset tileset, int columns, int rows)
    {
        _tileset = tileset;
        Columns = columns;
        Rows = rows;
        Count = Columns * Rows;
        _tiles = new int[Count];
        Scale = Vector2.One;
    }

    public void SetTile(int index, int tilesetId)
    {
        _tiles[index] = tilesetId;
    }

    public void SetTile(int column, int row, int tilesetId)
    {
        int index = row * Columns + column;
        SetTile(index, tilesetId);
    }

    public TextureRegion GetTile(int index)
    {
        return _tileset.getTile(_tiles[index]);
    }


    public TextureRegion GetTile(int column, int row)
    {
        int index = row * Columns + column;
        return GetTile(index);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < Count; i++)
        {
            int tileSetIndex = _tiles[i];
            TextureRegion tilme = _tileset.getTile(tileSetIndex);

            int x = i % Columns;
            int y = i / Columns;

            Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
            tilme.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
        }
    }

    public static Tilemap FromFile(ContentManager content, string filename)
    {
        string filePath = Path.Combine(content.RootDirectory, filename);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                XElement tilesetElement = root.Element("Tileset");

                string regionAtribute = tilesetElement.Attribute("region").Value;
                string[] split = regionAtribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int x = int.Parse(split[0]);
                int y = int.Parse(split[1]);
                int width = int.Parse(split[2]);
                int height = int.Parse(split[3]);

                int tileWidth = int.Parse(tilesetElement.Attribute("tilewidth").Value);
                int tileHeight = int.Parse(tilesetElement.Attribute("tileheight").Value);
                string contentPath = tilesetElement.Value;

                Texture2D texture = content.Load<Texture2D>(contentPath);

                TextureRegion region = new TextureRegion(texture, x, y, width, height);
                Tileset tileset = new Tileset(region, tileWidth, tileHeight);

                XElement tilesElement = root.Element("Tiles");
                string[] rows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                int columnCount = rows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

                Tilemap tilemap = new Tilemap(tileset, columnCount, rows.Length);

                for (int row = 0; row < rows.Length; row++)
                {
                    string[] cols = rows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    for (int col = 0; col < cols.Length; col++)
                    {
                        int tileIndex = int.Parse(cols[col]);
                        TextureRegion tRegion = tileset.getTile(tileIndex);
                        tilemap.SetTile(col, row, tileIndex);
                    }
                }
                return tilemap;
            }
        }
    }
}