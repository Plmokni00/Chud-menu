using GorillaLocomotion;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "PostTick")]
public static class PostTickPatch
{
	public static bool Prefix(VRRig __instance)
	{
		if (!__instance.isLocal)
		{
			return true;
		}
		return !Mods.LocalRigOverrideActive;
	}
}

[HarmonyPatch(typeof(VRRig), "PostTick")]
internal static class RigVisualPostTickPatch
{
	public static void Postfix(VRRig __instance)
	{
		if (__instance == null || !__instance.isLocal) return;
		if (ControllerInputPoller.instance == null) return;
		if (Mods.LocalRigOverrideActive) return;
		Mods.ApplyRigVisuals();
	}
}
