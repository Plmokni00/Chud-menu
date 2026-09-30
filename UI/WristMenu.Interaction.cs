using Chud.Backend;
using Chud.Diagnostics;
using Chud.Menu;
using Chud.Runtime;
using GTAG_NotificationLib;
using Object = UnityEngine.Object;
using Photon.Pun;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Chud.UI
{
    internal partial class WristMenu
    {
        public static void Press(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
            {
                return;
            }

            if (!ConsumeButtonPress())
            {
                return;
            }

            Audio.PlayButtonClick(Mods.IsRightHanded);
            Dispatch(buttonId);
        }

        private static void Dispatch(string buttonId)
        {
            IReadOnlyList<ButtonInfo> buttons = MenuRegistry.Instance.CurrentButtons;
            if (buttons == null)
            {
                return;
            }

            int pageCount = PageSlicing.PageCount(buttons.Count, PageSlicing.DefaultPageSize);

            if (buttonId == ReservedButtonIds.NextPage)
            {
                PageNumber = PageNumber < pageCount - 1 ? PageNumber + 1 : 0;
                Reopen();
                return;
            }

            if (buttonId == ReservedButtonIds.PreviousPage)
            {
                PageNumber = PageNumber > 0 ? PageNumber - 1 : pageCount - 1;
                Reopen();
                return;
            }

            if (buttonId == ReservedButtonIds.Disconnect)
            {
                PhotonNetwork.Disconnect();
                return;
            }

            ButtonInfo target = null;
            for (int i = 0; i < buttons.Count; i++)
            {
                ButtonInfo candidate = buttons[i];
                if (candidate != null && buttonId == candidate.id)
                {
                    target = candidate;
                    break;
                }
            }

            if (target == null || !target.enabled.HasValue)
            {
                return;
            }

            if (!CheckAvailability(target))
            {
                return;
            }

            if (target.type == ButtonType.Action)
            {
                InvokeAction(target, "method", target.method);
                return;
            }

            if (MenuRegistry.Instance.CurrentCategoryName == "Master Mods" && !PhotonNetwork.IsMasterClient)
            {
                NotifiLib.SendNotification("You are not master client!");
                return;
            }

            bool value = target.enabled.Value;
            if (!value && target.requiresLobby && !PhotonNetwork.InRoom)
            {
                NotifiLib.SendNotification("You can only play sounds inside a lobby");
                return;
            }

            target.enabled = !value;
            Mods.InvalidateActiveButtonsCache();

            if (target.enabled == true)
            {
                InvokeAction(target, "enableMethod", target.enableMethod ?? target.method);
            }
            else
            {
                InvokeAction(target, "disableMethod", target.disableMethod);
            }

            if (target.enabled == true && !string.IsNullOrEmpty(target.toolTip) && target.toolTip != Button.NoTooltip)
            {
                NotifiLib.SendNotification(target.buttonText + ": " + target.toolTip, 2);
            }

            if (Menu != (Object)null)
            {
                UpdateButtonVisual(target.id, target.buttonText, target.enabled.Value);
            }

            Mods.Save();
        }

        private static bool CheckAvailability(ButtonInfo button)
        {
            if (button.requiredGameMode == null ||
                (button.type != ButtonType.Action && button.enabled == true))
            {
                return true;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                NotifiLib.SendNotification("You are not master client!");
                return false;
            }

            if (Mods.IsInGameMode(button.requiredGameMode))
            {
                return true;
            }

            NotifiLib.SendNotification("Not in " + button.requiredGameMode.ToLower() + "!");
            return false;
        }

        private static void InvokeAction(ButtonInfo button, string slot, Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error("button '" + button.id + "' ('" + button.buttonText + "') failed in " + slot, ex);
            }
        }

        internal static void UpdateButtonVisual(string buttonId, string buttonText, bool isEnabled)
        {
            if (string.IsNullOrEmpty(buttonId) || Menu == (Object)null)
            {
                return;
            }

            if (IsNormal)
            {
                _normalLayout?.UpdateVisual(buttonId, isEnabled);
                return;
            }

            _modernLayout?.UpdateVisual(buttonId, buttonText, isEnabled);
        }

        public static void RebuildEnabledMods()
        {
            MenuRegistry registry = MenuRegistry.Instance;
            MenuCategory page = registry.Find(MenuRegistry.EnabledModsCategory);
            if (page == null)
            {
                return;
            }

            page.Buttons.Clear();
            page.Buttons.Add(Button.Action("enabled_exit", "Exit Enabled Mods", "Go to Main",
                () => NavigateTo(MenuRegistry.EnabledModsCategory)));

            var mirrors = new List<ButtonInfo>();

            for (int c = 0; c < registry.Categories.Count; c++)
            {
                MenuCategory category = registry.Categories[c];
                if (IsDerivedCategory(category.Name))
                {
                    continue;
                }

                for (int b = 0; b < category.Buttons.Count; b++)
                {
                    ButtonInfo button = category.Buttons[b];
                    if (button.enabled != true || button.disableMethod == null)
                    {
                        continue;
                    }

                    string id = button.id;
                    mirrors.Add(Button.Action(id, button.buttonText, button.toolTip ?? string.Empty,
                        () =>
                        {
                            FindAndToggleButton(id);
                            Reopen();
                        }));
                }
            }

            page.Buttons.AddRange(mirrors);
        }

        private static bool IsDerivedCategory(string name)
        {
            return name == MenuRegistry.MainCategory
                   || name == MenuRegistry.EnabledModsCategory
                   || name == MenuRegistry.ConsoleModsCategory
                   || name == MenuRegistry.ConsoleSettingsCategory;
        }

        public static void FindAndToggleButton(string buttonId)
        {
            ButtonInfo button = FindToggleable(buttonId);
            if (button == null)
            {
                return;
            }

            button.enabled = button.enabled != true;
            Mods.InvalidateActiveButtonsCache();

            if (button.enabled == true)
            {
                InvokeAction(button, "enableMethod", button.enableMethod ?? button.method);
            }
            else
            {
                InvokeAction(button, "disableMethod", button.disableMethod);
            }

            UpdateButtonVisual(button.id, button.buttonText, button.enabled.Value);
            Mods.Save();
        }

        private static ButtonInfo FindToggleable(string buttonId)
        {
            IReadOnlyList<MenuCategory> categories = MenuRegistry.Instance.Categories;
            for (int c = 0; c < categories.Count; c++)
            {
                List<ButtonInfo> buttons = categories[c].Buttons;
                for (int b = 0; b < buttons.Count; b++)
                {
                    ButtonInfo button = buttons[b];
                    if (button != null &&
                        button.id == buttonId &&
                        button.enabled.HasValue &&
                        button.type != ButtonType.Action)
                    {
                        return button;
                    }
                }
            }

            return null;
        }
    }
}