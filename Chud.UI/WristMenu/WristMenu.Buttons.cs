using System;
using System.Collections.Generic;
using Chud.Backend;
using Chud.Classes;
using GTAG_NotificationLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static void RebuildEnabledMods()
	{
		MenuCategory menuCategory = MenuManager.Instance.Categories.Find((MenuCategory c) => c.Name == "Enabled Mods");
		if (menuCategory == null)
		{
			return;
		}
		menuCategory.Buttons.Clear();
		menuCategory.Buttons.Add(new ButtonInfo
		{
			id = "enabled_exit",
			buttonText = "Exit Enabled Mods",
			method = delegate
			{
				MenuManager.Instance.ToggleCategory("Enabled Mods");
			},
			enabled = false,
			type = ButtonType.Action,
			toolTip = "Go to Main"
		});
		foreach (MenuCategory category in MenuManager.Instance.Categories)
		{
			if (category.Name == "Main" || category.Name == "Enabled Mods" || category.Name == "Console Mods" || category.Name == "Console Settings")
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled != true || button.disableMethod == null)
				{
					continue;
				}
				string capturedId = button.id;
				string capturedText = button.buttonText;
				menuCategory.Buttons.Add(new ButtonInfo
				{
					id = capturedId,
					buttonText = capturedText,
					method = delegate
					{
						Mods.FindAndToggleButton(capturedId);
						if (toggleMenu)
						{
							RefreshMenu();
						}
						else
						{
							DestroyMenu();
							if (instance != (Object)null)
							{
								instance.Draw();
							}
						}
					},
					enabled = true,
					type = ButtonType.Action,
					toolTip = (button.toolTip ?? "")
				});
			}
		}
	}

	public static void Toggle(string buttonId)
	{
		if (string.IsNullOrEmpty(buttonId)) return;
		if (Time.time - lastButtonPressTime < 0.25f)
		{
			return;
		}
		lastButtonPressTime = Time.time;
		PlayButtonClickSound(Mods.isRightHanded);
		List<ButtonInfo> currentButtons = MenuManager.Instance.CurrentButtons;
		if (currentButtons == null)
		{
			return;
		}
		int count = currentButtons.Count;
		int pageCount = (count + pageSize - 1) / pageSize;
		if (pageCount < 1)
		{
			pageCount = 1;
		}
		switch (buttonId)
		{
		case "NextPage":
			if (pageNumber < pageCount - 1)
			{
				pageNumber++;
			}
			else
			{
				pageNumber = 0;
			}
			if (toggleMenu)
			{
				RefreshMenu();
			}
			else
			{
				DestroyMenu();
				if (instance != (Object)null) instance.Draw();
			}
			return;
		case "PreviousPage":
			if (pageNumber > 0)
			{
				pageNumber--;
			}
			else
			{
				pageNumber = pageCount - 1;
			}
			if (toggleMenu)
			{
				RefreshMenu();
			}
			else
			{
				DestroyMenu();
				if (instance != (Object)null) instance.Draw();
			}
			return;
		case "DisconnectingButton":
			PhotonNetwork.Disconnect();
			return;
		}
		int buttonIndex = -1;
		for (int i = 0; i < currentButtons.Count; i++)
		{
			if (buttonId == currentButtons[i].id)
			{
				buttonIndex = i;
				break;
			}
		}
		if (buttonIndex < 0 || buttonIndex >= currentButtons.Count || !currentButtons[buttonIndex].enabled.HasValue)
		{
			return;
		}
		ButtonInfo buttonInfo = currentButtons[buttonIndex];
		if (buttonInfo.requiredGameMode != null && (buttonInfo.type == ButtonType.Action || buttonInfo.enabled != true))
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				NotifiLib.SendNotification("You are not master client!");
				return;
			}
			if (!Mods.IsInGameMode(buttonInfo.requiredGameMode))
			{
				NotifiLib.SendNotification("Not in " + buttonInfo.requiredGameMode.ToLower() + "!");
				return;
			}
		}
		if (buttonInfo.type == ButtonType.Action)
		{
			buttonInfo.method?.Invoke();
			return;
		}
		if (MenuManager.Instance.CurrentCategoryName == "Master Mods" && !PhotonNetwork.IsMasterClient)
		{
			NotifiLib.SendNotification("You are not master client!");
			return;
		}
		bool value = buttonInfo.enabled.Value;
		if (!value && buttonInfo.requiresLobby && !PhotonNetwork.InRoom)
		{
			NotifiLib.SendNotification("You can only play sounds inside a lobby");
			return;
		}
		buttonInfo.enabled = !value;
		Mods.InvalidateActiveButtonsCache();
		try
		{
			if (buttonInfo.enabled == true)
			{
				if (buttonInfo.enableMethod != null) buttonInfo.enableMethod();
				else buttonInfo.method?.Invoke();
			}
			else if (buttonInfo.disableMethod != null) buttonInfo.disableMethod();
		} catch { }
		if (buttonInfo.enabled == true && !string.IsNullOrEmpty(buttonInfo.toolTip) && buttonInfo.toolTip != "This button doesn't have a tooltip/tutorial")
		{
			NotifiLib.SendNotification(buttonInfo.buttonText + ": " + buttonInfo.toolTip, 2);
		}
		if (menu != (Object)null)
		{
			UpdateButtonVisual(buttonInfo.id, buttonInfo.buttonText, buttonInfo.enabled.Value);
		}
		Mods.Save();
	}

	internal static void UpdateButtonVisual(string buttonId, string buttonText, bool isEnabled)
	{
		if (string.IsNullOrEmpty(buttonId)) return;
		if (menu == (Object)null) return;
		foreach (Transform item in menu.transform)
		{
			Transform val2 = item;
			if (val2 == (Object)null) continue;
			BtnCollider component = ((Component)val2).GetComponent<BtnCollider>();
			if (component != (Object)null && component.buttonId == buttonId)
			{
				Renderer component2 = ((Component)val2).GetComponent<Renderer>();
				if (component2 == (Object)null) break;
				Color baseColor = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
				component2.material = MakeGradientMat(baseColor * 0.35f, baseColor);
				break;
			}
		}
		Color bc = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
		Color bt = bc * 0.35f;
		if (roundedRenderers.TryGetValue(buttonId, out var value))
		{
			foreach (Renderer item2 in value)
			{
				if (item2 != (Object)null)
				{
					item2.material = MakeGradientMat(bt, bc);
				}
			}
		}
		if (!(canvasObj != (Object)null))
		{
			return;
		}
		foreach (Transform item3 in canvasObj.transform)
		{
			Transform val4 = item3;
			Text component3 = ((Component)val4).GetComponent<Text>();
			if (component3 != (Object)null && component3.text == buttonText)
			{
				((Graphic)component3).color = (isEnabled ? EnableTextColor : DisableTextColor);
				break;
			}
		}
	}
}