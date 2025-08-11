using UnityEngine;
using TMPro;

public class AsciiViewportRenderer : MonoBehaviour
{
    [Header("Refs")]
    public GameRenderConfig config;
    public AsciiGrid grid;           // the on-screen grid (size = visible cells)
    public TMP_FontAsset font;
    
    [Header("Redraw")]
    public bool redrawEveryFrame = true;
    public Camera mainCam;
    
    private MapData mapData;          // full map data (tiles)
    private Vector2Int bl, visible;
    private float cellSize;
    
    // Public read-only properties for debugging
    public Vector2Int VisibleSize => visible;
    public Vector2Int BottomLeft => bl;
    
    void Awake()
    {
        if (!mainCam) mainCam = Camera.main;
        
        // Ensure this GameObject is parented under the Main Camera
        if (transform.parent == Camera.main.transform)
        {
            // Already properly parented
        }
        else
        {
            Debug.LogWarning("[Viewport] Parent this GameObject under the Main Camera so it always fills the screen.");
        }
        
        // Find render config if not assigned
        if (config == null)
        {
            config = FindObjectOfType<GameRenderConfig>();
            if (config == null)
            {
                Debug.LogError("[AsciiViewportRenderer] No GameRenderConfig found!");
            }
        }
        
        // Find font if not assigned
        if (font == null)
        {
            font = FindObjectOfType<TMP_FontAsset>();
            if (font == null)
            {
                Debug.LogError("[AsciiViewportRenderer] No TMP_FontAsset found!");
            }
        }
    }
    
    public void Init(MapData map)
    {
        SetMap(map);
    }
    
    public void SetMap(MapData map)
    {
        if (map == null)
        {
            Debug.LogError("[AsciiViewportRenderer] Map data is null!");
            return;
        }
        
        if (mainCam == null)
        {
            Debug.LogError("[AsciiViewportRenderer] Main camera is null!");
            return;
        }
        
        if (config == null)
        {
            Debug.LogError("[AsciiViewportRenderer] GameRenderConfig is null!");
            return;
        }
        
        if (font == null)
        {
            Debug.LogError("[AsciiViewportRenderer] No font available! Cannot initialize grid.");
            return;
        }
        
        mapData = map;
        
        // Cache the visible size exactly from config and compute cellSize from PPU
        visible = new Vector2Int(config.referenceWidth / config.pixelsPerCell, config.referenceHeight / config.pixelsPerCell);
        float ppu = PixelMath.GetPPU(mainCam);
        cellSize = config.pixelsPerCell / ppu;
        
        Debug.Log($"[Viewport] visible cells = {visible.x}x{visible.y}, pixelsPerCell={config.pixelsPerCell}, cellSize={cellSize:F6}");
        
        // Create or recreate the grid to exactly match visible dimensions
        if (grid == null)
        {
            var go = new GameObject("ViewportGrid");
            go.transform.SetParent(transform);
            grid = go.AddComponent<AsciiGrid>();
        }
        
        try
        {
            grid.Init(visible.x, visible.y, mainCam, config.pixelsPerCell, font, grid.transform);
            Debug.Log($"[AsciiViewportRenderer] Grid initialized successfully: {visible.x}x{visible.y}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AsciiViewportRenderer] Failed to initialize grid: {e.Message}");
            return;
        }
        
        // Position the viewport grid to align with the camera's viewport
        // The camera's orthographic size represents half the height in world units
        float cameraHeight = mainCam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCam.aspect;
        
        // Calculate the bottom-left corner of the camera's viewport in world coordinates
        float viewportLeft = mainCam.transform.position.x - (cameraWidth * 0.5f);
        float viewportBottom = mainCam.transform.position.y - (cameraHeight * 0.5f);
        
        // Position the grid so its bottom-left aligns with the camera's viewport bottom-left
        grid.transform.position = new Vector3(viewportLeft, viewportBottom, 0f);
        
        Debug.Log($"[Viewport] Camera viewport: {cameraWidth:F3}x{cameraHeight:F3}, Grid positioned at: ({viewportLeft:F3}, {viewportBottom:F3})");
        
        // Initialize viewport position and redraw
        var camCenter = mainCam.transform.position;
        camCenter.z = 0f;
        bl = ViewportMath.BottomLeftCell(camCenter, cellSize, visible, mapData.width, mapData.height);
        
        Redraw();
    }
    
    void LateUpdate()
    {
        if (mainCam == null || mapData == null || grid == null) return;
        
        // Always recompute bottom-left from current camera center and clamp to map bounds:
        var camPos = mainCam.transform.position; camPos.z = 0f;
        var newBL = ViewportMath.BottomLeftCell(camPos, cellSize, visible, mapData.width, mapData.height);
        newBL.x = Mathf.Clamp(newBL.x, 0, Mathf.Max(0, mapData.width - visible.x));
        newBL.y = Mathf.Clamp(newBL.y, 0, Mathf.Max(0, mapData.height - visible.y));

        bool changed = redrawEveryFrame || newBL != bl;
        bl = newBL;

        // Always reposition the viewport grid to stay aligned with camera viewport
        float cameraHeight = mainCam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCam.aspect;
        float viewportLeft = camPos.x - (cameraWidth * 0.5f);
        float viewportBottom = camPos.y - (cameraHeight * 0.5f);
        grid.transform.position = new Vector3(viewportLeft, viewportBottom, 0f);

        if (changed) Redraw();
    }
    
    void Redraw()
    {
        if (mapData == null || grid == null) return;
        
        // Fill the entire visible window (no early returns), using clamped x0/y0
        int x0 = Mathf.Clamp(bl.x, 0, Mathf.Max(0, mapData.width - visible.x));
        int y0 = Mathf.Clamp(bl.y, 0, Mathf.Max(0, mapData.height - visible.y));

        // First pass: Render map tiles
        for (int vy = 0; vy < visible.y; vy++)
        {
            int my = y0 + vy;
            for (int vx = 0; vx < visible.x; vx++)
            {
                int mx = x0 + vx;
                Tile t = (mx >= 0 && my >= 0 && mx < mapData.width && my < mapData.height) ? mapData.Get(mx, my) : Tile.Wall;
                grid.SetCell(vx, vy, t == Tile.Wall ? AsciiCell.Create('#', new Color32(64, 64, 64, 255), new Color32(32, 32, 32, 255)) : AsciiCell.Create('.', new Color32(128, 128, 128, 255), Color.black));
            }
        }
        
        // Second pass: Render game objects (player, enemies, etc.)
        RenderGameObjects(x0, y0);
        
        // Debug info every 30 frames
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[AsciiViewportRenderer] Viewport: BL({bl.x},{bl.y}) Size({visible.x}x{visible.y}) Map({mapData.width}x{mapData.height})");
        }
    }
    
    void RenderGameObjects(int mapOffsetX, int mapOffsetY)
    {
        // Find and render the player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            RenderGameObject(player.gameObject, player.transform.position, '@', Color.white, Color.black, mapOffsetX, mapOffsetY);
        }
        
        // Find and render other game objects that might need ASCII representation
        // (You can add more object types here as needed)
    }
    
    void RenderGameObject(GameObject obj, Vector3 worldPos, char glyph, Color32 fgColor, Color32 bgColor, int mapOffsetX, int mapOffsetY)
    {
        // Convert world position to map coordinates
        Vector2Int mapPos = new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize)
        );
        
        // Convert map coordinates to viewport coordinates
        int viewportX = mapPos.x - mapOffsetX;
        int viewportY = mapPos.y - mapOffsetY;
        
        // Check if the object is within the current viewport
        if (viewportX >= 0 && viewportX < visible.x && viewportY >= 0 && viewportY < visible.y)
        {
            // Render the object at this viewport position
            grid.SetCell(viewportX, viewportY, AsciiCell.Create(glyph, fgColor, bgColor));
            
            // Debug log for player rendering
            if (glyph == '@')
            {
                Debug.Log($"[AsciiViewportRenderer] Rendered player at viewport({viewportX},{viewportY}) map({mapPos.x},{mapPos.y}) world({worldPos.x:F2},{worldPos.y:F2})");
            }
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Viewport Rendering")]
    public void TestViewportRendering()
    {
        if (mapData != null)
        {
            Redraw();
            Debug.Log("[AsciiViewportRenderer] Forced viewport redraw");
        }
    }
    
    [ContextMenu("Force Viewport To Right Edge")]
    public void ForceViewportToRightEdge()
    {
        if (mapData != null && grid != null)
        {
            bl.x = Mathf.Max(0, mapData.width - visible.x);
            Redraw();
            Debug.Log($"[AsciiViewportRenderer] Forced viewport to right edge: BL({bl.x},{bl.y})");
        }
    }
    
    [ContextMenu("Force Viewport To Center")]
    public void ForceViewportToCenter()
    {
        if (mapData != null && grid != null)
        {
            bl.x = Mathf.Max(0, (mapData.width - visible.x) / 2);
            bl.y = Mathf.Max(0, (mapData.height - visible.y) / 2);
            Redraw();
            Debug.Log($"[AsciiViewportRenderer] Forced viewport to center: BL({bl.x},{bl.y})");
        }
    }
    
    [ContextMenu("Force Viewport Update")]
    public void ForceViewportUpdate()
    {
        if (mapData != null && grid != null)
        {
            var camPos = mainCam.transform.position;
            bl = ViewportMath.BottomLeftCell(camPos, cellSize, visible, mapData.width, mapData.height);
            Redraw();
            Debug.Log($"[AsciiViewportRenderer] Forced viewport update: BL({bl.x},{bl.y})");
        }
    }
}
