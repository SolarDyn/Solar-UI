// Assets.cs
// Solar Dynamics 2026

#region

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using NuclearOption.UIStyleSystem;
using TMPro;
using UnityEngine;

#endregion

namespace Solar.UI;

[HarmonyPatch(typeof(MainMenu), "Start")]
internal class AssetsPlugin
{
    private static bool _initialized;

    [UsedImplicitly]
    private static void Postfix()
    {
        if (_initialized)
            return;

        Assets.LoadStyleLabels();
        Assets.LoadFontAssets();

        _initialized = true;
    }
}

public static class Assets
{
    public static TMP_FontAsset[] Fonts;
    public static TMP_FontAsset FontDefaultHUD;

    public static Material[] Materials;
    public static Material MaterialDefaultHUD;

    public static readonly Dictionary<ThemeManager.ThemeContext, Dictionary<string, StyleLabel>> StyleLabels = new();

    public static void LoadStyleLabels()
    {
        StyleLabels[ThemeManager.ThemeContext.HUD] = new Dictionary<string, StyleLabel>();
        StyleLabels[ThemeManager.ThemeContext.Menu] = new Dictionary<string, StyleLabel>();
        StyleLabels[ThemeManager.ThemeContext.TacScreen] = new Dictionary<string, StyleLabel>();

        Plugin.Log("Loading HUD labels");
        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/HUD"))
        {
            Plugin.Log($"Loading style label {styleLabel.name}");
            StyleLabels[ThemeManager.ThemeContext.HUD].Add(styleLabel.name, styleLabel);
        }

        Plugin.Log("Loading Menu labels");
        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/Menu"))
        {
            Plugin.Log($"Loading style label {styleLabel.name}");
            StyleLabels[ThemeManager.ThemeContext.Menu].Add(styleLabel.name, styleLabel);
        }

        Plugin.Log("Loading TacScreen labels");
        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/TacScreen"))
        {
            Plugin.Log($"Loading style label {styleLabel.name}");
            StyleLabels[ThemeManager.ThemeContext.TacScreen].Add(styleLabel.name, styleLabel);
        }
    }

    public static void LoadFontAssets()
    {
        Fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        Plugin.Log($"Found {Fonts.Length} font assets.");
        foreach (TMP_FontAsset fontAsset in Fonts)
        {
            Plugin.Log($"Loading font asset: {fontAsset.name}");
            if (fontAsset.name == "Brass Mono Regular")
            {
                Plugin.Log($"FOUND: {fontAsset.name}");
                FontDefaultHUD = fontAsset;
            }
        }

        Materials = Resources.FindObjectsOfTypeAll<Material>();
        Plugin.Log($"Found {Materials.Length} materials.");
        foreach (Material material in Materials)
        {
            Plugin.Log($"Loading material: {material.name}");
            if (material.name == "Brass Mono Regular Thicker Additive")
            {
                Plugin.Log($"FOUND: {material.name}");
                MaterialDefaultHUD = material;
            }
        }
    }

    public static TMP_FontAsset GetDefaultFont()
    {
        return FontDefaultHUD;
    }
}