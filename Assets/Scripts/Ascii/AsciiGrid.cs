using UnityEngine;
using TMPro;

public class AsciiGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 19; // Smaller grid for larger cells
    [SerializeField] private int height = 10; // Smaller grid for larger cells
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private float fontSize = 16f; // Font size for TextMeshPro components
    
    [Header("Screen Fitting")]
    [SerializeField] private bool autoFitToScreen = true;
    [SerializeField] private Vector2 targetScreenSize = new Vector2(384f, 216f);
    
    // Public property to control auto-fit behavior
    public bool AutoFitToScreen
    {
        get { return autoFitToScreen; }
        set { autoFitToScreen = value; }
    }
    
    private AsciiCell[,] grid;
    private TextMeshPro[,] textComponents;
    private SpriteRenderer[,] backgroundSprites; // Add this to store background sprite references
    private GameObject asciiCellsParent;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public TMP_FontAsset FontAsset => fontAsset;
    public float FontSize => fontSize;
    
    // Debug method to visualize grid boundaries
    [ContextMenu("Log Grid Info")]
    public void LogGridInfo()
    {
        Debug.Log($"=== ASCII GRID INFO ===");
        Debug.Log($"Grid Dimensions: {width}x{height}");
        Debug.Log($"Cell Size: {cellSize}");
        Debug.Log($"Total Size: {width * cellSize} x {height * cellSize}");
        Debug.Log($"Grid Position: {asciiCellsParent.transform.position}");
        Debug.Log($"Grid Local Position: {asciiCellsParent.transform.localPosition}");
        Debug.Log($"======================");
    }
    
    // Method to change pixels per cell at runtime
    [ContextMenu("Change to 6px per cell")]
    public void ChangeTo6PixelsPerCell()
    {
        ChangePixelsPerCell(6);
    }
    
    [ContextMenu("Change to 8px per cell")]
    public void ChangeTo8PixelsPerCell()
    {
        ChangePixelsPerCell(8);
    }
    
    [ContextMenu("Change to 10px per cell")]
    public void ChangeTo10PixelsPerCell()
    {
        ChangePixelsPerCell(10);
    }

    [ContextMenu("Change font size to 16")]
    public void ChangeFontSizeTo16()
    {
        ChangeFontSize(16f);
    }

    [ContextMenu("Change font size to 24")]
    public void ChangeFontSizeTo24()
    {
        ChangeFontSize(24f);
    }

    [ContextMenu("Change font size to 32")]
    public void ChangeFontSizeTo32()
    {
        ChangeFontSize(32f);
    }
    
    public void ChangePixelsPerCell(int pixelsPerCell)
    {
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            float newCellSize = PixelMath.CellSizeForPixels(mainCam, pixelsPerCell);
            if (Mathf.Abs(newCellSize - cellSize) > 0.0001f)
            {
                cellSize = newCellSize;
                Debug.Log($"AsciiGrid: Changed to {pixelsPerCell}px per cell, new cellSize = {cellSize:F6}");
                InitializeGrid(); // Reinitialize with new cell size
            }
            else
            {
                Debug.Log($"AsciiGrid: Already using {pixelsPerCell}px per cell");
            }
        }
        else
        {
            Debug.LogWarning("AsciiGrid: No camera found for pixel-perfect sizing");
        }
    }

    public void ChangeFontSize(float newFontSize)
    {
        if (Mathf.Abs(newFontSize - fontSize) > 0.0001f)
        {
            fontSize = newFontSize;
            Debug.Log($"AsciiGrid: Changed font size to {fontSize}");
            
            // Update all existing text components with new font size
            if (textComponents != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (textComponents[x, y] != null)
                        {
                            textComponents[x, y].fontSize = fontSize;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log($"AsciiGrid: Already using font size {fontSize}");
        }
    }

    private void Awake()
    {
        if (fontAsset == null)
        {
            fontAsset = FindMonospaceFont();
        }
        
        if (fontAsset == null)
        {
            Debug.LogError("AsciiGrid: No font asset found and no monospace font available!");
            return;
        }
        
        // Use pixel-perfect cell sizing by default
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            cellSize = PixelMath.CellSizeForPixels(mainCam, 8); // Default to 8px per cell
            Debug.Log($"AsciiGrid: PPU={PixelMath.GetPPU(mainCam):F2}, cellSize(8px)={cellSize:F6}");
        }
        else
        {
            // Fallback to auto-fit if no camera
            if (autoFitToScreen)
            {
                cellSize = CalculatePerfectCellSize();
                Debug.Log($"AsciiGrid: Auto-calculated cell size: {cellSize} to fit {targetScreenSize.x}x{targetScreenSize.y} screen");
            }
        }
        
        Debug.Log($"AsciiGrid: Initializing with font: {fontAsset.name}");
        
        // Initialize immediately in Awake to ensure proper setup
        InitializeGrid();
        
        // One-time sanity logger on first frame
        StartCoroutine(LogSanityCheck());
    }
    
    private System.Collections.IEnumerator LogSanityCheck()
    {
        yield return new WaitForEndOfFrame();
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"=== PIXEL PERFECT SANITY CHECK ===");
            Debug.Log($"PPU={PixelMath.GetPPU(mainCam):F2}, cellSize(8px)={PixelMath.CellSizeForPixels(mainCam, 8):F6}");
            Debug.Log($"Grid: {width}x{height} cells, Total size: {width * cellSize:F6} x {height * cellSize:F6}");
            Debug.Log($"==================================");
        }
    }
    
    private float CalculatePerfectCellSize()
    {
        // Calculate the cell size that will make the grid perfectly fit the target screen
        float cellSizeX = targetScreenSize.x / width;
        float cellSizeY = targetScreenSize.y / height;
        
        // Use the smaller of the two to ensure the grid fits within the screen bounds
        float perfectCellSize = Mathf.Min(cellSizeX, cellSizeY);
        
        Debug.Log($"AsciiGrid: Screen size {targetScreenSize.x}x{targetScreenSize.y}, Grid {width}x{height}");
        Debug.Log($"AsciiGrid: Calculated cell sizes - X: {cellSizeX}, Y: {cellSizeY}, Using: {perfectCellSize}");
        
        return perfectCellSize;
    }

    private TMP_FontAsset FindMonospaceFont()
    {
        // Try to find any available monospace font
        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var font in fonts)
        {
            if (font.name.ToLower().Contains("mono") || font.name.ToLower().Contains("courier"))
            {
                return font;
            }
        }
        
        // Fallback to default font
        return TMP_Settings.defaultFontAsset;
    }

    public void Init(int gridWidth, int gridHeight, TMP_FontAsset font)
    {
        // Route through new Init method for backward compatibility
        Init(gridWidth, gridHeight, Camera.main, 8, font, null);
    }
    
    public void Init(int gridWidth, int gridHeight, Camera cam, int pixelsPerCell, TMP_FontAsset font, Transform parent)
    {
        // Only update if parameters actually changed
        if (this.width != gridWidth || this.height != gridHeight || this.fontAsset != font)
        {
            this.width = gridWidth;
            this.height = gridHeight;
            this.fontAsset = font;
            
            // Compute cellSize using PixelMath for pixel-perfect rendering
            if (cam != null)
            {
                this.cellSize = PixelMath.CellSizeForPixels(cam, pixelsPerCell);
                Debug.Log($"AsciiGrid: Computed cellSize = {cellSize:F6} for {pixelsPerCell}px per cell (PPU = {PixelMath.GetPPU(cam):F2})");
            }
            else
            {
                Debug.LogWarning("AsciiGrid: No camera provided, using existing cellSize");
            }
            
            // Set parent if provided
            if (parent != null)
            {
                transform.SetParent(parent);
            }

            Debug.Log($"AsciiGrid: Reinitializing {width}x{height} grid with font {font.name}");
            
            InitializeGrid(); // This will use the computed cellSize
        }
    }

    private void InitializeGrid()
    {
        // Initialize arrays
        grid = new AsciiCell[width, height];
        textComponents = new TextMeshPro[width, height];
        backgroundSprites = new SpriteRenderer[width, height]; // Initialize background sprites array

        // Create parent holder for all ASCII cells
        if (asciiCellsParent != null)
        {
            DestroyImmediate(asciiCellsParent);
        }
        
        asciiCellsParent = new GameObject("AsciiCells");
        asciiCellsParent.transform.SetParent(transform);
        
        // Position grid at world origin (0,0) to align with player movement bounds
        asciiCellsParent.transform.localPosition = Vector3.zero;

        // Create visual objects first
        CreateVisualObjects();
        
        // Initialize grid with empty cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = AsciiCell.Create(' ', Color.white, Color.black);
            }
        }
    }

    private void CreateVisualObjects()
    {
        // Clear existing arrays
        if (textComponents != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (textComponents[x, y] != null)
                    {
                        DestroyImmediate(textComponents[x, y].gameObject);
                    }
                    if (backgroundSprites[x, y] != null)
                    {
                        DestroyImmediate(backgroundSprites[x, y].gameObject);
                    }
                }
            }
        }
        
        // Create new visual objects
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create background object
                GameObject bgObj = new GameObject($"Cell_BG_{x}_{y}");
                bgObj.transform.SetParent(asciiCellsParent.transform);
                bgObj.transform.localPosition = new Vector3(
                    x * cellSize, 
                    y * cellSize, 
                    0f
                );
                
                SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
                bgRenderer.sprite = CreatePixelSprite();
                bgRenderer.color = Color.black; // Default background color
                bgRenderer.sortingOrder = 0;
                
                // Store background sprite reference
                backgroundSprites[x, y] = bgRenderer;
                
                // Create text object
                GameObject textObj = new GameObject($"Cell_Text_{x}_{y}");
                textObj.transform.SetParent(asciiCellsParent.transform);
                textObj.transform.localPosition = new Vector3(
                    (x + 0.5f) * cellSize, 
                    (y + 0.5f) * cellSize, 
                    0f
                );
                
                // Scale the text object to match cell size
                textObj.transform.localScale = Vector3.one * cellSize;
                
                // Add TextMeshPro component with crisp settings
                TextMeshPro text = textObj.AddComponent<TextMeshPro>();
                text.font = fontAsset;
                text.fontSize = fontSize; // Use the configurable font size
                text.alignment = TextAlignmentOptions.Center;
                text.enableWordWrapping = false;
                text.richText = false;
                text.text = "\u00A0"; // Non-breaking space as placeholder
                text.color = Color.white;
                
                // TMP material hardening for crisp rendering
                if (text.fontMaterial != null)
                {
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0);
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0);
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineColor, 0);
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowOffset, 0);
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
                }
                
                // Store reference
                textComponents[x, y] = text;
                
                // Debug log for first few cells
                if (x < 3 && y < 3)
                {
                    Debug.Log($"AsciiGrid: Created TMP object ({x},{y}) at position {textObj.transform.localPosition}");
                }
            }
        }
        
        // Add PixelSnapper to the AsciiCells holder for automatic pixel snapping
        AddPixelSnapperToHolder();
    }
    
             private void AddPixelSnapperToHolder()
         {
             if (asciiCellsParent != null)
             {
                 // Check if PixelSnapper already exists
                 var existingSnapper = asciiCellsParent.GetComponent<PixelSnapper>();
                 if (existingSnapper == null)
                 {
                     var snapper = asciiCellsParent.AddComponent<PixelSnapper>();
                     snapper.includeChildren = true;
                     Debug.Log("AsciiGrid: Added PixelSnapper to AsciiCells holder");
                 }
             }
         }

    public void SetCell(int x, int y, AsciiCell cell)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"AsciiGrid: SetCell out of bounds ({x},{y}) - Grid size: {width}x{height}");
            return;
        }
            
        grid[x, y] = cell;
        
        // Update the visual representation immediately
        if (textComponents[x, y] != null && backgroundSprites[x, y] != null)
        {
            // Update background color
            backgroundSprites[x, y].color = cell.bg;
            
            // Use NBSP for spaces, otherwise display the actual char
            string displayText = (cell.ch == ' ') ? "\u00A0" : cell.ch.ToString();
            textComponents[x, y].text = displayText;
            textComponents[x, y].color = cell.fg;
            
            // Debug log for first few cells
            if (x < 3 && y < 3)
            {
                Debug.Log($"AsciiGrid: Set cell ({x},{y}) = '{displayText}' with color {cell.fg}, bg {cell.bg}");
            }
        }
        else
        {
            Debug.LogError($"AsciiGrid: TextComponent or BackgroundSprite is null for cell ({x},{y})");
        }
    }

    public AsciiCell GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return AsciiCell.Empty;
                
        return grid[x, y];
    }

    public void Clear(AsciiCell fill)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SetCell(x, y, fill);
            }
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize, 0);
    }

    public Vector2Int WorldToGrid(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.y / cellSize);
        return new Vector2Int(x, y);
    }

    private void OnDestroy()
    {
        if (asciiCellsParent != null)
        {
            DestroyImmediate(asciiCellsParent);
        }
    }
    
    private Sprite CreatePixelSprite()
    {
        // Create a 1x1 white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        return sprite;
    }
}
