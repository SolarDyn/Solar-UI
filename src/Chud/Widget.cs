// Widget.cs
// Solar Dynamics 2026

#region

using NuclearOption.UIStyleSystem;

#endregion

namespace Solar.UI.Chud;

public abstract class Widget : HUDApp
{
    // For whatever reason, ChudText requires scale to be around 1200-1600 for font size to be reasonable.
    public const float scale = 2000f;
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

    // public override void RefreshSettings()
    // {
    //     base.RefreshSettings();
    //     // Do shit.
    // }

    // public override void Refresh()
    // {
    //     // Do shit.
    // }
}