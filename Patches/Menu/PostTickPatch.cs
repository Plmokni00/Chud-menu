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
