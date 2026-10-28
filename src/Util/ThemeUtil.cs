// ThemeManager.cs
// Solar Dynamics 2026

using Rewired.UI.ControlMapper;
using UnityEngine;

namespace Solar.UI.Util;

public static class ThemeUtil
{
    public static Color RGB(byte r, byte g, byte b, byte a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color HSL(float h, float s, float l, float a = 1f)
    {
        if (s <= 0)
            return new Color(l, l, l, a);
        float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
        float p = 2 * l - q;
        return new Color(HueToRgb(p, q, h + 1f / 3f), HueToRgb(p, q, h), HueToRgb(p, q, h - 1f / 3f), a);
    }

    public static float HueToRgb(float p, float q, float t)
    {
        if (t < 0) t++;
        if (t > 1) t--;
        return t switch
        {
            < 1f / 6f => p + (q - p) * 6 * t,
            < 1f / 2f => q,
            < 2f / 3f => p + (q - p) * (2f / 3f - t) * 6,
            _ => p,
        };
    }

    public struct MenuTheme
    {
        public Color menuButton;
        public Color menuButtonDisabled;
        public Color menuButtonHighlighted;
        public Color menuButtonDisabledHighlighted;
        public Color menuButtonPressed;
        public Color menuButtonSelected;

        public void UpdateCustomColorBlock(ref ThemeSettings.CustomColorBlock block)
        {
            block.normalColor = menuButton;
            block.disabledColor = menuButtonDisabled;
            block.highlightedColor = menuButtonHighlighted;
            block.disabledHighlightedColor = menuButtonDisabledHighlighted;
            block.pressedColor = menuButtonPressed;
            block.selectedColor = menuButtonSelected;
        }
    }
}