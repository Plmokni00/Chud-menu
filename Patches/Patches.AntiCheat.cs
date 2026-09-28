using Chud.Backend;
using HarmonyLib;
namespace Chud.Patches
{
	[HarmonyPatch(typeof(MonkeAgent), "SendReport")]
	public static class AntiCheatReportPatch
	{
		private static void Postfix(string susReason, string susId, string susNick)
		{
			Mods.RecordAntiCheatReport(susReason, susNick);
		}
	}
}