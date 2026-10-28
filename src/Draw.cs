// Draw.cs
// Solar Dynamics 2026

#region

using NuclearOption.UIStyleSystem;
using Solar.UI.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Solar.UI;

public static class Draw
{
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
            NuclearOption.UIStyleSystem.ThemeManager.ThemeContext themeContext = NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD,
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
                _ => 0.5f,
            };
            float pivotY = textComponent.verticalAlignment switch
            {
                VerticalAlignmentOptions.Top => 0,
                VerticalAlignmentOptions.Bottom => 1,
                _ => 0.5f,
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
                styleLabel ?? Assets.StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD]["HUD_TextMainColor"];
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

    public class UILine : UIElement
    {
        public Vector2 end;
        public Image imageComponent;
        public ImageStyleApplier imageStyleApplier;
        public Vector2 start;
        public float thickness;

        public UILine(
            string name,
            Transform parent,
            Vector2 start,
            Vector2 end,
            Color? color = null,
            float thickness = 2f,
            NuclearOption.UIStyleSystem.ThemeManager.ThemeContext themeContext = NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD,
            StyleLabel? styleLabel = null) : base(name, parent, (start + end) / 2f)
        {
            this.start = start;
            this.end = end;
            this.thickness = thickness;

            Vector2 direction = end - start;
            float length = direction.magnitude;

            imageComponent = container.AddComponent<Image>();

            if (true)
            {
                imageComponent.sprite = DrawUtil.GetAntialiasedSprite();
                imageComponent.type = Image.Type.Sliced;
            }
            else
            {
                imageComponent.sprite = null;
                imageComponent.type = Image.Type.Simple;
            }

            imageComponent.color = color ?? Color.white;
            imageComponent.material = Assets.MaterialDefaultHUD;
            // imageComponent.rectTransform.sizeDelta = new Vector2(thickness, thickness);

            containerRect.sizeDelta = new Vector2(length, thickness);
            containerRect.pivot = Vector2.one * 0.5f;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            containerRect.localRotation = Quaternion.Euler(0, 0, angle);

            imageStyleApplier = container.AddComponent<ImageStyleApplier>();
            imageStyleApplier.Context = themeContext;
            imageStyleApplier.styleLabel =
                styleLabel ?? Assets.StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD]["HUD_ImageMainColor"];
        }

        public void SetColor(Color color)
        {
            imageComponent.color = color;
        }

        public void SetThickness(float thickness)
        {
            this.thickness = thickness;
            UpdateGeometry();
        }

        public void SetCoordinates(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            UpdateGeometry();
        }

        public void UpdateGeometry()
        {
            Vector2 direction = end - start;
            float length = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            containerRect.sizeDelta = new Vector2(length, thickness);
            containerRect.anchoredPosition = (start + end) / 2f;
            containerRect.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public class UILinePivot : UIElement
    {
        public float angle;
        public Image imageComponent;
        public ImageStyleApplier imageStyleApplier;
        public float length;
        public Vector2 position;
        public float thickness;

        public UILinePivot(
            string name,
            Transform parent,
            Vector2 position,
            float length,
            float angle,
            Color? color = null,
            float thickness = 4f,
            NuclearOption.UIStyleSystem.ThemeManager.ThemeContext themeContext = NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD,
            StyleLabel? styleLabel = null) : base(name, parent, position)
        {
            this.position = position;
            this.length = length;
            this.angle = angle;

            this.thickness = thickness;

            imageComponent = container.AddComponent<Image>();

            if (true)
            {
                imageComponent.sprite = DrawUtil.GetAntialiasedSprite();
                imageComponent.type = Image.Type.Sliced;
                containerRect.sizeDelta += new Vector2(2, 2);
            }
            else
            {
                imageComponent.sprite = null;
                imageComponent.type = Image.Type.Simple;
            }

            imageComponent.color = color ?? Color.white;
            imageComponent.material = Assets.MaterialDefaultHUD;
            // imageComponent.rectTransform.sizeDelta = new Vector2(thickness, thickness);

            containerRect.sizeDelta = new Vector2(length, thickness);
            containerRect.pivot = new Vector2(0f, 0.5f);

            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            containerRect.localRotation = Quaternion.Euler(0, 0, angle);

            imageStyleApplier = container.AddComponent<ImageStyleApplier>();
            imageStyleApplier.Context = themeContext;
            imageStyleApplier.styleLabel =
                styleLabel ?? Assets.StyleLabels[NuclearOption.UIStyleSystem.ThemeManager.ThemeContext.HUD]["HUD_ImageMainColor"];
        }

        public void SetColor(Color color)
        {
            imageComponent.color = color;
        }

        public void SetThickness(float thickness)
        {
            this.thickness = thickness;
            Vector2 _sizeDelta = containerRect.sizeDelta;
            _sizeDelta.y = thickness;
            containerRect.sizeDelta = _sizeDelta;
        }

        public void SetPosition(Vector3? position, float? length, float? angle, Vector3? toPosition)
        {
            if (position.HasValue)
                this.position = position.Value;
            if (length.HasValue)
                this.length = length.Value;
            if (angle.HasValue)
                this.angle = angle.Value;
            else if (toPosition.HasValue)
                this.angle = Mathf.Atan2(toPosition.Value.y, toPosition.Value.x) * Mathf.Rad2Deg;

            UpdateGeometry();
        }

        public void UpdateGeometry()
        {
            // containerRect.anchoredPosition = position;
            // containerRect.localPosition = position;
            containerRect.position = position;
            Vector2 _sizeDelta = containerRect.sizeDelta;
            _sizeDelta.x = length;
            containerRect.sizeDelta = _sizeDelta;
            containerRect.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}