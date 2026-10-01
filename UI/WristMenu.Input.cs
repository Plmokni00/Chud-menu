using Chud.Backend;
using Chud.Diagnostics;
using Chud.Menu;
using Chud.Runtime;
using GTAG_NotificationLib;
using Object = UnityEngine.Object;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
namespace Chud.UI
{
    internal partial class WristMenu : MonoBehaviour
    {
        private const int PollInterval = 15;
        private const int BoardInterval = 60;
        private const float RayDistance = 512f;
        private const int PointerLayerMask = 1 << 2;
        private const string ConsoleModsName = "Console Mods";

        private void Update()
        {
            try
            {
                Tick();
            }
            catch (Exception ex)
            {
                Log.Error(Frame.WristMenuUpdate, ex);
            }
        }

        private void LateUpdate()
        {
            FollowAnchor();
        }

        private static void Tick()
        {
            ControllerInputPoller poller = GameContext.Poller;
            if (poller == null)
            {
                return;
            }

            PollInput(poller);

            bool qKeyDown = !GameContext.IsVrActive
                            && Keyboard.current != null
                            && ((ButtonControl)Keyboard.current.qKey).isPressed;

            HandleMenuFollow(qKeyDown);
            Mods.UpdateActiveMods();

            int frame = FrameCounter + 1;
            FrameCounter = frame;

            if (frame % PollInterval == 0)
            {
                UpdateMasterClientStatus();
                UpdateAdminStatus();
            }

            TickFpsCounter();
            SetBottomBar(ComposeBottomBar());
            UpdateStatusText();

            if (frame % BoardInterval == 0)
            {
                TickBoards();
            }
        }

        private static void PollInput(ControllerInputPoller poller)
        {
            GripDownL = poller.leftGrab;
            GripDownR = poller.rightGrab;
            TriggerDownL = poller.leftControllerIndexFloat == 1f;
            TriggerDownR = poller.rightControllerIndexFloat == 1f;
            BButtonDown = poller.rightControllerSecondaryButton;
            XButtonDown = poller.leftControllerPrimaryButton;
            YButtonDown = poller.leftControllerSecondaryButton;
            Joy = poller.rightControllerPrimary2DAxis;
            JoyL = poller.leftControllerPrimary2DAxis;
        }

        private static void UpdateStatusText()
        {
            if (IsNormal)
            {
                if (FpsText != (Object)null)
                {
                    FpsText.text = BottomBarText;
                }

                return;
            }

            _modernLayout?.Text?.SetText(ReservedNames.Status, BottomBarText);
        }

        private static void TickBoards()
        {
            if (!Directory.Exists(FolderName))
            {
                try
                {
                    Directory.CreateDirectory(FolderName);
                }
                catch (Exception ex)
                {
                    Log.Error("could not create the menu folder '" + FolderName + "'", ex);
                }
            }

            if (CustomBoardsEnabled)
            {
                if (!CustomBoardsApplied)
                {
                    Instance?.ApplyCustomBoardText();
                    CustomBoardsApplied = true;
                }
            }
            else if (CustomBoardsApplied)
            {
                CustomBoardsApplied = false;
            }
        }

        private static void HandleMenuFollow(bool qKeyDown)
        {
            bool held = (YButtonDown && !Mods.IsRightHanded) || (BButtonDown && Mods.IsRightHanded) || qKeyDown;

            if (ToggleMenu)
            {
                bool justPressed = held && !PrevToggleButton;
                PrevToggleButton = held;

                if (justPressed)
                {
                    if (Menu == (Object)null && !Close)
                    {
                        MenuStickyOpen = true;
                    }
                    else if (Menu != (Object)null && !Close)
                    {
                        MenuStickyOpen = false;
                        DestroyReference();
                        Instance?.StartCoroutine(CloseAnimation());
                        return;
                    }
                }

                held = MenuStickyOpen;
            }
            else
            {
                PrevToggleButton = held;
                MenuStickyOpen = false;
            }

            if (held)
            {
                if (Menu == (Object)null || !ToggleMenu)
                {
                    SetMenuCameraAnchored(qKeyDown);
                }

                if (Menu == (Object)null)
                {
                    Instance?.DrawInternal();
                    if (Menu != (Object)null)
                    {
                        Menu.transform.localScale = Vector3.one * 0.001f;
                    }

                    Instance?.StartCoroutine(OpenAnimation());
                }

                if (qKeyDown)
                {
                    OpenCameraAnchored();
                }
                else if (YButtonDown && !Mods.IsRightHanded)
                {
                    AttachToNewHand(false);
                }
                else if (BButtonDown && Mods.IsRightHanded)
                {
                    AttachToNewHand(true);
                }
            }
            else if (Menu != (Object)null && !Close)
            {
                DestroyReference();
                Instance?.StartCoroutine(CloseAnimation());
            }

            if (ToggleMenu && MenuStickyOpen && !Close)
            {
                if (Menu == (Object)null)
                {
                    Instance?.DrawInternal();
                }

                RestoreMenuAnchor();
                HandleMouseMenuClick();
            }
        }

        private static void OpenCameraAnchored()
        {
            ClearMenuFollow();
            if (ThirdPersonCamera == null)
            {
                SetThirdPersonCamera(ObjectFinder.FindShoulderCamera());
            }

            RestoreMenuAnchor();
            HandleMouseMenuClick();
        }

        private static void HandleMouseMenuClick()
        {
            if (Menu == (Object)null || Close || !MenuCameraAnchored)
            {
                return;
            }

            Camera camera = ThirdPersonCamera;
            Mouse mouse = Mouse.current;
            if (camera == null || mouse == null || Reference == (Object)null)
            {
                return;
            }

            bool pressed = mouse.leftButton.isPressed;

            if (pressed && !MouseWasPressed)
            {
                Ray ray = camera.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, RayDistance, PointerLayerMask, QueryTriggerInteraction.Collide) &&
                    hit.transform != Reference.transform)
                {
                    var button = hit.transform.gameObject.GetComponent<MenuButton>();
                    if (button != null && !string.IsNullOrEmpty(button.ButtonId))
                    {
                        Press(button.ButtonId);
                    }
                }
            }

            MouseWasPressed = pressed;
        }

        private static void UpdateMasterClientStatus()
        {
            MenuCategory category = MenuRegistry.Instance.Find(MenuRegistry.MasterModsCategory);
            if (category == null || category.Buttons.Count <= 1)
            {
                return;
            }

            bool isMaster = PhotonNetwork.IsMasterClient;
            category.Buttons[1].buttonText = isMaster ? "You are master client" : "Not master client";
            category.Buttons[1].toolTip = isMaster ? "You are the master client" : "You are not the master client";

            if (isMaster)
            {
                return;
            }

            for (int i = 2; i < category.Buttons.Count; i++)
            {
                ButtonInfo button = category.Buttons[i];
                if (button.enabled != true)
                {
                    continue;
                }

                if (button.disableMethod != null)
                {
                    InvokeAction(button, "disableMethod", button.disableMethod);
                }

                button.enabled = false;
                Mods.InvalidateActiveButtonsCache();
            }
        }

        private static void UpdateAdminStatus()
        {
            bool isAdmin = ServerData.IsLocalPlayerAdmin();
            MenuRegistry registry = MenuRegistry.Instance;

            MenuCategory consoleMods = registry.Find(MenuRegistry.ConsoleModsCategory);
            MenuCategory main = registry.Find(MenuRegistry.MainCategory);
            bool hasConsoleButton = main != null && main.Find(ConsoleModsName) != null;

            if (isAdmin && consoleMods != null && !hasConsoleButton)
            {
                GrantAdminAccess(main);
                return;
            }

            if (!isAdmin && hasConsoleButton)
            {
                RevokeAdminAccess(main);
            }
        }

        private static void GrantAdminAccess(MenuCategory main)
        {
            if (main == null)
            {
                return;
            }

            if (!AdminInitialized)
            {
                string name = ServerData.LocalAdminName();
                string prefix = ServerData.IsSuperAdmin(name) ? "super admin " : string.Empty;
                NotifiLib.SendNotification("Welcome " + prefix + name, 2);
                AdminInitialized = true;
            }

            if (main.Find(ConsoleModsName) != null)
            {
                return;
            }

            main.Buttons.Add(Button.Action(ConsoleModsName, "Go to Console Mods!",
                () => NavigateTo(MenuRegistry.ConsoleModsCategory)));

            Reopen();
        }

        private static void RevokeAdminAccess(MenuCategory main)
        {
            main?.RemoveWhere(b => b != null && b.buttonText == ConsoleModsName);

            MenuRegistry registry = MenuRegistry.Instance;
            if (registry.CurrentCategoryName == MenuRegistry.ConsoleModsCategory ||
                registry.CurrentCategoryName == MenuRegistry.ConsoleSettingsCategory)
            {
                registry.CurrentCategoryName = MenuRegistry.MainCategory;
            }

            PageNumber = 0;

            bool wasOpen = Menu != (Object)null;
            if (ToggleMenu)
            {
                if (wasOpen)
                {
                    RefreshMenu();
                }

                return;
            }

            DestroyMenu();
            if (wasOpen)
            {
                Instance?.DrawInternal();
            }
        }
    }
}