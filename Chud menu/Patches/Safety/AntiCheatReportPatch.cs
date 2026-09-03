using HarmonyLib;
using GTAG_NotificationLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonkeAgent), "SendReport")]
public static class AntiCheatReportPatch
{
	public static void Postfix(string susReason, string susId, string susNick)
	{
		if (!Mods.seeAntiCheatReports)
		{
			return;
		}
		if (string.IsNullOrEmpty(susReason) || string.IsNullOrEmpty(susNick))
		{
			return;
		}
		string key = susReason + "_" + susNick;
		if (Mods.antiCheatReportCounts.TryGetValue(key, out var count))
		{
			Mods.antiCheatReportCounts[key] = count + 1;
			NotifiLib.SendNotification(susReason + " — " + susNick + " <color=yellow>" + (count + 1) + "x</color>");
		}
		else
		{
			Mods.antiCheatReportCounts[key] = 1;
			NotifiLib.SendNotification(susReason + " — " + susNick);
		}
	}
}
