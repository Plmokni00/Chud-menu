using Chud.UI;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace Chud.Backend;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
internal class OnPlayerJoined : HarmonyPatch
{
	private static void Prefix(Player newPlayer)
	{
		int count = PhotonNetwork.CurrentRoom.PlayerCount;
		NotifiLib.SendNotification("<color=#88ff88>" + newPlayer.NickName + "</color> joined (<color=white>" + count + "</color> players)");
		Mods.ARSCheckPlayer(newPlayer);
		Mods.TrackedCosmeticsCheckPlayer(newPlayer);
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
internal class OnPlayerLeft : HarmonyPatch
{
	private static void Prefix(Player otherPlayer)
	{
		if (otherPlayer != PhotonNetwork.LocalPlayer)
		{
			int count = PhotonNetwork.CurrentRoom.PlayerCount;
			NotifiLib.SendNotification("<color=#ff8888>" + otherPlayer.NickName + "</color> left (<color=white>" + count + "</color> players)");
		}
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnJoinedRoom")]
internal class OnJoinedRoom : HarmonyPatch
{
	private static void Postfix()
	{
		int count = PhotonNetwork.CurrentRoom.PlayerCount;
		NotifiLib.SendNotification("You joined (<color=white>" + count + "</color> players)");
		Mods.ReapplyActiveMods();
		Mods.TrackedCosmeticsScan();
		if (Console.autoDetectConsoleUsers)
		{
			Console.ScheduleConsoleUserScan();
		}
	}
}

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnLeftRoom")]
internal class OnLocalLeftRoom : HarmonyPatch
{
	private static void Prefix()
	{
		Mods.DisableAntiReport();
	}
}


