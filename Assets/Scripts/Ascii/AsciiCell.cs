using UnityEngine;

public struct AsciiCell
{
    public char ch;
    public Color32 fg;
    public Color32 bg;

    public AsciiCell(char character, Color32 foreground, Color32 background)
    {
        ch = character;
        fg = foreground;
        bg = background;
    }

    public static AsciiCell Empty => new AsciiCell('\u00A0', Color.white, Color.clear);

    public static AsciiCell Create(char character, Color32 foreground, Color32 background)
    {
        return new AsciiCell(character, foreground, background);
    }
}
