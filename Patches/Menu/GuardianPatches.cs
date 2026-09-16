using GorillaLocomotion;
using HarmonyLib;

namespace Chud.Backend;

internal static class GuardianPatches
{
	public static bool clampedKnockback;
	public static bool knockedBack;
	public static bool launched;
	public static bool trajectoryOverridden;
	public static bool grabbedBy;
}

[HarmonyPatch(typeof(GTPlayer), "ApplyClampedKnockback")]
internal static class GuardianClampedKnockbackPatch
{
	public static bool Prefix() => !GuardianPatches.clampedKnockback;
}

[HarmonyPatch(typeof(GTPlayer), "ApplyKnockback")]
internal static class GuardianKnockbackPatch
{
	public static bool Prefix() => !GuardianPatches.knockedBack;
}

[HarmonyPatch(typeof(GTPlayer), "DoLaunch")]
internal static class GuardianLaunchPatch
{
	public static bool Prefix() => !GuardianPatches.launched;
}

[HarmonyPatch(typeof(VRRig), "ApplyLocalTrajectoryOverride")]
internal static class GuardianTrajectoryPatch
{
	public static bool Prefix() => !GuardianPatches.trajectoryOverridden;
}

[HarmonyPatch(typeof(VRRig), "GrabbedByPlayer")]
internal static class GuardianGrabbedByPatch
{
	public static bool Prefix() => !GuardianPatches.grabbedBy;
}
