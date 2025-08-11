using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("References")]
    public CellularAutomataGenerator generator;
    public MapRenderer mapRenderer;
    public AsciiGrid asciiGrid;
    public PlayerController playerController;
    public PlayerAim playerAim;
    public AsciiViewportRenderer viewportRenderer;
    public CameraFollow cameraFollow; // Changed from CameraFollowBounds to CameraFollow

    [Header("Player Spawning")]
    public bool autoSpawnPlayer = true;
    public bool centerPlayerOnSpawn = true;

    private MapData currentMap;

    void Start()
    {
        // Auto-find references if not assigned
        if (!generator)
            generator = FindObjectOfType<CellularAutomataGenerator>();
        
        if (!mapRenderer)
            mapRenderer = FindObjectOfType<MapRenderer>();
        
        if (!asciiGrid)
            asciiGrid = FindObjectOfType<AsciiGrid>();
        
        if (!playerController)
            playerController = FindObjectOfType<PlayerController>();
        
        if (!playerAim)
            playerAim = FindObjectOfType<PlayerAim>();
        
        if (!viewportRenderer)
            viewportRenderer = FindObjectOfType<AsciiViewportRenderer>();
        
        if (!cameraFollow)
            cameraFollow = FindObjectOfType<CameraFollow>(); // Changed from CameraFollowBounds

        // Generate and render the map
        GenerateAndRenderMap();

        // Spawn player if enabled
        if (autoSpawnPlayer)
        {
            SpawnPlayer();
        }
    }

    public void GenerateAndRenderMap()
    {
        if (!generator || !mapRenderer)
        {
            Debug.LogError("[MapController] Missing generator or mapRenderer!");
            return;
        }

        // Generate the map
        currentMap = generator.GenerateMap();
        
        // Initialize viewport renderer if available
        if (viewportRenderer != null)
        {
            viewportRenderer.Init(currentMap);
            Debug.Log("[MapController] Initialized viewport renderer with new map");
        }
        
        // Setup camera follow with new map and config
        if (cameraFollow != null)
        {
            // Find GameRenderConfig if not already assigned
            if (cameraFollow.config == null)
            {
                var renderConfig = FindObjectOfType<GameRenderConfig>();
                if (renderConfig != null)
                {
                    cameraFollow.SetConfig(renderConfig);
                    Debug.Log("[MapController] Assigned GameRenderConfig to camera");
                }
                else
                {
                    Debug.LogWarning("[MapController] No GameRenderConfig found for camera!");
                }
            }
            
            // Set the map data
            cameraFollow.SetMap(currentMap);
            
            // Recompute cell size based on camera
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraFollow.RecomputeCellSize(mainCam);
                Debug.Log("[MapController] Updated camera cell size and map bounds");
            }
        }
        
        // Render it to the ASCII grid (or delegate to viewport)
        mapRenderer.RenderMap();
        
        Debug.Log("[MapController] Map generated and rendered successfully");
    }

    public void SpawnPlayer()
    {
        if (!playerController || !mapRenderer)
        {
            Debug.LogError("[MapController] Missing playerController or mapRenderer!");
            return;
        }

        // Get a random floor position
        Vector2Int spawnPos = mapRenderer.GetRandomFloorPosition();
        
        if (spawnPos == Vector2Int.zero)
        {
            Debug.LogWarning("[MapController] Could not find valid spawn position!");
            return;
        }

        // Convert to world position
        Vector3 worldPos = mapRenderer.GetWorldPositionFromGridPosition(spawnPos);
        
        // Move player to spawn position
        playerController.transform.position = worldPos;
        
        // Center player to grid cell if enabled
        if (centerPlayerOnSpawn && playerAim)
        {
            playerAim.CenterPlayerToNearestGridCell();
        }

        Debug.Log($"[MapController] Player spawned at grid position {spawnPos} (world: {worldPos})");
    }

    public void RegenerateMap()
    {
        GenerateAndRenderMap();
        
        if (autoSpawnPlayer)
        {
            SpawnPlayer();
        }
    }
    
    public void RespawnPlayer()
    {
        if (autoSpawnPlayer)
        {
            SpawnPlayer();
        }
    }

    public MapData GetCurrentMap()
    {
        return currentMap;
    }

    public bool IsFloorAt(Vector2Int gridPos)
    {
        if (!mapRenderer) return false;
        return mapRenderer.IsFloorAt(gridPos);
    }

    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        if (!mapRenderer) return Vector3.zero;
        return mapRenderer.GetWorldPositionFromGridPosition(gridPos);
    }

    // Context menu for testing
    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        RegenerateMap();
    }

    [ContextMenu("Spawn Player")]
    public void SpawnPlayerManually()
    {
        SpawnPlayer();
    }

    [ContextMenu("Log Current Map Info")]
    public void LogMapInfo()
    {
        if (currentMap != null)
        {
            Debug.Log($"[MapController] Current map: {currentMap.width}x{currentMap.height}");
            
            int floorCount = 0;
            int wallCount = 0;
            
            for (int y = 0; y < currentMap.height; y++)
            {
                for (int x = 0; x < currentMap.width; x++)
                {
                    if (currentMap.Get(x, y) == Tile.Floor)
                        floorCount++;
                    else
                        wallCount++;
                }
            }
            
            Debug.Log($"[MapController] Floor tiles: {floorCount}, Wall tiles: {wallCount}");
        }
        else
        {
            Debug.Log("[MapController] No map generated yet");
        }
    }
    
    [ContextMenu("Restart Level")]
    public void RestartLevelFromContext()
    {
        if (mapRenderer != null)
        {
            mapRenderer.RestartLevel();
        }
    }
}
