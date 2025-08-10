using UnityEngine;

public static class MouseWorld
{
    private static UnityEngine.Camera mainCamera;
    
    /// <summary>
    /// Converts mouse screen position to world position using the main camera
    /// </summary>
    /// <returns>World position of the mouse cursor</returns>
    public static Vector3 GetMouseWorldPosition()
    {
        // Cache main camera reference
        if (mainCamera == null)
        {
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("MouseWorld: No main camera found!");
                return Vector3.zero;
            }
        }
        
        // Get mouse screen position
        Vector3 mouseScreenPos = Input.mousePosition;
        
        // Convert to world position
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        
        // Set Z to camera's Z position for 2D
        mouseWorldPos.z = mainCamera.transform.position.z;
        
        return mouseWorldPos;
    }
    
    /// <summary>
    /// Converts mouse screen position to world position using a specific camera
    /// </summary>
    /// <param name="camera">Camera to use for conversion</param>
    /// <returns>World position of the mouse cursor</returns>
    public static Vector3 GetMouseWorldPosition(UnityEngine.Camera camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("MouseWorld: Camera parameter is null!");
            return Vector3.zero;
        }
        
        // Get mouse screen position
        Vector3 mouseScreenPos = Input.mousePosition;
        
        // Convert to world position
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
        
        // Set Z to camera's Z position for 2D
        mouseWorldPos.z = camera.transform.position.z;
        
        return mouseWorldPos;
    }
    
    /// <summary>
    /// Gets the mouse world position as a 2D vector (X, Y only)
    /// </summary>
    /// <returns>2D world position of the mouse cursor</returns>
    public static Vector2 GetMouseWorldPosition2D()
    {
        Vector3 worldPos = GetMouseWorldPosition();
        return new Vector2(worldPos.x, worldPos.y);
    }
    
    /// <summary>
    /// Gets the mouse world position as a 2D vector using a specific camera
    /// </summary>
    /// <param name="camera">Camera to use for conversion</param>
    /// <returns>2D world position of the mouse cursor</returns>
    public static Vector2 GetMouseWorldPosition2D(UnityEngine.Camera camera)
    {
        Vector3 worldPos = GetMouseWorldPosition(camera);
        return new Vector2(worldPos.x, worldPos.y);
    }
}
