// RewiredDefaultBindings.cs
// Solar Dynamics 2026

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using UnityEngine;

#endregion

namespace Solar.UI.Input;

/// <summary>
///     Reapplies registered keyboard and mouse defaults after Rewired reloads its defaults.
///     Install once from plugin Awake. Custom InputBindings can be registered there;
///     GetExistingAction requires Rewired to be ready before looking up vanilla actions.
///     Uses the actual backing map category, not a Control Mapper tab alias.
///     All public methods must be called on Unity's main thread.
/// </summary>
public static class RewiredDefaultBindings
{
    private const string PatchId = "Solar.UI.Input.RewiredDefaultBindings";
    private static readonly List<BindingPreset> Presets = [];
    private static readonly Dictionary<string, InputBinding> ExistingActions = new(StringComparer.Ordinal);
    private static Harmony? _harmony;
    private static ManualLogSource? _log;
    private static bool _restoring;

    /// <summary>
    ///     Hooks Player.ControllerHelper.MapHelper.LoadDefaultMaps(ControllerType).
    ///     The supplied ControlMapper's Restore Defaults button calls this method.
    ///     No PatchAll call is needed for this helper.
    /// </summary>
    public static void Install(ManualLogSource logger)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (_harmony != null) return;

        MethodInfo target = AccessTools.Method(
                                typeof(Player.ControllerHelper.MapHelper), "LoadDefaultMaps",
                                [typeof(ControllerType)])
                            ?? throw new MissingMethodException(
                                "Rewired.Player.ControllerHelper.MapHelper",
                                "LoadDefaultMaps(ControllerType)");

        Harmony harmony = new(PatchId);
        _log = logger;
        try
        {
            harmony.Patch(target, postfix: new HarmonyMethod(
                    typeof(RewiredDefaultBindings), nameof(AfterLoadDefaultMaps))
                { priority = Priority.Last });
            _harmony = harmony;
        }
        catch
        {
            harmony.UnpatchSelf();
            throw;
        }
    }

    /// <summary>
    ///     Gets a reusable handle for an existing runtime action, including vanilla
    ///     actions such as "Fire", "Pitch", and "Roll". Does not create or register
    ///     a new Rewired action. Requires ReInput.isReady; use the action's scripting
    ///     name, not its display label. Pass the result to any Register* method.
    ///     Registering a preset affects future resets; to replace current assignments,
    ///     call RestoreKeyboardDefaults/RestoreMouseDefaults explicitly after saved
    ///     controls load, or use the game's Restore Defaults button.
    /// </summary>
    public static InputBinding GetExistingAction(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("An existing action name is required.", nameof(actionName));
        if (!ReInput.isReady)
            throw new InvalidOperationException("Rewired is not ready.");
        InputAction action = ReInput.mapping.GetAction(actionName)
                             ?? throw new ArgumentException($"Unknown Rewired action '{actionName}'.",
                                 nameof(actionName));

        if (ExistingActions.TryGetValue(action.name, out InputBinding? binding))
        {
            if (binding.Type != action.type)
                throw new InvalidOperationException($"The type of action '{action.name}' has changed.");
            binding.ActionId = action.id;
            return binding;
        }

        // Rewired exposes the finished labels, not separate custom suffixes.
        // Preserve those labels and leave the new suffix fields empty.
        binding = new InputBinding(
            action.name,
            ReInput.mapping.GetActionCategory(action.categoryId)?.name ?? string.Empty,
            string.IsNullOrEmpty(action.descriptiveName) ? action.name : action.descriptiveName,
            string.Empty,
            action.positiveDescriptiveName ?? string.Empty,
            string.Empty,
            action.negativeDescriptiveName ?? string.Empty,
            string.Empty,
            action.type,
            action.userAssignable,
            action.id,
            null)
        {
            ActionId = action.id,
        };
        ExistingActions.Add(action.name, binding);
        return binding;
    }

    /// <summary>
    ///     Adds a default key. Repeating this for one action adds alternate keys;
    ///     its existing mappings are removed only once during a restore.
    ///     Pole.Negative supports a key driving the negative side of an axis.
    ///     enabledIfCreated applies only if the backing map must be recreated.
    /// </summary>
    public static void RegisterKeyboard(
        InputBinding binding,
        KeyCode key,
        string mapCategory = "Default",
        string layout = "Default",
        ModifierKeyFlags modifiers = ModifierKeyFlags.None,
        Pole contribution = Pole.Positive,
        int playerId = 0,
        bool enabledIfCreated = true)
    {
        if (key == KeyCode.None)
            throw new ArgumentException(
                "Use RegisterKeyboardUnbound for an unbound default.", nameof(key));

        BindingPreset preset = GetOrCreatePreset(
            ControllerType.Keyboard, binding, mapCategory, layout, playerId, enabledIfCreated);

        preset.Add(new DefaultElement(
            ControllerElementType.Button, -1, AxisRange.Positive, key,
            modifiers, contribution, false));
    }

    /// <summary>Restores an action to having no keyboard assignments in this map.</summary>
    public static void RegisterKeyboardUnbound(
        InputBinding binding,
        string mapCategory = "Default",
        string layout = "Default",
        int playerId = 0,
        bool enabledIfCreated = true)
    {
        GetOrCreatePreset(ControllerType.Keyboard, binding, mapCategory, layout,
            playerId, enabledIfCreated).Elements.Clear();
    }

    /// <summary>
    ///     Adds a default mouse button, using its zero-based button index:
    ///     0 = left, 1 = right, 2 = middle, 3+ = additional buttons.
    ///     Resolves the Rewired element identifier at runtime; do not pass an element ID.
    ///     Repeated registrations add alternate defaults for the same action.
    ///     contribution also allows a mouse button to drive either side of an axis.
    /// </summary>
    public static void RegisterMouseButton(
        InputBinding binding,
        int buttonIndex,
        string mapCategory = "Default",
        string layout = "Default",
        Pole contribution = Pole.Positive,
        int playerId = 0,
        bool enabledIfCreated = true)
    {
        if (buttonIndex < 0) throw new ArgumentOutOfRangeException(nameof(buttonIndex));
        ValidateContribution(contribution);
        BindingPreset preset = GetOrCreatePreset(
            ControllerType.Mouse, binding, mapCategory, layout, playerId, enabledIfCreated);
        preset.Add(new DefaultElement(
            ControllerElementType.Button, buttonIndex, AxisRange.Positive,
            KeyCode.None, ModifierKeyFlags.None, contribution, false));
    }

    /// <summary>
    ///     Adds a default mouse axis, using its zero-based axis index:
    ///     0 = X, 1 = Y, 2 = scroll wheel.
    ///     Full maps both directions to an Axis action and supports inversion.
    ///     Positive/Negative selects a physical half-axis, which may drive a Button
    ///     action or either side of an Axis action (selected by contribution).
    ///     For split axes, reverse range/contribution instead of using invert.
    /// </summary>
    public static void RegisterMouseAxis(
        InputBinding binding,
        int axisIndex,
        string mapCategory = "Default",
        string layout = "Default",
        bool invert = false,
        AxisRange range = AxisRange.Full,
        Pole contribution = Pole.Positive,
        int playerId = 0,
        bool enabledIfCreated = true)
    {
        if (binding == null) throw new ArgumentNullException(nameof(binding));
        if (axisIndex < 0) throw new ArgumentOutOfRangeException(nameof(axisIndex));
        if (range != AxisRange.Full && range != AxisRange.Positive && range != AxisRange.Negative)
            throw new ArgumentOutOfRangeException(nameof(range));
        ValidateContribution(contribution);
        if (range == AxisRange.Full && binding.Type != InputActionType.Axis)
            throw new ArgumentException(
                "A full mouse axis requires an Axis action. Use Positive or Negative for a Button action.",
                nameof(binding));
        if (range != AxisRange.Full && invert)
            throw new ArgumentException(
                "For split axes, reverse range/contribution instead of using invert.", nameof(invert));

        BindingPreset preset = GetOrCreatePreset(
            ControllerType.Mouse, binding, mapCategory, layout, playerId, enabledIfCreated);
        preset.Add(new DefaultElement(
            ControllerElementType.Axis, axisIndex, range, KeyCode.None,
            ModifierKeyFlags.None, contribution, invert));
    }

    /// <summary>Restores an action to having no mouse assignments in this map.</summary>
    public static void RegisterMouseUnbound(
        InputBinding binding,
        string mapCategory = "Default",
        string layout = "Default",
        int playerId = 0,
        bool enabledIfCreated = true)
    {
        GetOrCreatePreset(ControllerType.Mouse, binding, mapCategory, layout,
            playerId, enabledIfCreated).Elements.Clear();
    }

    /// <summary>
    ///     Restores only this helper's registered keyboard defaults for one player.
    ///     Registered actions can be mod actions or vanilla actions.
    ///     Does not reset unregistered controls or save data. This is an explicit reset,
    ///     not an "add missing defaults" routine: do not run it every startup.
    ///     Can also be called after a custom restore-defaults delegate has finished.
    /// </summary>
    public static void RestoreKeyboardDefaults(int playerId = 0)
    {
        ApplyForPlayer(GetPlayer(playerId), ControllerType.Keyboard, true);
    }

    /// <summary>
    ///     Restores only registered mouse defaults. Does not reset unregistered controls,
    ///     keyboard bindings, or save data. Use after a custom restore delegate if it
    ///     bypasses LoadDefaultMaps. Do not call on every startup.
    /// </summary>
    public static void RestoreMouseDefaults(int playerId = 0)
    {
        ApplyForPlayer(GetPlayer(playerId), ControllerType.Mouse, true);
    }

    /// <summary>
    ///     Adds initial defaults while preserving existing assignments in each target
    ///     map and skipping key conflicts. Call once AFTER saved controls load, behind
    ///     your own persistent per-profile/per-migration flag. Calling this every
    ///     startup would restore keys the user intentionally cleared.
    ///     Does not save or set the caller's initialization flag.
    /// </summary>
    public static void ApplyMissingKeyboardDefaults(int playerId = 0)
    {
        ApplyForPlayer(GetPlayer(playerId), ControllerType.Keyboard, false);
    }

    /// <summary>
    ///     Adds initial mouse defaults, preserving existing assignments in each target
    ///     map and skipping conflicts. Call once AFTER saved controls load, behind
    ///     your own persistent per-profile/per-migration flag, then save normally.
    ///     Does not save, set the caller's initialization flag, or assign the mouse
    ///     to the player. The player must have mouse access for mouse input to work.
    /// </summary>
    public static void ApplyMissingMouseDefaults(int playerId = 0)
    {
        ApplyForPlayer(GetPlayer(playerId), ControllerType.Mouse, false);
    }

    private static Player GetPlayer(int playerId)
    {
        if (!ReInput.isReady)
            throw new InvalidOperationException("Rewired is not ready.");
        return ReInput.players.GetPlayer(playerId)
               ?? throw new ArgumentException("Unknown player.", nameof(playerId));
    }

    private static void ValidateContribution(Pole contribution)
    {
        if (contribution != Pole.Positive && contribution != Pole.Negative)
            throw new ArgumentOutOfRangeException(nameof(contribution));
    }

    private static BindingPreset GetOrCreatePreset(
        ControllerType controllerType, InputBinding binding, string mapCategory, string layout,
        int playerId, bool enabledIfCreated)
    {
        if (_restoring)
            throw new InvalidOperationException("Cannot register defaults during a restore.");
        if (binding == null) throw new ArgumentNullException(nameof(binding));
        if (string.IsNullOrWhiteSpace(mapCategory))
            throw new ArgumentException("A backing map category is required.", nameof(mapCategory));
        if (string.IsNullOrWhiteSpace(layout))
            throw new ArgumentException("A layout is required.", nameof(layout));
        if (playerId < 0) throw new ArgumentOutOfRangeException(nameof(playerId));

        foreach (BindingPreset preset in Presets)
        {
            if (preset.ControllerType != controllerType ||
                preset.PlayerId != playerId || preset.Binding.Name != binding.Name ||
                preset.MapCategory != mapCategory || preset.Layout != layout)
                continue;

            if (!ReferenceEquals(preset.Binding, binding) ||
                preset.EnabledIfCreated != enabledIfCreated)
                throw new InvalidOperationException(
                    $"Conflicting default registration for '{binding.Name}'.");
            return preset;
        }

        BindingPreset created = new(
            controllerType, binding, mapCategory, layout, playerId, enabledIfCreated);
        Presets.Add(created);
        return created;
    }

    // Harmony's __0 binds by position, independent of the parameter's runtime name.
    private static void AfterLoadDefaultMaps(
        Player.ControllerHelper.MapHelper __instance, ControllerType __0)
    {
        if ((__0 != ControllerType.Keyboard && __0 != ControllerType.Mouse) ||
            !ReInput.isReady || _restoring)
            return;

        // Locate the owner without reflecting Rewired's private/obfuscated fields.
        foreach (Player player in ReInput.players.Players)
        {
            if (!ReferenceEquals(player.controllers.maps, __instance)) continue;
            try
            {
                ApplyForPlayer(player, __0, true);
            }
            catch (Exception exception)
            {
                // The vanilla reset has already finished; report mod failures
                // without throwing out of ControlMapper's confirmation handler.
                _log?.LogError(
                    $"Restoring registered {__0} defaults for player {player.id} failed:\n{exception}");
            }

            return;
        }
    }

    private static void ApplyForPlayer(Player player, ControllerType controllerType, bool restore)
    {
        if (_restoring) return;
        _restoring = true;
        try
        {
            // Resolve every action and physical mouse element before deleting
            // anything. An invalid index must not erase the user's bindings.
            List<(BindingPreset Preset, ElementAssignment[] Assignments)> resolved = new();
            foreach (BindingPreset preset in Presets)
            {
                if (preset.PlayerId != player.id || preset.ControllerType != controllerType)
                    continue;
                if (!preset.Binding.Ready)
                    throw new InvalidOperationException(
                        $"Default action '{preset.Binding.Name}' is not registered at runtime.");
                resolved.Add((preset, [
                    .. preset.Elements
                        .Select(element => element.ToAssignment(controllerType, preset.Binding.ActionId)),
                ]));
            }

            foreach ((BindingPreset Preset, ElementAssignment[] Assignments) item in resolved)
            {
                BindingPreset preset = item.Preset;
                ControllerMap? map = player.controllers.maps.GetMap(
                    controllerType, 0, preset.MapCategory, preset.Layout);

                if (map == null)
                {
                    player.controllers.maps.AddEmptyMap(
                        controllerType, 0, preset.MapCategory, preset.Layout);
                    map = player.controllers.maps.GetMap(
                              controllerType, 0, preset.MapCategory, preset.Layout)
                          ?? throw new InvalidOperationException(
                              $"Could not create {controllerType} map '{preset.MapCategory}/{preset.Layout}'. " +
                              "Register the map category and layout before Rewired initializes.");
                    map.enabled = preset.EnabledIfCreated;
                }

                int actionId = preset.Binding.ActionId;
                if (restore)
                    map.DeleteElementMapsWithAction(actionId);
                else if (map.ElementMapsWithAction(actionId).Any())
                    continue;

                foreach (ElementAssignment assignment in item.Assignments)
                {
                    // Initial seeding respects user choices. An explicit reset
                    // applies the registered preset exactly, including shared keys.
                    if (!restore)
                    {
                        ElementAssignmentConflictCheck conflict =
                            assignment.ToElementAssignmentConflictCheck();
                        conflict.playerId = player.id;
                        conflict.controllerType = controllerType;
                        conflict.controllerId = 0;
                        conflict.controllerMapId = map.id;
                        conflict.controllerMapCategoryId = map.categoryId;
                        if (ReInput.controllers.conflictChecking
                            .DoesElementAssignmentConflict(conflict))
                            continue;
                    }

                    if (!map.CreateElementMap(assignment))
                        throw new InvalidOperationException(
                            $"Could not restore {controllerType} default for '{preset.Binding.Name}'.");
                }
            }
        }
        finally
        {
            _restoring = false;
        }
    }

    private readonly struct DefaultElement
    {
        private readonly ControllerElementType _elementType;
        private readonly int _elementIndex;
        private readonly AxisRange _range;
        private readonly KeyCode _key;
        private readonly ModifierKeyFlags _modifiers;
        private readonly Pole _contribution;
        private readonly bool _invert;

        public DefaultElement(ControllerElementType elementType, int elementIndex,
            AxisRange range, KeyCode key, ModifierKeyFlags modifiers, Pole contribution, bool invert)
        {
            _elementType = elementType;
            _elementIndex = elementIndex;
            _range = range;
            _key = key;
            _modifiers = modifiers;
            _contribution = contribution;
            _invert = invert;
        }

        public ElementAssignment ToAssignment(ControllerType controllerType, int actionId)
        {
            if (controllerType == ControllerType.Keyboard)
                return new ElementAssignment(_key, _modifiers, actionId, _contribution);

            Mouse mouse = ReInput.controllers.Mouse
                          ?? throw new InvalidOperationException("Rewired's mouse is unavailable.");
            // Indices and Rewired element identifier IDs are different concepts.
            // Resolve through the appropriate identifier list rather than assuming
            // that mouse button 0 has element identifier ID 0.
            int elementId;
            if (_elementType == ControllerElementType.Axis)
            {
                if (_elementIndex < 0 || _elementIndex >= mouse.AxisElementIdentifiers.Count)
                    throw new InvalidOperationException($"Mouse axis index {_elementIndex} is unavailable.");
                elementId = mouse.AxisElementIdentifiers[_elementIndex].id;
            }
            else
            {
                if (_elementIndex < 0 || _elementIndex >= mouse.ButtonElementIdentifiers.Count)
                    throw new InvalidOperationException($"Mouse button index {_elementIndex} is unavailable.");
                elementId = mouse.ButtonElementIdentifiers[_elementIndex].id;
            }

            // Matches the complete-assignment constructor in the game's Rewired
            // version. -1 creates a new element map instead of replacing one by ID.
            return new ElementAssignment(ControllerType.Mouse, _elementType, elementId,
                _range, KeyCode.None, ModifierKeyFlags.None, actionId, _contribution, _invert, -1);
        }
    }

    private sealed class BindingPreset
    {
        public readonly ControllerType ControllerType;
        public readonly InputBinding Binding;
        public readonly string MapCategory;
        public readonly string Layout;
        public readonly int PlayerId;
        public readonly bool EnabledIfCreated;
        public readonly List<DefaultElement> Elements = [];

        public BindingPreset(ControllerType controllerType, InputBinding binding, string mapCategory, string layout,
            int playerId, bool enabledIfCreated)
        {
            ControllerType = controllerType;
            Binding = binding;
            MapCategory = mapCategory;
            Layout = layout;
            PlayerId = playerId;
            EnabledIfCreated = enabledIfCreated;
        }

        public void Add(DefaultElement element)
        {
            if (!Elements.Contains(element)) Elements.Add(element);
        }
    }
}