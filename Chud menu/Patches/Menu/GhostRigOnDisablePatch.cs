using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "OnDisable")]
internal static class GhostRigOnDisablePatch
{
	public static bool Prefix(VRRig __instance)
	{
		if (__instance != null && __instance.gameObject != null && __instance.gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}

[HarmonyPatch(typeof(VRRig), "OnEnable")]
internal static class GhostRigOnEnablePatch
{
	public static bool Prefix(VRRig __instance)
	{
		if (__instance != null && __instance.gameObject != null && __instance.gameObject.name == "Chud_GhostRig")
		{
			try
			{
				return false;
			} catch { return false; }
		}
		return true;
	}
}

[HarmonyPatch(typeof(BodyDockPositions), "RefreshTransferrableItems")]
internal static class GhostBodyDockPatch
{
	public static bool Prefix(BodyDockPositions __instance)
	{
		if (__instance == null) return true;
		VRRig rig = __instance.GetComponentInParent<VRRig>();
		if (rig != null && rig.gameObject != null && rig.gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}

[HarmonyPatch(typeof(VRRigCollection), "OnRigTriggerEnter")]
internal static class GhostVRRigCollectionPatch
{
	public static bool Prefix(Collider other)
	{
		if (other == null) return true;
		// VRRigCollection works on RigContainer/head colliders, not VRRig parents.
		// Check both so the ghost (which has no RigContainer) never enters collections.
		VRRig rig = other.GetComponentInParent<VRRig>();
		if (rig != null && rig.gameObject != null && rig.gameObject.name == "Chud_GhostRig")
			return false;
		RigContainer container = other.GetComponentInParent<RigContainer>();
		if (container != null && container.Rig != null && container.Rig.gameObject != null &&
			container.Rig.gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}

[HarmonyPatch(typeof(VRRigCollection), "OnRigTriggerExit")]
internal static class GhostVRRigCollectionExitPatch
{
	public static bool Prefix(Collider other)
	{
		if (other == null) return true;
		VRRig rig = other.GetComponentInParent<VRRig>();
		if (rig != null && rig.gameObject != null && rig.gameObject.name == "Chud_GhostRig")
			return false;
		return true;
	}
}
