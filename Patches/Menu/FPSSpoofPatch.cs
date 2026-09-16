using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "PackCompetitiveData")]
internal class FPSSpoofPatch
{
	public static void Postfix(VRRig __instance, ref short __result)
	{
		if (!Mods.fpsSpoofActive || !__instance.isLocal)
			return;
		__result = (short)((__result & 0xFF00) | Mods.fpsSpoofValue);
	}
}

[HarmonyPatch(typeof(GTPlayerStats), "GetPackedValues")]
internal class FPSSpoofStatsPatch
{
	public static void Postfix(ref long __result)
	{
		if (!Mods.fpsSpoofActive)
			return;
		long fpsMask = ~(0xFFFFL << 16);
		__result = (__result & fpsMask) | ((long)(Mods.fpsSpoofValue & 0xFFFF) << 16);
	}
}
