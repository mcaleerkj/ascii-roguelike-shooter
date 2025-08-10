using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float mouseLeadAmount = 0.5f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    
    // Private fields
    private Vector3 velocity = Vector3.zero;
    private UnityEngine.Camera cam;
    
    private void Start()
    {
        // Get camera component
        cam = GetComponent<UnityEngine.Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No Camera component found!");
            return;
        }
        
                    // Find player if not assigned
            if (target == null)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("CameraFollow: Auto-assigned player as target");
                }
                else
                {
                    Debug.LogWarning("CameraFollow: No player found and no target assigned!");
                }
            }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate target position with mouse lead
        Vector3 targetPosition = CalculateTargetPosition();
        
        // Smooth follow
        Vector3 newPosition = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime
        );
        
        // Keep Z at camera's default
        newPosition.z = transform.position.z;
        
        transform.position = newPosition;
    }
    
    private Vector3 CalculateTargetPosition()
    {
        Vector3 basePosition = target.position + offset;
        
        // Add mouse lead
        if (cam != null)
        {
            Vector3 mouseWorldPos = MouseWorld.GetMouseWorldPosition();
            Vector3 mouseLead = (mouseWorldPos - target.position).normalized * mouseLeadAmount;
            basePosition += mouseLead;
        }
        
        return basePosition;
    }
    
    // Public methods
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetMouseLeadAmount(float amount)
    {
        mouseLeadAmount = Mathf.Clamp(amount, 0f, 2f);
    }
    
    public void SetSmoothTime(float time)
    {
        smoothTime = Mathf.Clamp(time, 0.01f, 1f);
    }
}
