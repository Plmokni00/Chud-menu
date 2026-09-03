using HarmonyLib;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(VRRig), "InitializeNoobMaterial")]
internal static class ModFixes
{
	[HarmonyPrefix]
	public static bool Prefix(VRRig __instance, float red, float green, float blue, PhotonMessageInfoWrapped info)
	{
		if (NetworkSystem.Instance == null || NetworkSystem.Instance.LocalPlayer == null)
			return true;
		if (info.senderID == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			if (float.IsNaN(red) || float.IsNaN(green) || float.IsNaN(blue))
				return false;
			red = Mathf.Clamp01(red);
			green = Mathf.Clamp01(green);
			blue = Mathf.Clamp01(blue);
			__instance.InitializeNoobMaterialLocal(red, green, blue);
			return false;
		}
		return true;
	}
}

[HarmonyPatch(typeof(GorillaGameManager), "OnPlayerPropertiesUpdate")]
internal static class ModFixNameSync
{
	[HarmonyPostfix]
	public static void Postfix(Player targetPlayer, Hashtable changedProps)
	{
		if (targetPlayer == null) return;
		try
		{
			int actorNum = targetPlayer.ActorNumber;
			foreach (RigContainer rc in VRRigCache.ActiveRigContainers)
			{
				if (rc == null) continue;
				NetPlayer creator = rc.Creator;
				if (creator != null && creator.ActorNumber == actorNum && (object)rc.Rig != null)
				{
					rc.Rig.UpdateName();
				}
			}
		}
		catch { }
	}
}
