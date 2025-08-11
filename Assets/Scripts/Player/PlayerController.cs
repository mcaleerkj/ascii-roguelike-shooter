using UnityEngine;

/// <summary>
/// Handles player movement and ASCII grid rendering.
/// For NT-style aiming, add the PlayerAim component to the same GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool disableCollision = false; // Set to false to enable wall collision detection
    
    [Header("Rendering")]
    [SerializeField] private char glyph = '@';
    [SerializeField] private Color32 fgColor = Color.white;
    [SerializeField] private Color32 bgColor = Color.black; // Changed from Color.clear to make player visible
    
    [Header("References")]
    [SerializeField] private AsciiGrid asciiGrid;
    [SerializeField] private MapRenderer mapRenderer;
    
    // Private fields
    private Vector2 aimDirection;
    private Vector2 lastPlayerPos;
    private bool isInitialized = false;
    
    private void Start()
    {
        // Find AsciiGrid if not assigned
        if (asciiGrid == null)
        {
            asciiGrid = FindObjectOfType<AsciiGrid>();
            if (asciiGrid == null)
            {
                Debug.LogError("PlayerController: No AsciiGrid found in scene!");
                return;
            }
        }
        
        // Find MapRenderer if not assigned
        if (mapRenderer == null)
        {
            mapRenderer = FindObjectOfType<MapRenderer>();
        }
        
        // Position player at center of grid
        Vector3 centerPosition = Vector3.zero;
        if (asciiGrid != null)
        {
            // Calculate center position based on grid dimensions
            float centerX = (asciiGrid.Width * asciiGrid.CellSize) / 2f;
            float centerY = (asciiGrid.Height * asciiGrid.CellSize) / 2f;
            centerPosition = new Vector3(centerX, centerY, 0f);
            
            // Set player position to grid center
            transform.position = centerPosition;
            Debug.Log($"PlayerController: Spawned player at grid center: {centerPosition}");
        }
        
        // Initialize player position
        lastPlayerPos = transform.position;
        isInitialized = true;
        
        // Draw initial player
        DrawPlayer();
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        HandleMovement();
        HandleAiming();
        
        // Only draw player if we're not using viewport rendering
        if (!IsUsingViewportRendering())
        {
            DrawPlayer();
        }
    }
    
    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        
        // WASD input
        if (Input.GetKey(KeyCode.W)) input.y += 1f;
        if (Input.GetKey(KeyCode.S)) input.y -= 1f;
        if (Input.GetKey(KeyCode.A)) input.x -= 1f;
        if (Input.GetKey(KeyCode.D)) input.x += 1f;
        
        // Normalize diagonal movement
        if (input.magnitude > 1f)
        {
            input.Normalize();
        }
        
        // Apply movement
        Vector2 movement = input * moveSpeed * Time.deltaTime;
        Vector2 newPosition = (Vector2)transform.position + movement;
        
        // Check for wall collisions before moving
        if (!disableCollision && IsUsingViewportRendering() && mapRenderer != null)
        {
            // Use MapRenderer for collision detection when using viewport rendering
            Vector2Int newGridPos = WorldToMapGrid(newPosition);
            
            // Debug collision detection
            if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
            {
                Debug.Log($"[PlayerController] Movement: {input}, New Grid Pos: {newGridPos}, Is Wall: {IsWallAtMapPosition(newGridPos)}");
            }
            
            if (IsWallAtMapPosition(newGridPos))
            {
                Debug.Log($"[PlayerController] Wall collision detected at grid position {newGridPos}");
                
                // Try to move only on X axis if Y movement hits a wall
                if (input.x != 0)
                {
                    Vector2 xOnlyPosition = new Vector2(newPosition.x, transform.position.y);
                    Vector2Int xOnlyGridPos = WorldToMapGrid(xOnlyPosition);
                    if (!IsWallAtMapPosition(xOnlyGridPos))
                    {
                        newPosition = xOnlyPosition;
                        Debug.Log($"[PlayerController] Allowing X-only movement to {xOnlyPosition}");
                    }
                    else
                    {
                        // Both X and Y movement hit walls, don't move
                        Debug.Log($"[PlayerController] Blocked in both directions");
                        return;
                    }
                }
                else if (input.y != 0)
                {
                    // Try to move only on Y axis if X movement hits a wall
                    Vector2 yOnlyPosition = new Vector2(transform.position.x, newPosition.y);
                    Vector2Int yOnlyGridPos = WorldToMapGrid(yOnlyPosition);
                    if (!IsWallAtMapPosition(yOnlyGridPos))
                    {
                        newPosition = yOnlyPosition;
                        Debug.Log($"[PlayerController] Allowing Y-only movement to {yOnlyPosition}");
                    }
                    else
                    {
                        // Both X and Y movement hit walls, don't move
                        Debug.Log($"[PlayerController] Blocked in both directions");
                        return;
                    }
                }
                else
                {
                    // No movement input, don't move
                    return;
                }
            }
            
            // Clamp to map bounds
            float maxX = mapRenderer.CurrentMap.width * GetCellSize();
            float maxY = mapRenderer.CurrentMap.height * GetCellSize();
            newPosition.x = Mathf.Clamp(newPosition.x, 0f, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, 0f, maxY);
        }
        else if (!disableCollision && asciiGrid != null)
        {
            // Legacy collision detection using AsciiGrid
            Vector2Int newGridPos = asciiGrid.WorldToGrid(newPosition);
            
            if (IsWallAtGridPosition(newGridPos))
            {
                // Try to move only on X axis if Y movement hits a wall
                if (input.x != 0)
                {
                    Vector2 xOnlyPosition = new Vector2(newPosition.x, transform.position.y);
                    Vector2Int xOnlyGridPos = asciiGrid.WorldToGrid(xOnlyPosition);
                    if (!IsWallAtGridPosition(xOnlyGridPos))
                    {
                        newPosition = xOnlyPosition;
                    }
                    else
                    {
                        // Both X and Y movement hit walls, don't move
                        return;
                    }
                }
                else if (input.y != 0)
                {
                    // Try to move only on Y axis if X movement hits a wall
                    Vector2 yOnlyPosition = new Vector2(transform.position.x, newPosition.y);
                    Vector2Int yOnlyGridPos = asciiGrid.WorldToGrid(yOnlyPosition);
                    if (!IsWallAtGridPosition(yOnlyGridPos))
                    {
                        newPosition = yOnlyPosition;
                    }
                    else
                    {
                        // Both X and Y movement hit walls, don't move
                        return;
                    }
                }
                else
                {
                    // Both X and Y movement hit walls, don't move
                    return;
                }
            }
            
            // Clamp to grid bounds (grid extends from 0 to width*cellSize and 0 to height*cellSize)
            float maxX = asciiGrid.Width * asciiGrid.CellSize;
            float maxY = asciiGrid.Height * asciiGrid.CellSize;
            
            // Debug: Log bounds for troubleshooting
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"PlayerController: Grid bounds - X: 0 to {maxX}, Y: 0 to {maxY}");
                Debug.Log($"PlayerController: Current position: {transform.position}, New position: {newPosition}");
            }
            
            newPosition.x = Mathf.Clamp(newPosition.x, 0f, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, 0f, maxY);
        }
        
        transform.position = newPosition;
    }
    
    private void HandleAiming()
    {
        // Aiming is now handled by PlayerAim component
        // This method is kept for compatibility but can be removed if not needed elsewhere
        var playerAim = GetComponent<PlayerAim>();
        if (playerAim != null)
        {
            aimDirection = playerAim.AimDir;
        }
        else
        {
            // Fallback to old method if PlayerAim is not present
            Vector3 mouseWorldPos = MouseWorld.GetMouseWorldPosition();
            Vector2 playerPos = transform.position;
            aimDirection = (mouseWorldPos - (Vector3)playerPos).normalized;
        }
    }
    
    private void DrawPlayer()
    {
        if (asciiGrid == null) return;
        
        // Clear previous player position
        if (lastPlayerPos != (Vector2)transform.position)
        {
            Vector2Int lastCell = asciiGrid.WorldToGrid(lastPlayerPos);
            if (IsValidGridPosition(lastCell.x, lastCell.y))
            {
                // Restore background (you might want to store the original background)
                asciiGrid.SetCell(lastCell.x, lastCell.y, AsciiCell.Create('.', Color.gray, Color.black));
            }
        }
        
        // Draw player at current position
        Vector2Int currentCell = asciiGrid.WorldToGrid(transform.position);
        if (IsValidGridPosition(currentCell.x, currentCell.y))
        {
            asciiGrid.SetCell(currentCell.x, currentCell.y, AsciiCell.Create(glyph, fgColor, bgColor));
            lastPlayerPos = transform.position;
            
            // Debug log to see what's happening (only when moving to reduce spam)
            if (lastPlayerPos != (Vector2)transform.position)
            {
                Debug.Log($"PlayerController: Drawing '{glyph}' at grid ({currentCell.x},{currentCell.y}) from world pos {transform.position}");
            }
        }
        else
        {
            Debug.LogWarning($"PlayerController: Invalid grid position ({currentCell.x},{currentCell.y}) for world pos {transform.position}");
        }
    }
    
    private bool IsValidGridPosition(int x, int y)
    {
        if (asciiGrid == null) return false;
        return x >= 0 && x < asciiGrid.Width && y >= 0 && y < asciiGrid.Height;
    }
    
    private bool IsWallAtGridPosition(Vector2Int gridPos)
    {
        if (asciiGrid == null) return false;
        
        // Check if position is within grid bounds
        if (!IsValidGridPosition(gridPos.x, gridPos.y)) return true; // Treat out of bounds as walls
        
        // Use MapRenderer's logic if available (more robust)
        if (mapRenderer != null)
        {
            return !mapRenderer.IsFloorAt(gridPos);
        }
        
        // Fallback: Get the cell at this position and check if it's a wall character
        AsciiCell cell = asciiGrid.GetCell(gridPos.x, gridPos.y);
        return cell.ch == '#'; // Assuming '#' is wall
    }
    
    // Public getters
    public Vector2 GetAimDirection() => aimDirection;
    public Vector2 GetPlayerPosition() => transform.position;
    
    // Debug method to visualize grid boundaries
    [ContextMenu("Log Grid Boundaries")]
    public void LogGridBoundaries()
    {
        if (asciiGrid != null)
        {
            Debug.Log($"Grid Boundaries: Width={asciiGrid.Width}, Height={asciiGrid.Height}, CellSize={asciiGrid.CellSize}");
            Debug.Log($"World Bounds: X=0 to {asciiGrid.Width * asciiGrid.CellSize}, Y=0 to {asciiGrid.Height * asciiGrid.CellSize}");
        }
        else
        {
            Debug.LogWarning("No AsciiGrid reference found!");
        }
    }
    
    [ContextMenu("Disable Collision")]
    public void DisableCollision()
    {
        disableCollision = true;
        Debug.Log("Collision detection disabled - player can move freely through walls");
    }
    
    [ContextMenu("Enable Collision")]
    public void EnableCollision()
    {
        disableCollision = false;
        Debug.Log("Collision detection enabled - player will be blocked by walls");
    }
    
    [ContextMenu("Test Wall Collision")]
    public void TestWallCollision()
    {
        if (asciiGrid == null) return;
        
        Vector2Int currentGridPos = asciiGrid.WorldToGrid(transform.position);
        Debug.Log($"PlayerController: Current grid position: {currentGridPos}");
        Debug.Log($"PlayerController: Is wall at current position: {IsWallAtGridPosition(currentGridPos)}");
        
        // Test positions around the player
        Vector2Int[] testPositions = {
            currentGridPos + Vector2Int.up,
            currentGridPos + Vector2Int.down,
            currentGridPos + Vector2Int.left,
            currentGridPos + Vector2Int.right
        };
        
        foreach (var pos in testPositions)
        {
            bool isWall = IsWallAtGridPosition(pos);
            Debug.Log($"PlayerController: Position {pos}: Is wall = {isWall}");
        }
    }
    
    [ContextMenu("Test Map Collision")]
    public void TestMapCollision()
    {
        if (mapRenderer == null || mapRenderer.CurrentMap == null)
        {
            Debug.LogWarning("PlayerController: No map data available for collision testing");
            return;
        }
        
        Vector2Int currentGridPos = WorldToMapGrid(transform.position);
        Debug.Log($"[PlayerController] Current map grid position: {currentGridPos}");
        Debug.Log($"[PlayerController] Map size: {mapRenderer.CurrentMap.width}x{mapRenderer.CurrentMap.height}");
        Debug.Log($"[PlayerController] Is wall at current position: {IsWallAtMapPosition(currentGridPos)}");
        
        // Test positions around the player
        Vector2Int[] testPositions = {
            currentGridPos + Vector2Int.up,
            currentGridPos + Vector2Int.down,
            currentGridPos + Vector2Int.left,
            currentGridPos + Vector2Int.right
        };
        
        foreach (var pos in testPositions)
        {
            bool isWall = IsWallAtMapPosition(pos);
            Debug.Log($"[PlayerController] Map position {pos}: Is wall = {isWall}");
        }
    }

    private bool IsUsingViewportRendering()
    {
        // Check if we're using the new viewport rendering system
        if (mapRenderer != null && mapRenderer.viewportRenderer != null && !mapRenderer.forceLegacy)
        {
            return true;
        }
        
        // Also check if there's a viewport renderer in the scene
        var viewportRenderer = FindObjectOfType<AsciiViewportRenderer>();
        if (viewportRenderer != null && viewportRenderer.enabled)
        {
            return true;
        }
        
        return false;
    }

    private float GetCellSize()
    {
        // Get cell size from viewport renderer or fallback to default
        var viewportRenderer = FindObjectOfType<AsciiViewportRenderer>();
        if (viewportRenderer != null)
        {
            // Try to get cell size from viewport renderer
            var config = viewportRenderer.config;
            if (config != null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    float ppu = PixelMath.GetPPU(mainCam);
                    return config.pixelsPerCell / ppu;
                }
            }
        }
        
        // Fallback to AsciiGrid cell size
        if (asciiGrid != null)
        {
            return asciiGrid.CellSize;
        }
        
        // Default fallback
        return 0.5f;
    }
    
    private Vector2Int WorldToMapGrid(Vector3 worldPos)
    {
        float cellSize = GetCellSize();
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize)
        );
    }
    
    private bool IsWallAtMapPosition(Vector2Int gridPos)
    {
        if (mapRenderer == null || mapRenderer.CurrentMap == null) 
        {
            Debug.LogWarning($"[PlayerController] IsWallAtMapPosition: mapRenderer or CurrentMap is null at {gridPos}");
            return true;
        }
        
        // Check if position is within map bounds
        if (gridPos.x < 0 || gridPos.x >= mapRenderer.CurrentMap.width || 
            gridPos.y < 0 || gridPos.y >= mapRenderer.CurrentMap.height)
        {
            Debug.Log($"[PlayerController] IsWallAtMapPosition: Out of bounds at {gridPos}, map size: {mapRenderer.CurrentMap.width}x{mapRenderer.CurrentMap.height}");
            return true; // Treat out of bounds as walls
        }
        
        // Use MapRenderer's logic
        bool isWall = !mapRenderer.IsFloorAt(gridPos);
        
        // Debug wall detection occasionally
        if (Time.frameCount % 120 == 0) // Log every 120 frames
        {
            Debug.Log($"[PlayerController] IsWallAtMapPosition: Grid {gridPos} = Wall: {isWall}");
        }
        
        return isWall;
    }
}
