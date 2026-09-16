using HarmonyLib;
using UnityEngine;

namespace Chud.Backend;

internal static class ColorUtil
{
	internal static Color PlayerColor(VRRig rig)
	{
		if ((object)rig == null) return Color.white;
		Color c = rig.playerColor;
		if (c.r < 4f / 255f && c.g < 4f / 255f && c.b < 4f / 255f)
			return Color.white;
		return c;
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SetNameTagText")]
internal static class UnsanitizedChestPatch
{
	private static string SanitizeName(string s)
	{
		if (string.IsNullOrEmpty(s)) return s;
		if (s.Length > 32) s = s.Substring(0, 32);
		s = s.Replace("<size", "<SIZE").Replace("size=", "SIZE=");
		return s;
	}
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			if ((Object)(object)__instance == (Object)null || (Object)(object)__instance.playerText1 == (Object)null) return;
			NetPlayer netPlayer = __instance.Creator;
			string rawName = (netPlayer != null) ? netPlayer.NickName : NetworkSystem.Instance.GetMyNickName();
			if (!string.IsNullOrEmpty(rawName)) __instance.playerText1.text = SanitizeName(rawName);
		}
		catch { }
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SerializeReadShared")]
internal static class ChestColorPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("OnSubscriptionData")]
internal static class ChestColorSubPatch
{
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance)
	{
		try
		{
			__instance.playerText1.color = ColorUtil.PlayerColor(__instance);
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
[HarmonyPatch("UpdatePlayerText")]
internal static class UnsanitizedBoardPatch
{
	[HarmonyPrefix]
	public static bool Prefix(GorillaPlayerScoreboardLine __instance)
	{
		if ((Object)(object)__instance == (Object)null) return false;
		if ((Object)(object)__instance.playerName == (Object)null) return false;
		if (__instance.linePlayer == null) return false;
		return true;
	}
}