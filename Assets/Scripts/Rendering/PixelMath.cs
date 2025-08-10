using UnityEngine;

public static class PixelMath
{
    public const float ReferenceHeightPixels = 216f; // PPC reference Y
    
    public static float GetPPU(Camera cam)
    {
        // orthographicSize = half world height
        float worldH = cam.orthographicSize * 2f;
        return ReferenceHeightPixels / worldH; // pixels per world unit
    }
    
    public static float CellSizeForPixels(Camera cam, int pixelsPerCell)
    {
        float ppu = GetPPU(cam);
        return pixelsPerCell / ppu;
    }
}
