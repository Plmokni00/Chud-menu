using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	private static Shader _unlitTexShader;
	private static Shader UnlitTexShader => _unlitTexShader ?? (_unlitTexShader = Shader.Find("Unlit/Texture"));
	private static readonly Dictionary<string, Material> _gradientCache = new Dictionary<string, Material>();
	private static string GradientKey(Color a, Color b) => $"{a.r:F3},{a.g:F3},{a.b:F3},{a.a:F3}|{b.r:F3},{b.g:F3},{b.b:F3},{b.a:F3}";
	public static Material MakeGradientMat(Color top, Color bot)
	{
		string k = GradientKey(top, bot);
		if (_gradientCache.TryGetValue(k, out var cm) && cm != null) { if (!gradientMaterials.Contains(cm)) gradientMaterials.Add(cm); return cm; }
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
		Shader s = UnlitTexShader;
		if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
		Material mat = new Material(s);
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.mainTexture = tex;
		mat.color = Color.white;
		mat.mainTextureScale = new Vector2(1, 0.5f);
		gradientMaterials.Add(mat);
		_gradientCache[k] = mat;
		return mat;
	}

	private static Shader _unlitColorShader;
	private static Shader UnlitColorShader => _unlitColorShader ?? (_unlitColorShader = Shader.Find("Unlit/Color"));
	public static Material MakePlainMat(Color c)
	{
		Shader s = UnlitColorShader;
		if ((Object)(object)s == (Object)null) s = Shader.Find("Universal Render Pipeline/Unlit");
		if ((Object)(object)s == (Object)null) s = Shader.Find("GUI/Text Shader");
		Material mat = new Material(s);
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.color = c;
		return mat;
	}

	internal static void UpdateGradientAnimations(float time)
	{
		float offsetY = time * 0.2f;
		Vector2 offset = new Vector2(0f, offsetY);
		for (int i = gradientMaterials.Count - 1; i >= 0; i--)
		{
			if ((Object)(object)gradientMaterials[i] == (Object)null)
				gradientMaterials.RemoveAt(i);
			else
				gradientMaterials[i].mainTextureOffset = offset;
		}
	}

	public static void RoundGameObject(GameObject obj, string identifier, Color gradientTop, Color gradientBot)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Vector3 localScale = obj.transform.localScale;
		Vector3 localPosition = obj.transform.localPosition;
		Transform transform = menu.transform;
		GameObject rounded = new GameObject(identifier + "_rounded");
		rounded.transform.parent = transform;
		rounded.transform.rotation = Quaternion.identity;
		rounded.transform.localPosition = localPosition;
		rounded.transform.localScale = localScale;
		MeshFilter mf = rounded.AddComponent<MeshFilter>();
		MeshRenderer mr = rounded.AddComponent<MeshRenderer>();
		mf.mesh = GenerateRoundedRectMesh(1f, 1f, 0.08f, 6, 0.85f);
		mr.material = MakeGradientMat(gradientTop, gradientBot);
		roundedRenderers[identifier] = new List<Renderer> { mr };
		component.enabled = false;
	}

	private static void DestroyGradientResources()
	{
		foreach (Material mat in gradientMaterials)
		{
			DestroyMaterial(mat);
		}
		gradientMaterials.Clear();
		_gradientCache.Clear();
		roundedRenderers.Clear();
		if (reference != null)
		{
			Renderer refRenderer = reference.GetComponent<Renderer>();
			if (refRenderer != null && refRenderer.material != null && !_gradientCache.ContainsValue(refRenderer.material))
			{
				DestroyMaterial(refRenderer.material);
			}
		}
	}

	private static void DestroyMaterial(Material mat)
	{
		if ((Object)(object)mat == (Object)null)
		{
			return;
		}
		if (mat.HasProperty("_MainTex"))
		{
			Texture mainTex = mat.mainTexture;
			if ((Object)(object)mainTex != (Object)null)
			{
				Object.Destroy((Object)(object)mainTex);
			}
		}
		Object.Destroy((Object)(object)mat);
	}
}