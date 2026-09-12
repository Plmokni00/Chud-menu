using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "OnDisable")]
internal static class MacroRigDisablePatch
{
    public static bool Prefix(VRRig __instance)
    {
        if (__instance != null && __instance.isLocal && Mods.macroPlaybackActive)
            return false;
        return true;
    }
}
