using UnityEngine;

public static class ViewportMath
{
    public static Vector2Int VisibleCells(Camera cam, int refW, int refH, int ppc)
    {
        // visible columns = refW / pixelsPerCell, rows = refH / pixelsPerCell
        return new Vector2Int(Mathf.Max(1, refW / ppc), Mathf.Max(1, refH / ppc));
    }
    
    // Given camera world center and cellSize, get bottom-left map cell index
    public static Vector2Int BottomLeftCell(Vector3 camWorldCenter, float cellSize, Vector2Int visibleCells, int mapW, int mapH)
    {
        // center cell coords
        int cx = Mathf.FloorToInt(camWorldCenter.x / cellSize);
        int cy = Mathf.FloorToInt(camWorldCenter.y / cellSize);
        
        int halfW = visibleCells.x / 2;
        int halfH = visibleCells.y / 2;
        
        // Calculate bottom-left cell index
        int x0 = cx - halfW;
        int y0 = cy - halfH;
        
        // Clamp to map boundaries
        x0 = Mathf.Clamp(x0, 0, Mathf.Max(0, mapW - visibleCells.x));
        y0 = Mathf.Clamp(y0, 0, Mathf.Max(0, mapH - visibleCells.y));
        
        // Ensure we don't go negative
        x0 = Mathf.Max(0, x0);
        y0 = Mathf.Max(0, y0);
        
        // Ensure we don't exceed map bounds
        if (x0 + visibleCells.x > mapW)
        {
            x0 = Mathf.Max(0, mapW - visibleCells.x);
        }
        
        if (y0 + visibleCells.y > mapH)
        {
            y0 = Mathf.Max(0, mapH - visibleCells.y);
        }
        
        return new Vector2Int(x0, y0);
    }
}
