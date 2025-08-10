using UnityEngine;

public static class PixelSnap
{
    // Assumes Pixel Perfect Camera reference Y = 216
    public static float PixelsPerWorldUnit(Camera cam, float referenceHeightPixels = 216f)
    {
        float worldH = cam.orthographicSize * 2f;
        return referenceHeightPixels / worldH;
    }

    public static Vector3 SnapWorldToPixel(Camera cam, Vector3 worldPos, float referenceHeightPixels = 216f)
    {
        float ppu = PixelsPerWorldUnit(cam, referenceHeightPixels);
        float x = Mathf.Round(worldPos.x * ppu) / ppu;
        float y = Mathf.Round(worldPos.y * ppu) / ppu;
        return new Vector3(x, y, worldPos.z);
    }
}
