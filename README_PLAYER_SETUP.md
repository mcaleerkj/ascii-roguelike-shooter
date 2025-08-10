# Player Setup Guide - NTShooter

This guide covers setting up the player character with NT-style aiming and movement in your ASCII grid system.

## Components Overview

### 1. PlayerController.cs
- **Purpose**: Handles WASD movement and ASCII grid rendering
- **Location**: `Assets/Scripts/Player/PlayerController.cs`
- **Dependencies**: AsciiGrid reference

### 2. PlayerAim.cs (NEW - NT-Style Aiming)
- **Purpose**: Handles mouse-based aiming with reticle orbiting around player center
- **Location**: `Assets/Scripts/Player/PlayerAim.cs`
- **Dependencies**: Camera, Reticle

### 3. Reticle.cs
- **Purpose**: Visual aiming indicator using TextMeshPro
- **Location**: `Assets/Scripts/Player/Reticle.cs`
- **Dependencies**: TextMeshPro, TMP_FontAsset

### 4. MouseWorld.cs
- **Purpose**: Utility for converting mouse screen position to world coordinates
- **Location**: `Assets/Scripts/Util/MouseWorld.cs`
- **Dependencies**: Main camera (auto-finds)

## Scene Setup Instructions

### Step 1: Create Player GameObject
1. Create an empty GameObject named "Player"
2. Position it at the center of your scene (0,0,0) or where you want the player to spawn

### Step 2: Add PlayerController
1. Add the `PlayerController` component to the Player GameObject
2. Configure settings:
   - **Move Speed**: 6 (default) - adjust for desired movement speed
   - **Glyph**: '@' (default) - ASCII character to represent the player
   - **FG Color**: White (default) - player character color
   - **BG Color**: Black (default) - player background color
   - **AsciiGrid**: Drag your AsciiGrid GameObject here

### Step 3: Add PlayerAim (NT-Style Aiming)
1. Add the `PlayerAim` component to the same Player GameObject
2. Configure settings:
   - **Main Cam**: Drag your Main Camera here (or leave empty for auto-find)
   - **Reticle**: Leave empty for auto-find, or drag your Reticle GameObject
   - **Aim Anchor**: Leave empty - will be auto-created as child
   - **Orbit Radius World**: 0.35 (default) - reticle orbit distance
   - **Min Aim Magnitude**: 0.0005 (default) - hide reticle when mouse is very close
   - **Center Player To Grid Cell**: true (default) - snap player to grid cell center on start
   - **Cell Size**: 0.16666667 (default) - must match your ASCII grid cell size
   - **Grid Origin**: Leave empty to assume (0,0,0) world origin
   - **Snap To Pixels**: true (default) - pixel-perfect positioning
   - **Log Debug**: false (default) - enable for troubleshooting
   - **Offset X**: 0 (default) - fine-tune reticle X position in world units
   - **Offset Y**: 0 (default) - fine-tune reticle Y position in world units

### Step 4: Create Reticle
1. Create an empty GameObject named "Reticle"
2. Add the `Reticle` component
3. Configure settings:
   - **Font Asset**: Assign a monospace TMP font (optional - will auto-detect)
   - **Pixels Per Cell**: 8 (default) - visual scale reference
   - **Glyph**: '•' (default) - aiming indicator character
   - **Color**: White (default) - reticle color

### Step 5: Position Reticle
1. **IMPORTANT**: The Reticle should NOT be under any scaled parent except the Player
2. Position it at the scene root level (not under Canvas or other UI elements)
3. The PlayerAim script will automatically parent it to the AimAnchor on start

## How It Works

### Movement System
- WASD keys control player movement
- Movement is clamped to ASCII grid boundaries
- Player position is automatically snapped to grid cell centers (if enabled)

### Aiming System
- Mouse position determines aim direction
- Reticle orbits around player center at fixed radius
- AimAnchor ensures perfect centering regardless of player hierarchy/scale
- Pixel snapping prevents shimmer at different zoom levels
- **Offset Settings**: Fine-tune reticle position relative to player pivot for perfect ASCII cell alignment

### Grid Integration
- Player is rendered as ASCII character in the grid
- Movement respects grid boundaries
- Optional grid cell centering for pixel-perfect positioning

## Testing

### Movement Test
1. Enter Play mode
2. Use WASD to move the player
3. Player should move smoothly and stay within grid bounds
4. Press Space to log grid boundaries in console

### Aiming Test
1. Move mouse around the player
2. Cyan gizmo circle should appear around player center (select Player to see)
3. Reticle should orbit smoothly around player center
4. Reticle should hide when mouse is very close to player

### Grid Centering Test
1. Toggle `centerPlayerToGridCell` in PlayerAim
2. Enter Play mode
3. Player should snap to nearest grid cell center
4. Check console for centering confirmation

### Offset Fine-Tuning Test
1. Enter Play mode
2. Adjust `Offset X` and `Offset Y` values in small increments (0.05)
3. Reticle should move relative to player pivot without affecting orbit logic
4. Use context menu options for quick offset testing:
   - Right-click PlayerAim component → "Reset Offset to (0,0)"
   - Right-click PlayerAim component → "Set Offset to (0.05, 0.05)"
   - Right-click PlayerAim component → "Set Offset to (0.1, 0.1)"

## Troubleshooting

### Reticle Not Visible
- Ensure Reticle is not under Canvas or UI elements
- Check that TMP font is assigned or available
- Verify PlayerAim component is properly configured

### Movement Issues
- Check AsciiGrid reference in PlayerController
- Verify grid bounds are correct
- Check console for error messages

### Aiming Issues
- Ensure Main Camera is assigned or available
- Check that AimAnchor was created (should see "AimAnchor" child under Player)
- Verify Reticle is properly parented to AimAnchor

### Performance Issues
- Large ASCII grids (80x45) create many GameObjects
- Consider reducing grid size for mobile devices
- Monitor draw calls in Unity Profiler

## Advanced Configuration

### Custom Orbit Radius
- Adjust `orbitRadiusWorld` in PlayerAim for different reticle distances
- Value is in world units (e.g., 0.35 = 35% of world unit)

### Reticle Offset Fine-Tuning
- Use `Offset X` and `Offset Y` to align reticle exactly with ASCII cell centers
- Values are in world units (e.g., 0.05 = 5% of world unit)
- Offset is applied after orbit calculation, so it doesn't affect aiming direction
- Use `SetOffset(x, y)` method for runtime adjustments
- Context menu options provide quick preset values for testing

### Grid Cell Size
- Must match your ASCII grid configuration
- Default 0.16666667 assumes 8px per cell at PPU=48
- Use PixelMath.CellSizeForPixels() for exact calculations

### Pixel Perfect Settings
- Keep Pixel Perfect Camera at 384x216
- Enable Upscale RT, Crop X/Y, Pixel Snapping
- Disable Stretch for crisp pixels

## Integration Notes

- PlayerController and PlayerAim work together but are independent
- PlayerAim handles all aiming logic and reticle positioning
- PlayerController handles movement and grid rendering
- Both components can be used separately if needed
- Aim direction is accessible via `playerAim.AimDir` for other systems
