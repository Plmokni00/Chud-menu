using Chud.Menu;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.UI
{
    public sealed class ModernTextLayer
    {
        public const float MaxStretch = 1.6f;

        public static readonly Quaternion Orientation = Quaternion.Euler(180f, 90f, 90f);

        private readonly Dictionary<string, Text> _labels = new Dictionary<string, Text>(32);
        private readonly Dictionary<string, float> _stretch = new Dictionary<string, float>(32);
        private readonly MenuFont _fonts;
        private GameObject _root;

        public ModernTextLayer(MenuFont fonts)
        {
            _fonts = fonts;
        }

        public GameObject Root => _root;

        public IReadOnlyDictionary<string, Text> Labels => _labels;

        public void Build(Transform parent, float originX)
        {
            Clear();

            var root = new GameObject("chud_text", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(originX, 0f, 0f);
            root.transform.localRotation = Orientation;
            root.transform.localScale = Vector3.one;

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            _root = root;
            WristMenu.CanvasObj = root;
        }

        public Text Add(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color)
        {
            _stretch.Remove(id);
            return Create(id, content, target, maxWidth, maxHeight, color, false);
        }

        public Text AddStretched(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color)
        {
            return Create(id, content, target, maxWidth, maxHeight, color, true);
        }

        public void SetText(string id, string content)
        {
            if (string.IsNullOrEmpty(id) || !_labels.TryGetValue(id, out Text label) || label == null)
            {
                return;
            }

            string next = content ?? string.Empty;
            if (label.text == next)
            {
                return;
            }

            float width = Mathf.Max(0.001f, label.rectTransform.sizeDelta.x / MenuFont.PixelsPerUnit);
            float height = Mathf.Max(0.001f,
                label.rectTransform.sizeDelta.y / MenuFont.PixelsPerUnit * (MenuFont.CapRatio / MenuFont.LineRatio));

            int newSize = _fonts.Fit(next, width, height);
            label.fontSize = newSize;
            label.text = next;

            if (_stretch.TryGetValue(id, out float saved))
            {
                float stretch = _fonts.FillStretch(next, newSize, width, MaxStretch);
                _stretch[id] = stretch;
                label.rectTransform.localScale = new Vector3(stretch / MenuFont.PixelsPerUnit,
                    1f / MenuFont.PixelsPerUnit, 1f / MenuFont.PixelsPerUnit);
            }
        }

        public void SetColor(string id, Color color)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (_labels.TryGetValue(id, out Text label) && label != null)
            {
                label.color = color;
            }
        }

        public RectTransform FindLabel(string id)
        {
            if (!string.IsNullOrEmpty(id) && _labels.TryGetValue(id, out Text label) && label != null)
            {
                return label.rectTransform;
            }

            return null;
        }

        public Vector3 LabelScale(string id, float multiplier)
        {
            float stretch = 1f;
            if (!string.IsNullOrEmpty(id) && _stretch.TryGetValue(id, out float saved) && saved > 0.01f)
            {
                stretch = saved;
            }

            return new Vector3(
                stretch * multiplier / MenuFont.PixelsPerUnit,
                multiplier / MenuFont.PixelsPerUnit,
                multiplier / MenuFont.PixelsPerUnit);
        }

        public void Clear()
        {
            _labels.Clear();
            _stretch.Clear();
            if (_root != null)
            {
                Object.Destroy(_root);
            }

            _root = null;
            WristMenu.CanvasObj = null;
            WristMenu.FpsText = null;
        }

        private Text Create(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color, bool stretch)
        {
            if (_root == null || WristMenu.Menu == null)
            {
                return null;
            }

            string value = content ?? string.Empty;
            int fontSize = _fonts.Fit(value, maxWidth, maxHeight);
            Font font = _fonts.Get(fontSize);
            if (font == null)
            {
                return null;
            }

            float stretchFactor = stretch ? _fonts.FillStretch(value, fontSize, maxWidth, MaxStretch) : 0f;

            var go = new GameObject("text_" + id, typeof(RectTransform));
            go.transform.SetParent(_root.transform, false);

            var rect = go.GetComponent<RectTransform>();
            var label = go.AddComponent<Text>();
            label.font = font;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.fontSize = fontSize;
            label.text = value;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(
                maxWidth * MenuFont.PixelsPerUnit,
                maxHeight * MenuFont.PixelsPerUnit * (MenuFont.LineRatio / MenuFont.CapRatio));

            if (stretch)
            {
                rect.localScale = new Vector3(stretchFactor / MenuFont.PixelsPerUnit,
                    1f / MenuFont.PixelsPerUnit, 1f / MenuFont.PixelsPerUnit);
            }
            else
            {
                rect.localScale = Vector3.one / MenuFont.PixelsPerUnit;
            }

            rect.localRotation = Quaternion.identity;
            rect.localPosition = new Vector3(-target.y, target.z, 0f);

            if (!string.IsNullOrEmpty(id))
            {
                _labels[id] = label;
                if (stretch)
                {
                    _stretch[id] = stretchFactor;
                }
            }

            return label;
        }
    }
}