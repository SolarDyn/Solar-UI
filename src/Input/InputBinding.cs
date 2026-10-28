// InputBinding.cs
// Solar Dynamics 2026

#region

using System;
using System.Collections.Generic;
using Rewired;

#endregion

namespace Solar.UI.Input;

/// <summary>
///     A reusable handle for one action. Rewired combines input from the player's
///     mapped keyboard, mouse, joysticks, and custom controllers.
/// </summary>
public sealed class InputBinding
{
    public List<InputDelegate> delegates = [];

    internal InputBinding(
        string name,
        string category,
        string label,
        string labelSuffix,
        string labelPos,
        string labelPosSuffix,
        string labelNeg,
        string labelNegSuffix,
        InputActionType type,
        bool userAssignable,
        int startingActionId,
        Action<InputAction>? configureAction)
    {
        Name = name;
        Category = category;
        Label = label;
        LabelSuffix = labelSuffix;
        LabelPos = labelPos;
        LabelPosSuffix = labelPosSuffix;
        LabelNeg = labelNeg;
        LabelNegSuffix = labelNegSuffix;
        Type = type;
        UserAssignable = userAssignable;
        StartingActionId = startingActionId;
        ConfigureAction = configureAction;
    }

    public string Name { get; }
    public string Category { get; }
    public string Label { get; }
    public string LabelSuffix { get; }
    public string LabelPos { get; }
    public string LabelPosSuffix { get; }
    public string LabelNeg { get; }
    public string LabelNegSuffix { get; }
    public InputActionType Type { get; }
    public bool UserAssignable { get; }

    /// <summary>Runtime ID, or -1 before successful UserData registration.</summary>
    public int ActionId { get; internal set; } = -1;

    internal int StartingActionId { get; }
    internal Action<InputAction>? ConfigureAction { get; }
    internal bool ReadFailureLogged { get; set; }

    public bool IsRegistered => ActionId >= 0;

    /// <summary>
    ///     UserData registration and runtime initialization are separate stages.
    ///     Check the active runtime action so a stale ID cannot read another action.
    /// </summary>
    public bool Ready
    {
        get
        {
            if (!IsRegistered || !ReInput.isReady)
                return false;

            InputAction? action = ReInput.mapping.GetAction(ActionId);
            return action != null &&
                   action.type == Type &&
                   string.Equals(action.name, Name, StringComparison.Ordinal);
        }
    }

    public bool GetButtonDown(int playerId = 0)
    {
        return Read(static (player, id) => player.GetButtonDown(id), false, playerId);
    }

    public bool GetButton(int playerId = 0)
    {
        return Read(static (player, id) => player.GetButton(id), false, playerId);
    }

    public bool GetButtonUp(int playerId = 0)
    {
        return Read(static (player, id) => player.GetButtonUp(id), false, playerId);
    }

    public bool GetNegativeButtonDown(int playerId = 0)
    {
        return Read(static (player, id) => player.GetNegativeButtonDown(id), false, playerId);
    }

    public bool GetNegativeButton(int playerId = 0)
    {
        return Read(static (player, id) => player.GetNegativeButton(id), false, playerId);
    }

    public bool GetNegativeButtonUp(int playerId = 0)
    {
        return Read(static (player, id) => player.GetNegativeButtonUp(id), false, playerId);
    }

    public float GetAxis(int playerId = 0)
    {
        return Read(static (player, id) => player.GetAxis(id), 0f, playerId);
    }

    public float GetAxisRaw(int playerId = 0)
    {
        return Read(static (player, id) => player.GetAxisRaw(id), 0f, playerId);
    }

    /// <summary>
    ///     Use any additional Rewired polling method without extending the handler.
    ///     Example: Read((player, id) => player.GetButtonDoublePressDown(id), false).
    ///     Cache a capturing delegate yourself if you call it every frame.
    /// </summary>
    public T Read<T>(Func<Player, int, T> reader, T fallback, int playerId = 0)
    {
        return BindingHandler.Read(this, reader, fallback, playerId);
    }

    // [UsedImplicitly]
    // public bool AddEventDelegate(Action<InputActionEventData> action, InputActionEventType inputType, int playerId = 0)
    // {
    //     return InputHandler.AddEventDelegate(action, inputType, this, playerId);
    // }

    public InputDelegate AddDelegate(
        InputActionEventType inputType,
        Action<InputActionEventData> action,
        int playerId = 0)
    {
        InputDelegate inputDelegate = new(inputType, action, this, playerId);
        delegates.Add(inputDelegate);
        inputDelegate._AddEventDelegate();
        return inputDelegate;
    }

    public InputDelegate AddDelegate(
        InputActionEventType inputType,
        Action<InputActionEventData> action,
        float threshold,
        int playerId = 0)
    {
        InputDelegate inputDelegate = new(inputType, action, this, playerId);
        delegates.Add(inputDelegate);
        inputDelegate._AddEventDelegate();
        return inputDelegate;
    }

    public void RemoveDelegate(InputDelegate inputDelegate)
    {
        delegates.Remove(inputDelegate);
    }

    public void ClearDelegates()
    {
        foreach (InputDelegate inputDelegate in delegates) RemoveDelegate(inputDelegate);
        delegates.Clear();
    }
}