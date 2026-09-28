using Chud.Rendering;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.UI
{
    internal partial class WristMenu : MonoBehaviour
    {
        public static string MenuTitle = "Chud Menu";

        public static string FolderName = "Chud Menu";

        public static string[] CustomBoardTexts =
        {
            "CHUD MENU USERS, LOOK HERE",
            "CHUD MENU",
            "Monkeys can climb. Crickets can leap. Horses can race. Owls can seek. Cheetahs can run. Eagles can fly. People can try. But that's about it.",
            "if u get banned with this, its on u, not me"
        };

        public static bool AnimationsEnabled = false;

        public static int MenuLayout = 0;

        public static bool ToggleMenu = false;

        public static bool ShowFPS = false;

        public static bool ShowSessionTime = false;

        public static bool CustomBoardsEnabled = true;

        public static bool CustomBoardsApplied = false;

        public static Color NormalColor = new Color(0.25f, 0.25f, 0.25f);

        public static Color ButtonColorDisable = new Color(0.4f, 0.4f, 0.4f);

        public static Color ButtonColorEnabled = new Color(0.7f, 0.7f, 0.7f);

        public static Color EnableTextColor = Color.white;

        public static Color DisableTextColor = new Color(0.85f, 0.85f, 0.85f);

        public static Color MenuTitleColor = Color.white;

        public static Color ToolTipColor = new Color(0.88f, 0.88f, 0.88f);

        public static Color DisconnectButtonColor = new Color(0.7f, 0f, 0f);

        public static Color DisconnectTextColor = Color.white;

        public static Color NextPrevButtonColor = new Color(0.22f, 0.22f, 0.22f);

        public static Color NextPrevTextColor = Color.white;

        public static Vector3 PointerScale = new Vector3(0.015f, 0.015f, 0.015f);

        public static Vector3 PointerPos = new Vector3(0f, -0.1f, 0f);

        public static bool GripDownR;

        public static bool TriggerDownR;

        public static bool BButtonDown;

        public static bool XButtonDown;

        public static bool YButtonDown;

        public static bool GripDownL;

        public static bool TriggerDownL;

        public static Vector2 Joy = Vector2.zero;

        public static Vector2 JoyL = Vector2.zero;

        public static bool LeftTriggerLocked;

        public static bool RightTriggerLocked;

        public static int PageSize = 7;

        public static int ClickCooldown = 10;

        public static int PageNumber = 0;

        public static bool Close = false;

        public static Font MenuFont;

        public static AudioClip CustomButtonClick;

        public static GameObject Menu;

        public static GameObject MenuObj;

        public static GameObject CanvasObj;

        public static GameObject Reference;

        public static Text FpsText;

        public static WristMenu Instance;

        public static bool HasInstance => Instance != (Object)null;

        internal static MenuMaterialSet Materials;

        internal static string BottomBarText => _bottomBar;

        private static string _bottomBar = string.Empty;

        private static float _lastButtonPressTime = -1f;

        private static GameObject _menuAnchor;

        private static bool _menuCameraAnchored;

        private static bool _menuAnchorIsRightHand;

        private static Transform _menuFollowHand;

        private static readonly Camera[] _cameraScratch = new Camera[1];

        private static DateTime _sessionStartTime = DateTime.Now;

        private static float _fpsAccumulator;

        private static int _fpsFrameCount;

        private static int _cachedFPS;

        private static int _frameCounter;

        private static bool _adminInitialized;

        private static bool _mouseWasPressed;

        private static Camera _tpc;

        private static bool _prevToggleButton;

        private static bool _menuStickyOpen;

        private static bool _fontInitialized;

        internal static bool AnimatorOwnsScale;

        internal static MenuFont Fonts;

        internal static void SetBottomBar(string value)
        {
            _bottomBar = value ?? string.Empty;
        }

        internal static void TickFpsCounter()
        {
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;
            if (_fpsFrameCount < 30)
            {
                return;
            }

            _cachedFPS = _fpsAccumulator > 0f ? Mathf.RoundToInt((float)_fpsFrameCount / _fpsAccumulator) : 0;
            _fpsAccumulator = 0f;
            _fpsFrameCount = 0;
        }

        internal static string ComposeBottomBar()
        {
            string fpsPart = ShowFPS ? "FPS: " + _cachedFPS : null;
            string sessionPart = null;
            if (ShowSessionTime)
            {
                TimeSpan span = DateTime.Now - _sessionStartTime;
                sessionPart = (int)span.TotalMinutes + ":" + span.Seconds.ToString("D2");
            }

            if (fpsPart != null && sessionPart != null) return fpsPart + " | " + sessionPart;
            if (fpsPart != null) return fpsPart;
            if (sessionPart != null) return sessionPart;
            return string.Empty;
        }

        internal static void ResetSessionClock()
        {
            _sessionStartTime = DateTime.Now;
        }

        internal static void InitFonts()
        {
            if (_fontInitialized)
            {
                return;
            }

            _fontInitialized = true;
            Fonts = new MenuFont();
            MenuFont = Fonts.Notification;
        }

        internal static void MarkButtonPress()
        {
            _lastButtonPressTime = Time.time;
        }

        internal static bool ConsumeButtonPress()
        {
            if (Time.time - _lastButtonPressTime < 0.25f)
            {
                return false;
            }

            _lastButtonPressTime = Time.time;
            return true;
        }

        internal static GameObject MenuAnchor => _menuAnchor;

        internal static bool MenuCameraAnchored => _menuCameraAnchored;

        internal static bool MenuAnchorIsRightHand => _menuAnchorIsRightHand;

        internal static Transform MenuFollowHand => _menuFollowHand;

        internal static void SetMenuAnchor(GameObject anchor, bool rightHand, Transform follow)
        {
            _menuAnchor = anchor;
            _menuAnchorIsRightHand = rightHand;
            _menuFollowHand = follow;
        }

        internal static void ClearMenuAnchor()
        {
            _menuAnchor = null;
            _menuFollowHand = null;
        }

        internal static void ClearMenuFollow()
        {
            _menuFollowHand = null;
        }

        internal static void SetMenuCameraAnchored(bool value)
        {
            _menuCameraAnchored = value;
        }

        internal static bool AdminInitialized
        {
            get => _adminInitialized;
            set => _adminInitialized = value;
        }

        internal static bool MouseWasPressed
        {
            get => _mouseWasPressed;
            set => _mouseWasPressed = value;
        }

        internal static Camera ThirdPersonCamera => _tpc;

        internal static void SetThirdPersonCamera(Camera camera)
        {
            _tpc = camera;
        }

        internal static bool PrevToggleButton
        {
            get => _prevToggleButton;
            set => _prevToggleButton = value;
        }

        internal static bool MenuStickyOpen
        {
            get => _menuStickyOpen;
            set => _menuStickyOpen = value;
        }

        internal static int FrameCounter
        {
            get => _frameCounter;
            set => _frameCounter = value;
        }

        internal static Camera[] CameraScratch => _cameraScratch;

        internal static void TickGradientAnimations(float time)
        {
            Materials?.TickGradients(time);
        }
    }
}