using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "PlayHandTapLocal")]
internal class JmanSoundPatch
{
	public static bool enabled;

	public static bool Prefix(VRRig __instance, int audioClipIndex)
	{
		if (!enabled)
		{
			return true;
		}
		if ((Object)(object)__instance == (Object)(object)VRRig.LocalRig)
		{
			return true;
		}
		return audioClipIndex < 336 || audioClipIndex > 338;
	}
}
