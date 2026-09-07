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
			abuttonDown = ControllerInputPoller.instance.rightControllerPrimaryButton;
			bbuttonDown = ControllerInputPoller.instance.rightControllerSecondaryButton;
			xbuttonDown = ControllerInputPoller.instance.leftControllerPrimaryButton;
			ybuttonDown = ControllerInputPoller.instance.leftControllerSecondaryButton;
			joy = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
			joyL = ControllerInputPoller.instance.leftControllerPrimary2DAxis;
			bool qKeyDown = !XRSettings.isDeviceActive && Keyboard.current != null && ((ButtonControl)Keyboard.current.qKey).isPressed;
			if (Mods.activeMenuStyle == 5 && (Object)(object)menu != (Object)null && !menu.GetComponent<Rigidbody>())
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
			List<string> list = new List<string>();
			if (showFPS)
			{
				list.Add("FPS: " + cachedFPS);
			}
			if (showSessionTime)
			{
				list.Add((int)timeSpan.TotalMinutes + ":" + timeSpan.Seconds.ToString("D2"));
			}
			bottomBarStr = string.Join(" | ", list);
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
		if ((Object)(object)_menuAnchor != (Object)null && (Object)(object)_menuFollowHand != (Object)null)
		{
			_menuAnchor.transform.position = _menuFollowHand.position + Vector3.up * 0.02f;
			_menuAnchor.transform.rotation = _menuFollowHand.rotation;
		}
	}

	private void HandleTriggerPageNav()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null) return;
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
		bool flag2 = (ybuttonDown && !Mods.isRightHanded) || (bbuttonDown && Mods.isRightHanded) || qKeyDown;

		if (toggleMenu)
		{
			bool justPressed = flag2 && !_prevToggleButton;
			_prevToggleButton = flag2;

			if (justPressed)
			{
				if ((Object)(object)menu == (Object)null && !Close)
				{
					_menuStickyOpen = true;
				}
				else if ((Object)(object)menu != (Object)null && !Close)
				{
					_menuStickyOpen = false;
					Object.Destroy((Object)(object)reference);
					reference = null;
					instance.StartCoroutine(CloseAni());
					return;
				}
			}

			if (_menuStickyOpen)
			{
				flag2 = true;
			}
			else
			{
				flag2 = false;
			}
		}
		else
		{
			_prevToggleButton = flag2;
			_menuStickyOpen = false;
		}

		if (flag2)
		{
			if ((Object)(object)menu == (Object)null)
			{
				_menuCameraAnchored = qKeyDown;
			}
			else if (!toggleMenu)
			{
				_menuCameraAnchored = qKeyDown;
			}
			if ((Object)(object)menu == (Object)null)
			{
				instance.Draw();
				menu.transform.localScale = Vector3.one * 0.001f;
				instance.StartCoroutine(OpenAni());
			}
			if (qKeyDown)
			{
				_menuFollowHand = null;
				if ((Object)(object)_tpc == (Object)null)
				{
					GameObject val = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if ((Object)(object)val != (Object)null)
					{
						_tpc = val.GetComponent<Camera>();
					}
					if ((Object)(object)_tpc == (Object)null)
					{
						val = GameObject.Find("Shoulder Camera");
						if ((Object)(object)val != (Object)null)
						{
							_tpc = val.GetComponent<Camera>();
			}
		}
	}
				if ((Object)(object)_tpc != (Object)null)
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
				if ((Object)(object)reference == (Object)null)
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
				if ((Object)(object)_menuAnchor == (Object)null)
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
				if ((Object)(object)reference == (Object)null)
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
				if ((Object)(object)_menuAnchor == (Object)null)
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
				if ((Object)(object)reference == (Object)null)
				{
					reference = MakeSphereButtonPresser();
					((Object)reference).name = "buttonPresser";
				}
				reference.transform.parent = GTPlayer.Instance.LeftHand.controllerTransform;
				reference.transform.localPosition = PointerPos;
				reference.transform.localScale = PointerScale;
			}
		}
		else if (!flag2 && (Object)(object)menu != (Object)null && !Close)
		{
			Object.Destroy((Object)(object)reference);
			reference = null;
			instance.StartCoroutine(CloseAni());
		}
		if (toggleMenu && _menuStickyOpen && !Close)
		{
			if ((Object)(object)menu == (Object)null)
			{
				instance.Draw();
			}
			RestoreMenuAnchor();
			HandleMouseMenuClick();
		}
	}

	private void HandleMouseMenuClick()
	{
		if ((Object)(object)menu == (Object)null || Close || !_menuCameraAnchored)
		{
			return;
		}
		if ((Object)(object)_tpc == (Object)null || Mouse.current == null || (Object)(object)reference == (Object)null)
		{
			return;
		}
		bool isPressed = Mouse.current.leftButton.isPressed;
		if (isPressed && !_mouseWasPressed)
		{
			Ray val2 = _tpc.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
			RaycastHit val3 = default(RaycastHit);
			if (Physics.Raycast(val2, out val3, 512f, 1 << 2, QueryTriggerInteraction.Collide) && (Object)(object)val3.transform != (Object)(object)reference.transform)
			{
				BtnCollider component = ((Component)val3.transform).gameObject.GetComponent<BtnCollider>();
				if ((Object)(object)component != (Object)null && !string.IsNullOrEmpty(component.relatedText))
				{
					Toggle(component.relatedText);
				}
			}
		}
		_mouseWasPressed = isPressed;
	}

	private void UpdateMasterClientStatus()
	{
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Master Mods");
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
		bool flag = PhotonNetwork.LocalPlayer != null && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId) && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
		bool flag2 = MenuManager.Categories.Any((MenuCategory c) => c.Name == "Console Mods");
		bool flag3 = false;
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Main");
		if (menuCategory != null)
		{
			flag3 = menuCategory.Buttons.Any((ButtonInfo b) => b.buttonText == "Console Mods");
		}
		if (flag && flag2 && !flag3)
		{
			string text = ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId];
			if (!_adminInitialized)
			{
				string text2 = (ServerData.SuperAdministrators.Contains(text) ? "super admin " : "");
				NotifiLib.SendNotification("Welcome " + text2 + text, 2);
				_adminInitialized = true;
			}
			if (menuCategory != null && !flag3)
			{
				menuCategory.Buttons.Add(new ButtonInfo
				{
					buttonText = "Console Mods",
					method = delegate
					{
						MenuManager.ToggleCategory("Console Mods");
					},
					enabled = false,
					type = ButtonType.Action,
					toolTip = "Go to Console Mods!"
				});
			}
		}
		else if (!flag && flag3)
		{
			menuCategory?.Buttons.RemoveAll((ButtonInfo b) => b.buttonText == "Console Mods");
			if (MenuManager.CurrentCategoryName == "Console Mods" || MenuManager.CurrentCategoryName == "Console Settings")
			{
				MenuManager.CurrentCategoryName = "Main";
			}
			pageNumber = 0;
			bool wasOpen = (Object)(object)menu != (Object)null;
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
				if (wasOpen && (Object)(object)instance != (Object)null)
				{
					instance.Draw();
				}
			}
		}
	}
}