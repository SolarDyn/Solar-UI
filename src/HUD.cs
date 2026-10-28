// HUD.cs
// Solar Dynamics 2026

#region

using System;
using System.Reflection;
using HarmonyLib;
using NuclearOption.UIStyleSystem;
using Solar.UI.Chud;

#endregion

namespace Solar.UI;

internal static class HUDPlugin
{
    private static bool _installed;

    internal static void Install()
    {
        if (_installed)
            throw new InvalidOperationException("HUDPlugin is already installed.");

        MethodInfo initialize = AccessTools.Method(typeof(HUDAppManager), nameof(HUDAppManager.Start), Type.EmptyTypes)
                                ?? throw new MissingMethodException(typeof(HUDAppManager).FullName, nameof(HUDAppManager.Start));

        Harmony harmony = new Harmony("Solar.UI.HUD");
        try
        {
            harmony.Patch(initialize, prefix: new HarmonyMethod(
                typeof(HUDPlugin), nameof(Prefix)) { priority = Priority.HigherThanNormal });
            _installed = true;
        }
        catch
        {
            harmony.UnpatchSelf();
            throw;
        }
    }

    private static bool Prefix(HUDAppManager __instance)
    {
        Aircraft aircraft = SceneSingleton<CombatHUD>.i.aircraft;

        return true;
    }
}

public class HUD
{
    public abstract class HUDWidget : HUDApp
    {
        protected Aircraft aircraft;

        protected bool initialized;

        public void OnDestroy()
        {
            NuclearOption.UIStyleSystem.ThemeManager.ThemeGroupChanged -= OnThemeGroupChanged;
        }

        public override void Initialize(Aircraft aircraft)
        {
            this.aircraft = aircraft;

            RefreshSettings();

            NuclearOption.UIStyleSystem.ThemeManager.ThemeGroupChanged += OnThemeGroupChanged;

            initialized = true;
        }

        public abstract void SetEnabled(bool isEnabled);

        public abstract void OnThemeGroupChanged();

        public override void RefreshSettings()
        {
            base.RefreshSettings();
            SetEnabled(PlayerSettings.gauges);
        }

        // public virtual void Refresh()
        // {
        //
        // }
    }
}