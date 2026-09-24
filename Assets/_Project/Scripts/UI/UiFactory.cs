using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PhysSim.UI
{
    /// <summary>
    /// Фабрика uGUI-виджетов, собираемых полностью кодом (нулевые ассеты).
    /// Тёмная тема в стиле редакторов: панели #2b2b2b, акцент #3f6fb5.
    /// </summary>
    public static class UiFactory
    {
        public static readonly Color PanelColor = new Color(0.16f, 0.16f, 0.17f, 0.97f);
        public static readonly Color PanelDarkColor = new Color(0.12f, 0.12f, 0.13f, 0.97f);
        public static readonly Color ButtonColor = new Color(0.25f, 0.26f, 0.28f, 1f);
        public static readonly Color ButtonAccentColor = new Color(0.25f, 0.44f, 0.71f, 1f);
        public static readonly Color InputColor = new Color(0.10f, 0.10f, 0.11f, 1f);
        public static readonly Color TextColor = new Color(0.88f, 0.89f, 0.9f, 1f);
        public static readonly Color DimTextColor = new Color(0.6f, 0.62f, 0.65f, 1f);
        public static readonly Color RowHighlightColor = new Color(0.25f, 0.44f, 0.71f, 0.55f);

        private static Font _font;

        public static Font GetFont()
        {
            if (_font != null)
            {
                return _font;
            }

            try
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (Exception)
            {
                try
                {
                    _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch (Exception)
                {
                    _font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                }
            }
            return _font;
        }

        public static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Растянуть по родителю с отступами.</summary>
        public static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static Image AddImage(GameObject go, Color color, bool raycastTarget)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Text AddText(GameObject go, string text, int size, TextAnchor anchor, Color color)
        {
            var label = go.AddComponent<Text>();
            label.font = GetFont();
            label.text = text;
            label.fontSize = size;
            label.alignment = anchor;
            label.color = color;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        public static Button AddButton(GameObject go, Image background)
        {
            var button = go.AddComponent<Button>();
            button.targetGraphic = background;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
            return button;
        }

        /// <summary>
        /// Оформить УЖЕ позиционированный rect как кнопку с подписью.
        /// </summary>
        public static Button DecorateButton(RectTransform rect, string caption, int fontSize,
            Color background, UnityAction onClick)
        {
            var image = AddImage(rect.gameObject, background, true);
            var button = AddButton(rect.gameObject, image);

            var labelRect = NewRect(rect, "Caption");
            Stretch(labelRect, 4f, 2f, 4f, 2f);
            AddText(labelRect.gameObject, caption, fontSize, TextAnchor.MiddleCenter, TextColor);

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            return button;
        }

        /// <summary>Поле ввода (тёмное, с плейсхолдером).</summary>
        public static InputField MakeInputField(Transform parent, string name, string placeholder,
            int fontSize, UnityAction<string> onCommit)
        {
            var rect = NewRect(parent, name);
            var background = AddImage(rect.gameObject, InputColor, true);

            var input = rect.gameObject.AddComponent<InputField>();
            input.targetGraphic = background;
            input.selectionColor = new Color(1f, 1f, 1f, 0.25f);

            var textRect = NewRect(rect, "Text");
            Place(textRect, Vector2.zero, Vector2.one, new Vector2(6f, 1f), new Vector2(-6f, -1f));
            var text = AddText(textRect.gameObject, "", fontSize, TextAnchor.MiddleLeft, TextColor);
            input.textComponent = text;

            var placeholderRect = NewRect(rect, "Placeholder");
            Place(placeholderRect, Vector2.zero, Vector2.one, new Vector2(6f, 1f), new Vector2(-6f, -1f));
            var placeholderText = AddText(placeholderRect.gameObject, placeholder, fontSize,
                TextAnchor.MiddleLeft, DimTextColor);
            input.placeholder = placeholderText;

            if (onCommit != null)
            {
                input.onEndEdit.AddListener(onCommit);
            }
            return input;
        }

        /// <summary>Слайдер (горизонтальный).</summary>
        public static Slider MakeSlider(Transform parent, string name, float min, float max, float value,
            UnityAction<float> onChanged)
        {
            var rect = NewRect(parent, name);

            var backgroundRect = NewRect(rect, "Background");
            Place(backgroundRect, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), Vector2.zero, Vector2.zero);
            AddImage(backgroundRect.gameObject, new Color(0.09f, 0.09f, 0.1f), false);

            var fillAreaRect = NewRect(rect, "Fill Area");
            Place(fillAreaRect, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), new Vector2(6f, 0f), new Vector2(-6f, 0f));
            var fillRect = NewRect(fillAreaRect, "Fill");
            Place(fillRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(fillRect.gameObject, ButtonAccentColor, false);

            var handleAreaRect = NewRect(rect, "Handle Area");
            Place(handleAreaRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-8f, 0f), new Vector2(-8f, 0f));
            var handleRect = NewRect(handleAreaRect, "Handle");
            Place(handleRect, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(-8f, 0f));
            var handleImage = AddImage(handleRect.gameObject, new Color(0.8f, 0.82f, 0.85f), false);

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            if (onChanged != null)
            {
                slider.onValueChanged.AddListener(onChanged);
            }
            return slider;
        }

        /// <summary>Чекбокс с подписью.</summary>
        public static Toggle MakeToggle(Transform parent, string name, string caption, bool isOn,
            UnityAction<bool> onChanged)
        {
            var rect = NewRect(parent, name);

            var checkRect = NewRect(rect, "Check");
            Place(checkRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, -9f), new Vector2(18f, 9f));
            var checkBackground = AddImage(checkRect.gameObject, InputColor, true);

            var markRect = NewRect(checkRect, "Mark");
            Place(markRect, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            var markImage = AddImage(markRect.gameObject, ButtonAccentColor, false);

            var labelRect = NewRect(rect, "Caption");
            Place(labelRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(24f, 0f), new Vector2(-4f, 0f));
            AddText(labelRect.gameObject, caption, 13, TextAnchor.MiddleLeft, TextColor);

            var toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = checkBackground;
            toggle.graphic = markImage;
            toggle.isOn = isOn;

            if (onChanged != null)
            {
                toggle.onValueChanged.AddListener(onChanged);
            }
            return toggle;
        }

        /// <summary>Скролл-список: возвращает ScrollRect и content (строки добавляет вызывающий).</summary>
        public static ScrollRect MakeScrollList(Transform parent, string name, out RectTransform content)
        {
            var rect = NewRect(parent, name);
            AddImage(rect.gameObject, PanelDarkColor, true);

            var viewportRect = NewRect(rect, "Viewport");
            Stretch(viewportRect, 0f, 0f, 2f, 0f);
            viewportRect.gameObject.AddComponent<RectMask2D>();

            content = NewRect(viewportRect, "Content");
            Place(content, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0.5f, 1f);

            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 16f;
            return scroll;
        }

        /// <summary>Строка списка: кнопка-строка с текстом и опциональным цветным свотчем.</summary>
        public static Button MakeRow(RectTransform content, int index, float rowHeight,
            string caption, int fontSize, Color? swatch, UnityAction onClick)
        {
            var rect = NewRect(content, $"Row_{index}");
            Place(rect, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(2f, 0f), new Vector2(-2f, 0f));
            rect.anchoredPosition = new Vector2(0f, -index * rowHeight);
            rect.sizeDelta = new Vector2(0f, rowHeight);

            var background = AddImage(rect.gameObject, new Color(0, 0, 0, 0), true);
            var button = AddButton(rect.gameObject, background);

            float textOffset = 6f;
            if (swatch.HasValue)
            {
                var swatchRect = NewRect(rect, "Swatch");
                Place(swatchRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(4f, -8f), new Vector2(20f, 8f));
                AddImage(swatchRect.gameObject, swatch.Value, false);
                textOffset = 26f;
            }

            var labelRect = NewRect(rect, "Caption");
            Place(labelRect, Vector2.zero, Vector2.one, new Vector2(textOffset, 0f), new Vector2(-6f, 0f));
            AddText(labelRect.gameObject, caption, fontSize, TextAnchor.MiddleLeft, TextColor);

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            return button;
        }

        /// <summary>Разбор числа из поля ввода: точка или запятая, инвариантная культура.</summary>
        public static bool TryParseFloat(string text, out float value)
        {
            if (string.IsNullOrEmpty(text))
            {
                value = 0f;
                return false;
            }

            return float.TryParse(text.Trim().Replace(',', '.'),
                NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static string Format(float value, string format = "0.###")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
