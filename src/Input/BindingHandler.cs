// Binding.cs
// Solar Dynamics 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using Solar.UI.Util;

namespace Solar.UI.Input;

public class BindingHandler
{

    private static readonly List<InputBinding> Bindings = [];
    private static readonly Dictionary<string, InputBinding> BindingsByName =
        new(StringComparer.Ordinal);


    private static readonly List<InputState> States = [];
    private static readonly Dictionary<string, InputState> StatesByName =
        new(StringComparer.Ordinal);

    // public static bool IsInitialized = false;

    private static bool _registrationStarted;

    private static bool DEBUG_SPAM = true;

    /// <summary>
    ///     Registers a new InputBinding.
    /// </summary>
    public static InputBinding Register(
        string name,
        string category,
        string label,
        string labelSuffix = "",
        string labelPos = "",
        string labelPosSuffix = "",
        string labelNeg = "",
        string labelNegSuffix = "",
        InputActionType type = InputActionType.Button,
        bool userAssignable = true,
        int startingActionId = 128,
        Action<InputAction>? configureAction = null)
    {
        RequireText(name, nameof(name));
        RequireText(category, nameof(category));
        RequireText(label, nameof(label));

        // if (!IsInitialized)
        //     throw new InvalidOperationException("Call InputHandler.Initialize first.");

        if (type != InputActionType.Button && type != InputActionType.Axis)
            throw new ArgumentOutOfRangeException(nameof(type));

        if (startingActionId < 0)
            throw new ArgumentOutOfRangeException(nameof(startingActionId));

        if (BindingsByName.TryGetValue(name, out InputBinding? existing))
        {
            if (existing.Label != label ||
                !string.Equals(existing.Category, category,
                    StringComparison.OrdinalIgnoreCase) ||
                existing.Type != type ||
                existing.UserAssignable != userAssignable ||
                existing.StartingActionId != startingActionId ||
                !Equals(existing.ConfigureAction, configureAction))
                throw new InvalidOperationException(
                    $"Action '{name}' was already queued with a different definition.");

            // Repeated setup of the same definition returns the same handle.
            return existing;
        }

        if (_registrationStarted || ReInput.isReady)
            throw new InvalidOperationException(
                $"Cannot register '{name}' after Rewired initialization has started. " +
                "Queue all actions in your plugin's Awake before InputManager_Base.Awake.");

        InputBinding binding = new(
            name, category, label, labelSuffix, labelPos, labelPosSuffix, labelNeg, labelNegSuffix, type, userAssignable,
            startingActionId, configureAction);

        BindingsByName.Add(name, binding);
        Bindings.Add(binding);
        return binding;
    }

    public static InputState RegisterState(
        string name,
        string category,
        string label,
        string labelSuffix = "",
        InputActionType type = InputActionType.Button,
        bool userAssignable = true,
        int startingActionId = 128,
        Action<InputAction>? configureAction = null)
    {
        RequireText(name, nameof(name));
        RequireText(category, nameof(category));
        RequireText(label, nameof(label));

        // if (!IsInitialized)
        //     throw new InvalidOperationException("Call InputHandler.Initialize first.");

        if (type != InputActionType.Button && type != InputActionType.Axis)
            throw new ArgumentOutOfRangeException(nameof(type));

        if (_registrationStarted || ReInput.isReady)
            throw new InvalidOperationException(
                $"Cannot register '{name}' after Rewired initialization has started. " +
                "Queue all actions in your plugin's Awake before InputManager_Base.Awake.");

        InputState state = new(name, category, label, labelSuffix, userAssignable, startingActionId);

        StatesByName.Add(name, state);
        States.Add(state);
        return state;
    }

    private static void RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
    }

    // public static InputActionCategory InsertActionCategory(UserData userData, int index)
    // {
    //     userData.InsertActionCategory(index);
    //     return userData.actionCategories[index];
    // }

    private static string GetActionName(int id, List<InputAction> actions)
    {
        return actions.FirstOrDefault(a => a.id == id)?.name ?? "";
    }

    private static string AeToStringShort(ActionElementMap ae, List<InputAction> actions)
    {
        ActionElementMap.s_toStringSB ??= new StringBuilder();

        ActionElementMap.s_toStringSB.Append($"{ae.arIJohFCyPzjQPiUgkKzaWgkbNOL.ToString().PadLeft(5)} ");
        // ActionElementMap.s_toStringSB.Append($"{ae.ClXloSgzMyciMLkHRIxpbOWpNaEdA.ToString().PadLeft(4)} ");
        ActionElementMap.s_toStringSB.Append($"{ae._actionId.ToString().PadLeft(4)} {GetActionName(ae._actionId, actions).PadRight(24)} ");
        // ActionElementMap.s_toStringSB.Append($"Action Descriptive Name {ae.actionDescriptiveName}");
        ActionElementMap.s_toStringSB.Append(ae.feGpPcocmLaFkFCqwosLzclsFpeGA ? " " : "N");
        ActionElementMap.s_toStringSB.Append($"{(ae._invert ? "!" : " ")}");
        switch (ae._axisRange)
        {
            case AxisRange.Positive:
                ActionElementMap.s_toStringSB.Append("+");
                break;
            case AxisRange.Full:
                ActionElementMap.s_toStringSB.Append(" ");
                break;
            case AxisRange.Negative:
                ActionElementMap.s_toStringSB.Append("-");
                break;
            default:
                ActionElementMap.s_toStringSB.Append("?");
                break;
        }
        ActionElementMap.s_toStringSB.Append($"{ae._elementType.ToString().PadRight(6)} ");
        ActionElementMap.s_toStringSB.Append($"{$"{ae._elementIdentifierId.ToString()}:{ae.dxSdLJlHZnQTvxiaVoUtDLUliYcJ.ToString()}".PadLeft(6)} ");
        ActionElementMap.s_toStringSB.Append($"{(ae._axisContribution == Pole.Negative ? "-" : "+")} ");
        ActionElementMap.s_toStringSB.Append($"{ae._keyboardKeyCode.ToString().PadRight(16)} ");
        if (ae.hasModifiers)
        {
            ActionElementMap.s_toStringSB.Append($"[{ae._modifierKey1}, ");
            ActionElementMap.s_toStringSB.Append($"{ae._modifierKey2}, ");
            ActionElementMap.s_toStringSB.Append($"{ae._modifierKey3}]");
            // ActionElementMap.s_toStringSB.Append($"<{ae.modifierKeyFlags}>");
        }
        string str = ActionElementMap.s_toStringSB.ToString();
        ActionElementMap.s_toStringSB.Length = 0;
        return str;
    }

    internal static void RegisterActions(InputManager_Base inputManager, RewiredCategories.Definitions definitions)
    {
        _registrationStarted = true;

        // Do not carry IDs or error state into another Input Manager's setup.
        // foreach (InputBinding binding in Bindings)
        // {
        //     binding.ActionId = -1;
        //     binding.ReadFailureLogged = false;
        // }

        if (Bindings.Count == 0)
            return;

        // These members use the same publicized Rewired assembly as the
        // original class. They are not part of Rewired's public runtime API.
        UserData userData = inputManager.userData;
        List<InputMapCategory> mapCategories = userData.mapCategories;
        List<InputActionCategory> actionCategories = userData.actionCategories;
        List<InputAction> actions = userData.actions;
        ActionCategoryMap categoryMap = userData.actionCategoryMap;
        List<ControllerMapLayoutManager_RuleSet_Editor> controllerMaps = userData.controllerMapLayoutManagerRuleSets;

        // InputActionCategory custom = InsertActionCategory(userData, 1);
        // custom.name = "Custom";
        // custom.descriptiveName = "Custom Solar UI input bindings.";



        if (actionCategories == null || actions == null || categoryMap == null)
        {
            Logger.LogError(
                "Custom input registration failed: Rewired UserData, actions, " +
                "categories, or the action category map was unavailable.");
            return;
        }

        HashSet<int> usedIds = [];
        foreach (InputAction action in actions)
            usedIds.Add(action.id);

        foreach (InputBinding binding in Bindings)
            try
            {
                RegisterAction(binding, definitions);
            }
            catch (Exception e)
            {
                // Failure of one definition does not prevent other actions.
                binding.ActionId = -1;
                Logger.LogError(
                    $"Failed registering InputBinding '{binding.Name}': {e}");
            }

        if (DEBUG_SPAM)
            definitions.Dump(Plugin.Log);
    }

    internal static void RegisterStates()
    {
        Plugin.Log($"BindingHandler.RegisterStates()");
        foreach (InputState state in States)
            state.Init();
    }

    private static void RegisterAction(InputBinding binding, RewiredCategories.Definitions definitions)
    {
        int id = binding.StartingActionId;
        string name = binding.Name;
        string category = binding.Category;

        string label = binding.Label;
        if (!string.IsNullOrEmpty(binding.LabelSuffix))
            label = TextUtil.FormatLabel(label, binding.LabelSuffix);

        string labelPos = binding.LabelPos;
        if (!string.IsNullOrEmpty(binding.LabelPosSuffix))
            labelPos = TextUtil.FormatLabel(labelPos, binding.LabelPosSuffix);

        string labelNeg = binding.LabelNeg;
        if (!string.IsNullOrEmpty(binding.LabelNegSuffix))
            labelNeg = TextUtil.FormatLabel(labelNeg, binding.LabelNegSuffix);

        string behavior = "Default";
        InputActionType actionType = binding.Type;
        bool userAssignable = binding.UserAssignable;
        if (actionType == InputActionType.Axis)
        {

        }

        binding.ActionId = definitions.RegisterAction(id, name, category, label, labelPos, labelNeg, behavior, actionType, userAssignable);
    }

    private static void RegisterAction_Old(
        InputBinding binding,
        List<InputActionCategory> categories,
        List<InputAction> actions,
        ActionCategoryMap categoryMap,
        HashSet<int> usedIds)
    {
        InputActionCategory? targetCategory = categories.Find(category =>
            string.Equals(category.name, binding.Category,
                StringComparison.OrdinalIgnoreCase));

        if (targetCategory == null)
            throw new InvalidOperationException(
                $"Rewired action category '{binding.Category}' was not found.");

        InputAction? existing = null;
        foreach (InputAction action in actions)
        {
            if (!string.Equals(action.name, binding.Name, StringComparison.Ordinal))
                continue;

            if (existing != null)
                throw new InvalidOperationException(
                    $"More than one Rewired action is named '{binding.Name}'.");

            existing = action;
        }

        if (existing != null)
        {
            if (existing.type != binding.Type ||
                existing.categoryId != targetCategory.id ||
                existing.descriptiveName != binding.Label ||
                existing.userAssignable != binding.UserAssignable)
                throw new InvalidOperationException(
                    $"An existing Rewired action named '{binding.Name}' " +
                    "has a different type, category, display name, or assignability.");

            // Already injected into this UserData: retain its ID and map.
            // Do not run the custom configurator twice.
            binding.ActionId = existing.id;
            return;
        }

        int actionId = binding.StartingActionId;
        while (usedIds.Contains(actionId))
        {
            if (actionId == int.MaxValue)
                throw new InvalidOperationException("No free Rewired action ID remains.");

            actionId++;
        }

        InputAction newAction = (InputAction)(
            Activator.CreateInstance(typeof(InputAction), true) ??
            throw new InvalidOperationException("Could not construct a Rewired InputAction."));

        // Give the configurator the proposed action identity, then restore
        // required fields in case it changed them.
        SetRequiredFields(newAction, binding, targetCategory.id, actionId);
        binding.ConfigureAction?.Invoke(newAction);
        SetRequiredFields(newAction, binding, targetCategory.id, actionId);

        actions.Add(newAction);
        try
        {
            categoryMap.AddAction(targetCategory.id, actionId);
        }
        catch
        {
            actions.Remove(newAction);
            throw;
        }

        usedIds.Add(actionId);
        binding.ActionId = actionId;

        Logger.Log(
            $"Registered '{binding.Label}' ({binding.Type}) as Rewired " +
            $"action {actionId} in category '{targetCategory.name}'.");
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

    internal static T Read<T>(
        InputBinding binding,
        Func<Player, int, T> reader,
        T fallback,
        int playerId)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        try
        {
            if (!binding.Ready)
                return fallback;

            Player? player = ReInput.players?.GetPlayer(playerId);
            if (player == null)
                return fallback;

            T result = reader(player, binding.ActionId);
            binding.ReadFailureLogged = false;
            return result;
        }
        catch (Exception exception)
        {
            // A persistent read failure should not flood the log every frame.
            if (!binding.ReadFailureLogged)
            {
                binding.ReadFailureLogged = true;
                Logger.LogError(
                    $"Failed reading '{binding.Name}' for player {playerId}: {exception}");
            }

            return fallback;
        }
    }

    // private static void DumpDefinitions()
    // {
    //     if (DEBUG_SPAM)
    //     {
    //         Plugin.Log("Controller Maps:");
    //         foreach (ControllerMapLayoutManager_RuleSet_Editor controllerMap in controllerMaps!)
    //         {
    //             Plugin.Log($"  {controllerMap.id} {controllerMap.name}:");
    //             foreach (ControllerMapLayoutManager_Rule_Editor rule in controllerMap.rules)
    //             {
    //                 Plugin.Log($"    {rule.tag} {rule.layoutId}: [{string.Join(", ", rule.categoryIds)}]");
    //             }
    //         }
    //
    //         Plugin.Log("Map Categories:");
    //         foreach (InputMapCategory mapCategory in mapCategories!)
    //         {
    //             Plugin.Log($"  {mapCategory.id}: {mapCategory.name} <{mapCategory.key}> ({mapCategory.tag})");
    //         }
    //
    //         Plugin.Log("Action Categories:");
    //         foreach (InputActionCategory actionCategory in actionCategories!)
    //         {
    //             Plugin.Log($"  {actionCategory.id}: {actionCategory.name} <{actionCategory.key}> ({actionCategory.tag})");
    //         }
    //
    //         Plugin.Log("Actions:");
    //         foreach (InputAction action in actions!)
    //         {
    //             Plugin.Log($"  {action.id}: {action.name} <{action.key}>");
    //         }
    //
    //         Plugin.Log($"Category Map: {categoryMap}");
    //         foreach (ActionCategoryMap.Entry entry in categoryMap.list)
    //         {
    //             Plugin.Log($"  {{Category: {entry.categoryId}, Actions: [{string.Join(", ", entry.ActionIds)}]}}");
    //         }
    //
    //         Plugin.Log("Input Behaviours:");
    //         foreach (InputBehavior behavior in userData.inputBehaviors)
    //         {
    //             Plugin.Log($"  {behavior.id}: {behavior.name} {behavior.ToJsonString()}");
    //         }
    //
    //         Plugin.Log($"{userData.configVars}");
    //
    //         {
    //             Plugin.Log("joystickLayouts");
    //             List<InputLayout> layouts = userData.joystickLayouts;
    //             foreach (InputLayout layout in layouts)
    //             {
    //                 Plugin.Log($"  {layout.id}: {layout.name} <{layout.key}>");
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("keyboardLayouts");
    //             List<InputLayout> layouts = userData.keyboardLayouts;
    //             foreach (InputLayout layout in layouts)
    //             {
    //                 Plugin.Log($"  {layout.id}: {layout.name} <{layout.key}>");
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("mouseLayouts");
    //             List<InputLayout> layouts = userData.mouseLayouts;
    //             foreach (InputLayout layout in layouts)
    //             {
    //                 Plugin.Log($"  {layout.id}: {layout.name} <{layout.key}>");
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("customControllerLayouts");
    //             List<InputLayout> layouts = userData.customControllerLayouts;
    //             foreach (InputLayout layout in layouts)
    //             {
    //                 Plugin.Log($"  {layout.id}: {layout.name} <{layout.key}>");
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("joystickMaps");
    //             List<ControllerMap_Editor> maps = userData.joystickMaps;
    //             foreach (ControllerMap_Editor map in maps)
    //             {
    //                 Plugin.Log($"  {map.id} [{map.categoryId}:{map.layoutId}] {map.name}:");
    //                 foreach (ActionElementMap ae in map.ActionElementMaps)
    //                 {
    //                     Plugin.Log($"    {AeToStringShort(ae, actions)}");
    //                 }
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("keyboardMaps");
    //             List<ControllerMap_Editor> maps = userData.keyboardMaps;
    //             foreach (ControllerMap_Editor map in maps)
    //             {
    //                 Plugin.Log($"  {map.id} [{map.categoryId}:{map.layoutId}] {map.name}:");
    //                 foreach (ActionElementMap ae in map.ActionElementMaps)
    //                 {
    //                     Plugin.Log($"    {AeToStringShort(ae, actions)}");
    //                 }
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("mouseMaps");
    //             List<ControllerMap_Editor> maps = userData.mouseMaps;
    //             foreach (ControllerMap_Editor map in maps)
    //             {
    //                 Plugin.Log($"  {map.id} [{map.categoryId}:{map.layoutId}] {map.name}:");
    //                 foreach (ActionElementMap ae in map.ActionElementMaps)
    //                 {
    //                     Plugin.Log($"    {AeToStringShort(ae, actions)}");
    //                 }
    //             }
    //         }
    //
    //         {
    //             Plugin.Log("customControllerMaps");
    //             List<ControllerMap_Editor> maps = userData.customControllerMaps;
    //             foreach (ControllerMap_Editor map in maps)
    //             {
    //                 Plugin.Log($"  {map.id} [{map.categoryId}:{map.layoutId}] {map.name}:");
    //                 foreach (ActionElementMap ae in map.ActionElementMaps)
    //                 {
    //                     Plugin.Log($"    {AeToStringShort(ae, actions)}");
    //                 }
    //             }
    //         }
    //     }
    // }

}