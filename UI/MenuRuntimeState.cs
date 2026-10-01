using UnityEngine;
namespace Chud.UI
{
    public sealed class MenuRuntimeState
    {
        private const float FpsSampleFrames = 30;

        private float _fpsAccumulator;
        private int _fpsFrameCount;
        private int _cachedFps;
        private int _frameCounter;

        private System.DateTime _sessionStart = System.DateTime.Now;

        private GameObject _menuAnchor;
        private bool _menuCameraAnchored;
        private bool _menuAnchorIsRightHand;
        private Transform _menuFollowHand;
        private readonly Camera[] _cameraScratch = new Camera[1];

        private Camera _thirdPersonCamera;

        private bool _mouseWasPressed;
        private float _lastButtonPressTime = -1f;
        private bool _prevToggleButton;
        private bool _menuStickyOpen;

        private bool _adminInitialized;

        private bool _fontsInitialized;
        private MenuFont _fonts;

        private string _bottomBar = string.Empty;

        public int RealFps => _cachedFps;

        public int FrameCounter
        {
            get => _frameCounter;
            set => _frameCounter = value;
        }

        public void TickFpsCounter()
        {
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;
            if (_fpsFrameCount < FpsSampleFrames)
            {
                return;
            }

            _cachedFps = _fpsAccumulator > 0f
                ? Mathf.RoundToInt((float)_fpsFrameCount / _fpsAccumulator)
                : 0;
            _fpsAccumulator = 0f;
            _fpsFrameCount = 0;
        }

        public void ResetSessionClock()
        {
            _sessionStart = System.DateTime.Now;
        }

        public int SessionMinutes() => (int)(System.DateTime.Now - _sessionStart).TotalMinutes;

        public int SessionSeconds() => (System.DateTime.Now - _sessionStart).Seconds;

        public GameObject MenuAnchor => _menuAnchor;

        public bool MenuCameraAnchored => _menuCameraAnchored;

        public bool MenuAnchorIsRightHand => _menuAnchorIsRightHand;

        public Transform MenuFollowHand => _menuFollowHand;

        public Camera[] CameraScratch => _cameraScratch;

        public void SetMenuAnchor(GameObject anchor, bool rightHand, Transform follow)
        {
            _menuAnchor = anchor;
            _menuAnchorIsRightHand = rightHand;
            _menuFollowHand = follow;
        }

        public void ClearMenuAnchor()
        {
            _menuAnchor = null;
            _menuFollowHand = null;
        }

        public void ClearMenuFollow()
        {
            _menuFollowHand = null;
        }

        public void SetMenuCameraAnchored(bool value)
        {
            _menuCameraAnchored = value;
        }

        public Camera ThirdPersonCamera => _thirdPersonCamera;

        public void SetThirdPersonCamera(Camera camera)
        {
            _thirdPersonCamera = camera;
        }

        public bool MouseWasPressed
        {
            get => _mouseWasPressed;
            set => _mouseWasPressed = value;
        }

        public bool PrevToggleButton
        {
            get => _prevToggleButton;
            set => _prevToggleButton = value;
        }

        public bool MenuStickyOpen
        {
            get => _menuStickyOpen;
            set => _menuStickyOpen = value;
        }

        public bool ConsumeButtonPress()
        {
            if (Time.time - _lastButtonPressTime < 0.25f)
            {
                return false;
            }

            _lastButtonPressTime = Time.time;
            return true;
        }

        public bool AdminInitialized
        {
            get => _adminInitialized;
            set => _adminInitialized = value;
        }

        public MenuFont Fonts
        {
            get
            {
                if (!_fontsInitialized)
                {
                    _fontsInitialized = true;
                    _fonts = new MenuFont();
                }

                return _fonts;
            }
        }

        public bool TryInitFonts(out MenuFont fonts)
        {
            if (_fontsInitialized)
            {
                fonts = _fonts;
                return false;
            }

            _fontsInitialized = true;
            _fonts = new MenuFont();
            fonts = _fonts;
            return true;
        }

        public string BottomBar
        {
            get => _bottomBar;
            set => _bottomBar = value ?? string.Empty;
        }
    }
}