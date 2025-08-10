using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private GameObject playerPrefab;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("SceneSetup: Setting up scene...");
        
        // Setup player
        SetupPlayer();
        
        // Setup camera
        SetupCamera();
        
        Debug.Log("SceneSetup: Scene setup complete!");
    }
    
    private void SetupPlayer()
    {
        // Find or create player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            if (playerPrefab != null)
            {
                GameObject playerObj = Instantiate(playerPrefab);
                player = playerObj.GetComponent<PlayerController>();
                Debug.Log("SceneSetup: Created player from prefab");
            }
            else
            {
                // Create a simple player GameObject
                GameObject playerObj = new GameObject("Player");
                player = playerObj.AddComponent<PlayerController>();
                
                                 // Position player at center of grid
                 var asciiGrid = FindObjectOfType<AsciiGrid>();
                 if (asciiGrid != null)
                 {
                     Vector3 centerPos = new Vector3(
                         (asciiGrid.Width * asciiGrid.CellSize) / 2f,
                         (asciiGrid.Height * asciiGrid.CellSize) / 2f,
                         0f
                     );
                     playerObj.transform.position = centerPos;
                     Debug.Log($"SceneSetup: Positioned player at grid center: {centerPos}");
                 }
                
                Debug.Log("SceneSetup: Created simple player GameObject");
            }
        }
        else
        {
            Debug.Log("SceneSetup: Player already exists in scene");
        }
    }
    
    private void SetupCamera()
    {
        // Find main camera
        UnityEngine.Camera mainCam = UnityEngine.Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("SceneSetup: No main camera found!");
            return;
        }
        
        // Add or configure CameraFollow
        CameraFollow cameraFollow = mainCam.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            Debug.Log("SceneSetup: Added CameraFollow to main camera");
        }
        
        // Find player to assign as target
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            cameraFollow.SetTarget(player.transform);
            Debug.Log("SceneSetup: Assigned player as camera target");
        }
        else
        {
            Debug.LogWarning("SceneSetup: No player found for camera target!");
        }
    }
    
    [ContextMenu("Clear Scene")]
    public void ClearScene()
    {
        // Remove setup components
        var setups = FindObjectsOfType<SceneSetup>();
        foreach (var setup in setups)
        {
            if (setup != this)
            {
                DestroyImmediate(setup.gameObject);
            }
        }
        
        Debug.Log("SceneSetup: Cleared scene setup components");
    }
}
