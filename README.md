# NTShooter - ASCII Grid System

A Unity 2D top-down ASCII shooter with NT-style controls, featuring a pixel-perfect ASCII grid rendering system.

## Features

- **Pixel-Perfect Rendering**: 384×216 resolution with proper scaling
- **ASCII Grid System**: 80×45 grid with individual TextMeshPro (3D) objects per cell
- **World-Space Rendering**: All ASCII characters are positioned in 3D world space
- **Automatic Font Detection**: Finds and uses available monospace fonts
- **Efficient Updates**: Direct text updates without dirty flag systems

## Setup Instructions

### 1. Camera Configuration

1. **Add PixelScaler Script**: Attach the `PixelScaler` script to your main camera
2. **Camera Settings**:
   - Set to Orthographic
   - Clear Flags: Solid Color
   - Background Color: Black
   - Orthographic Size: Will be set automatically by PixelScaler

### 2. ASCII Grid Setup

1. **Create Grid Object**: Create an empty GameObject in your scene
2. **Add AsciiGrid Component**: Attach the `AsciiGrid` script
3. **Configure Grid**:
   - Width: 80 (default)
   - Height: 45 (default)
   - Cell Size: 0.1 (default)
   - Font Asset: Assign a monospace TMP font (optional - will auto-detect)
   - Font Size: 16 (default) - Controls the size of TextMeshPro components

### 3. Testing

1. **Add Smoke Test**: Attach `AsciiGridSmokeTest` to the same GameObject
2. **Configure Test**:
   - Grid Width: 80
   - Grid Height: 45
   - Cell Size: 0.1
   - Font Asset: Same as AsciiGrid (optional)
3. **Run Scene**: You should see a bordered room with "HELLO ASCII" message
4. **Interactive**: Press 'R' to cycle through message colors

### 4. Pixel Perfect Camera Settings

For optimal results, configure your camera with these settings:
- **Resolution**: 384×216
- **Upscale RT**: ON
- **Crop X/Y**: ON
- **Stretch**: OFF
- **Pixel Snapping**: ON

## Scripts Overview

### AsciiCell.cs
- Simple struct containing character, foreground color, and background color
- Uses Color32 for efficient memory usage
- Provides Empty static property with non-breaking space

### AsciiGrid.cs
- Manages the 2D grid of ASCII cells
- Creates one TextMeshPro (3D) object per cell
- Positions cells at `(x + 0.5f) * cellSize, (y + 0.5f) * cellSize`
- Scales objects with `localScale = Vector3.one * cellSize`
- Uses configurable `fontSize` (default 16f) for TextMeshPro components
- Provides runtime font size adjustment via `ChangeFontSize()` method
- Includes context menu options for quick font size changes (16, 24, 32)

### AsciiGridSmokeTest.cs
- Demonstrates the grid system functionality
- Creates a bordered room with floor tiles
- Displays "HELLO ASCII" message in the center
- Allows color cycling with 'R' key

### PixelScaler.cs
- Manages pixel-perfect rendering
- Creates 384×216 RenderTexture
- Uses Point filtering for crisp pixels
- Handles camera setup and rendering

## Usage Examples

### Setting Individual Cells
```csharp
asciiGrid.SetCell(x, y, AsciiCell.Create('@', Color.white, Color.black));
```

### Clearing the Grid
```csharp
asciiGrid.Clear(AsciiCell.Create('.', Color.gray, Color.black));
```

### Getting Cell Data
```csharp
AsciiCell cell = asciiGrid.GetCell(x, y);
```

## Troubleshooting

### White Boxes
- Ensure no UI RawImage components are covering the grid
- Check that TextMeshPro components have proper fonts assigned
- Verify camera is positioned correctly

### No Text Visible
- Check console for font detection messages
- Ensure TMP_FontAsset is assigned or monospace fonts are available
- Verify cell positioning and scaling

### Performance Issues
- Grid size affects performance (80×45 = 3600 objects)
- Consider reducing grid size for mobile devices
- Monitor draw calls in Unity Profiler

## Dependencies

- Unity 2022+
- TextMeshPro package
- Built-in Render Pipeline (not URP)

## Notes

- All ASCII characters are world-space objects, not UI elements
- Non-breaking space (`\u00A0`) is used for empty cells to ensure proper rendering
- The system automatically finds monospace fonts if none are assigned
- Cell positioning uses local space relative to the AsciiGrid parent
