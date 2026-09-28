using Chud.Backend;
using Chud.Diagnostics;
using Chud.Menu;
using Chud.Rendering;
using Chud.Runtime;
using GTAG_NotificationLib;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using UnityEngine;
using Console = Chud.Backend.Console;
namespace Chud.UI
{
    internal partial class WristMenu : MonoBehaviour
    {
        private static NormalLayout _normalLayout;
        private static ModernLayout _modernLayout;

        private static bool IsNormal => MenuLayout == 0;

        public static void Draw()
        {
            try
            {
                ReleaseMenuObjects();

                Materials = new MenuMaterialSet();

                if (IsNormal)
                {
                    if (_normalLayout == null)
                    {
                        _normalLayout = new NormalLayout();
                    }

                    _normalLayout.Draw();
                }
                else
                {
                    if (_modernLayout == null)
                    {
                        _modernLayout = new ModernLayout();
                    }

                    _modernLayout.Draw();
                }
            }
            catch (Exception ex)
            {
                Log.Error("menu draw failed", ex);
            }
        }

        private static MenuAnimPlan BuildAnimPlan()
        {
            return IsNormal ? _normalLayout?.BuildAnimPlan() : _modernLayout?.BuildAnimPlan();
        }

        public static IEnumerator OpenAnimation()
        {
            MenuAnimPlan plan = BuildAnimPlan();
            if (plan == null)
            {
                yield break;
            }

            yield return MenuAnimator.Open(plan);
        }

        public static IEnumerator CloseAnimation()
        {
            yield return MenuAnimator.Close();
        }

        public static void DestroyMenu()
        {
            ReleaseMenuObjects();
            DestroyReference();
            DestroyAnchor();
            Close = false;
        }

        public static void RefreshMenu()
        {
            ReleaseMenuObjects();
            DestroyReference();
            Close = false;

            if (Instance != (Object)null)
            {
                Instance.DrawInternal();
            }

            RestoreMenuAnchor();
        }

        public static void Reopen()
        {
            if (ToggleMenu)
            {
                RefreshMenu();
                return;
            }

            DestroyMenu();
            Instance?.DrawInternal();
        }

        public static void NavigateTo(string categoryName)
        {
            if (!MenuRegistry.Instance.NavigateTo(categoryName))
            {
                return;
            }

            PageNumber = 0;
            Reopen();
        }

        internal void DrawInternal()
        {
            Draw();
        }

        private static void ReleaseMenuObjects()
        {
            _normalLayout?.Teardown();
            _modernLayout?.Teardown();

            Materials?.Dispose();
            Materials = null;

            if (Menu != (Object)null)
            {
                Object.Destroy(Menu);
            }

            Menu = null;
            MenuObj = null;
            CanvasObj = null;
            FpsText = null;
        }

        private static void DestroyReference()
        {
            if (Reference != (Object)null)
            {
                Object.Destroy(Reference);
            }

            Reference = null;
        }

        private static void DestroyAnchor()
        {
            if (MenuAnchor != (Object)null)
            {
                Object.Destroy(MenuAnchor);
            }

            ClearMenuAnchor();
        }

        internal static void SetLayerRecursive()
        {
            if (Menu == (Object)null)
            {
                return;
            }

            Transform[] all = Menu.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                all[i].gameObject.layer = 2;
            }

            Menu.layer = 2;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            try
            {
                Console.LoadConsole();
                ButtonCatalog.RegisterAll();
                InitFonts();
                ResetSessionClock();
                StartCoroutine(Audio.LoadButtonClickSounds());
                Mods.PreloadMinosSounds();
                DrawInternal();
                Mods.Load();
                StartCoroutine(ShowWelcome());
            }
            catch (Exception ex)
            {
                Log.Error("menu startup failed", ex);
            }
        }

        private static IEnumerator ShowWelcome()
        {
            yield return new WaitForSeconds(2f);
            NotifiLib.SendNotification("Thanks for choosing Chud Menu please enjoy :3", 2);
        }
    }
}