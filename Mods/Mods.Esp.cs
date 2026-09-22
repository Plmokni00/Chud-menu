using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Chud.Backend;

internal partial class Mods
{
	public const int TagStackConsole = 0;

	public const int TagStackCosmetics = 1;

	public const int TagStackId = 2;

	public const int TagStackPlatform = 3;

	public const int TagStackName = 4;

	public const int TagStackFps = 5;

	public const int TagStackArs = 6;

	public const int TagStackCrown = 7;

	private readonly Dictionary<VRRig, GameObject> boxEspObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> nameTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> fpsNameTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> idNameTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> platformNameTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> arsTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<VRRig, GameObject> cosmeticNameTagObjects = new Dictionary<VRRig, GameObject>();

	private readonly Dictionary<Player, LineRenderer> tracerLines = new Dictionary<Player, LineRenderer>();

	private readonly Dictionary<Player, LineRenderer[]> skeletonLines = new Dictionary<Player, LineRenderer[]>();

	private HashSet<string> arsPlayersToReport = new HashSet<string>();

	private bool arsActive = false;

	private bool arsDownloaded = false;

	private bool arsDownloading = false;

	private static readonly HttpClient arsHttpClient = new HttpClient();

	private FieldInfo _fpsField;

	private static readonly Dictionary<string, string> cosmeticNames = new Dictionary<string, string>
	{
		{ "LBAAK.", "Dev stick" },
		{ "LBANI.", "AA BADGE" },
		{ "LMAPY.", "Forest guide" },
		{ "LBADE.", "Finger painter" },
		{ "LBAGS.", "illustrator" },
		{ "LMAYQ.", "Golden gorilla ticket" },
		{ "LBARJ.", "COMMUNITY RIBBON" },
		{ "LBASS.", "PARTY ILLUSTRATOR BADGE" },
		{ "LMAJA.", "GT MONKE PLUSH" },
		{ "LMAYT.", "LAVA MONKE DOUGHBOI" },
		{ "LMBAO.", "Gorillacon golden phone" }
	};

	private FieldInfo _ownedCosmeticsField;

	private bool arsNameTagsActive = false;

	private string arsLastCheckedRoom = "";

	private bool cosmeticNotifierActive = false;

	private HashSet<string> cosmeticNotifierNotified = new HashSet<string>();

	private readonly List<VRRig> reusableBoxEspRemovals = new List<VRRig>();

	private readonly List<Player> reusableTracerRemovals = new List<Player>();

	private readonly List<Player> reusableSkeletonRemovals = new List<Player>();

	private readonly List<VRRig> reusableTagRemovals = new List<VRRig>();

	private readonly (Vector3, Vector3)[] reusableFingerConns = new (Vector3, Vector3)[6];

	private Camera cachedMainCamera;

	private Material skeletonSharedMaterial;

	private readonly Dictionary<VRRig, Transform[]> skeletonFingerCache = new Dictionary<VRRig, Transform[]>();

	private readonly List<VRRig> skeletonFingerStale = new List<VRRig>();

	private TagProvider NameTagProvider;

	private TagProvider FpsTagProvider;

	private TagProvider IdTagProvider;

	private TagProvider PlatformTagProvider;

	private TagProvider CosmeticTagProvider;

	private TagProvider ArsTagProvider;

	private void InitTagProviders()
	{
		NameTagProvider = new TagProvider("Chud_Nametag", TagStackName, GetNameTagText, TagColor, nameTagObjects);
		FpsTagProvider = new TagProvider("Chud_FPStag", TagStackFps, GetFpsTagText, TagColor, fpsNameTagObjects);
		IdTagProvider = new TagProvider("Chud_IDtag", TagStackId, GetIdTagText, TagColor, idNameTagObjects);
		PlatformTagProvider = new TagProvider("Chud_PlatformTag", TagStackPlatform, GetPlatformProperty, TagColor, platformNameTagObjects);
		CosmeticTagProvider = new TagProvider("Chud_CosmeticTag", TagStackCosmetics, GetCosmeticTagText, GetAlertTagColor, cosmeticNameTagObjects);
		ArsTagProvider = new TagProvider("Chud_ARStag", TagStackArs, GetArsTagText, GetAlertTagColor, arsTagObjects);
	}

	internal static float GetTagStackOffset(VRRig rig, int slot)
	{
		if (instance == null)
		{
			return 0.55f;
		}
		return instance.GetTagStackOffsetCore(rig, slot);
	}

	private float GetTagStackOffsetCore(VRRig rig, int slot)
	{
		int rank = 0;
		for (int s = 0; s < slot; s++)
		{
			if (IsTagActiveForRig(rig, s))
			{
				rank++;
			}
		}
		return 0.55f + (float)rank * 0.15f;
	}

	private bool IsTagActiveForRig(VRRig rig, int slot)
	{
		switch (slot)
		{
			case TagStackConsole:
				return Console.HasConsoleIndicator(rig);
			case TagStackCosmetics:
				return cosmeticNameTagObjects.ContainsKey(rig);
			case TagStackId:
				return idNameTagObjects.ContainsKey(rig);
			case TagStackPlatform:
				return platformNameTagObjects.ContainsKey(rig);
			case TagStackName:
				return nameTagObjects.ContainsKey(rig);
			case TagStackFps:
				return fpsNameTagObjects.ContainsKey(rig);
			case TagStackArs:
				return arsTagObjects.ContainsKey(rig);
			case TagStackCrown:
				return Console.conePool.ContainsKey(rig);
			default:
				return false;
		}
	}

	internal static Vector3 GetHeadAnchor(VRRig rig)
	{
		try
		{
			if (rig != null)
			{
				if (rig.headMesh != (Object)null)
					return rig.headMesh.transform.position;
				if (rig.head != null && rig.head.rigTarget != null)
					return rig.head.rigTarget.position;
			}
		}
		catch { }
		if (rig != null)
			return rig.transform.position + GetRigUp(rig) * (1.6f * EspScale(rig));
		return Vector3.zero;
	}

	internal static Vector3 GetRigUp(VRRig rig)
	{
		if (rig != null)
		{
			Vector3 up = rig.transform.up;
			if (up.sqrMagnitude > 0.001f)
			{
				return up.normalized;
			}
		}
		return Vector3.up;
	}

	public static Vector3 GetTagPosition(VRRig rig, int slot)
	{
		Vector3 anchor = GetHeadAnchor(rig);
		return anchor + GetRigUp(rig) * (GetTagStackOffset(rig, slot) * EspScale(rig));
	}

	internal static Camera MainCamera()
	{
		if (instance == null)
		{
			return Camera.main;
		}
		if (instance.cachedMainCamera == (Object)null)
		{
			instance.cachedMainCamera = Camera.main;
		}
		return instance.cachedMainCamera;
	}

	internal static void BillboardTag(GameObject obj)
	{
		Camera mainCam = MainCamera();
		if (!(mainCam == (Object)null))
		{
			Vector3 position = obj.transform.position;
			obj.transform.LookAt(2f * position - mainCam.transform.position);
		}
	}

	internal static Text CreateTagObj(string name, Dictionary<VRRig, GameObject> dict, VRRig rig)
	{
		if (comicSansFont == (Object)null)
		{
			comicSansFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 36);
		}
		GameObject val = new GameObject(name);
		Canvas val2 = val.AddComponent<Canvas>();
		val2.renderMode = RenderMode.WorldSpace;
		((Component)val2).transform.localScale = Vector3.one * (0.003f * EspScale(rig));
		Text val3 = val.AddComponent<Text>();
		if (comicSansFont != (Object)null)
		{
			val3.font = comicSansFont;
		}
		val3.fontSize = 30;
		val3.horizontalOverflow = HorizontalWrapMode.Overflow;
		val3.alignment = TextAnchor.MiddleCenter;
		((Graphic)val3).color = rig.playerColor;
		dict[rig] = val;
		return val3;
	}

	internal static void PlaceTag(GameObject obj, VRRig rig, int slot)
	{
		obj.transform.position = GetTagPosition(rig, slot);
		obj.transform.localScale = Vector3.one * (0.003f * EspScale(rig));
		BillboardTag(obj);
	}

	internal static float EspScale(VRRig rig)
	{
		float scale = 1f;
		try
		{
			if (rig != null)
				scale = rig.scaleFactor;
		}
		catch { scale = 1f; }
		if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
			scale = 1f;
		return scale;
	}

	public static void BoxEspRender()
	{
		if (instance == null)
		{
			return;
		}
		instance.BoxEspRenderCore();
	}

	private void BoxEspRenderCore()
	{
		reusableBoxEspRemovals.Clear();
		foreach (KeyValuePair<VRRig, GameObject> item in boxEspObjects)
		{
			if (VRRigCache.ActiveRigs.Contains(item.Key))
			{
				continue;
			}
			reusableBoxEspRemovals.Add(item.Key);
			Object.Destroy(item.Value);
		}
		foreach (VRRig item2 in reusableBoxEspRemovals)
		{
			boxEspObjects.Remove(item2);
		}
		foreach (VRRig item3 in VRRigCache.ActiveRigs)
		{
			if (item3.isLocal)
			{
				continue;
			}
			if (!boxEspObjects.TryGetValue(item3, out var value))
			{
				value = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy(value.GetComponent<BoxCollider>());
				value.GetComponent<Renderer>().enabled = false;
				value.transform.localScale = new Vector3(0.8f, 0.85f, 0f);
				Shader shader = CachedGuiTextShader;
				float num = 0.08f;
				GameObject val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy(val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, 0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy(val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, -0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy(val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy(val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(-0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				boxEspObjects.Add(item3, value);
			}
			Color color = item3.playerColor;
			try
			{
				GorillaGameManager val2 = GorillaGameManager.instance;
				if (val2 != (Object)null)
				{
					GorillaTagManager val3 = (GorillaTagManager)(object)((val2 is GorillaTagManager) ? val2 : null);
					if (val3 != null && item3.Creator != null && val3.IsInfected(item3.Creator))
					{
						color = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			float espScale = EspScale(item3);
			Vector3 espRoot = ((Component)item3).transform.position;
			Vector3 espHead = GetHeadAnchor(item3);
			float espHeight = Mathf.Abs(espHead.y - espRoot.y);
			if (espHeight < 0.4f * espScale || espHeight > 50f)
				espHeight = 0.85f * espScale;
			value.transform.position = (espRoot + espHead) * 0.5f;
			value.transform.LookAt(GorillaTagger.Instance.headCollider.transform.position);
			value.transform.localScale = new Vector3(0.8f * espScale, espHeight, 0f);
			foreach (Transform item4 in value.transform)
			{
				Transform val4 = item4;
				Renderer component = ((Component)val4).GetComponent<Renderer>();
				if (component != (Object)null)
				{
					component.material.color = color;
				}
			}
		}
	}

	public static void DisableBoxEsp()
	{
		if (instance == null)
		{
			return;
		}
		foreach (KeyValuePair<VRRig, GameObject> boxEspObject in instance.boxEspObjects)
		{
			Object.Destroy(boxEspObject.Value);
		}
		instance.boxEspObjects.Clear();
	}

	public static void Tracers()
	{
		if (instance == null)
		{
			return;
		}
		instance.TracersCore();
	}

	private void TracersCore()
	{
		reusableTracerRemovals.Clear();
		foreach (KeyValuePair<Player, LineRenderer> tracerLine in tracerLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(tracerLine.Key))
			{
				reusableTracerRemovals.Add(tracerLine.Key);
			}
		}
		if (reusableTracerRemovals.Count > 0)
		{
			foreach (Player item in reusableTracerRemovals)
			{
				Object.Destroy(((Component)tracerLines[item]).gameObject);
				tracerLines.Remove(item);
			}
		}
		Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
		foreach (Player val in playerListOthers)
		{
			VRRig vRRigFromPlayer = GorillaGameManager.StaticFindRigForPlayer(val);
			if (vRRigFromPlayer == (Object)null)
			{
				continue;
			}
			if (!tracerLines.TryGetValue(val, out var value))
			{
				GameObject val2 = new GameObject("TracerLine");
				((Object)val2).hideFlags = HideFlags.HideAndDontSave;
				value = val2.AddComponent<LineRenderer>();
				value.startWidth = 0.01f;
				value.endWidth = 0.01f;
				value.positionCount = 2;
				value.useWorldSpace = true;
				((Renderer)value).material.shader = CachedGuiTextShader;
				tracerLines[val] = value;
			}
			value.SetPosition(0, GetTracerStart());
			value.SetPosition(1, vRRigFromPlayer.transform.position);
			Color val3 = vRRigFromPlayer.playerColor;
			try
			{
				GorillaGameManager val4 = GorillaGameManager.instance;
				if (val4 != (Object)null)
				{
					GorillaTagManager val5 = (GorillaTagManager)(object)((val4 is GorillaTagManager) ? val4 : null);
					if (val5 != null && vRRigFromPlayer.Creator != null && val5.IsInfected(vRRigFromPlayer.Creator))
					{
						val3 = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			val3.a = 0.3f;
			value.startColor = val3;
			value.endColor = val3;
		}
	}

	private Vector3 GetTracerStart()
	{
		if (!XRSettings.isDeviceActive && (object)ghostRig != (Object)null && GhostWanted())
		{
			Transform ghostHand = null;
			try { ghostHand = ghostRig.rightHand?.rigTarget?.transform; } catch { }
			if ((object)ghostHand != (Object)null) return ghostHand.position;
			return ghostRig.transform.position + Vector3.up * 0.2f;
		}
		return GTPlayer.Instance.RightHand.controllerTransform.position;
	}

	public static void DisableTracers()
	{
		if (instance == null)
		{
			return;
		}
		foreach (LineRenderer value in instance.tracerLines.Values)
		{
			Object.Destroy(((Component)value).gameObject);
		}
		instance.tracerLines.Clear();
	}

	private struct SkeletonLink
	{
		public int first;
		public int second;
	}

	private static readonly SkeletonLink[] skeletonLinks = new SkeletonLink[]
	{
		new SkeletonLink { first = 4, second = 3 },
		new SkeletonLink { first = 5, second = 4 },
		new SkeletonLink { first = 19, second = 18 },
		new SkeletonLink { first = 20, second = 19 },
		new SkeletonLink { first = 3, second = 18 },
		new SkeletonLink { first = 21, second = 20 },
		new SkeletonLink { first = 22, second = 21 },
		new SkeletonLink { first = 25, second = 21 },
		new SkeletonLink { first = 29, second = 21 },
		new SkeletonLink { first = 31, second = 29 },
		new SkeletonLink { first = 27, second = 25 },
		new SkeletonLink { first = 24, second = 22 },
		new SkeletonLink { first = 6, second = 5 },
		new SkeletonLink { first = 7, second = 6 },
		new SkeletonLink { first = 10, second = 6 },
		new SkeletonLink { first = 14, second = 6 },
		new SkeletonLink { first = 16, second = 14 },
		new SkeletonLink { first = 12, second = 10 },
		new SkeletonLink { first = 9, second = 7 }
	};

	private const int SkeletonLineCount = 26;

	private const int SkeletonFingerStart = 20;

	private static readonly string[] skeletonFingerNames = new string[]
	{
		"thumb.03.L", "f_index.02.L", "f_middle.02.L",
		"thumb.03.R", "f_index.02.R", "f_middle.02.R"
	};

	private LineRenderer CreateSkeletonLine()
	{
		GameObject obj = new GameObject("skel");
		LineRenderer line = obj.AddComponent<LineRenderer>();
		line.startWidth = 0.025f;
		line.endWidth = 0.025f;
		line.positionCount = 2;
		line.useWorldSpace = true;
		if (skeletonSharedMaterial == null)
			skeletonSharedMaterial = new Material(CachedGuiTextShader);
		line.material = skeletonSharedMaterial;
		return line;
	}

	private void DrawSkeletonLine(LineRenderer line, Color color, Vector3 from, Vector3 to, float size)
	{
		if (line == null) return;
		line.startColor = color;
		line.endColor = color;
		line.startWidth = 0.025f * size;
		line.endWidth = 0.025f * size;
		line.SetPosition(0, from);
		line.SetPosition(1, to);
	}

	private Color SkeletonLineColor(VRRig rig)
	{
		Color color = rig.playerColor;
		try
		{
			GorillaGameManager gm = GorillaGameManager.instance;
			if (gm != null && gm is GorillaTagManager tgm && rig.Creator != null && tgm.IsInfected(rig.Creator))
				color = new Color(1f, 0.5f, 0f);
		}
		catch { }
		if (color.r == 0f && color.g == 0f && color.b == 0f)
			color = Color.white;
		return color;
	}

	private Transform[] GetSkeletonFingers(VRRig rig)
	{
		if (skeletonFingerCache.TryGetValue(rig, out Transform[] cached) && cached != null && cached.Length == 6)
			return cached;
		Transform[] found = new Transform[6];
		if (rig.mainSkin != null && rig.mainSkin.bones != null)
		{
			foreach (Transform b in rig.mainSkin.bones)
			{
				if (b == null) continue;
				for (int i = 0; i < 6; i++)
				{
					if (found[i] == null && b.name.StartsWith(skeletonFingerNames[i], StringComparison.OrdinalIgnoreCase))
						found[i] = b;
				}
			}
		}
		skeletonFingerCache[rig] = found;
		return found;
	}

	public static void SkeletonEsp()
	{
		if (instance == null)
		{
			return;
		}
		instance.SkeletonEspCore();
	}

	private void SkeletonEspCore()
	{
		reusableSkeletonRemovals.Clear();
		foreach (var kvp in skeletonLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(kvp.Key))
				reusableSkeletonRemovals.Add(kvp.Key);
		}
		foreach (Player p in reusableSkeletonRemovals)
		{
			if (skeletonLines.TryGetValue(p, out LineRenderer[] dead) && dead != null)
			{
				foreach (LineRenderer lr in dead)
				{
					if (lr != null)
						Object.Destroy(((Component)lr).gameObject);
				}
			}
			skeletonLines.Remove(p);
		}
		skeletonFingerStale.Clear();
		foreach (VRRig cachedRig in skeletonFingerCache.Keys)
		{
			if (cachedRig == null || !VRRigCache.ActiveRigs.Contains(cachedRig))
				skeletonFingerStale.Add(cachedRig);
		}
		foreach (VRRig stale in skeletonFingerStale)
			skeletonFingerCache.Remove(stale);

		foreach (Player player in PhotonNetwork.PlayerListOthers)
		{
			VRRig rig = GorillaGameManager.StaticFindRigForPlayer(player);
			if (rig == null) continue;
			if (rig.mainSkin == null || rig.mainSkin.bones == null) continue;
			if (rig.head == null || rig.head.rigTarget == null) continue;

			Transform[] bones = rig.mainSkin.bones;

			if (!skeletonLines.TryGetValue(player, out LineRenderer[] lines) || lines == null || lines.Length != SkeletonLineCount)
			{
				lines = new LineRenderer[SkeletonLineCount];
				for (int i = 0; i < SkeletonLineCount; i++)
					lines[i] = CreateSkeletonLine();
				skeletonLines[player] = lines;
			}

			Color color = SkeletonLineColor(rig);
			float s = EspScale(rig);
			Vector3 headPos = GetHeadAnchor(rig);
			DrawSkeletonLine(lines[0], color, headPos + new Vector3(0f, 0.16f * s, 0f), headPos - new Vector3(0f, 0.4f * s, 0f), s);

			for (int i = 0; i < skeletonLinks.Length; i++)
			{
				int a = skeletonLinks[i].first;
				int b = skeletonLinks[i].second;
				if (a < 0 || b < 0 || a >= bones.Length || b >= bones.Length) continue;
				Transform ta = bones[a];
				Transform tb = bones[b];
				if (ta == null || tb == null) continue;
				DrawSkeletonLine(lines[1 + i], color, ta.position, tb.position, s);
			}

			VRMap lm = rig.leftHand;
			Vector3 lHand = (lm != null && lm.rigTarget != null) ? lm.rigTarget.position : headPos;
			VRMap rm = rig.rightHand;
			Vector3 rHand = (rm != null && rm.rigTarget != null) ? rm.rigTarget.position : headPos;

			Vector3 forward = rig.head.rigTarget.forward;
			Vector3 right = rig.head.rigTarget.right;

			Transform[] fingers = GetSkeletonFingers(rig);
			Vector3 lThumb = fingers[0] != null ? fingers[0].position : lHand - right * (0.05f * s) + forward * (0.03f * s);
			Vector3 lIndex = fingers[1] != null ? fingers[1].position : lHand + forward * (0.06f * s);
			Vector3 lMiddle = fingers[2] != null ? fingers[2].position : lHand + forward * (0.06f * s) - right * (0.02f * s);
			Vector3 rThumb = fingers[3] != null ? fingers[3].position : rHand + right * (0.05f * s) + forward * (0.03f * s);
			Vector3 rIndex = fingers[4] != null ? fingers[4].position : rHand + forward * (0.06f * s);
			Vector3 rMiddle = fingers[5] != null ? fingers[5].position : rHand + forward * (0.06f * s) + right * (0.02f * s);

			reusableFingerConns[0] = (lHand, lThumb);
			reusableFingerConns[1] = (lHand, lIndex);
			reusableFingerConns[2] = (lHand, lMiddle);
			reusableFingerConns[3] = (rHand, rThumb);
			reusableFingerConns[4] = (rHand, rIndex);
			reusableFingerConns[5] = (rHand, rMiddle);

			for (int i = 0; i < 6; i++)
				DrawSkeletonLine(lines[SkeletonFingerStart + i], color, reusableFingerConns[i].Item1, reusableFingerConns[i].Item2, s);
		}
	}

	public static void DisableSkeletonEsp()
	{
		if (instance == null)
		{
			return;
		}
		foreach (LineRenderer[] arr in instance.skeletonLines.Values)
		{
			if (arr == null) continue;
			foreach (LineRenderer lr in arr)
			{
				if (lr != null)
					Object.Destroy(((Component)lr).gameObject);
			}
		}
		instance.skeletonLines.Clear();
		instance.skeletonFingerCache.Clear();
		if (instance.skeletonSharedMaterial != null)
		{
			Object.Destroy(instance.skeletonSharedMaterial);
			instance.skeletonSharedMaterial = null;
		}
	}

	private int GetFps(VRRig rig)
	{
		if (_fpsField == null)
		{
			_fpsField = AccessTools.Field(typeof(VRRig), "fps");
		}
		if (_fpsField == null)
		{
			return 0;
		}
		object value = _fpsField.GetValue(rig);
		return (value is int fps) ? fps : 0;
	}

	private Color TagColor(VRRig rig)
	{
		Color playerColor = rig.playerColor;
		if (playerColor.r == 0f && playerColor.g == 0f && playerColor.b == 0f)
		{
			return Color.white;
		}
		return playerColor;
	}

	private void CleanTagDict(Dictionary<VRRig, GameObject> dict)
	{
		reusableTagRemovals.Clear();
		foreach (KeyValuePair<VRRig, GameObject> item in dict)
		{
			if (!VRRigCache.ActiveRigs.Contains(item.Key))
			{
				reusableTagRemovals.Add(item.Key);
			}
		}
		if (reusableTagRemovals.Count == 0)
		{
			return;
		}
		foreach (VRRig item2 in reusableTagRemovals)
		{
			Object.Destroy(dict[item2]);
			dict.Remove(item2);
		}
	}

	private sealed class TagProvider
	{
		public readonly string ObjectName;

		public readonly int Slot;

		public readonly Func<VRRig, string> GetText;

		public readonly Func<VRRig, Color> GetColor;

		public readonly Dictionary<VRRig, GameObject> Objects;

		public TagProvider(string objectName, int slot, Func<VRRig, string> getText, Func<VRRig, Color> getColor, Dictionary<VRRig, GameObject> objects)
		{
			ObjectName = objectName;
			Slot = slot;
			GetText = getText;
			GetColor = getColor;
			Objects = objects;
		}
	}

	private string GetNameTagText(VRRig rig)
	{
		NetPlayer creator = rig.Creator;
		return ((creator != null) ? creator.NickName : null) ?? "?";
	}

	private string GetFpsTagText(VRRig rig)
	{
		return GetFps(rig) + " FPS";
	}

	private string GetIdTagText(VRRig rig)
	{
		NetPlayer creator = rig.Creator;
		return ((creator != null) ? creator.UserId : null) ?? "?";
	}

	private string GetCosmeticTagText(VRRig rig)
	{
		HashSet<string> owned = GetOwnedCosmetics(rig);
		if (owned == null || owned.Count == 0)
		{
			return null;
		}
		List<string> names = new List<string>(owned.Count);
		foreach (string item in owned)
		{
			if (cosmeticNames.TryGetValue(item, out var display))
			{
				names.Add(display);
			}
		}
		if (names.Count == 0)
		{
			return null;
		}
		return string.Join(", ", names);
	}

	private string GetArsTagText(VRRig rig)
	{
		NetPlayer creator = rig.Creator;
		string id = (creator != null) ? creator.UserId : null;
		if (id == null || !arsPlayersToReport.Contains(id))
		{
			return null;
		}
		return "ARS";
	}

	private Color GetAlertTagColor(VRRig rig)
	{
		return Color.red;
	}

	private void UpdateLiveTag(TagProvider provider)
	{
		CleanTagDict(provider.Objects);
		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (rig.isLocal)
			{
				continue;
			}
			string text = provider.GetText(rig);
			if (string.IsNullOrEmpty(text))
			{
				if (provider.Objects.TryGetValue(rig, out var stale))
				{
					Object.Destroy(stale);
					provider.Objects.Remove(rig);
				}
				continue;
			}
			GameObject go;
			if (!provider.Objects.TryGetValue(rig, out go))
			{
				Text created = CreateTagObj(provider.ObjectName, provider.Objects, rig);
				go = ((Component)created).gameObject;
			}
			Text label = go.GetComponent<Text>();
			if (label != (Object)null)
			{
				label.text = text;
				((Graphic)label).color = provider.GetColor(rig);
			}
			PlaceTag(go, rig, provider.Slot);
		}
	}

	private void UpdateStickyTag(TagProvider provider)
	{
		CleanTagDict(provider.Objects);
		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (rig.isLocal || provider.Objects.ContainsKey(rig))
			{
				continue;
			}
			string text = provider.GetText(rig);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			Text created = CreateTagObj(provider.ObjectName, provider.Objects, rig);
			created.text = text;
			((Graphic)created).color = provider.GetColor(rig);
		}
		foreach (KeyValuePair<VRRig, GameObject> entry in provider.Objects)
		{
			PlaceTag(entry.Value, entry.Key, provider.Slot);
		}
	}

	private void DisableTag(TagProvider provider)
	{
		foreach (GameObject go in provider.Objects.Values)
		{
			Object.Destroy(go);
		}
		provider.Objects.Clear();
	}

	public static void NameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateLiveTag(instance.NameTagProvider);
	}

	public static void DisableNameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.DisableTag(instance.NameTagProvider);
	}

	public static void FPSTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateLiveTag(instance.FpsTagProvider);
	}

	public static void DisableFPSTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.DisableTag(instance.FpsTagProvider);
	}

	public static void IDTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateLiveTag(instance.IdTagProvider);
	}

	public static void DisableIDTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.DisableTag(instance.IdTagProvider);
	}

	public static void PlatformTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateLiveTag(instance.PlatformTagProvider);
	}

	private string GetPlatformProperty(VRRig rig)
	{
		NetPlayer creator = rig.Creator;
		if (creator == null || creator.UserId == null)
		{
			return null;
		}
		Player photonPlayer = null;
		if (PhotonNetwork.InRoom)
		{
			foreach (Player player in PhotonNetwork.PlayerList)
			{
				if (player.UserId == creator.UserId)
				{
					photonPlayer = player;
					break;
				}
			}
		}
		ExitGames.Client.Photon.Hashtable customProperties = (photonPlayer != null) ? photonPlayer.CustomProperties : null;
		if (customProperties == null || customProperties.Count == 0)
		{
			return null;
		}
		object platformValue;
		if (customProperties.TryGetValue("platform", out platformValue) && platformValue != null)
		{
			return "Platform: " + platformValue;
		}
		return null;
	}

	public static void DisablePlatformTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.DisableTag(instance.PlatformTagProvider);
	}

	private HashSet<string> GetOwnedCosmetics(VRRig rig)
	{
		if (_ownedCosmeticsField == null)
		{
			_ownedCosmeticsField = AccessTools.Field(typeof(VRRig), "_playerOwnedCosmetics");
		}
		return _ownedCosmeticsField?.GetValue(rig) as HashSet<string>;
	}

	public static void CosmeticNameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateStickyTag(instance.CosmeticTagProvider);
	}

	public static void DisableCosmeticNameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.DisableTag(instance.CosmeticTagProvider);
	}

	public static void EnableARS()
	{
		if (instance == null)
		{
			return;
		}
		instance.arsActive = true;
		if (!instance.arsDownloaded && !instance.arsDownloading)
		{
			instance.arsDownloading = true;
			_ = instance.AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARS()
	{
		if (instance == null)
		{
			return;
		}
		instance.arsActive = false;
		if (instance.arsNameTagsActive)
		{
			return;
		}
		foreach (GameObject value in instance.arsTagObjects.Values)
		{
			Object.Destroy(value);
		}
		instance.arsTagObjects.Clear();
	}

	public static void EnableARSNameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.arsNameTagsActive = true;
		if (!instance.arsDownloaded && !instance.arsDownloading)
		{
			instance.arsDownloading = true;
			_ = instance.AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARSNameTags()
	{
		if (instance == null)
		{
			return;
		}
		instance.arsNameTagsActive = false;
		if (instance.arsActive)
		{
			return;
		}
		foreach (GameObject value in instance.arsTagObjects.Values)
		{
			Object.Destroy(value);
		}
		instance.arsTagObjects.Clear();
	}

	public static void ARSNameTagUpdate()
	{
		if (instance == null)
		{
			return;
		}
		if (!instance.arsNameTagsActive || instance.arsPlayersToReport.Count == 0)
		{
			return;
		}
		instance.UpdateStickyTag(instance.ArsTagProvider);
	}

	private void ARSDetect()
	{
		if (arsActive && arsPlayersToReport.Count != 0 && PhotonNetwork.InRoom)
		{
			string name = PhotonNetwork.CurrentRoom.Name;
			if (name != arsLastCheckedRoom)
			{
				arsLastCheckedRoom = name;
				StartCoroutine(ARSDelayedCheck(name));
			}
		}
	}

	private static IEnumerator ARSDelayedCheck(string expectedRoom)
	{
		yield return (object)new WaitForSeconds(Random.Range(2.5f, 10f));
		if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name != expectedRoom)
		{
			yield break;
		}
		ARSCheckAllPlayers();
	}

	private static void ARSCheckAllPlayers()
	{
		if (PhotonNetwork.InRoom)
		{
			Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
			foreach (Player photonPlayer in playerListOthers)
			{
				ARSCheckPlayer(photonPlayer);
			}
		}
	}

	public static void ARSCheckPlayer(Player photonPlayer)
	{
		if (instance == null || !instance.arsActive || instance.arsPlayersToReport.Count == 0)
		{
			return;
		}
		string userId = photonPlayer.UserId;
		if (userId == null || !instance.arsPlayersToReport.Contains(userId))
		{
			return;
		}
		string text = photonPlayer.NickName ?? userId;
		NotifiLib.SendNotification(text + " is on ARS", 3);
		foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (allScoreboardLine.linePlayer == NetworkSystem.Instance.GetNetPlayerByID(photonPlayer.ActorNumber))
			{
				allScoreboardLine.PressButton(true, GorillaPlayerLineButton.ButtonType.Toxicity);
				break;
			}
		}
	}

	private async Task AsyncGetARSPlayerIDs()
	{
		try
		{
			string raw = (await arsHttpClient.GetStringAsync("https://raw.githubusercontent.com/AutoReportSystem/ARSPlayerIDs/refs/heads/main/Player%20Ids.txt")).Trim();
			HashSet<string> ids = (arsPlayersToReport = (from id in raw.Split(',')
				select id.Trim() into id
				where !StringUtils.IsNullOrEmpty(id)
				select id).ToHashSet());
			arsDownloaded = true;
			System.Console.WriteLine("[ARS] Loaded " + ids.Count + " player IDs to detect");
		}
		catch (Exception ex)
		{
			Exception e = ex;
			System.Console.WriteLine("[ARS] Failed to download player IDs: " + e.Message);
			arsDownloaded = false;
		}
		arsDownloading = false;
	}

	public static void CosmeticNotifier()
	{
		if (instance != null)
		{
			instance.cosmeticNotifierActive = true;
		}
	}

	public static void DisableCosmeticNotifier()
	{
		if (instance == null)
		{
			return;
		}
		instance.cosmeticNotifierActive = false;
		instance.cosmeticNotifierNotified.Clear();
	}

	private void UpdateCosmeticNotifier()
	{
		if (!cosmeticNotifierActive)
		{
			return;
		}
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal || activeRig.Creator == null)
			{
				continue;
			}
			HashSet<string> ownedCosmetics = GetOwnedCosmetics(activeRig);
			if (ownedCosmetics == null || ownedCosmetics.Count == 0)
			{
				continue;
			}
			string userId = activeRig.Creator.UserId;
			if (cosmeticNotifierNotified.Contains(userId))
			{
				continue;
			}
			List<string> list = new List<string>(ownedCosmetics.Count);
			foreach (string item in ownedCosmetics)
			{
				if (cosmeticNames.TryGetValue(item, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count != 0)
			{
				cosmeticNotifierNotified.Add(userId);
				NotifiLib.SendNotification(activeRig.Creator.NickName + ": " + string.Join(", ", list), 5);
			}
		}
	}
}
