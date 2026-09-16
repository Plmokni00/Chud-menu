using GorillaNetworking;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GorillaComputer), "CheckAutoBanListForName")]
internal class AntiBanListPatch
{
	public static bool Prefix(string nameToCheck, ref bool __result)
	{
		if (!BanPatchState.enabled)
		{
			return true;
		}
		__result = true;
		return false;
	}
}
