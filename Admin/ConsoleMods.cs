using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using GTAG_NotificationLib;

namespace Chud.Backend;

public static partial class ConsoleMods
{
	private static void PlaySound(int id, string path, string clipName)
	{
		Console.ExecuteCommand("asset-playsound", ReceiverGroup.Others, id, path, clipName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playsound", id, path, clipName }, "asset-playsound");
	}

	private static void PlayAnimation(int id, string objectName, string animationName)
	{
		Console.ExecuteCommand("asset-playanimation", ReceiverGroup.Others, id, objectName, animationName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playanimation", id, objectName, animationName }, "asset-playanimation");
	}

	private static void SendLaser(bool enabled, bool rightHand, float r, float g, float b)
	{
		Console.ExecuteCommand("laser", ReceiverGroup.Others, enabled, rightHand, r, g, b);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "laser", enabled, rightHand, r, g, b }, "laser");
	}

	private static void Anchor(int id, int hand, int actor)
	{
		Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, id, hand, actor);
	}

	private static void SpawnSimpleAsset(ref int id, string bundle, string asset, Action<int> setup)
	{
		if (id < 0)
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, bundle, asset, setup));
		}
	}

	private static bool TryGetClipboardUrl(out string url)
	{
		url = GUIUtility.systemCopyBuffer;
		if (string.IsNullOrEmpty(url))
		{
			NotifiLib.SendNotification("Clipboard is empty - copy a URL first");
			return false;
		}
		return true;
	}

	public static void TPAllGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			Console.ExecuteCommand("tp", ReceiverGroup.Others, Mods.pointer.transform.position);
		});
	}

	public static void Run()
	{
		if (NoliStar.Enabled)
		{
			NoliStar.Run();
		}
		if (BanHammer.Enabled)
		{
			BanHammer.Run();
		}
		if (RainbowSword.Enabled)
		{
			RainbowSword.Run();
		}
		if (PhysicsGun.Enabled)
		{
			PhysicsGun.Run();
		}
		if (Laser.Enabled)
		{
			Laser.Run();
		}
		if (AdminGrab.Enabled)
		{
			AdminGrab.Run();
		}
		if (AdminGrabAll.Enabled)
		{
			AdminGrabAll.Run();
		}
		if (Pistol.Enabled)
		{
			Pistol.Run();
		}
		if (Coin.Enabled)
		{
			Coin.Run();
		}
		if (CherryBomb.Enabled)
		{
			CherryBomb.Run();
		}
		if (FreezeGun.Enabled)
		{
			FreezeGun.Run();
		}
		if (ScaleSelf.Enabled)
		{
			ScaleSelf.Run();
		}
	}

	public static void DestroyAsset(ref int id)
	{
		if (id < 0)
		{
			return;
		}
		Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, id);
		if (Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset asset))
		{
			asset.DestroyObject();
			Console.ConsoleAssets.Remove(id);
		}
		id = -1;
	}

	public static class WeirdEnderSword
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			Console.CustomBundleURLs["rgbendersword"] = "https://github.com/Seralyth/Console/raw/refs/heads/master/ServerData/rgbendersword";
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "rgbendersword", "sword", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Karambit
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			SpawnSimpleAsset(ref id, "karambit", "karambit", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.045f, 0.065f, 0f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(270f, 60f, 0f));
			});
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class RblxCarpet
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			SpawnSimpleAsset(ref id, "rblxcarpet", "robloxrainbowcarpet", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.2574666f, -0.007336602f, 0.1125555f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(1.562481f, 359.7548f, 155.0262f));
			});
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class McSword
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "mcsword", "Sword", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.03233476f, 0.0433403f, -0.08071579f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(302.1735f, 351.6904f, 280.6184f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, new Vector3(0.01450266f, 0.01450266f, 0.01450266f));
				if (Console.ConsoleAssets.TryGetValue(assetId, out Console.ConsoleAsset asset) && asset.obj != null)
				{
					Transform music = asset.obj.transform.Find("Music");
					if (music != null)
					{
						Object.Destroy(music.gameObject);
					}
				}
				Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, assetId, "Music", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class RobloxSword
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			SpawnSimpleAsset(ref id, "console.main1", "Sword", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			});
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Bag
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "bag", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.1427352f, 0.08271359f, 0.06961101f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(355.0145f, 350.4344f, 162.7124f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, new Vector3(9.717054f, 9.717054f, 9.717054f));
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Kormakur
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "KormakurSign", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.29f, -0.2f, -0.1272f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(355f, 275f, 265f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Boombox
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			SpawnBoombox();
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}

		private static void SpawnBoombox()
		{
			if (!TryGetClipboardUrl(out string url))
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Boombox", delegate (int assetId)
			{
				Anchor(assetId, 1, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0f, 0f, 0.15f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0f, 90f, 90f));
				Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, assetId, "Model", url);
			}));
		}
	}

	public static class Samsung
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (!TryGetClipboardUrl(out string url))
			{
				return;
			}
			SpawnSimpleAsset(ref id, "consolehamburburassets", "samsungphone", delegate (int assetId)
			{
				Anchor(assetId, 1, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(-0.075f, 0.1f, 0f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(80f, 90f, 180f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.3f);
				Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, assetId);
				Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "VideoPlayer", url);
			});
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class TV
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			SpawnTV();
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}

		private static void SpawnTV()
		{
			if (!TryGetClipboardUrl(out string url))
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "TV", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId, new Vector3(-57.1f, 5.6f, -37f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId, Quaternion.Euler(270f, 0f, 0f));
				Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "VideoPlayer", url);
			}));
		}
	}

	public static class Shreksophone
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "shrek", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5f);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Carti
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "carti", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5f);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class Travis
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId, new Vector3(-70f, 2f, -52f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.38f);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class TravisBeach
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(16.38702f, 12.29928f, 23.63119f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(352.4303f, 49.92272f, 0.8915782f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, new Vector3(0.38f, 0.38f, 0.38f));
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class TravisCritters
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(229.5867f, -98.26467f, 178.8833f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(4.141929f, 52.20211f, 2.67847f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, new Vector3(1.784783f, 1.784783f, 1.784783f));
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class TravisCity
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate (int assetId)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(-52.68209f, 16.36728f, -118.7615f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0.9019919f, 345.8464f, 1.200598f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, new Vector3(0.02183428f, 0.02183428f, 0.02183428f));
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static class AllowKickSelf
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.allowKickSelf = true;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.allowKickSelf = false;
		}
	}

	public static class AllowTpSelf
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.allowTpSelf = true;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.allowTpSelf = false;
		}
	}

	public static class DetectConsoleUsers
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.autoDetectConsoleUsers = true;
			Console.indicatorDelay = Time.time + 5f;
			Console.ScheduleConsoleUserScan();
		}

		public static void Disable()
		{
			Enabled = false;
			Console.autoDetectConsoleUsers = false;
			Console.ClearConsoleUserIndicators();
			Console.userDictionary.Clear();
		}
	}

	public static class NoAdminIndicator
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.ExecuteCommand("nocone", ReceiverGroup.Others, false);
		}

		public static void Disable()
		{
			Enabled = false;
			Console.ExecuteCommand("nocone", ReceiverGroup.Others, true);
		}
	}

	public static class FullAutoPistol
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.fullAutoPistol = true;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.fullAutoPistol = false;
		}
	}

	public static void KickAll()
	{
		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (rig.isLocal || rig.Creator == null)
			{
				continue;
			}
			Console.ExecuteCommand("strike", ReceiverGroup.All, rig.transform.position);
			Player player = Console.GetPlayerFromID(rig.Creator.UserId);
			if (player != null)
			{
				Console.ExecuteCommand("kick", player.ActorNumber, player.UserId);
			}
		}
	}

	public static class MinosPrime
	{
		public static bool Enabled;

		public static int id = -1;

		public static void Enable()
		{
			Console.CustomBundleURLs["minosprime"] = "https://github.com/Plmokni00/Chud-menu-files/raw/refs/heads/main/minosprime";
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "minosprime", "minosprime", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId, new Vector3(0.06263994f, 0.05301395f, -0.04137805f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId, Quaternion.Euler(286.3085f, 201.7456f, 347.1011f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.3518889f);
			}));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}

	public static void DestroyAllAssets()
	{
		foreach (KeyValuePair<int, Console.ConsoleAsset> entry in Console.ConsoleAssets)
		{
			entry.Value.DestroyObject();
		}
		Console.ConsoleAssets.Clear();
	}

	public static class CherryBomb
	{
		public static bool Enabled;

		private static int id = -1;

		private static bool detonated;

		private static float detonateTime;

		private static bool pendingDestroy;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				detonateTime = Time.time + 3.66f;
				detonated = false;
				pendingDestroy = false;
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "cherrybomb", "beam", delegate (int assetId)
				{
					if (pendingDestroy || !Enabled)
					{
						return;
					}
					Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId, GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(0f, 9.5f, 0f) + GorillaTagger.Instance.bodyCollider.transform.forward * -0.25f);
					Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "beam", "cherrybomb");
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			pendingDestroy = true;
			DestroyAsset(ref id);
			detonateTime = -1f;
			detonated = false;
			Enabled = false;
		}

		public static void Run()
		{
			if (!Enabled || id < 0 || Time.time <= detonateTime)
			{
				return;
			}
			if (!detonated)
			{
				detonated = true;
				Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, id, "beam", "show");
				if (Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset asset) && asset.obj != null)
				{
					Vector3 beamPos = asset.obj.transform.position;
					foreach (VRRig rig in VRRigCache.ActiveRigs)
					{
						if (rig.isLocal)
						{
							continue;
						}
						float dist = Vector3.Distance(rig.transform.position, beamPos);
						if (dist >= 15f || dist <= 1f)
						{
							continue;
						}
						Vector3 dir = (rig.transform.position - beamPos).normalized;
						NetPlayer creator = rig.Creator;
						if (creator == null)
						{
							continue;
						}
						Player target = creator.GetPlayerRef();
						if (target != null)
						{
							Console.ExecuteCommand("vel", target.ActorNumber, dir * 20f + Vector3.up * 5f);
						}
					}
				}
			}
			if (Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset current) && current.obj != null)
			{
				Console.TeleportPlayer(Vector3.Lerp(GorillaTagger.Instance.bodyCollider.transform.position, current.obj.transform.position + new Vector3(0f, -2f + Mathf.Sin(Time.time * 5f) * 1.25f, 0f), 0.01f));
				GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	public static class ConsoleLogging
	{
		public static bool Enabled;

		public static void Enable()
		{
			Enabled = true;
			Console.consoleLogging = true;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.consoleLogging = false;
		}
	}

	internal static Coroutine flingGunCoroutine;

	internal static int flingTargetActor;

	internal static int jailId = -1;

	private static VRRig kickGunTarget;

	public static void KickGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				kickGunTarget = rig;
				Console.ExecuteCommand("strike", ReceiverGroup.All, rig.transform.position);
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					Console.ExecuteCommand("kick", player.ActorNumber, player.UserId);
				}
			}
		}, delegate
		{
			kickGunTarget = null;
		});
		if (kickGunTarget != null && Mods.pointer != (Object)null && Mods.Line != (Object)null)
		{
			Mods.pointer.transform.position = ((Component)kickGunTarget).transform.position;
			Mods.Line.SetPosition(1, ((Component)kickGunTarget).transform.position);
		}
	}

	private static VRRig silentKickGunTarget;

	public static void SilentKickGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				silentKickGunTarget = rig;
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
				}
			}
		}, delegate
		{
			silentKickGunTarget = null;
		});
		if (silentKickGunTarget != null && Mods.pointer != (Object)null && Mods.Line != (Object)null)
		{
			Mods.pointer.transform.position = ((Component)silentKickGunTarget).transform.position;
			Mods.Line.SetPosition(1, ((Component)silentKickGunTarget).transform.position);
		}
	}

	public static void FlingGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					flingTargetActor = player.ActorNumber;
					if (flingGunCoroutine != null)
					{
						Mods.instance.StopCoroutine(flingGunCoroutine);
					}
					flingGunCoroutine = Mods.instance.StartCoroutine(FlingGunLoop());
				}
			}
		}, delegate
		{
			if (flingGunCoroutine != null)
			{
				Mods.instance.StopCoroutine(flingGunCoroutine);
				flingGunCoroutine = null;
			}
		});
	}

	private static IEnumerator FlingGunLoop()
	{
		while (true)
		{
			Console.ExecuteCommand("vel", flingTargetActor, Random.onUnitSphere * 30f + Vector3.up * 15f);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public static void LightningGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			Console.ExecuteCommand("strike", ReceiverGroup.All, Mods.pointer.transform.position);
		});
	}

	public static void VibrateGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					Console.ExecuteCommand("vibrate", player.ActorNumber, 3, 5f);
				}
			}
		});
	}

	public static class FreezeGun
	{
		public static bool Enabled;

		private static readonly Dictionary<int, Vector3> frozenTargets = new Dictionary<int, Vector3>();

		private static float lastFreeze;

		public static void Fire()
		{
			Enabled = true;
			Mods.MakeRightHandGun(delegate
			{
				VRRig rig = Mods.GetGunTargetPlayer();
				if (rig == null || rig.isLocal || rig.Creator == null)
				{
					return;
				}
				int actor = rig.Creator.ActorNumber;
				if (frozenTargets.ContainsKey(actor))
				{
					frozenTargets.Remove(actor);
					NotifiLib.SendNotification("Unfroze " + rig.Creator.NickName);
				}
				else
				{
					Vector3 freezePos = Mods.raycastHit.point;
					frozenTargets[actor] = freezePos;
					Console.ExecuteCommand("tp", actor, freezePos);
					NotifiLib.SendNotification("Froze " + rig.Creator.NickName);
				}
			});
		}

		public static void Disable()
		{
			Enabled = false;
			frozenTargets.Clear();
			Mods.CleanupGun();
		}

		public static void Run()
		{
			if (frozenTargets.Count == 0 || Time.time - lastFreeze < 0.05f)
			{
				return;
			}
			lastFreeze = Time.time;
			List<int> stale = null;
			foreach (KeyValuePair<int, Vector3> entry in frozenTargets)
			{
				Player player = PhotonNetwork.CurrentRoom?.GetPlayer(entry.Key);
				if (player == null)
				{
					(stale ??= new List<int>()).Add(entry.Key);
					continue;
				}
				Console.ExecuteCommand("tp", entry.Key, entry.Value);
			}
			if (stale != null)
			{
				foreach (int actor in stale)
				{
					frozenTargets.Remove(actor);
				}
			}
		}
	}

	public static class ScaleSelf
	{
		public static bool Enabled;

		private static float currentScale = 1f;

		private static NativeSizeChangerSettings scaleSettings;

		private static float lastBroadcast;

		public static void Enable()
		{
			Enabled = true;
			currentScale = 1f;
			scaleSettings = new NativeSizeChangerSettings
			{
				playerSizeScale = 1f,
				ExpireOnRoomJoin = false,
				ExpireInWater = false,
				ExpireAfterSeconds = 0f,
				ExpireOnDistance = 0f,
				WorldPosition = Vector3.zero,
				ActivationTime = Time.time
			};
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(scaleSettings);
			Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);
		}

		public static void Disable()
		{
			Enabled = false;
			currentScale = 1f;
			scaleSettings = null;
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(null);
			Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);
		}

		public static void Run()
		{
			ControllerInputPoller poller = ControllerInputPoller.instance;
			if (poller == null)
			{
				return;
			}
			if (poller.leftControllerIndexFloat > 0.3f)
			{
				currentScale = Mathf.Clamp(currentScale - Time.deltaTime * 3f, 0.1f, 10f);
			}
			if (poller.rightControllerIndexFloat > 0.3f)
			{
				currentScale = Mathf.Clamp(currentScale + Time.deltaTime * 3f, 0.1f, 10f);
			}
			scaleSettings.playerSizeScale = currentScale;
			scaleSettings.ActivationTime = Time.time;
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(scaleSettings);
			if (Time.time - lastBroadcast > 0.25f)
			{
				lastBroadcast = Time.time;
				Console.ExecuteCommand("scale", ReceiverGroup.All, currentScale);
			}
		}
	}

	public static void JailGun()
	{
		if (jailId < 0)
		{
			jailId = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(jailId, "jailcell", "jail", null));
		}
		Mods.MakeRightHandGun(delegate
		{
			VRRig target = Mods.GetGunTargetPlayer();
			if (target != null)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, jailId, ((Component)target).transform.position + new Vector3(-1f, -3f, -18f));
			}
		});
	}

	public static void JailGunOff()
	{
		Mods.CleanupGun();
		if (jailId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, jailId);
			jailId = -1;
		}
	}
}
