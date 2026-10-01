// HUD.cs
// Solar Dynamics 2026

#region

using HarmonyLib;
using NuclearOption.UIStyleSystem;

#endregion

namespace Solar.UI;

[HarmonyPatch(typeof(HUDAppManager), "Start")]
internal static class HUDPlugin
{
    private static bool Prefix(HUDAppManager __instance)
    {
        Aircraft aircraft = SceneSingleton<CombatHUD>.i.aircraft;

        return true;
    }
}

public class HUD
{
    public abstract class Widget : HUDApp
    {
        protected Aircraft aircraft;

        public void OnDestroy()
        {
            ThemeManager.ThemeGroupChanged -= OnThemeGroupChanged;
        }

        public override void Initialize(Aircraft aircraft)
        {
            this.aircraft = aircraft;

            RefreshSettings();

            Refresh();

            ThemeManager.ThemeGroupChanged += OnThemeGroupChanged;
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