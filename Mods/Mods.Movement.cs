using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Chud.UI;
using GorillaLocomotion;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Voice.Unity;
using POpusCodec.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Pointer = UnityEngine.InputSystem.Pointer;
using Random = UnityEngine.Random;

namespace Chud.Backend;

internal partial class Mods
{
	private bool joystickFlyActive = false;

	private bool noGravityActive = false;

	private Vector3 _flyDesiredVelocity = Vector3.zero;

	private bool wasdFlyActive = false;

	private bool wasdFlyNoMouseLock = false;

	private float wasdPitch;

	private bool flyActive = false;

	private float pullPower = 0.05f;

	private readonly Dictionary<bool, bool> previousTouchingGround = new Dictionary<bool, bool>();

	private int noclipCacheFrame = 0;

	private MeshCollider[] noclipCache = (MeshCollider[])(object)new MeshCollider[0];

	private BoxCollider[] noclipBoxCache = (BoxCollider[])(object)new BoxCollider[0];

	private readonly Dictionary<Collider, bool> noclipOriginalStates = new Dictionary<Collider, bool>();

	private Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);

	private Material platMaterial;

	private bool once_left;

	private bool once_right;

	private bool once_left_false;

	private bool once_right_false;

	private GameObject jump_left_local = null;

	private GameObject jump_right_local = null;

	private bool stickyRightActive = false;

	private bool stickyLeftActive = false;

	private bool grabGreenBugActive = false;

	private bool grabDougBugActive = false;

	private bool grabAllBugsActive = false;

	private bool grabSpazBugActive = false;

	private float grabBugLastScan;

	private readonly List<ThrowableBug> cachedGrabBugs = new List<ThrowableBug>();

	private const float GRAB_BUG_SCAN_INTERVAL = 3f;

	private bool minosPrimedForSlam = false;

	private bool minosWaitingForImpact = false;

	private AudioClip minosCrushClip = null;

	private AudioClip minosSlamClip = null;

	private bool minosClipsLoaded = false;

	private bool minosSecondaryWasDown = false;

	private bool minosPrimaryWasDown = false;

	private Coroutine minosRestoreCoroutine = null;

	private AudioSource minosLocalSource = null;

	private const string MinosCrushUrl = "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/CRUSH%20!.mp3";

	private const string MinosSlamUrl = "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/slam%20sound.mp3";

	private bool spiderMonkeyEnabled;

	private Quaternion spiderMonkeyRot;

	private Quaternion spiderMonkeyTargetRot;

	private static readonly FieldInfo _lastHitInfoHand = AccessTools.Field(typeof(GTPlayer), "lastHitInfoHand");

	private Vector3 _predPrevLeftHand = Vector3.zero;

	private Vector3 _predPrevRightHand = Vector3.zero;

	private Vector3 _predPrevHead = Vector3.zero;

	private Vector3 _predLeftVel = Vector3.zero;

	private Vector3 _predRightVel = Vector3.zero;

	private Vector3 _predHeadVel = Vector3.zero;

	private bool controllerPredActive = false;

	public static void JoystickFly()
	{
		if (instance == null)
		{
			return;
		}
		instance.joystickFlyActive = true;
		instance.RegisterFlyGravityOverride();
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
		if (instance == null)
		{
			return;
		}
		instance.joystickFlyActive = false;
		instance.UnregisterFlyGravityOverride();
	}

	public static void EnableWASDFly()
	{
		if (instance == null)
		{
			return;
		}
		instance.wasdFlyActive = true;
		instance.wasdPitch = 0f;
		instance.RegisterFlyGravityOverride();
	}

	public static void DisableWASDFly()
	{
		if (instance == null)
		{
			return;
		}
		instance.wasdFlyActive = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		instance.UnregisterFlyGravityOverride();
	}

	public static void EnableFly()
	{
		if (instance != null)
		{
			instance.flyActive = true;
		}
	}

	public static void DisableFly()
	{
		if (instance != null)
		{
			instance.flyActive = false;
		}
	}

	private void UpdateFly()
	{
		if (flyActive)
		{
			if (ControllerInputPoller.instance != (Object)null && GorillaTagger.Instance != (Object)null && !(GorillaTagger.Instance.rigidbody == (Object)null) && (isRightHanded ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerInputPoller.instance.rightControllerSecondaryButton))
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
		if (instance != null)
		{
			instance.wasdFlyNoMouseLock = on;
		}
	}

	public static void SetWASDFlyMouseSense(int index)
	{
		index %= WasdSenseValues.Length;
		if (index < 0) index = WasdSenseValues.Length - 1;
		wasdFlyMouseSense = WasdSenseValues[index];
		NotifiLib.SendNotification("WASD Mouse Sense: " + wasdFlyMouseSense.ToString("0.00"));
	}

	private void UpdateWASDFly()
	{
		if (!wasdFlyActive)
		{
			return;
		}
		if (GorillaTagger.Instance == (Object)null)
		{
			return;
		}
		Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
		if (rigidbody == (Object)null)
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

	private static readonly Action<GTPlayer> _flyGravityCallback = delegate (GTPlayer gt)
	{
		Rigidbody rb = gt.playerRigidBody;
		rb.linearVelocity = Vector3.zero;
	};

	private void RegisterFlyGravityOverride()
	{
		if (GTPlayer.Instance != (Object)null)
		{
			GTPlayer.Instance.SetGravityOverride(instance, _flyGravityCallback);
		}
	}

	private void UnregisterFlyGravityOverride()
	{
		if (GTPlayer.Instance != (Object)null && !joystickFlyActive && !wasdFlyActive && !noGravityActive)
		{
			GTPlayer.Instance.UnsetGravityOverride(instance);
		}
	}

	public static void NoGravity()
	{
		if (instance == null)
		{
			return;
		}
		instance.noGravityActive = true;
		instance.RegisterFlyGravityOverride();
	}

	public static void DisableNoGravity()
	{
		if (instance == null)
		{
			return;
		}
		instance.noGravityActive = false;
		instance.UnregisterFlyGravityOverride();
	}

	public static void Platforms()
	{
		if (instance == null)
		{
			return;
		}
		instance.PlatformsThing(invis: false, false);
	}

	public static void StickyPlatforms()
	{
		if (instance == null)
		{
			return;
		}
		instance.PlatformsThing(invis: false, true);
	}

	private void PlatformsThing(bool invis, bool sticky)
	{
		RPlat = WristMenu.gripDownR;
		LPlat = WristMenu.gripDownL;
		if (platMaterial != null) platMaterial.color = WristMenu.ButtonColorEnabled;
		ProcessPlatform(RPlat, ref jump_right_local, ref once_right, ref once_right_false, ref stickyRightActive, true, sticky);
		ProcessPlatform(LPlat, ref jump_left_local, ref once_left, ref once_left_false, ref stickyLeftActive, false, sticky);
	}

	private void ProcessPlatform(bool plat, ref GameObject jumpObj, ref bool once, ref bool onceFalse, ref bool stickyActive, bool isRight, bool sticky)
	{
		if (plat)
		{
			if (!once && jumpObj == (Object)null)
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
						Object.Destroy(box.GetComponent<Renderer>());
						Object.Destroy(box.GetComponent<Rigidbody>());
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
		else if (!onceFalse && jumpObj != (Object)null)
		{
			Object.Destroy(jumpObj);
			jumpObj = null;
			stickyActive = false;
			once = false;
			onceFalse = true;
		}
	}

	private void ClampHandToCage(Vector3 center, bool isRight)
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

	public static void GrabGreenBug()
	{
		if (instance != null)
		{
			instance.grabGreenBugActive = !instance.grabGreenBugActive;
		}
	}

	public static void DisableGrabGreenBug()
	{
		if (instance != null)
		{
			instance.grabGreenBugActive = false;
		}
	}

	public static void GrabDougBug()
	{
		if (instance != null)
		{
			instance.grabDougBugActive = !instance.grabDougBugActive;
		}
	}

	public static void DisableGrabDougBug()
	{
		if (instance != null)
		{
			instance.grabDougBugActive = false;
		}
	}

	public static void GrabAllBugs()
	{
		if (instance != null)
		{
			instance.grabAllBugsActive = !instance.grabAllBugsActive;
		}
	}

	public static void DisableGrabAllBugs()
	{
		if (instance != null)
		{
			instance.grabAllBugsActive = false;
		}
	}

	public static void SpazBugs()
	{
		if (instance != null)
		{
			instance.grabSpazBugActive = !instance.grabSpazBugActive;
		}
	}

	public static void DisableSpazBugs()
	{
		if (instance != null)
		{
			instance.grabSpazBugActive = false;
		}
	}

	private void UpdateGrabBugs()
	{
		if (!grabGreenBugActive && !grabDougBugActive && !grabAllBugsActive && !grabSpazBugActive)
			return;

		bool rightGrip = ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.rightGrab;
		bool leftGrip = ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.leftGrab;
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
			if (bug == (Object)null)
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
				if (model == (Object)null) continue;
				SkinnedMeshRenderer renderer = model.GetComponent<SkinnedMeshRenderer>();
				if (renderer == (Object)null || renderer.material == (Object)null) continue;
				string matName = renderer.material.name;
				bool isGreen = matName.Contains("PlumpBeetle2");
				bool isDoug = !isGreen && matName.Contains("PlumpBeetle");
				bool shouldGrab = grabAllBugsActive || (grabGreenBugActive && isGreen) || (grabDougBugActive && isDoug);

				if (!shouldGrab)
					continue;

				if (!bug.IsMyItem())
					bug.WorldShareableRequestOwnership();

				Rigidbody rb = bug.GetComponent<Rigidbody>();
				if (rb != (Object)null)
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
		if (instance == null)
		{
			return;
		}
		instance.NoclipCore();
	}

	private void NoclipCore()
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
			if (val == (Object)null)
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
			if (val2 == (Object)null || val2.isTrigger)
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
		if (instance == null)
		{
			return;
		}
		instance.noclipCache = Resources.FindObjectsOfTypeAll<MeshCollider>();
		instance.noclipBoxCache = Resources.FindObjectsOfTypeAll<BoxCollider>();
		foreach (var kvp in instance.noclipOriginalStates)
		{
			if (kvp.Key != (Object)null)
			{
				kvp.Key.enabled = kvp.Value;
			}
		}
		instance.noclipOriginalStates.Clear();
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
		if (instance == null)
		{
			return;
		}
		instance.controllerPredActive = true;
		instance._predPrevLeftHand = GorillaTagger.Instance.leftHandTransform.position;
		instance._predPrevRightHand = GorillaTagger.Instance.rightHandTransform.position;
		instance._predPrevHead = VRRig.LocalRig.head.rigTarget.transform.position;
	}

	public static void DisableControllerPredictions()
	{
		if (instance != null)
		{
			instance.controllerPredActive = false;
		}
	}

	public static void SetControllerPrediction(int index)
	{
		index %= ControllerPredValues.Length;
		controllerPredIndex = ((index < 0) ? (ControllerPredValues.Length - 1) : index);
		controllerPred = ControllerPredValues[controllerPredIndex];
		NotifiLib.SendNotification("Set to " + ControllerPredNames[controllerPredIndex]);
	}

	private void ControllerPredTick()
	{
		if (!controllerPredActive)
			return;
		if (GorillaTagger.Instance == (Object)null)
			return;
		VRRig local = VRRig.LocalRig;
		if (local == (Object)null || local.head == null || local.head.rigTarget == (Object)null)
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
		if (local.leftHand != null && local.leftHand.rigTarget != (Object)null)
			local.leftHand.rigTarget.transform.position += (_predLeftVel - _predHeadVel) * controllerPred;
		if (local.rightHand != null && local.rightHand.rigTarget != (Object)null)
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
		if (instance != null)
		{
			instance.pullPower = PullPowerValues[pullPowerInt];
		}
		NotifiLib.SendNotification("Pull power: " + PullPowerNames[pullPowerInt]);
	}

	private void ProcessPullHand(bool left)
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
		if (instance == null)
		{
			return;
		}
		instance.ProcessPullHand(left: false);
		instance.ProcessPullHand(left: true);
	}

	private void UpdateJoystickFly()
	{
		if (GTPlayer.Instance == (Object)null || GorillaTagger.Instance == (Object)null || GorillaTagger.Instance.rigidbody == (Object)null) return;
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

	public static void MinosPrime()
	{
		if (instance == null)
		{
			return;
		}
		instance.MinosPrimeCore();
	}

	private void MinosPrimeCore()
	{
		PreloadMinosSounds();
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
			Camera mainCam = MainCamera();
			Vector3 val = ((mainCam != (Object)null) ? mainCam.transform.forward : Vector3.forward);
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
		if (instance == null)
		{
			return;
		}
		instance.minosPrimedForSlam = false;
		instance.minosWaitingForImpact = false;
		instance.minosSecondaryWasDown = false;
		instance.minosPrimaryWasDown = false;
		if (instance.minosRestoreCoroutine != null)
		{
			instance.StopCoroutine(instance.minosRestoreCoroutine);
			instance.minosRestoreCoroutine = null;
		}
		RestoreRecorder();
	}

	private void PlayMinosClip(AudioClip clip)
	{
		if (clip == (Object)null)
		{
			return;
		}
		if (minosLocalSource == (Object)null)
		{
			GameObject val = new GameObject("MinosAudio");
			Object.DontDestroyOnLoad(val);
			minosLocalSource = val.AddComponent<AudioSource>();
			minosLocalSource.spatialBlend = 0f;
			minosLocalSource.volume = 1f;
		}
		minosLocalSource.Stop();
		minosLocalSource.PlayOneShot(clip, 2f);
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (myRecorder != (Object)null)
		{
			if (minosRestoreCoroutine != null)
			{
				StopCoroutine(minosRestoreCoroutine);
			}
			myRecorder.SourceType = Recorder.InputSourceType.AudioClip;
		myRecorder.AudioClip = clip;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = true;
			minosRestoreCoroutine = StartCoroutine(RestoreMicAfter(clip.length));
		}
	}

	private static IEnumerator RestoreMicAfter(float delay)
	{
		yield return (object)new WaitForSeconds(delay + 0.4f);
		if (instance && ((Behaviour)instance).isActiveAndEnabled)
		{
			RestoreRecorder();
			instance.minosRestoreCoroutine = null;
		}
	}

	private static void RestoreRecorder()
	{
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (!(myRecorder == (Object)null))
		{
			myRecorder.SourceType = Recorder.InputSourceType.Microphone;
			myRecorder.AudioClip = null;
			myRecorder.RestartRecording(true);
			myRecorder.DebugEchoMode = false;
		}
	}

	public static void PreloadMinosSounds()
	{
		if (instance == null || instance.minosClipsLoaded) return;
		instance.minosClipsLoaded = true;
		instance.StartCoroutine(instance.LoadMinosSounds());
	}

	private IEnumerator LoadMinosSounds()
	{
		yield return SoundCache.GetClip(MinosCrushUrl, c => minosCrushClip = c);
		yield return SoundCache.GetClip(MinosSlamUrl, c => minosSlamClip = c);
	}

	private void SpiderMonkeyTick()
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
}
