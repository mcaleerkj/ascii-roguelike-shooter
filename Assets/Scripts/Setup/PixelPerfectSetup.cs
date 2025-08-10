using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Automatically sets up pixel-perfect components in the scene
/// </summary>
public class PixelPerfectSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool showSetupLogs = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPixelPerfectComponents();
        }
    }
    
    [ContextMenu("Setup Pixel Perfect Components")]
    public void SetupPixelPerfectComponents()
    {
        if (showSetupLogs)
            Debug.Log("PixelPerfectSetup: Starting automatic setup...");
        
        // Find and setup AsciiGrid
        var asciiGrid = FindObjectOfType<AsciiGrid>();
        if (asciiGrid != null)
        {
            SetupAsciiGrid(asciiGrid);
        }
        
        // Find and setup Player
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            SetupPlayer(player);
        }
        
        // Find and setup Camera
        var mainCamera = UnityEngine.Camera.main;
        if (mainCamera != null)
        {
            SetupCamera(mainCamera);
        }
        
        if (showSetupLogs)
            Debug.Log("PixelPerfectSetup: Setup complete!");
    }
    
    private void SetupAsciiGrid(AsciiGrid asciiGrid)
    {
        if (showSetupLogs)
            Debug.Log("PixelPerfectSetup: Setting up AsciiGrid...");
            
        // AsciiGrid will automatically add PixelSnapper to its holder
        // No additional setup needed here
    }
    
    private void SetupPlayer(PlayerController player)
    {
        if (showSetupLogs)
            Debug.Log("PixelPerfectSetup: Setting up Player...");
            
        // Add PixelSnapper to player
        var playerSnapper = player.GetComponent<PixelSnapper>();
        if (playerSnapper == null)
        {
            playerSnapper = player.gameObject.AddComponent<PixelSnapper>();
            playerSnapper.includeChildren = false; // Player only, not children
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Added PixelSnapper to Player");
        }
        
        // Add PlayerAim if not present
        var playerAim = player.GetComponent<PlayerAim>();
        if (playerAim == null)
        {
            playerAim = player.gameObject.AddComponent<PlayerAim>();
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Added PlayerAim to Player");
        }
        
        // Create Reticle if not present
        var reticle = player.GetComponentInChildren<Reticle>();
        if (reticle == null)
        {
            GameObject reticleObj = new GameObject("Reticle");
            reticleObj.transform.SetParent(player.transform);
            reticleObj.transform.localPosition = Vector3.zero;
            reticleObj.transform.localScale = Vector3.one; // Ensure neutral scale
            
            reticle = reticleObj.AddComponent<Reticle>();
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Created Reticle for Player");
        }
        
        // Configure reticle with proper cell size
        if (reticle != null)
        {
            // Reticle now uses pixel-perfect sizing automatically
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Reticle uses pixel-perfect sizing (8px per cell)");
            
            // Ensure reticle has a font asset
            if (reticle.fontAsset == null)
            {
                var asciiGridComponent = FindObjectOfType<AsciiGrid>();
                if (asciiGridComponent != null)
                {
                    reticle.fontAsset = asciiGridComponent.FontAsset;
                    if (showSetupLogs)
                        Debug.Log("PixelPerfectSetup: Assigned AsciiGrid font to Reticle");
                }
            }
        }
        
        // Add PixelSnapper to reticle
        var reticleSnapper = reticle.GetComponent<PixelSnapper>();
        if (reticleSnapper == null)
        {
            reticleSnapper = reticle.gameObject.AddComponent<PixelSnapper>();
            reticleSnapper.includeChildren = false; // Don't snap children, we handle TMP scaling
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Added PixelSnapper to Reticle");
        }
        
        // Configure PlayerAim references
        if (playerAim != null)
        {
            if (playerAim.mainCam == null)
                playerAim.mainCam = UnityEngine.Camera.main;
            if (playerAim.reticle == null)
                playerAim.reticle = reticle;
                
            if (showSetupLogs)
                Debug.Log("PixelPerfectSetup: Configured PlayerAim references");
        }
    }
    
    private void SetupCamera(UnityEngine.Camera camera)
    {
        if (showSetupLogs)
            Debug.Log("PixelPerfectSetup: Setting up Camera...");
            
        // Camera doesn't need PixelSnapper by default
        // Pixel Perfect Camera component should already be configured
    }
    
    [ContextMenu("Check Pixel Perfect Camera Settings")]
    public void CheckPixelPerfectCameraSettings()
    {
        var mainCamera = UnityEngine.Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("PixelPerfectSetup: No main camera found!");
            return;
        }
        
        var pixelPerfectCamera = mainCamera.GetComponent<UnityEngine.U2D.PixelPerfectCamera>();
        if (pixelPerfectCamera == null)
        {
            Debug.LogWarning("PixelPerfectSetup: No Pixel Perfect Camera component found on main camera!");
            return;
        }
        
        Debug.Log($"PixelPerfectSetup: Pixel Perfect Camera Settings:");
        Debug.Log($"  - Assets Pixels Per Unit: {pixelPerfectCamera.assetsPPU}");
        Debug.Log($"  - Reference Resolution: {pixelPerfectCamera.refResolutionX}x{pixelPerfectCamera.refResolutionY}");
        Debug.Log($"  - Pixel Snapping: {pixelPerfectCamera.pixelSnapping}");
        Debug.Log($"  - Current Pixel Ratio: {pixelPerfectCamera.pixelRatio}");
    }
}
