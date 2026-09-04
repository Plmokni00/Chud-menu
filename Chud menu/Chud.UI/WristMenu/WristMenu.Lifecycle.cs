using System;
using System.Collections;
using Chud.Backend;
using GorillaLocomotion;
using GTAG_NotificationLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static void DestroyMenu()
	{
		DestroyGradientResources();
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
		Object.Destroy((Object)(object)menu);
		menu = null;
		menuObj = null;
		canvasObj = null;
	}

	public static void RefreshMenu()
	{
		DestroyGradientResources();
		if ((Object)(object)reference != (Object)null)
		{
			Object.Destroy((Object)(object)reference);
		}
		reference = null;
		Close = false;
		if ((Object)(object)menu != (Object)null)
		{
			Object.Destroy((Object)(object)menu);
		}
		menu = null;
		menuObj = null;
		canvasObj = null;
		instance.Draw();
		RestoreMenuAnchor();
	}

	// Live re-anchor when the Right Hand setting is toggled while the menu is open in toggle mode.
	// Rebuilds the menu on the new hand (menu on the chosen hand, pointer on the opposite hand).
	public static void ReanchorToCurrentHand()
	{
		if (!toggleMenu || _menuCameraAnchored)
		{
			return;
		}
		if ((Object)(object)menu == (Object)null || Close)
		{
			return;
		}
		_menuAnchorIsRightHand = Mods.isRightHanded;
		_menuFollowHand = (_menuAnchorIsRightHand ? GTPlayer.Instance.RightHand.controllerTransform : GTPlayer.Instance.LeftHand.controllerTransform);
		if ((Object)(object)_menuAnchor == (Object)null)
		{
			_menuAnchor = new GameObject("menuAnchor");
		}
		RefreshMenu();
	}

	private static void RestoreMenuAnchor()
	{
		if ((Object)(object)menu == (Object)null)
		{
			return;
		}
		if (_menuCameraAnchored)
		{
			Transform cam = (Object)(object)_tpc != (Object)null ? ((Component)_tpc).transform : ((Component)GTPlayer.Instance.headCollider).transform;
			menu.transform.parent = cam;
			menu.transform.position = cam.position + cam.forward * 0.5f + Vector3.down * 0.03f;
			menu.transform.rotation = cam.rotation * Quaternion.Euler(-90f, 90f, 0f);
			if ((Object)(object)reference == (Object)null || (Object)(object)reference.GetComponent<Renderer>() == (Object)null || (Object)(object)reference.GetComponent<Renderer>().material == (Object)null)
			{
				if ((Object)(object)reference != (Object)null)
				{
					Object.Destroy((Object)(object)reference);
				}
				reference = MakeSphereButtonPresser();
				((Object)reference).name = "buttonPresser";
			}
			reference.transform.parent = GTPlayer.Instance.RightHand.controllerTransform;
			reference.transform.localPosition = PointerPos;
			reference.transform.localScale = PointerScale;
		}
		else if ((Object)(object)_menuAnchor != (Object)null)
		{
			if ((Object)(object)_menuFollowHand == (Object)null)
			{
				_menuFollowHand = (_menuAnchorIsRightHand ? GTPlayer.Instance.RightHand.controllerTransform : GTPlayer.Instance.LeftHand.controllerTransform);
			}
			menu.transform.parent = _menuAnchor.transform;
			menu.transform.localPosition = Vector3.zero;
			menu.transform.localRotation = _menuAnchorIsRightHand ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
			_menuAnchor.transform.position = _menuFollowHand.position;
			_menuAnchor.transform.rotation = _menuFollowHand.rotation;
			if ((Object)(object)reference == (Object)null || (Object)(object)reference.GetComponent<Renderer>() == (Object)null || (Object)(object)reference.GetComponent<Renderer>().material == (Object)null)
			{
				if ((Object)(object)reference != (Object)null)
				{
					Object.Destroy((Object)(object)reference);
				}
				reference = MakeSphereButtonPresser();
				((Object)reference).name = "buttonPresser";
			}
			reference.transform.parent = (_menuAnchorIsRightHand ? GTPlayer.Instance.LeftHand.controllerTransform : GTPlayer.Instance.RightHand.controllerTransform);
			reference.transform.localPosition = PointerPos;
			reference.transform.localScale = PointerScale;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		Backend.Console.LoadConsole();
		MenuConfig.InitCategories();
		InitMenuFont();
		sessionStartTime = DateTime.Now;
		this.StartCoroutine(LoadCustomButtonClickAudio());
		Draw();
		Mods.Load();
		StartCoroutine(ShowWelcomeDelayed());
	}

	private System.Collections.IEnumerator ShowWelcomeDelayed()
	{
		yield return new WaitForSeconds(2f);
		NotifiLib.SendNotification("Thanks for choosing Chud Menu please enjoy :3", 2);
	}
}