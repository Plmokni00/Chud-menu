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
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == "Enabled Mods");
		if (menuCategory == null)
		{
			return;
		}
		menuCategory.Buttons.Clear();
		menuCategory.Buttons.Add(new ButtonInfo
		{
			buttonText = "Exit Enabled Mods",
			method = delegate
			{
				MenuManager.ToggleCategory("Enabled Mods");
			},
			enabled = false,
			type = ButtonType.Action,
			toolTip = "Go to Main"
		});
		foreach (MenuCategory category in MenuManager.Categories)
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
				string capturedText = button.buttonText;
				menuCategory.Buttons.Add(new ButtonInfo
				{
					buttonText = capturedText,
					method = delegate
					{
						Mods.FindAndToggleButton(capturedText);
						if (toggleMenu)
						{
							RefreshMenu();
						}
						else
						{
							DestroyMenu();
							if ((Object)(object)instance != (Object)null)
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

	public static void Toggle(string relatedText)
	{
		if (string.IsNullOrEmpty(relatedText)) return;
		if (Time.time - lastButtonPressTime < 0.25f)
		{
			return;
		}
		lastButtonPressTime = Time.time;
		PlayButtonClickSound(Mods.isRightHanded);
		List<ButtonInfo> currentButtons = MenuManager.CurrentButtons;
		if (currentButtons == null)
		{
			return;
		}
		int count = currentButtons.Count;
		int num = (count + pageSize - 1) / pageSize;
		if (num < 1)
		{
			num = 1;
		}
		switch (relatedText)
		{
		case "NextPage":
			if (pageNumber < num - 1)
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
				if ((Object)(object)instance != (Object)null) instance.Draw();
			}
			return;
		case "PreviousPage":
			if (pageNumber > 0)
			{
				pageNumber--;
			}
			else
			{
				pageNumber = num - 1;
			}
			if (toggleMenu)
			{
				RefreshMenu();
			}
			else
			{
				DestroyMenu();
				if ((Object)(object)instance != (Object)null) instance.Draw();
			}
			return;
		case "DisconnectingButton":
			PhotonNetwork.Disconnect();
			return;
		}
		int num2 = -1;
		for (int i = 0; i < currentButtons.Count; i++)
		{
			if (relatedText == currentButtons[i].buttonText)
			{
				num2 = i;
				break;
			}
		}
		if (num2 < 0 || num2 >= currentButtons.Count || !currentButtons[num2].enabled.HasValue)
		{
			return;
		}
		ButtonInfo buttonInfo = currentButtons[num2];
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
		if (MenuManager.CurrentCategoryName == "Master Mods" && !PhotonNetwork.IsMasterClient)
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
		if ((Object)(object)menu != (Object)null)
		{
			UpdateButtonVisual(relatedText, buttonInfo.enabled.Value);
		}
		Mods.Save();
	}

	internal static void UpdateButtonVisual(string buttonText, bool isEnabled)
	{
		if (string.IsNullOrEmpty(buttonText)) return;
		if ((Object)(object)menu == (Object)null) return;
		foreach (Transform item in menu.transform)
		{
			Transform val2 = item;
			if ((Object)(object)val2 == (Object)null) continue;
			BtnCollider component = ((Component)val2).GetComponent<BtnCollider>();
			if ((Object)(object)component != (Object)null && component.relatedText == buttonText)
			{
				Renderer component2 = ((Component)val2).GetComponent<Renderer>();
				if ((Object)(object)component2 == (Object)null) break;
				Color baseColor = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
				component2.material = MakeGradientMat(baseColor * 0.35f, baseColor);
				break;
			}
		}
		Color bc = isEnabled ? ButtonColorEnabled : ButtonColorDisable;
		Color bt = bc * 0.35f;
		if (roundedRenderers.TryGetValue(buttonText, out var value))
		{
			foreach (Renderer item2 in value)
			{
				if ((Object)(object)item2 != (Object)null)
				{
					item2.material = MakeGradientMat(bt, bc);
				}
			}
		}
		if (!((Object)(object)canvasObj != (Object)null))
		{
			return;
		}
		foreach (Transform item3 in canvasObj.transform)
		{
			Transform val4 = item3;
			Text component3 = ((Component)val4).GetComponent<Text>();
			if ((Object)(object)component3 != (Object)null && component3.text == buttonText)
			{
				((Graphic)component3).color = (isEnabled ? EnableTextColor : DisableTextColor);
				break;
			}
		}
	}
}