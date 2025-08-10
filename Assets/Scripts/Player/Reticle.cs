using UnityEngine;
using TMPro;

[ExecuteAlways, DisallowMultipleComponent]
public class Reticle : MonoBehaviour
{
    [Header("Visuals")]
    public TMP_FontAsset fontAsset;
    public int pixelsPerCell = 8; // visual scale, not position
    public char glyph = '•';
    public Color32 color = new Color32(255,255,255,255);

    private TextMeshPro tmp;

    void Awake() { EnsureTMP(); ApplyStyle(); }
    void OnValidate() { EnsureTMP(); ApplyStyle(); }

    void EnsureTMP()
    {
        // Kill any RectTransform/UGUI leftovers
        var r = GetComponent<RectTransform>();
        if (r) { var t = gameObject.AddComponent<Transform>(); t.SetPositionAndRotation(r.position, r.rotation); t.localScale = r.localScale; DestroyImmediate(r); }

        if (!tmp)
        {
            tmp = GetComponentInChildren<TextMeshPro>();
            if (!tmp)
            {
                var go = new GameObject("ReticleText", typeof(TextMeshPro));
                go.transform.SetParent(transform, false);
                tmp = go.GetComponent<TextMeshPro>();
            }
        }
    }

    void ApplyStyle()
    {
        if (!tmp) return;
        if (fontAsset) tmp.font = fontAsset;

        tmp.text = glyph.ToString();
        tmp.fontSize = 1f; // scale by transform
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.richText = false;
        tmp.color = color;

        // Scale glyph so one char ~= one cell; assumes PPU=48 and 8px target cell => 0.16666667 world scale
        // If you already use PixelMath, you can swap to compute exact scale; otherwise keep transform scale = 1 and let parent handle world placement.
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = Vector3.one; // parent/anchor dictates world placement
    }

    public void SetVisible(bool v) { if (tmp) tmp.gameObject.SetActive(v); }
    public void SetGlyph(char c) { glyph = c; if (tmp) tmp.text = c.ToString(); }
    public void SetColor(Color32 c) { color = c; if (tmp) tmp.color = c; }
}
