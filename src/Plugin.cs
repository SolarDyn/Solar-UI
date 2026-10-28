// Plugin.cs
// Solar Dynamics 2026

#region

using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using Solar.UI.Chud;
using Solar.UI.Input;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static readonly bool IsDebug = true;
    internal static ManualLogSource? LoggerCache { get; private set; }

    internal static List<string> ModLabels = [];

    private void Awake()
    {
        Instance = this;

        LoggerCache ??= Logger;

        LoggerCache.LogMessage("Version " + PluginVersion + " loading...");

        ModLabels.Add($"{PluginName}  |  {PluginGUID} v{PluginVersion}");

        AssetsPlugin.Install();

        RewiredCategories.Install(Logger);

        StartCoroutine(RewiredCategories.RewiredReadyCoroutine());

        // BindingHandler.Initialize(LoggerCache);

        // Harmony.PatchAll();

        CollimatedHUDPlugin.Install();

        SceneManager.sceneLoaded += OnSceneLoaded;

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

    // Testing Shit

    public const string AvionicsMap = "Avionics";
    public const string SystemsCategory = "AircraftSystems";
    public const string SensorsCategory = "Sensors";
    // public const string ExampleSystemAction = "ExampleSystem";
    // public const string ExampleSensorAction = "ExampleSensor";

    // Map-category, action-category, and action IDs occupy separate namespaces.
    public const int AvionicsMapId = 32;
    public const int SystemsCategoryId = 256;
    public const int SensorsCategoryId = 257;
    // public const int ExampleSystemActionId = 128;
    // public const int ExampleSensorActionId = 129;

    private int _flightMapId = -1;
    private int _keyboardLayoutId;
    private int _mouseLayoutId;
    private int _joystickLayoutId;

    private void DefineCategories(RewiredCategories.Definitions data)
    {
        data.AddMapCategory(AvionicsMapId, AvionicsMap, "Avionics");
        data.AddActionCategory(SystemsCategoryId, SystemsCategory, "Aircraft Systems");
        data.AddActionCategory(SensorsCategoryId, SensorsCategory, "Sensors");

        // These give the example categories an assignable row. They have no default binding.
        // Replace these calls with your own registrar if you already create actions elsewhere.
        // data.AddButtonAction(ExampleSystemActionId, ExampleSystemAction,
        //     "Example Aircraft System", SystemsCategory);
        // data.AddButtonAction(ExampleSensorActionId, ExampleSensorAction,
        //     "Example Sensor Control", SensorsCategory);

        // Optional definition ordering/labels; UI order is configured separately below.
        // data.OrderActionCategories("Flight", SystemsCategory, SensorsCategory, "Camera");
        // data.RenameActionCategory("Flight", "Flight Controls");
        // data.RenameMapCategory(AvionicsMap, "Displays and Sensors");

        data.Dump(line => Logger.LogInfo(line));
    }

    private void ConfigureMapper(RewiredCategories.Layout layout)
    {
        // Inspect this output for the actual vanilla map-category scripting names.
        layout.Dump(line => Logger.LogInfo(line));
        layout.ShowCategoryHeadings();

        // This new tab displays our Sensors action category.
        layout.SetCategories(AvionicsMap, SensorsCategory);

        // Flight is an ACTION category from your earlier Nuclear Option input dump.
        // Discover the map category containing it instead of assuming a vanilla tab name.
        string? flightTab = layout.FindTabShowingCategory("Flight");
        if (flightTab != null)
        {
            _flightMapId = ReInput.mapping.GetMapCategoryId(flightTab);
            layout.AddCategories(flightTab, SystemsCategory);
            layout.Dump(line => Logger.LogInfo(line));
            layout.MoveCategoryBefore(flightTab, SystemsCategory, "Flight");
            layout.MoveTabBefore(AvionicsMap, flightTab);

            // Other ordering options (names must already be displayed):
            // layout.OrderTabs(flightTab, AvionicsMap); // all other tabs follow, unchanged
            // layout.OrderCategories(flightTab, "Flight", SystemsCategory);
        }
        else
        {
            // The bundled mapper may use a different setup. Still expose both custom categories.
            layout.SetCategories(AvionicsMap, SystemsCategory, SensorsCategory);
            Logger.LogWarning("No tab displays the Flight action category. Check the mapper dump " +
                "and select an existing gameplay map ID for the live-map enable policy.");
        }

        _keyboardLayoutId = layout.KeyboardLayoutId;
        _mouseLayoutId = layout.MouseLayoutId;
        _joystickLayoutId = layout.JoystickLayoutId;
        //EnsureAvionicsMaps();
    }

    private void Update()
    {
        // New map categories need live controller maps as well as definition/UI entries.
        // Checking assigned devices also covers reconnects and resets that remove these maps.
        // EnsureAvionicsMaps();

        if (!ReInput.isReady || !RewiredCategories.DefinitionsReady) return;
        Player player = ReInput.players.GetPlayer(0);
        // if (player.GetButtonDown(ExampleSystemActionId))
        //     Logger.LogInfo("Example system input received.");
        // if (player.GetButtonDown(ExampleSensorActionId))
        //     Logger.LogInfo("Example sensor input received.");
    }

    private void EnsureAvionicsMaps()
    {
        if (!ReInput.isReady || !RewiredCategories.DefinitionsReady || _flightMapId < 0) return;
        Player player = ReInput.players.GetPlayer(0);
        if (player.controllers.hasKeyboard)
            SyncMap(player, ControllerType.Keyboard, ReInput.controllers.Keyboard.id, _keyboardLayoutId);
        if (player.controllers.hasMouse)
            SyncMap(player, ControllerType.Mouse, ReInput.controllers.Mouse.id, _mouseLayoutId);
        foreach (Joystick joystick in player.controllers.Joysticks)
            SyncMap(player, ControllerType.Joystick, joystick.id, _joystickLayoutId);
    }

    private void SyncMap(Player player, ControllerType type, int controllerId, int layoutId)
    {
        ControllerMap source = player.controllers.maps.GetFirstMapInCategory(type, controllerId, _flightMapId);
        ControllerMap target = RewiredCategories.EnsureControllerMap(
            player, type, controllerId, AvionicsMapId, layoutId);

        // Mirror the game's existing flight-map enable state for this device.
        // Select a different source or add additional gating if your input context requires it.
        target.enabled = source != null && source.enabled;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(UpdateModLabel());
    }

    private static IEnumerator UpdateModLabel()
    {
        GameObject canvas = GameObject.Find("MainCanvas");
        if (!canvas)
        {
            LoggerCache!.LogWarning("MainCanvas not found.");
            yield break;
        }

        GameObject modLabel = GameObject.Find("ModLabel");
        if (!modLabel)
        {
            modLabel = CreateModLabel(canvas.transform);
        }

        modLabel.GetComponent<TextMeshProUGUI>().text = string.Join("\n", ModLabels);
    }

    private static GameObject CreateModLabel(Transform parent)
    {
        GameObject modLabel = new("ModLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = modLabel.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = modLabel.GetComponent<TextMeshProUGUI>();

        modLabel.transform.SetParent(parent, false);

        rect.anchoredPosition = new Vector2(-10f, 10f);
        rect.anchorMin = new Vector2(1f, 0.0f);
        rect.anchorMax = new Vector2(1f, 0.0f);
        rect.pivot = new Vector2(1f, 0.0f);
        rect.sizeDelta = new Vector2(400f, 50f);

        tmp.colorGradient = new VertexGradient(Color.white, Color.white, Color.grey, Color.grey);
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.fontSize = 12;
        tmp.font = Assets.TMP_FontAssets.Get("Brass Mono Regular");

        return modLabel;
    }
}