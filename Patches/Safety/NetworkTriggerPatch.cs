using GorillaNetworking;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(GorillaNetworkJoinTrigger), "OnBoxTriggered")]
internal class NetworkTriggerPatch
{
	public static bool enabled;

	public static bool Prefix()
	{
		return !enabled;
	}
}
