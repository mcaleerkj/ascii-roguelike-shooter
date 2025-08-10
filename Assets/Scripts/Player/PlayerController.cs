using UnityEngine;

/// <summary>
/// Handles player movement and ASCII grid rendering.
/// For NT-style aiming, add the PlayerAim component to the same GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    
    [Header("Rendering")]
    [SerializeField] private char glyph = '@';
    [SerializeField] private Color32 fgColor = Color.white;
    [SerializeField] private Color32 bgColor = Color.black; // Changed from Color.clear to make player visible
    
    [Header("References")]
    [SerializeField] private AsciiGrid asciiGrid;
    
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
        DrawPlayer();
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
        
        // Clamp to grid bounds (grid extends from 0 to width*cellSize and 0 to height*cellSize)
        if (asciiGrid != null)
        {
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
    
    // Public getters
    public Vector2 GetAimDirection() => aimDirection;
    public Vector2 GetPlayerPosition() => transform.position;
    
    // Debug method to visualize grid boundaries
    [ContextMenu("Log Grid Boundaries")]
    public void LogGridBoundaries()
    {
        if (asciiGrid != null)
        {
            float maxX = asciiGrid.Width * asciiGrid.CellSize;
            float maxY = asciiGrid.Height * asciiGrid.CellSize;
            
            Debug.Log($"=== GRID BOUNDARIES ===");
            Debug.Log($"Grid Size: {asciiGrid.Width}x{asciiGrid.CellSize} = {maxX} x {asciiGrid.Height}x{asciiGrid.CellSize} = {maxY}");
            Debug.Log($"Player Position: {transform.position}");
            Debug.Log($"Grid Cell: {asciiGrid.WorldToGrid(transform.position)}");
            Debug.Log($"Movement Bounds: X: 0 to {maxX}, Y: 0 to {maxY}");
            Debug.Log($"======================");
        }
    }
}
