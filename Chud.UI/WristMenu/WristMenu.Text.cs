using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	internal const float TextPixelsPerUnit = 2000f;
	internal const float TextCapRatio = 0.7f;
	internal const float TextLineRatio = 1.16f;
	private static readonly int[] TextFontLadder = new int[] { 24, 34, 44, 54, 64, 74, 86, 100, 115, 132, 150, 175, 205, 230, 260 };

	internal static readonly Quaternion TextOrientation = Quaternion.Euler(180f, 90f, 90f);

	internal static readonly Dictionary<string, Text> TextLabels = new Dictionary<string, Text>();

	private static RectTransform _textRoot;

	private static Font _notificationFont;

	private static Font _measureFont;

	private static readonly Dictionary<int, Font> _fontsBySize = new Dictionary<int, Font>();

	private static readonly Dictionary<string, float> _textStretch = new Dictionary<string, float>();

	private static string _fontFamily;

	internal static void InitTextLayer()
	{
		DestroyTextLayer();
		GameObject root = new GameObject("chud_text", typeof(RectTransform));
		root.transform.SetParent(menu.transform, false);
		root.transform.localPosition = new Vector3(LabelX, 0f, 0f);
		root.transform.localRotation = TextOrientation;
		root.transform.localScale = Vector3.one;
		Canvas canvas = root.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		RectTransform rect = root.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		_textRoot = rect;
		canvasObj = root;
	}

	internal static void DestroyTextLayer()
	{
		TextLabels.Clear();
		_textStretch.Clear();
		if (_textRoot != null)
		{
			Object.Destroy(_textRoot.gameObject);
		}
		_textRoot = null;
		canvasObj = null;
		fpsText = null;
	}

	internal static void ClearMenuText()
	{
		DestroyTextLayer();
	}

	internal static void InitMenuFonts()
	{
		if (_notificationFont == null)
		{
			_notificationFont = GetFontForSize(30, false);
		}
		if (_measureFont == null)
		{
			_measureFont = GetFontForSize(104, false);
		}
	}

	internal static Font GetNotificationFont()
	{
		InitMenuFonts();
		return _notificationFont;
	}

	private static Font GetFontForSize(int size, bool snap)
	{
		int wanted = snap ? SnapTextSize(size) : Mathf.Clamp(size, 6, 200);
		Font font;
		if (_fontsBySize.TryGetValue(wanted, out font) && font != null)
		{
			return font;
		}
		font = CreateMenuFont(wanted);
		_fontsBySize[wanted] = font;
		return font;
	}

	private static int SnapTextSize(int size)
	{
		int snapped = TextFontLadder[0];
		for (int i = 0; i < TextFontLadder.Length; i++)
		{
			if (TextFontLadder[i] <= size)
			{
				snapped = TextFontLadder[i];
			}
		}
		return snapped;
	}

	private static Font CreateMenuFont(int size)
	{
		if (_fontFamily == null)
		{
			_fontFamily = ResolveFontFamily();
		}
		Font font = null;
		try
		{
			font = Font.CreateDynamicFontFromOSFont(_fontFamily, size);
		}
		catch
		{
			font = null;
		}
		if (font == null)
		{
			try
			{
				font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			}
			catch
			{
				font = null;
			}
		}
		if (font == null)
		{
			try
			{
				font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
			catch
			{
				font = null;
			}
		}
		return font;
	}

	private static string ResolveFontFamily()
	{
		string[] installed = null;
		try
		{
			installed = Font.GetOSInstalledFontNames();
		}
		catch
		{
			installed = null;
		}
		string[] preferred = new string[] { "Comic Sans MS", "Comic Sans", "Chalkboard", "Comic Neue", "Segoe Print", "Segoe UI", "Arial", "Verdana", "Tahoma" };
		if (installed != null)
		{
			for (int p = 0; p < preferred.Length; p++)
			{
				for (int i = 0; i < installed.Length; i++)
				{
					if (string.Equals(installed[i], preferred[p], System.StringComparison.OrdinalIgnoreCase))
					{
						return installed[i];
					}
				}
			}
			if (installed.Length > 0 && !string.IsNullOrEmpty(installed[0]))
			{
				return installed[0];
			}
		}
		return "Arial";
	}

	private static float MeasureTextEmWidth(string content)
	{
		InitMenuFonts();
		Font font = _measureFont;
		if (font == null || string.IsNullOrEmpty(content))
		{
			return 0f;
		}
		const int measureSize = 104;
		font.RequestCharactersInTexture(content, measureSize, FontStyle.Bold);
		float advance = 0f;
		CharacterInfo info;
		for (int i = 0; i < content.Length; i++)
		{
			if (font.GetCharacterInfo(content[i], out info, measureSize, FontStyle.Bold))
			{
				advance += info.advance;
			}
		}
		return advance / measureSize;
	}

	private static int FitTextSize(string content, float maxWidth, float maxHeight)
	{
		int limit = Mathf.FloorToInt(maxHeight * TextPixelsPerUnit / TextCapRatio);
		float emWidth = MeasureTextEmWidth(content);
		if (emWidth > 0.0001f && maxWidth > 0f)
		{
			limit = Mathf.Min(limit, Mathf.FloorToInt(maxWidth * TextPixelsPerUnit / emWidth));
		}
		return SnapTextSize(Mathf.Clamp(limit, TextFontLadder[0], TextFontLadder[TextFontLadder.Length - 1]));
	}

	internal static Vector3 TextLabelScale(float multiplier)
	{
		return Vector3.one * (multiplier / TextPixelsPerUnit);
	}

	internal static Vector3 TextLabelScale(string id, float multiplier)
	{
		float stretch = 1f;
		if (!string.IsNullOrEmpty(id) && _textStretch.TryGetValue(id, out float saved) && saved > 0.01f)
		{
			stretch = saved;
		}
		return new Vector3(stretch * multiplier / TextPixelsPerUnit, multiplier / TextPixelsPerUnit, multiplier / TextPixelsPerUnit);
	}

	private static float ComputeFillStretch(string content, int fontSize, float maxWidth)
	{
		float emWidth = MeasureTextEmWidth(content);
		if (emWidth <= 0.0001f || maxWidth <= 0f || fontSize <= 0)
		{
			return 1f;
		}
		float natural = emWidth * fontSize / TextPixelsPerUnit;
		if (natural <= 0.0001f)
		{
			return 1f;
		}
		return Mathf.Clamp(maxWidth / natural, 1f, ButtonTextMaxStretch);
	}

	private static Text CreateText(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color)
	{
		if (_textRoot == null || menu == null)
		{
			return null;
		}
		string value = content ?? string.Empty;
		int fontSize = FitTextSize(value, maxWidth, maxHeight);
		Font font = GetFontForSize(fontSize, true);
		if (font == null)
		{
			return null;
		}
		GameObject go = new GameObject("text_" + id, typeof(RectTransform));
		go.transform.SetParent(_textRoot, false);
		RectTransform rect = go.GetComponent<RectTransform>();
		Text label = go.AddComponent<Text>();
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
		rect.sizeDelta = new Vector2(maxWidth * TextPixelsPerUnit, maxHeight * TextPixelsPerUnit * (TextLineRatio / TextCapRatio));
		rect.localScale = TextLabelScale(1f);
		rect.localRotation = Quaternion.identity;
		rect.localPosition = new Vector3(-target.y, target.z, 0f);
		if (!string.IsNullOrEmpty(id))
		{
			TextLabels[id] = label;
		}
		return label;
	}

	private static Text MakeMenuText(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color)
	{
		_textStretch.Remove(id);
		return CreateText(id, content, target, maxWidth, maxHeight, color);
	}

	private static Text MakeMenuTextStretched(string id, string content, Vector3 target, float maxWidth, float maxHeight, Color color)
	{
		if (_textRoot == null || menu == null)
		{
			return null;
		}
		string value = content ?? string.Empty;
		int fontSize = FitTextSize(value, maxWidth, maxHeight);
		Font font = GetFontForSize(fontSize, true);
		if (font == null)
		{
			return null;
		}
		float stretch = ComputeFillStretch(value, fontSize, maxWidth);
		GameObject go = new GameObject("text_" + id, typeof(RectTransform));
		go.transform.SetParent(_textRoot, false);
		RectTransform rect = go.GetComponent<RectTransform>();
		Text label = go.AddComponent<Text>();
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
		rect.sizeDelta = new Vector2(maxWidth * TextPixelsPerUnit, maxHeight * TextPixelsPerUnit * (TextLineRatio / TextCapRatio));
		rect.localScale = new Vector3(stretch / TextPixelsPerUnit, 1f / TextPixelsPerUnit, 1f / TextPixelsPerUnit);
		rect.localRotation = Quaternion.identity;
		rect.localPosition = new Vector3(-target.y, target.z, 0f);
		if (!string.IsNullOrEmpty(id))
		{
			TextLabels[id] = label;
			_textStretch[id] = stretch;
		}
		return label;
	}

	internal static void SetMenuText(string id, string content)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		Text label;
		if (!TextLabels.TryGetValue(id, out label) || label == null)
		{
			return;
		}
		string next = content ?? string.Empty;
		if (label.text == next)
		{
			return;
		}
		float size = Mathf.Max(0.001f, label.rectTransform.sizeDelta.x / TextPixelsPerUnit);
		float height = Mathf.Max(0.001f, label.rectTransform.sizeDelta.y / TextPixelsPerUnit * (TextCapRatio / TextLineRatio));
		int newSize = FitTextSize(next, size, height);
		label.fontSize = newSize;
		label.text = next;
		if (_textStretch.ContainsKey(id))
		{
			float stretch = ComputeFillStretch(next, newSize, size);
			_textStretch[id] = stretch;
			label.rectTransform.localScale = new Vector3(stretch / TextPixelsPerUnit, 1f / TextPixelsPerUnit, 1f / TextPixelsPerUnit);
		}
		if (id == "status")
		{
			ApplyStatusLayout();
		}
	}

	internal static void ApplyStatusLayout()
	{
		Text status;
		bool hasStatus = TextLabels.TryGetValue("status", out status) && status != null && !string.IsNullOrEmpty(status.text);
		Text title;
		if (TextLabels.TryGetValue("title", out title) && title != null)
		{
			float z = hasStatus ? TitleZ - TitleStatusShift : TitleZ;
			title.rectTransform.localPosition = new Vector3(title.rectTransform.localPosition.x, z, 0f);
		}
	}

	internal static void SetMenuTextColor(string id, Color color)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		Text label;
		if (TextLabels.TryGetValue(id, out label) && label != null)
		{
			label.color = color;
		}
	}
}
