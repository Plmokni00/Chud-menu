using System.Collections;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	private static readonly Dictionary<string, Vector3> _animationScales = new Dictionary<string, Vector3>();

	private static readonly Dictionary<string, Vector3> _visualRestScales = new Dictionary<string, Vector3>();

	private static Vector3 VisualRestScale(string id)
	{
		Vector3 recorded;
		if (!string.IsNullOrEmpty(id) && _visualRestScales.TryGetValue(id, out recorded))
		{
			return recorded;
		}
		return Vector3.one;
	}

	public static IEnumerator OpenAni()
	{
		if (menuLayout == 0)
		{
			return MenuLayout2.OpenAni2();
		}
		return OpenAni1();
	}

	private static IEnumerator OpenAni1()
	{
		if (menu == (Object)null)
		{
			yield break;
		}
		float scaleFactor = _menuCameraAnchored ? 1f : ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f);
		if (!animationsEnabled)
		{
			menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * MenuScaleFactor * scaleFactor;
			yield break;
		}
		Vector3 targetScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * MenuScaleFactor * scaleFactor;
		Vector3 foldedScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, 0f) * MenuScaleFactor * scaleFactor;

		_animationScales.Clear();
		_visualRestScales.Clear();
		List<Transform> pageButtons = new List<Transform>();
		Transform disconnectButton = null;
		foreach (Transform child in menu.transform)
		{
			BtnCollider bc = child.GetComponent<BtnCollider>();
			if (bc == null || string.IsNullOrEmpty(bc.buttonId))
			{
				continue;
			}
			_animationScales[bc.buttonId] = child.localScale;
			List<Renderer> rends;
			if (roundedRenderers.TryGetValue(bc.buttonId, out rends) && rends != null && rends.Count > 0 && rends[0] != (Object)null)
			{
				_visualRestScales[bc.buttonId] = rends[0].transform.localScale;
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
			if (menu == (Object)null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / bookDur);
			float eased = t * t * (3f - 2f * t);
			menu.transform.localScale = Vector3.Lerp(foldedScale, targetScale, eased);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (menu != (Object)null)
		{
			menu.transform.localScale = targetScale;
		}

		float btnDur = 0.08f;
		float navDur = 0.04f;
		float stagger = 0.03f;
		for (int i = 0; i < pageButtons.Count; i++)
		{
			if (menu == (Object)null)
			{
				yield break;
			}
			instance.StartCoroutine(BuildItem(pageButtons[i], btnDur));
			yield return new WaitForSeconds(stagger);
		}
		yield return new WaitForSeconds(btnDur);
		instance.StartCoroutine(BuildItem(disconnectButton, navDur));
		yield return new WaitForSeconds(navDur);
	}

	private static Vector3 ItemRestScale(Transform item)
	{
		if (item == (Object)null)
		{
			return Vector3.one;
		}
		BtnCollider bc = item.GetComponent<BtnCollider>();
		if (bc != null && !string.IsNullOrEmpty(bc.buttonId))
		{
			Vector3 recorded;
			if (_animationScales.TryGetValue(bc.buttonId, out recorded))
			{
				return recorded;
			}
		}
		return item.localScale;
	}

	private static RectTransform FindItemLabel(string id)
	{
		Text label;
		if (!string.IsNullOrEmpty(id) && TextLabels.TryGetValue(id, out label) && label != null)
		{
			return label.rectTransform;
		}
		return null;
	}

	private static void SetBuildItemScale(Transform cylinder, Vector3 scale, bool showText)
	{
		if (cylinder == (Object)null)
		{
			return;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.buttonId))
		{
			return;
		}
		string id = bc.buttonId;
		cylinder.localScale = scale;
		if (roundedRenderers.TryGetValue(id, out var rends) && rends != null && rends.Count > 0 && rends[0] != (Object)null)
		{
			rends[0].transform.localScale = scale;
		}
		RectTransform label = FindItemLabel(id);
		if (label != null)
		{
			label.localScale = TextLabelScale(id, showText ? 1f : 0f);
		}
	}

	private static IEnumerator BuildItem(Transform cylinder, float dur)
	{
		if (cylinder == (Object)null)
		{
			yield break;
		}
		BtnCollider bc = cylinder.GetComponent<BtnCollider>();
		if (bc == null || string.IsNullOrEmpty(bc.buttonId))
		{
			yield break;
		}
		string id = bc.buttonId;
		Vector3 target = ItemRestScale(cylinder);
		Vector3 visualTarget = VisualRestScale(id);
		RectTransform label = FindItemLabel(id);
		float elapsed = 0f;
		while (elapsed < dur)
		{
			if (menu == (Object)null || cylinder == (Object)null)
			{
				yield break;
			}
			float t = Mathf.Clamp01(elapsed / dur);
			float eased = 1f - (1f - t) * (1f - t);
			Vector3 s = Vector3.Lerp(Vector3.zero, target, eased);
			cylinder.localScale = s;
			if (roundedRenderers.TryGetValue(id, out var rends) && rends != null && rends.Count > 0 && rends[0] != (Object)null)
			{
				rends[0].transform.localScale = Vector3.Lerp(Vector3.zero, visualTarget, eased);
			}
			if (label != null)
			{
				label.localScale = TextLabelScale(id, eased);
			}
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (cylinder != (Object)null)
		{
			cylinder.localScale = target;
		}
		if (roundedRenderers.TryGetValue(id, out var rends2) && rends2 != null && rends2.Count > 0 && rends2[0] != (Object)null)
		{
			rends2[0].transform.localScale = visualTarget;
		}
		if (label != null)
		{
			label.localScale = TextLabelScale(id, 1f);
		}
	}

	public static IEnumerator CloseAni()
	{
		if (menuLayout == 0)
		{
			return MenuLayout2.CloseAni2();
		}
		return CloseAni1();
	}

	private static IEnumerator CloseAni1()
	{
		if (menu == (Object)null || Close)
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
			if (menu == (Object)null)
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
		MenuLayout2.Teardown2();
		DestroyGradientResources();
		if (menu != (Object)null)
		{
			Object.Destroy(menu);
		}
		menu = null;
		menuObj = null;
		canvasObj = null;
		if (reference != (Object)null)
		{
			Object.Destroy(reference);
		}
		reference = null;
		if (_menuAnchor != (Object)null)
		{
			Object.Destroy(_menuAnchor);
		}
		_menuAnchor = null;
		_menuFollowHand = null;
		_animationScales.Clear();
		_visualRestScales.Clear();
		Close = false;
	}
}
