using UnityEngine;

public class MapData
{
    public int width;
    public int height;
    public Tile[] tiles;

    public MapData(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.tiles = new Tile[width * height];
    }

    public Tile Get(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return Tile.Wall; // Return wall for out-of-bounds
        
        return tiles[y * width + x];
    }

    public void Set(int x, int y, Tile tile)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return; // Ignore out-of-bounds
        
        tiles[y * width + x] = tile;
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public int GetIndex(int x, int y)
    {
        return y * width + x;
    }

    public Vector2Int GetPosition(int index)
    {
        int y = index / width;
        int x = index % width;
        return new Vector2Int(x, y);
    }
}
