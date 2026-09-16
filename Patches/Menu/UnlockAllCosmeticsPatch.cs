using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "IsItemAllowed")]
internal class UnlockAllCosmeticsPatch
{
	public static bool enabled;

	public static void Postfix(ref bool __result)
	{
		if (enabled) __result = true;
	}
}
