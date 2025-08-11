using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Data")]
    [SerializeField] private Transform target;
    public GameRenderConfig config;
    public MapData map;
    [Tooltip("World units per cell. Set via RecomputeCellSize(cam) = pixelsPerCell / PPU.")]
    public float cellSize = 0.16666667f;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private float mouseLeadAmount = 0f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private Vector3 vel;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) Debug.LogError("CameraFollow: No Camera component.");
        if (!target)
        {
            var p = FindObjectOfType<PlayerController>();
            if (p) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!target || map == null || config == null) return;

        // Visible window (cells -> world)
        int visX = Mathf.Max(1, config.referenceWidth  / config.pixelsPerCell);
        int visY = Mathf.Max(1, config.referenceHeight / config.pixelsPerCell);
        float viewW = visX * cellSize;
        float viewH = visY * cellSize;
        float halfW = viewW * 0.5f;
        float halfH = viewH * 0.5f;

        // Desired center
        Vector3 desired = target.position;
        desired += (Vector3)offset;

        if (mouseLeadAmount > 0f && cam != null)
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(cam.transform.position.z);
            Vector3 mw = cam.ScreenToWorldPoint(mouse);
            Vector2 lead = ((Vector2)(mw - target.position)).normalized * mouseLeadAmount;
            desired += (Vector3)lead;
        }

        // Map bounds in world
        float mapW = map.width  * cellSize;
        float mapH = map.height * cellSize;

        // Clamp so full viewport stays inside the map
        float cx = Mathf.Clamp(desired.x, halfW, Mathf.Max(halfW, mapW - halfW));
        float cy = Mathf.Clamp(desired.y, halfH, Mathf.Max(halfH, mapH - halfH));

        Vector3 center = new Vector3(cx, cy, transform.position.z);

        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, center, ref vel, smoothTime);
    }

    // Utilities
    public void SetTarget(Transform t) => target = t;
    public void SetMap(MapData m) => map = m;
    public void SetConfig(GameRenderConfig c) => config = c;

    public void RecomputeCellSize(Camera c)
    {
        if (config == null || c == null) return;
        float ppu = PixelMath.GetPPU(c); // referenceHeight / worldHeight
        cellSize = config.pixelsPerCell / ppu;
    }
}
