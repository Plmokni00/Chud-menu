using HarmonyLib;
using GorillaLocomotion;

namespace Chud.Backend;

[HarmonyPatch(typeof(GTPlayer), "ApplyNativeScaleAdjustment")]
internal class ScaleSpeedPatch
{
	[HarmonyPrefix]
	public static bool Prefix(ref float __result, float adjustedMagnitude)
	{
		if (ConsoleMods.ScaleSelf.Enabled)
		{
			__result = adjustedMagnitude;
			return false;
		}
		return true;
	}
}
