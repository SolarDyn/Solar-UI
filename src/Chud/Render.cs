// Render.cs
// Solar Dynamics 2026

#region

using TMPro;
using UnityEngine;

#endregion

namespace Solar.UI.Chud;

public class Render
{
    public abstract class ChudElement
    {
        public GameObject element;
        public Transform viewpoint;

        public ChudElement(
            string name,
            Transform viewpoint,
            Transform parent,
            Vector3 localPosition)
        {
            this.viewpoint = viewpoint;

            element = new GameObject(name);
            element.layer = CollimatedHUD.UILayer;
            element.transform.SetParent(parent, false);

            SetLocalPosition(localPosition);

            UpdateRotation();
        }

        public void SetEnabled(bool enabled)
        {
            element.SetActive(enabled);
        }

        public void SetPosition(Vector3 position)
        {
            element.transform.position = position;
        }

        public void SetLocalPosition(Vector3 position)
        {
            element.transform.localPosition = position;
        }

        public void UpdateRotation(float rotationOffset = 0f, Vector3? up = null)
        {
            Vector3 forward = element.transform.position - viewpoint.position;
            // Vector3 rot = -element.transform.localPosition.normalized;
            if (up.HasValue)
                element.transform.rotation = Quaternion.LookRotation(forward, up.Value) *
                                             Quaternion.Euler(0f, 0f, rotationOffset);
            else
                element.transform.rotation = Quaternion.LookRotation(forward, viewpoint.up) *
                                             Quaternion.Euler(0f, 0f, rotationOffset);
        }
    }

    public class ChudSprite : ChudElement
    {
        public SpriteRenderer spriteRenderer;

        public ChudSprite(
            string name,
            Transform viewpoint,
            Transform parent,
            Vector3 localPosition,
            Sprite sprite,
            float scale = 1f) : base(name, viewpoint, parent, localPosition)
        {
            spriteRenderer = element.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.transform.localScale = Vector3.one * scale;
            spriteRenderer.material = Assets.Materials.Get("Text_additive");
        }

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }
    }

    public class ChudText : ChudElement
    {
        public TextMeshProUGUI tmp;
        public Canvas canvas;

        public ChudText(
            string name,
            Transform viewpoint,
            Transform parent,
            Vector3 localPosition,
            string text = "",
            TextAlignmentOptions alignment = TextAlignmentOptions.Center) : base(name, viewpoint, parent, localPosition)
        {
            tmp = element.AddComponent<TextMeshProUGUI>();
            canvas = element.AddComponent<Canvas>();

            tmp.alignment = alignment;

            float pivotX = tmp.horizontalAlignment switch
            {
                HorizontalAlignmentOptions.Left => 0,
                HorizontalAlignmentOptions.Right => 1,
                _ => 0.5f,
            };
            float pivotY = tmp.verticalAlignment switch
            {
                VerticalAlignmentOptions.Top => 0,
                VerticalAlignmentOptions.Bottom => 1,
                _ => 0.5f,
            };

            RectTransform rect = element.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            //rect.sizeDelta = new Vector2(1000f, 1000f);
            rect.pivot = new Vector2(pivotX, pivotY);

            tmp.font = Assets.TMP_FontAssets.Get("Brass Mono Regular");
            tmp.fontMaterial = Assets.Materials.Get("Brass Mono Regular Thicker Additive");
            // tmp.material = Assets.Materials.Get("Brass Mono Regular Thicker Additive");
            tmp.text = text;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
        }

        public void SetColor(Color color)
        {
            tmp.color = color;
        }

        public void SetFontSize(float fontSize)
        {
            tmp.fontSize = fontSize;
        }
    }
}