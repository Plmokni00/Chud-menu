using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GorillaGuardianZoneManager), "SetGuardian")]
internal class GuardianBreakPatch
{
	public static bool Prefix(GorillaGuardianZoneManager __instance, NetPlayer newGuardian)
	{
		if (!Mods.breakGuardianActive) return true;
		if (newGuardian != null && newGuardian.IsLocal) return true;
		return false;
	}
}
