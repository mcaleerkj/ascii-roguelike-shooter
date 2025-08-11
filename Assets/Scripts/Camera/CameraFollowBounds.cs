using UnityEngine;

public class CameraFollowBounds : MonoBehaviour
{
    public Transform target;
    public MapData map;
    public GameRenderConfig config;
    public float smooth = 0.12f;
    
    Camera cam;
    Vector3 vel;
    float cellSize;
    
    void Awake() 
    { 
        cam = GetComponent<Camera>(); 
    }
    
    void LateUpdate()
    {
        if (!target || map == null || cam == null || config == null) return;
        
        // Calculate cell size from camera PPU and pixels per cell
        float ppu = PixelMath.GetPPU(cam);
        cellSize = config.pixelsPerCell / ppu;
        
        // Visible window in cells
        int visX = config.referenceWidth / config.pixelsPerCell;
        int visY = config.referenceHeight / config.pixelsPerCell;
        
        // Map size in world units
        float mapW = map.width * cellSize;
        float mapH = map.height * cellSize;
        
        // Half window in world units
        float halfW = (visX * cellSize) * 0.5f;
        float halfH = (visY * cellSize) * 0.5f;
        
        // Target position (player)
        Vector3 targetPos = target.position;
        
        // Clamp camera center so viewport stays inside map bounds
        float cx = Mathf.Clamp(targetPos.x, halfW, Mathf.Max(halfW, mapW - halfW));
        float cy = Mathf.Clamp(targetPos.y, halfH, Mathf.Max(halfH, mapH - halfH));
        
        // Ensure we don't go beyond map boundaries
        if (mapW <= visX * cellSize)
        {
            // Map is smaller than viewport - center on map
            cx = mapW * 0.5f;
        }
        
        if (mapH <= visY * cellSize)
        {
            // Map is smaller than viewport - center on map
            cy = mapH * 0.5f;
        }
        
        Vector3 desired = new Vector3(cx, cy, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, smooth);
        
        // Debug info every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[CameraFollowBounds] Target: {targetPos}, Clamped: ({cx},{cy}), Map: {mapW}x{mapH}, Viewport: {visX}x{visY}, CellSize: {cellSize}");
        }
    }
    
    [ContextMenu("Force Camera to Right Edge")]
    public void ForceCameraToRightEdge()
    {
        if (map == null || config == null) return;
        
        float ppu = PixelMath.GetPPU(cam);
        float cellSize = config.pixelsPerCell / ppu;
        
        int visX = config.referenceWidth / config.pixelsPerCell;
        int visY = config.referenceHeight / config.pixelsPerCell;
        
        float mapW = map.width * cellSize;
        float halfW = (visX * cellSize) * 0.5f;
        
        // Force camera to right edge
        float cx = mapW - halfW;
        float cy = transform.position.y;
        
        Vector3 desired = new Vector3(cx, cy, transform.position.z);
        transform.position = desired;
        
        Debug.Log($"[CameraFollowBounds] Forced camera to right edge: ({cx}, {cy})");
    }
    
    [ContextMenu("Force Camera to Center")]
    public void ForceCameraToCenter()
    {
        if (map == null || config == null) return;
        
        float ppu = PixelMath.GetPPU(cam);
        float cellSize = config.pixelsPerCell / ppu;
        
        float mapW = map.width * cellSize;
        float mapH = map.height * cellSize;
        
        // Force camera to center
        float cx = mapW * 0.5f;
        float cy = mapH * 0.5f;
        
        Vector3 desired = new Vector3(cx, cy, transform.position.z);
        transform.position = desired;
        
        Debug.Log($"[CameraFollowBounds] Forced camera to center: ({cx}, {cy})");
    }
}
