// DrawUtil.cs
// Solar Dynamics 2026

#region

using UnityEngine;

#endregion

namespace Solar.UI.Util;

public static class DrawUtil
{
    private static Sprite? _antialiasedSprite;

    public static HUDAppManager GetHUDAppManager()
    {
        return SceneSingleton<HUDAppManager>.i;
    }

    public static Sprite GetAntialiasedSprite()
    {
        if (_antialiasedSprite != null) return _antialiasedSprite!;

        const int SIZE = 64; // 64x64 texture with a 1-pixel transparent border for anti-aliasing

        Color fill = Color.white;
        Color border = Color.white;

        Texture2D tex = new(SIZE, SIZE, TextureFormat.RGBA32, false);

        // byte[] data = new byte[SIZE * SIZE * 4];
        // Fix George's shenanigan one day.

        for (int x = 0; x < SIZE; x++)
        for (int y = 0; y < SIZE; y++)
        {
            bool isEdge = x == 0 || x == SIZE - 1 || y == 0 || y == SIZE - 1;
            tex.SetPixel(x, y, isEdge ? border : fill);
        }

        tex.Apply();

        _antialiasedSprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));

        Plugin.Log("Created antialiased sprite.");

        return _antialiasedSprite;
    }
}