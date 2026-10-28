// Assets.cs
// Solar Dynamics 2026

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using NuclearOption.UIStyleSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

#endregion

namespace Solar.UI;

internal static class AssetsPlugin
{
    private const string PatchId = "Solar.UI.Assets";
    private static bool _initialized;

    public static void Install()
    {
        Harmony harmony = new(PatchId);

        try
        {
            MethodInfo info_MainMenu_Start = AccessTools.Method(typeof(MainMenu), "Start", Type.EmptyTypes);
            HarmonyMethod method_MainMenu_Start = new(typeof(AssetsPlugin), nameof(MainMenu_Start_Postfix)){priority = Priority.High};
            harmony.Patch(info_MainMenu_Start, postfix: method_MainMenu_Start);
        }
        catch (Exception e)
        {
            Plugin.Log($"AssetsPlugin failed to install:\n{e}");
        }
    }

    private static void MainMenu_Start_Postfix(MainMenu __instance)
    {
        if (_initialized)
            return;

        _initialized = true;

        Assets.LoadResources();
        Assets.SetDefaults();
        // This is done automatically when needed.
        Assets.LoadAssets();

        Plugin.Log("Assets loaded.");
    }
}

public static class Assets
{
    // AssetCollections
    public static AssetCollection<AudioClip> AudioClips = new();
    public static AssetCollection<TMP_FontAsset> TMP_FontAssets = new();
    public static AssetCollection<Material> Materials = new();
    public static AssetCollection<Texture2D> Texture2Ds = new();
    public static AssetCollection<Sprite> Sprites = new();
    public static AssetCollection<Shader> Shaders = new();
    public static AssetCollection<AudioMixer> AudioMixers = new();
    public static AssetCollection<GameObject> GameObjects = new();

    // NuclearOption/Resources
    public static Dictionary<string, ThemeGroup> ThemeGroups = new();
    public static readonly Dictionary<ThemeManager.ThemeContext, Dictionary<string, StyleLabel>> StyleLabels = new();

    // Defaults
    public static TMP_FontAsset FontDefaultHUD;
    public static Material MaterialDefaultHUD;

    public static void LoadAssets()
    {
        AudioClips.Load();
        TMP_FontAssets.Load();
        Materials.Load();
        Texture2Ds.Load();
        Sprites.Load();
        Shaders.Load();
        AudioMixers.Load();
        GameObjects.Load();
    }

    public static void LoadResources()
    {
        foreach (ThemeGroup themeGroup in Resources.LoadAll<ThemeGroup>("StyleSystem/ThemeGroups"))
            ThemeGroups.Add(themeGroup.name, themeGroup);

        // Plugin.Log($"Resources<{typeof(ThemeGroup).FullName}> : Loaded {ThemeGroups.Count} assets.");

        StyleLabels[ThemeManager.ThemeContext.HUD] = new Dictionary<string, StyleLabel>();
        StyleLabels[ThemeManager.ThemeContext.Menu] = new Dictionary<string, StyleLabel>();
        StyleLabels[ThemeManager.ThemeContext.TacScreen] = new Dictionary<string, StyleLabel>();

        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/HUD"))
            StyleLabels[ThemeManager.ThemeContext.HUD].Add(styleLabel.name, styleLabel);
        // Plugin.Log($"Resources<{typeof(StyleLabel).FullName}[{nameof(NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD)}]> : Loaded {StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD].Count} assets.");
        // foreach ((string key, StyleLabel value) in StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD])
        //     Plugin.Log($" -> {key} / {value.displayLabel}: {value}");


        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/Menu"))
            StyleLabels[ThemeManager.ThemeContext.Menu].Add(styleLabel.name, styleLabel);
        // Plugin.Log($"Resources<{typeof(StyleLabel).FullName}[{nameof(NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.Menu)}]> : Loaded {StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.Menu].Count} assets.");
        // foreach ((string key, StyleLabel value) in StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.Menu])
        //     Plugin.Log($" -> {key} / {value.displayLabel}: {value}");

        foreach (StyleLabel styleLabel in Resources.LoadAll<StyleLabel>("StyleSystem/Labels/TacScreen"))
            StyleLabels[ThemeManager.ThemeContext.TacScreen].Add(styleLabel.name, styleLabel);
        // Plugin.Log($"Resources<{typeof(StyleLabel).FullName}[{nameof(NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.TacScreen)}]> : Loaded {StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.TacScreen].Count} assets.");
        // foreach ((string key, StyleLabel value) in StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.TacScreen])
        //     Plugin.Log($" -> {key} / {value.displayLabel}: {value}");
    }

    public static void SetDefaults()
    {
        FontDefaultHUD = TMP_FontAssets.Get("Brass Mono Regular");
        MaterialDefaultHUD = Materials.Get("Brass Mono Regular Thicker Additive");
        // Also Assets.Materials.Get("Text_additive") is good for CHUD.
    }

    [Obsolete("Most likely going to remove this. It's just not needed.")]
    public static TMP_FontAsset GetDefaultFont()
    {
        return FontDefaultHUD;
    }

    public static IEnumerable<T> GetPrefabs<T>()
        where T : Component
    {
        return Resources.FindObjectsOfTypeAll<T>().Where(component => !component.gameObject.scene.IsValid());
    }

    public class AssetCollection<T> where T : Object
    {
        private Dictionary<string, T>? dict_cache;

        [UsedImplicitly]
        public Dictionary<string, T> dict
        {
            get
            {
                if (dict_cache is null)
                    Load();
                return dict_cache!;
            }
        }

        [UsedImplicitly]
        public void Load()
        {
            dict_cache = new Dictionary<string, T>();

            foreach (T obj in Resources.FindObjectsOfTypeAll<T>())
            {
                if (dict_cache.ContainsKey(obj.name))
                    // Plugin.Log($"AssetCollection<{typeof(T)}>   {$"\"{obj.name}\"",-50} [Duplicate]");
                    continue;

                // Plugin.Log($"AssetCollection<{typeof(T)}> + \"{obj.name}\"");
                dict_cache.Add(obj.name, obj);
            }

            // Plugin.Log($"AssetCollection<{typeof(T)}> : Loaded {dict.Count} assets.");
        }

        [UsedImplicitly]
        public bool TryGet(string name, out T asset)
        {
            return dict.TryGetValue(name, out asset);
        }

        [UsedImplicitly]
        public T Get(string name)
        {
            if (!TryGet(name, out T asset))
                throw new KeyNotFoundException($"{typeof(T)} asset not found: {name}");
            return asset;
        }
    }
}

public static class AssetsVanilla
{
    public static class Audio
    {
        public static AudioMixer MasterAudioMixer;

        static Audio()
        {
            MasterAudioMixer = Assets.AudioMixers.Get("MasterAudioMixer");
        }

        public static class Group
        {
            public static AudioMixerGroup Master;
            public static AudioMixerGroup Effects;
            public static AudioMixerGroup Effects_General;
            public static AudioMixerGroup Effects_Heavy;
            public static AudioMixerGroup Interface;
            public static AudioMixerGroup RadarWarning;
            public static AudioMixerGroup MissileAlert;
            public static AudioMixerGroup JammedNoise;
            public static AudioMixerGroup Menu;
            public static AudioMixerGroup Music;

            static Group()
            {
                Master = MasterAudioMixer.FindMatchingGroups("Master")[0];
                Effects = MasterAudioMixer.FindMatchingGroups("Effects")[0];
                Effects_General = MasterAudioMixer.FindMatchingGroups("Effects_General")[0];
                Effects_Heavy = MasterAudioMixer.FindMatchingGroups("Effects_Heavy")[0];
                Interface = MasterAudioMixer.FindMatchingGroups("Interface")[0];
                RadarWarning = MasterAudioMixer.FindMatchingGroups("RadarWarning")[0];
                MissileAlert = MasterAudioMixer.FindMatchingGroups("MissileAlert")[0];
                JammedNoise = MasterAudioMixer.FindMatchingGroups("JammedNoise")[0];
                Menu = MasterAudioMixer.FindMatchingGroups("Menu")[0];
                Music = MasterAudioMixer.FindMatchingGroups("Music")[0];
            }
        }
    }
}