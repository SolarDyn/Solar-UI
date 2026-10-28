// InputDelegate.cs
// Solar Dynamics 2026

#region

using System;
using Rewired;

#endregion

namespace Solar.UI.Input;

public class InputDelegate
{
    public Action<InputActionEventData> action;
    public InputBinding binding;
    public InputActionEventType inputType;
    public int playerId;

    public InputDelegate(InputActionEventType inputType, Action<InputActionEventData> action, InputBinding binding,
        int playerId = 0)
    {
        this.binding = binding;
        this.action = action;
        this.inputType = inputType;
        this.playerId = playerId;
    }

    public void Dispose()
    {
        binding.RemoveDelegate(this);
    }

    internal void _AddEventDelegate()
    {
        if (!binding.IsRegistered)
            throw new Exception(
                $"$Failed adding event delegate [P{playerId}] {binding.Name}[{binding.ActionId}]<{inputType.ToString()}>: Binding not registered.");

        try
        {
            Player? player = ReInput.players?.GetPlayer(playerId);
            if (player == null)
                throw new ArgumentException($"Player {playerId} not found");

            player.AddInputEventDelegate(action, UpdateLoopType.Update, inputType, binding.ActionId);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"$Failed adding event delegate [P{playerId}] {binding.Name}[{binding.ActionId}]<{inputType.ToString()}>: Exception:\r{exception}");
        }
    }

    internal void _RemoveEventDelegate()
    {
        if (!binding.IsRegistered)
            throw new Exception(
                $"$Failed removing event delegate [P{playerId}] {binding.Name}[{binding.ActionId}]<{inputType.ToString()}>: Binding not registered.");

        try
        {
            Player? player = ReInput.players?.GetPlayer(playerId);
            if (player == null)
                throw new ArgumentException($"Player {playerId} not found");

            player.RemoveInputEventDelegate(action, UpdateLoopType.Update, inputType, binding.ActionId);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"$Failed removing event delegate [P{playerId}] {binding.Name}[{binding.ActionId}]<{inputType.ToString()}>: Exception:\r{exception}");
        }
    }
}