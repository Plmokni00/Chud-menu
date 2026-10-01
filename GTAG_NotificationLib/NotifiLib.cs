using Chud.UI;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine;
namespace GTAG_NotificationLib;

public class NotifiLib : MonoBehaviour
{
	private GameObject hudObj;

	private GameObject hudObjParent;

	private GameObject mainCamera;

	private static Text notificationText;

	private Material textMaterial;

	private bool hasInit = false;

	private static bool _isEnabled = true;

	public static bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled == value)
			{
				return;
			}
			_isEnabled = value;
			if (!_isEnabled)
			{
				ClearAllNotifications();
			}
		}
	}

	public static int DecayTime = 150;

	private static float lastNotifyTime;

	private struct VrLine
	{
		public string text;

		public float expireTime;
	}

	private static readonly List<VrLine> _vrLines = new List<VrLine>();

	private const int MAX_VR_LINES = 4;

	private static bool? _lastSeenDesktopMode;

	private static readonly List<DesktopNoti> _desktopNotis = new List<DesktopNoti>();

	private const int MAX_DESKTOP_NOTIS = 4;

	private static GUIStyle _notiStyle;

	private static Texture2D _notiBgTex;

	private struct DesktopNoti
	{
		public string text;

		public float expireTime;
	}

	public static bool IsDesktopMode()
	{
		return !XRSettings.isDeviceActive;
	}

	public static bool IsVrMode()
	{
		return XRSettings.isDeviceActive;
	}

	private static Texture2D GetNotiBgTex()
	{
		if (_notiBgTex == (Object)null)
		{
			_notiBgTex = new Texture2D(1, 1);
			_notiBgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
			_notiBgTex.Apply();
		}
		return _notiBgTex;
	}

	private void OnGUI()
	{
		if (!_isEnabled || !IsDesktopMode() || _desktopNotis.Count == 0)
		{
			return;
		}
		if (_notiStyle == null)
		{
			_notiStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 20,
				richText = true,
				wordWrap = true,
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(8, 8, 0, 0)
			};
			if (WristMenu.MenuFont != (Object)null)
			{
				_notiStyle.font = WristMenu.MenuFont;
			}
		}
		float y = 10f;
		float height = 36f;
		for (int i = 0; i < _desktopNotis.Count; i++)
		{
			DesktopNoti noti = _desktopNotis[i];
			float alpha = Mathf.Clamp01((noti.expireTime - Time.time) / 0.5f);
			if (alpha <= 0f) continue;
			string stripped = Regex.Replace(noti.text, "<[^>]*>", "");
			float width = _notiStyle.CalcSize(new GUIContent(stripped)).x + 24f;
			Color prevColor = GUI.color;
			GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, alpha);
			GUI.DrawTexture(new Rect(10f, y, width, height), GetNotiBgTex());
			GUI.Label(new Rect(10f, y, width, height), noti.text, _notiStyle);
			GUI.color = prevColor;
			y += height + 6f;
		}
	}

	private static void CleanDesktopNotis()
	{
		for (int i = _desktopNotis.Count - 1; i >= 0; i--)
		{
			if (Time.time >= _desktopNotis[i].expireTime)
			{
				_desktopNotis.RemoveAt(i);
			}
		}
	}

	private static void AddDesktopNoti(string plainText, int decayMultiplier)
	{
		CleanDesktopNotis();
		for (int i = 0; i < _desktopNotis.Count; i++)
		{
			if (_desktopNotis[i].text == plainText && Time.time < _desktopNotis[i].expireTime - 0.5f) return;
		}
		if (_desktopNotis.Count >= MAX_DESKTOP_NOTIS)
		{
			_desktopNotis.RemoveAt(0);
		}
		float duration = (DecayTime * decayMultiplier) * 0.02f;
		_desktopNotis.Add(new DesktopNoti
		{
			text = plainText,
			expireTime = Time.time + duration
		});
	}

	private void Init()
	{
		mainCamera = GameObject.Find("Main Camera");
		if (mainCamera == (Object)null && Camera.main != (Object)null)
		{
			mainCamera = Camera.main.gameObject;
		}
		if (mainCamera != (Object)null)
		{
			hudObj = new GameObject("NOTIFICATIONLIB_HUD_OBJ");
			hudObjParent = new GameObject("NOTIFICATIONLIB_HUD_OBJ2");
			Canvas canvas = hudObj.AddComponent<Canvas>();
			hudObj.AddComponent<CanvasScaler>();
			hudObj.AddComponent<GraphicRaycaster>();
			canvas.enabled = true;
			canvas.renderMode = RenderMode.WorldSpace;
			Camera cam = mainCamera.GetComponent<Camera>();
			canvas.worldCamera = cam;
			hudObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
			hudObj.GetComponent<RectTransform>().position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
			hudObjParent.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z - 4.6f);
			hudObj.transform.SetParent(hudObjParent.transform);
			hudObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
			Quaternion rotation = hudObj.GetComponent<RectTransform>().rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			eulerAngles.y = -270f;
			hudObj.transform.localScale = Vector3.one;
			hudObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(eulerAngles);
			GameObject textObject = new GameObject();
			textObject.transform.parent = hudObj.transform;
			notificationText = textObject.AddComponent<Text>();
			notificationText.text = "";
			notificationText.fontSize = 30;
			notificationText.font = WristMenu.MenuFont;
			notificationText.rectTransform.sizeDelta = new Vector2(800f, 200f);
			notificationText.alignment = (TextAnchor)6;
			notificationText.rectTransform.localScale = new Vector3(0.002f, 0.002f, 1f);
			notificationText.rectTransform.localPosition = new Vector3(-0.3f, -0.35f, -0.15f);
if (textMaterial == (Object)null)
		{
			Shader shader = Shader.Find("GUI/Text Shader");
			if (shader != (Object)null)
			{
				textMaterial = new Material(shader);
			}
		}
		if (textMaterial != (Object)null)
		{
			notificationText.material = textMaterial;
		}
			hudObjParent.SetActive(IsVrMode() && _isEnabled);
		}
	}

	private void EnsureInit()
	{
		if (mainCamera == (Object)null)
		{
			GameObject found = GameObject.Find("Main Camera");
			if (found == (Object)null && Camera.main != (Object)null)
			{
				found = Camera.main.gameObject;
			}
			mainCamera = found;
		}
		if (!hasInit && (Object)(object)mainCamera != (Object)null)
		{
			Init();
			hasInit = (Object)(object)hudObjParent != (Object)null;
		}
		if (hasInit && (hudObjParent == (Object)null || notificationText == (Object)null))
		{
			hasInit = false;
		}
	}

private static void HandleModeSwitch()
    {
        SyncModeSwitch();
    }

    private static void SyncModeSwitch()
    {
        bool desktop = IsDesktopMode();
        if (_lastSeenDesktopMode.HasValue && _lastSeenDesktopMode.Value != desktop)
        {
            if (desktop)
            {
                ClearVrLines();
            }
            else
            {
                _desktopNotis.Clear();
            }
        }
        _lastSeenDesktopMode = desktop;
    }

	private void FollowCamera()
	{
		if (mainCamera == (Object)null || hudObjParent == (Object)null)
		{
			return;
		}
		if (!IsVrMode())
		{
			return;
		}
		hudObjParent.transform.position = mainCamera.transform.position;
		hudObjParent.transform.rotation = mainCamera.transform.rotation;
	}

	private void RefreshVrFont()
	{
		if (notificationText != (Object)null && notificationText.font == (Object)null && WristMenu.MenuFont != (Object)null)
		{
			notificationText.font = WristMenu.MenuFont;
		}
	}

	private void UpdateVrVisibility()
	{
		if (hudObjParent == (Object)null)
		{
			return;
		}
		bool visible = IsVrMode() && _isEnabled;
		if (hudObjParent.activeSelf != visible)
		{
			hudObjParent.SetActive(visible);
		}
	}

	private void TickVrExpiry()
	{
		if (notificationText == (Object)null)
		{
			return;
		}
		if (_vrLines.Count == 0)
		{
			if (notificationText.text != "")
			{
				notificationText.text = "";
			}
			return;
		}
		bool removed = false;
		for (int i = _vrLines.Count - 1; i >= 0; i--)
		{
			if (Time.time >= _vrLines[i].expireTime)
			{
				_vrLines.RemoveAt(i);
				removed = true;
			}
		}
		if (removed)
		{
			RebuildVrText();
			return;
		}
		if (notificationText.text == "")
		{
			RebuildVrText();
		}
	}

	private static void CleanExpiredVrLines()
	{
		for (int i = _vrLines.Count - 1; i >= 0; i--)
		{
			if (Time.time >= _vrLines[i].expireTime)
			{
				_vrLines.RemoveAt(i);
			}
		}
	}

	private static void RebuildVrText()
	{
		if (notificationText == (Object)null)
		{
			return;
		}
		if (_vrLines.Count == 0)
		{
			notificationText.text = "";
			return;
		}
		string[] parts = new string[_vrLines.Count];
		for (int i = 0; i < _vrLines.Count; i++)
		{
			parts[i] = _vrLines[i].text;
		}
		notificationText.text = string.Join("\n", parts) + "\n";
	}

	private static void ClearVrLines()
	{
		_vrLines.Clear();
		if (notificationText != (Object)null)
		{
			notificationText.text = "";
		}
	}

	private static void AddVrNoti(string richText, int decayMultiplier)
	{
		CleanExpiredVrLines();
		for (int i = 0; i < _vrLines.Count; i++)
		{
			if (_vrLines[i].text == richText && Time.time < _vrLines[i].expireTime - 0.5f)
			{
				return;
			}
		}
		while (_vrLines.Count >= MAX_VR_LINES)
		{
			_vrLines.RemoveAt(0);
		}
		float duration = (DecayTime * decayMultiplier) * 0.02f;
		if (duration < 0.5f)
		{
			duration = 0.5f;
		}
		_vrLines.Add(new VrLine
		{
			text = richText,
			expireTime = Time.time + duration
		});
		RebuildVrText();
	}

	private void OnDestroy()
	{
		if (textMaterial != (Object)null)
		{
			Destroy(textMaterial);
			textMaterial = null;
		}
		if (hudObjParent != (Object)null)
		{
			Destroy(hudObjParent);
			hudObjParent = null;
		}
		if (hudObj != (Object)null)
		{
			Destroy(hudObj);
			hudObj = null;
		}
		if (_notiBgTex != (Object)null)
		{
			Destroy(_notiBgTex);
			_notiBgTex = null;
		}
		notificationText = null;
	}

	private void LateUpdate()
	{
		EnsureInit();
		HandleModeSwitch();
		RefreshVrFont();
		FollowCamera();
		TickVrExpiry();
		UpdateVrVisibility();
	}

	public static void SendNotification(string text, int decayMultiplier = 1)
	{
		if (!(lastNotifyTime < Time.time))
		{
			return;
		}
		lastNotifyTime = Time.time + 0.2f;
		if (!_isEnabled)
		{
			return;
		}
		if (text.StartsWith("<color=") || text.StartsWith("["))
		{
			int num = text.IndexOf("] ");
			if (num >= 0 && num < 20) text = text.Substring(num + 2);
		}
		string plainText = Regex.Replace(text, "<[^>]*>", "").Trim();
		if (plainText.Length > 120) plainText = plainText.Substring(0, 120);
		if (plainText.Length == 0)
		{
			return;
		}
		bool desktop = IsDesktopMode();
		SyncModeSwitch();
		if (desktop)
		{
			string desktopRich = "<color=green>[Noti]</color> - " + plainText;
			AddDesktopNoti(desktopRich, decayMultiplier);
			return;
		}
		string vrPlain = plainText;
		if (vrPlain.Length > 50 && !vrPlain.Contains("\n"))
		{
			int mid = vrPlain.Length / 2;
			int split = vrPlain.LastIndexOf(' ', mid);
			if (split < 18) split = vrPlain.IndexOf(' ', mid);
			if (split > 18 && split < vrPlain.Length - 8) { vrPlain = vrPlain.Substring(0, split) + "\n" + vrPlain.Substring(split + 1); }
		}
		string vrRich = "<color=green>[Noti]</color> - " + vrPlain;
		AddVrNoti(vrRich, decayMultiplier);
	}

	public static void ClearAllNotifications()
	{
		ClearVrLines();
		_desktopNotis.Clear();
	}

}