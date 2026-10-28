// RewiredCategories.cs
// Solar Dynamics 2026

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using NuclearOption.UIStyleSystem;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using Rewired.UI.ControlMapper;
using Solar.UI.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace Solar.UI.Input;

public static class RewiredCategories
{
    private const string PatchId = "Solar.UI.Input.RewiredCategories";
    private static readonly ConditionalWeakTable<object, object> ConfiguredMappers = new();

    private static ManualLogSource? _log;
    private static List<Action<Definitions>> _actionDefinitions = [];
    private static List<Action<Layout>> _actionLayouts = [];
    private static List<Action> _actionDefaults = [];
    private static Definitions? _definitions;
    private static bool _installed;
    private static bool _processed;

    public static bool DefinitionsReady => _definitions != null;

    public static void RegisterDefinition(Action<Definitions> action)
    {
        if (_processed) throw new InvalidOperationException("Register Input configurations from your Plugin.Awake().");
        _actionDefinitions.Add(action);
        _log!.LogInfo($"Registered Definition: {action.Method.Module.Assembly.FullName}:{action.Method.Name}");
    }

    public static void RegisterLayout(Action<Layout> action)
    {
        if (_processed) throw new InvalidOperationException("Register Input configurations from your Plugin.Awake().");
        _actionLayouts.Add(action);
        _log!.LogInfo($"Registered Layout: {action.Method.Module.Assembly.FullName}:{action.Method.Name}");
    }

    public static void RegisterDefaults(Action action)
    {
        if (_processed) throw new InvalidOperationException("Register Input configurations from your Plugin.Awake().");
        _actionDefaults.Add(action);
        _log!.LogInfo($"Registered Defaults: {action.Method.Module.Assembly.FullName}:{action.Method.Name}");
    }

    /// <summary>
    ///     Call once from the shared input library's plugin Awake, before Rewired starts.
    ///     This helper installs its own patches; do not additionally PatchAll this class.
    ///     Configuration callbacks must be deterministic and repeatable across scenes.
    /// </summary>
    public static void Install(
        ManualLogSource logger)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_installed) throw new InvalidOperationException("RewiredCategories is already installed.");
        if (ReInput.isReady)
            throw new InvalidOperationException(
                "Install RewiredCategories before Rewired initializes; restart after installing the patch.");


        MethodInfo methodManagerAwake = AccessTools.Method(typeof(InputManager_Base), "Awake", Type.EmptyTypes)
                                        ?? throw new MissingMethodException(typeof(InputManager_Base).FullName, "Awake()");

        MethodInfo methodMapperInitialize = AccessTools.Method(typeof(ControlMapper), "Initialize", Type.EmptyTypes)
                                            ?? throw new MissingMethodException(typeof(ControlMapper).FullName, "Initialize()");

        Harmony harmony = new(PatchId);

        try
        {
            harmony.Patch(
                methodManagerAwake,
                prefix: new HarmonyMethod(typeof(RewiredCategories), nameof(BeforeInputManagerAwake)) { priority = Priority.First },
                postfix: new HarmonyMethod(typeof(RewiredCategories), nameof(AfterInputManagerAwake)) { priority = Priority.First }
                );

            harmony.Patch(
                methodMapperInitialize,
                prefix: new HarmonyMethod(typeof(RewiredCategories), nameof(BeforeMapperInitialize)) { priority = Priority.Last },
                postfix: new HarmonyMethod(typeof(RewiredCategories), nameof(AfterMapperInitialize)) { priority = Priority.Normal }
                );

            RewiredDefaultBindings.Install(_log!);

            _installed = true;
        }
        catch
        {
            harmony.UnpatchSelf();
            throw;
        }
    }

    private static void BeforeInputManagerAwake(InputManager_Base __instance)
    {
        _processed = true;

        // Plugin.Log("BeforeInputManagerAwake");

        UserData userData = __instance.userData;
        Definitions definitions = new(userData);

        foreach (Action<Definitions> action in _actionDefinitions)
        {
            try
            {
                action(definitions);
            }
            catch (Exception e)
            {
                _log!.LogError($"Rewired category definition setup failed ({action.Method.Module.Assembly.FullName}.{action.Method.Name}):\n" + e);
            }
        }

        InputStateConfig.Save();

        BindingHandler.RegisterActions(__instance, definitions);

        _definitions = definitions;

        definitions.Commit();

        foreach (ActionCategoryMap.Entry entry in userData.actionCategoryMap.list)
        {
            _log!.LogInfo(
                $"Category map [{entry.categoryId}]: " +
                $"[{string.Join(", ", entry.ActionIds)}]");
        }
    }

    private static void AfterInputManagerAwake(InputManager_Base __instance)
    {
        BindingHandler.RegisterStates();
    }

    private static void BeforeMapperInitialize(ControlMapper __instance)
    {
        // Plugin.Log("BeforeMapperInitialize");
        if (!ReInput.isReady || _definitions == null ||
            ConfiguredMappers.TryGetValue(__instance, out _)) return;

        // _log!.LogWarning(__instance.themeSettings);

        __instance._ignoreMouseXAxisAssignment = false;
        __instance._ignoreMouseYAxisAssignment = false;

        __instance._showFullAxisInputFields = true;
        __instance._showSplitAxisInputFields = true;

        __instance._showSettingsGroupLabel = true;
        // __instance._showInputBehaviorSettings = true;

        __instance._keyboardColMaxWidth = 250;
        __instance._mouseColMaxWidth = 125;
        __instance._controllerColMaxWidth = 250;
        __instance._controllerInputFieldCount = 2;
        __instance._inputRowHeight = 40;
        TextMeshProUGUI _textLabel = __instance.prefabs._inputGridLabel.GetComponent<TextMeshProUGUI>();
        _textLabel.fontSize = 24f;
        // _textLabel.alignment = TextAlignmentOptions.Right;


        //
        //
        // // Optional layout changes:
        // __instance._inputRowHeight = 32;
        // __instance._inputRowCategorySpacing = 12;
        // __instance._actionLabelWidth = 340;

        // foreach (string categoryName in new[] { "AircraftSystems", "Flight" })
        // {
        //     InputActionCategory category =
        //         (InputActionCategory) ReInput.mapping.GetActionCategory(categoryName);
        //
        //     if (category == null)
        //     {
        //         _log!.LogError($"Runtime category missing: {categoryName}");
        //         continue;
        //     }
        //
        //     foreach (bool sorted in new[] { false, true })
        //     {
        //         string actions = string.Join(", ",
        //             ReInput.mapping
        //                 .UserAssignableActionsInCategory(category.id, sorted)
        //                 .Select(a => $"{a.id}:{a.name}"));
        //
        //         _log!.LogInfo(
        //             $"{categoryName} [{category.id}], sorted={sorted}: {actions}");
        //     }
        // }

        // foreach (string name in new[]
        //          {
        //              "AMSForward", "AMSAft", "AMSLeft", "AMSRight", "TriggerDetent"
        //          })
        // {
        //     InputAction action = ReInput.mapping.GetAction(name);
        //
        //     _log!.LogInfo(action == null
        //         ? $"Runtime action missing: {name}"
        //         : $"Runtime action: {action.id}:{action.name}, " +
        //           $"category={action.categoryId}, assignable={action.userAssignable}");
        // }

        try
        {
            Layout layout = new(__instance, _definitions);
            foreach (Action<Layout> action in _actionLayouts)
            {
                action(layout);
            }
            layout.Commit();
            ConfiguredMappers.Add(__instance, new object());
        }
        catch (Exception e)
        {
            _log!.LogError("Rewired Control Mapper layout setup failed:\n" + e);
        }
    }

    private static void AfterMapperInitialize(ControlMapper __instance)
    {
        // __instance.prefabs.window.scene.name;

        // Theme Changes
        TMP_FontAsset font = Assets.TMP_FontAssets.Get("Brass Mono Regular");

        ThemeSettings theme =
            Object.Instantiate(__instance._themeSettings);

        Sprite outlineButton = Object.Instantiate(Assets.Sprites.Get("outline"));
        outlineButton.name = "OutlineButton";

        // theme._buttonSettings.imageSettings._sprite = outlineButton;

        ThemeUtil.MenuTheme menuTheme = new()
        {
            menuButton = ThemeUtil.HSL(0.3333f, 0.00f, 0.30f),
            menuButtonDisabled = ThemeUtil.HSL(0.3333f, 0.00f, 0.15f),
            menuButtonHighlighted = ThemeUtil.HSL(0.3333f, 0.90f, 0.50f),
            menuButtonDisabledHighlighted = ThemeUtil.HSL(0.1000f, 0.90f, 0.20f),
            menuButtonPressed = ThemeUtil.HSL(0.5000f, 0.90f, 0.50f),
            menuButtonSelected = ThemeUtil.HSL(0.3333f, 0.90f, 0.30f),
        };

        ThemeUtil.MenuTheme buttonTheme = new()
        {
            menuButton = ThemeUtil.HSL(0.3333f, 0.00f, 0.20f),
            menuButtonDisabled = ThemeUtil.HSL(0.3333f, 0.00f, 0.10f),
            menuButtonHighlighted = ThemeUtil.HSL(0.3333f, 0.90f, 0.50f),
            menuButtonDisabledHighlighted = ThemeUtil.HSL(0.3333f, 0.90f, 0.30f),
            menuButtonPressed = ThemeUtil.HSL(0.5000f, 0.90f, 0.50f),
            menuButtonSelected = ThemeUtil.HSL(0.3333f, 0.90f, 0.30f),
        };

        // ThemeSettings.CustomColorBlock colors = theme._selectableSettings._colors;
        // colors.normalColor = Color.white;
        // colors.highlightedColor = Color.green;
        // colors.selectedColor = Color.yellow;
        // colors.pressedColor = Color.cyan;
        // colors.disabledColor = Color.grey;

        buttonTheme.UpdateCustomColorBlock(ref theme._buttonSettings._colors);
        menuTheme.UpdateCustomColorBlock(ref theme._selectableSettings._colors);
        menuTheme.UpdateCustomColorBlock(ref theme._scrollbarSettings._colors);
        menuTheme.UpdateCustomColorBlock(ref theme._inputGridFieldSettings._colors);

        theme.name = "Solar UI";
        theme._mainWindowBackground._sprite = Assets.Sprites.Get("LoadingScreenRevoker2");
        theme._buttonTextSettings._font = font;
        theme._buttonTextSettings._color = Color.green;
        theme._textSettings._font = font;
        theme._textSettings._color = Color.white;
        theme._inputGridFieldTextSettings._font = font;
        theme._inputGridFieldTextSettings._color = Color.green;
        // theme._textSettings._sizeMultiplier = 1f;
        // theme._textSettings._lineSpacing = 0f;

        // Inspect the theme's serialized settings.

        // Change the cloned theme's color/font/style settings here.

        __instance._themeSettings = theme;
        __instance._useThemeSettings = true;
        // __instance._inputRowHeight = 30;
        __instance._inputRowCategorySpacing = 20;
        __instance._actionLabelWidth = 600;
        __instance._showActionCategoryLabels = true;

        HorizontalLayoutGroup _layoutInnerGroup = __instance.references._inputGridInnerGroup.GetComponent<HorizontalLayoutGroup>();
        _layoutInnerGroup.childForceExpandWidth = true;
        _layoutInnerGroup.spacing = 20f;

        HorizontalLayoutGroup _layoutHeadersGroups = __instance.references._inputGridHeadersGroup.GetComponent<HorizontalLayoutGroup>();
        _layoutHeadersGroups.childForceExpandWidth = true;
        _layoutHeadersGroups.spacing = 20f;

        LayoutElement _layoutElement = __instance.references._inputGridContainer.gameObject.GetComponent<LayoutElement>();
        _layoutElement.flexibleWidth = 1f;

        // __instance.references._inputGridContainer.


        __instance.InspectorPropertyChanged();
    }

    internal static IEnumerator RewiredReadyCoroutine()
    {
        _log!.LogDebug("Defaults: Waiting for Rewired to be ready...");

        yield return new WaitUntil(() => ReInput.isReady);

        _log!.LogDebug("Defaults: Rewired is ready!");

        foreach (Action action in _actionDefaults)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                _log!.LogError($"Rewired category default setup failed ({action.Method.Module.Assembly.FullName}.{action.Method.Name}):\n" + e);
            }
        }
    }

    public static void SetMapEnabled(string map, bool enabled)
    {
        Player player = ReInput.players.GetPlayer(0);
        player.controllers.maps.SetMapsEnabled(enabled, map);
    }

    /// <summary>
    ///     Creates only a missing live controller map. Existing bindings and enabled
    ///     state are preserved. Call after saved controls have loaded, and again when
    ///     assigning/reconnecting controllers. Layout IDs must exist in Rewired.
    /// </summary>
    public static ControllerMap EnsureControllerMap(
        Player player, ControllerType controllerType, int controllerId,
        int mapCategoryId, int layoutId, bool initiallyEnabled = false)
    {
        if (!ReInput.isReady) throw new InvalidOperationException("Rewired is not ready.");
        ControllerMap map = player.controllers.maps.GetMap(
            controllerType, controllerId, mapCategoryId, layoutId);
        if (map != null) return map;

        player.controllers.maps.AddEmptyMap(controllerType, controllerId, mapCategoryId, layoutId);
        map = player.controllers.maps.GetMap(controllerType, controllerId, mapCategoryId, layoutId);
        if (map == null)
            throw new InvalidOperationException(
                $"Could not create map {mapCategoryId}/{layoutId} for {controllerType} {controllerId}.");
        map.enabled = initiallyEnabled;
        return map;
    }

    private static void ReplaceContents(IList target, IEnumerable values)
    {
        // _log!.LogInfo($"ReplaceContents({target}, {values})");
        // _log!.LogInfo($"  {target}[{target.Count}]");
        // foreach (object t in target) _log!.LogInfo($"   -> {t}");

        List<object> valuesList = [];
        foreach (object value in values)
            valuesList.Add(value);

        // _log!.LogInfo($"  {values}[{valuesList.Count}]");
        // foreach (object value in valuesList) _log!.LogInfo($"   -> {value}");

        if (target.IsReadOnly || target.IsFixedSize)
            throw new NotSupportedException("Expected a mutable Rewired definition list.");

        target.Clear();
        foreach (object value in valuesList) target.Add(value);
    }

    private static void OrderFirst<T>(List<T> values, IEnumerable<T> first)
    {
        List<T> prefix = first.Distinct().ToList();
        if (prefix.Any(v => !values.Contains(v)))
            throw new ArgumentException("Cannot order an entry that is not present in this list.");
        List<T> rest = values.Where(v => !prefix.Contains(v)).ToList();
        values.Clear();
        values.AddRange(prefix);
        values.AddRange(rest);
    }

    private static void MoveBefore<T>(List<T> values, T value, T before)
    {
        // Plugin.Log($"MoveBefore<{typeof(T)}>({values}, {value}, {before})");
        // Plugin.Log($"[{values.Count}/{values.Capacity}]");
        // foreach (T x in values) Plugin.Log($" > {x}");
        if (!values.Contains(value) || !values.Contains(before))
            throw new ArgumentException(
                $"MoveBefore<{typeof(T)}>({values}, {value}, {before}): Both entries must already be present in the list.");
        // Plugin.Log($"MoveBefore<{typeof(T)}>({values}, {value}, {before})");
        if (EqualityComparer<T>.Default.Equals(value, before)) return;
        values.Remove(value);
        values.Insert(values.IndexOf(before), value);
    }

    /// <summary>All category names below are scripting names, not display labels.</summary>
    public sealed class Definitions
    {
        private readonly UserData _userData;

        private readonly List<InputMapCategory> _sourceMaps;
        private readonly List<InputActionCategory> _sourceCategories;
        private readonly List<InputAction> _sourceActions;

        private readonly List<InputMapCategory> _maps;
        private readonly List<InputActionCategory> _categories;
        private readonly List<InputAction> _actions;
        private readonly List<InputAction> _newActions = [];

        private List<int> _usedActionIds;

        private bool _committed;

        internal Definitions(UserData userData)
        {
            _userData = userData;

            _sourceMaps = userData.mapCategories;
            _sourceCategories = userData.actionCategories;
            _sourceActions = userData.actions;

            // if (new[] { _sourceMaps, _sourceCategories, _sourceActions }
            //     .Any(l => l.IsReadOnly || l.IsFixedSize))
            //     throw new NotSupportedException("Expected mutable Rewired definition lists.");

            _maps = [.. _sourceMaps];
            _categories = [.. _sourceCategories];
            _actions = [.. _sourceActions];
        }

        public int GetMapCategoryId(string name)
        {
            return Find(_maps, name).id;
        }

        public int GetActionCategoryId(string name)
        {
            return Find(_categories, name).id;
        }

        public int GetInputBehaviorId(string name)
        {
            return _userData.inputBehaviors.Find(b => b.name == name).id;
        }

        public string GetMapCategoryName(int id)
        {
            return FindById(_maps, id).name;
        }

        public string GetActionCategoryName(int id)
        {
            return FindById(_categories, id).name;
        }

        public List<InputAction> Actions => _actions;

        /// <summary>Use a fixed ID; collisions throw rather than silently changing saved-control identity.</summary>
        public int AddMapCategory(int id, string name, string label, bool userAssignable = true)
        {
            Editing();
            InputMapCategory? existing = ValidateIdentity(_maps, id, name);
            if (existing != null) return existing.id;
            InputMapCategory category = (InputMapCategory)Members.Create(typeof(InputMapCategory));
            InitializeCategory(category, id, name, label, userAssignable);
            Members.Write(category, "checkConflictsWithAllCategories", true);
            _maps.Add(category);
            _log!.LogInfo($"+ Map Category: {name,20} -> [{string.Join(", ", _maps.Select(c => c.name))}]");
            return id;
        }

        public int AddActionCategory(int id, string name, string label, bool userAssignable = true)
        {
            Editing();
            InputActionCategory? existing = ValidateIdentity(_categories, id, name);
            if (existing != null) return existing.id;
            InputActionCategory category = (InputActionCategory)Members.Create(typeof(InputActionCategory));
            InitializeCategory(category, id, name, label, userAssignable);
            _categories.Add(category);
            _log!.LogInfo($"+ Action Category: {name,20} -> [{string.Join(", ", _categories.Select(c => c.name))}]");
            return id;
        }

        public int RegisterAction(
            int id,
            string name,
            string category,
            string label,
            string labelPos = "",
            string labelNeg = "",
            string behavior = "Default",
            InputActionType actionType = InputActionType.Button,
            bool userAssignable = true)
        {

            Editing();

            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));

            RequireText(name, "RegisterAction -> name");
            RequireText(label, "RegisterAction -> label");
            RequireText(category, "RegisterAction -> category");
            RequireText(behavior, "RegisterAction -> behavior");

            // InputActionCategory? targetCategory = _categories.Find(c => string.Equals(c.name, category, StringComparison.OrdinalIgnoreCase));
            // if (targetCategory == null)
            //     throw new InvalidOperationException($"Rewired action category '{category}' was not found.");

            int categoryId = GetActionCategoryId(category);
            int behaviorId = GetInputBehaviorId(behavior);

            InputAction? existing = null;
            foreach (InputAction action in _actions)
            {
                if (!string.Equals(action.name, name, StringComparison.Ordinal))
                    continue;

                if (existing != null)
                    throw new InvalidOperationException(
                        $"More than one Rewired action is named '{name}'.");

                existing = action;
            }

            if (existing != null)
            {
                if (existing.type != actionType ||
                    existing.categoryId != categoryId ||
                    existing.descriptiveName != label ||
                    existing.userAssignable != userAssignable)
                    throw new InvalidOperationException(
                        $"An existing Rewired action named '{name}' " +
                        "has a different type, category, display name, or assignability.");

                // Already injected into this UserData: retain its ID and map.
                // Do not run the custom configurator twice.
                id = existing.id;
                return id;
            }

            _usedActionIds = [.. _actions.Select(a => a.id)];
            while (_usedActionIds.Contains(id))
            {
                if (id == int.MaxValue)
                    throw new InvalidOperationException("No free Rewired action ID remains.");
                id++;
            }

            InputAction newAction = (InputAction)(Activator.CreateInstance(typeof(InputAction), true) ??
                                               throw new InvalidOperationException("Could not construct a Rewired InputAction."));

            newAction.id = id;
            newAction.name = name;
            newAction.descriptiveName = label;
            newAction.categoryId = categoryId;
            newAction.behaviorId = behaviorId;
            newAction.type = actionType;
            newAction.userAssignable = userAssignable;

            if (!string.IsNullOrEmpty(labelPos))
            {
                newAction.positiveDescriptiveName = labelPos;
            }

            if (!string.IsNullOrEmpty(labelNeg))
            {
                newAction.negativeDescriptiveName = labelNeg;
            }

            _actions.Add(newAction);
            _newActions.Add(newAction);

            _log!.LogInfo($"+ Action: {newAction.id,4} {newAction.name,32} ({category})");

            return id;
        }

        /// <summary>
        ///     Optional convenience for new button actions. Existing actions stay in their
        ///     original categories. Use your existing input registrar instead if preferred.
        /// </summary>
        // public int AddButtonAction(int id, string name, string label, string actionCategory,
        //     int inputBehaviorId = 0)
        // {
        //     Editing();
        //     if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
        //     if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Action name is required.");
        //     int categoryId = GetActionCategoryId(actionCategory);
        //     foreach (InputAction existing in _actions)
        //     {
        //         if (existing.name == name && existing.id == id && existing.categoryId == categoryId)
        //             return id;
        //         if (existing.name == name || existing.id == id)
        //             throw new InvalidOperationException(
        //                 $"Action identity conflicts with '{existing.name}' (ID {existing.id}).");
        //     }
        //     // Cat
        //     // InputAction action = (InputAction)Members.Create(typeof(InputAction));
        //     // Members.Write(action, "id", id);
        //     // Members.Write(action, "name", name);
        //     // Members.Write(action, "descriptiveName", label);
        //     // Members.Write(action, "categoryId", categoryId);
        //     // Members.Write(action, "type", InputActionType.Button);
        //     // Members.Write(action, "behaviorId", inputBehaviorId);
        //     // Members.Write(action, "userAssignable", true);
        //
        //     // Han
        //     InputAction action = (InputAction)(Activator.CreateInstance(typeof(InputAction), true) ??
        //                                        throw new InvalidOperationException("Could not construct a Rewired InputAction."));
        //
        //     action.id = id;
        //     action.name = name;
        //     action.descriptiveName = label;
        //     action.type = InputActionType.Button;
        //     action.categoryId = categoryId;
        //     action.behaviorId = inputBehaviorId;
        //     action.userAssignable = true;
        //
        //     _actions.Add(action);
        //     _newActions.Add(action);
        //     return id;
        // }

        private static void RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        private static void SetRequiredFields(
            InputAction action, InputBinding binding, int categoryId, int actionId)
        {
            action.id = actionId;
            action.name = binding.Name;
            action.type = binding.Type;
            action.descriptiveName = binding.Label;
            action.categoryId = categoryId;
            action._userAssignable = binding.UserAssignable;
        }

        public void RenameMapCategory(string name, string label)
        {
            Editing();
            int index = _maps.IndexOf(Find(_maps, name));
            InputMapCategory copy = (InputMapCategory)Members.Clone(_maps[index]);
            Members.Write(copy, "descriptiveName", label);
            _maps[index] = copy;
        }

        public void RenameActionCategory(string name, string label)
        {
            Editing();
            int index = _categories.IndexOf(Find(_categories, name));
            InputActionCategory copy = (InputActionCategory)Members.Clone(_categories[index]);
            Members.Write(copy, "descriptiveName", label);
            _categories[index] = copy;
        }

        /// <summary>Sets conflict checking for one map category; relationships are directional.</summary>
        public void SetMapCategoryConflicts(string name, bool checkAll, params string[] otherMaps)
        {
            Editing();
            int[] ids = otherMaps.Select(GetMapCategoryId).Distinct().ToArray();
            int index = _maps.IndexOf(Find(_maps, name));
            InputMapCategory copy = (InputMapCategory)Members.Clone(_maps[index]);
            Members.Write(copy, "checkConflictsWithAllCategories", checkAll);
            Members.Write(copy, "checkConflictsCategoryIds", ids);
            _maps[index] = copy;
        }

        // These change definition order. Layout.OrderTabs/OrderCategories control the UI.
        // Unlisted vanilla/mod categories retain their relative order after the requested ones.
        public void OrderMapCategories(params string[] first)
        {
            Editing();
            OrderFirst(_maps, first.Select(n => Find(_maps, n)));
        }

        public void OrderActionCategories(params string[] first)
        {
            Editing();
            OrderFirst(_categories, first.Select(n => Find(_categories, n)));
        }

        public void Dump(Action<string> write)
        {
            write($"Map Categories:\n{TextUtil.FormatTableJoined(
                ["ID", "Name", "Label"],
                [.. _maps.Select<InputMapCategory, string[]>(x => [x.id.ToString(), x.name, Regex.Escape(x.descriptiveName)])]
                )}");

            write($"Action Categories:\n{TextUtil.FormatTableJoined(
                ["ID", "Name", "Label"],
                [.. _categories.Select<InputActionCategory, string[]>(x => [x.id.ToString(), x.name, Regex.Escape(x.descriptiveName)])]
            )}");

            write($"Actions:\n{TextUtil.FormatTableJoined(
                ["ID", "Category", "Name", "Label", "Type", "Assignable"],
                [.. _actions.Select<InputAction, string[]>(x => [x.id.ToString(), GetActionCategoryName(x.categoryId), x.name, Regex.Escape(x.descriptiveName), x.type.ToString(), x.userAssignable.ToString()])]
            )}");
        }

        internal int GetActionId(string name)
        {
            // Other pre-Awake patches may have appended actions after our Commit.
            IEnumerable actions = _committed ? _sourceActions : (IEnumerable)_actions;
            foreach (InputAction action in actions)
                if (action.name == name)
                    return action.id;
            throw new ArgumentException("Unknown Rewired action: " + name);
        }

        internal void Commit()
        {
            Editing();

            ReplaceContents(_sourceMaps, _maps);
            ReplaceContents(_sourceCategories, _categories);
            ReplaceContents(_sourceActions, _actions);

            ActionCategoryMap categoryMap = _userData.actionCategoryMap;

            // Categories must exist in the ordering map before adding actions.
            foreach (InputActionCategory category in _categories)
            {
                if (categoryMap.IndexOfCategory(category.id) < 0)
                    categoryMap.AddCategory(category.id);
            }

            // Include existing actions to repair previously missing memberships.
            // AddAction ignores duplicates, preserving the existing action order.
            foreach (InputAction action in _actions)
            {
                if (!categoryMap.AddAction(action.categoryId, action.id))
                {
                    throw new InvalidOperationException(
                        $"Could not add action '{action.name}' ({action.id}) " +
                        $"to category map {action.categoryId}.");
                }
            }

            _committed = true;
        }

        private void Editing()
        {
            if (_committed) throw new InvalidOperationException("Edit definitions only in the define callback.");
        }

        private static void InitializeCategory(InputCategory category, int id, string name,
            string label, bool userAssignable)
        {
            Members.Write(category, "id", id);
            Members.Write(category, "name", name);
            Members.Write(category, "descriptiveName", label);
            Members.Write(category, "userAssignable", userAssignable);
        }

        private static T? ValidateIdentity<T>(List<T> list, int id, string name) where T : InputCategory
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.");
            foreach (T c in list)
            {
                if (c.name == name)
                {
                    if (c.id != id)
                        throw new InvalidOperationException(
                            $"Category '{name}' already has ID {c.id}; requested {id}.");
                    return c;
                }

                if (c.id == id)
                    throw new InvalidOperationException(
                        $"Category ID {id} is already used by '{c.name}'.");
            }

            return null;
        }

        private static T Find<T>(List<T> list, string name) where T : InputCategory
        {
            return list.Find(c => c.name == name)
                   ?? throw new ArgumentException("Unknown Rewired category scripting name: " + name);
        }

        private static T FindById<T>(List<T> list, int id) where T : InputCategory
        {
            return list.Find(c => c.id == id)
                   ?? throw new ArgumentException("Unknown Rewired category ID: " + id);
        }
    }

    /// <summary>Edits a private copy of ControlMapper's mapping sets, preserving all unedited tabs.</summary>
    public sealed class Layout
    {
        private readonly ControlMapper _mapper;
        private readonly RewiredCategories.Definitions _definitions;
        private readonly Type _setType;
        private readonly List<ControlMapper.MappingSet> _sets;
        private bool? _headings;

        public int KeyboardLayoutId => (int)Members.Read(_mapper, "keyboardMapDefaultLayout");
        public int MouseLayoutId => (int)Members.Read(_mapper, "mouseMapDefaultLayout");
        public int JoystickLayoutId => (int)Members.Read(_mapper, "joystickMapDefaultLayout");

        internal Layout(ControlMapper mapper, RewiredCategories.Definitions definitions)
        {
            _mapper = mapper;
            _definitions = definitions;
            ControlMapper.MappingSet[] source = mapper._mappingSets;
            // _log!.LogInfo($"Layout({mapper}, {definitions})");
            // _log!.LogInfo($"  source: {source}<{source.GetType()}>");
            // foreach (ControlMapper.MappingSet x in source) _log!.LogInfo($"    {x}");
            _setType = source.GetType().IsArray
                ? source.GetType().GetElementType()!
                : source.GetType().GetGenericArguments().Single();
            _sets = [.. source.Select(CopyMappingSet)];
        }

        public void ShowCategoryHeadings(bool show = true)
        {
            _headings = show;
        }

        /// <summary>Replaces the section list for this tab, in the supplied order.</summary>
        public void SetCategories(string mapCategory, params string[] actionCategories)
        {
            int[] ids = actionCategories.Select(_definitions.GetActionCategoryId).Distinct().ToArray();
            ControlMapper.MappingSet set = GetOrAddSet(mapCategory);
            set._actionListMode = ControlMapper.MappingSet.ActionListMode.ActionCategory;
            WriteIds(set, "actionCategoryIds", ids);
            WriteIds(set, "actionIds", []);
        }

        /// <summary>Appends sections without removing existing vanilla sections. Rejects individual-action mode.</summary>
        public void AddCategories(string mapCategory, params string[] actionCategories)
        {
            int[] added = [.. actionCategories.Select(_definitions.GetActionCategoryId)];
            ControlMapper.MappingSet set = GetOrAddSet(mapCategory);
            RequireCategoryMode(set);
            int[] ids = [.. set.actionCategoryIds.Concat(added).Distinct()];
            WriteIds(set, "actionCategoryIds", ids);
        }

        /// <summary>Uses an explicit action list instead of category sections, in the supplied order.</summary>
        public void SetActions(string mapCategory, params string[] actionNames)
        {
            int[] ids = [.. actionNames.Select(_definitions.GetActionId).Distinct()];
            ControlMapper.MappingSet set = GetOrAddSet(mapCategory);
            set._actionListMode = ControlMapper.MappingSet.ActionListMode.Action;
            WriteIds(set, "actionIds", ids);
            WriteIds(set, "actionCategoryIds", []);
        }

        /// <summary>Requested tabs first; unlisted tabs remain afterward in their previous relative order.</summary>
        public void OrderTabs(params string[] first)
        {
            OrderFirst(_sets, first.Select(ExistingSet));
        }

        /// <summary>Requested sections first; unlisted sections retain their relative order.</summary>
        public void OrderCategories(string mapCategory, params string[] first)
        {
            ControlMapper.MappingSet set = ExistingSet(mapCategory);
            RequireCategoryMode(set);
            List<int> ids = [.. set.actionCategoryIds];
            OrderFirst(ids, first.Select(_definitions.GetActionCategoryId));
            WriteIds(set, "actionCategoryIds", ids);
        }

        public void MoveTabBefore(string tab, string before)
        {
            MoveBefore(_sets, ExistingSet(tab), ExistingSet(before));
        }

        public void MoveCategoryBefore(string mapCategory, string category, string before)
        {
            // Plugin.Log($"MoveCategoryBefore({mapCategory}, {category}, {before})");
            ControlMapper.MappingSet set = ExistingSet(mapCategory);
            // Plugin.Log($"object: {set}");
            RequireCategoryMode(set);
            List<int> ids = [.. set.actionCategoryIds];
            // Plugin.Log($"ids: {ids}[{ids.Count}/{ids.Capacity}]");
            // foreach (int id in ids) Plugin.Log($" > {id}");
            MoveBefore(ids, _definitions.GetActionCategoryId(category),
                _definitions.GetActionCategoryId(before));
            WriteIds(set, "actionCategoryIds", ids);
        }

        /// <summary>Finds an existing tab displaying this action category, avoiding guesses about vanilla map names.</summary>
        public string? FindTabShowingCategory(string actionCategory)
        {
            int id = _definitions.GetActionCategoryId(actionCategory);
            foreach (ControlMapper.MappingSet set in _sets)
                if (set.actionListMode == ControlMapper.MappingSet.ActionListMode.ActionCategory &&
                    set.actionCategoryIds.Contains(id))
                    return _definitions.GetMapCategoryName(set.mapCategoryId);
            return null;
        }

        public void Dump(Action<string> write)
        {
            foreach (ControlMapper.MappingSet set in _sets)
            {
                string name = _definitions.GetMapCategoryName(set.mapCategoryId);
                ControlMapper.MappingSet.ActionListMode mode = set.actionListMode;
                string entries = mode == ControlMapper.MappingSet.ActionListMode.ActionCategory
                    ? string.Join(", ", set.actionCategoryIds.Select(_definitions.GetActionCategoryName))
                    : string.Join(", ", set.actionIds);
                write($"Mapper tab '{name}' [{mode}]: {entries}");
            }
        }

        internal void Commit()
        {
            // All changes above affect cloned sets. No vanilla UI configuration is changed until here.
            _mapper._mappingSets = [.. _sets];
            if (_headings.HasValue)
                _mapper._showActionCategoryLabels = _headings.Value;
        }

        private ControlMapper.MappingSet ExistingSet(string name)
        {
            // Plugin.Log("ExistingSet: " + name);
            int id = _definitions.GetMapCategoryId(name);
            return _sets.Find(s => s.mapCategoryId == id)
                   ?? throw new ArgumentException("Map category is not displayed by this mapper: " + name);
        }

        private ControlMapper.MappingSet GetOrAddSet(string name)
        {
            int id = _definitions.GetMapCategoryId(name);
            ControlMapper.MappingSet? set = _sets.Find(s => (int)Members.Read(s, "mapCategoryId") == id);
            if (set != null) return set;

            // set = new ControlMapper.MappingSet(id, ControlMapper.MappingSet.ActionListMode.ActionCategory, [], []);
            set = (ControlMapper.MappingSet) Members.Create(typeof(ControlMapper.MappingSet));
            set._mapCategoryId = id;
            set._actionListMode = ControlMapper.MappingSet.ActionListMode.ActionCategory;
            WriteIds(set, "actionCategoryIds", Array.Empty<int>());
            WriteIds(set, "actionIds", Array.Empty<int>());

            _sets.Add(set);
            return set;
        }

        [Obsolete("Replace with \"set.actionCategoryIds\".")]
        private static IEnumerable<int> CategoryIds(ControlMapper.MappingSet set)
        {
            return set.actionCategoryIds;
        }

        private static ControlMapper.MappingSet CopyMappingSet(ControlMapper.MappingSet source)
        {
            ControlMapper.MappingSet copy = (ControlMapper.MappingSet) Members.Create(source.GetType());
            Members.CopyExtraConfiguration(source, copy,
                "mapCategoryId", "actionListMode", "actionCategoryIds", "actionIds");
            Members.Write(copy, "mapCategoryId", Members.Read(source, "mapCategoryId"));
            Members.Write(copy, "actionListMode", Members.Read(source, "actionListMode"));
            WriteIds(copy, "actionCategoryIds", Members.ReadList(source, "actionCategoryIds").Cast<int>());
            WriteIds(copy, "actionIds", Members.ReadList(source, "actionIds").Cast<int>());
            return copy;
        }

        private static ControlMapper.MappingSet CopyMappingSet_DontUse(ControlMapper.MappingSet source)
        {
            ControlMapper.MappingSet copy = new(source._mapCategoryId, source._actionListMode, source._actionCategoryIds, source._actionIds);

            // {
            //     _mapCategoryId = source._mapCategoryId,
            //     _actionCategoryIds = source._actionCategoryIds,
            //     _actionIds = source._actionIds,
            //     _actionListMode = source._actionListMode,
            // };


            // Members.CopyExtraConfiguration(source, copy,
            //     "mapCategoryId", "actionListMode", "actionCategoryIds", "actionIds");

            // Members.Write(copy, "mapCategoryId", Members.Read(source, "mapCategoryId"));
            // Members.Write(copy, "actionListMode", Members.Read(source, "actionListMode"));
            _log!.LogWarning($"New Mapping Set:");
            _log!.LogWarning($" -> mapCategoryId: {copy.mapCategoryId}");
            _log!.LogWarning($" -> actionListMode: {copy.actionListMode}");
            _log!.LogWarning($" -> actionCategoryIds: {copy.actionCategoryIds}");
            _log!.LogWarning($" -> actionIds: {copy.actionIds}");
            // MemberInfo[] memberInfos = copy.GetType().GetMembers();
            // _log!.LogWarning($"{copy}<{copy.GetType()}>: {memberInfos.Length} Members");
            // foreach (MemberInfo memberInfo in memberInfos) _log!.LogWarning($"{memberInfo}");
            // WriteIds(copy, "actionCategoryIds", Members.ReadList(source, "actionCategoryIds").Cast<int>());
            // WriteIds(copy, "actionIds", Members.ReadList(source, "actionIds").Cast<int>());
            return copy;
        }

        private static void WriteIds(object set, string name, IEnumerable<int> values)
        {
            int[] snapshot = [.. values];
            IList storage = Members.ReadStoredList(set, name);
            if (storage is int[])
            {
                // Arrays cannot grow. Capture this property's exact view, replace
                // the array, then rebind any owner field caching that same view.
                // Matching only the old array would be unsafe: two properties can
                // share Array.Empty<int>() but have separate read-only wrappers.
                object oldView = Members.Read(set, name);
                Members.Write(set, name, snapshot);
                int[] replacement = Members.ReadStoredList(set, name) as int[]
                                    ?? throw new InvalidOperationException(name +
                                                                           " storage changed to an unexpected type.");
                RebindCachedIdView(set, name, oldView, replacement);
            }
            else
            {
                // Builds that store mutable lists can keep their existing storage.
                ReplaceContents(storage, snapshot);
            }

            int[] observed = Members.ReadList(set, name).Cast<int>().ToArray();
            if (!observed.SequenceEqual(snapshot))
                throw new InvalidOperationException(
                    $"Writing {set.GetType().FullName}.{name} did not update its getter. " +
                    $"Expected [{string.Join(", ", snapshot)}]; observed [{string.Join(", ", observed)}]. " +
                    "Inspect the MappingSet declaration to identify its cache.");
        }

        private static void RebindCachedIdView(object set, string name, object oldView, int[] values)
        {
            if (oldView is not ReadOnlyCollection<int>) return;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            List<FieldInfo> matches = new();
            for (Type? type = set.GetType(); type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(flags))
                    if (ReferenceEquals(field.GetValue(set), oldView))
                        matches.Add(field);

            // Usually there is one cached wrapper. If an empty view is shared by
            // multiple fields, require an unambiguous property-specific name.
            if (matches.Count > 1)
            {
                string key = name.Replace("_", "");
                matches = matches.Where(f => f.Name.Replace("_", "").IndexOf(
                    key, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (matches.Count != 1)
                    throw new InvalidOperationException(
                        "Ambiguous cached ID view for " + set.GetType().FullName + "." + name +
                        "; inspect the MappingSet declaration before changing cache fields.");
            }

            // Give each member its own wrapper even when its array is empty.
            ReadOnlyCollection<int> replacement = new(values);
            foreach (FieldInfo field in matches)
            {
                if (!field.FieldType.IsInstanceOfType(replacement))
                    throw new InvalidOperationException("Unsupported ID-view cache field: " + field.Name);
                field.SetValue(set, replacement);
            }
        }

        private static void RequireCategoryMode(ControlMapper.MappingSet set)
        {
            // Plugin.Log("RequireCategoryMode: " + set);
            if (set.actionListMode != ControlMapper.MappingSet.ActionListMode.ActionCategory)
                throw new InvalidOperationException(
                    "This tab uses individual actions. Use SetCategories to intentionally replace that list.");
        }
    }

    // Version-sensitive access is isolated here. Prefer a real property setter,
    // otherwise use its named field, underscore field, or auto-property backing field.
    // An unsupported/obfuscated member throws with its exact name; no silent writes.
    private static class Members
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public |
                                           BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly MethodInfo MemberwiseClone = typeof(object).GetMethod(
            "MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;

        internal static object Create(Type type)
        {
            return Activator.CreateInstance(type, true)
                   ?? throw new InvalidOperationException("Cannot construct " + type.FullName);
        }

        internal static object Clone(object value)
        {
            return MemberwiseClone.Invoke(value, null)!;
        }

        internal static object Read(object instance, string name)
        {
            for (Type? type = instance.GetType(); type != null; type = type.BaseType)
            {
                PropertyInfo? property = type.GetProperty(name, Flags);
                if (property?.GetGetMethod(true) != null)
                    return property.GetValue(instance, null)
                           ?? throw new InvalidOperationException(type.FullName + "." + name + " is null.");
                FieldInfo? field = FindField(type, name);
                if (field != null)
                    return field.GetValue(instance)
                           ?? throw new InvalidOperationException(type.FullName + "." + name + " is null.");
            }

            throw Missing(instance, name);
        }

        internal static IList ReadList(object instance, string name)
        {
            return Read(instance, name) as IList
                   ?? throw new InvalidOperationException(instance.GetType().FullName + "." + name + " is not IList.");
        }

        internal static IList ReadStoredList(object instance, string name)
        {
            // _log!.LogWarning($"ReadStoredList({instance}, {name})");
            for (Type? type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo? field = FindField(type, name);
                if (field != null)
                    return field.GetValue(instance) as IList
                           ?? throw new InvalidOperationException(type.FullName + "." + name +
                                                                  " storage is not IList.");
            }

            // A mutable public property can expose storage directly in other versions.
            IList list = ReadList(instance, name);
            if (!list.IsReadOnly && !list.IsFixedSize) return list;
            throw new MissingMemberException(instance.GetType().FullName,
                name + " mutable backing list");
        }

        internal static void CopyExtraConfiguration(object source, object target, params string[] handled)
        {
            for (Type? type = source.GetType(); type != null; type = type.BaseType)
            {
                HashSet<FieldInfo> known = new(handled.Select(n => FindField(type, n))
                    .Where(f => f != null).Select(f => f!));
                foreach (FieldInfo field in type.GetFields(Flags))
                {
                    if (field.IsStatic || field.IsInitOnly || field.IsNotSerialized || known.Contains(field)) continue;
                    bool serialized = field.IsPublic || field.GetCustomAttributesData().Any(a =>
                        a.AttributeType.FullName == "UnityEngine.SerializeField" ||
                        a.AttributeType.FullName == "UnityEngine.SerializeReference");
                    if (!serialized) continue; // Runtime caches are deliberately not copied.
                    object? value = field.GetValue(source);
                    if (value is IList items)
                    {
                        IList? destination = field.GetValue(target) as IList;
                        if (destination != null && !destination.IsReadOnly && !destination.IsFixedSize)
                        {
                            ReplaceContents(destination, items);
                            continue;
                        }

                        value = ConvertValue(field.FieldType, items.Cast<object>().ToArray());
                    }

                    field.SetValue(target, value);
                }
            }
        }

        internal static void Write(object instance, string name, object value)
        {
            for (Type? type = instance.GetType(); type != null; type = type.BaseType)
            {
                PropertyInfo? property = type.GetProperty(name, Flags);
                MethodInfo? setter = property?.GetSetMethod(true);
                if (setter != null)
                {
                    setter.Invoke(instance, new[] { ConvertValue(property!.PropertyType, value) });
                    return;
                }

                FieldInfo? field = FindField(type, name);
                if (field == null) continue;
                field.SetValue(instance, ConvertValue(field.FieldType, value));
                return;
            }

            throw Missing(instance, name);
        }

        private static object ConvertValue(Type target, object value)
        {
            if (target.IsInstanceOfType(value)) return value;
            if (target.IsEnum && value is string text) return Enum.Parse(target, text);
            if (value is IEnumerable items && !(value is string))
            {
                object[] values = items.Cast<object>().ToArray();
                if (target.IsArray)
                {
                    Array array = Array.CreateInstance(target.GetElementType()!, values.Length);
                    for (int i = 0; i < values.Length; i++) array.SetValue(values[i], i);
                    return array;
                }

                if (target.IsGenericType)
                {
                    Type element = target.GetGenericArguments().Single();
                    IList list = (IList)Create(typeof(List<>).MakeGenericType(element));
                    foreach (object item in values) list.Add(item);
                    if (target.IsInstanceOfType(list)) return list;
                }
            }

            throw new InvalidCastException("Cannot assign " + value.GetType().FullName + " to " + target.FullName);
        }

        private static FieldInfo? FindField(Type type, string name)
        {
            return type.GetField(name, Flags) ?? type.GetField("_" + name, Flags)
                ?? type.GetField("<" + name + ">k__BackingField", Flags);
        }

        private static MissingMemberException Missing(object instance, string name)
        {
            return new MissingMemberException(
                $"Rewired member '{instance.GetType().FullName}.{name}' was not found. " +
                "Inspect this member in the game's bundled assemblies and update Members access accordingly.");
        }
    }
}