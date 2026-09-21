using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using GTAG_NotificationLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	private void Update()
	{
		try
		{
			if (ControllerInputPoller.instance == null) return;
			gripDownL = ControllerInputPoller.instance.leftGrab;			gripDownR = ControllerInputPoller.instance.rightGrab;
		triggerDownL = ControllerInputPoller.instance.leftControllerIndexFloat == 1f;
		triggerDownR = ControllerInputPoller.instance.rightControllerIndexFloat == 1f;
		bbuttonDown = ControllerInputPoller.instance.rightControllerSecondaryButton;
			xbuttonDown = ControllerInputPoller.instance.leftControllerPrimaryButton;
			ybuttonDown = ControllerInputPoller.instance.leftControllerSecondaryButton;
			joy = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
			joyL = ControllerInputPoller.instance.leftControllerPrimary2DAxis;
			bool qKeyDown = !XRSettings.isDeviceActive && Keyboard.current != null && ((ButtonControl)Keyboard.current.qKey).isPressed;
			if (Mods.activeMenuStyle == 5 && menu != (Object)null && !menu.GetComponent<Rigidbody>())
			{
				HandleTriggerPageNav();
			}
			HandleMenuFollow(qKeyDown);
			Mods.UpdateActiveMods();
			_frameCounter++;
			if (_frameCounter % 15 == 0)
			{
				UpdateMasterClientStatus();
				CheckAdminStatus();
			}
			fpsAccumulator += Time.unscaledDeltaTime;
			fpsFrameCount++;
			if (fpsFrameCount >= 30)
			{
				int num = ((fpsAccumulator > 0f) ? Mathf.RoundToInt((float)fpsFrameCount / fpsAccumulator) : 0);
				fpsAccumulator = 0f;
				fpsFrameCount = 0;
				cachedFPS = num;
			}
			TimeSpan timeSpan = DateTime.Now - sessionStartTime;
			string fpsPart = showFPS ? "FPS: " + cachedFPS : null;
			string sessionPart = showSessionTime ? (int)timeSpan.TotalMinutes + ":" + timeSpan.Seconds.ToString("D2") : null;
			if (fpsPart != null && sessionPart != null)
				bottomBarStr = fpsPart + " | " + sessionPart;
			else if (fpsPart != null)
				bottomBarStr = fpsPart;
			else if (sessionPart != null)
				bottomBarStr = sessionPart;
			else
				bottomBarStr = "";
			if (menu != null && fpsText != null)
			{
				fpsText.text = bottomBarStr;
			}
			if (_frameCounter % 60 == 0)
			{
				if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);
				if (customBoardsEnabled && !customBoardsApplied) { UpdateCustomBoardText(); customBoardsApplied = true; }
				else if (!customBoardsEnabled) customBoardsApplied = false;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("[Chud] WristMenu.Update: " + e);
		}
	}

	private void LateUpdate()
	{
		if (_menuAnchor != (Object)null && _menuFollowHand != (Object)null)
		{
			_menuAnchor.transform.position = _menuFollowHand.position + Vector3.up * 0.02f;
			_menuAnchor.transform.rotation = _menuFollowHand.rotation;
		}
	}

	private void HandleTriggerPageNav()
	{
		if (VRRig.LocalRig == (Object)null) return;
		if (triggerDownL)
		{
			if (!leftTriggerLocked)
			{
				Toggle("PreviousPage");
				try { VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f); } catch { }
				leftTriggerLocked = true;
			}
		}
		else leftTriggerLocked = false;
		if (triggerDownR)
		{
			if (!rightTriggerLocked)
			{
				Toggle("NextPage");
				try { VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, false, 0.1f); } catch { }
				rightTriggerLocked = true;
			}
		}
		else rightTriggerLocked = false;
	}

	private void HandleMenuFollow(bool qKeyDown)
	{
		bool menuButtonHeld = (ybuttonDown && !Mods.isRightHanded) || (bbuttonDown && Mods.isRightHanded) || qKeyDown;

		if (toggleMenu)
		{
			bool justPressed = menuButtonHeld && !_prevToggleButton;
			_prevToggleButton = menuButtonHeld;

			if (justPressed)
			{
				if (menu == (Object)null && !Close)
				{
					_menuStickyOpen = true;
				}
				else if (menu != (Object)null && !Close)
				{
					_menuStickyOpen = false;
					Object.Destroy(reference);
					reference = null;
					instance.StartCoroutine(CloseAni());
					return;
				}
			}

			if (_menuStickyOpen)
			{
				menuButtonHeld = true;
			}
			else
			{
				menuButtonHeld = false;
			}
		}
		else
		{
			_prevToggleButton = menuButtonHeld;
			_menuStickyOpen = false;
		}

		if (menuButtonHeld)
		{
			if (menu == (Object)null)
			{
				_menuCameraAnchored = qKeyDown;
			}
			else if (!toggleMenu)
			{
				_menuCameraAnchored = qKeyDown;
			}
			if (menu == (Object)null)
			{
				instance.Draw();
				menu.transform.localScale = Vector3.one * 0.001f;
				instance.StartCoroutine(OpenAni());
			}
			if (qKeyDown)
			{
				_menuFollowHand = null;
				if (_tpc == (Object)null)
				{
					GameObject val = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if (val != (Object)null)
					{
						_tpc = val.GetComponent<Camera>();
					}
					if (_tpc == (Object)null)
					{
						val = GameObject.Find("Shoulder Camera");
						if (val != (Object)null)
						{
							_tpc = val.GetComponent<Camera>();
			}
		}
	}
				if (_tpc != (Object)null)
				{
					menu.transform.parent = ((Component)_tpc).transform;
					menu.transform.position = ((Component)_tpc).transform.position + ((Component)_tpc).transform.forward * 0.5f + Vector3.down * 0.03f;
					menu.transform.rotation = ((Component)_tpc).transform.rotation * Quaternion.Euler(-90f, 90f, 0f);
					HandleMouseMenuClick();
				}
				else
				{
					menu.transform.parent = ((Component)GTPlayer.Instance.headCollider).transform;
					menu.transform.position = ((Component)GTPlayer.Instance.headCollider).transform.position + ((Component)GTPlayer.Instance.headCollider).transform.forward * 0.5f + Vector3.down * 0.03f;
					menu.transform.rotation = ((Component)GTPlayer.Instance.headCollider).transform.rotation * Quaternion.Euler(-90f, 90f, 0f);
				}
				if (reference == (Object)null)
				{
					reference = MakeSphereButtonPresser();
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.RightHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
			}
			else if (ybuttonDown && !Mods.isRightHanded)
			{
				if (_menuAnchor == (Object)null)
				{
					_menuAnchor = new GameObject("menuAnchor");
				}
				_menuFollowHand = GTPlayer.Instance.LeftHand.controllerTransform;
				_menuAnchorIsRightHand = false;
				menu.transform.parent = _menuAnchor.transform;
				menu.transform.localPosition = Vector3.zero;
				menu.transform.localRotation = Quaternion.identity;
				_menuAnchor.transform.position = _menuFollowHand.position + Vector3.up * 0.02f;
				_menuAnchor.transform.rotation = _menuFollowHand.rotation;
				if (reference == (Object)null)
				{
					reference = MakeSphereButtonPresser();
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.RightHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
			}
			else if (bbuttonDown && Mods.isRightHanded)
			{
				if (_menuAnchor == (Object)null)
				{
					_menuAnchor = new GameObject("menuAnchor");
				}
				_menuFollowHand = GTPlayer.Instance.RightHand.controllerTransform;
				_menuAnchorIsRightHand = true;
				menu.transform.parent = _menuAnchor.transform;
				menu.transform.localPosition = Vector3.zero;
				menu.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				_menuAnchor.transform.position = _menuFollowHand.position + Vector3.up * 0.02f;
				_menuAnchor.transform.rotation = _menuFollowHand.rotation;
				if (reference == (Object)null)
				{
					reference = MakeSphereButtonPresser();
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.LeftHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
			}
		}
		else if (!menuButtonHeld && menu != (Object)null && !Close)
		{
			Object.Destroy(reference);
			reference = null;
			instance.StartCoroutine(CloseAni());
		}
		if (toggleMenu && _menuStickyOpen && !Close)
		{
			if (menu == (Object)null)
			{
				instance.Draw();
			}
			RestoreMenuAnchor();
			HandleMouseMenuClick();
		}
	}

	private void HandleMouseMenuClick()
	{
		if (menu == (Object)null || Close || !_menuCameraAnchored)
		{
			return;
		}
		if (_tpc == (Object)null || Mouse.current == null || reference == (Object)null)
		{
			return;
		}
		bool isPressed = Mouse.current.leftButton.isPressed;
		if (isPressed && !_mouseWasPressed)
		{
			Ray val2 = _tpc.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
			RaycastHit val3 = default(RaycastHit);
			if (Physics.Raycast(val2, out val3, 512f, 1 << 2, QueryTriggerInteraction.Collide) && val3.transform != reference.transform)
			{
				BtnCollider component = ((Component)val3.transform).gameObject.GetComponent<BtnCollider>();
				if (component != (Object)null && !string.IsNullOrEmpty(component.buttonId))
				{
					Toggle(component.buttonId);
				}
			}
		}
		_mouseWasPressed = isPressed;
	}

	private void UpdateMasterClientStatus()
	{
		MenuCategory menuCategory = MenuManager.Instance.Categories.Find((MenuCategory c) => c.Name == "Master Mods");
		if (menuCategory == null || menuCategory.Buttons.Count <= 1)
		{
			return;
		}
		bool isMasterClient = PhotonNetwork.IsMasterClient;
		menuCategory.Buttons[1].buttonText = (isMasterClient ? "You are master client" : "Not master client");
		menuCategory.Buttons[1].toolTip = (isMasterClient ? "You are the master client" : "You are not the master client");
		if (isMasterClient)
		{
			return;
		}
		for (int num = 2; num < menuCategory.Buttons.Count; num++)
		{
			ButtonInfo buttonInfo = menuCategory.Buttons[num];
			if (buttonInfo.enabled != true)
			{
				continue;
			}
			if (buttonInfo.disableMethod != null)
			{
				try
				{
					buttonInfo.disableMethod();
				}
				catch
				{
				}
			}
			buttonInfo.enabled = false;
			Mods.InvalidateActiveButtonsCache();
		}
	}

	private void CheckAdminStatus()
	{
		bool isAdmin = PhotonNetwork.LocalPlayer != null && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId) && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
		bool hasConsoleCategory = MenuManager.Instance.Categories.Any((MenuCategory c) => c.Name == "Console Mods");
		bool hasConsoleButton = false;
		MenuCategory menuCategory = MenuManager.Instance.Categories.Find((MenuCategory c) => c.Name == "Main");
		if (menuCategory != null)
		{
			hasConsoleButton = menuCategory.Buttons.Any((ButtonInfo b) => b.id == "main_console_mods");
		}
		if (isAdmin && hasConsoleCategory && !hasConsoleButton)
		{
			string text = ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId];
			if (!_adminInitialized)
			{
				string text2 = (ServerData.SuperAdministrators.Contains(text) ? "super admin " : "");
				NotifiLib.SendNotification("Welcome " + text2 + text, 2);
				_adminInitialized = true;
			}
			if (menuCategory != null && !hasConsoleButton)
			{
				menuCategory.Buttons.Add(new ButtonInfo
				{
					id = "main_console_mods",
					buttonText = "Console Mods",
					method = delegate
					{
						MenuManager.Instance.ToggleCategory("Console Mods");
					},
					enabled = false,
					type = ButtonType.Action,
					toolTip = "Go to Console Mods!"
				});
			}
		}
		else if (!isAdmin && hasConsoleButton)
		{
			menuCategory?.Buttons.RemoveAll((ButtonInfo b) => b.id == "main_console_mods");
			if (MenuManager.Instance.CurrentCategoryName == "Console Mods" || MenuManager.Instance.CurrentCategoryName == "Console Settings")
			{
				MenuManager.Instance.CurrentCategoryName = "Main";
			}
			pageNumber = 0;
			bool wasOpen = menu != (Object)null;
			if (toggleMenu)
			{
				if (wasOpen)
				{
					RefreshMenu();
				}
			}
			else
			{
				DestroyMenu();
				if (wasOpen && instance != (Object)null)
				{
					instance.Draw();
				}
			}
		}
	}
}