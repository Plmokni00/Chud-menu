using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GTAG_NotificationLib;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Pointer = UnityEngine.InputSystem.Pointer;
using Random = UnityEngine.Random;

namespace Chud.Backend;

internal partial class Mods : MonoBehaviour
{
	public struct MenuColors
	{
		public Color NormalColor;

		public Color ButtonColorEnabled;

		public Color ButtonColorDisable;

		public Color EnableTextColor;

		public Color DisableTextColor;

		public Color NextPrevButtonColor;

		public Color MenuTitleColor;
	}

	public static Mods instance;

	internal static bool ghostMonkeOn = false;

	internal static bool invisMonkeOn = false;

	internal static bool cloningGhostRig;

	public static bool LocalRigOverrideActive
	{
		get
		{
			if (instance == null)
			{
				return false;
			}
			return ghostMonkeOn || invisMonkeOn || instance.grabRigActive || instance.orbitActive || instance.copyMovementActive || instance.tagGunLockedTarget != null || instance.tagAllTarget != null;
		}
	}

	private static Shader CachedGuiTextShader => ShaderCache.GuiText;

	private static Shader CachedUberShader => ShaderCache.Uber;

	private List<ButtonInfo> _cachedActiveButtons = new List<ButtonInfo>();

	private bool _activeButtonsDirty = true;

	public static float flySpeed = 8f;

	public static readonly float[] FlySpeedValues = new float[] { 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f, 17f, 18f, 19f, 20f };

	public static readonly string[] FlySpeedNames = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };

	public static float controllerPred = 0.0125f;

	public static readonly float[] ControllerPredValues = new float[] { 0.00625f, 0.0125f, 0.025f, 0.05f };

	public static readonly string[] ControllerPredNames = new string[] { "Low", "Normal", "High", "Extreme" };

	public static int controllerPredIndex = 1;

	public static bool fpsSpoofActive = false;

	public static int fpsSpoofValue = 60;

	public static readonly int[] FPSSpoofValues = new int[] { 0, 20, 45, 60, 67, 72, 80, 85, 120, 200, 225 };

	public static readonly float[] WasdSenseValues = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f, 2.75f, 3f };

	public static float wasdFlyMouseSense = 1f;

	public static int speedboostCycle = 0;

	public static float jspeed = 7.5f;

	public static float jmulti = 1.1f;

	public static readonly float[] SpeedBoostSpeeds = new float[] { 7.5f, 8f, 9f, 12f, 15f, 20f, 30f, 50f, 100f, 200f };

	public static readonly float[] SpeedBoostMultis = new float[] { 1.1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f };

	public static readonly string[] SpeedBoostNames = new string[] { "Normal", "Slightly Fast", "Fast", "Faster", "Much Faster", "Very Fast", "Extremely Fast", "Incredibly Fast", "Unbelievably Fast", "Maximum" };

	public static readonly float[] PullPowerValues = new float[] { 0.03f, 0.05f, 0.08f, 0.1f, 0.15f, 0.2f, 0.3f, 0.4f, 0.5f };

	public static readonly string[] PullPowerNames = new string[] { "Slightly Weak", "Normal", "Slightly Strong", "Strong", "Stronger", "Much Stronger", "Very Strong", "Extremely Strong", "Maximum" };

	public static int pullPowerInt;

	public static bool blockJmanSounds = false;

	public static bool antiGuardianGrab = false;

	public static bool antiBlockCrash = false;

	public static bool seeAntiCheatReports = false;

	public static readonly Dictionary<string, int> antiCheatReportCounts = new Dictionary<string, int>();

	public static bool antiReportEnabled;

	public static int antiReportRangeIndex = 1;

	public static float antiReportRange = 0.35f;

	public static readonly float[] antiReportRanges = new float[] { 0.25f, 0.35f, 0.5f, 0.7f, 1f, 1.25f, 1.5f, 2f };

	public static Font comicSansFont;

	public static readonly float[] TagAuraRanges = new float[] { 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f };

	public static float tagAuraRange = 1.5f;

	public static int tagAuraRangeIndex = 2;

	private float tagAuraCooldown;

	private LineRenderer tagAuraRing;

	public static bool thirdPersonEnabled;

	public static int waterSplashSpeedIndex = 1;

	public static readonly float[] WaterSplashCooldowns = new float[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.4f, 0.5f, 0.75f, 1f };

	public static readonly string[] WaterSplashNames = new string[] { "0.05s", "0.1s", "0.15s", "0.2s", "0.25s", "0.3s", "0.4s", "0.5s", "0.75s", "1s" };

	public static int menuColorIndex = 0;

	private static readonly int[] notificationTimeValues = new int[10] { 50, 75, 100, 125, 150, 200, 250, 300, 400, 500 };

	private static readonly string[] notificationTimeNames = new string[10] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" };

	public static int ButtonSound = 67;

	public static GameObject pointer = null;

	public static LineRenderer Line;

	public static RaycastHit raycastHit;

	public static bool gripHeld = false;

	public static bool triggerHeld = false;

	public static bool RPlat;

	public static bool LPlat;

	public static bool isRightHanded = false;

	public static int activeMenuStyle = 3;

	public static bool breakGuardianActive = false;

	private Harmony breakGuardianHarmony;

	private bool notificationsEnabled = true;

	private int notificationDecayTime = 150;

	private int notificationTimeIndex = 5;

	private static readonly int[] LegacyColorIndexMap = new int[] { 0, 7, 2, 3, 9, 8, 6, 5, 5, 1 };

	private const int PaletteVersion = 1;

	private bool saveDirty = false;

	private float lastSaveTime = -99f;

	private const float SaveFlushInterval = 5f;

	private float antiReportDelay;

	private GameObject antiReportSphere;

	private Material antiReportMat;

	private bool pcButtonClickEnabled = false;

	private Vector3? pcButtonOldLocalPosition;

	private Camera pcButtonCachedCamera;

	private int? noInvisLayerMask;

	private bool pcGunsEnabled = false;

	private Harmony vimHarmony;

	private static readonly string treePinCosmeticId = "LBAAA.";

	private static readonly string tryOnAllButtonId = "fun_tryon_all";

	private static readonly string removeAllButtonId = "fun_remove_all";

	private bool tryOnAllActive;

	private Coroutine tryOnAllCoroutine;

	private CosmeticsController.CosmeticItem[] tryOnAllSavedWorn;

	private bool removeAllActive;

	private Coroutine removeAllCoroutine;

	private MethodInfo cachedAddCosmeticMethod;

	private ParameterInfo[] cachedAddCosmeticParameters;

	private bool boopActive = false;

	private bool boopLastL;

	private bool boopLastR;

	private float boopCooldown;

	private int randomColorSpazTick;

	private bool bitcrunchMicActive = false;

	private int bitcrunchOrigSampleRate = 16000;

	private int bitcrunchOrigBitrate = 24000;

	private float splashCooldown;

	private bool antiNameBanApplied = true;

	private Vector3 stumpPosition = new Vector3(-66.871f, 12.086f, -82.637f);

	private string savedGroupKickRoom;

	private float lastUntagNotif = 0f;

	private float lastUntagSelfTime;

	private VRRig guardianSpazTarget;

	private float guardianSpazTimer;

	private float lastGuardianGunTime;

	private float lastUnguardianGunTime;

	private bool spazAllActive = false;

	private int spazAllFrameCounter = 0;

	private bool spazSelfActive = false;

	private int spazSelfFrameCounter = 0;

	private bool gunTriggerWasDown = false;

	private Camera pcGunCamera;

	private bool lagGunRunning;

	private int lagGunTargetActor = -1;

	private VRRig lagGunLockedTarget;

	private static readonly byte[] lagPayload = new byte[128];

	public static string ConfigPath => WristMenu.FolderName + "\\Config.json";

	private void Awake()
	{
		instance = this;
		InitTagProviders();
	}

	public static void InvalidateActiveButtonsCache()
	{
		if (instance != null)
		{
			instance._activeButtonsDirty = true;
		}
	}

	public static bool IsInGameMode(string gameMode)
	{
		if (string.IsNullOrEmpty(gameMode))
		{
			return true;
		}
		GorillaGameManager gm = GorillaGameManager.instance;
		switch (gameMode)
		{
			case "Infection":
				return gm is GorillaTagManager;
			case "Guardian":
				return gm is GorillaGuardianManager;
			case "Paintbrawl":
				return gm is GorillaPaintbrawlManager;
			default:
				return true;
		}
	}

	private void RebuildActiveButtonsCache()
	{
		_cachedActiveButtons.Clear();
		foreach (MenuCategory category in MenuManager.Instance.Categories)
		{
			if (category.Buttons == null) continue;
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled == true && button.type != ButtonType.Action && button.method != null)
				{
					if (button.type == ButtonType.Gun || button.type == ButtonType.FrameToggle)
					{
						_cachedActiveButtons.Add(button);
					}
				}
			}
		}
		_activeButtonsDirty = false;
	}

	private void Update()
	{
		if (instance == null)
		{
			return;
		}
		if (wasdFlyActive)
			UpdateWASDFly();
		if (flyActive)
			UpdateFly();
		bool xDown = WristMenu.xbuttonDown;
		if (xDown && !xButtonWasDown && thirdPersonEnabled)
			thirdPersonViewActive = !thirdPersonViewActive;
		xButtonWasDown = xDown;
		if (thirdPersonEnabled && thirdPersonViewActive)
			EnableThirdPerson();
		else if (FreeCamObject != (Object)null)
			DisableThirdPersonView();
	}

	private void LateUpdate()
	{
		if (VRRig.LocalRig == (Object)null)
		{
			return;
		}
		if (joystickFlyActive || wasdFlyActive || flyActive)
		{
			GTPlayer.Instance.transform.position += _flyDesiredVelocity * Time.deltaTime;
			_flyDesiredVelocity = Vector3.zero;
		}
		if (ghostMonkeOn)
		{
			if (!XRSettings.isDeviceActive)
			{
				VRRig local = VRRig.LocalRig;
				EnsureLocalRigEnabled();
				Vector3 pos = GTPlayer.Instance.transform.position + Vector3.up * 0.2f;
				Quaternion rot = GTPlayer.Instance.transform.rotation;
				local.transform.SetPositionAndRotation(pos, rot);
				if (local.head != null && local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
			}
			else
			{
				EnsureLocalRigEnabled();
				VRRig.LocalRig.transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
				ApplyRigSnapshot(ref ghostMonkeSnapshot);
			}
		}
		if (invisMonkeOn)
		{
			if (!XRSettings.isDeviceActive)
			{
				VRRig local = VRRig.LocalRig;
				EnsureLocalRigEnabled();
				Vector3 pos = GTPlayer.Instance.transform.position + Vector3.up * 0.2f;
				Quaternion rot = GTPlayer.Instance.transform.rotation;
				local.transform.SetPositionAndRotation(pos, rot);
				if (local.head != null && local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
			}
			else
			{
				EnsureLocalRigEnabled();
				VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
			}
		}
		if (ghostRigSubscribed && GhostWanted())
			GhostRigTick();
		if (VRRig.LocalRig.playerText1 != null)
			VRRig.LocalRig.playerText1.color = ColorUtil.PlayerColor(VRRig.LocalRig);
		GrabRigTick();
		CopyMovementTick();
		OrbitTick();
		TagRigVisualTick();
		if (!LocalRigOverrideActive)
		{
			SpiderMonkeyTick();
		}
		UpdateBoop();
		if (stickyRightActive && jump_right_local != null) ClampHandToCage(jump_right_local.transform.position, true);
		if (stickyLeftActive && jump_left_local != null) ClampHandToCage(jump_left_local.transform.position, false);
		UpdateGrabBugs();
	}

	public static void UpdateActiveMods()
	{
		if (instance == null)
		{
			return;
		}
		instance.UpdateActiveModsCore();
	}

	private void UpdateActiveModsCore()
	{
		AntiBlockCrashTick();
		UpdatePCButtonClick();
		UpdatePCGuns();
		if (joystickFlyActive) UpdateJoystickFly();
		UpdateCosmeticNotifier();
		Console.UpdateConsoleUserIndicators();
		ARSDetect();
		AntiReportTick();
		AntiReportVisual();

		if (_activeButtonsDirty)
		{
			RebuildActiveButtonsCache();
		}
		bool flag = false;
		for (int i = 0; i < _cachedActiveButtons.Count; i++)
		{
			ButtonInfo button = _cachedActiveButtons[i];
			if (button.enabled != true || button.method == null)
			{
				continue;
			}
			button.method();
			if (button.type == ButtonType.Gun)
			{
				flag = true;
			}
		}
		ARSNameTagUpdate();
		if (spazAllActive || spazSelfActive)
		{
			if (spazAllActive)
			{
				spazAllFrameCounter++;
				if (spazAllFrameCounter >= 5)
				{
					spazAllFrameCounter = 0;
					RunSpaz();
				}
			}
			if (spazSelfActive)
			{
				spazSelfFrameCounter++;
				if (spazSelfFrameCounter >= 5)
				{
					spazSelfFrameCounter = 0;
					RunSpaz();
				}
			}
		}
		if (!flag && pointer != (Object)null)
		{
			Object.Destroy(pointer);
			pointer = null;
			if (Line != (Object)null)
			{
				Object.Destroy(((Component)Line).gameObject);
				Line = null;
			}
			gunTriggerWasDown = false;
		}
		WristMenu.UpdateGradientAnimations(Time.time);
		ConsoleMods.Run();
		Console.UpdateAdminIndicators();
		FlushSave();
	}

	public static void Save()
	{
		if (instance == null)
		{
			return;
		}
		if (Time.time - instance.lastSaveTime >= SaveFlushInterval)
		{
			instance.WriteSave();
			return;
		}
		instance.saveDirty = true;
	}

	private static void FlushSave()
	{
		if (instance == null || !instance.saveDirty)
		{
			return;
		}
		if (Time.time - instance.lastSaveTime < SaveFlushInterval)
		{
			return;
		}
		instance.WriteSave();
	}

	private void WriteSave()
	{
		try
		{
			if (!Directory.Exists(WristMenu.FolderName))
				Directory.CreateDirectory(WristMenu.FolderName);

			var root = new JObject();

			var enabledButtons = new JArray();
			foreach (MenuCategory category in MenuManager.Instance.Categories)
			{
				if (category.Buttons == null || category.Name == "Enabled Mods") continue;
				foreach (ButtonInfo button in category.Buttons)
				{
					if (button.enabled.HasValue && button.enabled.Value && !string.IsNullOrEmpty(button.buttonText))
						enabledButtons.Add(button.id ?? button.buttonText);
				}
			}
			root["EnabledButtons"] = enabledButtons;

			root["FlySpeed"] = flySpeed;
			root["SpeedboostCycle"] = speedboostCycle;
			root["PullPowerInt"] = pullPowerInt;
			root["WasdFlyMouseSense"] = wasdFlyMouseSense;
			root["IsRightHanded"] = isRightHanded;
			root["MenuColorIndex"] = menuColorIndex;
			root["PaletteVersion"] = PaletteVersion;
			root["NotificationTimeIndex"] = notificationTimeIndex;
			root["Jspeed"] = jspeed;
			root["Jmulti"] = jmulti;
			root["TagAuraRange"] = tagAuraRange;
			root["TagAuraRangeIndex"] = tagAuraRangeIndex;
			root["AdminScale"] = Console.adminScale;
			root["AnimationsEnabled"] = WristMenu.animationsEnabled;
			root["ToggleMenu"] = WristMenu.toggleMenu;
			root["ShowFPS"] = WristMenu.showFPS;
			root["ShowSessionTime"] = WristMenu.showSessionTime;
			root["CustomBoardsEnabled"] = WristMenu.customBoardsEnabled;
			root["BlockJmanSounds"] = blockJmanSounds;
			root["AntiGuardianGrab"] = antiGuardianGrab;
			root["AntiBlockCrash"] = antiBlockCrash;
			root["SeeAntiCheatReports"] = seeAntiCheatReports;
			root["AntiReportEnabled"] = antiReportEnabled;
			root["AntiReportRangeIndex"] = antiReportRangeIndex;
			root["WaterSplashSpeedIndex"] = waterSplashSpeedIndex;
			root["BreakGuardianActive"] = breakGuardianActive;
			root["ButtonClickIndex"] = WristMenu.buttonClickIndex;

			root["ConsoleAllowKickSelf"] = Console.allowKickSelf;
			root["ConsoleAllowTpSelf"] = Console.allowTpSelf;
			root["ConsoleDisableFlingSelf"] = Console.disableFlingSelf;
			root["ConsoleLaserEnabled"] = Console.laserEnabled;
			root["ConsoleAutoDetectConsoleUsers"] = Console.autoDetectConsoleUsers;
			root["ConsoleLogging"] = Console.consoleLogging;
			root["ConsoleFullAutoPistol"] = Console.fullAutoPistol;

			string json = root.ToString(Formatting.Indented);
			if (string.IsNullOrEmpty(json) || json.Length < 10) return;
			string tempPath = ConfigPath + ".tmp";
			File.WriteAllText(tempPath, json);
			if (File.Exists(ConfigPath))
				File.Replace(tempPath, ConfigPath, null);
			else
				File.Move(tempPath, ConfigPath);
			lastSaveTime = Time.time;
			saveDirty = false;
		}
		catch (Exception e)
		{
			NotifiLib.SendNotification("Failed to save config: " + e.Message);
		}
	}

	public static void Load()
	{
		if (instance == null)
		{
			return;
		}
		instance.LoadCore();
	}

	private void LoadCore()
	{
		try
		{
			if (!File.Exists(ConfigPath))
			{
				string tempPath = ConfigPath + ".tmp";
				if (File.Exists(tempPath))
				{
					try
					{
						if (JObject.Parse(File.ReadAllText(tempPath)) == null)
							return;
					}
					catch
					{
						try { File.Delete(tempPath); } catch { }
						return;
					}
					File.Move(tempPath, ConfigPath);
				}
				else
					return;
			}
			string json = File.ReadAllText(ConfigPath);
			if (string.IsNullOrEmpty(json) || json.Length < 10) return;
			var root = JObject.Parse(json);
			if (root == null) return;

			flySpeed = (float)(root["FlySpeed"] ?? 8f);
			speedboostCycle = (int)(root["SpeedboostCycle"] ?? 0);
			pullPowerInt = (int)(root["PullPowerInt"] ?? 0);
			wasdFlyMouseSense = (float)(root["WasdFlyMouseSense"] ?? 1f);
			isRightHanded = (bool)(root["IsRightHanded"] ?? false);
			menuColorIndex = (int)(root["MenuColorIndex"] ?? 0);
			if ((int)(root["PaletteVersion"] ?? 0) < PaletteVersion)
			{
				if (menuColorIndex >= 0 && menuColorIndex < LegacyColorIndexMap.Length)
				{
					menuColorIndex = LegacyColorIndexMap[menuColorIndex];
				}
			}
			notificationTimeIndex = (int)(root["NotificationTimeIndex"] ?? 5);
			jspeed = (float)(root["Jspeed"] ?? 7.5f);
			jmulti = (float)(root["Jmulti"] ?? 1.1f);
			tagAuraRange = (float)(root["TagAuraRange"] ?? 1.5f);
			tagAuraRangeIndex = (int)(root["TagAuraRangeIndex"] ?? 3);

			Console.adminScale = (float)(root["AdminScale"] ?? 1f);

			WristMenu.animationsEnabled = (bool)(root["AnimationsEnabled"] ?? false);
			WristMenu.toggleMenu = (bool)(root["ToggleMenu"] ?? false);
			WristMenu.showFPS = (bool)(root["ShowFPS"] ?? false);
			WristMenu.showSessionTime = (bool)(root["ShowSessionTime"] ?? false);
			WristMenu.customBoardsEnabled = (bool)(root["CustomBoardsEnabled"] ?? true);
			blockJmanSounds = (bool)(root["BlockJmanSounds"] ?? false);
			antiGuardianGrab = (bool)(root["AntiGuardianGrab"] ?? false);
			antiBlockCrash = (bool)(root["AntiBlockCrash"] ?? false);
			seeAntiCheatReports = (bool)(root["SeeAntiCheatReports"] ?? false);
			antiReportEnabled = (bool)(root["AntiReportEnabled"] ?? false);
			antiReportRangeIndex = (int)(root["AntiReportRangeIndex"] ?? 1);
			antiReportRange = antiReportRanges[antiReportRangeIndex % antiReportRanges.Length];
			waterSplashSpeedIndex = (int)(root["WaterSplashSpeedIndex"] ?? 1);
			breakGuardianActive = (bool)(root["BreakGuardianActive"] ?? false);
			WristMenu.buttonClickIndex = (int)(root["ButtonClickIndex"] ?? 0);
			Console.allowKickSelf = (bool)(root["ConsoleAllowKickSelf"] ?? false);
			Console.allowTpSelf = (bool)(root["ConsoleAllowTpSelf"] ?? true);
			Console.disableFlingSelf = (bool)(root["ConsoleDisableFlingSelf"] ?? false);
			Console.laserEnabled = (bool)(root["ConsoleLaserEnabled"] ?? false);
			Console.autoDetectConsoleUsers = (bool)(root["ConsoleAutoDetectConsoleUsers"] ?? false);
			Console.consoleLogging = (bool)(root["ConsoleLogging"] ?? false);
			Console.fullAutoPistol = (bool)(root["ConsoleFullAutoPistol"] ?? false);

			if (menuColorIndex < 0 || menuColorIndex >= 10)
				menuColorIndex = 0;
			if (notificationTimeIndex < 0 || notificationTimeIndex >= notificationTimeValues.Length)
				notificationTimeIndex = 5 % notificationTimeValues.Length;
			if (antiReportRangeIndex < 0 || antiReportRangeIndex >= antiReportRanges.Length)
				antiReportRangeIndex = 1 % antiReportRanges.Length;
			if (waterSplashSpeedIndex < 0)
				waterSplashSpeedIndex = 0;
			if (WristMenu.buttonClickIndex < 0 || WristMenu.buttonClickIndex >= WristMenu.ButtonClickUrls.Length)
				WristMenu.buttonClickIndex = 0;
			if (speedboostCycle < 0 || speedboostCycle >= SpeedBoostNames.Length)
				speedboostCycle = 0;
			if (pullPowerInt < 0 || pullPowerInt >= PullPowerValues.Length)
				pullPowerInt = 1 % PullPowerValues.Length;
			if (tagAuraRangeIndex < 0 || tagAuraRangeIndex >= TagAuraRanges.Length)
				tagAuraRangeIndex = 3 % TagAuraRanges.Length;
			ApplyMenuColor(menuColorIndex);
			notificationDecayTime = notificationTimeValues[notificationTimeIndex];
			NotifiLib.DecayTime = notificationDecayTime;
			antiReportRange = antiReportRanges[antiReportRangeIndex];
			tagAuraRange = TagAuraRanges[tagAuraRangeIndex];

			var savedButtons = root["EnabledButtons"] as JArray;
			if (savedButtons != null)
			{
				var idLookup = new Dictionary<string, ButtonInfo>(StringComparer.Ordinal);
				var textLookup = new Dictionary<string, ButtonInfo>(StringComparer.Ordinal);
				foreach (MenuCategory cat in MenuManager.Instance.Categories)
				{
					if (cat.Buttons == null || cat.Name == "Enabled Mods") continue;
					foreach (ButtonInfo btn in cat.Buttons)
					{
						if (btn.type == ButtonType.Action || !btn.enabled.HasValue || string.IsNullOrEmpty(btn.buttonText)) continue;
						if (!string.IsNullOrEmpty(btn.id) && !idLookup.ContainsKey(btn.id))
							idLookup[btn.id] = btn;
						if (!textLookup.ContainsKey(btn.buttonText))
							textLookup[btn.buttonText] = btn;
					}
				}

				var matched = new HashSet<ButtonInfo>();
				foreach (JToken token in savedButtons)
				{
					string savedName = (string)token;
					if (string.IsNullOrEmpty(savedName)) continue;
					ButtonInfo found = null;
					if (!idLookup.TryGetValue(savedName, out found))
						textLookup.TryGetValue(savedName, out found);
					if (found != null)
						matched.Add(found);
				}

				foreach (MenuCategory cat in MenuManager.Instance.Categories)
				{
					if (cat.Buttons == null || cat.Name == "Enabled Mods") continue;
					foreach (ButtonInfo btn in cat.Buttons)
					{
						if (btn.type == ButtonType.Action || !btn.enabled.HasValue) continue;
						if (btn.enabled == true && !matched.Contains(btn))
						{
							try { btn.disableMethod?.Invoke(); } catch { }
							btn.enabled = false;
						}
					}
				}

				foreach (ButtonInfo btn in matched)
				{
					if (btn.enabled != true)
						btn.enabled = true;
				}
			}
		}
		catch { }
		InvalidateActiveButtonsCache();
		try { ReapplyActiveMods(); } catch { }
	}

	public static void ReapplyActiveMods()
	{
		foreach (MenuCategory category in MenuManager.Instance.Categories)
		{
			if (category.Buttons == null)
			{
				continue;
			}
			foreach (ButtonInfo button in category.Buttons)
			{
				if (button.enabled == true && button.type != ButtonType.Action)
				{
					if (button.enableMethod != null)
						button.enableMethod();
					else
						button.method?.Invoke();
				}
			}
		}
	}

	public static void ToggleNotifications()
	{
		if (instance == null)
		{
			return;
		}
		if (!instance.notificationsEnabled)
		{
			NotifiLib.IsEnabled = true;
			instance.notificationsEnabled = true;
		}
	}

	public static void DisableNotifications()
	{
		if (instance == null)
		{
			return;
		}
		if (instance.notificationsEnabled)
		{
			NotifiLib.IsEnabled = false;
			instance.notificationsEnabled = false;
		}
	}

	public static void ClearNotifications()
	{
		NotifiLib.ClearAllNotifications();
	}

	public static void SetNotificationTime(int index)
	{
		if (instance == null)
		{
			return;
		}
		instance.notificationTimeIndex = index % notificationTimeValues.Length;
		if (instance.notificationTimeIndex < 0) instance.notificationTimeIndex = notificationTimeValues.Length - 1;
		instance.notificationDecayTime = notificationTimeValues[instance.notificationTimeIndex];
		NotifiLib.DecayTime = instance.notificationDecayTime;
		NotifiLib.SendNotification("Notification time: " + notificationTimeNames[instance.notificationTimeIndex]);
	}

	private void ApplyMenuColor(int index)
	{
		MenuColors menuColors = GetMenuColors(index);
		WristMenu.NormalColor = menuColors.NormalColor;
		WristMenu.ButtonColorEnabled = menuColors.ButtonColorEnabled;
		WristMenu.ButtonColorDisable = menuColors.ButtonColorDisable;
		WristMenu.EnableTextColor = Color.white;
		WristMenu.DisableTextColor = new Color(0.75f, 0.75f, 0.75f);
		WristMenu.NextPrevButtonColor = menuColors.NextPrevButtonColor;
		WristMenu.MenuTitleColor = Color.white;
		WristMenu.ToolTipColor = new Color(0.8f, 0.8f, 0.8f);
		WristMenu.NextPrevTextColor = Color.white;
		WristMenu.DisconnectButtonColor = new Color(0.5f, 0f, 0f);
		WristMenu.DisconnectTextColor = Color.white;
	}

	public static void SetMenuColor(int index)
	{
		menuColorIndex = index;
		if (instance != null)
		{
			instance.ApplyMenuColor(index);
		}
		string[] colorNames = new string[] { "Gray", "Brown", "Red", "Orange", "Yellow", "Pink", "Purple", "Blue", "Cyan", "Green" };
		string name = (index >= 0 && index < colorNames.Length) ? colorNames[index] : "Custom";
		NotifiLib.SendNotification("Menu Color: " + name, 2);
		Save();
		if (WristMenu.toggleMenu)
		{
			WristMenu.RefreshMenu();
		}
		else
		{
			WristMenu.DestroyMenu();
			WristMenu.instance.Draw();
		}
	}

	public static MenuColors GetMenuColors(int index)
	{
		MenuColors result = default(MenuColors);
		switch (index)
		{
		case 0:
			result.NormalColor = new Color(0.12f, 0.12f, 0.14f);
			result.ButtonColorEnabled = new Color(0.55f, 0.55f, 0.6f);
			result.ButtonColorDisable = new Color(0.22f, 0.22f, 0.28f);
			result.EnableTextColor = Color.white;
			result.DisableTextColor = new Color(0.7f, 0.7f, 0.75f);
			result.NextPrevButtonColor = new Color(0.18f, 0.18f, 0.22f);
			break;
		case 1:
			result.NormalColor = new Color(0.15f, 0.1f, 0.04f);
			result.ButtonColorEnabled = new Color(0.7f, 0.45f, 0.2f);
			result.ButtonColorDisable = new Color(0.35f, 0.22f, 0.1f);
			result.EnableTextColor = new Color(0.9f, 0.75f, 0.5f);
			result.DisableTextColor = new Color(0.65f, 0.55f, 0.35f);
			result.NextPrevButtonColor = new Color(0.22f, 0.14f, 0.06f);
			break;
		case 2:
			result.NormalColor = new Color(0.18f, 0.04f, 0.04f);
			result.ButtonColorEnabled = new Color(0.9f, 0.2f, 0.2f);
			result.ButtonColorDisable = new Color(0.5f, 0.08f, 0.08f);
			result.EnableTextColor = new Color(1f, 0.6f, 0.6f);
			result.DisableTextColor = new Color(0.7f, 0.3f, 0.3f);
			result.NextPrevButtonColor = new Color(0.28f, 0.06f, 0.06f);
			break;
		case 3:
			result.NormalColor = new Color(0.18f, 0.1f, 0.04f);
			result.ButtonColorEnabled = new Color(0.9f, 0.5f, 0.1f);
			result.ButtonColorDisable = new Color(0.55f, 0.28f, 0.08f);
			result.EnableTextColor = new Color(1f, 0.8f, 0.5f);
			result.DisableTextColor = new Color(0.75f, 0.5f, 0.3f);
			result.NextPrevButtonColor = new Color(0.3f, 0.15f, 0.06f);
			break;
		case 4:
			result.NormalColor = new Color(0.18f, 0.15f, 0.04f);
			result.ButtonColorEnabled = new Color(0.9f, 0.8f, 0.15f);
			result.ButtonColorDisable = new Color(0.5f, 0.42f, 0.08f);
			result.EnableTextColor = new Color(1f, 0.95f, 0.6f);
			result.DisableTextColor = new Color(0.75f, 0.68f, 0.35f);
			result.NextPrevButtonColor = new Color(0.3f, 0.26f, 0.06f);
			break;
		case 5:
			result.NormalColor = new Color(0.24f, 0.12f, 0.18f);
			result.ButtonColorEnabled = new Color(0.7f, 0.24f, 0.48f);
			result.ButtonColorDisable = new Color(0.42f, 0.18f, 0.29f);
			result.EnableTextColor = new Color(1f, 0.85f, 0.93f);
			result.DisableTextColor = new Color(0.72f, 0.45f, 0.58f);
			result.NextPrevButtonColor = new Color(0.3f, 0.13f, 0.2f);
			break;
		case 6:
			result.NormalColor = new Color(0.14f, 0.04f, 0.2f);
			result.ButtonColorEnabled = new Color(0.6f, 0.25f, 0.9f);
			result.ButtonColorDisable = new Color(0.3f, 0.1f, 0.5f);
			result.EnableTextColor = new Color(0.8f, 0.6f, 1f);
			result.DisableTextColor = new Color(0.5f, 0.3f, 0.7f);
			result.NextPrevButtonColor = new Color(0.2f, 0.08f, 0.32f);
			break;
		case 7:
			result.NormalColor = new Color(0.05f, 0.12f, 0.15f);
			result.ButtonColorEnabled = new Color(0.133f, 0.267f, 0.333f);
			result.ButtonColorDisable = new Color(0.07f, 0.17f, 0.21f);
			result.EnableTextColor = new Color(0.55f, 0.7f, 0.85f);
			result.DisableTextColor = new Color(0.33f, 0.47f, 0.6f);
			result.NextPrevButtonColor = new Color(0.06f, 0.13f, 0.17f);
			break;
		case 8:
			result.NormalColor = new Color(0.04f, 0.14f, 0.18f);
			result.ButtonColorEnabled = new Color(0.15f, 0.75f, 0.9f);
			result.ButtonColorDisable = new Color(0.08f, 0.38f, 0.5f);
			result.EnableTextColor = new Color(0.5f, 0.9f, 1f);
			result.DisableTextColor = new Color(0.3f, 0.65f, 0.75f);
			result.NextPrevButtonColor = new Color(0.06f, 0.22f, 0.3f);
			break;
		case 9:
			result.NormalColor = new Color(0.1f, 0.15f, 0.1f);
			result.ButtonColorEnabled = new Color(0.4f, 0.6f, 0.4f);
			result.ButtonColorDisable = new Color(0.22f, 0.33f, 0.22f);
			result.EnableTextColor = new Color(0.6f, 0.9f, 0.6f);
			result.DisableTextColor = new Color(0.35f, 0.55f, 0.35f);
			result.NextPrevButtonColor = new Color(0.12f, 0.18f, 0.12f);
			break;
		default:
			result = GetMenuColors(0);
			break;
		}
		result.MenuTitleColor = result.EnableTextColor;
		return result;
	}

	public static void TPGun()
	{
		MakeRightHandGun(delegate
		{
			if (GTPlayer.Instance != (Object)null && pointer != (Object)null)
			{
				Vector3 pos = pointer.transform.position;
				Vector3 playerPos = GorillaTagger.Instance.transform.position - GorillaTagger.Instance.bodyCollider.transform.position + pos;
				GTPlayer.Instance.TeleportTo(playerPos, GTPlayer.Instance.transform.rotation, true, false);
				VRRig.LocalRig.transform.position = pos;
			}
		});
	}

	public static void JoinCode(string code)
	{
		NotifiLib.SendNotification("Joining room: " + code);
		if (instance == null)
		{
			return;
		}
		instance.StartCoroutine(JoinRoomDirect(code));
	}

	private static IEnumerator JoinRoomDirect(string code)
	{
		yield return new WaitForSeconds(5f);
		PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, JoinType.Solo);
	}

	public static void JoinRandomPublic()
	{
		GorillaNetworkJoinTrigger trigger = PhotonNetworkController.Instance.currentJoinTrigger;
		if (trigger == null && ZoneManagement.instance != null && ZoneManagement.instance.activeZones.Count > 0)
			trigger = GorillaComputer.instance.GetJoinTriggerForZone(ZoneManagement.instance.activeZones.First().GetName());
		if (trigger == null && GorillaComputer.instance != null)
		{
			try
			{
				var dict = Traverse.Create(GorillaComputer.instance).Field("primaryTriggersByZone").GetValue<Dictionary<string, GorillaNetworkJoinTrigger>>();
				if (dict != null)
				{
					foreach (var kv in dict)
					{
						if (kv.Value != null)
						{
							trigger = kv.Value;
							break;
						}
					}
				}
			}
			catch { }
		}
		if (trigger == null)
		{
			NotifiLib.SendNotification("No join trigger found");
			return;
		}
		if (instance == null)
		{
			return;
		}
		instance.StartCoroutine(JoinRandomPublicRoutine(trigger));
	}

	private static IEnumerator JoinRandomPublicRoutine(GorillaNetworkJoinTrigger trigger)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
			float timeout = 8f;
			while (timeout > 0f && NetworkSystem.Instance.netState != NetSystemState.Idle)
			{
				timeout -= Time.deltaTime;
				yield return null;
			}
			yield return new WaitForSeconds(0.4f);
		}
		PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger, JoinType.Solo);
	}

	public static void GroupKickAll()
	{
		if (!PhotonNetwork.InRoom || !NetworkSystem.Instance.SessionIsPrivate)
		{
			NotifiLib.SendNotification("Only works in private rooms!");
			return;
		}
		if (instance == null)
		{
			return;
		}
		instance.savedGroupKickRoom = PhotonNetwork.CurrentRoom.Name;
		GorillaComputer.instance.OnGroupJoinButtonPress(
			GorillaComputer.instance.groupMapJoinIndex,
			GorillaComputer.instance.friendJoinCollider
		);
		instance.StartCoroutine(RejoinAfterGroupKick());
	}

	private static IEnumerator RejoinAfterGroupKick()
	{
		if (instance == null)
		{
			yield break;
		}
		while (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name == instance.savedGroupKickRoom)
			yield return null;
		yield return new WaitForSeconds(3f);
		if (string.IsNullOrEmpty(instance.savedGroupKickRoom)) yield break;
		for (int i = 0; i < 4; i++)
		{
			if (PhotonNetwork.InRoom)
				PhotonNetwork.Disconnect();
			yield return new WaitForSeconds(2f);
			PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(instance.savedGroupKickRoom, JoinType.Solo);
			float timeout = 10f;
			while (timeout > 0f && (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name != instance.savedGroupKickRoom))
			{
				timeout -= Time.deltaTime;
				yield return null;
			}
			if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == instance.savedGroupKickRoom)
			{
				instance.savedGroupKickRoom = null;
				yield break;
			}
		}
		instance.savedGroupKickRoom = null;
	}

	public static void GetPlayerIDGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				GUIUtility.systemCopyBuffer = rig.Creator.UserId;
				NotifiLib.SendNotification("Copied: " + rig.Creator.UserId);
			}
		});
	}

	public static void GetIDSelf()
	{
		string text = (GUIUtility.systemCopyBuffer = PhotonNetwork.LocalPlayer.UserId);
		NotifiLib.SendNotification("Copied self: " + text);
	}

	public static void UnlockVim()
	{
		if (instance == null || instance.vimHarmony != null)
		{
			return;
		}
		instance.vimHarmony = new Harmony("chudmenu.vim");
		Type type = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType("GorillaTagScripts.SubscriptionManager");
			if (type != null)
			{
				break;
			}
		}
		if (type != null)
		{
			MethodInfo method = type.GetMethod("IsLocalSubscribed", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				MethodInfo method2 = typeof(Mods).GetMethod("VimPrefix", BindingFlags.Static | BindingFlags.Public);
				instance.vimHarmony.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	public static void DisableUnlockVim()
	{
		if (instance == null || instance.vimHarmony == null)
		{
			return;
		}
		instance.vimHarmony.UnpatchSelf();
		instance.vimHarmony = null;
	}

	public static bool VimPrefix(ref bool __result)
	{
		__result = true;
		return false;
	}

	public static void EnableSeeAntiCheatReports()
	{
		seeAntiCheatReports = true;
	}

	public static void DisableSeeAntiCheatReports()
	{
		seeAntiCheatReports = false;
		antiCheatReportCounts.Clear();
	}

	public static void EnableAntiReport()
	{
		antiReportEnabled = true;
	}

	public static void DisableAntiReport()
	{
		antiReportEnabled = false;
		if (instance != null && instance.antiReportSphere != null)
		{
			Object.Destroy(instance.antiReportSphere);
			instance.antiReportSphere = null;
		}
	}

	public static void SetAntiReportRange(int index)
	{
		antiReportRangeIndex = index % antiReportRanges.Length;
		if (antiReportRangeIndex < 0) antiReportRangeIndex = antiReportRanges.Length - 1;
		antiReportRange = antiReportRanges[antiReportRangeIndex];
		NotifiLib.SendNotification("Range: " + antiReportRange.ToString("0.00") + "m");
	}

	private void AntiReportTick()
	{
		if (!antiReportEnabled || !NetworkSystem.Instance.InRoom)
			return;
		if (!(Time.time > antiReportDelay))
			return;
		foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (line.linePlayer == null || !line.linePlayer.IsLocal)
				continue;
			Vector3 reportPos = line.reportButton.gameObject.transform.position;
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (rig == null || rig.isLocal || rig.isOfflineVRRig)
					continue;
				if (Vector3.Distance(rig.rightHandTransform.position, reportPos) < antiReportRange ||
				    Vector3.Distance(rig.leftHandTransform.position, reportPos) < antiReportRange)
				{
					Player player = Console.GetPlayerFromID(rig.Creator.UserId);
					string name = player != null ? player.NickName : "?";
					NotifiLib.SendNotification(name + " attempted to report you");
					antiReportDelay = Time.time + 1f;
					NetworkSystem.Instance.ReturnToSinglePlayer();
					return;
				}
			}
		}
	}

	private void CreateAntiReportSphere()
	{
		if (antiReportSphere != null) return;
		antiReportSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Object.Destroy(antiReportSphere.GetComponent<Collider>());
		if (antiReportMat == null)
		{
			antiReportMat = new Material(Shader.Find("GUI/Text Shader"));
			antiReportMat.color = new Color(1f, 0f, 0f, 0.25f);
		}
		antiReportSphere.GetComponent<Renderer>().material = antiReportMat;
	}

	private void AntiReportVisual()
	{
		if (!antiReportEnabled)
		{
			if (antiReportSphere != null) antiReportSphere.SetActive(false);
			return;
		}
		if (!NetworkSystem.Instance.InRoom)
		{
			if (antiReportSphere != null) antiReportSphere.SetActive(false);
			return;
		}
		CreateAntiReportSphere();
		antiReportMat.color = new Color(1f, 0f, 0f, 0.25f);
		bool found = false;
		foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (line.linePlayer == null || !line.linePlayer.IsLocal)
				continue;
			Vector3 center = line.reportButton.gameObject.transform.position;
			antiReportSphere.transform.position = center;
			antiReportSphere.transform.localScale = Vector3.one * antiReportRange;
			found = true;
			break;
		}
		antiReportSphere.SetActive(found);
	}

	public static void AntiAFK()
	{
		try
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = true;
		}
		catch
		{
		}
	}

	public static void DisableAntiAFK()
	{
		try
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = false;
		}
		catch
		{
		}
	}

	public static void DisableNetworkTriggers()
	{
		NetworkTriggerPatch.enabled = true;
	}

	public static void EnableNetworkTriggers()
	{
		NetworkTriggerPatch.enabled = false;
	}

	public static void DisableQuitBox()
	{
		QuitBoxPatch.enabled = false;
	}

	public static void EnableQuitBox()
	{
		QuitBoxPatch.enabled = true;
	}

	public static void EnablePCButtonClick()
	{
		if (instance == null)
		{
			return;
		}
		instance.pcButtonClickEnabled = true;
	}

	public static void DisablePCButtonClick()
	{
		if (instance == null)
		{
			return;
		}
		instance.pcButtonClickEnabled = false;
		if (instance.pcButtonOldLocalPosition.HasValue)
		{
			GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = instance.pcButtonOldLocalPosition.Value;
			instance.pcButtonOldLocalPosition = null;
		}
		if (GorillaTagger.Instance.rightHandTriggerCollider != (Object)null)
		{
			TransformFollow component = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
			if (component != (Object)null)
			{
				((Behaviour)component).enabled = true;
			}
		}
	}

	private void UpdatePCButtonClick()
	{
		if (!pcButtonClickEnabled || GorillaTagger.Instance == (Object)null || GorillaTagger.Instance.rightHandTriggerCollider == (Object)null)
		{
			return;
		}
		if (Mouse.current != null && Mouse.current.leftButton.isPressed)
		{
			if (pcButtonCachedCamera == (Object)null)
			{
				Camera[] array = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
				foreach (Camera val2 in array)
				{
					if (((Object)val2).name == "Shoulder Camera" || (((Component)val2).gameObject.transform.parent != (Object)null && ((Object)((Component)val2).gameObject.transform.parent).name == "Third Person Camera"))
					{
						pcButtonCachedCamera = val2;
						break;
					}
				}
			}
			Camera val = pcButtonCachedCamera;
			if (!(val != (Object)null))
			{
				return;
			}
			Ray val3 = val.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
			RaycastHit val4 = default(RaycastHit);
			if (!Physics.Raycast(val3, out val4, 512f, GetNoInvisLayerMask()))
			{
				return;
			}
			if (!pcButtonOldLocalPosition.HasValue)
			{
				pcButtonOldLocalPosition = GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition;
				TransformFollow component = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
				if (component != (Object)null)
				{
					((Behaviour)component).enabled = false;
				}
			}
			GorillaTagger.Instance.rightHandTriggerCollider.transform.position = val4.point;
		}
		else
		{
			if (pcButtonOldLocalPosition.HasValue)
			{
				GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = pcButtonOldLocalPosition.Value;
				pcButtonOldLocalPosition = null;
			}
			TransformFollow component2 = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
			if (component2 != (Object)null)
			{
				((Behaviour)component2).enabled = true;
			}
		}
	}

	public static int GetNoInvisLayerMask()
	{
		if (instance != null && instance.noInvisLayerMask.HasValue)
		{
			return instance.noInvisLayerMask.Value;
		}
		if (instance == null)
		{
			return (GTPlayer.Instance != null) ? (int)GTPlayer.Instance.locomotionEnabledLayers : -1;
		}
		return instance.GetNoInvisLayerMaskCore();
	}

	private int GetNoInvisLayerMaskCore()
	{
		if (!noInvisLayerMask.HasValue)
		{
			int excluded = 0;
			string[] layerNames = new string[] { "TransparentFX", "Ignore Raycast", "Zone", "Gorilla Trigger", "Gorilla Boundary", "GorillaCosmetics", "GorillaParticle" };
			foreach (string layerName in layerNames)
			{
				int layer = LayerMask.NameToLayer(layerName);
				if (layer >= 0)
				{
					excluded |= 1 << layer;
				}
			}
			noInvisLayerMask = ~excluded;
		}
		return noInvisLayerMask ?? ((GTPlayer.Instance != null) ? (int)GTPlayer.Instance.locomotionEnabledLayers : -1);
	}

	public static void EnablePCGuns()
	{
		if (instance == null)
		{
			return;
		}
		instance.pcGunsEnabled = true;
	}

	public static void DisablePCGuns()
	{
		if (instance == null)
		{
			return;
		}
		instance.pcGunsEnabled = false;
	}

	private void UpdatePCGuns()
	{
		if (!pcGunsEnabled || Mouse.current == null || XRSettings.isDeviceActive)
		{
			return;
		}
		ControllerInputPoller poller = ControllerInputPoller.instance;
		if (poller == (Object)null)
		{
			return;
		}
		if (Mouse.current.leftButton.isPressed)
		{
			poller.rightControllerIndexFloat = 1f;
			poller.rightControllerTriggerButton = true;
			WristMenu.triggerDownR = true;
			poller.leftControllerIndexFloat = 1f;
			poller.leftControllerTriggerButton = true;
			WristMenu.triggerDownL = true;
		}
		else
		{
			poller.rightControllerIndexFloat = 0f;
			poller.rightControllerTriggerButton = false;
			poller.leftControllerIndexFloat = 0f;
			poller.leftControllerTriggerButton = false;
		}
		if (Mouse.current.rightButton.isPressed)
		{
			poller.rightGrab = true;
			poller.rightControllerGripFloat = 1f;
			WristMenu.gripDownR = true;
			poller.leftGrab = true;
			poller.leftControllerGripFloat = 1f;
			WristMenu.gripDownL = true;
		}
		else
		{
			poller.rightGrab = false;
			poller.rightControllerGripFloat = 0f;
			poller.leftGrab = false;
			poller.leftControllerGripFloat = 0f;
		}
	}

	public static void MuteGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				try
				{
					foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
					{
						if (line.linePlayer != null && line.linePlayer.UserId == rig.Creator.UserId)
						{
							line.muteButton.isOn = !line.muteButton.isOn;
							line.PressButton(line.muteButton.isOn, GorillaPlayerLineButton.ButtonType.Mute);
						}
					}
				}
				catch
				{
				}
			}
		});
	}

	public static void EnableRightHand()
	{
		isRightHanded = true;
		WristMenu.ReanchorToCurrentHand();
	}

	public static void DisableRightHand()
	{
		isRightHanded = false;
		WristMenu.ReanchorToCurrentHand();
	}

	public static void MakeGun(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action onTrigger, Action onRelease)
	{
		if (instance == null)
		{
			return;
		}
		instance.MakeGunCore(color, pointersize, linesize, pointershape, arm, liner, onTrigger, onRelease);
	}

	private void MakeGunCore(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action onTrigger, Action onRelease)
	{
		if (arm == GTPlayer.Instance.RightHand.controllerTransform)
		{
			gripHeld = WristMenu.gripDownR;
			triggerHeld = WristMenu.triggerDownR;
		}
		else if (arm == GTPlayer.Instance.LeftHand.controllerTransform)
		{
			gripHeld = WristMenu.gripDownL;
			triggerHeld = WristMenu.triggerDownL;
		}
		if (gripHeld)
		{
			Transform visualHand = null;
			try
			{
				bool isRight = arm == GTPlayer.Instance.RightHand.controllerTransform;
				visualHand = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
			}
			catch { }
			bool isFlatscreen = !XRSettings.isDeviceActive;
			bool useGhost = isFlatscreen && (object)ghostRig != (Object)null && (tagGunLockedTarget != null || tagAllTarget != null || grabRigActive || ghostMonkeOn || invisMonkeOn || (copyMovementActive && copyMovementTarget != null) || orbitActive);
			Vector3 gunOrigin;
			if (useGhost)
			{
				Transform gh = null;
				try
				{
					bool isRight = arm == GTPlayer.Instance.RightHand.controllerTransform;
					gh = isRight ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform;
				}
				catch { }
				gunOrigin = gh != null ? gh.position : ghostRig.transform.position + Vector3.up * 0.2f;
			}
			else gunOrigin = (visualHand != null ? visualHand.position : arm.position);
			Vector3 gunDir;
			if (isFlatscreen && pcGunsEnabled && Mouse.current != null)
			{
				if (pcGunCamera == (Object)null)
				{
					GameObject shoulderCamObj = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if (shoulderCamObj != (Object)null)
					{
						pcGunCamera = shoulderCamObj.GetComponent<Camera>();
					}
					if (pcGunCamera == (Object)null)
					{
						shoulderCamObj = GameObject.Find("Shoulder Camera");
						if (shoulderCamObj != (Object)null)
						{
							pcGunCamera = shoulderCamObj.GetComponent<Camera>();
						}
					}
				}
				if (pcGunCamera != (Object)null)
				{
					Ray mouseRay = pcGunCamera.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
					gunDir = mouseRay.direction;
				}
				else
				{
					if (useGhost)
					{
						Transform gh2 = null;
						try { bool isR = arm == GTPlayer.Instance.RightHand.controllerTransform; gh2 = isR ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
						gunDir = gh2 != null ? -gh2.up : (visualHand != null ? -visualHand.up : -arm.up);
					}
					else gunDir = (visualHand != null ? -visualHand.up : -arm.up);
				}
				Physics.Raycast(gunOrigin, gunDir, out raycastHit, 512f, GetNoInvisLayerMask());
			}
			else
			{
				if (useGhost)
				{
					Transform gh3 = null;
					try { bool isR2 = arm == GTPlayer.Instance.RightHand.controllerTransform; gh3 = isR2 ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
					gunDir = gh3 != null ? -gh3.up : (visualHand != null ? -visualHand.up : -arm.up);
				}
				else gunDir = (visualHand != null ? -visualHand.up : -arm.up);
				Physics.Raycast(gunOrigin, gunDir, out raycastHit, 512f, GetNoInvisLayerMask());
			}
		if (pointer == (Object)null)
		{
			pointer = GameObject.CreatePrimitive(pointershape);
		}
		pointer.transform.localScale = pointersize;
		pointer.GetComponent<Renderer>().material.shader = ShaderCache.Uber;
		pointer.transform.position = raycastHit.point;
		pointer.GetComponent<Renderer>().material.color = color;
		pointer.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
			if (liner)
			{
				if (Line == (Object)null)
				{
					GameObject gunLineObj = new GameObject("GunLine");
					Line = gunLineObj.AddComponent<LineRenderer>();
				Line.material.shader = ShaderCache.Uber;
				Line.startWidth = linesize;
				Line.endWidth = linesize;
				Line.positionCount = 2;
				Line.useWorldSpace = true;
			}
		Line.startColor = Color.white;
		Line.endColor = Color.white;
			Line.material.color = color;
			Line.material.SetColor("_BaseColor", color);
				Vector3 lineStart;
				if (useGhost)
				{
					Transform gh4 = null;
					try { bool isR3 = arm == GTPlayer.Instance.RightHand.controllerTransform; gh4 = isR3 ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
					lineStart = gh4 != null ? gh4.position : ghostRig.transform.position + Vector3.up * 0.2f;
				}
				else lineStart = (visualHand != null ? visualHand.position : arm.position);
			Line.SetPosition(0, lineStart);
			Line.SetPosition(1, pointer.transform.position);
			float pulse = triggerHeld ? (1f + Mathf.Sin(Time.time * 12f) * 0.4f) : 1f;
			Line.startWidth = linesize * pulse;
			Line.endWidth = linesize * pulse;
			Line.positionCount = 2;
		}
		Object.Destroy(pointer.GetComponent<BoxCollider>());
		Object.Destroy(pointer.GetComponent<Rigidbody>());
		Object.Destroy(pointer.GetComponent<Collider>());
			if (triggerHeld && !gunTriggerWasDown)
			{
				try
				{
					onTrigger();
				}
				catch
				{
				}
			}
			else if (!triggerHeld)
			{
				try
				{
					onRelease();
				}
				catch
				{
				}
			}
		if (triggerHeld)
		{
			pointer.GetComponent<Renderer>().material.color = WristMenu.ButtonColorDisable;
		pointer.GetComponent<Renderer>().material.SetColor("_BaseColor", WristMenu.ButtonColorDisable);
		}
			gunTriggerWasDown = triggerHeld;
		}
		else
		{
			if (pointer != (Object)null)
			{
				Object.Destroy(pointer, Time.deltaTime);
				pointer = null;
			}
			if (Line != (Object)null)
			{
				Object.Destroy(((Component)Line).gameObject);
				Line = null;
			}
			gunTriggerWasDown = false;
		}
	}

	internal static void MakeRightHandGun(Action onTrigger, Action onRelease = null)
	{
		Transform arm = isRightHanded ? GTPlayer.Instance.LeftHand.controllerTransform : GTPlayer.Instance.RightHand.controllerTransform;
		MakeGun(WristMenu.ButtonColorEnabled, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, arm, liner: true, onTrigger, onRelease ?? delegate { });
	}

	internal static VRRig GetGunTargetPlayer()
	{
		if ((object)raycastHit.collider == null) return null;
		VRRig rig = raycastHit.collider.GetComponentInParent<VRRig>();
		return rig != null && rig.Creator != null ? rig : null;
	}

	private GameObject FreeCamObject;

	private bool thirdPersonViewActive;

	private bool xButtonWasDown;

	public static void CleanupGun()
	{
		if (pointer != (Object)null)
		{
			Object.Destroy(pointer, Time.deltaTime);
			pointer = null;
		}
		if (Line != (Object)null)
		{
			Object.Destroy(((Component)Line).gameObject);
			Line = null;
		}
		if (instance != null)
		{
			instance.gunTriggerWasDown = false;
		}
	}

	private VRRig tagGunLockedTarget = null;

	private int tagGunFramesUntilTag;

	private VRRig tagAllTarget;

	private int tagAllFramesUntilTag;

	private List<VRRig> tagAllTargets;

	private int tagAllIndex;

	public static void TagGun()
	{
		if (instance == null)
		{
			return;
		}
		instance.TagGunCore();
	}

	private void TagGunCore()
	{
		bool gripDown = isRightHanded ? WristMenu.gripDownL : WristMenu.gripDownR;
		if (!gripDown)
		{
			if (tagGunLockedTarget != null)
			{
				tagGunLockedTarget = null;
				UnsubscribeTagRigVisual();
				TryUnsubscribeGhostRig();
			}
			CleanupGun();
		}
		else
		{
			MakeRightHandGun(delegate
			{
				VRRig val3 = GetGunTargetPlayer();
				if (val3 != null && !val3.isLocal)
				{
				GorillaTagManager val5 = GorillaGameManager.instance as GorillaTagManager;
				if (val5 != null && !val5.IsInfected(val3.Creator))
				{
					tagGunLockedTarget = val3;
					tagGunFramesUntilTag = 12;
					SubscribeTagRigVisual();
					SubscribeGhostRig();
				}
				}
			}, delegate { });
			if (tagGunLockedTarget != null && pointer != null && Line != null)
			{
				pointer.transform.position = ((Component)tagGunLockedTarget).transform.position;
				Line.SetPosition(1, ((Component)tagGunLockedTarget).transform.position);
			}
		}
		GorillaGameManager val = GorillaGameManager.instance;
		GorillaTagManager val2 = (val is GorillaTagManager tgm) ? tgm : null;
		if (val2 == null || tagGunLockedTarget == null) return;
		if (tagGunLockedTarget.Creator == null || val2.IsInfected(tagGunLockedTarget.Creator))
		{
			tagGunLockedTarget = null;
			UnsubscribeTagRigVisual();
			TryUnsubscribeGhostRig();
			return;
		}
		tagGunFramesUntilTag--;
		if (tagGunFramesUntilTag <= 0)
		{
			tagGunFramesUntilTag = 12;
			GameMode.ReportTag(tagGunLockedTarget.Creator);
		}
	}

	public static void UntagSelf()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		if (!(val != (Object)null))
		{
			return;
		}
		GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
		if (instance != null && val2 != null && val2.IsInfected(NetworkSystem.Instance.LocalPlayer) && Time.time > instance.lastUntagSelfTime)
		{
			val2.currentInfected.RemoveAll((NetPlayer p) => p.UserId == NetworkSystem.Instance.LocalPlayer.UserId);
			instance.lastUntagSelfTime = Time.time + 0.3f;
			NotifiLib.SendNotification("Untagged self");
		}
	}

	public static void TagAll()
	{
		if (instance == null)
		{
			return;
		}
		instance.TagAllCore();
	}

	private void TagAllCore()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		GorillaTagManager val2 = (val is GorillaTagManager tgm) ? tgm : null;
		if (val2 == null) return;

		if (tagAllTarget == null || tagAllTarget.Creator == null || val2.IsInfected(tagAllTarget.Creator))
		{
			if (tagAllTargets == null || tagAllIndex >= tagAllTargets.Count)
			{
				tagAllTargets = new List<VRRig>();
				foreach (VRRig r in VRRigCache.ActiveRigs)
					if (!r.isLocal && r.Creator != null && !val2.IsInfected(r.Creator))
						tagAllTargets.Add(r);
				tagAllIndex = 0;
			}

			if (tagAllIndex >= tagAllTargets.Count)
				return;

			tagAllTarget = tagAllTargets[tagAllIndex];
			tagAllIndex++;
			tagAllFramesUntilTag = 30;
			SubscribeTagRigVisual();
			SubscribeGhostRig();
		}

		tagAllFramesUntilTag--;

		if (tagAllFramesUntilTag <= 0)
		{
			tagAllFramesUntilTag = 30;
			GameMode.ReportTag(tagAllTarget.Creator);
		}
	}

	public static void DisableTagAll()
	{
		if (instance == null)
		{
			return;
		}
		instance.tagAllTarget = null;
		instance.tagAllTargets = null;
		instance.tagAllIndex = 0;
		instance.tagAllFramesUntilTag = 0;
		instance.UnsubscribeTagRigVisual();
		instance.TryUnsubscribeGhostRig();
		if (VRRig.LocalRig != (Object)null)
			EnsureLocalRigEnabled();
	}

	public static void TeleportToSpawn()
	{
		GorillaTagger gt = GorillaTagger.Instance;
		if (gt == null) return;
		GTPlayer player = GTPlayer.Instance;
		if (player == null) return;
		Vector3 stump = instance != null ? instance.stumpPosition : new Vector3(-66.871f, 12.086f, -82.637f);
		Transform bodyT = gt.bodyCollider.transform;
		player.TeleportTo(stump - bodyT.position + player.transform.position, player.transform.rotation, true, false);
		bodyT.position = stump;
		if (VRRig.LocalRig != null)
			VRRig.LocalRig.transform.position = stump;
		((Collider)gt.bodyCollider).enabled = false;
		((MonoBehaviour)gt).StartCoroutine(ReenableBodyCollider());
	}

	private static IEnumerator ReenableBodyCollider()
	{
		yield return (object)new WaitForSeconds(1.5f);
		if (GorillaTagger.Instance != null)
			((Collider)GorillaTagger.Instance.bodyCollider).enabled = true;
	}

	public static void SpazAll()
	{
		if (instance != null)
		{
			instance.spazAllActive = true;
		}
	}

	public static void DisableSpazAll()
	{
		if (instance != null)
		{
			instance.spazAllActive = false;
		}
	}

	public static void SpazSelf()
	{
		if (instance != null)
		{
			instance.spazSelfActive = true;
		}
	}

	public static void DisableSpazSelf()
	{
		if (instance != null)
		{
			instance.spazSelfActive = false;
		}
	}

	private void RunSpaz()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		if (val == (Object)null)
		{
			return;
		}
		GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
		if (val2 == null || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (spazAllActive)
		{
			Player[] playerList = PhotonNetwork.PlayerList;
			for (int i = 0; i < playerList.Length; i++)
			{
				NetPlayer p = playerList[i];
				if (val2.isCurrentlyTag)
				{
					if (val2.currentIt == p)
					{
						val2.currentIt = null;
					}
					else if (val2.currentIt == null)
					{
						val2.currentIt = p;
					}
				}
				else if (val2.IsInfected(p))
				{
					val2.currentInfected.RemoveAll((NetPlayer x) => x.UserId == p.UserId);
				}
				else
				{
					val2.AddInfectedPlayer(p, true);
				}
			}
		}
		if (!spazSelfActive)
		{
			return;
		}
		NetPlayer self = NetworkSystem.Instance.LocalPlayer;
		if (val2.isCurrentlyTag)
		{
			if (val2.currentIt == self)
			{
				val2.currentIt = null;
			}
			else
			{
				val2.currentIt = self;
			}
		}
		else if (val2.IsInfected(self))
		{
			val2.currentInfected.RemoveAll((NetPlayer x) => x.UserId == self.UserId);
		}
		else
		{
			val2.AddInfectedPlayer(self, true);
		}
	}

	public static void UntagGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null)
			{
				GorillaGameManager gm = GorillaGameManager.instance;
				if (gm != null)
				{
					GorillaTagManager tagMan = gm as GorillaTagManager;
					if (instance != null && tagMan != null && tagMan.IsInfected(rig.Creator) && Time.time > instance.lastUntagNotif)
					{
						tagMan.currentInfected.RemoveAll(p => p.UserId == rig.Creator.UserId);
						instance.lastUntagNotif = Time.time + 0.3f;
						NotifiLib.SendNotification("Untagged " + rig.Creator.NickName);
					}
				}
			}
		});
	}

	public static void TagAura()
	{
		if (instance == null || tagAuraRange <= 0f) return;
		instance.TagAuraCore();
	}

	private void TagAuraCore()
	{
		if (Time.time < tagAuraCooldown) return;
		if (VRRig.LocalRig == (Object)null) return;
		GorillaGameManager gm = GorillaGameManager.instance;
		GorillaTagManager tgm = (gm is GorillaTagManager) ? (GorillaTagManager)(object)gm : null;
		if (tgm == null) return;
		Collider[] hits = Physics.OverlapSphere(VRRig.LocalRig.transform.position, tagAuraRange);
		foreach (Collider col in hits)
		{
			VRRig rig = col.GetComponentInParent<VRRig>();
			if (rig == null || rig.isLocal || rig.Creator == null || tgm.IsInfected(rig.Creator)) continue;
			GameMode.ReportTag(rig.Creator);
			tagAuraCooldown = Time.time + 0.1f;
		}
	}

	public static void DisableTagAura()
	{
		if (instance != null)
		{
			instance.tagAuraCooldown = 0f;
		}
	}

	public static void TagAuraVisual()
	{
		if (instance == null)
		{
			return;
		}
		instance.TagAuraVisualCore();
	}

	private void TagAuraVisualCore()
	{
		VRRig local = VRRig.LocalRig;
		if (local == null) return;
		CreateAuraRing();
		tagAuraRing.material.color = WristMenu.ButtonColorEnabled;
		Vector3 center = local.transform.position;
		for (int i = 0; i <= 32; i++)
		{
			float angle = (float)i / 32f * 360f * Mathf.Deg2Rad;
			Vector3 p = center + new Vector3(Mathf.Cos(angle) * tagAuraRange, 0f, Mathf.Sin(angle) * tagAuraRange);
			tagAuraRing.SetPosition(i, p);
		}
	}

	private void CreateAuraRing()
	{
		if (tagAuraRing != null) return;
		GameObject go = new GameObject("TagAuraRing");
		tagAuraRing = go.AddComponent<LineRenderer>();
		tagAuraRing.material = new Material(CachedUberShader);
		tagAuraRing.startWidth = 0.03f;
		tagAuraRing.endWidth = 0.03f;
		tagAuraRing.positionCount = 33;
		tagAuraRing.useWorldSpace = true;
	}

	public static void DisableTagAuraVisual()
	{
		if (instance != null && instance.tagAuraRing != null)
		{
			Object.Destroy(instance.tagAuraRing.gameObject);
			instance.tagAuraRing = null;
		}
	}

	public static void SetTagAuraRange(int index)
	{
		tagAuraRangeIndex = index % TagAuraRanges.Length;
		tagAuraRange = TagAuraRanges[tagAuraRangeIndex];
		NotifiLib.SendNotification("Range: " + tagAuraRange.ToString("0.0") + "m");
	}

	public static void BreakGuardian()
	{
		breakGuardianActive = true;
		if (instance == null)
		{
			return;
		}
		if (instance.breakGuardianHarmony == null)
		{
			instance.breakGuardianHarmony = new Harmony("chudmenu.breakguardian");
			instance.breakGuardianHarmony.Patch(
				typeof(GorillaGuardianZoneManager).GetMethod("SetGuardian", BindingFlags.Public | BindingFlags.Instance),
				prefix: new HarmonyMethod(typeof(GuardianBreakPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public))
			);
		}
		if (PhotonNetwork.IsMasterClient)
		{
			GorillaGuardianManager guardian = GorillaGameManager.instance as GorillaGuardianManager;
			if (guardian != null)
			{
				foreach (GorillaGuardianZoneManager zm in GorillaGuardianZoneManager.zoneManagers)
				{
					if (zm.CurrentGuardian != null && !zm.CurrentGuardian.IsLocal && !zm.IsPlayerGuardian(PhotonNetwork.LocalPlayer))
					{
						guardian.EjectGuardian(zm.CurrentGuardian);
					}
				}
			}
		}
	}

	public static void DisableBreakGuardian()
	{
		breakGuardianActive = false;
		if (instance != null && instance.breakGuardianHarmony != null)
		{
			instance.breakGuardianHarmony.UnpatchSelf();
			instance.breakGuardianHarmony = null;
		}
	}

	public static void GuardianSelf()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			NotifiLib.SendNotification("You are not master client!");
			return;
		}
		NetPlayer local = PhotonNetwork.LocalPlayer;
		foreach (GorillaGuardianZoneManager zm in GorillaGuardianZoneManager.zoneManagers)
		{
			zm.SetGuardian(local);
		}

	}

	public static void UnguardianSelf()
	{
		GorillaGuardianManager guardian = GorillaGameManager.instance as GorillaGuardianManager;
		if (guardian == null) return;
		NetPlayer local = PhotonNetwork.LocalPlayer;
		if (!guardian.IsPlayerGuardian(local)) return;
		if (PhotonNetwork.IsMasterClient)
		{
			guardian.EjectGuardian(local);
		}
		else
		{
			guardian.RequestEjectGuardian(local);
		}
	}

	public static void GuardianGun()
	{
		MakeRightHandGun(delegate
		{
			if (instance == null || Time.time < instance.lastGuardianGunTime) return;
			VRRig rig = GetGunTargetPlayer();
			if (rig == null || rig.isLocal || rig.Creator == null) return;
			foreach (GorillaGuardianZoneManager zm in GorillaGuardianZoneManager.zoneManagers)
			{
				zm.SetGuardian(rig.Creator);
			}
			instance.lastGuardianGunTime = Time.time + 0.3f;
		});
	}

	public static void UnguardianGun()
	{
		MakeRightHandGun(delegate
		{
			if (instance == null || Time.time < instance.lastUnguardianGunTime) return;
			VRRig rig = GetGunTargetPlayer();
			if (rig == null || rig.Creator == null) return;
			GorillaGuardianManager guardian = GorillaGameManager.instance as GorillaGuardianManager;
			if (guardian == null) return;
			if (!guardian.IsPlayerGuardian(rig.Creator)) return;
			if (PhotonNetwork.IsMasterClient)
			{
				guardian.EjectGuardian(rig.Creator);
			}
			else
			{
				guardian.RequestEjectGuardian(rig.Creator);
			}
			instance.lastUnguardianGunTime = Time.time + 0.3f;
		});
	}

	public static void GuardianSpazGun()
	{
		if (instance == null)
		{
			return;
		}
		instance.GuardianSpazGunCore();
	}

	private void GuardianSpazGunCore()
	{
		bool gripDown = isRightHanded ? WristMenu.gripDownL : WristMenu.gripDownR;
		if (!gripDown)
		{
			guardianSpazTarget = null;
			CleanupGun();
			return;
		}
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				guardianSpazTarget = rig;
			}
		}, delegate { });
		if (guardianSpazTarget == null || guardianSpazTarget.Creator == null) return;
		if (pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)guardianSpazTarget).transform.position;
			Line.SetPosition(1, ((Component)guardianSpazTarget).transform.position);
		}
		if (Time.time < guardianSpazTimer) return;
		guardianSpazTimer = Time.time + 0.15f;
		GorillaGuardianManager guardian = GorillaGameManager.instance as GorillaGuardianManager;
		if (guardian == null) return;
		if (guardian.IsPlayerGuardian(guardianSpazTarget.Creator))
		{
			guardian.EjectGuardian(guardianSpazTarget.Creator);
		}
		else
		{
			foreach (GorillaGuardianZoneManager zm in GorillaGuardianZoneManager.zoneManagers)
			{
				zm.SetGuardian(guardianSpazTarget.Creator);
			}
		}
	}

	public static void PaintBrawlKillAll()
	{
		GorillaGameManager gm = GorillaGameManager.instance;
		GorillaPaintbrawlManager pb = gm as GorillaPaintbrawlManager;
		if (pb == null || !NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			NetPlayer p = playerList[i];
			if (p.IsLocal) continue;
			try { pb.HitPlayer(p); } catch { }
		}
	}

	public static void PaintBrawlKillGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig == null || rig.isLocal || rig.Creator == null) return;
			GorillaPaintbrawlManager pb = GorillaGameManager.instance as GorillaPaintbrawlManager;
			if (pb == null || !NetworkSystem.Instance.IsMasterClient) return;
			pb.HitPlayer(rig.Creator);
		});
	}

	public static void AntiNameBan()
	{
		if (instance != null)
		{
			instance.antiNameBanApplied = true;
		}
		BanPatchState.enabled = true;
	}

	public static void DisableAntiNameBan()
	{
		if (instance == null)
		{
			return;
		}
		if (instance.antiNameBanApplied)
		{
			BanPatchState.enabled = false;
			instance.antiNameBanApplied = false;
		}
	}

	public static void BitcrunchMic()
	{
		if (instance == null || instance.bitcrunchMicActive)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (!(myRecorder == (Object)null))
		{
			instance.bitcrunchOrigSampleRate = (int)myRecorder.SamplingRate;
			instance.bitcrunchOrigBitrate = myRecorder.Bitrate;
			myRecorder.SamplingRate = (SamplingRate)8000;
			myRecorder.Bitrate = 8000;
			myRecorder.RestartRecording(true);
			instance.bitcrunchMicActive = true;
		}
	}

	public static void DisableBitcrunchMic()
	{
		if (instance == null || !instance.bitcrunchMicActive)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (myRecorder != (Object)null)
		{
			myRecorder.SamplingRate = (SamplingRate)instance.bitcrunchOrigSampleRate;
			myRecorder.Bitrate = instance.bitcrunchOrigBitrate;
			myRecorder.RestartRecording(true);
		}
		instance.bitcrunchMicActive = false;
	}

	public static void Boop()
	{
		if (instance != null)
		{
			instance.boopActive = true;
		}
	}

	public static void DisableBoop()
	{
		if (instance == null)
		{
			return;
		}
		instance.boopActive = false;
		instance.boopCooldown = 0f;
	}

	private void UpdateBoop()
	{
		if (!boopActive)
		{
			return;
		}
		if (boopCooldown > 0f)
		{
			boopCooldown -= Time.deltaTime;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (!activeRig.isLocal && !(activeRig.headMesh == (Object)null))
			{
				float num = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, activeRig.headMesh.transform.position);
				float num2 = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, activeRig.headMesh.transform.position);
				if (!flag && num < 0.275f)
				{
					flag = true;
				}
				if (!flag2 && num2 < 0.275f)
				{
					flag2 = true;
				}
			}
		}
		if (flag && !boopLastL)
		{
			VRRig.LocalRig.PlayHandTapLocal(84, true, 999999f);
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, new object[3] { 84, true, 999999f });
			boopCooldown = 0.05f;
		}
		if (flag2 && !boopLastR)
		{
			VRRig.LocalRig.PlayHandTapLocal(84, false, 999999f);
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, new object[3] { 84, false, 999999f });
			boopCooldown = 0.05f;
		}
		boopLastL = flag;
		boopLastR = flag2;
	}

	public static void RandomColorSpaz()
	{
		if (instance == null)
		{
			return;
		}
		instance.randomColorSpazTick++;
		if (instance.randomColorSpazTick % 15 == 0)
		{
			float num = Random.Range(0.15f, 0.95f);
			float num2 = Random.Range(0.15f, 0.95f);
			float num3 = Random.Range(0.15f, 0.95f);
			if (VRRig.LocalRig != null) VRRig.LocalRig.InitializeNoobMaterialLocal(num, num2, num3);
			if (GorillaTagger.Instance != null && GorillaTagger.Instance.myVRRig != null)
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[3] { num, num2, num3 });
		}
	}

	public static void DisableRandomColorSpaz()
	{
		float r = PlayerPrefs.GetFloat("redValue", 1f);
		float g = PlayerPrefs.GetFloat("greenValue", 1f);
		float b = PlayerPrefs.GetFloat("blueValue", 1f);
		r = Mathf.Clamp01(r); g = Mathf.Clamp01(g); b = Mathf.Clamp01(b);
		if (instance != null)
		{
			instance.randomColorSpazTick = 0;
		}
		try
		{
			Color c = new Color(r, g, b, 1f);
			if (VRRig.LocalRig != null)
			{
				VRRig.LocalRig.InitializeNoobMaterialLocal(r, g, b);
				VRRig.LocalRig.SetColor(c);
				if (VRRig.LocalRig.bodyRenderer != null) VRRig.LocalRig.bodyRenderer.UpdateColor(c);
				if (VRRig.LocalRig.mainSkin != null) VRRig.LocalRig.mainSkin.material.color = c;
			}
			if (GorillaTagger.Instance != null)
			{
				if (GorillaTagger.Instance.myVRRig != null) GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[3] { r, g, b });
				if (GorillaTagger.Instance.offlineVRRig != null)
				{
					GorillaTagger.Instance.offlineVRRig.InitializeNoobMaterialLocal(r, g, b);
					GorillaTagger.Instance.offlineVRRig.SetColor(c);
					if (GorillaTagger.Instance.offlineVRRig.bodyRenderer != null) GorillaTagger.Instance.offlineVRRig.bodyRenderer.UpdateColor(c);
					if (GorillaTagger.Instance.offlineVRRig.mainSkin != null) GorillaTagger.Instance.offlineVRRig.mainSkin.material.color = c;
				}
			}
		}
		catch { }
	}

	public static void WaterSplash()
	{
		if (instance == null)
		{
			return;
		}
		instance.SpawnSplash();
	}

	public static void DisableWaterSplash() { }

	public static void SetWaterSplashSpeed(int index)
	{
		waterSplashSpeedIndex = index % WaterSplashCooldowns.Length;
		NotifiLib.SendNotification("Water splash speed: " + WaterSplashNames[waterSplashSpeedIndex]);
	}

	private void SpawnSplash()
	{
		if (Time.time < splashCooldown) return;
		bool right = WristMenu.gripDownR;
		bool left = WristMenu.gripDownL;
		if (!right && !left) return;
		if (GorillaTagger.Instance == null || GorillaTagger.Instance.myVRRig == null) return;
		splashCooldown = Time.time + WaterSplashCooldowns[waterSplashSpeedIndex % WaterSplashCooldowns.Length];
		if (ObjectPools.instance == null) return;
		Transform hand = right ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
		if (hand == null) return;
		Vector3 pos = hand.position;
		Quaternion rot = hand.rotation;
		float scale = Mathf.Clamp(1f, 1E-05f, 1f);
		float bound = Mathf.Clamp(0.5f, 0.0001f, 0.5f);
		if (GTPlayer.Instance == null || GTPlayer.Instance.waterParams == null) return;
		GameObject splashFx = ObjectPools.instance.Instantiate(GTPlayer.Instance.waterParams.splashEffect, pos, rot, scale, true);
		if (splashFx != null)
			splashFx.GetComponent<WaterSplashEffect>().PlayEffect(true, false, scale);
		GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", RpcTarget.Others, new object[]
		{
			pos, rot, scale, bound, true, false
		});
	}

	public static void SetButtonClickSound(int index)
	{
		WristMenu.ApplyButtonClickSound(index);
		NotifiLib.SendNotification("Button click: " + WristMenu.ButtonClickNames[WristMenu.buttonClickIndex]);
		Save();
	}

	public static void UnlockAllCosmetics()
	{
		CosmeticsController val = CosmeticsController.instance;
		if (val == (Object)null || !val.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		foreach (CosmeticsController.CosmeticItem allCosmetic in val.allCosmetics)
		{
			if (!string.IsNullOrEmpty(allCosmetic.itemName) && !val.IsOwnedByPlayFabID(allCosmetic.itemName))
			{
				try
				{
					val.ProcessExternalUnlock(allCosmetic.itemName, false, false);
				}
				catch
				{
				}
			}
		}
	}

	public static void EnableTryOnAll()
	{
		if (instance == null || instance.tryOnAllActive) return;
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		instance.tryOnAllActive = true;
		instance.tryOnAllSavedWorn = new CosmeticsController.CosmeticItem[16];
		for (int i = 0; i < 16; i++)
		{
			instance.tryOnAllSavedWorn[i] = controller.currentWornSet.items[i];
		}
		CosmeticsController.CosmeticItem treePin = controller.GetItemFromDict(treePinCosmeticId);
		if (treePin.isNullItem)
		{
			treePin = controller.nullItem;
		}
		for (int i = 0; i < 16; i++)
		{
			controller.currentWornSet.items[i] = treePin;
		}
		controller.UpdateWornCosmetics(true);
		instance.tryOnAllCoroutine = instance.StartCoroutine(TryOnAllCycleRoutine());
	}

	public static void DisableTryOnAll()
	{
		if (instance == null || !instance.tryOnAllActive) return;
		instance.tryOnAllActive = false;
		if (instance.tryOnAllCoroutine != null)
		{
			instance.StopCoroutine(instance.tryOnAllCoroutine);
			instance.tryOnAllCoroutine = null;
		}
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null)
		{
			return;
		}
		if (instance.tryOnAllSavedWorn != null)
		{
			for (int i = 0; i < 16; i++)
			{
				if (controller.currentWornSet.items[i].itemName == treePinCosmeticId)
				{
					controller.currentWornSet.items[i] = instance.tryOnAllSavedWorn[i];
				}
			}
			instance.tryOnAllSavedWorn = null;
			controller.UpdateWornCosmetics(true);
		}
		controller.tryOnSet.ClearSet(controller.nullItem);
		controller.UpdateWornCosmetics(true);
	}

	private static IEnumerator TryOnAllCycleRoutine()
	{
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			yield break;
		}
		List<CosmeticsController.CosmeticItem> items = BuildTryOnFilteredList(controller);
		if (items.Count == 0)
		{
			FindAndToggleButton(tryOnAllButtonId);
			yield break;
		}
		for (int i = 0; i < items.Count; i++)
		{
			VRRig localRig = VRRig.LocalRig;
			if (localRig == (Object)null)
			{
				break;
			}
			CosmeticsController.CosmeticItem currentItem = items[i];
			try
			{
				controller.tryOnSet.ClearSet(controller.nullItem);
				controller.ApplyCosmeticItemToSet(controller.tryOnSet, currentItem, false, false);
				MakeCosmeticOwned(localRig, currentItem.itemName);
				controller.UpdateWornCosmetics(true);
			}
			catch (Exception e)
			{
				Debug.LogError("[Chud] TryOnAll: " + e.Message);
			}
			yield return new WaitForSeconds(0.05f);
		}
		FindAndToggleButton(tryOnAllButtonId);
	}

	private static List<CosmeticsController.CosmeticItem> BuildTryOnFilteredList(CosmeticsController controller)
	{
		List<CosmeticsController.CosmeticItem> items = new List<CosmeticsController.CosmeticItem>();
		foreach (CosmeticsController.CosmeticItem item in controller.allCosmetics)
		{
			if (item.isNullItem) continue;
			string itemName = item.itemName;
			if (string.IsNullOrEmpty(itemName) || itemName == "null" || itemName == "Slingshot" || itemName == treePinCosmeticId) continue;
			if (item.itemCategory == CosmeticsController.CosmeticCategory.Collectable) continue;
			if (item.itemCategory == CosmeticsController.CosmeticCategory.Face) continue;
			if (item.itemCategory == CosmeticsController.CosmeticCategory.Paw) continue;
			if (item.cost <= 0) continue;
			items.Add(item);
		}
		return items;
	}

	private static void MakeCosmeticOwned(VRRig rig, string itemName)
	{
		if (rig == (Object)null || string.IsNullOrEmpty(itemName)) return;
		if (instance == null)
		{
			return;
		}
		if (instance.cachedAddCosmeticMethod == null)
		{
			instance.cachedAddCosmeticMethod = AccessTools.Method(rig.GetType(), "AddCosmetic");
			if (instance.cachedAddCosmeticMethod == null) return;
			instance.cachedAddCosmeticParameters = instance.cachedAddCosmeticMethod.GetParameters();
		}
		object[] args = new object[instance.cachedAddCosmeticParameters.Length];
		args[0] = itemName;
		for (int i = 1; i < args.Length; i++)
		{
			args[i] = Type.Missing;
		}
		instance.cachedAddCosmeticMethod.Invoke(rig, args);
	}

	public static void EnableRemoveAllCosmetics()
	{
		if (instance == null || instance.removeAllActive) return;
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		instance.removeAllActive = true;
		instance.removeAllCoroutine = instance.StartCoroutine(RemoveAllCycleRoutine());
	}

	public static void DisableRemoveAllCosmetics()
	{
		if (instance == null || !instance.removeAllActive) return;
		instance.removeAllActive = false;
		if (instance.removeAllCoroutine != null)
		{
			instance.StopCoroutine(instance.removeAllCoroutine);
			instance.removeAllCoroutine = null;
		}
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null)
		{
			return;
		}
		controller.tryOnSet.ClearSet(controller.nullItem);
		controller.UpdateWornCosmetics(true);
	}

	private static IEnumerator RemoveAllCycleRoutine()
	{
		CosmeticsController controller = CosmeticsController.instance;
		if (controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			yield break;
		}
		List<CosmeticsController.CosmeticItem> items = BuildTryOnFilteredList(controller);
		if (items.Count == 0)
		{
			FindAndToggleButton(removeAllButtonId);
			yield break;
		}
		for (int i = 0; i < items.Count; i++)
		{
			CosmeticsController.CosmeticItem currentItem = items[i];
			try
			{
				controller.tryOnSet.ClearSet(controller.nullItem);
				controller.ApplyCosmeticItemToSet(controller.tryOnSet, currentItem, false, false);
				controller.UpdateWornCosmetics(true);
			}
			catch (Exception e)
			{
				Debug.LogError("[Chud] RemoveAll: " + e.Message);
			}
			yield return new WaitForSeconds(0.05f);
			try
			{
				controller.tryOnSet.ClearSet(controller.nullItem);
				controller.UpdateWornCosmetics(true);
			}
			catch (Exception e)
			{
				Debug.LogError("[Chud] RemoveAll: " + e.Message);
			}
			yield return new WaitForSeconds(0.05f);
		}
		FindAndToggleButton(removeAllButtonId);
	}

	public static void FindAndToggleButton(string buttonId)
	{
		foreach (MenuCategory category in MenuManager.Instance.Categories)
		{
			ButtonInfo buttonInfo = category.Buttons.Find((ButtonInfo b) => b.id == buttonId && b.enabled.HasValue && b.type != ButtonType.Action);
			if (buttonInfo != null)
			{
			bool value = buttonInfo.enabled.Value;
			buttonInfo.enabled = !value;
			InvalidateActiveButtonsCache();
			if (buttonInfo.enabled == true)
				{
					if (buttonInfo.enableMethod != null)
						buttonInfo.enableMethod();
					else
						buttonInfo.method?.Invoke();
				}
				else if (buttonInfo.disableMethod != null)
				{
					buttonInfo.disableMethod();
				}
				WristMenu.UpdateButtonVisual(buttonInfo.id, buttonInfo.buttonText, buttonInfo.enabled.Value);
				Save();
				break;
			}
		}
	}

	private static string soundboardBasePath = Path.Combine(new string[]
	{
		Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "Chud Menu", "Sounds"
	});

	public static List<ButtonInfo> BuildSoundboardCategory()
	{
		Directory.CreateDirectory(soundboardBasePath);
		List<ButtonInfo> buttons = new List<ButtonInfo>
		{
			new ButtonInfo
			{
				id = "soundboard_exit",
				buttonText = "Exit Soundboard",
				method = delegate
				{
					MenuManager.Instance.ToggleCategory("Soundboard");
				},
				enabled = false,
				type = ButtonType.Action,
				toolTip = "Go to Main"
			}
		};
		if (Directory.Exists(soundboardBasePath))
		{
			string[] files = Directory.GetFiles(soundboardBasePath);
			foreach (string text in files)
			{
				string text2 = text;
				string text3 = Path.GetFileNameWithoutExtension(text2);
				string fileName = text3;
				buttons.Add(new ButtonInfo
				{
					id = "soundboard_file_" + SanitizeSoundboardId(fileName),
					buttonText = fileName,
					enableMethod = delegate
					{
						SoundboardStop();
						SoundboardPlay(text2);
						NotifiLib.SendNotification(fileName);
					},
					disableMethod = SoundboardStop,
					enabled = false,
					requiresLobby = true,
					toolTip = ""
				});
			}
		}
		EnsureSoundboardPreload();
		return buttons;
	}

	private static string SanitizeSoundboardId(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return "unnamed";
		}
		StringBuilder id = new StringBuilder(fileName.Length);
		foreach (char c in fileName.ToLowerInvariant())
		{
			if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
			{
				id.Append(c);
			}
			else if (id.Length > 0 && id[id.Length - 1] != '_')
			{
				id.Append('_');
			}
		}
		if (id.Length == 0)
		{
			return "unnamed";
		}
		return id.ToString().TrimEnd('_');
	}

	private AudioClip _soundboardClip;

	private readonly Dictionary<string, AudioClip> soundboardCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

	private bool soundboardPreloadStarted = false;

	private static void EnsureSoundboardPreload()
	{
		if (instance == null || instance.soundboardPreloadStarted) return;
		instance.soundboardPreloadStarted = true;
		instance.StartCoroutine(instance.PreloadSoundboard());
	}

	private IEnumerator PreloadSoundboard()
	{
		if (!Directory.Exists(soundboardBasePath)) yield break;
		string[] files = null;
		try { files = Directory.GetFiles(soundboardBasePath); } catch { yield break; }
		if (files == null) yield break;
		foreach (string path in files)
		{
			if (soundboardCache.ContainsKey(path)) continue;
			try { if (new FileInfo(path).Length > 10485760L) continue; } catch { continue; }
			string ext = "";
			try { ext = Path.GetExtension(path).ToLower(); } catch { continue; }
			if (ext != ".ogg" && ext != ".wav" && ext != ".mp3") continue;
			AudioClip clip = null;
			UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + path.Replace("\\", "/"), ext == ".wav" ? AudioType.WAV : (ext == ".mp3" ? AudioType.MPEG : AudioType.OGGVORBIS));
			try
			{
				yield return req.SendWebRequest();
				if ((int)req.result == 1)
				{
					try { clip = DownloadHandlerAudioClip.GetContent(req); } catch { }
					if (clip != (Object)null)
						soundboardCache[path] = clip;
				}
			}
			finally
			{
				((IDisposable)req)?.Dispose();
			}
			yield return null;
		}
	}

	private static void SoundboardPlay(string path)
	{
		if (!PhotonNetwork.InRoom)
		{
			NotifiLib.SendNotification("You can only play sounds inside a lobby");
			return;
		}
		EnsureSoundboardPreload();
		if (instance == null)
		{
			return;
		}
		if (instance.soundboardCache.TryGetValue(path, out AudioClip cached) && cached != (Object)null)
		{
			Recorder instantRecorder = (GorillaTagger.Instance != (Object)null) ? GorillaTagger.Instance.myRecorder : null;
			if (instantRecorder != null)
			{
				instantRecorder.SourceType = Recorder.InputSourceType.AudioClip;
				instantRecorder.AudioClip = cached;
				instantRecorder.RestartRecording(true);
				instantRecorder.DebugEchoMode = true;
			}
			return;
		}
		instance.StartCoroutine(instance.SoundboardLoadAndPlay(path));
	}

	private IEnumerator SoundboardLoadAndPlay(string path)
	{
		AudioType audioType = AudioType.OGGVORBIS;
		string a = Path.GetExtension(path).ToLower();
		if (a == ".wav")
		{
			audioType = AudioType.WAV;
		}
		else if (a == ".mp3")
		{
			audioType = AudioType.MPEG;
		}
		string text = "file:///" + path.Replace("\\", "/");
		UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(text, audioType);
		try
		{
			yield return unityWebRequest.SendWebRequest();
			if ((int)unityWebRequest.result == 1)
			{
				AudioClip audioClip = DownloadHandlerAudioClip.GetContent(unityWebRequest);
				try { soundboardCache[path] = audioClip; } catch { }
				Recorder myRecorder = GorillaTagger.Instance.myRecorder;
				if (myRecorder != null)
				{
					if (_soundboardClip != (Object)null)
						Object.Destroy(_soundboardClip);
					_soundboardClip = audioClip;
					myRecorder.SourceType = Recorder.InputSourceType.AudioClip;
					myRecorder.AudioClip = audioClip;
					myRecorder.RestartRecording(true);
					myRecorder.DebugEchoMode = true;
				}
			}
		}
		finally
		{
			((IDisposable)unityWebRequest)?.Dispose();
		}
	}

	private static void SoundboardStop()
	{
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (myRecorder != null)
		{
			myRecorder.SourceType = Recorder.InputSourceType.Microphone;
			myRecorder.AudioClip = null;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = false;
		}
		if (instance != null && instance._soundboardClip != (Object)null)
		{
			Object.Destroy(instance._soundboardClip);
			instance._soundboardClip = null;
		}
	}

	public static void EnableBackflip()
	{
		if (instance != null)
		{
			instance.backflipEnabled = true;
		}
	}

	public static void DisableBackflip()
	{
		if (instance == null)
		{
			return;
		}
		instance.backflipEnabled = false;
		instance.backflipActive = false;
	}

	public static void EnableFrontflip()
	{
		if (instance != null)
		{
			instance.frontflipEnabled = true;
		}
	}

	public static void DisableFrontflip()
	{
		if (instance == null)
		{
			return;
		}
		instance.frontflipEnabled = false;
		instance.frontflipActive = false;
	}

	public static void EnableSpinningTorso()
	{
		if (instance != null)
		{
			instance.spinningTorsoEnabled = true;
		}
	}

	public static void DisableSpinningTorso()
	{
		if (instance != null)
		{
			instance.spinningTorsoEnabled = false;
		}
	}

	public static void EnableFakeFBT()
	{
		if (instance != null)
		{
			instance.fakeFBTEnabled = true;
		}
	}

	public static void DisableFakeFBT()
	{
		if (instance != null)
		{
			instance.fakeFBTEnabled = false;
		}
	}

	public static void EnableDinnerbone()
	{
		if (instance != null)
		{
			instance.dinnerboneEnabled = true;
		}
	}

	public static void DisableDinnerbone()
	{
		if (instance != null)
		{
			instance.dinnerboneEnabled = false;
		}
	}

	public static void EnableNatsukiNeck()
	{
		if (instance == null)
		{
			return;
		}
		instance.natsukiNeckEnabled = true;
		instance.natsukiHasSaved = false;
		VRRig rig = VRRig.LocalRig;
		if (rig != null && rig.head != null && rig.head.rigTarget != (Object)null)
		{
			instance.natsukiSavedPos = rig.head.rigTarget.transform.position;
			instance.natsukiSavedRot = rig.head.rigTarget.transform.rotation;
			instance.natsukiHasSaved = true;
		}
	}

	public static void DisableNatsukiNeck()
	{
		if (instance == null)
		{
			return;
		}
		instance.natsukiNeckEnabled = false;
		if (instance.natsukiHasSaved)
		{
			instance.natsukiHasSaved = false;
			VRRig rig = VRRig.LocalRig;
			if (rig != null && rig.head != null && rig.head.rigTarget != (Object)null)
				rig.head.rigTarget.transform.SetPositionAndRotation(instance.natsukiSavedPos, instance.natsukiSavedRot);
		}
	}

	public static void EnableSpiderMonkey()
	{
		if (instance == null)
		{
			return;
		}
		instance.spiderMonkeyEnabled = true;
		instance.spiderMonkeyRot = Quaternion.identity;
		instance.spiderMonkeyTargetRot = Quaternion.identity;
	}

	public static void DisableSpiderMonkey()
	{
		if (instance == null)
		{
			return;
		}
		instance.spiderMonkeyEnabled = false;
		GTPlayer.Instance.UnsetGravityOverride(GTPlayer.Instance);
		GTPlayerTransform.ApplyRotationOverride(Quaternion.identity, Time.frameCount);
	}

	public static void LagGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					if (instance == null)
					{
						return;
					}
					instance.lagGunLockedTarget = rig;
					instance.lagGunTargetActor = player.ActorNumber;
					if (!instance.lagGunRunning)
					{
						instance.lagGunRunning = true;
						instance.StartCoroutine(instance.LagGunLoop());
					}
				}
			}
		}, delegate
		{
			StopLagGun();
		});
		if (instance != null && instance.lagGunLockedTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)instance.lagGunLockedTarget).transform.position;
			Line.SetPosition(1, ((Component)instance.lagGunLockedTarget).transform.position);
		}
	}

	public static void StopLagGun()
	{
		if (instance == null)
		{
			return;
		}
		instance.lagGunRunning = false;
		instance.lagGunTargetActor = -1;
		instance.lagGunLockedTarget = null;
	}

	public static void StopLagGunFull()
	{
		StopLagGun();
		CleanupGun();
	}

	private IEnumerator LagGunLoop()
	{
		RaiseEventOptions opts = new RaiseEventOptions
		{
			TargetActors = new int[] { lagGunTargetActor }
		};
		while (lagGunRunning)
		{
			if (!lagGunRunning || pointer == null || !(isRightHanded ? WristMenu.triggerDownL : WristMenu.triggerDownR))
			{
				StopLagGun();
				yield break;
			}
			for (int i = 0; i < 340; i++)
				PhotonNetwork.RaiseEvent(3, lagPayload, opts, SendOptions.SendUnreliable);
			yield return new WaitForSeconds(1.2f);
			for (int i = 0; i < 340; i++)
				PhotonNetwork.RaiseEvent(3, lagPayload, opts, SendOptions.SendUnreliable);
			yield return new WaitForSeconds(1.2f);
		}
	}

	public static void CopyMovementGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null && !rig.isLocal)
			{
				if (instance == null)
				{
					return;
				}
				instance.copyMovementTarget = rig;
				instance.copyMovementActive = true;
				instance.SubscribeGhostRig();
			}
		}, delegate
		{
			StopCopyMovementGun();
		});
		if (instance != null && instance.copyMovementTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)instance.copyMovementTarget).transform.position;
			Line.SetPosition(1, ((Component)instance.copyMovementTarget).transform.position);
		}
	}

	public static void StopCopyMovementGun()
	{
		if (instance == null)
		{
			return;
		}
		if (instance.copyMovementActive && VRRig.LocalRig != (Object)null)
		{
			EnsureLocalRigEnabled();
		}
		instance.copyMovementActive = false;
		instance.copyMovementTarget = null;
		instance.TryUnsubscribeGhostRig();
	}

	public static void StopCopyMovementGunFull()
	{
		StopCopyMovementGun();
		CleanupGun();
	}

	public static void OrbitGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null && !rig.isLocal)
			{
				if (instance == null)
				{
					return;
				}
				instance.orbitTarget = rig;
				instance.orbitActive = true;
				instance.orbitAngle = 0f;
				instance.SubscribeGhostRig();
			}
		}, delegate { StopOrbit(); });
		if (instance != null && instance.orbitTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)instance.orbitTarget).transform.position;
			Line.SetPosition(1, ((Component)instance.orbitTarget).transform.position);
		}
	}

	public static void StopOrbit()
	{
		if (instance == null)
		{
			return;
		}
		if (instance.orbitActive && VRRig.LocalRig != (Object)null) EnsureLocalRigEnabled();
		instance.orbitActive = false;
		instance.orbitTarget = null;
		instance.TryUnsubscribeGhostRig();
	}

	public static void StopOrbitFull()
	{
		StopOrbit();
		CleanupGun();
	}

	public static void EnableThirdPerson()
	{
		if (instance == null)
		{
			return;
		}
		instance.EnableThirdPersonCore();
	}

	private void EnableThirdPersonCore()
	{
		thirdPersonEnabled = true;
		if (FreeCamObject == (Object)null)
		{
			FreeCamObject = new GameObject("Chud_CameraObj");
			FreeCamObject.transform.position = GorillaTagger.Instance.headCollider.transform.position;
			Camera val = FreeCamObject.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = CameraType.Game;
		}
		FreeCamObject.transform.position = GorillaTagger.Instance.bodyCollider.transform.TransformPoint(new Vector3(0f, 0.5f, -1.5f));
		FreeCamObject.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
	}

	public static void DisableThirdPerson()
	{
		thirdPersonEnabled = false;
		if (instance != null)
		{
			instance.thirdPersonViewActive = false;
			instance.DisableThirdPersonView();
		}
	}

	private void DisableThirdPersonView()
	{
		if (FreeCamObject != (Object)null)
		{
			Object.Destroy(FreeCamObject.GetComponent<Camera>());
			Object.Destroy(FreeCamObject);
			FreeCamObject = null;
		}
	}

	public static void BlockJmanSounds()
	{
		blockJmanSounds = true;
		JmanSoundPatch.enabled = true;
	}

	public static void DisableBlockJmanSounds()
	{
		blockJmanSounds = false;
		JmanSoundPatch.enabled = false;
	}

	public static void AntiGuardianGrab()
	{
		antiGuardianGrab = true;
		GuardianPatches.launched = true;
		GuardianPatches.knockedBack = true;
		GuardianPatches.clampedKnockback = true;
		GuardianPatches.trajectoryOverridden = true;
		GuardianPatches.grabbedBy = true;
	}

	public static void DisableAntiGuardianGrab()
	{
		antiGuardianGrab = false;
		GuardianPatches.launched = false;
		GuardianPatches.knockedBack = false;
		GuardianPatches.clampedKnockback = false;
		GuardianPatches.trajectoryOverridden = false;
		GuardianPatches.grabbedBy = false;
	}

	public static void AntiBlockCrash()
	{
		antiBlockCrash = true;
	}

	public static void DisableAntiBlockCrash()
	{
		antiBlockCrash = false;
		try
		{
			if (GorillaTagScripts.BuilderTable.TryGetBuilderTableForZone(GorillaTagScripts.BuilderTable.BUILDER_ZONE, out var table))
			{
				if (table.builderRenderer != null) table.builderRenderer.Show(true);
			}
		}
		catch { }
	}

	private void AntiBlockCrashTick()
	{
		if (!antiBlockCrash) return;
		try
		{
			if (!GorillaTagScripts.BuilderTable.TryGetBuilderTableForZone(GorillaTagScripts.BuilderTable.BUILDER_ZONE, out var table)) return;
			if (table.pieces == null || table.pieces.Count == 0) return;
			foreach (BuilderPiece p in table.pieces)
			{
				if (p == null || !p.gameObject.activeSelf || p.isBuiltIntoTable) continue;
				try { table.builderRenderer.RemovePiece(p); } catch { }
				p.gameObject.SetActive(false);
				if (p.rigidBody != null) Object.Destroy(p.rigidBody);
				if (p.colliders != null)
				{
					for (int c = 0; c < p.colliders.Count; c++)
					{
						Collider col = p.colliders[c];
						if (col != null) col.enabled = false;
					}
				}
			}
		}
		catch { }
	}
}