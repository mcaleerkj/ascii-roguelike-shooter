/*
 * MapRenderer - Renders map data to an ASCII grid
 * 
 * SCENE WIRING FOR BIG MAPS:
 * - Leave MapRenderer.AsciiGrid unassigned
 * - Assign MapRenderer.viewportRenderer to the viewport object
 * - Ensure MapController calls viewportRenderer.Init(map) after generation
 * - Ensure CameraFollowBounds.map is set to the same MapData
 * 
 * The renderer will automatically delegate to AsciiViewportRenderer when available,
 * otherwise fall back to the legacy AsciiGrid rendering system.
 */

using UnityEngine;
using TMPro;

public class MapRenderer : MonoBehaviour
{
    [Header("Map Generation")]
    public CellularAutomataGenerator generator;
    
    [Header("Legacy Rendering")]
    public AsciiGrid asciiGrid;
    
    [Header("Viewport Rendering")]
    public AsciiViewportRenderer viewportRenderer;
    public bool forceLegacy = false;
    
    [Header("Rendering")]
    public GameRenderConfig renderConfig;
    public char wallGlyph = '#';
    public char floorGlyph = '.';
    public Color32 wallColor = new Color32(64, 64, 64, 255); // Dark gray
    public Color32 floorColor = new Color32(128, 128, 128, 255); // Mid gray
    public Color32 wallBgColor = new Color32(32, 32, 32, 255); // Darker gray
    public Color32 floorBgColor = Color.black;
    
    private MapData currentMap;
    
    public MapData CurrentMap => currentMap;

    void Start()
    {
        if (asciiGrid == null)
        {
            asciiGrid = FindObjectOfType<AsciiGrid>();
        }
        
        if (generator == null)
        {
            generator = FindObjectOfType<CellularAutomataGenerator>();
        }

        // Find or create viewport renderer
        if (viewportRenderer == null)
        {
            viewportRenderer = FindObjectOfType<AsciiViewportRenderer>();
            if (viewportRenderer == null)
            {
                Debug.LogWarning("[MapRenderer] No AsciiViewportRenderer assigned! Please assign one in the inspector for viewport rendering, or use forceLegacy=true for legacy rendering.");
            }
            else
            {
                Debug.Log("[MapRenderer] Found existing AsciiViewportRenderer");
            }
        }

        // Find render config if not assigned
        if (renderConfig == null)
        {
            renderConfig = FindObjectOfType<GameRenderConfig>();
            if (renderConfig == null)
            {
                Debug.LogWarning("[MapRenderer] No GameRenderConfig found! Please create one.");
            }
            else
            {
                Debug.Log($"[MapRenderer] Found GameRenderConfig: {renderConfig.referenceWidth}x{renderConfig.referenceHeight} @ {renderConfig.pixelsPerCell}px/cell");
            }
        }

        if (generator != null)
        {
            RenderMap();
        }
        else
        {
            Debug.LogError("[MapRenderer] Missing CellularAutomataGenerator reference!");
        }
    }
    
    void Update()
    {
        // Press 'R' to regenerate and restart the level
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void RenderMap()
    {
        if (generator == null) return;
        
        // Generate the map
        currentMap = generator.GenerateMap();
        Debug.Log($"[MapRenderer] Generated map: {currentMap.width}x{currentMap.height}");
        
        // Use viewport renderer if available and not forcing legacy
        if (viewportRenderer != null && !forceLegacy)
        {
            Debug.Log($"[MapRenderer] Delegated to viewport: visible={viewportRenderer.VisibleSize}, map={currentMap.width}x{currentMap.height}");
            viewportRenderer.Init(currentMap);
            return;
        }
        
        // Fallback to legacy rendering system
        if (asciiGrid != null)
        {
            Debug.Log("[MapRenderer] Using legacy AsciiGrid rendering");
            ClearGrid();
            RenderMapToGrid();
        }
        else
        {
            Debug.LogWarning("[MapRenderer] No AsciiGrid available for fallback rendering!");
        }
        
        // Disable any AsciiGridSmokeTest that might be interfering
        DisableSmokeTest();
    }

    private void ClearGrid()
    {
        if (asciiGrid == null) return;

        // Clear all cells to empty
        for (int y = 0; y < asciiGrid.Height; y++)
        {
            for (int x = 0; x < asciiGrid.Width; x++)
            {
                asciiGrid.SetCell(x, y, AsciiCell.Create(' ', Color.white, Color.black));
            }
        }
    }

    private void RenderMapToGrid()
    {
        if (currentMap == null || asciiGrid == null) return;

        // Calculate grid offset to center the map
        int gridOffsetX = (asciiGrid.Width - currentMap.width) / 2;
        int gridOffsetY = (asciiGrid.Height - currentMap.height) / 2;

        // Render each tile
        for (int y = 0; y < currentMap.height; y++)
        {
            for (int x = 0; x < currentMap.width; x++)
            {
                Tile tile = currentMap.Get(x, y);
                int gridX = gridOffsetX + x;
                int gridY = gridOffsetY + y;

                // Check if this position is within the ASCII grid bounds
                if (gridX >= 0 && gridX < asciiGrid.Width && gridY >= 0 && gridY < asciiGrid.Height)
                {
                    char glyph;
                    Color32 fgColor, bgColor;

                    if (tile == Tile.Wall)
                    {
                        glyph = wallGlyph;
                        fgColor = wallColor;
                        bgColor = wallBgColor;
                    }
                    else
                    {
                        glyph = floorGlyph;
                        fgColor = floorColor;
                        bgColor = floorBgColor;
                    }

                    asciiGrid.SetCell(gridX, gridY, AsciiCell.Create(glyph, fgColor, bgColor));
                }
            }
        }
    }

    public Vector2Int GetRandomFloorPosition()
    {
        if (currentMap == null) return Vector2Int.zero;

        // Find all floor positions
        System.Collections.Generic.List<Vector2Int> floorPositions = new System.Collections.Generic.List<Vector2Int>();
        
        for (int y = 0; y < currentMap.height; y++)
        {
            for (int x = 0; x < currentMap.width; x++)
            {
                if (currentMap.Get(x, y) == Tile.Floor)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (floorPositions.Count == 0)
        {
            Debug.LogWarning("[MapRenderer] No floor positions found!");
            return new Vector2Int(currentMap.width / 2, currentMap.height / 2);
        }

        // Return a random floor position
        Vector2Int randomPos = floorPositions[Random.Range(0, floorPositions.Count)];
        
        // Convert to world position (assuming grid is centered)
        if (asciiGrid != null)
        {
            int gridOffsetX = (asciiGrid.Width - currentMap.width) / 2;
            int gridOffsetY = (asciiGrid.Height - currentMap.height) / 2;
            
            randomPos.x += gridOffsetX;
            randomPos.y += gridOffsetY;
        }

        return randomPos;
    }

    public Vector3 GetWorldPositionFromGridPosition(Vector2Int gridPos)
    {
        if (asciiGrid == null) return Vector3.zero;

        // Convert grid position to world position
        float worldX = (gridPos.x + 0.5f) * asciiGrid.CellSize;
        float worldY = (gridPos.y + 0.5f) * asciiGrid.CellSize;
        
        return new Vector3(worldX, worldY, 0f);
    }

    public bool IsFloorAt(Vector2Int gridPos)
    {
        if (currentMap == null) return false;

        // Convert from ASCII grid coordinates to map coordinates
        if (asciiGrid != null)
        {
            int gridOffsetX = (asciiGrid.Width - currentMap.width) / 2;
            int gridOffsetY = (asciiGrid.Height - currentMap.height) / 2;
            
            gridPos.x -= gridOffsetX;
            gridPos.y -= gridOffsetY;
        }

        if (gridPos.x < 0 || gridPos.x >= currentMap.width || 
            gridPos.y < 0 || gridPos.y >= currentMap.height)
            return false;

        return currentMap.Get(gridPos.x, gridPos.y) == Tile.Floor;
    }

    // Context menu for testing
    [ContextMenu("Render New Map")]
    public void RenderNewMap()
    {
        RenderMap();
    }

    [ContextMenu("Get Random Floor Position")]
    public void LogRandomFloorPosition()
    {
        Vector2Int pos = GetRandomFloorPosition();
        Debug.Log($"[MapRenderer] Random floor position: {pos}");
    }
    
    [ContextMenu("Disable Smoke Tests")]
    public void DisableSmokeTestsManually()
    {
        DisableSmokeTest();
    }
    
    public void RestartLevel()
    {
        Debug.Log("[MapRenderer] Restarting level...");
        
        // Regenerate and render the map
        RenderMap();
        
        // Find and respawn the player at a valid floor position
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Vector2Int floorPos = GetRandomFloorPosition();
            Vector3 worldPos = GetWorldPositionFromGridPosition(floorPos);
            player.transform.position = worldPos;
            Debug.Log($"[MapRenderer] Player respawned at {floorPos} (world: {worldPos})");
        }
        
        // Also respawn any other entities that might need it
        MapController mapController = FindObjectOfType<MapController>();
        if (mapController != null)
        {
            mapController.RespawnPlayer();
        }
    }
    
    private void DisableSmokeTest()
    {
        // Find and disable any AsciiGridSmokeTest components that might interfere
        AsciiGridSmokeTest[] smokeTests = FindObjectsOfType<AsciiGridSmokeTest>();
        foreach (var smokeTest in smokeTests)
        {
            if (smokeTest != null)
            {
                smokeTest.SetTestGridEnabled(false);
                Debug.Log("[MapRenderer] Disabled AsciiGridSmokeTest to prevent interference");
            }
        }
    }
}
