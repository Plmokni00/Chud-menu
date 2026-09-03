using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "Awake")]
internal class RigAwakePatch
{
	public static bool Prefix(VRRig __instance)
	{
		if ((Object)(object)__instance == (Object)null) return true;
		if (Mods.cloningGhostRig) return false;
		string n = ((Component)__instance).gameObject.name;
		if (n == "Chud_GhostRig" || n == "Chud_GhostRigHolder") return false;
		return true;
	}
}
