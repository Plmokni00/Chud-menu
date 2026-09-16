using System.Collections;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static IEnumerator OpenAni()
	{
		if ((Object)(object)menu == (Object)null)
		{
			yield break;
		}
		float scaleFactor = _menuCameraAnchored ? 1f : ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f);
		if (!animationsEnabled)
		{
			menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * 0.8f * scaleFactor;
			yield break;
		}
		Vector3 targetScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * 0.8f * scaleFactor;
		Vector3 foldedScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, 0f) * 0.8f * scaleFactor;
		List<Transform> pageButtons = new List<Transform>();
		Transform prevButton = null;
		Transform nextButton = null;
		Transform disconnectButton = null;
		foreach (Transform child in menu.transform)
		{
			BtnCollider bc = child.GetComponent<BtnCollider>();
			if (bc == null || string.IsNullOrEmpty(bc.relatedText))
			{
				continue;
			}
			switch (bc.relatedText)
			{
			case "PreviousPage":
				prevButton = child;
				break;
			case "NextPage":
				nextButton = child;
				break;
			case "DisconnectingButton":
				disconnectButton = child;
				break;
			default:
				pageButtons.Add(child);
				break;
			}
		}
		SetBuildItemScale(disconnectButton, Vector3.zero, false);
		foreach (Transform b in pageButtons)
		{
			SetBuildItemScale(b, Vector3.zero, false);
		}
		pageButtons.Sort((a, b) => b.localPosition.z.CompareTo(a.localPosition.z));
		float elapsed = 0f;
		float bookDur = 0.18f;
		while (elapsed < bookDur)
		{
			if ((Object)(object)menu == (Object)null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / bookDur);
			float eased = t * t * (3f - 2f * t);
			menu.transform.localScale = Vector3.Lerp(foldedScale, targetScale, eased);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((Object)(object)menu != (Object)null)
		{
			menu.transform.localScale = targetScale;
		}
		float btnDur = 0.08f;
		float navDur = 0.04f;
		float stagger = 0.03f;
		for (int i = 0; i < pageButtons.Count; i++)
		{
			instance.StartCoroutine(BuildItem(pageButtons[i], btnDur));
			yield return new WaitForSeconds(stagger);
		}
		yield return new WaitForSeconds(btnDur);
		instance.StartCoroutine(BuildItem(disconnectButton, navDur));
		yield return new WaitForSeconds(navDur);
	}

	private static Transform FindCanvasText(string content)
	{
		if ((Object)(object)canvasObj == (Object)null)
		{
			return null;
		}
		foreach (Transform t in canvasObj.transform)
		{
			Text txt = t.GetComponent<Text>();
			if (txt != null && txt.text == content)
			{
				return t;
			}
		}
		return null;
	}

	private static void SetBuildItemScale(Transform cylinder, Vector3 scale, bool showText)
	{
		if ((Object)(object)cylinder == (Object)null)
		{
			return;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.relatedText))
		{
			return;
		}
		string id = bc.relatedText;
		string textContent = id;
		if (id == "PreviousPage") textContent = "<";
		else if (id == "NextPage") textContent = ">";
		else if (id == "DisconnectingButton") textContent = "Disconnect";
		cylinder.localScale = scale;
		if (roundedRenderers.TryGetValue(id, out var rends) && rends != null && rends.Count > 0)
		{
			rends[0].transform.localScale = scale;
		}
		Transform txt = FindCanvasText(textContent);
		if (txt != null)
		{
			txt.localScale = showText ? Vector3.one : Vector3.zero;
		}
	}

	private static IEnumerator BuildItem(Transform cylinder, float dur)
	{
		if ((Object)(object)cylinder == (Object)null)
		{
			yield break;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.relatedText))
		{
			yield break;
		}
		string id = bc.relatedText;
		Vector3 target = (id == "PreviousPage" || id == "NextPage") ? new Vector3(0.09f, 0.2f, 0.9f) : new Vector3(BUTTON_CYLINDER_SCALE_X, BUTTON_CYLINDER_SCALE_Y, BUTTON_CYLINDER_SCALE_Z);
		string textContent = id;
		if (id == "PreviousPage") textContent = "<";
		else if (id == "NextPage") textContent = ">";
		else if (id == "DisconnectingButton") textContent = "Disconnect";
		Transform txt = FindCanvasText(textContent);
		float elapsed = 0f;
		while (elapsed < dur)
		{
			if ((Object)(object)menu == (Object)null || (Object)(object)cylinder == (Object)null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / dur);
			float eased = 1f - (1f - t) * (1f - t);
			Vector3 s = Vector3.Lerp(Vector3.zero, target, eased);
			cylinder.localScale = s;
			if (roundedRenderers.TryGetValue(id, out var rends) && rends != null && rends.Count > 0)
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
		if ((Object)(object)cylinder != (Object)null)
		{
			cylinder.localScale = target;
		}
		if (roundedRenderers.TryGetValue(id, out var rends2) && rends2 != null && rends2.Count > 0)
		{
			rends2[0].transform.localScale = target;
		}
		if (txt != null)
		{
			txt.localScale = Vector3.one;
		}
	}

	public static IEnumerator CloseAni()
	{
		if ((Object)(object)menu == (Object)null || Close)
		{
			yield break;
		}
		if (!animationsEnabled)
		{
			DestroyMenu();
			yield break;
		}
		Close = true;
		float elapsed = 0f;
		Vector3 startScale = menu.transform.localScale;
		Vector3 targetScale = Vector3.zero;
		while (elapsed < 0.3f)
		{
			if ((Object)(object)menu == (Object)null)
			{
				Close = false;
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			float bounce = t * t * ((s + 1f) * t - s);
			menu.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		DestroyGradientResources();
		if ((Object)(object)menu != (Object)null)
		{
			Object.Destroy((Object)(object)menu);
		}
		menu = null;
		menuObj = null;
		canvasObj = null;
		if ((Object)(object)reference != (Object)null)
		{
			Object.Destroy((Object)(object)reference);
		}
		reference = null;
		if ((Object)(object)_menuAnchor != (Object)null)
		{
			Object.Destroy((Object)(object)_menuAnchor);
		}
		_menuAnchor = null;
		_menuFollowHand = null;
		Close = false;
	}
}