using Chud.Backend;
using Chud.Rendering;
using Object = UnityEngine.Object;
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

        public static bool AnimationsEnabled = Defaults.AnimationsEnabled;

        public static int MenuLayout = Defaults.MenuLayout;

        public static bool ToggleMenu = Defaults.ToggleMenu;

        public static bool ShowFPS = Defaults.ShowFPS;

        public static bool ShowSessionTime = Defaults.ShowSessionTime;

        public static bool CustomBoardsEnabled = Defaults.CustomBoardsEnabled;

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

        internal static MenuRuntimeState State => _state ?? (_state = new MenuRuntimeState());

        private static MenuRuntimeState _state;

        internal static string BottomBarText => State.BottomBar;

        internal static bool AnimatorOwnsScale;

        internal static MenuFont Fonts => State.Fonts;

        internal static void SetBottomBar(string value)
        {
            State.BottomBar = value;
        }

        internal static int RealFps => State.RealFps;

        internal static void TickFpsCounter()
        {
            State.TickFpsCounter();
        }

        internal static string ComposeBottomBar()
        {
            string fpsPart = ShowFPS ? "FPS: " + State.RealFps : null;
            string sessionPart = null;
            if (ShowSessionTime)
            {
                sessionPart = State.SessionMinutes() + ":" + State.SessionSeconds().ToString("D2");
            }

            if (fpsPart != null && sessionPart != null) return fpsPart + " | " + sessionPart;
            if (fpsPart != null) return fpsPart;
            if (sessionPart != null) return sessionPart;
            return string.Empty;
        }

        internal static void ResetSessionClock()
        {
            State.ResetSessionClock();
        }

        internal static void InitFonts()
        {
            if (State.TryInitFonts(out MenuFont fonts))
            {
                MenuFont = fonts.Notification;
            }
        }

        internal static bool ConsumeButtonPress()
        {
            return State.ConsumeButtonPress();
        }

        internal static GameObject MenuAnchor => State.MenuAnchor;

        internal static bool MenuCameraAnchored => State.MenuCameraAnchored;

        internal static bool MenuAnchorIsRightHand => State.MenuAnchorIsRightHand;

        internal static Transform MenuFollowHand => State.MenuFollowHand;

        internal static void SetMenuAnchor(GameObject anchor, bool rightHand, Transform follow)
        {
            State.SetMenuAnchor(anchor, rightHand, follow);
        }

        internal static void ClearMenuAnchor()
        {
            State.ClearMenuAnchor();
        }

        internal static void ClearMenuFollow()
        {
            State.ClearMenuFollow();
        }

        internal static void SetMenuCameraAnchored(bool value)
        {
            State.SetMenuCameraAnchored(value);
        }

        internal static bool AdminInitialized
        {
            get => State.AdminInitialized;
            set => State.AdminInitialized = value;
        }

        internal static bool MouseWasPressed
        {
            get => State.MouseWasPressed;
            set => State.MouseWasPressed = value;
        }

        internal static Camera ThirdPersonCamera => State.ThirdPersonCamera;

        internal static void SetThirdPersonCamera(Camera camera)
        {
            State.SetThirdPersonCamera(camera);
        }

        internal static bool PrevToggleButton
        {
            get => State.PrevToggleButton;
            set => State.PrevToggleButton = value;
        }

        internal static bool MenuStickyOpen
        {
            get => State.MenuStickyOpen;
            set => State.MenuStickyOpen = value;
        }

        internal static int FrameCounter
        {
            get => State.FrameCounter;
            set => State.FrameCounter = value;
        }

        internal static Camera[] CameraScratch => State.CameraScratch;

        internal static void TickGradientAnimations(float time)
        {
            Materials?.TickGradients(time);
        }
    }
}