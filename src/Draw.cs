// Draw.cs
// Solar Dynamics 2026

#region

using NuclearOption.UIStyleSystem;
using TMPro;
using UnityEngine;

#endregion

namespace Solar.UI;

public static class Draw
{
    public static HUDAppManager GetHUDAppManager()
    {
        return SceneSingleton<HUDAppManager>.i;
    }

    public class UIElement
    {
        public GameObject container;
        public RectTransform containerRect;

        public UIElement(
            string name,
            Transform parent,
            Vector2 position)
        {
            container = new GameObject(name);
            container.transform.SetParent(parent, false);

            containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = Vector2.zero;
            containerRect.localScale = Vector3.one * 0.5f;
            containerRect.anchorMin = Vector2.one * 0.5f;
            containerRect.anchorMax = Vector2.one * 0.5f;
            SetPosition(position);
        }

        public void SetEnabled(bool enabled)
        {
            container.SetActive(enabled);
        }

        public void SetPosition(Vector2 position)
        {
            containerRect.anchoredPosition = position;
        }
    }

    public class UIText : UIElement
    {
        public TextMeshProUGUI textComponent;
        public TextStyleApplier textStyleApplier;

        public UIText(
            string name,
            Transform parent,
            Vector2 position,
            Vector2? size = null,
            Color? color = null,
            int fontSize = 36,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            ThemeManager.ThemeContext themeContext = ThemeManager.ThemeContext.HUD,
            StyleLabel? styleLabel = null,
            string text = "") : base(name, parent, position)
        {
            containerRect.sizeDelta = size ?? new Vector2(0, 0);

            textComponent = container.AddComponent<TextMeshProUGUI>();
            textComponent.alignment = alignment;

            float pivotX = textComponent.horizontalAlignment switch
            {
                HorizontalAlignmentOptions.Left => 0,
                HorizontalAlignmentOptions.Right => 1,
                _ => 0.5f
            };
            float pivotY = textComponent.verticalAlignment switch
            {
                VerticalAlignmentOptions.Top => 0,
                VerticalAlignmentOptions.Bottom => 1,
                _ => 0.5f
            };
            containerRect.pivot = new Vector2(pivotX, pivotY);

            textComponent.color = color ?? Color.white;
            textComponent.font = Assets.FontDefaultHUD;
            textComponent.material = Assets.MaterialDefaultHUD;
            textComponent.text = text;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            textComponent.enableWordWrapping = false;

            SetFontSize(fontSize);

            textStyleApplier = container.AddComponent<TextStyleApplier>();
            textStyleApplier.Context = themeContext;
            textStyleApplier.initialMaterial = Assets.MaterialDefaultHUD;
            textStyleApplier.styleLabel =
                styleLabel ?? Assets.StyleLabels[ThemeManager.ThemeContext.HUD]["HUD_TextMainColor"];
        }

        public void SetText(string text)
        {
            textComponent.text = text;
            // containerRect.sizeDelta = new Vector2(textComponent.preferredWidth, textComponent.fontSize);
        }

        public void SetColor(Color color)
        {
            textComponent.color = color;
        }

        public void SetFontSize(int fontSize)
        {
            textComponent.fontSize = fontSize;
            // containerRect.sizeDelta = new Vector2(textComponent.preferredWidth, textComponent.fontSize);
        }
    }
}