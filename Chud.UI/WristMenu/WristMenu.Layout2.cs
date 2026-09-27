using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal static class MenuLayout2
{
	private const float L2MenuRadius = 0.1f;
	private const float L2MenuHeight = 0.3f;
	private const float L2MenuDepth = 0.4f;
	private const float L2MenuInitialDepthFactor = 0.95625f;
	private const float L2MenuScaleFactor = 0.8f;
	private const float L2ButtonScaleX = 0.09f;
	private const float L2ButtonScaleY = 0.9f;
	private const float L2ButtonScaleZ = 0.08f;
	private const float L2NavScaleX = 0.09f;
	private const float L2NavScaleY = 0.2f;
	private const float L2NavScaleZ = 0.9f;
	private const float L2PanelPosX = 0.05f;
	private const float L2SurfaceX = 0.56f;
	private const float L2DisconnectZ = 0.6f;
	private const float L2NavY = 0.65f;
	private const float L2FirstButtonZ = 0.28f;
	private const float L2TitlePosX = 0.06f;
	private const float L2TitlePosZ = 0.175f;
	private const float L2TitleWidth = 0.28f;
	private const float L2TitleHeight = 0.05f;
	private const float L2StatusPosX = 0.06f;
	private const float L2StatusPosZ = 0.135f;
	private const float L2StatusWidth = 0.28f;
	private const float L2StatusHeight = 0.02f;
	private const float L2LabelPosX = 0.064f;
	private const float L2LabelWidth = 0.2f;
	private const float L2LabelHeight = 0.03f;
	private const float L2LabelBaseZ = 0.111f;
	private const float L2LabelFirstZ = 0.28f;
	private const float L2LabelCompress = 2.6f;
	private const float L2NavLabelY = 0.195f;
	private const int L2FontSize = 200;
	private const int L2FontMinSize = 0;
	private const int L2FontMaxSize = 200;
	private const float L2CanvasDynamicPixelsPerUnit = 1900f;
	private const float L2CanvasReferencePixelsPerUnit = 100f;

	private static Mesh _cylinderMesh2;
	private static Mesh _sphereMesh2;
	private static Font _font2;
	private static Shader _unlitTexShader2;
	private static Shader _unlitColorShader2;
	private static readonly Dictionary<string, Material> _gradientCache2 = new Dictionary<string, Material>();
	private static readonly Dictionary<string, Mesh> _roundedMeshCache2 = new Dictionary<string, Mesh>();
	private static readonly List<Material> _gradientMaterials2 = new List<Material>();
	private static readonly Dictionary<string, List<Renderer>> _roundedRenderers2 = new Dictionary<string, List<Renderer>>();

	private static Mesh CylinderMesh2
	{
		get
		{
			if (_cylinderMesh2 == null)
			{
				GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				_cylinderMesh2 = temp.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(temp);
			}
			return _cylinderMesh2;
		}
	}

	private static Mesh SphereMesh2
	{
		get
		{
			if (_sphereMesh2 == null)
			{
				GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				_sphereMesh2 = temp.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(temp);
			}
			return _sphereMesh2;
		}
	}

	private static Shader UnlitTexShader2
	{
		get
		{
			if (_unlitTexShader2 == null)
			{
				_unlitTexShader2 = Shader.Find("Unlit/Texture");
			}
			return _unlitTexShader2;
		}
	}

	private static Shader UnlitColorShader2
	{
		get
		{
			if (_unlitColorShader2 == null)
			{
				_unlitColorShader2 = Shader.Find("Unlit/Color");
			}
			return _unlitColorShader2;
		}
	}

	internal static void InitFont2()
	{
		if (_font2 != null)
		{
			return;
		}
		try
		{
			_font2 = Font.CreateDynamicFontFromOSFont("Comic Sans MS", L2FontSize);
		}
		catch
		{
			_font2 = null;
		}
		if (_font2 == null)
		{
			_font2 = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}
	}

	private static string GradientKey2(Color a, Color b)
	{
		return a.r.ToString("F3") + "," + a.g.ToString("F3") + "," + a.b.ToString("F3") + "," + a.a.ToString("F3") + "|" + b.r.ToString("F3") + "," + b.g.ToString("F3") + "," + b.b.ToString("F3") + "," + b.a.ToString("F3");
	}

	private static Material MakeGradientMat2(Color top, Color bot)
	{
		string k = GradientKey2(top, bot);
		Material cm;
		if (_gradientCache2.TryGetValue(k, out cm) && cm != null)
		{
			if (!_gradientMaterials2.Contains(cm))
			{
				_gradientMaterials2.Add(cm);
			}
			return cm;
		}
		int h = 16;
		Texture2D tex = new Texture2D(2, h, TextureFormat.RGBA32, false);
		tex.hideFlags = HideFlags.HideAndDontSave;
		Color highlight = Color.Lerp(bot, Color.white, 0.075f);
		for (int y = 0; y < h; y++)
		{
			float t = 1f - Mathf.Abs((float)y / (h - 1) * 2f - 1f);
			tex.SetPixel(0, y, Color.Lerp(bot, highlight, t));
			tex.SetPixel(1, y, Color.Lerp(bot, highlight, t));
		}
		tex.Apply();
		tex.wrapMode = TextureWrapMode.Repeat;
		Shader s = UnlitTexShader2;
		if (s == null)
		{
			s = Shader.Find("Universal Render Pipeline/Unlit");
		}
		Material mat = new Material(s);
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.mainTexture = tex;
		mat.color = Color.white;
		mat.mainTextureScale = new Vector2(1, 0.5f);
		_gradientMaterials2.Add(mat);
		_gradientCache2[k] = mat;
		return mat;
	}

	private static Material MakePlainMat2(Color c)
	{
		Shader s = UnlitColorShader2;
		if (s == null)
		{
			s = Shader.Find("Universal Render Pipeline/Unlit");
		}
		if (s == null)
		{
			s = Shader.Find("GUI/Text Shader");
		}
		Material mat = new Material(s);
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.color = c;
		return mat;
	}

	private static Mesh GenerateRoundedRectMesh2(float width, float height, float radius, int cornerSegments, float depth)
	{
		string key = width + ":" + height + ":" + radius + ":" + cornerSegments + ":" + depth;
		Mesh cached;
		if (_roundedMeshCache2.TryGetValue(key, out cached) && cached != null)
		{
			return cached;
		}
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.HideAndDontSave;
		float hw = width * 0.5f;
		float hh = height * 0.5f;
		float hd = depth * 0.5f;
		radius = Mathf.Min(radius, Mathf.Min(hw, hh));
		List<Vector2> outline = new List<Vector2>();
		Vector2[] centers = new Vector2[]
		{
			new Vector2(hw - radius, hh - radius),
			new Vector2(hw - radius, -(hh - radius)),
			new Vector2(-(hw - radius), -(hh - radius)),
			new Vector2(-(hw - radius), hh - radius)
		};
		float[] starts = new float[] { 90f, 0f, -90f, -180f };
		float[] ends = new float[] { 0f, -90f, -180f, -270f };
		for (int c = 0; c < 4; c++)
		{
			for (int i = 0; i <= cornerSegments; i++)
			{
				float angle = Mathf.Lerp(starts[c], ends[c], (float)i / cornerSegments) * Mathf.Deg2Rad;
				float y = centers[c].y + Mathf.Sin(angle) * radius;
				float z = centers[c].x + Mathf.Cos(angle) * radius;
				outline.Add(new Vector2(y, z));
			}
		}
		int outlineCount = outline.Count;
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();
		int frontCenter = verts.Count;
		verts.Add(new Vector3(hd, 0, 0));
		uvs.Add(new Vector2(0.5f, 0.5f));
		int frontStart = verts.Count;
		for (int i = 0; i < outlineCount; i++)
		{
			verts.Add(new Vector3(hd, outline[i].x, outline[i].y));
			uvs.Add(new Vector2((outline[i].y + hw) / width, (outline[i].x + hh) / height));
		}
		for (int i = 0; i < outlineCount; i++)
		{
			int next = (i + 1) % outlineCount;
			tris.Add(frontCenter);
			tris.Add(frontStart + i);
			tris.Add(frontStart + next);
		}
		int backCenter = verts.Count;
		verts.Add(new Vector3(-hd, 0, 0));
		uvs.Add(new Vector2(0.5f, 0.5f));
		int backStart = verts.Count;
		for (int i = 0; i < outlineCount; i++)
		{
			verts.Add(new Vector3(-hd, outline[i].x, outline[i].y));
			uvs.Add(new Vector2((outline[i].y + hw) / width, (outline[i].x + hh) / height));
		}
		for (int i = 0; i < outlineCount; i++)
		{
			int next = (i + 1) % outlineCount;
			tris.Add(backCenter);
			tris.Add(backStart + next);
			tris.Add(backStart + i);
		}
		for (int i = 0; i < outlineCount; i++)
		{
			int next = (i + 1) % outlineCount;
			int f0 = frontStart + i;
			int f1 = frontStart + next;
			int b0 = backStart + i;
			int b1 = backStart + next;
			tris.Add(f0);
			tris.Add(b0);
			tris.Add(f1);
			tris.Add(f1);
			tris.Add(b0);
			tris.Add(b1);
		}
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(tris, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		_roundedMeshCache2[key] = mesh;
		return mesh;
	}

	private static GameObject MakeCylinder2()
	{
		GameObject go = new GameObject();
		go.AddComponent<MeshFilter>().mesh = CylinderMesh2;
		go.AddComponent<MeshRenderer>();
		return go;
	}

	private static GameObject MakeCylinderButton2()
	{
		GameObject go = MakeCylinder2();
		MeshCollider mc = go.AddComponent<MeshCollider>();
		mc.sharedMesh = GenerateRoundedRectMesh2(1f, 1f, 0.08f, 6, 0.85f);
		mc.convex = true;
		mc.isTrigger = true;
		return go;
	}

	private static void RoundGameObject2(GameObject obj, string identifier, Color gradientTop, Color gradientBot)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if (component == null)
		{
			return;
		}
		Vector3 localScale = obj.transform.localScale;
		Vector3 localPosition = obj.transform.localPosition;
		Transform transform = WristMenu.menu.transform;
		GameObject rounded = new GameObject(identifier + "_rounded");
		rounded.transform.parent = transform;
		rounded.transform.rotation = Quaternion.identity;
		rounded.transform.localPosition = localPosition;
		rounded.transform.localScale = localScale;
		MeshFilter mf = rounded.AddComponent<MeshFilter>();
		MeshRenderer mr = rounded.AddComponent<MeshRenderer>();
		mf.mesh = GenerateRoundedRectMesh2(1f, 1f, 0.08f, 6, 0.85f);
		mr.material = MakeGradientMat2(gradientTop, gradientBot);
		_roundedRenderers2[identifier] = new List<Renderer> { mr };
		component.enabled = false;
	}

	private static Text AddMenuText2(string content, Vector2 size, Vector3 pos, Color color, bool rich)
	{
		GameObject labelObj = new GameObject();
		labelObj.transform.parent = WristMenu.canvasObj.transform;
		Text label = labelObj.AddComponent<Text>();
		label.font = _font2;
		label.text = content;
		label.fontSize = L2FontSize;
		label.supportRichText = rich;
		label.color = color;
		label.fontStyle = FontStyle.Bold;
		label.alignment = TextAnchor.MiddleCenter;
		label.resizeTextForBestFit = true;
		label.resizeTextMinSize = L2FontMinSize;
		label.resizeTextMaxSize = L2FontMaxSize;
		RectTransform rect = label.GetComponent<RectTransform>();
		rect.localPosition = Vector3.zero;
		rect.sizeDelta = size;
		rect.localPosition = pos;
		rect.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		return label;
	}

	private static GameObject AddMenuButton2(string id, string display, Vector3 scale, Vector3 pos, Color top, Color bot)
	{
		GameObject buttonObj = MakeCylinderButton2();
		buttonObj.transform.parent = WristMenu.menu.transform;
		buttonObj.transform.rotation = Quaternion.identity;
		buttonObj.transform.localScale = scale;
		buttonObj.transform.localPosition = pos;
		buttonObj.GetComponent<Renderer>().material = MakeGradientMat2(top, bot);
		BtnCollider collider = buttonObj.AddComponent<BtnCollider>();
		collider.buttonId = id;
		collider.displayText = display;
		RoundGameObject2(buttonObj, id, top, bot);
		return buttonObj;
	}

	internal static void Draw2()
	{
		InitFont2();
		if (MenuManager.Instance.CurrentCategoryName == "Enabled Mods")
		{
			WristMenu.RebuildEnabledMods();
		}
		WristMenu.pageSize = 7;
		WristMenu.menu = new GameObject();
		WristMenu.menu.transform.localScale = new Vector3(L2MenuRadius, L2MenuHeight, L2MenuDepth * L2MenuInitialDepthFactor);
		WristMenu.menuObj = MakeCylinder2();
		WristMenu.menuObj.transform.parent = WristMenu.menu.transform;
		WristMenu.menuObj.transform.rotation = Quaternion.identity;
		WristMenu.menuObj.transform.localScale = new Vector3(0.1f, 1f, 1f);
		Renderer bgRenderer = WristMenu.menuObj.GetComponent<Renderer>();
		Color bgTop = WristMenu.NormalColor * 0.35f;
		Color bgBot = WristMenu.NormalColor;
		bgRenderer.material = MakeGradientMat2(bgTop, bgBot);
		WristMenu.menuObj.transform.position = new Vector3(L2PanelPosX, 0f, 0f);
		RoundGameObject2(WristMenu.menuObj, "__background__", bgTop, bgBot);
		WristMenu.canvasObj = new GameObject();
		WristMenu.canvasObj.transform.parent = WristMenu.menu.transform;
		Canvas canvas = WristMenu.canvasObj.AddComponent<Canvas>();
		CanvasScaler scaler = WristMenu.canvasObj.AddComponent<CanvasScaler>();
		WristMenu.canvasObj.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.WorldSpace;
		scaler.dynamicPixelsPerUnit = L2CanvasDynamicPixelsPerUnit;
		scaler.referencePixelsPerUnit = L2CanvasReferencePixelsPerUnit;
		GameObject titleObj = new GameObject();
		titleObj.transform.parent = WristMenu.canvasObj.transform;
		Text titleText = titleObj.AddComponent<Text>();
		titleText.font = _font2;
		titleText.text = WristMenu.MenuTitle;
		titleText.fontSize = L2FontSize;
		titleText.color = WristMenu.MenuTitleColor;
		titleText.fontStyle = FontStyle.Bold;
		titleText.alignment = TextAnchor.MiddleCenter;
		titleText.resizeTextForBestFit = true;
		titleText.resizeTextMinSize = L2FontMinSize;
		titleText.resizeTextMaxSize = L2FontMaxSize;
		RectTransform titleRect = titleText.GetComponent<RectTransform>();
		titleRect.localPosition = Vector3.zero;
		titleRect.sizeDelta = new Vector2(L2TitleWidth, L2TitleHeight);
		titleRect.position = new Vector3(L2TitlePosX, 0f, L2TitlePosZ);
		titleRect.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject bottomBarObj = new GameObject();
		bottomBarObj.transform.parent = WristMenu.canvasObj.transform;
		WristMenu.fpsText = bottomBarObj.AddComponent<Text>();
		WristMenu.fpsText.font = _font2;
		WristMenu.fpsText.text = WristMenu.BottomBarText;
		WristMenu.fpsText.fontSize = L2FontSize;
		WristMenu.fpsText.color = WristMenu.ToolTipColor;
		WristMenu.fpsText.fontStyle = FontStyle.Bold;
		WristMenu.fpsText.alignment = TextAnchor.MiddleCenter;
		WristMenu.fpsText.resizeTextForBestFit = true;
		WristMenu.fpsText.resizeTextMinSize = L2FontMinSize;
		WristMenu.fpsText.resizeTextMaxSize = L2FontMaxSize;
		RectTransform bottomBarRect = WristMenu.fpsText.GetComponent<RectTransform>();
		bottomBarRect.localPosition = Vector3.zero;
		bottomBarRect.sizeDelta = new Vector2(L2StatusWidth, L2StatusHeight);
		bottomBarRect.position = new Vector3(L2StatusPosX, 0f, L2StatusPosZ);
		bottomBarRect.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		List<ButtonInfo> currentButtons = MenuManager.Instance.CurrentButtons;
		Color dcTop = WristMenu.DisconnectButtonColor * 0.35f;
		Color dcBot = WristMenu.DisconnectButtonColor;
		AddMenuButton2("DisconnectingButton", "Disconnect", new Vector3(L2ButtonScaleX, L2ButtonScaleY, L2ButtonScaleZ), new Vector3(L2SurfaceX, 0f, L2DisconnectZ), dcTop, dcBot);
		AddMenuText2("Disconnect", new Vector2(L2LabelWidth, L2LabelHeight), new Vector3(L2LabelPosX, 0f, L2LabelBaseZ - (L2LabelFirstZ - L2DisconnectZ) / L2LabelCompress), WristMenu.DisconnectTextColor, true);
		Color npTop = WristMenu.NextPrevButtonColor * 0.35f;
		Color npBot = WristMenu.NextPrevButtonColor;
		AddMenuButton2("PreviousPage", "<", new Vector3(L2NavScaleX, L2NavScaleY, L2NavScaleZ), new Vector3(L2SurfaceX, L2NavY, 0f), npTop, npBot);
		AddMenuText2("<", new Vector2(L2LabelWidth, L2LabelHeight), new Vector3(L2LabelPosX, L2NavLabelY, 0f), WristMenu.NextPrevTextColor, false);
		AddMenuButton2("NextPage", ">", new Vector3(L2NavScaleX, L2NavScaleY, L2NavScaleZ), new Vector3(L2SurfaceX, -L2NavY, 0f), npTop, npBot);
		AddMenuText2(">", new Vector2(L2LabelWidth, L2LabelHeight), new Vector3(L2LabelPosX, -L2NavLabelY, 0f), WristMenu.NextPrevTextColor, false);
		if (currentButtons != null)
		{
			ButtonInfo[] page = currentButtons.Skip(WristMenu.pageNumber * WristMenu.pageSize).Take(WristMenu.pageSize).ToArray();
			string[] labels = (from b in page select b.buttonText).ToArray();
			string[] ids = (from b in page select b.id ?? b.buttonText).ToArray();
			for (int slot = 0; slot < labels.Length; slot++)
			{
				float slotOffset = (float)slot * ((WristMenu.pageSize == 7) ? 0.116f : 0.1f);
				int buttonIndex = -1;
				for (int scanIndex = 0; scanIndex < currentButtons.Count; scanIndex++)
				{
					if (ids[slot] == currentButtons[scanIndex].id)
					{
						buttonIndex = scanIndex;
						break;
					}
				}
				bool? isEnabled = null;
				if (buttonIndex >= 0 && buttonIndex < currentButtons.Count)
				{
					isEnabled = currentButtons[buttonIndex].enabled;
				}
				Color baseColor = (isEnabled == true) ? WristMenu.ButtonColorEnabled : WristMenu.ButtonColorDisable;
				Color topColor = baseColor * 0.35f;
				Color bottomColor = baseColor;
				AddMenuButton2(ids[slot], labels[slot], new Vector3(L2ButtonScaleX, L2ButtonScaleY, L2ButtonScaleZ), new Vector3(L2SurfaceX, 0f, L2FirstButtonZ - slotOffset), topColor, bottomColor);
				AddMenuText2(labels[slot], new Vector2(L2LabelWidth, L2LabelHeight), new Vector3(L2LabelPosX, 0f, L2LabelBaseZ - slotOffset / L2LabelCompress), ((isEnabled == true) ? WristMenu.EnableTextColor : WristMenu.DisableTextColor), true);
			}
		}
		float playerScale = 1f;
		try
		{
			if (GTPlayer.Instance != null)
			{
				playerScale = GTPlayer.Instance.scale;
			}
		}
		catch
		{
			playerScale = 1f;
		}
		WristMenu.menu.transform.localScale = new Vector3(L2MenuRadius, L2MenuHeight, L2MenuDepth) * L2MenuScaleFactor * playerScale;
		try
		{
			foreach (Transform t in WristMenu.menu.GetComponentsInChildren<Transform>(true))
			{
				t.gameObject.layer = 2;
			}
			WristMenu.menu.layer = 2;
		}
		catch
		{
		}
	}

	internal static void UpdateButtonVisual2(string buttonId, string buttonText, bool isEnabled)
	{
		if (string.IsNullOrEmpty(buttonId))
		{
			return;
		}
		if (WristMenu.menu == null)
		{
			return;
		}
		Color bc = isEnabled ? WristMenu.ButtonColorEnabled : WristMenu.ButtonColorDisable;
		Color bt = bc * 0.35f;
		List<Renderer> value;
		if (_roundedRenderers2.TryGetValue(buttonId, out value))
		{
			foreach (Renderer item2 in value)
			{
				if (item2 != null)
				{
					item2.material = MakeGradientMat2(bt, bc);
				}
			}
		}
		if (WristMenu.canvasObj == null)
		{
			return;
		}
		foreach (Transform item3 in WristMenu.canvasObj.transform)
		{
			Transform val4 = item3;
			Text component3 = val4.GetComponent<Text>();
			if (component3 != null && component3.text == buttonText)
			{
				component3.color = (isEnabled ? WristMenu.EnableTextColor : WristMenu.DisableTextColor);
				break;
			}
		}
	}

	internal static IEnumerator OpenAni2()
	{
		if (WristMenu.menu == null)
		{
			yield break;
		}
		float scaleFactor = 1f;
		try
		{
			if (GTPlayer.Instance != null)
			{
				scaleFactor = GTPlayer.Instance.scale;
			}
		}
		catch
		{
			scaleFactor = 1f;
		}
		if (!WristMenu.animationsEnabled)
		{
			WristMenu.menu.transform.localScale = new Vector3(L2MenuRadius, L2MenuHeight, L2MenuDepth) * L2MenuScaleFactor * scaleFactor;
			yield break;
		}
		Vector3 targetScale = new Vector3(L2MenuRadius, L2MenuHeight, L2MenuDepth) * L2MenuScaleFactor * scaleFactor;
		Vector3 foldedScale = new Vector3(L2MenuRadius, L2MenuHeight, 0f) * L2MenuScaleFactor * scaleFactor;
		List<Transform> pageButtons = new List<Transform>();
		Transform disconnectButton = null;
		foreach (Transform child in WristMenu.menu.transform)
		{
			BtnCollider bc = child.GetComponent<BtnCollider>();
			if (bc == null || string.IsNullOrEmpty(bc.buttonId))
			{
				continue;
			}
			if (bc.buttonId == "DisconnectingButton")
			{
				disconnectButton = child;
			}
			else if (bc.buttonId != "PreviousPage" && bc.buttonId != "NextPage")
			{
				pageButtons.Add(child);
			}
		}
		SetBuildItemScale2(disconnectButton, Vector3.zero, false);
		foreach (Transform b in pageButtons)
		{
			SetBuildItemScale2(b, Vector3.zero, false);
		}
		pageButtons.Sort((a, b) => b.localPosition.z.CompareTo(a.localPosition.z));
		float elapsed = 0f;
		float bookDur = 0.18f;
		while (elapsed < bookDur)
		{
			if (WristMenu.menu == null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / bookDur);
			float eased = t * t * (3f - 2f * t);
			WristMenu.menu.transform.localScale = Vector3.Lerp(foldedScale, targetScale, eased);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (WristMenu.menu != null)
		{
			WristMenu.menu.transform.localScale = targetScale;
		}
		float btnDur = 0.08f;
		float navDur = 0.04f;
		float stagger = 0.03f;
		for (int i = 0; i < pageButtons.Count; i++)
		{
			if (WristMenu.menu == null)
			{
				yield break;
			}
			if (WristMenu.instance != null)
			{
				WristMenu.instance.StartCoroutine(BuildItem2(pageButtons[i], btnDur));
			}
			yield return new WaitForSeconds(stagger);
		}
		yield return new WaitForSeconds(btnDur);
		if (WristMenu.instance != null)
		{
			WristMenu.instance.StartCoroutine(BuildItem2(disconnectButton, navDur));
		}
		yield return new WaitForSeconds(navDur);
	}

	private static Transform FindCanvasText2(string content)
	{
		if (WristMenu.canvasObj == null)
		{
			return null;
		}
		foreach (Transform t in WristMenu.canvasObj.transform)
		{
			Text txt = t.GetComponent<Text>();
			if (txt != null && txt.text == content)
			{
				return t;
			}
		}
		return null;
	}

	private static void SetBuildItemScale2(Transform cylinder, Vector3 scale, bool showText)
	{
		if (cylinder == null)
		{
			return;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.buttonId))
		{
			return;
		}
		string id = bc.buttonId;
		string textContent = bc.displayText;
		if (id == "PreviousPage")
		{
			textContent = "<";
		}
		else if (id == "NextPage")
		{
			textContent = ">";
		}
		else if (id == "DisconnectingButton")
		{
			textContent = "Disconnect";
		}
		cylinder.localScale = scale;
		List<Renderer> rends;
		if (_roundedRenderers2.TryGetValue(id, out rends) && rends != null && rends.Count > 0 && rends[0] != null)
		{
			rends[0].transform.localScale = scale;
		}
		Transform txt = FindCanvasText2(textContent);
		if (txt != null)
		{
			txt.localScale = showText ? Vector3.one : Vector3.zero;
		}
	}

	private static IEnumerator BuildItem2(Transform cylinder, float dur)
	{
		if (cylinder == null)
		{
			yield break;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.buttonId))
		{
			yield break;
		}
		string id = bc.buttonId;
		Vector3 target = (id == "PreviousPage" || id == "NextPage") ? new Vector3(L2NavScaleX, L2NavScaleY, L2NavScaleZ) : new Vector3(L2ButtonScaleX, L2ButtonScaleY, L2ButtonScaleZ);
		string textContent = bc.displayText;
		if (id == "PreviousPage")
		{
			textContent = "<";
		}
		else if (id == "NextPage")
		{
			textContent = ">";
		}
		else if (id == "DisconnectingButton")
		{
			textContent = "Disconnect";
		}
		Transform txt = FindCanvasText2(textContent);
		float elapsed = 0f;
		while (elapsed < dur)
		{
			if (WristMenu.menu == null || cylinder == null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / dur);
			float eased = 1f - (1f - t) * (1f - t);
			Vector3 s = Vector3.Lerp(Vector3.zero, target, eased);
			cylinder.localScale = s;
			List<Renderer> rends;
			if (_roundedRenderers2.TryGetValue(id, out rends) && rends != null && rends.Count > 0 && rends[0] != null)
			{
				rends[0].transform.localScale = s;
			}
			if (txt != null)
			{
				txt.localScale = Vector3.one * eased;
			}
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (cylinder != null)
		{
			cylinder.localScale = target;
		}
		List<Renderer> rends2;
		if (_roundedRenderers2.TryGetValue(id, out rends2) && rends2 != null && rends2.Count > 0 && rends2[0] != null)
		{
			rends2[0].transform.localScale = target;
		}
		if (txt != null)
		{
			txt.localScale = Vector3.one;
		}
	}

	internal static IEnumerator CloseAni2()
	{
		if (WristMenu.menu == null || WristMenu.Close)
		{
			yield break;
		}
		if (!WristMenu.animationsEnabled)
		{
			WristMenu.DestroyMenu();
			yield break;
		}
		WristMenu.Close = true;
		float elapsed = 0f;
		Vector3 startScale = WristMenu.menu.transform.localScale;
		Vector3 targetScale = Vector3.zero;
		while (elapsed < 0.3f)
		{
			if (WristMenu.menu == null)
			{
				WristMenu.Close = false;
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			float bounce = t * t * ((s + 1f) * t - s);
			WristMenu.menu.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		Teardown2();
		WristMenu.DestroyMenu();
	}

	internal static void Teardown2()
	{
		foreach (Material mat in _gradientMaterials2)
		{
			DestroyMaterial2(mat);
		}
		_gradientMaterials2.Clear();
		_gradientCache2.Clear();
		_roundedRenderers2.Clear();
	}

	private static void DestroyMaterial2(Material mat)
	{
		if (mat == null)
		{
			return;
		}
		if (mat.HasProperty("_MainTex"))
		{
			Texture mainTex = mat.mainTexture;
			if (mainTex != null)
			{
				Object.Destroy(mainTex);
			}
		}
		Object.Destroy(mat);
	}
}
