// InputState.cs
// Solar Dynamics 2026

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Rewired;
using Solar.UI.Util;

namespace Solar.UI.Input;


public class InputState
{
    public bool State
    {
        get { return state; }
    }

    public event Action<bool> EventStateChanged = delegate { };
    public event Action EventStateOn = delegate { };
    public event Action EventStateOff = delegate { };

    private const string ColorSuffix = "<color=#FFFF7F>";

    private bool state = false;
    private bool stateHeld = false;
    private bool stateLatch = false;

    private bool stateCache;

    public InputBinding? bindingHold;
    public InputBinding? bindingToggle;
    public InputBinding? bindingOn;
    public InputBinding? bindingOff;

    public bool enabledToggle = true;
    public bool enabledHold = false;
    public bool enabledLatch = true;

    public string Name { get; }
    public string Category { get; }
    public string Label { get; }
    public string LabelSuffix { get; }
    public InputActionType Type { get; }
    public bool UserAssignable { get; }
    public int StartingId { get; }

    public InputState(
        string name,
        string category,
        string label,
        string labelSuffix = "",
        bool userAssignable = true,
        int startingId = 128)
    {
        Name = name;
        Label = label;
        LabelSuffix = labelSuffix;
        Category = category;
        Type = InputActionType.Button;
        UserAssignable = userAssignable;
        StartingId = startingId;

        InputStateConfig.AddState(this);

        if (enabledToggle)
        {
            bindingToggle = BindingHandler.Register(
                $"{Name}_Toggle",
                Category,
                Label,
                LabelSuffix + ColorSuffix + " <Toggle>",
                type: InputActionType.Button,
                userAssignable: UserAssignable,
                startingActionId: StartingId);
        }

        if (enabledHold)
        {
            bindingHold = BindingHandler.Register(
                $"{Name}_Hold",
                Category,
                Label,
                LabelSuffix + ColorSuffix + "   <Hold>",
                type: InputActionType.Button,
                userAssignable: UserAssignable,
                startingActionId: StartingId);
        }

        if (enabledLatch)
        {
            bindingOn = BindingHandler.Register(
                $"{Name}_On",
                Category,
                Label,
                LabelSuffix + ColorSuffix + "     <On>",
                type: InputActionType.Button,
                userAssignable: UserAssignable,
                startingActionId: StartingId);

            bindingOff = BindingHandler.Register(
                $"{Name}_Off",
                Category,
                Label,
                LabelSuffix + ColorSuffix + "    <Off>",
                type: InputActionType.Button,
                userAssignable: UserAssignable,
                startingActionId: StartingId);
        }
    }

    internal void Init()
    {
        Plugin.Log($"InputState.Init() called for state {Name}.");
        Plugin.Log($"{enabledToggle} {enabledHold} {enabledLatch}");
        Plugin.Log($"{bindingToggle is not null} {bindingHold is not null} {bindingOn is not null} {bindingOff is not null}");

        bindingHold?.AddDelegate(InputActionEventType.ButtonJustPressed, Hold);
        bindingHold?.AddDelegate(InputActionEventType.ButtonJustReleased, Hold);

        bindingToggle?.AddDelegate(InputActionEventType.ButtonJustPressed, Toggle);

        bindingOn?.AddDelegate(InputActionEventType.ButtonJustPressed, On);

        bindingOff?.AddDelegate(InputActionEventType.ButtonJustPressed, Off);
    }

    private void Toggle(InputActionEventData data)
    {
        stateLatch = !stateLatch;

        UpdateState();
    }

    private void Hold(InputActionEventData data)
    {
        stateHeld = data.eventType switch
        {
            InputActionEventType.ButtonJustPressed => true,
            InputActionEventType.ButtonJustReleased => false,
            _ => stateHeld,
        };

        UpdateState();
    }

    private void On(InputActionEventData data)
    {
        stateLatch = true;

        UpdateState();
    }

    private void Off(InputActionEventData data)
    {
        stateLatch = false;

        UpdateState();
    }

    private void UpdateState()
    {
        stateCache = stateHeld || stateLatch;

        if (stateCache == state)
            return;

        state = stateCache;

        if (state)
        {
            EventStateOn.Invoke();
        }
        else
        {
            EventStateOff.Invoke();
        }

        EventStateChanged.Invoke(state);
    }


}

public static class InputStateConfig
{
    private static JsonConfig config;

    private static Dictionary<string, InputState> inputStates = new();

    private static JObject defaults;

    static InputStateConfig()
    {
        defaults = new JObject
        {
            ["states"] = new JObject(),
        };

        config = new JsonConfig(
            typeof(Plugin).Assembly,
            "config/input.json",
            defaults);
    }

    internal static void AddState(InputState state)
    {
        inputStates[state.Name] = state;

        JObject? stateConfig = config.Data.Value<JObject>("states")?.Value<JObject>(state.Name);

        if (stateConfig is not null)
        {
            state.enabledToggle = stateConfig.Value<bool>("enabledToggle");
            state.enabledHold = stateConfig.Value<bool>("enabledHold");
            state.enabledLatch = stateConfig.Value<bool>("enabledLatch");
        }
    }

    internal static void Save()
    {
        foreach (InputState state in inputStates.Values)
        {
            defaults["states"]![state.Name] = new JObject
            {
                ["enabledToggle"] = state.enabledToggle,
                ["enabledHold"] = state.enabledHold,
                ["enabledLatch"] = state.enabledLatch,
            };
        }

        config.UpdateDefaults(defaults);
    }
}