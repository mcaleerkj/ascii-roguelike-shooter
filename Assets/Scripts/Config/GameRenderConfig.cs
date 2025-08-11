using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "GameRenderConfig", menuName = "Config/Game Render Config")]
public class GameRenderConfig : ScriptableObject
{
    [Header("Reference Resolution")]
    public int referenceWidth = 384;
    public int referenceHeight = 216;

    [Header("Grid / Cell Settings")]
    public int pixelsPerCell = 8;

    [Header("Font Settings")]
    public TMP_FontAsset fontAsset; // Optional, assign your ASCII font here
}
