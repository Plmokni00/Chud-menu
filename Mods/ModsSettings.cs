using System.Collections.Generic;
namespace Chud.Backend
{
    public static class Defaults
    {
        public const float FlySpeed = 8f;
        public const int SpeedboostCycle = 1;
        public const int PullPowerInt = 2;
        public const float WasdFlyMouseSense = 1.25f;
        public const bool IsRightHanded = false;
        public const int MenuColorIndex = 0;
        public const int PaletteVersion = 1;
        public const int NotificationTimeIndex = 1;
        public const float Jspeed = 8f;
        public const float Jmulti = 1.5f;
        public const int TagAuraRangeIndex = 4;
        public const float AdminScale = 1f;
        public const bool AnimationsEnabled = true;
        public const bool ToggleMenu = false;
        public const bool ShowFPS = true;
        public const bool ShowSessionTime = true;
        public const bool CustomBoardsEnabled = true;
        public const bool BlockJmanSounds = false;
        public const bool AntiGuardianGrab = false;
        public const bool AntiBlockCrash = false;
        public const bool SeeAntiCheatReports = false;
        public const bool AntiReportEnabled = false;
        public const int AntiReportRangeIndex = 3;
        public const int WaterSplashSpeedIndex = 4;
        public const int ControllerPredIndex = 0;
        public const int FpsSpoofValue = 60;
        public const int ButtonClickIndex = 0;
        public const int MenuLayout = 0;
        public const bool ConsoleAllowKickSelf = false;
        public const bool ConsoleAllowTpSelf = true;
        public const bool ConsoleDisableFlingSelf = false;
        public const bool ConsoleLaserEnabled = false;
        public const bool ConsoleAutoDetectConsoleUsers = false;
        public const bool ConsoleLogging = false;
        public const bool ConsoleFullAutoPistol = false;

        public static readonly string[] EnabledMods =
        {
            "Menu Animations",
            "Show FPS",
            "Show Session Time",
            "Toggle Notifications",
            "Anti Report"
        };

        public static bool IsEnabledByDefault(string name)
        {
            for (int i = 0; i < EnabledMods.Length; i++)
            {
                if (EnabledMods[i] == name)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class ModsSettings
    {
        public float FlySpeed = Defaults.FlySpeed;
        public int SpeedBoostCycle = Defaults.SpeedboostCycle;
        public int PullPowerInt = Defaults.PullPowerInt;
        public float WasdFlyMouseSense = Defaults.WasdFlyMouseSense;
        public bool IsRightHanded = Defaults.IsRightHanded;
        public int MenuColorIndex = Defaults.MenuColorIndex;
        public float JSpeed = Defaults.Jspeed;
        public float JMulti = Defaults.Jmulti;
        public float TagAuraRange = 1.5f;
        public int TagAuraRangeIndex = Defaults.TagAuraRangeIndex;
        public int WaterSplashSpeedIndex = Defaults.WaterSplashSpeedIndex;
        public float ControllerPred = 0.0125f;
        public int ControllerPredIndex = Defaults.ControllerPredIndex;

        public bool FpsSpoofActive;
        public int FpsSpoofValue = Defaults.FpsSpoofValue;
        public bool FpsSpoofRandomActive;
        public int FpsSpoofDisplay = 60;
        public int FpsSpoofReference;
        public float FpsSpoofReferenceDecay;
        public float FpsSpoofRandomTimer;
        public int FpsSpoofRandomValue = 60;

        public bool AntiReportEnabled = Defaults.AntiReportEnabled;
        public float AntiReportRange = 0.35f;
        public int AntiReportRangeIndex = Defaults.AntiReportRangeIndex;
        public bool SeeAntiCheatReports;
        public Dictionary<string, int> AntiCheatReportCounts = new Dictionary<string, int>();

        public bool BlockJmanSoundsEnabled;
        public bool AntiGuardianGrab;
        public bool AntiBlockCrash;

        public bool RPlat;
        public bool LPlat;

        public int NotificationTimeIndex = Defaults.NotificationTimeIndex;
    }

    public static class Ranges
    {
        public static readonly float[] TagAuraRanges =
        {
            0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f
        };

        public static readonly float[] AntiReportRanges =
        {
            0.25f, 0.35f, 0.5f, 0.7f, 1f, 1.25f, 1.5f, 2f
        };
    }
}
