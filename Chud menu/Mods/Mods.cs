using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Pointer = UnityEngine.InputSystem.Pointer;
using Random = UnityEngine.Random;

namespace Chud.Backend;

internal class Mods : MonoBehaviour
{
	#region Fields and Constants
	private struct TransformSnapshot
	{
		public Vector3 headPos;

		public Quaternion headRot;

		public Vector3 leftHandPos;

		public Quaternion leftHandRot;

		public Vector3 rightHandPos;

		public Quaternion rightHandRot;

		public float leftIndexT;

		public float leftMiddleT;

		public float leftThumbT;

		public float rightIndexT;

		public float rightMiddleT;

		public float rightThumbT;
	}

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

	private static Shader CachedGuiTextShader => ShaderCache.GuiText;
	private static Shader CachedUberShader => ShaderCache.Uber;

	private static List<ButtonInfo> _cachedActiveButtons = new List<ButtonInfo>();
	private static bool _activeButtonsDirty = true;
	#endregion

	public static void InvalidateActiveButtonsCache()
	{
		_activeButtonsDirty = true;
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

	private static void RebuildActiveButtonsCache()
	{
		_cachedActiveButtons.Clear();
		foreach (MenuCategory category in MenuManager.Categories)
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

	private static bool joystickFlyActive = false;

	private static bool noGravityActive = false;

	public static float flySpeed = 8f;

	public static readonly float[] FlySpeedValues = new float[] { 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f, 17f, 18f, 19f, 20f };

	public static readonly string[] FlySpeedNames = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };

	public static float controllerPred = 0.0125f;

	private static bool controllerPredActive = false;

	public static readonly float[] ControllerPredValues = new float[] { 0.00625f, 0.0125f, 0.025f, 0.05f };

	public static readonly string[] ControllerPredNames = new string[] { "Low", "Normal", "High", "Extreme" };

	public static int controllerPredIndex = 1;

	public static bool fpsSpoofActive = false;

	public static int fpsSpoofValue = 60;

	public static readonly int[] FPSSpoofValues = new int[] { 0, 20, 45, 60, 67, 72, 80, 85, 120, 200, 225 };

	private static Vector3 _predPrevLeftHand = Vector3.zero;
	private static Vector3 _predPrevRightHand = Vector3.zero;

	private static Vector3 _predPrevHead = Vector3.zero;

	private static Vector3 _predLeftVel = Vector3.zero;

	private static Vector3 _predRightVel = Vector3.zero;

	private static Vector3 _predHeadVel = Vector3.zero;

	private static bool wasdFlyActive = false;

	private static bool wasdFlyNoMouseLock = false;

	private static float wasdFlyMouseSense = 1f;

	public static readonly float[] WasdSenseValues = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f, 2.75f, 3f };

	private static float wasdPitch;

	private static bool flyActive = false;

	public static int speedboostCycle = 0;

	public static float jspeed = 7.5f;

	public static float jmulti = 1.1f;

	public static int pullPowerInt;

	private static float pullPower = 0.05f;

	public static readonly float[] SpeedBoostSpeeds = new float[] { 7.5f, 8f, 9f, 12f, 15f, 20f, 30f, 50f, 100f, 200f };

	public static readonly float[] SpeedBoostMultis = new float[] { 1.1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f };

	public static readonly string[] SpeedBoostNames = new string[] { "Normal", "Slightly Fast", "Fast", "Faster", "Much Faster", "Very Fast", "Extremely Fast", "Incredibly Fast", "Unbelievably Fast", "Maximum" };

	public static readonly float[] PullPowerValues = new float[] { 0.03f, 0.05f, 0.08f, 0.1f, 0.15f, 0.2f, 0.3f, 0.4f, 0.5f };

	public static readonly string[] PullPowerNames = new string[] { "Slightly Weak", "Normal", "Slightly Strong", "Strong", "Stronger", "Much Stronger", "Very Strong", "Extremely Strong", "Maximum" };

	private static readonly Dictionary<bool, bool> previousTouchingGround = new Dictionary<bool, bool>();

	internal static bool ghostMonkeOn = false;

	private static bool grabRigActive = false;

	internal static bool invisMonkeOn = false;

	public static bool LocalRigOverrideActive => ghostMonkeOn || invisMonkeOn || grabRigActive || orbitActive || copyMovementActive || tagGunLockedTarget != null || tagAllTarget != null;

	private static bool ghostMonkeLastPress = false;

	private static Vector3 ghostMonkeFrozenPos;

	private static Quaternion ghostMonkeFrozenRot;

	private static TransformSnapshot ghostMonkeSnapshot;

	private static Vector3 invisMonkeSavedPos;

	private static bool invisMonkeLastPress = false;

		private static bool invisMonkeSkinsDisabled = false;

	private static bool antiNameBanApplied = true;

	private static bool bitcrunchMicActive = false;

	private static int bitcrunchOrigSampleRate = 16000;

	private static int bitcrunchOrigBitrate = 24000;

	private static bool boopActive = false;

	private static bool boopLastL;

	private static bool boopLastR;

	private static float boopCooldown;

	private static int randomColorSpazTick;

	private static bool minosPrimedForSlam = false;

	private static bool minosWaitingForImpact = false;

	private static AudioClip minosCrushClip = null;

	private static AudioClip minosSlamClip = null;

	private static bool minosClipsLoaded = false;

	private static bool minosSecondaryWasDown = false;

	private static bool minosPrimaryWasDown = false;

	private static Coroutine minosRestoreCoroutine = null;

	private static AudioSource minosLocalSource = null;

	private const string MinosCrushUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/CRUSH%20!.mp3";

	private const string MinosSlamUrl = "https://raw.githubusercontent.com/vhghfhnfgvbngv/plmokni/main/slam%20sound.mp3";

	private static GameObject FreeCamObject;
	public static bool thirdPersonEnabled;
	private static bool thirdPersonViewActive;
	private static bool xButtonWasDown;
private static VRRig ghostRig;

	private static Material ghostRigMaterial;

	internal static bool cloningGhostRig;
	private static bool ghostRigSubscribed;

	private static readonly Dictionary<VRRig, GameObject> boxEspObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> nameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> fpsNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> idNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> platformNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> arsTagObjects = new Dictionary<VRRig, GameObject>();

	private static HashSet<string> arsPlayersToReport = new HashSet<string>();

	private static bool arsActive = false;

	private static bool arsDownloaded = false;

	private static bool arsDownloading = false;

	private static readonly HttpClient arsHttpClient = new HttpClient();

	public static Font comicSansFont;

	private static FieldInfo _fpsField;

	// Name tag stack order, bottom (closest to head) to top.
	// 0 Console -> 1 Cosmetics -> 2 ID -> 3 Platform -> 4 Name -> 5 FPS -> 6 ARS -> 7 Admin crown
	public const int TagStackConsole = 0;
	public const int TagStackCosmetics = 1;
	public const int TagStackId = 2;
	public const int TagStackPlatform = 3;
	public const int TagStackName = 4;
	public const int TagStackFps = 5;
	public const int TagStackArs = 6;
	public const int TagStackCrown = 7;

	private static readonly Dictionary<string, string> cosmeticNames = new Dictionary<string, string>
	{
		{ "LBAAK.", "Dev stick" },
		{ "LBANI.", "AA BADGE" },
		{ "LMAPY.", "Forest guide" },
		{ "LBADE.", "Finger painter" },
		{ "LBAGS.", "illustrator" },
		{ "LMAYQ.", "Golden gorilla ticket" },
		{ "LBARJ.", "COMMUNITY RIBBON" },
		{ "LBASS.", "PARTY ILLUSTRATOR BADGE" },
		{ "LMAJA.", "GT MONKE PLUSH" },
		{ "LMAYT.", "LAVA MONKE DOUGHBOI" },
		{ "LMBAO.", "Gorillacon golden phone" }
	};

	private static readonly HashSet<string> trackedWebhookCosmetics = new HashSet<string>
	{
		"LBAAK.", "LBANI.", "LMAPY.", "LBADE.", "LBAGS.", "LBARJ.", "LBASS."
	};

	private static readonly Dictionary<VRRig, GameObject> cosmeticNameTagObjects = new Dictionary<VRRig, GameObject>();

	private static FieldInfo _ownedCosmeticsField;

	private static bool arsNameTagsActive = false;

	private static string arsLastCheckedRoom = "";

	private static bool cosmeticNotifierActive = false;

	private static HashSet<string> cosmeticNotifierNotified = new HashSet<string>();

	private static bool notificationsEnabled = true;

	public static int menuColorIndex = 0;

	private static int notificationDecayTime = 150;

	private static int notificationTimeIndex = 5;

	private static readonly int[] notificationTimeValues = new int[10] { 50, 75, 100, 125, 150, 200, 250, 300, 400, 500 };

	private static readonly string[] notificationTimeNames = new string[10] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" };

	private static Harmony vimHarmony;

	private static float lastUntagNotif = 0f;

	private static int tagGunFramesUntilTag;
	private static VRRig tagGunLockedTarget = null;

	private static VRRig tagAllTarget;
	private static int tagAllFramesUntilTag;
	private static List<VRRig> tagAllTargets;
	private static int tagAllIndex;



	private static float lastUntagSelfTime;

	private static VRRig guardianSpazTarget;
	private static float guardianSpazTimer;
	private static float lastGuardianGunTime;
	private static float lastUnguardianGunTime;

	private static Vector3 stumpPosition = new Vector3(-66.871f, 12.086f, -82.637f);

	private static bool spazAllActive = false;

	private static int spazAllFrameCounter = 0;

	private static bool spazSelfActive = false;

	private static int spazSelfFrameCounter = 0;


	private static bool gunTriggerWasDown = false;

	private static Camera pcGunCamera;

	public static bool blockJmanSounds = false;

	public static bool antiGuardianGrab = false;

	public static bool antiBlockCrash = false;

	public static bool seeAntiCheatReports = false;

	public static readonly Dictionary<string, int> antiCheatReportCounts = new Dictionary<string, int>();

	public static bool antiReportEnabled;
	public static int antiReportRangeIndex = 1;
	public static float antiReportRange = 0.35f;
	private static readonly float[] antiReportRanges = new float[] { 0.25f, 0.35f, 0.5f, 0.7f, 1f, 1.25f, 1.5f, 2f };
	private static float antiReportDelay;
	private static GameObject antiReportSphere;
	private static Material antiReportMat;

	private static bool pcButtonClickEnabled = false;

	private static Vector3? pcButtonOldLocalPosition;

	private static Camera pcButtonCachedCamera;

	private static readonly List<VRRig> reusableBoxEspRemovals = new List<VRRig>();

	private static readonly List<Player> reusableTracerRemovals = new List<Player>();

	private static readonly List<Player> reusableSkeletonRemovals = new List<Player>();

	private static readonly List<VRRig> reusableTagRemovals = new List<VRRig>();

	private static readonly (Vector3, Vector3)[] reusableFingerConns = new (Vector3, Vector3)[6];

	private static int? noInvisLayerMask;

	private static bool pcGunsEnabled = false;

	public static int activeMenuStyle = 3;

	public static bool breakGuardianActive = false;
	private static Harmony breakGuardianHarmony;

	public static bool isRightHanded = false;

	public static int ButtonSound = 67;

	public static GameObject pointer = null;

	public static LineRenderer Line;

	public static RaycastHit raycastHit;

	public static bool gripHeld = false;

	public static bool triggerHeld = false;

	private static readonly Dictionary<Player, LineRenderer> tracerLines = new Dictionary<Player, LineRenderer>();

	private static readonly Dictionary<Player, LineRenderer[]> skeletonLines = new Dictionary<Player, LineRenderer[]>();

	private static int noclipCacheFrame = 0;

	private static MeshCollider[] noclipCache = (MeshCollider[])(object)new MeshCollider[0];
	private static BoxCollider[] noclipBoxCache = (BoxCollider[])(object)new BoxCollider[0];

	private static readonly Dictionary<Collider, bool> noclipOriginalStates = new Dictionary<Collider, bool>();

	private static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
	private static Material platMaterial;

	private static bool once_left;

	private static bool once_right;

	private static bool once_left_false;

	private static bool once_right_false;

	private static GameObject jump_left_local = null;

	private static GameObject jump_right_local = null;

	private static bool stickyRightActive = false;

	private static bool stickyLeftActive = false;

	public static bool RPlat;

	public static bool LPlat;

	private static bool grabGreenBugActive = false;

	private static bool grabDougBugActive = false;

	private static bool grabAllBugsActive = false;

	private static bool grabSpazBugActive = false;

	private static float grabBugLastScan;

	private static readonly List<ThrowableBug> cachedGrabBugs = new List<ThrowableBug>();

	private const float GRAB_BUG_SCAN_INTERVAL = 3f;


	public static string ConfigPath => WristMenu.FolderName + "\\Config.json";


	private void Awake()
	{
		instance = this;
	}


	public static void JoystickFly()
	{
		joystickFlyActive = true;
		RegisterFlyGravityOverride();
	}

	public static void SetFlySpeed(int index)
	{
		index %= FlySpeedValues.Length;
		if (index < 0) index = FlySpeedValues.Length - 1;
		flySpeed = FlySpeedValues[index];
		NotifiLib.SendNotification("Fly Speed: " + FlySpeedNames[index]);
	}

	public static void DisableJoystickFly()
	{
		joystickFlyActive = false;
		UnregisterFlyGravityOverride();
	}

	public static void EnableWASDFly()
	{
		wasdFlyActive = true;
		wasdPitch = 0f;
		RegisterFlyGravityOverride();
	}

	public static void DisableWASDFly()
	{
		wasdFlyActive = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		UnregisterFlyGravityOverride();
	}

	public static void EnableFly()
	{
		flyActive = true;
	}

	public static void DisableFly()
	{
		flyActive = false;
	}

	private static void UpdateFly()
	{
		if (flyActive)
		{
			if ((Object)(object)ControllerInputPoller.instance != (Object)null && (Object)(object)GorillaTagger.Instance != (Object)null && !((Object)(object)GorillaTagger.Instance.rigidbody == (Object)null) && (isRightHanded ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerInputPoller.instance.rightControllerSecondaryButton))
			{
				RegisterFlyGravityOverride();
				Transform transform = GTPlayer.Instance.transform;
				transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flySpeed);
				_flyDesiredVelocity = Vector3.zero;
			}
			else
			{
				UnregisterFlyGravityOverride();
				_flyDesiredVelocity = Vector3.zero;
			}
		}
	}

	public static void SetWASDFlyNoMouseLock(bool on)
	{
		wasdFlyNoMouseLock = on;
		
	}

	public static void SetWASDFlyMouseSense(int index)
	{
		index %= WasdSenseValues.Length;
		if (index < 0) index = WasdSenseValues.Length - 1;
		wasdFlyMouseSense = WasdSenseValues[index];
		NotifiLib.SendNotification("WASD Mouse Sense: " + wasdFlyMouseSense.ToString("0.00"));
	}

	private static void UpdateWASDFly()
	{
		if (!wasdFlyActive)
		{
			return;
		}
		if ((Object)(object)GorillaTagger.Instance == (Object)null)
		{
			return;
		}
		Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
		if ((Object)(object)rigidbody == (Object)null)
		{
			return;
		}
		if (GTPlayer.Instance == null || GTPlayer.Instance.headCollider == null || GTPlayer.Instance.transform == null)
		{
			return;
		}
		Transform transform = ((Component)GTPlayer.Instance.headCollider).transform;
		Transform transform2 = GTPlayer.Instance.transform;
		Vector3 val = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		Vector3 normalized = val.normalized;
		val = Vector3.ProjectOnPlane(transform.right, Vector3.up);
		Vector3 normalized2 = val.normalized;
		Vector3 val2 = Vector3.zero;
		Keyboard current = Keyboard.current;
		if (current != null)
		{
			if (((ButtonControl)current.wKey).isPressed)
			{
				val2 += normalized;
			}
			if (((ButtonControl)current.sKey).isPressed)
			{
				val2 -= normalized;
			}
			if (((ButtonControl)current.aKey).isPressed)
			{
				val2 -= normalized2;
			}
			if (((ButtonControl)current.dKey).isPressed)
			{
				val2 += normalized2;
			}
			if (((ButtonControl)current.spaceKey).isPressed)
			{
				val2 += Vector3.up;
			}
			if (current.ctrlKey.isPressed)
			{
				val2 -= Vector3.up;
			}
		}
		if (val2.sqrMagnitude > 0.01f)
		{
			GTPlayer.Instance.transform.position += val2.normalized * flySpeed * Time.deltaTime;
			if (GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null) GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
		}
		Mouse current2 = Mouse.current;
		if (current2 != null && current2.rightButton.isPressed)
		{
			if (!wasdFlyNoMouseLock)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			Vector2 val4 = ((InputControl<Vector2>)(object)((Pointer)current2).delta).ReadValue() * wasdFlyMouseSense * 0.15f;
			transform2.Rotate(Vector3.up, val4.x, Space.World);
			wasdPitch = Mathf.Clamp(wasdPitch - val4.y, -90f, 90f);
			Quaternion targetRot = Quaternion.Euler(wasdPitch, 0f, 0f);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * 12f);
		}
		else if (!wasdFlyNoMouseLock && (int)Cursor.lockState == 1)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	private static readonly Action<GTPlayer> _flyGravityCallback = delegate(GTPlayer gt)
	{
		Rigidbody rb = gt.playerRigidBody;
		rb.linearVelocity = Vector3.zero;
	};

	private static void RegisterFlyGravityOverride()
	{
		if ((Object)(object)GTPlayer.Instance != (Object)null)
		{
			GTPlayer.Instance.SetGravityOverride(instance, _flyGravityCallback);
		}
	}

	private static void UnregisterFlyGravityOverride()
	{
		if ((Object)(object)GTPlayer.Instance != (Object)null && !joystickFlyActive && !wasdFlyActive && !noGravityActive)
		{
			GTPlayer.Instance.UnsetGravityOverride(instance);
		}
	}

	public static void NoGravity()
	{
		noGravityActive = true;
		RegisterFlyGravityOverride();
	}

	public static void DisableNoGravity()
	{
		noGravityActive = false;
		UnregisterFlyGravityOverride();
	}

	public static void Platforms()
	{
		PlatformsThing(invis: false, false);
	}

	public static void StickyPlatforms()
	{
		PlatformsThing(invis: false, true);
	}

	public static void GrabGreenBug()
	{
		grabGreenBugActive = !grabGreenBugActive;
	}

	public static void DisableGrabGreenBug()
	{
		grabGreenBugActive = false;
	}

	public static void GrabDougBug()
	{
		grabDougBugActive = !grabDougBugActive;
	}

	public static void DisableGrabDougBug()
	{
		grabDougBugActive = false;
	}

	public static void GrabAllBugs()
	{
		grabAllBugsActive = !grabAllBugsActive;
	}

	public static void DisableGrabAllBugs()
	{
		grabAllBugsActive = false;
	}

	public static void SpazBugs()
	{
		grabSpazBugActive = !grabSpazBugActive;
	}

	public static void DisableSpazBugs()
	{
		grabSpazBugActive = false;
	}

	public static void UpdateGrabBugs()
	{
		if (!grabGreenBugActive && !grabDougBugActive && !grabAllBugsActive && !grabSpazBugActive)
			return;

		bool rightGrip = (Object)(object)ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.rightGrab;
		bool leftGrip = (Object)(object)ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.leftGrab;
		bool anyGrip = rightGrip || leftGrip;

		if (!anyGrip && !grabSpazBugActive)
			return;

		if (Time.time > grabBugLastScan + GRAB_BUG_SCAN_INTERVAL)
		{
			grabBugLastScan = Time.time;
			cachedGrabBugs.Clear();
			cachedGrabBugs.AddRange(Resources.FindObjectsOfTypeAll<ThrowableBug>());
		}

		Transform rightHand = GorillaTagger.Instance.rightHandTransform;
		Transform leftHand = GorillaTagger.Instance.leftHandTransform;
		Transform hand = rightGrip ? rightHand : leftHand;

		for (int i = cachedGrabBugs.Count - 1; i >= 0; i--)
		{
			ThrowableBug bug = cachedGrabBugs[i];
			if ((Object)(object)bug == (Object)null)
			{
				cachedGrabBugs.RemoveAt(i);
				continue;
			}
			if (bug.name != "Floating Bug Holdable")
				continue;

			try
			{
				if (grabSpazBugActive)
				{
					if (!bug.IsMyItem())
						bug.WorldShareableRequestOwnership();
					float phase = (float)(bug.GetInstanceID() % 97) * 0.010309f;
					float t = (Mathf.Sin((Time.time + phase) * 12f) + 1f) * 0.5f;
					bug.transform.position = Vector3.Lerp(leftHand.position, rightHand.position, t);
					bug.transform.rotation = Random.rotation;
					continue;
				}

				if (!anyGrip)
					continue;

				Transform model = bug.transform.Find("model/PlumpBeetle");
				if ((Object)(object)model == (Object)null) continue;
				SkinnedMeshRenderer renderer = model.GetComponent<SkinnedMeshRenderer>();
				if ((Object)(object)renderer == (Object)null || (Object)(object)renderer.material == (Object)null) continue;
				string matName = renderer.material.name;
				bool isGreen = matName.Contains("PlumpBeetle2");
				bool isDoug = !isGreen && matName.Contains("PlumpBeetle");
				bool shouldGrab = grabAllBugsActive || (grabGreenBugActive && isGreen) || (grabDougBugActive && isDoug);

				if (!shouldGrab)
					continue;

				if (!bug.IsMyItem())
					bug.WorldShareableRequestOwnership();

				Rigidbody rb = bug.GetComponent<Rigidbody>();
				if ((Object)(object)rb != (Object)null)
					rb.position = hand.position;
				else
					bug.transform.position = hand.position;

				if (!float.IsPositiveInfinity(bug.maxDistanceFromOriginBeforeRespawn))
					bug.maxDistanceFromOriginBeforeRespawn = float.MaxValue;
				if (!float.IsPositiveInfinity(bug.maxDistanceFromTargetPlayerBeforeRespawn))
					bug.maxDistanceFromTargetPlayerBeforeRespawn = float.MaxValue;
			}
			catch { }
		}
	}

	public static void Noclip()
	{
		noclipCacheFrame++;
		if (noclipCacheFrame % 60 == 0 || noclipCache.Length == 0)
		{
			noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
		}
		if (noclipBoxCache.Length == 0)
		{
			noclipBoxCache = Resources.FindObjectsOfTypeAll<BoxCollider>();
		}
		bool noclipBtn = isRightHanded ? WristMenu.ybuttonDown : WristMenu.bbuttonDown;
		foreach (MeshCollider val in noclipCache)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Collider c = (Collider)(object)val;
			if (!noclipOriginalStates.ContainsKey(c))
			{
				noclipOriginalStates[c] = c.enabled;
			}
			c.enabled = !noclipBtn;
		}
		foreach (BoxCollider val2 in noclipBoxCache)
		{
			if ((Object)(object)val2 == (Object)null || val2.isTrigger)
			{
				continue;
			}
			Collider c2 = (Collider)(object)val2;
			if (!noclipOriginalStates.ContainsKey(c2))
			{
				noclipOriginalStates[c2] = c2.enabled;
			}
			c2.enabled = !noclipBtn;
		}
	}

	public static void NoclipOff()
	{
		noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
		noclipBoxCache = Resources.FindObjectsOfTypeAll<BoxCollider>();
		foreach (var kvp in noclipOriginalStates)
		{
			if ((Object)(object)kvp.Key != (Object)null)
			{
				kvp.Key.enabled = kvp.Value;
			}
		}
		noclipOriginalStates.Clear();
	}

	public static void SetSpeedBoostAmount(int index)
	{
		speedboostCycle = index % SpeedBoostSpeeds.Length;
		if (speedboostCycle < 0) speedboostCycle = SpeedBoostSpeeds.Length - 1;
		jspeed = SpeedBoostSpeeds[speedboostCycle];
		jmulti = SpeedBoostMultis[speedboostCycle];
		NotifiLib.SendNotification("Speed: " + SpeedBoostNames[speedboostCycle]);
	}

	public static void SpeedBoost()
	{
		if (GTPlayer.Instance == null) return;
		float maxJumpSpeed = jspeed;
		float jumpMultiplier = jmulti;
		GTPlayer.Instance.maxJumpSpeed = maxJumpSpeed;
		GTPlayer.Instance.jumpMultiplier = jumpMultiplier;
		Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
		if (GTPlayer.Instance.BodyOnGround && component.linearVelocity.y > 0f)
		{
			component.linearVelocity = new Vector3(component.linearVelocity.x, 0f, component.linearVelocity.z);
		}
	}

	public static void DisableSpeedBoost()
	{
		GTPlayer.Instance.maxJumpSpeed = 6.5f;
		GTPlayer.Instance.jumpMultiplier = 1.1f;
	}

	public static void EnableControllerPredictions()
	{
		controllerPredActive = true;
		_predPrevLeftHand = GorillaTagger.Instance.leftHandTransform.position;
		_predPrevRightHand = GorillaTagger.Instance.rightHandTransform.position;
		_predPrevHead = VRRig.LocalRig.head.rigTarget.transform.position;
		TorsoPatch.VRRigLateUpdate -= ControllerPredTick;
		TorsoPatch.VRRigLateUpdate += ControllerPredTick;
	}

	public static void DisableControllerPredictions()
	{
		controllerPredActive = false;
		TorsoPatch.VRRigLateUpdate -= ControllerPredTick;
	}

	public static void SetControllerPrediction(int index)
	{
		index %= ControllerPredValues.Length;
		controllerPredIndex = ((index < 0) ? (ControllerPredValues.Length - 1) : index);
		controllerPred = ControllerPredValues[controllerPredIndex];
		NotifiLib.SendNotification("Set to " + ControllerPredNames[controllerPredIndex]);
	}

	private static void ControllerPredTick()
	{
		if (!controllerPredActive)
			return;
		if ((Object)(object)GorillaTagger.Instance == (Object)null)
			return;
		VRRig local = VRRig.LocalRig;
		if ((Object)(object)local == (Object)null || local.head == null || (Object)(object)local.head.rigTarget == (Object)null)
			return;
		float dt = Time.deltaTime;
		if (dt <= 0.0001f)
			return;
		Vector3 leftHandPos = GorillaTagger.Instance.leftHandTransform.position;
		Vector3 rightHandPos = GorillaTagger.Instance.rightHandTransform.position;
		Vector3 headPos = local.head.rigTarget.transform.position;
		_predLeftVel = Vector3.Lerp(_predLeftVel, (leftHandPos - _predPrevLeftHand) / dt, 0.5f);
		_predRightVel = Vector3.Lerp(_predRightVel, (rightHandPos - _predPrevRightHand) / dt, 0.5f);
		_predHeadVel = Vector3.Lerp(_predHeadVel, (headPos - _predPrevHead) / dt, 0.5f);
		_predPrevLeftHand = leftHandPos;
		_predPrevRightHand = rightHandPos;
		_predPrevHead = headPos;
		if (local.leftHand != null && (Object)(object)local.leftHand.rigTarget != (Object)null)
			local.leftHand.rigTarget.transform.position += (_predLeftVel - _predHeadVel) * controllerPred;
		if (local.rightHand != null && (Object)(object)local.rightHand.rigTarget != (Object)null)
			local.rightHand.rigTarget.transform.position += (_predRightVel - _predHeadVel) * controllerPred;
	}

	public static void EnableFPSSpoof()
	{
		fpsSpoofActive = true;
	}

	public static void DisableFPSSpoof()
	{
		fpsSpoofActive = false;
	}

	public static void SetFPSSpoof(int index)
	{
		index %= FPSSpoofValues.Length;
		fpsSpoofValue = FPSSpoofValues[((index < 0) ? (FPSSpoofValues.Length - 1) : index)];
		NotifiLib.SendNotification("Set to " + fpsSpoofValue + " fps");
	}

	public static void SetPullModPower(int index)
	{
		pullPowerInt = index % PullPowerValues.Length;
		if (pullPowerInt < 0) pullPowerInt = PullPowerValues.Length - 1;
		pullPower = PullPowerValues[pullPowerInt];
		NotifiLib.SendNotification("Pull power: " + PullPowerNames[pullPowerInt]);
	}

	private static void ProcessPullHand(bool left)
	{
		if (!(left ? (!ControllerInputPoller.instance.leftGrab) : (!ControllerInputPoller.instance.rightGrab)))
		{
			bool flag = GTPlayer.Instance.IsHandTouching(left);
			previousTouchingGround.TryGetValue(left, out var value);
			if (!flag && value)
			{
				Vector3 up = Vector3.up;
				Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
				Vector3 val = GTVector3Extensions.X_Z(component.linearVelocity);
				Transform transform = GTPlayer.Instance.transform;
				Vector3 position = transform.position;
				Vector3 val2 = val - up * Vector3.Dot(val, up);
				transform.position = position + val2.normalized * (val.magnitude / GTPlayer.Instance.maxJumpSpeed * (pullPower * 5f)) * GTPlayer.Instance.scale;
			}
			previousTouchingGround[left] = flag;
		}
	}

	public static void PullMod()
	{
		ProcessPullHand(left: false);
		ProcessPullHand(left: true);
	}

	private static void TakeRigSnapshot(out TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		s = default(TransformSnapshot);
		if (localRig.head != null && (Object)(object)localRig.head.rigTarget != (Object)null)
		{
			s.headPos = localRig.head.rigTarget.transform.position;
			s.headRot = localRig.head.rigTarget.transform.rotation;
		}
		if (localRig.leftHand != null && (Object)(object)localRig.leftHand.rigTarget != (Object)null)
		{
			s.leftHandPos = localRig.leftHand.rigTarget.transform.position;
			s.leftHandRot = localRig.leftHand.rigTarget.transform.rotation;
		}
		if (localRig.rightHand != null && (Object)(object)localRig.rightHand.rigTarget != (Object)null)
		{
			s.rightHandPos = localRig.rightHand.rigTarget.transform.position;
			s.rightHandRot = localRig.rightHand.rigTarget.transform.rotation;
		}
		s.leftIndexT = ((VRMap)localRig.leftIndex).calcT;
		s.leftMiddleT = ((VRMap)localRig.leftMiddle).calcT;
		s.leftThumbT = ((VRMap)localRig.leftThumb).calcT;
		s.rightIndexT = ((VRMap)localRig.rightIndex).calcT;
		s.rightMiddleT = ((VRMap)localRig.rightMiddle).calcT;
		s.rightThumbT = ((VRMap)localRig.rightThumb).calcT;
	}

	private static void ApplyRigSnapshot(ref TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		if (localRig.head != null && (Object)(object)localRig.head.rigTarget != (Object)null)
		{
			localRig.head.rigTarget.transform.SetPositionAndRotation(s.headPos, s.headRot);
		}
		if (localRig.leftHand != null && (Object)(object)localRig.leftHand.rigTarget != (Object)null)
		{
			localRig.leftHand.rigTarget.transform.SetPositionAndRotation(s.leftHandPos, s.leftHandRot);
		}
		if (localRig.rightHand != null && (Object)(object)localRig.rightHand.rigTarget != (Object)null)
		{
			localRig.rightHand.rigTarget.transform.SetPositionAndRotation(s.rightHandPos, s.rightHandRot);
		}
		((VRMap)localRig.leftIndex).calcT = s.leftIndexT;
		((VRMap)localRig.leftIndex).LerpFinger(1f, false);
		((VRMap)localRig.leftMiddle).calcT = s.leftMiddleT;
		((VRMap)localRig.leftMiddle).LerpFinger(1f, false);
		((VRMap)localRig.leftThumb).calcT = s.leftThumbT;
		((VRMap)localRig.leftThumb).LerpFinger(1f, false);
		((VRMap)localRig.rightIndex).calcT = s.rightIndexT;
		((VRMap)localRig.rightIndex).LerpFinger(1f, false);
		((VRMap)localRig.rightMiddle).calcT = s.rightMiddleT;
		((VRMap)localRig.rightMiddle).LerpFinger(1f, false);
		((VRMap)localRig.rightThumb).calcT = s.rightThumbT;
		((VRMap)localRig.rightThumb).LerpFinger(1f, false);
	}

	public static void GhostMonke()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		bool ghostMonkeButton = isRightHanded ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerInputPoller.instance.rightControllerSecondaryButton;
		if (ghostMonkeButton && !ghostMonkeLastPress)
		{
			ghostMonkeOn = !ghostMonkeOn;
			if (ghostMonkeOn)
			{
				ghostMonkeFrozenPos = VRRig.LocalRig.transform.position;
				ghostMonkeFrozenRot = VRRig.LocalRig.transform.rotation;
				TakeRigSnapshot(out ghostMonkeSnapshot);
				SubscribeGhostRig();
			}
			else
			{
				EnsureLocalRigEnabled();
				TryUnsubscribeGhostRig();
			}
		}
		ghostMonkeLastPress = ghostMonkeButton;
		if (ghostMonkeOn)
		{
			EnsureLocalRigEnabled();
			if (!XRSettings.isDeviceActive)
			{
				VRRig local = VRRig.LocalRig;
				Vector3 pos = GTPlayer.Instance.transform.position + Vector3.up * 0.2f;
				Quaternion rot = GTPlayer.Instance.transform.rotation;
				local.transform.SetPositionAndRotation(pos, rot);
				if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
				return;
			}
			VRRig.LocalRig.transform.SetPositionAndRotation(ghostMonkeFrozenPos, ghostMonkeFrozenRot);
			ApplyRigSnapshot(ref ghostMonkeSnapshot);
		}
	}

	public static void DisableGhostMonke()
	{
		EnsureLocalRigEnabled();
		ghostMonkeOn = false;
		TryUnsubscribeGhostRig();
	}

	private static void InvisMonkeSetSkins(bool disable)
	{
		if (!((Object)(object)VRRig.LocalRig == (Object)null) && disable != invisMonkeSkinsDisabled)
		{
			SkinnedMeshRenderer mainSkin = VRRig.LocalRig.mainSkin;
			if (!((Object)(object)mainSkin == (Object)null))
			{
				((Renderer)mainSkin).enabled = !disable;
				invisMonkeSkinsDisabled = disable;
			}
		}
	}

	public static void InvisMonke()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		bool invisMonkeButton = isRightHanded ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerInputPoller.instance.rightControllerPrimaryButton;
		if (invisMonkeButton && !invisMonkeLastPress)
		{
			if (!invisMonkeOn)
			{
				invisMonkeSavedPos = VRRig.LocalRig.transform.position;
				invisMonkeOn = true;
				InvisMonkeSetSkins(disable: true);
				SubscribeGhostRig();
			}
			else
			{
				EnsureLocalRigEnabled();
				VRRig.LocalRig.transform.position = invisMonkeSavedPos;
				InvisMonkeSetSkins(disable: false);
				invisMonkeOn = false;
				TryUnsubscribeGhostRig();
			}
		}
		invisMonkeLastPress = invisMonkeButton;
		if (invisMonkeOn)
		{
			EnsureLocalRigEnabled();
			if (!XRSettings.isDeviceActive)
			{
				VRRig local = VRRig.LocalRig;
				Vector3 pos = GTPlayer.Instance.transform.position + Vector3.up * 0.2f;
				Quaternion rot = GTPlayer.Instance.transform.rotation;
				local.transform.SetPositionAndRotation(pos, rot);
				if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
				return;
			}
			VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
		}
	}

	public static void DisableInvisMonke()
	{
		if ((Object)(object)VRRig.LocalRig != (Object)null && invisMonkeOn)
		{
			EnsureLocalRigEnabled();
			VRRig.LocalRig.transform.position = invisMonkeSavedPos;
			InvisMonkeSetSkins(disable: false);
		}
		invisMonkeOn = false;
		TryUnsubscribeGhostRig();
	}

	private static void EnsureLocalRigEnabled()
	{
		if ((Object)(object)VRRig.LocalRig != (Object)null && !VRRig.LocalRig.enabled)
			VRRig.LocalRig.enabled = true;
	}

	public static void GrabRig()
	{
		if (WristMenu.gripDownR)
		{
			if (!grabRigActive)
			{
				grabRigActive = true;
				TorsoPatch.VRRigLateUpdate -= GrabRigTick;
				TorsoPatch.VRRigLateUpdate += GrabRigTick;
				SubscribeGhostRig();
			}
		}
		else if (grabRigActive)
		{
			grabRigActive = false;
			TorsoPatch.VRRigLateUpdate -= GrabRigTick;
			EnsureLocalRigEnabled();
			TryUnsubscribeGhostRig();
		}
	}

	public static void DisableGrabRig()
	{
		grabRigActive = false;
		TorsoPatch.VRRigLateUpdate -= GrabRigTick;
		EnsureLocalRigEnabled();
		TryUnsubscribeGhostRig();
	}

	private static void GrabRigTick()
	{
		if (!grabRigActive || (Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		EnsureLocalRigEnabled();
		if (!XRSettings.isDeviceActive)
		{
			VRRig local = VRRig.LocalRig;
			Vector3 pos = GTPlayer.Instance.transform.position + Vector3.up * 0.2f;
			Quaternion rot = GTPlayer.Instance.transform.rotation;
			local.transform.SetPositionAndRotation(pos, rot);
			if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
				local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
			return;
		}
		Transform hand = GorillaTagger.Instance.rightHandTransform;
		VRRig local2 = VRRig.LocalRig;
		local2.transform.SetPositionAndRotation(hand.position, hand.rotation);
		if (local2.head != null && (Object)(object)local2.head.rigTarget != (Object)null)
			local2.head.rigTarget.transform.SetPositionAndRotation(hand.position, hand.rotation);
	}

	private void Update()
	{
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
		else if ((Object)(object)FreeCamObject != (Object)null)
			DisableThirdPersonView();
	}

	private static Vector3 _flyDesiredVelocity = Vector3.zero;

	private void LateUpdate()
	{
		if ((Object)(object)VRRig.LocalRig == (Object)null)
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
				if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
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
				if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
			}
			else
			{
				EnsureLocalRigEnabled();
				VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
			}
		}
		UpdateBoop();
		if (stickyRightActive && jump_right_local != null) ClampHandToCage(jump_right_local.transform.position, true);
		if (stickyLeftActive && jump_left_local != null) ClampHandToCage(jump_left_local.transform.position, false);
		UpdateGrabBugs();
	}


	private static void ClampHandToCage(Vector3 center, bool isRight)
	{
		float radius = 0.15f;
		Transform hand = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
		if (hand == null) return;
		Vector3 offset = hand.position - center;
		float dist = offset.magnitude;
		if (dist > radius)
		{
			hand.position = center + offset / dist * radius;
		}
	}

	private static void UpdateJoystickFly()
	{
		if ((Object)(object)GTPlayer.Instance == (Object)null || (Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)GorillaTagger.Instance.rigidbody == (Object)null) return;
		Vector2 joyL = WristMenu.joyL;
		Vector2 joy = WristMenu.joy;
		if (joyL.magnitude < 0.12f) joyL = Vector2.zero;
		if (joy.magnitude < 0.12f) joy = Vector2.zero;
		if (joyL.sqrMagnitude < 0.001f && joy.sqrMagnitude < 0.001f) return;
		Transform head = ((Component)GTPlayer.Instance.headCollider).transform;
		Vector3 fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
		Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
		Vector3 dir = fwd * joyL.y + right * joyL.x + Vector3.up * joy.y;
		if (dir.sqrMagnitude < 0.01f) return;
		GTPlayer.Instance.transform.position += dir.normalized * flySpeed * Time.deltaTime;
		if (GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null) GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
	}

	public static void AntiNameBan()
	{
		antiNameBanApplied = true;
		BanPatchState.enabled = true;
	}

	public static void DisableAntiNameBan()
	{
		if (antiNameBanApplied)
		{
			BanPatchState.enabled = false;
			antiNameBanApplied = false;
		}
	}

	public static void BitcrunchMic()
	{
		if (!bitcrunchMicActive)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if (!((Object)(object)myRecorder == (Object)null))
			{
				bitcrunchOrigSampleRate = (int)myRecorder.SamplingRate;
				bitcrunchOrigBitrate = myRecorder.Bitrate;
				myRecorder.SamplingRate = (SamplingRate)8000;
				myRecorder.Bitrate = 8000;
				myRecorder.RestartRecording(true);
				bitcrunchMicActive = true;
				
			}
		}
	}

	public static void DisableBitcrunchMic()
	{
		if (bitcrunchMicActive)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if ((Object)(object)myRecorder != (Object)null)
			{
				myRecorder.SamplingRate = (SamplingRate)bitcrunchOrigSampleRate;
				myRecorder.Bitrate = bitcrunchOrigBitrate;
				myRecorder.RestartRecording(true);
			}
			bitcrunchMicActive = false;
			
		}
	}

	public static void Boop()
	{
		boopActive = true;
	}

	public static void DisableBoop()
	{
		boopActive = false;
		boopCooldown = 0f;
	}

	private static void UpdateBoop()
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
			if (!activeRig.isLocal && !((Object)(object)activeRig.headMesh == (Object)null))
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
		randomColorSpazTick++;
		if (randomColorSpazTick % 15 == 0)
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
		randomColorSpazTick = 0;
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

	private static float splashCooldown;

	public static int waterSplashSpeedIndex = 1;

	public static readonly float[] WaterSplashCooldowns = new float[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.4f, 0.5f, 0.75f, 1f };

	public static readonly string[] WaterSplashNames = new string[] { "0.05s", "0.1s", "0.15s", "0.2s", "0.25s", "0.3s", "0.4s", "0.5s", "0.75s", "1s" };

	private static void SpawnSplash()
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

	public static void WaterSplash()
	{
		SpawnSplash();
	}

	public static void DisableWaterSplash() { }

	public static void SetWaterSplashSpeed(int index)
	{
		waterSplashSpeedIndex = index % WaterSplashCooldowns.Length;
		NotifiLib.SendNotification("Water splash speed: " + WaterSplashNames[waterSplashSpeedIndex]);
	}

	public static void MinosPrime()
	{
		if (!minosClipsLoaded)
		{
			minosClipsLoaded = true;
			instance.StartCoroutine(LoadMinosSounds());
		}
		bool minosSecondaryBtn = isRightHanded ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerInputPoller.instance.rightControllerSecondaryButton;
		bool minosPrimaryBtn = isRightHanded ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerInputPoller.instance.rightControllerPrimaryButton;
		if (minosSecondaryBtn && !minosSecondaryWasDown)
		{
			GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(GorillaTagger.Instance.rigidbody.linearVelocity.x, 20f, GorillaTagger.Instance.rigidbody.linearVelocity.z);
			PlayMinosClip(minosCrushClip);
			minosPrimedForSlam = true;
			minosWaitingForImpact = false;
		}
		if (minosPrimaryBtn && !minosPrimaryWasDown && minosPrimedForSlam)
		{
			Vector3 val = (((Object)(object)Camera.main != (Object)null) ? Camera.main.transform.forward : Vector3.forward);
			GorillaTagger.Instance.rigidbody.linearVelocity = val * 35f;
			minosPrimedForSlam = false;
			minosWaitingForImpact = true;
		}
		if (minosWaitingForImpact)
		{
			Vector3 linearVelocity = GorillaTagger.Instance.rigidbody.linearVelocity;
			if (linearVelocity.magnitude < 5f)
			{
				minosWaitingForImpact = false;
				PlayMinosClip(minosSlamClip);
			}
		}
		minosSecondaryWasDown = minosSecondaryBtn;
		minosPrimaryWasDown = minosPrimaryBtn;
	}

	public static void DisableMinosPrime()
	{
		minosPrimedForSlam = false;
		minosWaitingForImpact = false;
		minosSecondaryWasDown = false;
		minosPrimaryWasDown = false;
		if (minosRestoreCoroutine != null)
		{
			instance.StopCoroutine(minosRestoreCoroutine);
			minosRestoreCoroutine = null;
		}
		RestoreRecorder();
	}

	private static void PlayMinosClip(AudioClip clip)
	{
		if ((Object)(object)clip == (Object)null)
		{
			return;
		}
		if ((Object)(object)minosLocalSource == (Object)null)
		{
			GameObject val = new GameObject("MinosAudio");
			Object.DontDestroyOnLoad((Object)(object)val);
			minosLocalSource = val.AddComponent<AudioSource>();
			minosLocalSource.spatialBlend = 0f;
			minosLocalSource.volume = 1f;
		}
		minosLocalSource.Stop();
		minosLocalSource.PlayOneShot(clip, 2f);
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if ((Object)(object)myRecorder != (Object)null)
		{
			if (minosRestoreCoroutine != null)
			{
				instance.StopCoroutine(minosRestoreCoroutine);
			}
			myRecorder.SourceType = Recorder.InputSourceType.AudioClip;
			myRecorder.AudioClip = clip;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = true;
			minosRestoreCoroutine = instance.StartCoroutine(RestoreMicAfter(clip.length));
		}
	}

	private static IEnumerator RestoreMicAfter(float delay)
	{
		yield return (object)new WaitForSeconds(delay + 0.4f);
		if ((Object)(object)instance && ((Behaviour)instance).isActiveAndEnabled)
		{
			RestoreRecorder();
			minosRestoreCoroutine = null;
		}
	}

	private static void RestoreRecorder()
	{
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (!((Object)(object)myRecorder == (Object)null))
		{
			myRecorder.SourceType = Recorder.InputSourceType.Microphone;
			myRecorder.AudioClip = null;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = false;
		}
	}

	private static IEnumerator LoadMinosSounds()
	{
		UnityWebRequest req1 = UnityWebRequestMultimedia.GetAudioClip(MinosCrushUrl, AudioType.MPEG);
		try
		{
			yield return req1.SendWebRequest();
			if ((int)req1.result == 1)
			{
				minosCrushClip = DownloadHandlerAudioClip.GetContent(req1);
			}
		}
		finally
		{
			((IDisposable)req1)?.Dispose();
		}
		UnityWebRequest req2 = UnityWebRequestMultimedia.GetAudioClip(MinosSlamUrl, AudioType.MPEG);
		try
		{
			yield return req2.SendWebRequest();
			if ((int)req2.result == 1)
			{
				minosSlamClip = DownloadHandlerAudioClip.GetContent(req2);
			}
		}
		finally
		{
			((IDisposable)req2)?.Dispose();
		}
	}

	public static void UpdateActiveMods()
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
		// ARS runs AFTER the tag buttons, so ARS sits at the TOP of the tag stack (farthest from head).
		// DO NOT move this above the buttons loop or ARS drops to the BOTTOM.
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
		if (!flag && (Object)(object)pointer != (Object)null)
		{
			Object.Destroy((Object)(object)pointer);
			pointer = null;
			if ((Object)(object)Line != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)Line).gameObject);
				Line = null;
			}
			gunTriggerWasDown = false;
		}
		WristMenu.UpdateGradientAnimations(Time.time);
		ConsoleMods.Run();
		Console.UpdateAdminIndicators();
	}

	public static void EnableThirdPerson()
	{
		thirdPersonEnabled = true;
		if ((Object)(object)FreeCamObject == (Object)null)
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
		thirdPersonViewActive = false;
		if ((Object)(object)FreeCamObject != (Object)null)
		{
			Object.Destroy((Object)(object)FreeCamObject.GetComponent<Camera>());
			Object.Destroy((Object)(object)FreeCamObject);
			FreeCamObject = null;
		}
	}

	private static void DisableThirdPersonView()
	{
		if ((Object)(object)FreeCamObject != (Object)null)
		{
			Object.Destroy((Object)(object)FreeCamObject.GetComponent<Camera>());
			Object.Destroy((Object)(object)FreeCamObject);
			FreeCamObject = null;
		}
	}

	public static void BoxEspRender()
	{
		reusableBoxEspRemovals.Clear();
		foreach (KeyValuePair<VRRig, GameObject> item in boxEspObjects.Where((KeyValuePair<VRRig, GameObject> box) => !VRRigCache.ActiveRigs.Contains(box.Key)))
		{
			reusableBoxEspRemovals.Add(item.Key);
			Object.Destroy((Object)(object)item.Value);
		}
		foreach (VRRig item2 in reusableBoxEspRemovals)
		{
			boxEspObjects.Remove(item2);
		}
		foreach (VRRig item3 in VRRigCache.ActiveRigs.Where((VRRig rig) => !rig.isLocal))
		{
			if (!boxEspObjects.TryGetValue(item3, out var value))
			{
				value = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy((Object)(object)value.GetComponent<BoxCollider>());
				value.GetComponent<Renderer>().enabled = false;
				value.transform.localScale = new Vector3(0.8f, 0.85f, 0f);
				Shader shader = CachedGuiTextShader;
				float num = 0.08f;
				GameObject val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, 0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0f, -0.425f, 0f);
				val.transform.localScale = new Vector3(0.8f, num, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				val = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.Destroy((Object)(object)val.GetComponent<BoxCollider>());
				val.transform.SetParent(value.transform);
				val.transform.localPosition = new Vector3(-0.4f, 0f, 0f);
				val.transform.localScale = new Vector3(num, 0.85f, 1f);
				val.GetComponent<Renderer>().material.shader = shader;
				boxEspObjects.Add(item3, value);
			}
			Color color = item3.playerColor;
			try
			{
				GorillaGameManager val2 = GorillaGameManager.instance;
				if ((Object)(object)val2 != (Object)null)
				{
					GorillaTagManager val3 = (GorillaTagManager)(object)((val2 is GorillaTagManager) ? val2 : null);
					if (val3 != null && item3.Creator != null && val3.IsInfected(item3.Creator))
					{
						color = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			value.transform.position = ((Component)item3).transform.position;
			value.transform.LookAt(GorillaTagger.Instance.headCollider.transform.position);
			foreach (Transform item4 in value.transform)
			{
				Transform val4 = item4;
				Renderer component = ((Component)val4).GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					component.material.color = color;
				}
			}
		}
	}

	public static void DisableBoxEsp()
	{
		foreach (KeyValuePair<VRRig, GameObject> boxEspObject in boxEspObjects)
		{
			Object.Destroy((Object)(object)boxEspObject.Value);
		}
		boxEspObjects.Clear();
	}

	public static void Tracers()
	{
		reusableTracerRemovals.Clear();
		foreach (KeyValuePair<Player, LineRenderer> tracerLine in tracerLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(tracerLine.Key))
			{
				reusableTracerRemovals.Add(tracerLine.Key);
			}
		}
		if (reusableTracerRemovals.Count > 0)
		{
			foreach (Player item in reusableTracerRemovals)
			{
				Object.Destroy((Object)(object)((Component)tracerLines[item]).gameObject);
				tracerLines.Remove(item);
			}
		}
		Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
		foreach (Player val in playerListOthers)
		{
			VRRig vRRigFromPlayer = GorillaGameManager.StaticFindRigForPlayer(val);
			if ((Object)(object)vRRigFromPlayer == (Object)null)
			{
				continue;
			}
			if (!tracerLines.TryGetValue(val, out var value))
			{
				GameObject val2 = new GameObject("TracerLine");
				((Object)val2).hideFlags = HideFlags.HideAndDontSave;
				value = val2.AddComponent<LineRenderer>();
				value.startWidth = 0.01f;
				value.endWidth = 0.01f;
				value.positionCount = 2;
				value.useWorldSpace = true;
				((Renderer)value).material.shader = CachedGuiTextShader;
				tracerLines[val] = value;
			}
			value.SetPosition(0, GetTracerStart());
			value.SetPosition(1, vRRigFromPlayer.transform.position);
			Color val3 = vRRigFromPlayer.playerColor;
			try
			{
				GorillaGameManager val4 = GorillaGameManager.instance;
				if ((Object)(object)val4 != (Object)null)
				{
					GorillaTagManager val5 = (GorillaTagManager)(object)((val4 is GorillaTagManager) ? val4 : null);
					if (val5 != null && vRRigFromPlayer.Creator != null && val5.IsInfected(vRRigFromPlayer.Creator))
					{
						val3 = new Color(1f, 0.5f, 0f);
					}
				}
			}
			catch
			{
			}
			val3.a = 0.3f;
			value.startColor = val3;
			value.endColor = val3;
		}
	}

	private static Vector3 GetTracerStart()
	{
		if (!XRSettings.isDeviceActive && (object)ghostRig != (Object)null && GhostWanted())
		{
			Transform ghostHand = null;
			try { ghostHand = ghostRig.rightHand?.rigTarget?.transform; } catch { }
			if ((object)ghostHand != (Object)null) return ghostHand.position;
			return ghostRig.transform.position + Vector3.up * 0.2f;
		}
		return GTPlayer.Instance.RightHand.controllerTransform.position;
	}

	public static void DisableTracers()
	{
		foreach (LineRenderer value in tracerLines.Values)
		{
			Object.Destroy((Object)(object)((Component)value).gameObject);
		}
		tracerLines.Clear();
	}

	// Seralyth-Menu bone index pairs: 19 connections using mainSkin.bones indices
	private static readonly int[] bonePairs = {
		4,3, 5,4, 19,18, 20,19, 3,18, 21,20, 22,21, 25,21, 29,21, 31,29,
		27,25, 24,22, 6,5, 7,6, 10,6, 14,6, 16,14, 12,10, 9,7
	};
	private static readonly int boneConnCount = 19;

	public static void SkeletonEsp()
	{
		reusableSkeletonRemovals.Clear();
		foreach (var kvp in skeletonLines)
		{
			if (!PhotonNetwork.PlayerListOthers.Contains(kvp.Key))
			{
				reusableSkeletonRemovals.Add(kvp.Key);
			}
		}
		if (reusableSkeletonRemovals.Count > 0)
		{
			foreach (Player p in reusableSkeletonRemovals)
			{
				foreach (LineRenderer lr in skeletonLines[p])
					Object.Destroy(((Component)lr).gameObject);
				skeletonLines.Remove(p);
			}
		}

		// Total lines: 1 (head vertical) + 19 (bone index connections) + up to 6 (fingers) = 26
		int totalLines = 1 + boneConnCount; // base lines
		int fingerStart = totalLines; // fingers added on the fly if bones found

		foreach (Player player in PhotonNetwork.PlayerListOthers)
		{
			VRRig rig = GorillaGameManager.StaticFindRigForPlayer(player);
			if (rig == null) continue;
			if (rig.mainSkin == null || rig.mainSkin.bones == null) continue;
			if (rig.head == null || rig.head.rigTarget == null) continue;

			Transform[] bones = rig.mainSkin.bones;

			if (!skeletonLines.TryGetValue(player, out var lines))
			{
				int count = totalLines;
				lines = new LineRenderer[count + 6]; // room for fingers
				for (int i = 0; i < count + 6; i++)
					lines[i] = null;

				// Create base line renderers
				for (int i = 0; i < count; i++)
				{
					GameObject obj = new GameObject("skel" + i);
					LineRenderer lr = obj.AddComponent<LineRenderer>();
					lr.startWidth = 0.025f;
					lr.endWidth = 0.025f;
					lr.positionCount = 2;
					lr.useWorldSpace = true;
					lr.material = new Material(CachedGuiTextShader);
					lines[i] = lr;
				}
				skeletonLines[player] = lines;
			}

			Color color = rig.playerColor;
			try
			{
				GorillaGameManager gm = GorillaGameManager.instance;
				if (gm != null && gm is GorillaTagManager tgm && rig.Creator != null && tgm.IsInfected(rig.Creator))
					color = new Color(1f, 0.5f, 0f);
			}
			catch { }

			if (color.r == 0f && color.g == 0f && color.b == 0f)
				color = Color.white;

			// Line 0: head vertical line (same as Seralyth)
			Vector3 headPos = rig.head.rigTarget.position;
			LineRenderer headLine = lines[0];
			headLine.startColor = color;
			headLine.endColor = color;
			headLine.SetPosition(0, headPos + new Vector3(0f, 0.16f, 0f));
			headLine.SetPosition(1, headPos - new Vector3(0f, 0.4f, 0f));

			// Lines 1-19: bone index connections
			for (int i = 0; i < boneConnCount; i++)
			{
				int idxA = bonePairs[i * 2];
				int idxB = bonePairs[i * 2 + 1];
				if (idxA >= bones.Length || idxB >= bones.Length) continue;
				if (bones[idxA] == null || bones[idxB] == null) continue;

				LineRenderer lr = lines[1 + i];
				lr.startColor = color;
				lr.endColor = color;
				lr.SetPosition(0, bones[idxA].position);
				lr.SetPosition(1, bones[idxB].position);
			}

			// Extra: finger connections by name lookup
			VRMap lm = rig.leftHand;
			Vector3 lHand = (lm != null && lm.rigTarget != null) ? lm.rigTarget.position : headPos;
			VRMap rm = rig.rightHand;
			Vector3 rHand = (rm != null && rm.rigTarget != null) ? rm.rigTarget.position : headPos;

			Vector3 forward = rig.head.rigTarget.forward;
			Vector3 right = rig.head.rigTarget.right;

			Transform lThumbT = FindBoneTransform(rig, "thumb.03.L");
			Transform lIndexT = FindBoneTransform(rig, "f_index.02.L");
			Transform lMiddleT = FindBoneTransform(rig, "f_middle.02.L");
			Transform rThumbT = FindBoneTransform(rig, "thumb.03.R");
			Transform rIndexT = FindBoneTransform(rig, "f_index.02.R");
			Transform rMiddleT = FindBoneTransform(rig, "f_middle.02.R");

			Vector3 lThumb = lThumbT != null ? lThumbT.position : lHand - right * 0.05f + forward * 0.03f;
			Vector3 lIndex = lIndexT != null ? lIndexT.position : lHand + forward * 0.06f;
			Vector3 lMiddle = lMiddleT != null ? lMiddleT.position : lHand + forward * 0.06f - right * 0.02f;
			Vector3 rThumb = rThumbT != null ? rThumbT.position : rHand + right * 0.05f + forward * 0.03f;
			Vector3 rIndex = rIndexT != null ? rIndexT.position : rHand + forward * 0.06f;
			Vector3 rMiddle = rMiddleT != null ? rMiddleT.position : rHand + forward * 0.06f + right * 0.02f;

			reusableFingerConns[0] = (lHand, lThumb);
			reusableFingerConns[1] = (lHand, lIndex);
			reusableFingerConns[2] = (lHand, lMiddle);
			reusableFingerConns[3] = (rHand, rThumb);
			reusableFingerConns[4] = (rHand, rIndex);
			reusableFingerConns[5] = (rHand, rMiddle);

			for (int i = 0; i < 6; i++)
			{
				LineRenderer lr = lines[fingerStart + i];
				if (lr == null)
				{
					GameObject obj = new GameObject("finger" + i);
					lr = obj.AddComponent<LineRenderer>();
					lr.startWidth = 0.025f;
					lr.endWidth = 0.025f;
					lr.positionCount = 2;
					lr.useWorldSpace = true;
					lr.material = new Material(CachedGuiTextShader);
					lines[fingerStart + i] = lr;
				}
				lr.startColor = color;
				lr.endColor = color;
				lr.SetPosition(0, reusableFingerConns[i].Item1);
				lr.SetPosition(1, reusableFingerConns[i].Item2);
			}
		}
	}

	private static Transform FindBoneTransform(VRRig rig, string prefix)
	{
		if (rig.mainSkin != null && rig.mainSkin.bones != null)
		{
			foreach (Transform b in rig.mainSkin.bones)
			{
				if (b != null && b.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					return b;
			}
		}
		return null;
	}

	public static void DisableSkeletonEsp()
	{
		foreach (LineRenderer[] arr in skeletonLines.Values)
		{
			foreach (LineRenderer lr in arr)
				Object.Destroy(((Component)lr).gameObject);
		}
		skeletonLines.Clear();
	}

	private static int GetFps(VRRig rig)
	{
		if (_fpsField == null)
		{
			_fpsField = AccessTools.Field(typeof(VRRig), "fps");
		}
		if (_fpsField == null)
		{
			return 0;
		}
		object value = _fpsField.GetValue(rig);
		return (value is int fps) ? fps : 0;
	}

	public static float GetTagStackOffset(VRRig rig, int slot)
	{
		int rank = 0;
		for (int s = 0; s < slot; s++)
		{
			if (IsTagActiveForRig(rig, s))
			{
				rank++;
			}
		}
		return 0.55f + (float)rank * 0.15f;
	}

	private static bool IsTagActiveForRig(VRRig rig, int slot)
	{
		switch (slot)
		{
			case TagStackConsole:
				return Console.HasConsoleIndicator(rig);
			case TagStackCosmetics:
				return cosmeticNameTagObjects.ContainsKey(rig);
			case TagStackId:
				return idNameTagObjects.ContainsKey(rig);
			case TagStackPlatform:
				return platformNameTagObjects.ContainsKey(rig);
			case TagStackName:
				return nameTagObjects.ContainsKey(rig);
			case TagStackFps:
				return fpsNameTagObjects.ContainsKey(rig);
			case TagStackArs:
				return arsTagObjects.ContainsKey(rig);
			case TagStackCrown:
				return Console.conePool.ContainsKey(rig);
			default:
				return false;
		}
	}

	public static Vector3 GetTagPosition(VRRig rig, int slot)
	{
		VRMap head = rig.head;
		Vector3? obj;
		if (head == null)
		{
			obj = null;
		}
		else
		{
			Transform rigTarget = head.rigTarget;
			obj = ((rigTarget != null) ? new Vector3?(rigTarget.position) : ((Vector3?)null));
		}
		Vector3 val = (Vector3)(obj ?? (rig.transform.position + Vector3.up * 1.6f));
		return val + Vector3.up * GetTagStackOffset(rig, slot);
	}

	internal static void BillboardTag(GameObject obj)
	{
		if (!((Object)(object)Camera.main == (Object)null))
		{
			Vector3 position = obj.transform.position;
			obj.transform.LookAt(2f * position - Camera.main.transform.position);
		}
	}

	internal static Text CreateTagObj(string name, Dictionary<VRRig, GameObject> dict, VRRig rig)
	{
		if ((Object)(object)comicSansFont == (Object)null)
		{
			comicSansFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 36);
		}
		GameObject val = new GameObject(name);
		Canvas val2 = val.AddComponent<Canvas>();
		val2.renderMode = RenderMode.WorldSpace;
		((Component)val2).transform.localScale = Vector3.one * 0.003f;
		Text val3 = val.AddComponent<Text>();
		if ((Object)(object)comicSansFont != (Object)null)
		{
			val3.font = comicSansFont;
		}
		val3.fontSize = 30;
		val3.horizontalOverflow = HorizontalWrapMode.Overflow;
		val3.alignment = TextAnchor.MiddleCenter;
		((Graphic)val3).color = rig.playerColor;
		dict[rig] = val;
		return val3;
	}

	private static Color TagColor(VRRig rig)
	{
		Color playerColor = rig.playerColor;
		if (playerColor.r == 0f && playerColor.g == 0f && playerColor.b == 0f)
		{
			return Color.white;
		}
		return playerColor;
	}

	private static void CleanTagDict(Dictionary<VRRig, GameObject> dict)
	{
		reusableTagRemovals.Clear();
		foreach (KeyValuePair<VRRig, GameObject> item in dict)
		{
			if (!VRRigCache.ActiveRigs.Contains(item.Key))
			{
				reusableTagRemovals.Add(item.Key);
			}
		}
		if (reusableTagRemovals.Count == 0)
		{
			return;
		}
		foreach (VRRig item2 in reusableTagRemovals)
		{
			Object.Destroy((Object)(object)dict[item2]);
			dict.Remove(item2);
		}
	}

	public static void NameTags()
	{
		CleanTagDict(nameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!nameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_Nametag", nameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.NickName : null) ?? "?";
				val.text = text;
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					NetPlayer creator2 = activeRig.Creator;
					string text2 = ((creator2 != null) ? creator2.NickName : null) ?? "?";
					component.text = text2;
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig, TagStackName);
			BillboardTag(value);
		}
	}

	public static void DisableNameTags()
	{
		foreach (GameObject value in nameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		nameTagObjects.Clear();
	}

	public static void FPSTags()
	{
		CleanTagDict(fpsNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!fpsNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_FPStag", fpsNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				string text = GetFps(activeRig) + " FPS";
				val.text = text;
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					string text2 = GetFps(activeRig) + " FPS";
					component.text = text2;
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig, TagStackFps);
			BillboardTag(value);
		}
	}

	public static void DisableFPSTags()
	{
		foreach (GameObject value in fpsNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		fpsNameTagObjects.Clear();
	}

	public static void IDTags()
	{
		CleanTagDict(idNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			if (!idNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_IDtag", idNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.UserId : null) ?? "?";
				val.text = text;
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					NetPlayer creator2 = activeRig.Creator;
					string text2 = ((creator2 != null) ? creator2.UserId : null) ?? "?";
					component.text = text2;
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig, TagStackId);
			BillboardTag(value);
		}
	}

	public static void DisableIDTags()
	{
		foreach (GameObject value in idNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		idNameTagObjects.Clear();
	}

	public static void PlatformTags()
	{
		CleanTagDict(platformNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal)
			{
				continue;
			}
			string text = GetPlatformProperty(activeRig);
			if (!platformNameTagObjects.TryGetValue(activeRig, out var value))
			{
				Text val = CreateTagObj("Chud_PlatformTag", platformNameTagObjects, activeRig);
				value = ((Component)val).gameObject;
				val.text = text;
				((Graphic)val).color = TagColor(activeRig);
			}
			else
			{
				Text component = value.GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					component.text = text;
					((Graphic)component).color = TagColor(activeRig);
				}
			}
			value.transform.position = GetTagPosition(activeRig, TagStackPlatform);
			BillboardTag(value);
		}
	}

	private static string GetPlatformProperty(VRRig rig)
	{
		NetPlayer creator = rig.Creator;
		if (creator == null || creator.UserId == null)
		{
			return null;
		}
		Player photonPlayer = null;
		if (PhotonNetwork.InRoom)
		{
			foreach (Player player in PhotonNetwork.PlayerList)
			{
				if (player.UserId == creator.UserId)
				{
					photonPlayer = player;
					break;
				}
			}
		}
		ExitGames.Client.Photon.Hashtable customProperties = (photonPlayer != null) ? photonPlayer.CustomProperties : null;
		if (customProperties == null || customProperties.Count == 0)
		{
			return null;
		}
		object platformValue;
		if (customProperties.TryGetValue("platform", out platformValue) && platformValue != null)
		{
			return "Platform: " + platformValue;
		}
		return null;
	}

	public static void DisablePlatformTags()
	{
		foreach (GameObject value in platformNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		platformNameTagObjects.Clear();
	}

	private static HashSet<string> GetOwnedCosmetics(VRRig rig)
	{
		if (_ownedCosmeticsField == null)
		{
			_ownedCosmeticsField = AccessTools.Field(typeof(VRRig), "_playerOwnedCosmetics");
		}
		return _ownedCosmeticsField?.GetValue(rig) as HashSet<string>;
	}

	public static void CosmeticNameTags()
	{
		CleanTagDict(cosmeticNameTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal || cosmeticNameTagObjects.ContainsKey(activeRig))
			{
				continue;
			}
			HashSet<string> ownedCosmetics = GetOwnedCosmetics(activeRig);
			if (ownedCosmetics == null || ownedCosmetics.Count == 0)
			{
				continue;
			}
			List<string> list = new List<string>(ownedCosmetics.Count);
			foreach (string item in ownedCosmetics)
			{
				if (cosmeticNames.TryGetValue(item, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count != 0)
			{
				string text = string.Join(", ", list);
				Text val = CreateTagObj("Chud_CosmeticTag", cosmeticNameTagObjects, activeRig);
				val.text = text;
				((Graphic)val).color = Color.red;
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> cosmeticNameTagObject in cosmeticNameTagObjects)
		{
			cosmeticNameTagObject.Value.transform.position = GetTagPosition(cosmeticNameTagObject.Key, TagStackCosmetics);
			BillboardTag(cosmeticNameTagObject.Value);
		}
	}

	public static void DisableCosmeticNameTags()
	{
		foreach (GameObject value in cosmeticNameTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		cosmeticNameTagObjects.Clear();
	}

	public static void EnableARS()
	{
		arsActive = true;
		if (!arsDownloaded && !arsDownloading)
		{
			arsDownloading = true;
			_ = AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARS()
	{
		arsActive = false;
		if (arsNameTagsActive)
		{
			return;
		}
		foreach (GameObject value in arsTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		arsTagObjects.Clear();
	}

	public static void EnableARSNameTags()
	{
		arsNameTagsActive = true;
		if (!arsDownloaded)
		{
			_ = AsyncGetARSPlayerIDs();
		}
	}

	public static void DisableARSNameTags()
	{
		arsNameTagsActive = false;
		if (arsActive)
		{
			return;
		}
		foreach (GameObject value in arsTagObjects.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		arsTagObjects.Clear();
	}

	public static void ARSNameTagUpdate()
	{
		if (!arsNameTagsActive || arsPlayersToReport.Count == 0)
		{
			return;
		}
		CleanTagDict(arsTagObjects);
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (!activeRig.isLocal)
			{
				NetPlayer creator = activeRig.Creator;
				string text = ((creator != null) ? creator.UserId : null);
				if (text != null && arsPlayersToReport.Contains(text) && !arsTagObjects.ContainsKey(activeRig))
				{
					Text val = CreateTagObj("Chud_ARStag", arsTagObjects, activeRig);
					val.text = "ARS";
					((Graphic)val).color = Color.red;
				}
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> arsTagObject in arsTagObjects)
		{
			arsTagObject.Value.transform.position = GetTagPosition(arsTagObject.Key, TagStackArs);
			BillboardTag(arsTagObject.Value);
		}
	}

	public static void ARSDetect()
	{
		if (arsActive && arsPlayersToReport.Count != 0 && PhotonNetwork.InRoom)
		{
			string name = PhotonNetwork.CurrentRoom.Name;
			if (name != arsLastCheckedRoom)
			{
				arsLastCheckedRoom = name;
				instance.StartCoroutine(ARSDelayedCheck());
			}
		}
	}

	private static IEnumerator ARSDelayedCheck()
	{
		yield return (object)new WaitForSeconds(Random.Range(2.5f, 10f));
		ARSCheckAllPlayers();
	}

	private static void ARSCheckAllPlayers()
	{
		if (PhotonNetwork.InRoom)
		{
			Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
			foreach (Player photonPlayer in playerListOthers)
			{
				ARSCheckPlayer(photonPlayer);
			}
		}
	}

	public static void ARSCheckPlayer(Player photonPlayer)
	{
		if (!arsActive || arsPlayersToReport.Count == 0)
		{
			return;
		}
		string userId = photonPlayer.UserId;
		if (userId == null || !arsPlayersToReport.Contains(userId))
		{
			return;
		}
		string text = photonPlayer.NickName ?? userId;
		NotifiLib.SendNotification(text + " is on ARS", 3);
		foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (allScoreboardLine.linePlayer == NetworkSystem.Instance.GetNetPlayerByID(photonPlayer.ActorNumber))
			{
				allScoreboardLine.PressButton(true, GorillaPlayerLineButton.ButtonType.Toxicity);
				break;
			}
		}
	}

	private static async Task AsyncGetARSPlayerIDs()
	{
		try
		{
			string raw = (await arsHttpClient.GetStringAsync("https://raw.githubusercontent.com/AutoReportSystem/ARSPlayerIDs/refs/heads/main/Player%20Ids.txt")).Trim();
			HashSet<string> ids = (arsPlayersToReport = (from id in raw.Split(',')
				select id.Trim() into id
				where !StringUtils.IsNullOrEmpty(id)
				select id).ToHashSet());
			arsDownloaded = true;
			System.Console.WriteLine("[ARS] Loaded " + ids.Count + " player IDs to detect");
		}
		catch (Exception ex)
		{
			Exception e = ex;
			System.Console.WriteLine("[ARS] Failed to download player IDs: " + e.Message);
			arsDownloaded = false;
		}
		arsDownloading = false;
	}

	public static void CosmeticNotifier()
	{
		cosmeticNotifierActive = true;
	}

	public static void DisableCosmeticNotifier()
	{
		cosmeticNotifierActive = false;
		cosmeticNotifierNotified.Clear();
	}

	private static void UpdateCosmeticNotifier()
	{
		if (!cosmeticNotifierActive)
		{
			return;
		}
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			if (activeRig.isLocal || activeRig.Creator == null)
			{
				continue;
			}
			HashSet<string> ownedCosmetics = GetOwnedCosmetics(activeRig);
			if (ownedCosmetics == null || ownedCosmetics.Count == 0)
			{
				continue;
			}
			string userId = activeRig.Creator.UserId;
			if (cosmeticNotifierNotified.Contains(userId))
			{
				continue;
			}
			List<string> list = new List<string>(ownedCosmetics.Count);
			foreach (string item in ownedCosmetics)
			{
				if (cosmeticNames.TryGetValue(item, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count != 0)
			{
				cosmeticNotifierNotified.Add(userId);
				NotifiLib.SendNotification(activeRig.Creator.NickName + ": " + string.Join(", ", list), 5);
			}
		}
	}

	private static readonly HttpClient _trackHttp = new HttpClient();
	private static readonly HashSet<string> _trackedReported = new HashSet<string>();
	private static string _trackRoom = "";
	private static string _trackRoomPrivacy = "";

	public static void TrackedCosmeticsScan()
	{
		if (!PhotonNetwork.InRoom) return;
		_trackedReported.Clear();
		_trackRoom = PhotonNetwork.CurrentRoom.Name;
		_trackRoomPrivacy = PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private";
		instance.StartCoroutine(DelayedRoomScan());
	}

	private static IEnumerator DelayedRoomScan()
	{
		if (!PhotonNetwork.InRoom) yield break;
		for (int attempt = 0; attempt < 5; attempt++)
		{
			yield return new WaitForSeconds(3f);
			if (!PhotonNetwork.InRoom) yield break;
			bool allDone = true;
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (rig.isLocal || rig.Creator == null) continue;
				string key = rig.Creator.UserId + "|" + _trackRoom;
				if (_trackedReported.Contains(key)) continue;
				HashSet<string> cosmetics = GetOwnedCosmetics(rig);
				if (cosmetics == null || cosmetics.Count == 0)
				{
					allDone = false;
					continue;
				}
				CheckAndReport(rig.Creator.UserId, rig.Creator.NickName, cosmetics);
			}
			if (allDone) break;
		}
	}

	public static void TrackedCosmeticsCheckPlayer(Player player)
	{
		if (player == null) return;
		_trackRoom = PhotonNetwork.CurrentRoom.Name;
		_trackRoomPrivacy = PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private";
		instance.StartCoroutine(DelayedPlayerCheck(player));
	}

	private static IEnumerator DelayedPlayerCheck(Player player)
	{
		if (player == null) yield break;
		for (int attempt = 0; attempt < 5; attempt++)
		{
			yield return new WaitForSeconds(3f);
			if (player == null || !PhotonNetwork.InRoom) yield break;
			VRRig rig = null;
			foreach (VRRig r in VRRigCache.ActiveRigs)
			{
				if (r.Creator != null && r.Creator.UserId == player.UserId)
				{
					rig = r;
					break;
				}
			}
			if ((Object)(object)rig == (Object)null) continue;
			HashSet<string> cosmetics = GetOwnedCosmetics(rig);
			if (cosmetics == null || cosmetics.Count == 0) continue;
			CheckAndReport(player.UserId, player.NickName, cosmetics);
			yield break;
		}
	}

	private static void CheckAndReport(string uid, string nick, HashSet<string> owned)
	{
		if (owned == null || owned.Count == 0) return;
		string key = uid + "|" + _trackRoom;
		if (_trackedReported.Contains(key)) return;
		List<string> found = new List<string>();
		foreach (string c in owned)
		{
			if (trackedWebhookCosmetics.Contains(c) && cosmeticNames.TryGetValue(c, out var n))
				found.Add(n);
		}
		if (found.Count == 0) return;
		_trackedReported.Add(key);
		SendTrackedWebhook(nick, uid, found);
	}

	private static async void SendTrackedWebhook(string nick, string uid, List<string> cosmetics)
	{
		try
		{
			string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
			string json = "{\"embeds\":[{\"title\":\"Tracked Cosmetic Detected\",\"color\":16730112,\"fields\":[{\"name\":\"Player\",\"value\":\"" + nick + "\",\"inline\":true},{\"name\":\"User ID\",\"value\":\"" + uid + "\",\"inline\":true},{\"name\":\"Cosmetics\",\"value\":\"" + string.Join(", ", cosmetics) + "\",\"inline\":false},{\"name\":\"Room\",\"value\":\"" + _trackRoom + "\",\"inline\":true},{\"name\":\"Type\",\"value\":\"" + _trackRoomPrivacy + "\",\"inline\":true},{\"name\":\"Time\",\"value\":\"" + time + "\",\"inline\":true}]}]}";
			await _trackHttp.PostAsync(Zcdcece.Get(), new StringContent(json, Encoding.UTF8, "application/json"));
		}
		catch { }
	}

	public static void Save()
	{
		try
		{
			if (!Directory.Exists(WristMenu.FolderName))
				Directory.CreateDirectory(WristMenu.FolderName);

			var root = new JObject();

			var enabledButtons = new JArray();
			foreach (MenuCategory category in MenuManager.Categories)
			{
				if (category.Buttons == null || category.Name == "Enabled Mods") continue;
				foreach (ButtonInfo button in category.Buttons)
				{
					if (button.enabled.HasValue && button.enabled.Value && !string.IsNullOrEmpty(button.buttonText))
						enabledButtons.Add(button.buttonText);
				}
			}
			root["EnabledButtons"] = enabledButtons;

			root["FlySpeed"] = flySpeed;
			root["SpeedboostCycle"] = speedboostCycle;
			root["PullPowerInt"] = pullPowerInt;
			root["WasdFlyMouseSense"] = wasdFlyMouseSense;
			root["IsRightHanded"] = isRightHanded;
			root["MenuColorIndex"] = menuColorIndex;
			root["NotificationTimeIndex"] = notificationTimeIndex;
			root["Jspeed"] = jspeed;
			root["Jmulti"] = jmulti;
			root["TagAuraRange"] = tagAuraRange;
			root["TagAuraRangeIndex"] = tagAuraRangeIndex;
			root["AdminScale"] = Console.adminScale;
			root["CustomSoundUrl"] = ConsoleMods.customSoundUrl;
			root["CustomVideoUrl"] = ConsoleMods.customVideoUrl;
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

			root["LaserColorIndex"] = ConsoleMods.laserColorIndex;
			root["SelectedSoundIndex"] = ConsoleMods.selectedSoundIndex;
			root["SelectedVideoIndex"] = ConsoleMods.selectedVideoIndex;

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
				File.Delete(ConfigPath);
			File.Move(tempPath, ConfigPath);
		}
		catch { }
	}

	public static void Load()
	{
		try
		{
			if (!File.Exists(ConfigPath))
			{
				string tempPath = ConfigPath + ".tmp";
				if (File.Exists(tempPath))
					File.Move(tempPath, ConfigPath);
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
			ConsoleMods.laserColorIndex = (int)(root["LaserColorIndex"] ?? 0);
			wasdFlyMouseSense = (float)(root["WasdFlyMouseSense"] ?? 1f);
			isRightHanded = (bool)(root["IsRightHanded"] ?? false);
			menuColorIndex = (int)(root["MenuColorIndex"] ?? 0);
			notificationTimeIndex = (int)(root["NotificationTimeIndex"] ?? 5);
			jspeed = (float)(root["Jspeed"] ?? 7.5f);
			jmulti = (float)(root["Jmulti"] ?? 1.1f);
			tagAuraRange = (float)(root["TagAuraRange"] ?? 1.5f);
			tagAuraRangeIndex = (int)(root["TagAuraRangeIndex"] ?? 3);

			Console.adminScale = (float)(root["AdminScale"] ?? 1f);
			ConsoleMods.customSoundUrl = (string)root["CustomSoundUrl"] ?? "";
			ConsoleMods.customVideoUrl = (string)root["CustomVideoUrl"] ?? "";

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
			Console.allowKickSelf = (bool)(root["ConsoleAllowKickSelf"] ?? false);
			Console.allowTpSelf = (bool)(root["ConsoleAllowTpSelf"] ?? true);
			Console.disableFlingSelf = (bool)(root["ConsoleDisableFlingSelf"] ?? false);
			Console.laserEnabled = (bool)(root["ConsoleLaserEnabled"] ?? false);
			Console.autoDetectConsoleUsers = (bool)(root["ConsoleAutoDetectConsoleUsers"] ?? false);
			Console.consoleLogging = (bool)(root["ConsoleLogging"] ?? false);
			Console.fullAutoPistol = (bool)(root["ConsoleFullAutoPistol"] ?? false);

			ConsoleMods.selectedSoundIndex = (int)(root["SelectedSoundIndex"] ?? 0);
			ConsoleMods.selectedVideoIndex = (int)(root["SelectedVideoIndex"] ?? 0);
			if (ConsoleMods.laserColorIndex < 0 || ConsoleMods.laserColorIndex >= ConsoleMods.laserColors.Length)
				ConsoleMods.laserColorIndex = 0;
			if (menuColorIndex < 0 || menuColorIndex >= 10)
				menuColorIndex = 0;
			if (notificationTimeIndex < 0 || notificationTimeIndex >= notificationTimeValues.Length)
				notificationTimeIndex = 5 % notificationTimeValues.Length;
			if (antiReportRangeIndex < 0 || antiReportRangeIndex >= antiReportRanges.Length)
				antiReportRangeIndex = 1 % antiReportRanges.Length;
			if (waterSplashSpeedIndex < 0)
				waterSplashSpeedIndex = 0;
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
				var buttonLookup = new Dictionary<string, ButtonInfo>(StringComparer.Ordinal);
				foreach (MenuCategory cat in MenuManager.Categories)
				{
					if (cat.Buttons == null || cat.Name == "Enabled Mods") continue;
					foreach (ButtonInfo btn in cat.Buttons)
					{
						if (btn.type != ButtonType.Action && btn.enabled.HasValue && !string.IsNullOrEmpty(btn.buttonText) && !buttonLookup.ContainsKey(btn.buttonText))
							buttonLookup[btn.buttonText] = btn;
					}
				}

				var savedSet = new HashSet<string>(StringComparer.Ordinal);
				foreach (JToken token in savedButtons)
				{
					string savedName = (string)token;
					if (!string.IsNullOrEmpty(savedName)) savedSet.Add(savedName);
				}

				foreach (var kvp in buttonLookup)
				{
					if (kvp.Value.enabled == true && !savedSet.Contains(kvp.Value.buttonText))
					{
						try { kvp.Value.disableMethod?.Invoke(); } catch { }
						kvp.Value.enabled = false;
					}
				}

				foreach (string savedName in savedSet)
				{
					if (buttonLookup.TryGetValue(savedName, out var btn) && btn != null && btn.enabled != true)
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
		foreach (MenuCategory category in MenuManager.Categories)
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
		if (!notificationsEnabled)
		{
			NotifiLib.IsEnabled = true;
			notificationsEnabled = true;
		}
	}

	public static void DisableNotifications()
	{
		if (notificationsEnabled)
		{
			NotifiLib.IsEnabled = false;
			notificationsEnabled = false;
		}
	}

	public static void ClearNotifications()
	{
		NotifiLib.ClearAllNotifications();
	}

	public static void SetNotificationTime(int index)
	{
		notificationTimeIndex = index % notificationTimeValues.Length;
		if (notificationTimeIndex < 0) notificationTimeIndex = notificationTimeValues.Length - 1;
		notificationDecayTime = notificationTimeValues[notificationTimeIndex];
		NotifiLib.DecayTime = notificationDecayTime;
		NotifiLib.SendNotification("Notification time: " + notificationTimeNames[notificationTimeIndex]);
	}

	private static void ApplyMenuColor(int index)
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
		ApplyMenuColor(index);
		string[] colorNames = new string[] { "Gray", "Blue", "Red", "Orange", "Green", "Cyan", "Purple", "Magenta", "Pink", "Brown" };
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

	private static void PlatformsThing(bool invis, bool sticky)
	{
		RPlat = WristMenu.gripDownR;
		LPlat = WristMenu.gripDownL;
		if (platMaterial != null) platMaterial.color = WristMenu.ButtonColorEnabled;
		ProcessPlatform(RPlat, ref jump_right_local, ref once_right, ref once_right_false, ref stickyRightActive, true, sticky);
		ProcessPlatform(LPlat, ref jump_left_local, ref once_left, ref once_left_false, ref stickyLeftActive, false, sticky);
	}

	private static void ProcessPlatform(bool plat, ref GameObject jumpObj, ref bool once, ref bool onceFalse, ref bool stickyActive, bool isRight, bool sticky)
	{
		if (plat)
		{
			if (!once && (Object)(object)jumpObj == (Object)null)
			{
				var hand = isRight ? GTPlayer.Instance.RightHand : GTPlayer.Instance.LeftHand;
				Transform handTransform = isRight ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
					if (sticky)
				{
					Vector3 handPos = handTransform.position;
					jumpObj = new GameObject(isRight ? "StickyRight" : "StickyLeft");
					jumpObj.transform.position = handPos;
					jumpObj.transform.rotation = Quaternion.identity;
					jumpObj.transform.localScale = Vector3.one;
					GameObject platObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
					platObj.transform.SetParent(jumpObj.transform);
					platObj.transform.localScale = scale;
					platObj.transform.localPosition = new Vector3(0f, -0.01f, 0f) + hand.controllerTransform.position - handPos;
					platObj.transform.localRotation = hand.controllerTransform.rotation;
					platObj.AddComponent<GorillaSurfaceOverride>().overrideIndex = 0;
					if (platMaterial == null) platMaterial = new Material(CachedUberShader);
					platObj.GetComponent<Renderer>().material = platMaterial;
					platObj.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
					int boxCount = 60;
					float cageRadius = 0.12f;
					float boxSize = 0.08f;
					float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;
					for (int i = 0; i < boxCount; i++)
					{
						float theta = Mathf.Acos(1f - 2f * (i + 0.5f) / boxCount);
						float phi = 2f * Mathf.PI * i / goldenRatio;
						Vector3 dir = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi), Mathf.Cos(theta));
						GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
						box.transform.SetParent(jumpObj.transform);
						Object.Destroy((Object)(object)box.GetComponent<Renderer>());
						Object.Destroy((Object)(object)box.GetComponent<Rigidbody>());
						box.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
						box.transform.localPosition = dir * cageRadius;
					}
					stickyActive = true;
				}
				else
				{
					jumpObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
					jumpObj.transform.localScale = scale;
					jumpObj.transform.position = new Vector3(0f, -0.01f, 0f) + hand.controllerTransform.position;
					jumpObj.transform.rotation = hand.controllerTransform.rotation;
					GorillaSurfaceOverride surf = jumpObj.AddComponent<GorillaSurfaceOverride>();
					surf.overrideIndex = 0;
					if (platMaterial == null) platMaterial = new Material(CachedUberShader);
					jumpObj.GetComponent<Renderer>().material = platMaterial;
					jumpObj.GetComponent<Renderer>().material.color = WristMenu.ButtonColorEnabled;
				}
				once = true;
				onceFalse = false;
			}
		}
		else if (!onceFalse && (Object)(object)jumpObj != (Object)null)
		{
			Object.Destroy((Object)(object)jumpObj);
			jumpObj = null;
			stickyActive = false;
			once = false;
			onceFalse = true;
		}
	}


	public static void TPGun()
	{
		MakeRightHandGun(delegate
		{
			if ((Object)(object)GTPlayer.Instance != (Object)null && (Object)(object)pointer != (Object)null)
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
		PhotonNetwork.Disconnect();
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
		if (trigger == null)
		{
			NotifiLib.SendNotification("No join trigger found");
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

	private static string savedGroupKickRoom;

	public static void GroupKickAll()
	{
		if (!PhotonNetwork.InRoom || !NetworkSystem.Instance.SessionIsPrivate)
		{
			NotifiLib.SendNotification("Only works in private rooms!");
			return;
		}
		savedGroupKickRoom = PhotonNetwork.CurrentRoom.Name;
		GorillaComputer.instance.OnGroupJoinButtonPress(
			GorillaComputer.instance.groupMapJoinIndex,
			GorillaComputer.instance.friendJoinCollider
		);
		instance.StartCoroutine(RejoinAfterGroupKick());
	}

	private static IEnumerator RejoinAfterGroupKick()
	{
		while (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name == savedGroupKickRoom)
			yield return null;
		yield return new WaitForSeconds(3f);
		if (string.IsNullOrEmpty(savedGroupKickRoom)) yield break;
		for (int i = 0; i < 4; i++)
		{
			if (PhotonNetwork.InRoom)
				PhotonNetwork.Disconnect();
			yield return new WaitForSeconds(2f);
			PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(savedGroupKickRoom, JoinType.Solo);
			float timeout = 10f;
			while (timeout > 0f && (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.Name != savedGroupKickRoom))
			{
				timeout -= Time.deltaTime;
				yield return null;
			}
			if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == savedGroupKickRoom)
			{
				savedGroupKickRoom = null;
				yield break;
			}
		}
		savedGroupKickRoom = null;
	}

	public static void CreateRoom(string roomName, bool pub)
	{
		int savedDecay = NotifiLib.DecayTime;
		NotifiLib.DecayTime = 240;
		NotifiLib.SendNotification("Creating room: " + roomName + ". This works best in city, it will take a bit for people to join", 1);
		NotifiLib.DecayTime = savedDecay;
		var trigger = PhotonNetworkController.Instance.currentJoinTrigger ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");

		bool isSubscribed = false;
		try
		{
			Type subType = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.FirstOrDefault(t => t.FullName == "GorillaTagScripts.SubscriptionManager");
			if (subType != null)
			{
				var method = subType.GetMethod("IsLocalSubscribed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				if (method != null)
					isSubscribed = (bool)method.Invoke(null, null);
			}
		}
		catch { }

		var hash = new ExitGames.Client.Photon.Hashtable
		{
			{ "platform", "OTHER" },
			{ "gameMode", trigger.GetFullDesiredGameModeString() },
			{ "language", "English" },
			{ "fan_club", isSubscribed ? "true" : "false" },
			{ "queueName", GorillaComputer.instance.currentQueue }
		};

		var config = new RoomConfig
		{
			createIfMissing = true,
			isJoinable = true,
			isPublic = pub,
			MaxPlayers = (byte)(isSubscribed ? 20 : 10),
			CustomProps = hash
		};

		NetworkSystem.Instance.ConnectToRoom(roomName, config, -1);
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
		if (vimHarmony != null)
		{
			return;
		}
		vimHarmony = new Harmony("chudmenu.vim");
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
				vimHarmony.Patch((MethodBase)method, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	public static void DisableUnlockVim()
	{
		if (vimHarmony != null)
		{
			vimHarmony.UnpatchSelf();
			vimHarmony = null;
		}
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
		if (antiReportSphere != null) { Object.Destroy(antiReportSphere); antiReportSphere = null; }
	}

	public static void SetAntiReportRange(int index)
	{
		antiReportRangeIndex = index % antiReportRanges.Length;
		if (antiReportRangeIndex < 0) antiReportRangeIndex = antiReportRanges.Length - 1;
		antiReportRange = antiReportRanges[antiReportRangeIndex];
		NotifiLib.SendNotification("Range: " + antiReportRange.ToString("0.00") + "m");
	}

	public static void AntiReportTick()
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

	private static void CreateAntiReportSphere()
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

	public static void AntiReportVisual()
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

	public static bool VimPrefix(ref bool __result)
	{
		__result = true;
		return false;
	}

	public static void TagGun()
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
		if (!((Object)(object)val != (Object)null))
		{
			return;
		}
		GorillaTagManager val2 = (GorillaTagManager)(object)((val is GorillaTagManager) ? val : null);
		if (val2 != null && val2.IsInfected(NetworkSystem.Instance.LocalPlayer) && Time.time > lastUntagSelfTime)
		{
			val2.currentInfected.RemoveAll((NetPlayer p) => p.UserId == NetworkSystem.Instance.LocalPlayer.UserId);
			lastUntagSelfTime = Time.time + 0.3f;
			NotifiLib.SendNotification("Untagged self");
		}
	}

	public static void TagAll()
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
		tagAllTarget = null;
		tagAllTargets = null;
		tagAllIndex = 0;
		tagAllFramesUntilTag = 0;
		UnsubscribeTagRigVisual();
		TryUnsubscribeGhostRig();
		if ((Object)(object)VRRig.LocalRig != (Object)null)
			EnsureLocalRigEnabled();
	}

	private static bool tagRigVisualSubscribed = false;

	private static void TagRigVisualTick()
	{
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)localRig == (Object)null) return;

		VRRig target = null;
		if (tagGunLockedTarget != null && !tagGunLockedTarget.isLocal && (Object)(object)tagGunLockedTarget != (Object)null)
			target = tagGunLockedTarget;
		else if (tagAllTarget != null && !tagAllTarget.isLocal && (Object)(object)tagAllTarget != (Object)null)
			target = tagAllTarget;

		if (target == null) return;

		EnsureLocalRigEnabled();
		Vector3 targetPos = ((Component)target).transform.position - new Vector3(0f, 3f, 0f);
		localRig.transform.position = targetPos;
		if (localRig.head != null && (Object)(object)localRig.head.rigTarget != (Object)null)
			localRig.head.rigTarget.transform.position = targetPos;
		if (localRig.leftHand != null && (Object)(object)localRig.leftHand.rigTarget != (Object)null)
			localRig.leftHand.rigTarget.transform.position = targetPos;
		if (localRig.rightHand != null && (Object)(object)localRig.rightHand.rigTarget != (Object)null)
			localRig.rightHand.rigTarget.transform.position = targetPos;
	}

	private static void SubscribeTagRigVisual()
	{
		if (!tagRigVisualSubscribed)
		{
			TorsoPatch.VRRigLateUpdate += TagRigVisualTick;
			tagRigVisualSubscribed = true;
		}
	}

	private static void UnsubscribeTagRigVisual()
	{
		if (tagRigVisualSubscribed)
		{
			TorsoPatch.VRRigLateUpdate -= TagRigVisualTick;
			tagRigVisualSubscribed = false;
		}
	}

	private static void GhostRigTick()
	{
		VRRig local = VRRig.LocalRig;
		if ((Object)(object)local == (Object)null)
		{
			HideGhostRig();
			return;
		}
		if (!GhostWanted())
		{
			HideGhostRig();
			UnsubscribeGhostRig();
			return;
		}
		EnsureGhostRig();
		if ((Object)(object)ghostRig == (Object)null) return;
		if (!ghostRig.gameObject.activeSelf)
			ghostRig.gameObject.SetActive(true);
		GorillaTagger tagger = GorillaTagger.Instance;
		if ((Object)(object)tagger == (Object)null || (Object)(object)tagger.headCollider == (Object)null)
			return;
		Transform headT = tagger.headCollider.transform;
		float scale = local.scaleFactor;
		if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
			scale = (GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f;
		Quaternion bodyRot = GorillaLocomotion.GTPlayerTransform.BodyRotation;
		Quaternion liveRot = headT.rotation;
		if ((Object)(object)GTPlayer.Instance != (Object)null && (Object)(object)GTPlayer.Instance.mainCamera != (Object)null)
			liveRot = GTPlayer.Instance.mainCamera.transform.rotation;
		Vector3 trackOffset = (local.head != null) ? local.head.trackingPositionOffset : Vector3.zero;
		Vector3 headPos = headT.position + liveRot * trackOffset * scale;
		ghostRig.transform.SetPositionAndRotation(headPos + ghostRig.transform.rotation * local.headBodyOffset * scale, bodyRot);
		if (ghostRig.head != null && (Object)(object)ghostRig.head.rigTarget != (Object)null)
			ghostRig.head.rigTarget.transform.SetPositionAndRotation(headPos, liveRot);
		Transform liveOffset = ((Object)(object)local.playerOffsetTransform != (Object)null) ? local.playerOffsetTransform : ghostRig.playerOffsetTransform;
		if (XRSettings.isDeviceActive)
		{
			if (ghostRig.leftHand != null)
				ghostRig.leftHand.MapMine(scale, liveOffset);
			if (ghostRig.rightHand != null)
				ghostRig.rightHand.MapMine(scale, liveOffset);
		}
		float fingerLerp = ghostRig.lerpValueFingers;
		if (ghostRig.rightIndex != null) ghostRig.rightIndex.MapMyFinger(fingerLerp);
		if (ghostRig.rightMiddle != null) ghostRig.rightMiddle.MapMyFinger(fingerLerp);
		if (ghostRig.rightThumb != null) ghostRig.rightThumb.MapMyFinger(fingerLerp);
		if (ghostRig.leftIndex != null) ghostRig.leftIndex.MapMyFinger(fingerLerp);
		if (ghostRig.leftMiddle != null) ghostRig.leftMiddle.MapMyFinger(fingerLerp);
		if (ghostRig.leftThumb != null) ghostRig.leftThumb.MapMyFinger(fingerLerp);
		if ((Object)(object)ghostRigMaterial != (Object)null)
		{
			Color want = local.playerColor;
			if (want.r < 4f / 255f && want.g < 4f / 255f && want.b < 4f / 255f)
				want = Color.white;
			want.a = 0.5f;
			ghostRigMaterial.color = want;
			ApplyGhostMaterial();
		}
	}

	private static Renderer[] ghostSkinRenderers;

	private static bool GhostWanted()
	{
		return tagGunLockedTarget != null || tagAllTarget != null || grabRigActive || ghostMonkeOn || invisMonkeOn || (copyMovementActive && copyMovementTarget != null) || orbitActive;
	}

	private static void ApplyGhostMaterial()
	{
		if ((Object)(object)ghostRig == (Object)null || (Object)(object)ghostRigMaterial == (Object)null) return;
		if (ghostSkinRenderers == null)
			CacheGhostRenderers();
		if (ghostSkinRenderers == null) return;
		foreach (Renderer r in ghostSkinRenderers)
		{
			if ((Object)(object)r == (Object)null) continue;
			r.material = ghostRigMaterial;
		}
	}

	private static void CacheGhostRenderers()
	{
		if ((Object)(object)ghostRig == (Object)null) return;
		List<Renderer> found = new List<Renderer>();
		if ((Object)(object)ghostRig.mainSkin != (Object)null)
			found.Add(ghostRig.mainSkin);
		Renderer[] renderers = ghostRig.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in renderers)
		{
			if ((Object)(object)r == (Object)null) continue;
			if ((Object)(object)r == (Object)ghostRig.mainSkin) continue;
			if (r is SkinnedMeshRenderer) found.Add(r);
		}
		ghostSkinRenderers = found.ToArray();
	}

	private static void EnsureGhostRig()
	{
		if ((Object)(object)ghostRig != (Object)null) return;

		VRRig local = VRRig.LocalRig;
		if ((Object)(object)local == (Object)null) return;

		GameObject ghostRigHolder = new GameObject("Chud_GhostRigHolder");
		ghostRigHolder.SetActive(false);

		cloningGhostRig = true;
		try
		{
			ghostRig = (VRRig)Object.Instantiate(local, local.transform.position, local.transform.rotation, ghostRigHolder.transform);
		}
		finally
		{
			cloningGhostRig = false;
		}
		if ((Object)(object)ghostRig == (Object)null)
		{
			Object.Destroy(ghostRigHolder);
			return;
		}
		ghostRig.isOfflineVRRig = true;
		ghostRig.gameObject.name = "Chud_GhostRig";
		ghostRig.gameObject.SetActive(false);
		Transform localParent = ((Component)local).transform.parent;
		ghostRig.transform.SetParent(localParent);

		Object.Destroy(ghostRigHolder);

		if ((Object)(object)ghostRig.transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio") != (Object)null)
			ghostRig.transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio").gameObject.SetActive(false);
		if ((Object)(object)ghostRig.transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio") != (Object)null)
			ghostRig.transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio").gameObject.SetActive(false);
		if ((Object)(object)ghostRig.transform.Find("rig/body_pivot/SlideAudio") != (Object)null)
			ghostRig.transform.Find("rig/body_pivot/SlideAudio").gameObject.SetActive(false);

		VRRig[] childRigs = ghostRig.GetComponentsInChildren<VRRig>(true);
		foreach (VRRig child in childRigs)
		{
			if (child != ghostRig)
				Object.Destroy(child.gameObject);
		}
		foreach (AutoSyncTransforms sync in ghostRig.GetComponentsInChildren<AutoSyncTransforms>(true))
		{
			sync.enabled = false;
			Object.Destroy(sync);
		}
		foreach (Photon.Pun.PhotonView pv in ghostRig.GetComponentsInChildren<Photon.Pun.PhotonView>(true))
			Object.Destroy(pv);
		foreach (Photon.Pun.PhotonTransformView ptv in ghostRig.GetComponentsInChildren<Photon.Pun.PhotonTransformView>(true))
			Object.Destroy(ptv);
		foreach (Collider col in ghostRig.GetComponentsInChildren<Collider>(true))
			Object.Destroy(col);
		foreach (Rigidbody rb in ghostRig.GetComponentsInChildren<Rigidbody>(true))
			Object.Destroy(rb);
		foreach (GorillaPlayerScoreboardLine line in ghostRig.GetComponentsInChildren<GorillaPlayerScoreboardLine>(true))
			Object.Destroy(line);

		CleanGhostRigGameplay();
		CacheGhostRenderers();

		if ((Object)(object)ghostRigMaterial == (Object)null)
		{
			Shader s = Shader.Find("GorillaTag/UberShader");
			if ((Object)(object)s == (Object)null) s = Shader.Find("Universal Render Pipeline/Unlit");
			if ((Object)(object)s == (Object)null) s = Shader.Find("GUI/Text Shader");
			ghostRigMaterial = new Material(s);
			ghostRigMaterial.hideFlags = HideFlags.HideAndDontSave;
			if (ghostRigMaterial.HasProperty("_Surface"))
				ghostRigMaterial.SetFloat("_Surface", 1f);
			if (ghostRigMaterial.HasProperty("_Blend"))
				ghostRigMaterial.SetFloat("_Blend", 0f);
			if (ghostRigMaterial.HasProperty("_SrcBlend"))
				ghostRigMaterial.SetFloat("_SrcBlend", 5f);
			if (ghostRigMaterial.HasProperty("_DstBlend"))
				ghostRigMaterial.SetFloat("_DstBlend", 10f);
			if (ghostRigMaterial.HasProperty("_ZWrite"))
				ghostRigMaterial.SetFloat("_ZWrite", 0f);
			ghostRigMaterial.renderQueue = 3000;
		}

		HideGhostRig();
	}

	private static void CleanGhostRigGameplay()
	{
		CosmeticsController.CosmeticSet emptySet = new CosmeticsController.CosmeticSet();
		if (CosmeticsController.instance != null)
			emptySet.ClearSet(CosmeticsController.instance.nullItem);
		ghostRig.cosmeticSet = emptySet;
		ghostRig.cosmeticsObjectRegistry = new CosmeticItemRegistry(ghostRig);

		List<GameObject> cosmeticObjects = new List<GameObject>();
		foreach (PlayerColoredCosmetic cosmetic in ghostRig.GetComponentsInChildren<PlayerColoredCosmetic>(true))
			cosmeticObjects.Add(cosmetic.gameObject);
		foreach (HoldableObject holdable in ghostRig.GetComponentsInChildren<HoldableObject>(true))
			cosmeticObjects.Add(holdable.gameObject);
		foreach (GameObject cosmeticObject in cosmeticObjects)
		{
			if ((Object)(object)cosmeticObject != (Object)null)
			{
				cosmeticObject.SetActive(false);
				Object.Destroy(cosmeticObject);
			}
		}
	}

	private static void HideGhostRig()
	{
		if ((Object)(object)ghostRig != (Object)null)
		{
			ghostRig.gameObject.SetActive(false);
			ghostRig.transform.position = Vector3.one * 9999f;
		}
	}

	private static void SubscribeGhostRig()
	{
		if (!ghostRigSubscribed)
		{
			TorsoPatch.VRRigLateUpdate += GhostRigTick;
			ghostRigSubscribed = true;
		}
	}

	private static void UnsubscribeGhostRig()
	{
		if (ghostRigSubscribed)
		{
			TorsoPatch.VRRigLateUpdate -= GhostRigTick;
			ghostRigSubscribed = false;
			HideGhostRig();
		}
	}

	private static void TryUnsubscribeGhostRig()
	{
		if (!GhostWanted())
			UnsubscribeGhostRig();
	}

	private static float tagAuraCooldown;
	public static float tagAuraRange = 1.5f;
	public static int tagAuraRangeIndex = 2;
	public static readonly float[] TagAuraRanges = new float[] { 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f };
	private static LineRenderer tagAuraRing;

	public static void TagAura()
	{
		if (tagAuraRange <= 0f) return;
		if (Time.time < tagAuraCooldown) return;
		if ((Object)(object)VRRig.LocalRig == (Object)null) return;
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
		tagAuraCooldown = 0f;
	}

	private static void CreateAuraRing()
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

	public static void TagAuraVisual()
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

	public static void DisableTagAuraVisual()
	{
		if (tagAuraRing != null) { Object.Destroy(tagAuraRing.gameObject); tagAuraRing = null; }
	}

	public static void SetTagAuraRange(int index)
	{
		tagAuraRangeIndex = index % TagAuraRanges.Length;
		tagAuraRange = TagAuraRanges[tagAuraRangeIndex];
		NotifiLib.SendNotification("Range: " + tagAuraRange.ToString("0.0") + "m");
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
					if (tagMan != null && tagMan.IsInfected(rig.Creator) && Time.time > lastUntagNotif)
					{
						tagMan.currentInfected.RemoveAll(p => p.UserId == rig.Creator.UserId);
						lastUntagNotif = Time.time + 0.3f;
						NotifiLib.SendNotification("Untagged " + rig.Creator.NickName);
					}
				}
			}
		});
	}

	public static void TeleportToSpawn()
	{
		GorillaTagger gt = GorillaTagger.Instance;
		if (gt == null) return;
		GTPlayer player = GTPlayer.Instance;
		if (player == null) return;
		Vector3 stump = stumpPosition;
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
		spazAllActive = true;
	}

	public static void DisableSpazAll()
	{
		spazAllActive = false;
	}

	public static void SpazSelf()
	{
		spazSelfActive = true;
	}

	public static void DisableSpazSelf()
	{
		spazSelfActive = false;
	}

	private static void RunSpaz()
	{
		GorillaGameManager val = GorillaGameManager.instance;
		if ((Object)(object)val == (Object)null)
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

	public static void MakeGun(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action onTrigger, Action onRelease)
	{
		if ((Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform)
		{
			gripHeld = WristMenu.gripDownR;
			triggerHeld = WristMenu.triggerDownR;
		}
		else if ((Object)(object)arm == (Object)(object)GTPlayer.Instance.LeftHand.controllerTransform)
		{
			gripHeld = WristMenu.gripDownL;
			triggerHeld = WristMenu.triggerDownL;
		}
		if (gripHeld)
		{
			Transform visualHand = null;
			try
			{
				bool isRight = (Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform;
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
					bool isRight = (Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform;
					gh = isRight ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform;
				}
				catch { }
				gunOrigin = gh != null ? gh.position : ghostRig.transform.position + Vector3.up * 0.2f;
			}
			else gunOrigin = (visualHand != null ? visualHand.position : arm.position);
			Vector3 gunDir;
			if (isFlatscreen && pcGunsEnabled && Mouse.current != null)
			{
				if ((Object)(object)pcGunCamera == (Object)null)
				{
					GameObject val = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
					if ((Object)(object)val != (Object)null)
					{
						pcGunCamera = val.GetComponent<Camera>();
					}
					if ((Object)(object)pcGunCamera == (Object)null)
					{
						val = GameObject.Find("Shoulder Camera");
						if ((Object)(object)val != (Object)null)
						{
							pcGunCamera = val.GetComponent<Camera>();
						}
					}
				}
				if ((Object)(object)pcGunCamera != (Object)null)
				{
					Ray val2 = pcGunCamera.ScreenPointToRay(((Pointer)Mouse.current).position.ReadValue());
					gunDir = val2.direction;
				}
				else
				{
					if (useGhost)
					{
						Transform gh2 = null;
						try { bool isR = (Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform; gh2 = isR ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
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
					try { bool isR2 = (Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform; gh3 = isR2 ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
					gunDir = gh3 != null ? -gh3.up : (visualHand != null ? -visualHand.up : -arm.up);
				}
				else gunDir = (visualHand != null ? -visualHand.up : -arm.up);
				Physics.Raycast(gunOrigin, gunDir, out raycastHit, 512f, GetNoInvisLayerMask());
			}
			if ((Object)(object)pointer == (Object)null)
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
				if ((Object)(object)Line == (Object)null)
				{
					GameObject val3 = new GameObject("GunLine");
					Line = val3.AddComponent<LineRenderer>();
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
					try { bool isR3 = (Object)(object)arm == (Object)(object)GTPlayer.Instance.RightHand.controllerTransform; gh4 = isR3 ? ghostRig.rightHand?.rigTarget?.transform : ghostRig.leftHand?.rigTarget?.transform; } catch { }
					lineStart = gh4 != null ? gh4.position : ghostRig.transform.position + Vector3.up * 0.2f;
				}
				else lineStart = (visualHand != null ? visualHand.position : arm.position);
				Line.SetPosition(0, lineStart);
				Line.SetPosition(1, pointer.transform.position);
				float pulse = triggerHeld ? (1f + Mathf.Sin(Time.time * 12f) * 0.4f) : 1f;
				Line.startWidth = linesize * pulse;
				Line.endWidth = linesize * pulse;
			}
			Object.Destroy((Object)(object)pointer.GetComponent<BoxCollider>());
			Object.Destroy((Object)(object)pointer.GetComponent<Rigidbody>());
			Object.Destroy((Object)(object)pointer.GetComponent<Collider>());
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
			if ((Object)(object)pointer != (Object)null)
			{
				Object.Destroy((Object)(object)pointer, Time.deltaTime);
				pointer = null;
			}
			if ((Object)(object)Line != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)Line).gameObject);
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

	private static void AntiBlockCrashTick()
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

	public static void BreakGuardian()
	{
		breakGuardianActive = true;
		if (breakGuardianHarmony == null)
		{
			breakGuardianHarmony = new Harmony("chudmenu.breakguardian");
			breakGuardianHarmony.Patch(
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
		if (breakGuardianHarmony != null)
		{
			breakGuardianHarmony.UnpatchSelf();
			breakGuardianHarmony = null;
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

	// ====== Guardian Guns ======
	public static void GuardianGun()
	{
		MakeRightHandGun(delegate
		{
			if (Time.time < lastGuardianGunTime) return;
			VRRig rig = GetGunTargetPlayer();
			if (rig == null || rig.isLocal || rig.Creator == null) return;
			foreach (GorillaGuardianZoneManager zm in GorillaGuardianZoneManager.zoneManagers)
			{
				zm.SetGuardian(rig.Creator);
			}
			lastGuardianGunTime = Time.time + 0.3f;
		});
	}

	public static void UnguardianGun()
	{
		MakeRightHandGun(delegate
		{
			if (Time.time < lastUnguardianGunTime) return;
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
			lastUnguardianGunTime = Time.time + 0.3f;
		});
	}

	public static void GuardianSpazGun()
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

	// ====== Paint Brawl Mods ======
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
		pcButtonClickEnabled = true;
	}

	public static void DisablePCButtonClick()
	{
		pcButtonClickEnabled = false;
		if (pcButtonOldLocalPosition.HasValue)
		{
			GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = pcButtonOldLocalPosition.Value;
			pcButtonOldLocalPosition = null;
		}
		if ((Object)(object)GorillaTagger.Instance.rightHandTriggerCollider != (Object)null)
		{
			TransformFollow component = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>();
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = true;
			}
		}
	}

	private static void UpdatePCButtonClick()
	{
		if (!pcButtonClickEnabled || (Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)GorillaTagger.Instance.rightHandTriggerCollider == (Object)null)
		{
			return;
		}
		if (Mouse.current != null && Mouse.current.leftButton.isPressed)
		{
			if ((Object)(object)pcButtonCachedCamera == (Object)null)
			{
				Camera[] array = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
				foreach (Camera val2 in array)
				{
					if (((Object)val2).name == "Shoulder Camera" || ((Object)(object)((Component)val2).gameObject.transform.parent != (Object)null && ((Object)((Component)val2).gameObject.transform.parent).name == "Third Person Camera"))
					{
						pcButtonCachedCamera = val2;
						break;
					}
				}
			}
			Camera val = pcButtonCachedCamera;
			if (!((Object)(object)val != (Object)null))
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
				if ((Object)(object)component != (Object)null)
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
			if ((Object)(object)component2 != (Object)null)
			{
				((Behaviour)component2).enabled = true;
			}
		}
	}

	public static int GetNoInvisLayerMask()
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
		pcGunsEnabled = true;
	}

	public static void DisablePCGuns()
	{
		pcGunsEnabled = false;
	}

	private static void UpdatePCGuns()
	{
		if (!pcGunsEnabled || Mouse.current == null || XRSettings.isDeviceActive)
		{
			return;
		}
		ControllerInputPoller poller = ControllerInputPoller.instance;
		if ((Object)(object)poller == (Object)null)
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
			result.NormalColor = new Color(0.05f, 0.12f, 0.15f);
			result.ButtonColorEnabled = new Color(0.133f, 0.267f, 0.333f);
			result.ButtonColorDisable = new Color(0.07f, 0.17f, 0.21f);
			result.EnableTextColor = new Color(0.55f, 0.7f, 0.85f);
			result.DisableTextColor = new Color(0.33f, 0.47f, 0.6f);
			result.NextPrevButtonColor = new Color(0.06f, 0.13f, 0.17f);
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
			result.NormalColor = new Color(0.1f, 0.15f, 0.1f);
			result.ButtonColorEnabled = new Color(0.4f, 0.6f, 0.4f);
			result.ButtonColorDisable = new Color(0.22f, 0.33f, 0.22f);
			result.EnableTextColor = new Color(0.6f, 0.9f, 0.6f);
			result.DisableTextColor = new Color(0.35f, 0.55f, 0.35f);
			result.NextPrevButtonColor = new Color(0.12f, 0.18f, 0.12f);
			break;
		case 5:
			result.NormalColor = new Color(0.04f, 0.14f, 0.18f);
			result.ButtonColorEnabled = new Color(0.15f, 0.75f, 0.9f);
			result.ButtonColorDisable = new Color(0.08f, 0.38f, 0.5f);
			result.EnableTextColor = new Color(0.5f, 0.9f, 1f);
			result.DisableTextColor = new Color(0.3f, 0.65f, 0.75f);
			result.NextPrevButtonColor = new Color(0.06f, 0.22f, 0.3f);
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
			result.NormalColor = new Color(0.18f, 0.04f, 0.16f);
			result.ButtonColorEnabled = new Color(0.85f, 0.25f, 0.7f);
			result.ButtonColorDisable = new Color(0.45f, 0.1f, 0.35f);
			result.EnableTextColor = new Color(1f, 0.5f, 0.85f);
			result.DisableTextColor = new Color(0.7f, 0.3f, 0.55f);
			result.NextPrevButtonColor = new Color(0.28f, 0.06f, 0.22f);
			break;
		case 8:
			result.NormalColor = new Color(0.18f, 0.06f, 0.1f);
			result.ButtonColorEnabled = new Color(0.85f, 0.25f, 0.5f);
			result.ButtonColorDisable = new Color(0.5f, 0.14f, 0.28f);
			result.EnableTextColor = new Color(1f, 0.6f, 0.8f);
			result.DisableTextColor = new Color(0.75f, 0.35f, 0.5f);
			result.NextPrevButtonColor = new Color(0.28f, 0.08f, 0.16f);
			break;
		case 9:
			result.NormalColor = new Color(0.15f, 0.1f, 0.04f);
			result.ButtonColorEnabled = new Color(0.7f, 0.45f, 0.2f);
			result.ButtonColorDisable = new Color(0.35f, 0.22f, 0.1f);
			result.EnableTextColor = new Color(0.9f, 0.75f, 0.5f);
			result.DisableTextColor = new Color(0.65f, 0.55f, 0.35f);
			result.NextPrevButtonColor = new Color(0.22f, 0.14f, 0.06f);
			break;
		default:
			result = GetMenuColors(0);
			break;
		}
		result.MenuTitleColor = result.EnableTextColor;
		return result;
	}

	public static void UnlockAllCosmetics()
	{
		CosmeticsController val = CosmeticsController.instance;
		if ((Object)(object)val == (Object)null || !val.v2_allCosmeticsInfoAssetRef_isLoaded)
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

	// ====== Try On All / Remove All Cosmetics (Mirror) ======
	private static bool tryOnAllActive;
	private static Coroutine tryOnAllCoroutine;
	private static CosmeticsController.CosmeticItem[] tryOnAllSavedWorn;
	private static readonly string treePinCosmeticId = "LBAAA.";
	private static readonly string tryOnAllButtonText = "SS tryon all cosmetics (Mirror)";
	private static readonly string removeAllButtonText = "Remove all cosmetics (Mirror)";
	private static bool removeAllActive;
	private static Coroutine removeAllCoroutine;

	private static MethodInfo cachedAddCosmeticMethod;
	private static ParameterInfo[] cachedAddCosmeticParameters;

	public static void EnableTryOnAll()
	{
		if (tryOnAllActive) return;
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		tryOnAllActive = true;
		tryOnAllSavedWorn = new CosmeticsController.CosmeticItem[16];
		for (int i = 0; i < 16; i++)
		{
			tryOnAllSavedWorn[i] = controller.currentWornSet.items[i];
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
		tryOnAllCoroutine = instance.StartCoroutine(TryOnAllCycleRoutine());
	}

	public static void DisableTryOnAll()
	{
		if (!tryOnAllActive) return;
		tryOnAllActive = false;
		if (tryOnAllCoroutine != null)
		{
			instance.StopCoroutine(tryOnAllCoroutine);
			tryOnAllCoroutine = null;
		}
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null)
		{
			return;
		}
		if (tryOnAllSavedWorn != null)
		{
			for (int i = 0; i < 16; i++)
			{
				if (controller.currentWornSet.items[i].itemName == treePinCosmeticId)
				{
					controller.currentWornSet.items[i] = tryOnAllSavedWorn[i];
				}
			}
			tryOnAllSavedWorn = null;
			controller.UpdateWornCosmetics(true);
		}
		controller.tryOnSet.ClearSet(controller.nullItem);
		controller.UpdateWornCosmetics(true);
	}

	private static IEnumerator TryOnAllCycleRoutine()
	{
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			yield break;
		}
		List<CosmeticsController.CosmeticItem> items = BuildTryOnFilteredList(controller);
		if (items.Count == 0)
		{
			FindAndToggleButton(tryOnAllButtonText);
			yield break;
		}
		for (int i = 0; i < items.Count; i++)
		{
			VRRig localRig = VRRig.LocalRig;
			if ((Object)(object)localRig == (Object)null)
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
		FindAndToggleButton(tryOnAllButtonText);
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
		if ((Object)(object)rig == (Object)null || string.IsNullOrEmpty(itemName)) return;
		if (cachedAddCosmeticMethod == null)
		{
			cachedAddCosmeticMethod = AccessTools.Method(rig.GetType(), "AddCosmetic");
			if (cachedAddCosmeticMethod == null) return;
			cachedAddCosmeticParameters = cachedAddCosmeticMethod.GetParameters();
		}
		object[] args = new object[cachedAddCosmeticParameters.Length];
		args[0] = itemName;
		for (int i = 1; i < args.Length; i++)
		{
			args[i] = Type.Missing;
		}
		cachedAddCosmeticMethod.Invoke(rig, args);
	}

	// ====== Remove All Cosmetics (Mirror) ======
	public static void EnableRemoveAllCosmetics()
	{
		if (removeAllActive) return;
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			return;
		}
		removeAllActive = true;
		removeAllCoroutine = instance.StartCoroutine(RemoveAllCycleRoutine());
	}

	public static void DisableRemoveAllCosmetics()
	{
		if (!removeAllActive) return;
		removeAllActive = false;
		if (removeAllCoroutine != null)
		{
			instance.StopCoroutine(removeAllCoroutine);
			removeAllCoroutine = null;
		}
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null)
		{
			return;
		}
		controller.tryOnSet.ClearSet(controller.nullItem);
		controller.UpdateWornCosmetics(true);
	}

	private static IEnumerator RemoveAllCycleRoutine()
	{
		CosmeticsController controller = CosmeticsController.instance;
		if ((Object)(object)controller == (Object)null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
		{
			yield break;
		}
		List<CosmeticsController.CosmeticItem> items = BuildTryOnFilteredList(controller);
		if (items.Count == 0)
		{
			FindAndToggleButton(removeAllButtonText);
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
		FindAndToggleButton(removeAllButtonText);
	}

	public static void FindAndToggleButton(string buttonText)
	{
		foreach (MenuCategory category in MenuManager.Categories)
		{
			ButtonInfo buttonInfo = category.Buttons.Find((ButtonInfo b) => b.buttonText == buttonText && b.enabled.HasValue && b.type != ButtonType.Action);
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
				WristMenu.UpdateButtonVisual(buttonInfo.buttonText, buttonInfo.enabled.Value);
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
				buttonText = "Exit Soundboard",
				method = delegate
				{
					MenuManager.ToggleCategory("Soundboard");
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
					buttonText = fileName,
					enableMethod = delegate
					{
						SoundboardStop();
						SoundboardPlay(text2);
					},
					disableMethod = SoundboardStop,
					enabled = false,
					requiresLobby = true,
					toolTip = fileName
				});
			}
		}
		EnsureSoundboardPreload();
		return buttons;
	}

	private static AudioClip _soundboardClip;

	private static readonly Dictionary<string, AudioClip> soundboardCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

	private static bool soundboardPreloadStarted = false;

	private static void EnsureSoundboardPreload()
	{
		if (soundboardPreloadStarted) return;
		if ((Object)(object)instance == (Object)null) return;
		soundboardPreloadStarted = true;
		instance.StartCoroutine(PreloadSoundboard());
	}

	private static IEnumerator PreloadSoundboard()
	{
		if (!Directory.Exists(soundboardBasePath)) yield break;
		string[] files = null;
		try { files = Directory.GetFiles(soundboardBasePath); } catch { yield break; }
		if (files == null) yield break;
		foreach (string path in files)
		{
			if (soundboardCache.ContainsKey(path)) continue;
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
					if ((Object)(object)clip != (Object)null)
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
		if (soundboardCache.TryGetValue(path, out AudioClip cached) && (Object)(object)cached != (Object)null)
		{
			Recorder instantRecorder = ((Object)(object)GorillaTagger.Instance != (Object)null) ? GorillaTagger.Instance.myRecorder : null;
			if (instantRecorder != null)
			{
				instantRecorder.SourceType = Recorder.InputSourceType.AudioClip;
				instantRecorder.AudioClip = cached;
				instantRecorder.RestartRecording(true);
				instantRecorder.DebugEchoMode = true;
			}
			return;
		}
		instance.StartCoroutine(SoundboardLoadAndPlay(path));
	}

	private static IEnumerator SoundboardLoadAndPlay(string path)
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
					if ((Object)(object)_soundboardClip != (Object)null)
						Object.Destroy((Object)(object)_soundboardClip);
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
		if ((Object)(object)_soundboardClip != (Object)null)
		{
			Object.Destroy((Object)(object)_soundboardClip);
			_soundboardClip = null;
		}
	}

	public static void EnableBackflip()
	{
		backflipEnabled = true;
		TorsoPatch.VRRigLateUpdate -= FlipTick;
		TorsoPatch.VRRigLateUpdate += FlipTick;
	}

	public static void DisableBackflip()
	{
		backflipEnabled = false;
		backflipActive = false;
		if (!frontflipEnabled)
			TorsoPatch.VRRigLateUpdate -= FlipTick;
	}

	public static void EnableFrontflip()
	{
		frontflipEnabled = true;
		TorsoPatch.VRRigLateUpdate -= FlipTick;
		TorsoPatch.VRRigLateUpdate += FlipTick;
	}

	public static void DisableFrontflip()
	{
		frontflipEnabled = false;
		frontflipActive = false;
		if (!backflipEnabled)
			TorsoPatch.VRRigLateUpdate -= FlipTick;
	}

	private static void FlipTick()
	{
		bool btn = isRightHanded ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerInputPoller.instance.rightControllerSecondaryButton;
		if (backflipEnabled && btn && !lastFlipButton && !frontflipActive)
		{
			backflipActive = true;
			backflipRotation = 0f;
			backflipStartRot = VRRig.LocalRig.transform.rotation;
		}
		if (frontflipEnabled && btn && !lastFlipButton && !backflipActive)
		{
			frontflipActive = true;
			frontflipRotation = 0f;
			frontflipStartRot = VRRig.LocalRig.transform.rotation;
		}
		lastFlipButton = btn;
		if (backflipActive)
		{
			float step = Time.deltaTime * 540f;
			backflipRotation += step;
			if (backflipRotation < 360f)
				VRRig.LocalRig.transform.rotation = backflipStartRot * Quaternion.Euler(-backflipRotation, 0f, 0f);
			else
				backflipActive = false;
		}
		if (frontflipActive)
		{
			float step = Time.deltaTime * 540f;
			frontflipRotation += step;
			if (frontflipRotation < 360f)
				VRRig.LocalRig.transform.rotation = frontflipStartRot * Quaternion.Euler(frontflipRotation, 0f, 0f);
			else
				frontflipActive = false;
		}
	}
	private static bool backflipActive;
	private static float backflipRotation;
	private static Quaternion backflipStartRot;
	private static bool backflipEnabled;
	private static bool frontflipActive;
	private static float frontflipRotation;
	private static Quaternion frontflipStartRot;
	private static bool frontflipEnabled;
	private static bool lastFlipButton;
	private static bool spinningTorsoEnabled;
	public static void EnableSpinningTorso()
	{
		spinningTorsoEnabled = true;
		TorsoPatch.VRRigLateUpdate -= SpinningTorsoTick;
		TorsoPatch.VRRigLateUpdate += SpinningTorsoTick;
	}
	public static void DisableSpinningTorso()
	{
		spinningTorsoEnabled = false;
		TorsoPatch.VRRigLateUpdate -= SpinningTorsoTick;
	}
	private static void SpinningTorsoTick()
	{
		if (!spinningTorsoEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null) return;
		Quaternion tilt = Quaternion.Euler(-90f, 0f, 0f);
		Quaternion spin = Quaternion.AngleAxis(Time.time * 360f % 360f, Vector3.up);
		rig.transform.rotation = spin * tilt;
		rig.head.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.leftHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.rightHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
	}
	private static bool fakeFBTEnabled;
	public static void EnableFakeFBT()
	{
		fakeFBTEnabled = true;
		TorsoPatch.VRRigLateUpdate -= FakeFBTTick;
		TorsoPatch.VRRigLateUpdate += FakeFBTTick;
	}
	public static void DisableFakeFBT()
	{
		fakeFBTEnabled = false;
		TorsoPatch.VRRigLateUpdate -= FakeFBTTick;
	}
	private static void FakeFBTTick()
	{
		if (!fakeFBTEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null) return;
		rig.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
		rig.head.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.leftHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.rightHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
	}
	private static bool dinnerboneEnabled;
	public static void EnableDinnerbone()
	{
		dinnerboneEnabled = true;
		TorsoPatch.VRRigLateUpdate -= DinnerboneTick;
		TorsoPatch.VRRigLateUpdate += DinnerboneTick;
	}
	public static void DisableDinnerbone()
	{
		dinnerboneEnabled = false;
		TorsoPatch.VRRigLateUpdate -= DinnerboneTick;
	}
	private static void DinnerboneTick()
	{
		if (!dinnerboneEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null) return;
		rig.transform.rotation = rig.transform.rotation * Quaternion.Euler(0f, 0f, 180f);
		rig.transform.position += Vector3.down * 0.3f;
	}
	private static bool natsukiNeckEnabled;
	private static Vector3 natsukiSavedPos;
	private static Quaternion natsukiSavedRot;
	private static bool natsukiHasSaved;
	public static void EnableNatsukiNeck()
	{
		natsukiNeckEnabled = true;
		natsukiHasSaved = false;
		VRRig rig = VRRig.LocalRig;
		if (rig != null && rig.head != null && (Object)(object)rig.head.rigTarget != (Object)null)
		{
			natsukiSavedPos = rig.head.rigTarget.transform.position;
			natsukiSavedRot = rig.head.rigTarget.transform.rotation;
			natsukiHasSaved = true;
		}
		TorsoPatch.VRRigLateUpdate -= NatsukiNeckTick;
		TorsoPatch.VRRigLateUpdate += NatsukiNeckTick;
	}
	public static void DisableNatsukiNeck()
	{
		natsukiNeckEnabled = false;
		TorsoPatch.VRRigLateUpdate -= NatsukiNeckTick;
		if (natsukiHasSaved)
		{
			natsukiHasSaved = false;
			VRRig rig = VRRig.LocalRig;
			if (rig != null && rig.head != null && (Object)(object)rig.head.rigTarget != (Object)null)
				rig.head.rigTarget.transform.SetPositionAndRotation(natsukiSavedPos, natsukiSavedRot);
		}
	}
	private static void NatsukiNeckTick()
	{
		if (!natsukiNeckEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null || rig.head == null || (Object)(object)rig.head.rigTarget == (Object)null) return;
		if ((Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)GorillaTagger.Instance.headCollider == (Object)null) return;
		Quaternion live = GorillaTagger.Instance.headCollider.transform.rotation;
		float scale = rig.scaleFactor;
		if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
			scale = 1f;
		Vector3 offset = (rig.head != null) ? rig.head.trackingPositionOffset : Vector3.zero;
		rig.head.rigTarget.transform.SetPositionAndRotation(
			GorillaTagger.Instance.headCollider.transform.position + live * offset * scale,
			live * Quaternion.Euler(0f, 0f, -90f));
		rig.transform.rotation = GorillaLocomotion.GTPlayerTransform.BodyRotation;
	}
	private static bool spiderMonkeyEnabled;
	private static Quaternion spiderMonkeyRot;
	private static Quaternion spiderMonkeyTargetRot;
	private static readonly FieldInfo _lastHitInfoHand = AccessTools.Field(typeof(GTPlayer), "lastHitInfoHand");
	public static void EnableSpiderMonkey()
	{
		spiderMonkeyEnabled = true;
		spiderMonkeyRot = Quaternion.identity;
		spiderMonkeyTargetRot = Quaternion.identity;
		TorsoPatch.VRRigLateUpdate -= SpiderMonkeyTick;
		TorsoPatch.VRRigLateUpdate += SpiderMonkeyTick;
	}
	public static void DisableSpiderMonkey()
	{
		spiderMonkeyEnabled = false;
		TorsoPatch.VRRigLateUpdate -= SpiderMonkeyTick;
		GTPlayer.Instance.UnsetGravityOverride(GTPlayer.Instance);
		GTPlayerTransform.ApplyRotationOverride(Quaternion.identity, Time.frameCount);
	}
	private static void SpiderMonkeyTick()
	{
		if (!spiderMonkeyEnabled) return;
		if (GTPlayer.Instance.IsHandTouching(true) || GTPlayer.Instance.IsHandTouching(false))
		{
			RaycastHit ray = (RaycastHit)_lastHitInfoHand.GetValue(GTPlayer.Instance);
			Vector3 up = ray.normal.normalized;
			Vector3 forward = Vector3.Cross(Vector3.right, up);
			spiderMonkeyTargetRot = Quaternion.LookRotation(forward, up);
		}
		float t = 1f - Mathf.Exp(-5f * Time.deltaTime);
		spiderMonkeyRot = Quaternion.Slerp(spiderMonkeyRot, spiderMonkeyTargetRot, t);
		GTPlayerTransform.ApplyRotationOverride(spiderMonkeyRot, Time.frameCount);
		GTPlayer.Instance.SetGravityOverride(GTPlayer.Instance, p => p.AddForce(spiderMonkeyRot * Physics.gravity, ForceMode.Acceleration));
	}
	private static bool lagGunRunning;
	private static int lagGunTargetActor = -1;
	private static VRRig lagGunLockedTarget;

	private static bool copyMovementActive;
	private static VRRig copyMovementTarget;

	private static VRRig orbitTarget = null;
	private static bool orbitActive = false;
	private static float orbitAngle = 0f;

	private static readonly byte[] lagPayload = new byte[128];

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
					lagGunLockedTarget = rig;
					lagGunTargetActor = player.ActorNumber;
					if (!lagGunRunning)
					{
						lagGunRunning = true;
						instance.StartCoroutine(LagGunLoop());
					}
				}
			}
		}, delegate
		{
			StopLagGun();
		});
		if (lagGunLockedTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)lagGunLockedTarget).transform.position;
			Line.SetPosition(1, ((Component)lagGunLockedTarget).transform.position);
		}
	}

	public static void StopLagGun()
	{
		lagGunRunning = false;
		lagGunTargetActor = -1;
		lagGunLockedTarget = null;
	}

	public static void StopLagGunFull()
	{
		StopLagGun();
		CleanupGun();
	}

	private static IEnumerator LagGunLoop()
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
				copyMovementTarget = rig;
				copyMovementActive = true;
				TorsoPatch.VRRigLateUpdate -= CopyMovementTick;
				TorsoPatch.VRRigLateUpdate += CopyMovementTick;
			}
		}, delegate
		{
			StopCopyMovementGun();
		});
		if (copyMovementTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)copyMovementTarget).transform.position;
			Line.SetPosition(1, ((Component)copyMovementTarget).transform.position);
		}
	}

	public static void StopCopyMovementGun()
	{
		TorsoPatch.VRRigLateUpdate -= CopyMovementTick;
		if (copyMovementActive && (Object)(object)VRRig.LocalRig != (Object)null)
		{
			EnsureLocalRigEnabled();
		}
		copyMovementActive = false;
		copyMovementTarget = null;
	}

	public static void StopCopyMovementGunFull()
	{
		StopCopyMovementGun();
		CleanupGun();
	}

	private static void CopyMovementTick()
	{
		if (!copyMovementActive || (Object)(object)copyMovementTarget == (Object)null || (Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		if (!(isRightHanded ? WristMenu.gripDownL : WristMenu.gripDownR))
		{
			StopCopyMovementGun();
			return;
		}
		VRRig target = copyMovementTarget;
		VRRig local = VRRig.LocalRig;
		EnsureLocalRigEnabled();
		local.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
		if (target.head != null && (Object)(object)target.head.rigTarget != (Object)null && local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
		{
			local.head.rigTarget.transform.SetPositionAndRotation(target.head.rigTarget.transform.position, target.head.rigTarget.transform.rotation);
		}
		if (target.leftHand != null && (Object)(object)target.leftHand.rigTarget != (Object)null && local.leftHand != null && (Object)(object)local.leftHand.rigTarget != (Object)null)
		{
			local.leftHand.rigTarget.transform.SetPositionAndRotation(target.leftHand.rigTarget.transform.position, target.leftHand.rigTarget.transform.rotation);
		}
		if (target.rightHand != null && (Object)(object)target.rightHand.rigTarget != (Object)null && local.rightHand != null && (Object)(object)local.rightHand.rigTarget != (Object)null)
		{
			local.rightHand.rigTarget.transform.SetPositionAndRotation(target.rightHand.rigTarget.transform.position, target.rightHand.rigTarget.transform.rotation);
		}
	}

	public static void OrbitGun()
	{
		MakeRightHandGun(delegate
		{
			VRRig rig = GetGunTargetPlayer();
			if (rig != null && !rig.isLocal)
			{
				orbitTarget = rig;
				orbitActive = true;
				orbitAngle = 0f;
				SubscribeGhostRig();
				TorsoPatch.VRRigLateUpdate -= OrbitTick;
				TorsoPatch.VRRigLateUpdate += OrbitTick;
			}
		}, delegate { StopOrbit(); });
		if (orbitTarget != null && pointer != null && Line != null)
		{
			pointer.transform.position = ((Component)orbitTarget).transform.position;
			Line.SetPosition(1, ((Component)orbitTarget).transform.position);
		}
	}

	public static void StopOrbit()
	{
		TorsoPatch.VRRigLateUpdate -= OrbitTick;
		if (orbitActive && (Object)(object)VRRig.LocalRig != (Object)null) EnsureLocalRigEnabled();
		orbitActive = false;
		orbitTarget = null;
		TryUnsubscribeGhostRig();
	}

	public static void StopOrbitFull()
	{
		StopOrbit();
		CleanupGun();
	}

	private static void OrbitTick()
	{
		if (!orbitActive || (Object)(object)orbitTarget == (Object)null || (Object)(object)VRRig.LocalRig == (Object)null) return;
		if (!(isRightHanded ? WristMenu.gripDownL : WristMenu.gripDownR)) { StopOrbit(); return; }
		VRRig local = VRRig.LocalRig;
		EnsureLocalRigEnabled();
		orbitAngle += Time.deltaTime * 165f;
		if (orbitAngle > 360f) orbitAngle -= 360f;
		float rad = orbitAngle * Mathf.Deg2Rad;
		Vector3 center = ((Component)orbitTarget).transform.position;
		float radius = 1.5f;
		float height = 0.9f;
		Vector3 offset = new Vector3(Mathf.Cos(rad) * radius, height, Mathf.Sin(rad) * radius);
		Vector3 pos = center + offset;
		Quaternion look = Quaternion.LookRotation(center - pos, Vector3.up);
		local.transform.SetPositionAndRotation(pos, look);
		Vector3 headPos = pos + Vector3.up * 0.25f;
		if (local.head != null && (Object)(object)local.head.rigTarget != (Object)null)
			local.head.rigTarget.transform.SetPositionAndRotation(headPos, look);
		Vector3 right = look * Vector3.right;
		Vector3 leftPos = pos + right * -1.1f + Vector3.up * 0.15f;
		Vector3 rightPos = pos + right * 1.1f + Vector3.up * 0.15f;
		Quaternion leftRot = look * Quaternion.Euler(0, 0, 90);
		Quaternion rightRot = look * Quaternion.Euler(0, 0, -90);
		if (local.leftHand != null && (Object)(object)local.leftHand.rigTarget != (Object)null)
			local.leftHand.rigTarget.transform.SetPositionAndRotation(leftPos, leftRot);
		if (local.rightHand != null && (Object)(object)local.rightHand.rigTarget != (Object)null)
			local.rightHand.rigTarget.transform.SetPositionAndRotation(rightPos, rightRot);
		if (local.leftIndex != null) { local.leftIndex.calcT = 0f; local.leftIndex.LerpFinger(1f, false); }
		if (local.leftMiddle != null) { local.leftMiddle.calcT = 0f; local.leftMiddle.LerpFinger(1f, false); }
		if (local.leftThumb != null) { local.leftThumb.calcT = 0f; local.leftThumb.LerpFinger(1f, false); }
		if (local.rightIndex != null) { local.rightIndex.calcT = 0f; local.rightIndex.LerpFinger(1f, false); }
		if (local.rightMiddle != null) { local.rightMiddle.calcT = 0f; local.rightMiddle.LerpFinger(1f, false); }
		if (local.rightThumb != null) { local.rightThumb.calcT = 0f; local.rightThumb.LerpFinger(1f, false); }
	}

	public static void CleanupGun()
	{
		if ((Object)(object)pointer != (Object)null)
		{
			Object.Destroy((Object)(object)pointer, Time.deltaTime);
			pointer = null;
		}
		if ((Object)(object)Line != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)Line).gameObject);
			Line = null;
		}
		gunTriggerWasDown = false;
	}

}


[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public class TorsoPatch
{
	public static event Action VRRigLateUpdate;

	public static void Postfix(VRRig __instance)
	{
		if (__instance.isLocal)
		{
			VRRigLateUpdate?.Invoke();

			if ((Object)(object)__instance.playerText1 != (Object)null)
				__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
	}
}
