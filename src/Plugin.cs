// Plugin.cs
// Solar Dynamics 2026

#region

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

#endregion

namespace Solar.UI;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string PluginGUID = MyPluginInfo.PLUGIN_GUID;
    internal const string PluginName = MyPluginInfo.PLUGIN_NAME;
    internal const string PluginVersion = MyPluginInfo.PLUGIN_VERSION;

    internal static Plugin Instance = null!;
    internal static readonly Harmony Harmony = new(PluginGUID);

    public static readonly bool IsDebug = PluginGUID.EndsWith("Debug");
    internal static ManualLogSource? LoggerCache { get; private set; }

    private void Awake()
    {
        Instance = this;

        LoggerCache ??= Logger;

        LoggerCache.LogMessage("Version " + PluginVersion + " loading...");

        Harmony.PatchAll();

        LoggerCache.LogMessage("Loaded.");
    }

    private void OnDestroy()
    {
        LoggerCache ??= Logger;

        LoggerCache.LogMessage("Unloading...");

        Harmony?.UnpatchSelf();

        LoggerCache.LogMessage("Unloaded.");
    }

    internal static void Log(string message)
    {
        if (IsDebug) LoggerCache?.LogDebug(message);
    }
}