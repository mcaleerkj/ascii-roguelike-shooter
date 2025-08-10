using UnityEngine;
using TMPro;

public class AsciiGridSmokeTest : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 19; // Smaller grid for larger cells
    [SerializeField] private int gridHeight = 10; // Smaller grid for larger cells
    [SerializeField] private TMP_FontAsset fontAsset;
    
    [Header("Screen Fitting")]
    [SerializeField] private bool enableAutoFit = true;
    
    [Header("Test Content")]
    [SerializeField] private string testMessage = "HELLO ASCII";
    [SerializeField] private Color32[] testColors = {
        Color.white,
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta
    };
    
    private AsciiGrid asciiGrid;
    private int currentColorIndex = 0;
    private bool messageDrawn = false;

    private void Awake()
    {
        // Create AsciiGrid component if it doesn't exist
        asciiGrid = GetComponent<AsciiGrid>();
        if (asciiGrid == null)
        {
            asciiGrid = gameObject.AddComponent<AsciiGrid>();
        }
        
        // Wait a frame to ensure the grid is fully initialized
        StartCoroutine(InitializeAfterGrid());
    }
    
    private System.Collections.IEnumerator InitializeAfterGrid()
    {
        yield return new WaitForEndOfFrame();
        
        // Ensure auto-fit is enabled on the AsciiGrid instance
        asciiGrid.AutoFitToScreen = enableAutoFit;
        
        // Initialize the grid with pixel-perfect sizing
        asciiGrid.Init(gridWidth, gridHeight, Camera.main, 8, fontAsset, transform);
        
        Debug.Log("AsciiGridSmokeTest: Auto-fit enabled - grid will automatically fit screen");
        
        // No need for a second WaitForEndOfFrame as Init now calls InitializeGrid
        // Draw the test room
        DrawTestRoom();
    }

    private void Update()
    {
        // Press 'R' to change message color
        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeMessageColor();
        }
    }

    private void DrawTestRoom()
    {
        // Clear grid with floor character
        asciiGrid.Clear(AsciiCell.Create('.', Color.gray, Color.black));
        
        // Draw border with '#'
        DrawBorder();
        
        // Draw test message
        DrawMessage();
        
        messageDrawn = true;
        
        Debug.Log("AsciiGridSmokeTest: Test room drawn - player can now move around!");
    }

    private void DrawBorder()
    {
        // Top and bottom borders
        for (int x = 0; x < gridWidth; x++)
        {
            asciiGrid.SetCell(x, 0, AsciiCell.Create('#', Color.white, Color.black));
            asciiGrid.SetCell(x, gridHeight - 1, AsciiCell.Create('#', Color.white, Color.black));
        }
        
        // Left and right borders
        for (int y = 0; y < gridHeight; y++)
        {
            asciiGrid.SetCell(0, y, AsciiCell.Create('#', Color.white, Color.black));
            asciiGrid.SetCell(gridWidth - 1, y, AsciiCell.Create('#', Color.white, Color.black));
        }
    }

    private void DrawMessage()
    {
        if (string.IsNullOrEmpty(testMessage))
            return;
                
        // Calculate center position
        int centerY = gridHeight / 2;
        int startX = (gridWidth - testMessage.Length) / 2;
        
        // Ensure message fits
        if (startX < 1) startX = 1;
        if (startX + testMessage.Length >= gridWidth - 1)
            startX = gridWidth - testMessage.Length - 1;
        
        // Draw each character
        for (int i = 0; i < testMessage.Length; i++)
        {
            int x = startX + i;
            if (x > 0 && x < gridWidth - 1)
            {
                char ch = testMessage[i];
                asciiGrid.SetCell(x, centerY, AsciiCell.Create(ch, testColors[currentColorIndex], Color.black));
            }
        }
        
        Debug.Log($"AsciiGridSmokeTest: Drew message '{testMessage}' at position ({startX}, {centerY})");
    }

    private void ChangeMessageColor()
    {
        if (!messageDrawn) return;
        
        // Cycle to next color
        currentColorIndex = (currentColorIndex + 1) % testColors.Length;
        
        // Redraw message with new color
        DrawMessage();
        
        Debug.Log($"AsciiGridSmokeTest: Changed message color to {testColors[currentColorIndex]}");
    }

    public void SetTestMessage(string newMessage)
    {
        testMessage = newMessage;
        if (messageDrawn)
        {
            // Clear the message area and redraw
            ClearMessageArea();
            DrawMessage();
        }
    }

    private void ClearMessageArea()
    {
        // Clear the center row where messages are drawn
        int centerY = gridHeight / 2;
        for (int x = 1; x < gridWidth - 1; x++)
        {
            asciiGrid.SetCell(x, centerY, AsciiCell.Create('.', Color.gray, Color.black));
        }
    }

    public AsciiGrid GetAsciiGrid()
    {
        return asciiGrid;
    }
}
