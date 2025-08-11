using UnityEngine;
using TMPro;

public class ViewportSetup : MonoBehaviour
{
    [Header("Configuration")]
    public GameRenderConfig renderConfig;
    public TMP_FontAsset monospaceFont;
    
    [Header("Components")]
    public MapRenderer mapRenderer;
    public Camera mainCamera;
    public Transform player;
    
    [ContextMenu("Setup Viewport System")]
    public void SetupViewportSystem()
    {
        Debug.Log("[ViewportSetup] Setting up viewport system...");
        
        // Create GameRenderConfig if not exists
        if (renderConfig == null)
        {
            renderConfig = ScriptableObject.CreateInstance<GameRenderConfig>();
            Debug.Log("[ViewportSetup] Created new GameRenderConfig");
        }
        
        // Find or create MapRenderer
        if (mapRenderer == null)
        {
            mapRenderer = FindObjectOfType<MapRenderer>();
            if (mapRenderer == null)
            {
                var go = new GameObject("MapRenderer");
                mapRenderer = go.AddComponent<MapRenderer>();
                Debug.Log("[ViewportSetup] Created new MapRenderer");
            }
        }
        
        // Find main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Find player
        if (player == null)
        {
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
        
        // Setup MapRenderer
        if (mapRenderer != null)
        {
            mapRenderer.renderConfig = renderConfig;
            Debug.Log("[ViewportSetup] Assigned renderConfig to MapRenderer");
        }
        
        // Setup CameraFollowBounds
        if (mainCamera != null)
        {
            var cameraFollow = mainCamera.GetComponent<CameraFollowBounds>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollowBounds>();
                Debug.Log("[ViewportSetup] Added CameraFollowBounds to main camera");
            }
            
            if (mapRenderer != null)
            {
                cameraFollow.map = mapRenderer.CurrentMap;
            }
            if (player != null)
            {
                cameraFollow.target = player;
            }
            cameraFollow.config = renderConfig;
            
            Debug.Log("[ViewportSetup] Configured CameraFollowBounds");
        }
        
        Debug.Log("[ViewportSetup] Viewport system setup complete!");
    }
    
    [ContextMenu("Create GameRenderConfig Asset")]
    public void CreateGameRenderConfigAsset()
    {
        if (renderConfig == null)
        {
            renderConfig = ScriptableObject.CreateInstance<GameRenderConfig>();
        }
        
        // This would normally save to disk, but for now just log
        Debug.Log("[ViewportSetup] GameRenderConfig created. Please save it as an asset manually.");
    }
}
