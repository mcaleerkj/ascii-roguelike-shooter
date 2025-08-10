using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAim : MonoBehaviour
{
    [Header("Refs")]
    public Camera mainCam;
    public Transform aimAnchor;     // child created automatically if null
    public Reticle reticle;

    [Header("Orbit")]
    [Tooltip("Orbit radius in WORLD units.")]
    public float orbitRadiusWorld = 0.35f;
    [Tooltip("Hide reticle if mouse is extremely close to player.")]
    public float minAimMagnitude = 0.0005f;

    [Header("Grid Centering (optional)")]
    [Tooltip("If true, we'll center the Player to the nearest grid cell center on Awake.")]
    public bool centerPlayerToGridCell = true;
    [Tooltip("World cell size; must match your ASCII grid (e.g., 0.16666667 for 8px @ PPU=48).")]
    public float cellSize = 0.16666667f;
    [Tooltip("World origin of the grid (0,0) in world space. Leave empty to assume (0,0).")]
    public Transform gridOrigin;

    [Header("Offset Settings")]
    [Tooltip("Offset the reticle position in local space from the player's pivot point.")]
    public float offsetX = 0f;
    [Tooltip("Offset the reticle position in local space from the player's pivot point.")]
    public float offsetY = 0f;

    [Header("Snapping")]
    public bool snapToPixels = true;
    public bool logDebug = false;

    public Vector2 AimDir { get; private set; }

    void Reset()
    {
        if (!mainCam) mainCam = Camera.main;
    }

    void Awake()
    {
        if (!mainCam) mainCam = Camera.main;
        EnsureAimAnchor();
        EnsureReticleParented();

        if (centerPlayerToGridCell)
            CenterPlayerToNearestGridCell();
    }

    void EnsureAimAnchor()
    {
        if (aimAnchor == null)
        {
            var anchorGo = new GameObject("AimAnchor");
            anchorGo.transform.SetParent(this.transform, false);
            anchorGo.transform.localPosition = Vector3.zero;
            anchorGo.transform.localRotation = Quaternion.identity;
            anchorGo.transform.localScale = Vector3.one;
            aimAnchor = anchorGo.transform;
        }
        else
        {
            // guarantee zeroed local so it's truly centered
            aimAnchor.SetParent(this.transform, false);
            aimAnchor.localPosition = Vector3.zero;
            aimAnchor.localRotation = Quaternion.identity;
            aimAnchor.localScale = Vector3.one;
        }
    }

    void EnsureReticleParented()
    {
        if (!reticle) reticle = FindObjectOfType<Reticle>();
        if (!reticle) return;

        // Parent the reticle to the aimAnchor so its localPosition is the orbit vector
        if (reticle.transform.parent != aimAnchor)
        {
            reticle.transform.SetParent(aimAnchor, worldPositionStays: false);
        }
        // Zero at center; orbit uses localPosition
        reticle.transform.localPosition = Vector3.zero;
        reticle.transform.localRotation = Quaternion.identity;
        reticle.transform.localScale = Vector3.one;
    }

    void CenterPlayerToNearestGridCell()
    {
        Vector3 origin = gridOrigin ? gridOrigin.position : Vector3.zero;

        // Convert to grid coords, round to cell center, convert back to world
        float gx = (transform.position.x - origin.x) / cellSize;
        float gy = (transform.position.y - origin.y) / cellSize;

        gx = Mathf.Round(gx);
        gy = Mathf.Round(gy);

        float cx = origin.x + (gx + 0.5f) * cellSize - 0.5f * cellSize;
        float cy = origin.y + (gy + 0.5f) * cellSize - 0.5f * cellSize;

        Vector3 centered = new Vector3(cx, cy, transform.position.z);

        // Optional pixel snap after centering
        if (snapToPixels && Camera.main != null)
            centered = PixelSnap.SnapWorldToPixel(Camera.main, centered);

        transform.position = centered;

        if (logDebug)
            Debug.Log($"[PlayerAim] Centered player to grid cell center at {centered}, cellSize={cellSize}");
    }

    void LateUpdate()
    {
        if (!mainCam || !reticle || !aimAnchor) return;

        // Mouse world
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(mouse);
        mouseWorld.z = 0f;

        // Direction from player center (aimAnchor is at player's exact center)
        Vector2 dir = (Vector2)(mouseWorld - aimAnchor.position);
        if (dir.sqrMagnitude < (minAimMagnitude * minAimMagnitude))
        {
            reticle.SetVisible(false);
            AimDir = Vector2.zero;
            return;
        }

        dir.Normalize();
        AimDir = dir;

        // Place reticle on a circle by setting LOCAL position on the anchor
        Vector3 localOrbit = (Vector3)(dir * orbitRadiusWorld);
        
        // Apply offset in local space from player's pivot point
        localOrbit += new Vector3(offsetX, offsetY, 0f);
        
        reticle.transform.localPosition = localOrbit;

        if (snapToPixels)
        {
            // Snap the final world position of the reticle to pixel grid
            Vector3 worldPos = reticle.transform.position;
            worldPos = PixelSnap.SnapWorldToPixel(mainCam, worldPos);
            reticle.transform.position = worldPos;
        }

        reticle.SetVisible(true);

        if (logDebug)
            Debug.Log($"[PlayerAim] Anchor {aimAnchor.position}  Mouse {mouseWorld}  AimDir {dir}  Offset ({offsetX:F3}, {offsetY:F3})  Reticle {reticle.transform.position}");
    }

    void OnDrawGizmosSelected()
    {
        if (aimAnchor == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(aimAnchor.position, orbitRadiusWorld);
    }

    // Public method to change offset at runtime
    public void SetOffset(float x, float y)
    {
        offsetX = x;
        offsetY = y;
        if (logDebug)
            Debug.Log($"[PlayerAim] Offset set to ({x:F3}, {y:F3})");
    }

    // Context menu methods for testing offsets
    [ContextMenu("Reset Offset to (0,0)")]
    public void ResetOffset()
    {
        SetOffset(0f, 0f);
        Debug.Log("[PlayerAim] Offset reset to (0,0)");
    }

    [ContextMenu("Set Offset to (0.05, 0.05)")]
    public void SetOffsetToSmall()
    {
        offsetX = 0.05f;
        offsetY = 0.05f;
        Debug.Log("[PlayerAim] Offset set to (0.05, 0.05)");
    }

    [ContextMenu("Set Offset to (0.1, 0.1)")]
    public void SetOffsetToMedium()
    {
        offsetX = 0.1f;
        offsetY = 0.1f;
        Debug.Log("[PlayerAim] Offset set to (0.1, 0.1)");
    }

    [ContextMenu("Set Offset to (-0.05, 0.05)")]
    public void SetOffsetToLeftUp()
    {
        offsetX = -0.05f;
        offsetY = 0.05f;
        Debug.Log("[PlayerAim] Offset set to (-0.05, 0.05)");
    }
}
