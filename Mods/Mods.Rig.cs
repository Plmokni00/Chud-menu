using System.Collections.Generic;
using Chud.UI;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace Chud.Backend;

internal partial class Mods
{
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

	private bool grabRigActive = false;

	private bool ghostMonkeLastPress = false;

	private Vector3 ghostMonkeFrozenPos;

	private Quaternion ghostMonkeFrozenRot;

	private TransformSnapshot ghostMonkeSnapshot;

	private Vector3 invisMonkeSavedPos;

	private bool invisMonkeLastPress = false;

	private bool invisMonkeSkinsDisabled = false;

	private VRRig ghostRig;

	private Material ghostRigMaterial;

	private bool ghostRigSubscribed;

	private Renderer[] ghostSkinRenderers;

	private bool copyMovementActive;

	private VRRig copyMovementTarget;

	private VRRig orbitTarget = null;

	private bool orbitActive = false;

	private float orbitAngle = 0f;

	private bool tagRigVisualSubscribed = false;

	private bool backflipActive;

	private float backflipRotation;

	private Quaternion backflipStartRot;

	private bool backflipEnabled;

	private bool frontflipActive;

	private float frontflipRotation;

	private Quaternion frontflipStartRot;

	private bool frontflipEnabled;

	private bool lastFlipButton;

	private bool spinningTorsoEnabled;

	private bool fakeFBTEnabled;

	private bool dinnerboneEnabled;

	private bool natsukiNeckEnabled;

	private Vector3 natsukiSavedPos;

	private Quaternion natsukiSavedRot;

	private bool natsukiHasSaved;

	public static void ApplyRigVisuals()
	{
		if (instance == null)
		{
			return;
		}
		instance.ControllerPredTick();
		instance.FlipTick();
		instance.SpinningTorsoTick();
		instance.FakeFBTTick();
		instance.DinnerboneTick();
		instance.NatsukiNeckTick();
	}

	private void TakeRigSnapshot(out TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		s = default(TransformSnapshot);
		if (localRig.head != null && localRig.head.rigTarget != (Object)null)
		{
			s.headPos = localRig.head.rigTarget.transform.position;
			s.headRot = localRig.head.rigTarget.transform.rotation;
		}
		if (localRig.leftHand != null && localRig.leftHand.rigTarget != (Object)null)
		{
			s.leftHandPos = localRig.leftHand.rigTarget.transform.position;
			s.leftHandRot = localRig.leftHand.rigTarget.transform.rotation;
		}
		if (localRig.rightHand != null && localRig.rightHand.rigTarget != (Object)null)
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

	private void ApplyRigSnapshot(ref TransformSnapshot s)
	{
		VRRig localRig = VRRig.LocalRig;
		if (localRig.head != null && localRig.head.rigTarget != (Object)null)
		{
			localRig.head.rigTarget.transform.SetPositionAndRotation(s.headPos, s.headRot);
		}
		if (localRig.leftHand != null && localRig.leftHand.rigTarget != (Object)null)
		{
			localRig.leftHand.rigTarget.transform.SetPositionAndRotation(s.leftHandPos, s.leftHandRot);
		}
		if (localRig.rightHand != null && localRig.rightHand.rigTarget != (Object)null)
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
		if (instance == null || VRRig.LocalRig == (Object)null)
		{
			return;
		}
		instance.GhostMonkeCore();
	}

	private void GhostMonkeCore()
	{
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
				if (local.head != null && local.head.rigTarget != (Object)null)
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
		if (instance != null)
		{
			instance.TryUnsubscribeGhostRig();
		}
	}

	private void InvisMonkeSetSkins(bool disable)
	{
		if (!(VRRig.LocalRig == (Object)null) && disable != invisMonkeSkinsDisabled)
		{
			SkinnedMeshRenderer mainSkin = VRRig.LocalRig.mainSkin;
			if (!(mainSkin == (Object)null))
			{
				((Renderer)mainSkin).enabled = !disable;
				invisMonkeSkinsDisabled = disable;
			}
		}
	}

	public static void InvisMonke()
	{
		if (instance == null || VRRig.LocalRig == (Object)null)
		{
			return;
		}
		instance.InvisMonkeCore();
	}

	private void InvisMonkeCore()
	{
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
				if (local.head != null && local.head.rigTarget != (Object)null)
					local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
				return;
			}
			VRRig.LocalRig.transform.position = new Vector3(9999f, 9999f, 9999f);
		}
	}

	public static void DisableInvisMonke()
	{
		if (VRRig.LocalRig != (Object)null && invisMonkeOn)
		{
			EnsureLocalRigEnabled();
			VRRig.LocalRig.transform.position = instance != null ? instance.invisMonkeSavedPos : VRRig.LocalRig.transform.position;
			if (instance != null)
			{
				instance.InvisMonkeSetSkins(disable: false);
			}
		}
		invisMonkeOn = false;
		if (instance != null)
		{
			instance.TryUnsubscribeGhostRig();
		}
	}

	private static void EnsureLocalRigEnabled()
	{
		if (VRRig.LocalRig != (Object)null && !VRRig.LocalRig.enabled)
			VRRig.LocalRig.enabled = true;
	}

	public static void GrabRig()
	{
		if (instance == null)
		{
			return;
		}
		if (WristMenu.gripDownR)
		{
			if (!instance.grabRigActive)
			{
				instance.grabRigActive = true;

				instance.SubscribeGhostRig();
			}
		}
		else if (instance.grabRigActive)
		{
			instance.grabRigActive = false;
			EnsureLocalRigEnabled();
			instance.TryUnsubscribeGhostRig();
		}
	}

	public static void DisableGrabRig()
	{
		if (instance == null)
		{
			return;
		}
		instance.grabRigActive = false;
		EnsureLocalRigEnabled();
		instance.TryUnsubscribeGhostRig();
	}

	private void GrabRigTick()
	{
		if (!grabRigActive || VRRig.LocalRig == (Object)null)
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
			if (local.head != null && local.head.rigTarget != (Object)null)
				local.head.rigTarget.transform.SetPositionAndRotation(pos + Vector3.up * 0.2f, rot);
			return;
		}
		Transform hand = GorillaTagger.Instance.rightHandTransform;
		VRRig local2 = VRRig.LocalRig;
		local2.transform.SetPositionAndRotation(hand.position, hand.rotation);
		if (local2.head != null && local2.head.rigTarget != (Object)null)
			local2.head.rigTarget.transform.SetPositionAndRotation(hand.position, hand.rotation);
	}

	private void TagRigVisualTick()
	{
		VRRig localRig = VRRig.LocalRig;
		if (localRig == (Object)null) return;

		VRRig target = null;
		if (tagGunLockedTarget != null && !tagGunLockedTarget.isLocal && tagGunLockedTarget != (Object)null)
			target = tagGunLockedTarget;
		else if (tagAllTarget != null && !tagAllTarget.isLocal && tagAllTarget != (Object)null)
			target = tagAllTarget;

		if (target == null) return;

		EnsureLocalRigEnabled();
		Vector3 targetPos = ((Component)target).transform.position - new Vector3(0f, 3f, 0f);
		localRig.transform.position = targetPos;
		if (localRig.head != null && localRig.head.rigTarget != (Object)null)
			localRig.head.rigTarget.transform.position = targetPos;
		if (localRig.leftHand != null && localRig.leftHand.rigTarget != (Object)null)
			localRig.leftHand.rigTarget.transform.position = targetPos;
		if (localRig.rightHand != null && localRig.rightHand.rigTarget != (Object)null)
			localRig.rightHand.rigTarget.transform.position = targetPos;
	}

	private void SubscribeTagRigVisual()
	{
		if (!tagRigVisualSubscribed)
		{
			tagRigVisualSubscribed = true;
		}
	}

	private void UnsubscribeTagRigVisual()
	{
		if (tagRigVisualSubscribed)
		{
			tagRigVisualSubscribed = false;
		}
	}

	private void GhostRigTick()
	{
		VRRig local = VRRig.LocalRig;
		if (local == (Object)null)
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
		if (ghostRig == (Object)null) return;
		if (!ghostRig.gameObject.activeSelf)
			ghostRig.gameObject.SetActive(true);
		GorillaTagger tagger = GorillaTagger.Instance;
		if (tagger == (Object)null || tagger.headCollider == (Object)null)
			return;
		Transform headT = tagger.headCollider.transform;
		float scale = local.scaleFactor;
		if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
			scale = (GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f;
		Quaternion bodyRot = GorillaLocomotion.GTPlayerTransform.BodyRotation;
		Quaternion liveRot = headT.rotation;
		if (GTPlayer.Instance != (Object)null && GTPlayer.Instance.mainCamera != (Object)null)
			liveRot = GTPlayer.Instance.mainCamera.transform.rotation;
		Vector3 headWorldPos = (GTPlayer.Instance != (Object)null && GTPlayer.Instance.mainCamera != (Object)null) ? GTPlayer.Instance.mainCamera.transform.position : headT.position;
		Vector3 trackOffset = (local.head != null) ? local.head.trackingPositionOffset : Vector3.zero;
		Vector3 headPos = headWorldPos + liveRot * trackOffset * scale;
		ghostRig.transform.SetPositionAndRotation(headPos + ghostRig.transform.rotation * local.headBodyOffset * scale, bodyRot);
		if (ghostRig.head != null && ghostRig.head.rigTarget != (Object)null)
			ghostRig.head.rigTarget.transform.SetPositionAndRotation(headPos, liveRot);
		if (XRSettings.isDeviceActive)
		{
			Transform liveOffset = (local.playerOffsetTransform != (Object)null) ? local.playerOffsetTransform : ghostRig.playerOffsetTransform;
			if (ghostRig.leftHand != null)
				ghostRig.leftHand.MapMine(scale, liveOffset);
			if (ghostRig.rightHand != null)
				ghostRig.rightHand.MapMine(scale, liveOffset);
		}
		float fingerLerp = ghostRig.lerpValueFingers;
		if (XRSettings.isDeviceActive)
		{
			if (ghostRig.rightIndex != null) ghostRig.rightIndex.MapMyFinger(fingerLerp);
			if (ghostRig.rightMiddle != null) ghostRig.rightMiddle.MapMyFinger(fingerLerp);
			if (ghostRig.rightThumb != null) ghostRig.rightThumb.MapMyFinger(fingerLerp);
			if (ghostRig.leftIndex != null) ghostRig.leftIndex.MapMyFinger(fingerLerp);
			if (ghostRig.leftMiddle != null) ghostRig.leftMiddle.MapMyFinger(fingerLerp);
			if (ghostRig.leftThumb != null) ghostRig.leftThumb.MapMyFinger(fingerLerp);
		}
		if (ghostRigMaterial != (Object)null)
		{
			Color want = local.playerColor;
			if (want.r < 4f / 255f && want.g < 4f / 255f && want.b < 4f / 255f)
				want = Color.white;
			want.a = 0.5f;
			ghostRigMaterial.color = want;
			ApplyGhostMaterial();
		}
	}

	private bool GhostWanted()
	{
		return tagGunLockedTarget != null || tagAllTarget != null || grabRigActive || ghostMonkeOn || invisMonkeOn || (copyMovementActive && copyMovementTarget != null) || orbitActive;
	}

	private void ApplyGhostMaterial()
	{
		if (ghostRig == (Object)null || ghostRigMaterial == (Object)null) return;
		if (ghostSkinRenderers == null)
			CacheGhostRenderers();
		if (ghostSkinRenderers == null) return;
		foreach (Renderer r in ghostSkinRenderers)
		{
			if (r == (Object)null) continue;
			r.material = ghostRigMaterial;
		}
	}

	private void CacheGhostRenderers()
	{
		if (ghostRig == (Object)null) return;
		List<Renderer> found = new List<Renderer>();
		if (ghostRig.mainSkin != (Object)null)
			found.Add(ghostRig.mainSkin);
		Renderer[] renderers = ghostRig.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in renderers)
		{
			if (r == (Object)null) continue;
			if (r == (Object)ghostRig.mainSkin) continue;
			if (r is SkinnedMeshRenderer) found.Add(r);
		}
		ghostSkinRenderers = found.ToArray();
	}

	private void EnsureGhostRig()
	{
		if (ghostRig != (Object)null) return;

		VRRig local = VRRig.LocalRig;
		if (local == (Object)null) return;

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
		if (ghostRig == (Object)null)
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

		if (ghostRig.transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio") != (Object)null)
			ghostRig.transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio").gameObject.SetActive(false);
		if (ghostRig.transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio") != (Object)null)
			ghostRig.transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio").gameObject.SetActive(false);
		if (ghostRig.transform.Find("rig/body_pivot/SlideAudio") != (Object)null)
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
		foreach (Component c in ghostRig.GetComponentsInChildren<Component>(true))
		{
			if (c == null || c is Transform) continue;
			string tn = c.GetType().Name;
			if (tn == "TagEffectsPackToggle" || tn == "RigidbodyWaterInteraction" || tn == "ConstantForce")
				Object.Destroy(c);
		}
		foreach (Collider col in ghostRig.GetComponentsInChildren<Collider>(true))
			Object.Destroy(col);
		foreach (Rigidbody rb in ghostRig.GetComponentsInChildren<Rigidbody>(true))
			Object.Destroy(rb);
		foreach (GorillaPlayerScoreboardLine line in ghostRig.GetComponentsInChildren<GorillaPlayerScoreboardLine>(true))
			Object.Destroy(line);

		CleanGhostRigGameplay();
		CacheGhostRenderers();

		if (ghostRigMaterial == (Object)null)
		{
			Shader s = Shader.Find("GorillaTag/UberShader");
			if (s == (Object)null) s = Shader.Find("Universal Render Pipeline/Unlit");
			if (s == (Object)null) s = Shader.Find("GUI/Text Shader");
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

	private void CleanGhostRigGameplay()
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
			if (cosmeticObject != (Object)null)
			{
				cosmeticObject.SetActive(false);
				Object.Destroy(cosmeticObject);
			}
		}
	}

	private void HideGhostRig()
	{
		if (ghostRig != (Object)null)
		{
			ghostRig.gameObject.SetActive(false);
			ghostRig.transform.position = Vector3.one * 9999f;
		}
	}

	private void SubscribeGhostRig()
	{
		if (!ghostRigSubscribed)
		{
			ghostRigSubscribed = true;
		}
	}

	private void UnsubscribeGhostRig()
	{
		if (ghostRigSubscribed)
		{
			ghostRigSubscribed = false;
			HideGhostRig();
		}
	}

	private void TryUnsubscribeGhostRig()
	{
		if (!GhostWanted())
			UnsubscribeGhostRig();
	}

	private void CopyMovementTick()
	{
		if (!copyMovementActive || copyMovementTarget == (Object)null || VRRig.LocalRig == (Object)null)
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
		if (target.head != null && target.head.rigTarget != (Object)null && local.head != null && local.head.rigTarget != (Object)null)
		{
			local.head.rigTarget.transform.SetPositionAndRotation(target.head.rigTarget.transform.position, target.head.rigTarget.transform.rotation);
		}
		if (target.leftHand != null && target.leftHand.rigTarget != (Object)null && local.leftHand != null && local.leftHand.rigTarget != (Object)null)
		{
			local.leftHand.rigTarget.transform.SetPositionAndRotation(target.leftHand.rigTarget.transform.position, target.leftHand.rigTarget.transform.rotation);
		}
		if (target.rightHand != null && target.rightHand.rigTarget != (Object)null && local.rightHand != null && local.rightHand.rigTarget != (Object)null)
		{
			local.rightHand.rigTarget.transform.SetPositionAndRotation(target.rightHand.rigTarget.transform.position, target.rightHand.rigTarget.transform.rotation);
		}
	}

	private void OrbitTick()
	{
		if (!orbitActive || orbitTarget == (Object)null || VRRig.LocalRig == (Object)null) return;
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
		if (local.head != null && local.head.rigTarget != (Object)null)
			local.head.rigTarget.transform.SetPositionAndRotation(headPos, look);
		Vector3 right = look * Vector3.right;
		Vector3 leftPos = pos + right * -1.1f + Vector3.up * 0.15f;
		Vector3 rightPos = pos + right * 1.1f + Vector3.up * 0.15f;
		Quaternion leftRot = look * Quaternion.Euler(0, 0, 90);
		Quaternion rightRot = look * Quaternion.Euler(0, 0, -90);
		if (local.leftHand != null && local.leftHand.rigTarget != (Object)null)
			local.leftHand.rigTarget.transform.SetPositionAndRotation(leftPos, leftRot);
		if (local.rightHand != null && local.rightHand.rigTarget != (Object)null)
			local.rightHand.rigTarget.transform.SetPositionAndRotation(rightPos, rightRot);
		if (local.leftIndex != null) { local.leftIndex.calcT = 0f; local.leftIndex.LerpFinger(1f, false); }
		if (local.leftMiddle != null) { local.leftMiddle.calcT = 0f; local.leftMiddle.LerpFinger(1f, false); }
		if (local.leftThumb != null) { local.leftThumb.calcT = 0f; local.leftThumb.LerpFinger(1f, false); }
		if (local.rightIndex != null) { local.rightIndex.calcT = 0f; local.rightIndex.LerpFinger(1f, false); }
		if (local.rightMiddle != null) { local.rightMiddle.calcT = 0f; local.rightMiddle.LerpFinger(1f, false); }
		if (local.rightThumb != null) { local.rightThumb.calcT = 0f; local.rightThumb.LerpFinger(1f, false); }
	}

	private void FlipTick()
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

	private void SpinningTorsoTick()
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

	private void FakeFBTTick()
	{
		if (!fakeFBTEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null) return;
		rig.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
		rig.head.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.leftHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
		rig.rightHand.MapMine(rig.scaleFactor, rig.playerOffsetTransform);
	}

	private void DinnerboneTick()
	{
		if (!dinnerboneEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null) return;
		float scale = rig.scaleFactor;
		if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
			scale = 1f;
		rig.transform.rotation = GTPlayerTransform.BodyRotation * Quaternion.Euler(0f, 0f, 180f);
		rig.transform.position += rig.transform.rotation * Vector3.up * 0.25f * scale;
	}

	private void NatsukiNeckTick()
	{
		if (!natsukiNeckEnabled) return;
		VRRig rig = VRRig.LocalRig;
		if (rig == null || rig.head == null || rig.head.rigTarget == (Object)null) return;
		if (GorillaTagger.Instance == (Object)null || GorillaTagger.Instance.headCollider == (Object)null) return;
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
}
