using UnityEngine;
using System.Collections.Generic;

public class CellularAutomataGenerator : MonoBehaviour
{
    [Header("Map Dimensions")]
    public int width = 200; // Increased for larger maps
    public int height = 120; // Increased for larger maps

    [Header("Generation Parameters")]
    [Range(0, 100)]
    public int fillPercent = 45; // chance to start as Wall
    [Range(0, 8)]
    public int birthLimit = 4; // neighbors needed to become Wall
    [Range(0, 8)]
    public int deathLimit = 3; // neighbors needed to become Floor
    [Range(1, 10)]
    public int steps = 5; // number of CA iterations
    [Range(10, 1000)]
    public int minCaveSize = 50; // connectivity threshold

    [Header("Randomization")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private System.Random random;

    public MapData GenerateMap()
    {
        // Initialize random
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999);
        }
        random = new System.Random(seed);

        // Step 1: Initialize grid with random wall/floor
        MapData map = new MapData(width, height);
        InitializeRandomGrid(map);

        // Step 2: Perform CA iterations
        for (int i = 0; i < steps; i++)
        {
            map = DoCellularAutomataStep(map);
        }

        // Step 3: Ensure outer border is Wall
        SetBorderWalls(map);

        // Step 4: Flood-fill and keep largest region
        EnsureConnectivity(map);

        // Step 5: Optionally carve corridors between regions
        CarveCorridors(map);

        Debug.Log($"[CellularAutomataGenerator] Generated map with seed {seed}");
        return map;
    }

    private void InitializeRandomGrid(MapData map)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile tile = random.Next(100) < fillPercent ? Tile.Wall : Tile.Floor;
                map.Set(x, y, tile);
            }
        }
    }

    private MapData DoCellularAutomataStep(MapData oldMap)
    {
        MapData newMap = new MapData(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int wallNeighbors = CountWallNeighbors(oldMap, x, y);
                Tile currentTile = oldMap.Get(x, y);

                if (currentTile == Tile.Wall)
                {
                    // Wall rules
                    if (wallNeighbors < deathLimit)
                        newMap.Set(x, y, Tile.Floor);
                    else
                        newMap.Set(x, y, Tile.Wall);
                }
                else
                {
                    // Floor rules
                    if (wallNeighbors > birthLimit)
                        newMap.Set(x, y, Tile.Wall);
                    else
                        newMap.Set(x, y, Tile.Floor);
                }
            }
        }

        return newMap;
    }

    private int CountWallNeighbors(MapData map, int x, int y)
    {
        int wallCount = 0;

        // Check 8-neighborhood
        for (int ny = y - 1; ny <= y + 1; ny++)
        {
            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                if (nx == x && ny == y) continue; // Skip center cell

                if (map.Get(nx, ny) == Tile.Wall)
                    wallCount++;
            }
        }

        return wallCount;
    }

    private void SetBorderWalls(MapData map)
    {
        // Set top and bottom borders
        for (int x = 0; x < width; x++)
        {
            map.Set(x, 0, Tile.Wall);
            map.Set(x, height - 1, Tile.Wall);
        }

        // Set left and right borders
        for (int y = 0; y < height; y++)
        {
            map.Set(0, y, Tile.Wall);
            map.Set(width - 1, y, Tile.Wall);
        }
    }

    private void EnsureConnectivity(MapData map)
    {
        bool[] visited = new bool[width * height];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        // Find all floor regions using flood-fill
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map.Get(x, y) == Tile.Floor && !visited[map.GetIndex(x, y)])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    FloodFill(map, x, y, visited, region);
                    regions.Add(region);
                }
            }
        }

        // Find the largest region
        List<Vector2Int> largestRegion = null;
        int maxSize = 0;

        foreach (var region in regions)
        {
            if (region.Count > maxSize)
            {
                maxSize = region.Count;
                largestRegion = region;
            }
        }

        // Convert all smaller regions to walls
        foreach (var region in regions)
        {
            if (region != largestRegion && region.Count < minCaveSize)
            {
                foreach (var pos in region)
                {
                    map.Set(pos.x, pos.y, Tile.Wall);
                }
            }
        }

        Debug.Log($"[CellularAutomataGenerator] Found {regions.Count} regions, largest has {maxSize} tiles");
    }

    private void FloodFill(MapData map, int startX, int startY, bool[] visited, List<Vector2Int> region)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[map.GetIndex(startX, startY)] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);

            // Check 4-neighborhood (no diagonals for flood-fill)
            Vector2Int[] neighbors = {
                new Vector2Int(current.x + 1, current.y),
                new Vector2Int(current.x - 1, current.y),
                new Vector2Int(current.x, current.y + 1),
                new Vector2Int(current.x, current.y - 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (map.IsValidPosition(neighbor.x, neighbor.y) && 
                    map.Get(neighbor.x, neighbor.y) == Tile.Floor && 
                    !visited[map.GetIndex(neighbor.x, neighbor.y)])
                {
                    visited[map.GetIndex(neighbor.x, neighbor.y)] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private void CarveCorridors(MapData map)
    {
        // Simple corridor carving - find isolated floor regions and connect them
        // This is a basic implementation that can be enhanced later
        bool[] visited = new bool[width * height];
        List<List<Vector2Int>> isolatedRegions = new List<List<Vector2Int>>();

        // Find isolated floor regions
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map.Get(x, y) == Tile.Floor && !visited[map.GetIndex(x, y)])
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    FloodFill(map, x, y, visited, region);
                    
                    if (region.Count >= minCaveSize)
                    {
                        isolatedRegions.Add(region);
                    }
                }
            }
        }

        // Connect regions with simple corridors
        if (isolatedRegions.Count > 1)
        {
            for (int i = 0; i < isolatedRegions.Count - 1; i++)
            {
                Vector2Int start = isolatedRegions[i][0];
                Vector2Int end = isolatedRegions[i + 1][0];
                CarveCorridor(map, start, end);
            }
        }
    }

    private void CarveCorridor(MapData map, Vector2Int start, Vector2Int end)
    {
        // Simple L-shaped corridor
        Vector2Int current = start;

        // Move horizontally first
        while (current.x != end.x)
        {
            current.x += (end.x > current.x) ? 1 : -1;
            if (map.IsValidPosition(current.x, current.y))
                map.Set(current.x, current.y, Tile.Floor);
        }

        // Then move vertically
        while (current.y != end.y)
        {
            current.y += (end.y > current.y) ? 1 : -1;
            if (map.IsValidPosition(current.x, current.y))
                map.Set(current.x, current.y, Tile.Floor);
        }
    }

    // Context menu for testing
    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999);
        }
        MapData map = GenerateMap();
        Debug.Log($"[CellularAutomataGenerator] Generated new map with seed {seed}");
    }

    [ContextMenu("Generate Map with Current Seed")]
    public void GenerateMapWithCurrentSeed()
    {
        MapData map = GenerateMap();
        Debug.Log($"[CellularAutomataGenerator] Generated map with seed {seed}");
    }
}
