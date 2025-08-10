using UnityEngine;

public class PixelPerfectTest : MonoBehaviour
{
    [Header("Test Settings")]
    public Camera testCamera;
    public int testPixelsPerCell = 8;
    
    void Start()
    {
        if (testCamera == null)
            testCamera = Camera.main;
            
        if (testCamera != null)
        {
            Debug.Log("=== PIXEL PERFECT TEST ===");
            Debug.Log($"Camera orthographic size: {testCamera.orthographicSize}");
            Debug.Log($"PPU: {PixelMath.GetPPU(testCamera):F2}");
            Debug.Log($"Cell size for {testPixelsPerCell}px: {PixelMath.CellSizeForPixels(testCamera, testPixelsPerCell):F6}");
            Debug.Log($"Expected PPU for 384x216: 48.00");
            Debug.Log($"Expected cell size for 8px: 0.166667");
            Debug.Log("==========================");
        }
    }
    
    [ContextMenu("Test 6px per cell")]
    public void Test6PixelsPerCell()
    {
        if (testCamera != null)
        {
            float cellSize = PixelMath.CellSizeForPixels(testCamera, 6);
            Debug.Log($"6px per cell → cellSize = {cellSize:F6}");
        }
    }
    
    [ContextMenu("Test 8px per cell")]
    public void Test8PixelsPerCell()
    {
        if (testCamera != null)
        {
            float cellSize = PixelMath.CellSizeForPixels(testCamera, 8);
            Debug.Log($"8px per cell → cellSize = {cellSize:F6}");
        }
    }
    
    [ContextMenu("Test 10px per cell")]
    public void Test10PixelsPerCell()
    {
        if (testCamera != null)
        {
            float cellSize = PixelMath.CellSizeForPixels(testCamera, 10);
            Debug.Log($"10px per cell → cellSize = {cellSize:F6}");
        }
    }
}
