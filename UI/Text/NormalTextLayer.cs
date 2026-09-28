using Chud.Menu;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.UI
{
    public sealed class NormalTextLayer
    {
        public const int FontSize = 200;
        public const int FontMinSize = 0;
        public const int FontMaxSize = 200;
        public const float DynamicPixelsPerUnit = 1900f;
        public const float ReferencePixelsPerUnit = 100f;

        public const string FontFamily = "Comic Sans MS";

        public static readonly Quaternion Rotation = Quaternion.Euler(180f, 90f, 90f);

        private readonly Dictionary<string, Text> _byId = new Dictionary<string, Text>(32);
        private GameObject _root;
        private Font _font;

        public GameObject Root => _root;

        public IReadOnlyDictionary<string, Text> ById => _byId;

        private static Font CreateFont()
        {
            Font font = null;

            try
            {
                font = Font.CreateDynamicFontFromOSFont(FontFamily, FontSize);
            }
            catch (System.Exception)
            {
                font = null;
            }

            if (font != null)
            {
                return font;
            }

            try
            {
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public void Build(Transform parent)
        {
            Clear();

            if (_font == null)
            {
                _font = CreateFont();
            }

            var root = new GameObject();
            root.transform.parent = parent;

            var canvas = root.AddComponent<Canvas>();
            var scaler = root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            scaler.dynamicPixelsPerUnit = DynamicPixelsPerUnit;
            scaler.referencePixelsPerUnit = ReferencePixelsPerUnit;

            _root = root;
            WristMenu.CanvasObj = root;
        }

        public void Track(string id, Text label)
        {
            if (string.IsNullOrEmpty(id) || label == null)
            {
                return;
            }

            _byId[id] = label;
        }

        public Text Add(string id, string content, Vector2 size, Vector3 localPos, Color color, bool richText)
        {
            if (_root == null)
            {
                return null;
            }

            var go = new GameObject();
            go.transform.SetParent(_root.transform);

            var label = go.AddComponent<Text>();
            label.font = _font;
            label.text = content;
            label.fontSize = FontSize;
            label.supportRichText = richText;
            label.color = color;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = FontMinSize;
            label.resizeTextMaxSize = FontMaxSize;

            var rect = label.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = size;
            rect.localPosition = localPos;
            rect.rotation = Rotation;

            if (!string.IsNullOrEmpty(id))
            {
                _byId[id] = label;
            }

            return label;
        }

        public void SetText(string id, string content)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (_byId.TryGetValue(id, out Text label) && label != null)
            {
                label.text = content;
            }
        }

        public void SetColor(string id, Color color)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (_byId.TryGetValue(id, out Text label) && label != null)
            {
                label.color = color;
            }
        }

        public Transform FindLabel(string id)
        {
            if (!string.IsNullOrEmpty(id) && _byId.TryGetValue(id, out Text label) && label != null)
            {
                return label.transform;
            }

            return null;
        }

        public void Clear()
        {
            _byId.Clear();
            if (_root != null)
            {
                Object.Destroy(_root);
            }

            _root = null;
            WristMenu.CanvasObj = null;
            WristMenu.FpsText = null;
        }
    }
}