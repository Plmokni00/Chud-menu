using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using GTAG_NotificationLib;

namespace Chud.Backend;

public static class ConsoleMods
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

	public static class NoliStar
	{
		public static bool Enabled;

		private static int id = -1;

		private static float updateDelay;

		private static float respawnTime;

		private static bool holdingTrigger;

		private static Vector3 throwDirection;

		private static Vector3 networkedPos;

		private static Quaternion networkedRot;

		private static int state;

		public static void Enable()
		{
			if (Enabled)
			{
				return;
			}
			Enabled = true;
			id = -1;
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Run()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Star", delegate (int assetId)
				{
					PlaySound(assetId, "Model", "StarSpawn");
				}));
			}
			if (!Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset starAsset) || starAsset.obj == null)
			{
				return;
			}
			GameObject starObj = starAsset.obj;
			ControllerInputPoller poller = ControllerInputPoller.instance;
			float trigger = poller.rightControllerIndexFloat;
			if (trigger > 0.5f && state == 0)
			{
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit aimRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				GameObject crosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				crosshair.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				crosshair.transform.position = aimRay.point == Vector3.zero ? aimRay.transform.position + aimRay.transform.forward * 20f : aimRay.point;
				crosshair.GetComponent<Renderer>().material.color = Color.white;
				Object.Destroy(crosshair, Time.deltaTime);
				Object.Destroy(crosshair.GetComponent<Collider>());
			}
			if (trigger < 0.5f && holdingTrigger && state == 0)
			{
				state = 1;
				PlayAnimation(id, "Model", "Throw");
				PlaySound(id, "Model", "ThrowStar");
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit dirRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				throwDirection = (dirRay.point - starObj.transform.position).normalized;
			}
			holdingTrigger = trigger > 0.5f;
			switch (state)
			{
				case 0:
					starObj.transform.position = GorillaTagger.Instance.rightHandTransform.position + Vector3.up * 0.2f;
					starObj.transform.rotation = Quaternion.Euler(Time.time * 32f, Time.time * 10f, Time.time * 47f);
					break;
				case 1:
				{
					Physics.Raycast(starObj.transform.position, throwDirection, out RaycastHit hitRay, 0.5f, GTPlayer.Instance.locomotionEnabledLayers);
					if (hitRay.point == Vector3.zero)
					{
						starObj.transform.position += throwDirection * (Time.deltaTime * 15f);
						starObj.transform.rotation = Quaternion.Euler(Time.time * 239f, Time.time * 201f, Time.time * 170f);
					}
					else
					{
						PlayAnimation(id, "Model", "Explode");
						bool killed = false;
						foreach (VRRig rig in VRRigCache.ActiveRigs)
						{
							if (!rig.isLocal && Vector3.Distance(starObj.transform.position, rig.transform.position) < 2.32775f && rig.Creator != null)
							{
								Player target = Console.GetPlayerFromID(rig.Creator.UserId);
								if (target != null)
								{
									Console.ExecuteCommand("silkick", target.ActorNumber, target.UserId);
								}
								killed = true;
							}
						}
						PlaySound(id, "Model", killed ? "KillStar" : "BreakStar");
						state = 2;
						respawnTime = Time.time + 3f;
					}
					break;
				}
				case 2:
					if (Time.time > respawnTime)
					{
						PlayAnimation(id, "Model", "Default");
						PlaySound(id, "Model", "StarSpawn");
						state = 0;
					}
					break;
			}
			if (Time.time > updateDelay && (networkedRot != starObj.transform.rotation || networkedPos != starObj.transform.position))
			{
				updateDelay = Time.time + 0.05f;
				networkedPos = starObj.transform.position;
				networkedRot = starObj.transform.rotation;
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.Others, id, starObj.transform.position);
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.Others, id, starObj.transform.rotation);
			}
		}
	}

	public static class BanHammer
	{
		public static bool Enabled;

		public static int id = -1;

		private static float slashDelay;

		private static float pauseSfx;

		private static bool lastVelTooHigh;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "banhammer", "BanHammer", delegate (int assetId)
				{
					Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}, addSurfaceOverride: true));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelay = 0f;
			pauseSfx = 0f;
			lastVelTooHigh = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset asset) || asset.obj == null)
			{
				return;
			}
			Transform rayPoint = asset.obj.transform.Find("Model/HitBox");
			if (rayPoint == null)
			{
				return;
			}
			if (!rayPoint.TryGetComponent(out MeshCollider _))
			{
				rayPoint.gameObject.AddComponent<MeshCollider>();
			}
			Physics.SphereCast(rayPoint.position, 0.2f, rayPoint.forward, out RaycastHit playerRay, 0.4f, Mods.GetNoInvisLayerMask());
			Physics.SphereCast(rayPoint.position, 0.2f, rayPoint.forward, out RaycastHit worldRay, 0.4f, GTPlayer.Instance.locomotionEnabledLayers);
			Vector3 handVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
			Vector3 bodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
			bool velTooHigh = (handVel - bodyVel).magnitude > 10f;
			if (Time.time > slashDelay)
			{
				if (playerRay.collider != null)
				{
					VRRig target = playerRay.collider.GetComponentInParent<VRRig>();
					if (target != null && !target.isLocal)
					{
						slashDelay = Time.time + 1f;
						pauseSfx = Time.time + 1f;
						Console.instance.StartCoroutine(KillFX());
						NetPlayer netPlayer = target.Creator;
						Console.ExecuteCommand("silkick", netPlayer.ActorNumber, netPlayer.UserId);
					}
				}
				if (worldRay.collider != null)
				{
					slashDelay = Time.time + 0.3f;
					pauseSfx = Time.time + 0.5f;
					float totalVel = handVel.magnitude + bodyVel.magnitude;
					GorillaTagger.Instance.rigidbody.linearVelocity += worldRay.normal * Mathf.Clamp(totalVel, 1f, 14f);
					Console.instance.StartCoroutine(HitFX());
				}
			}
			if (velTooHigh && !lastVelTooHigh && Time.time > pauseSfx)
			{
				pauseSfx = Time.time + 0.3f;
				PlaySound(id, "Model/SwingSFX", "Swing");
			}
			lastVelTooHigh = velTooHigh;
		}

		private static IEnumerator KillFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/KillSFX", "HammerKill");
			PlayAnimation(id, "Model", "HitPlayer");
		}

		private static IEnumerator HitFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/SwingSFX", "HammerHit");
			PlayAnimation(id, "Model", "HitGround");
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.transform.position) < 2f)
				{
					Console.ExecuteCommand("vel", rig.Creator.ActorNumber, (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 5f);
				}
			}
		}
	}

	public static class RainbowSword
	{
		public static bool Enabled;

		public static int id = -1;

		private static float slashDelay;

		private static float pauseSfx;

		private static bool lastVelTooHigh;

		public static void Enable()
		{
			if (id >= 0)
			{
				return;
			}
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "rbsword", "Sword", delegate (int assetId)
			{
				Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}, addSurfaceOverride: true));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelay = 0f;
			pauseSfx = 0f;
			lastVelTooHigh = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset asset) || asset.obj == null)
			{
				return;
			}
			Transform rayPoint = asset.obj.transform.Find("Sword/HitBox");
			if (rayPoint == null)
			{
				return;
			}
			Physics.SphereCast(rayPoint.position, 0.1f, rayPoint.forward, out RaycastHit hit, 0.7f, Mods.GetNoInvisLayerMask());
			if (Time.time > slashDelay && hit.collider != null)
			{
				try
				{
					VRRig target = hit.collider.GetComponentInParent<VRRig>();
					if (target != null && !target.isLocal && target.Creator != null)
					{
						slashDelay = Time.time + 0.5f;
						pauseSfx = Time.time + 1f;
						PlaySound(id, "Sword/SFX", "Slash" + Random.Range(1, 3));
						PlayAnimation(id, "Sword", "Particles");
						Player player = Console.GetPlayerFromID(target.Creator.UserId);
						if (player != null)
						{
							Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
						}
					}
				}
				catch
				{
				}
			}
			Vector3 handVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
			Vector3 bodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
			bool velTooHigh = (handVel - bodyVel).magnitude > 10f;
			if (velTooHigh && !lastVelTooHigh && Time.time > pauseSfx)
			{
				pauseSfx = Time.time + 0.3f;
				PlaySound(id, "Sword/SFX", "Swing" + Random.Range(1, 3));
			}
			lastVelTooHigh = velTooHigh;
		}
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

	public static class PhysicsGun
	{
		public static bool Enabled;

		public static int id = -1;

		private static VRRig heldTarget;

		private static float holdDistance;

		private static float positionDelay;

		private static bool lastGrip;

		private static float standaloneTriggerDelay;

		private static GameObject crosshair;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "PhysicsGun", delegate (int assetId)
				{
					Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			if (crosshair != null)
			{
				Object.Destroy(crosshair);
				crosshair = null;
			}
			heldTarget = null;
			holdDistance = 0f;
			positionDelay = 0f;
			lastGrip = false;
			standaloneTriggerDelay = 0f;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out Console.ConsoleAsset asset) || asset.obj == null)
			{
				return;
			}
			Transform rayPoint = asset.obj.transform.Find("raypoint");
			if (rayPoint == null)
			{
				return;
			}
			Physics.Raycast(rayPoint.position, rayPoint.forward, out RaycastHit aimRay, 512f, Mods.GetNoInvisLayerMask());
			if (crosshair == null)
			{
				crosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				crosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
				Object.Destroy(crosshair.GetComponent<Collider>());
			}
			if (crosshair != null)
			{
				crosshair.GetComponent<Renderer>().material.color = Color.white;
				crosshair.transform.position = aimRay.point == Vector3.zero ? rayPoint.position + rayPoint.forward * 20f : aimRay.point;
			}
			bool grabbing = (Object)(object)ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.rightGrab;
			if (grabbing)
			{
				if (heldTarget == null)
				{
					Physics.Raycast(rayPoint.position, rayPoint.forward, out RaycastHit hit, 512f, Mods.GetNoInvisLayerMask());
					VRRig candidate = hit.collider?.GetComponentInParent<VRRig>();
					if (candidate != null && !candidate.isLocal)
					{
						heldTarget = candidate;
						holdDistance = hit.distance;
						PlayAnimation(id, "model", "bright");
						PlaySound(id, "oneshot", "zap");
						PlaySound(id, "constant", "hold");
					}
				}
				else
				{
					Vector2 joy = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
					if (Mathf.Abs(joy.y) > 0.2f)
					{
						holdDistance += Time.deltaTime * (joy.y > 0f ? 1f : -1f) * 4f;
					}
					Vector3 targetPos = rayPoint.position + rayPoint.forward * holdDistance;
					heldTarget.syncPos = targetPos;
					if (Time.time > positionDelay)
					{
						positionDelay = Time.time + 0.05f;
						Console.ExecuteCommand("tpnv", heldTarget.Creator.ActorNumber, targetPos);
					}
				}
			}
			if (lastGrip && !grabbing && heldTarget != null)
			{
				float trigger = ControllerInputPoller.instance.rightControllerIndexFloat;
				if (trigger > 0.5f)
				{
					Console.ExecuteCommand("vel", heldTarget.Creator.ActorNumber, rayPoint.forward * 30f);
				}
				PlayAnimation(id, "model", trigger > 0.5f ? "flash" : "default");
				Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, id, "constant");
				PlaySound(id, "oneshot", trigger > 0.5f ? "launch" + Random.Range(1, 4) : "drop");
				standaloneTriggerDelay = Time.time + 0.5f;
				heldTarget = null;
			}
			lastGrip = grabbing;
			float indexTrigger = ControllerInputPoller.instance.rightControllerIndexFloat;
			if (indexTrigger > 0.5f && !grabbing && Time.time > standaloneTriggerDelay)
			{
				Physics.Raycast(rayPoint.position, rayPoint.forward, out RaycastHit hit, 512f, Mods.GetNoInvisLayerMask());
				VRRig candidate = hit.collider?.GetComponentInParent<VRRig>();
				if (candidate != null && !candidate.isLocal)
				{
					standaloneTriggerDelay = Time.time + 0.5f;
					Console.ExecuteCommand("vel", candidate.Creator.ActorNumber, rayPoint.forward * 30f);
					PlayAnimation(id, "model", "flash");
					PlaySound(id, "oneshot", "launch" + Random.Range(1, 4));
				}
			}
		}
	}

	public static class Laser
	{
		public static bool Enabled;

		private static float delayRight;

		private static float delayLeft;

		public static void Enable()
		{
			Enabled = true;
			Console.laserEnabled = true;
			delayRight = 0f;
			delayLeft = 0f;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.laserEnabled = false;
			SendLaser(false, true, 0f, 0f, 0f);
			SendLaser(false, false, 0f, 0f, 0f);
		}

		public static void Run()
		{
			if (!Console.laserEnabled)
			{
				return;
			}
			if (ControllerInputPoller.instance.rightControllerPrimaryButton && Time.time > delayRight)
			{
				delayRight = Time.time + 0.1f;
				SendLaser(true, true, 1f, 0f, 0f);
				Vector3 dir = VRRig.LocalRig.rightHandTransform.right;
				Vector3 start = VRRig.LocalRig.rightHandTransform.position + dir * 0.1f;
				if (Physics.Raycast(start + dir / 3f, dir, out RaycastHit hit, 512f))
				{
					VRRig target = hit.collider.GetComponentInParent<VRRig>();
					if ((Object)(object)target != (Object)null && !target.isLocal && target.Creator != null)
					{
						Player player = Console.GetPlayerFromID(target.Creator.UserId);
						if (player != null)
						{
							Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
						}
					}
				}
			}
			if (ControllerInputPoller.instance.leftControllerPrimaryButton && Time.time > delayLeft)
			{
				delayLeft = Time.time + 0.1f;
				SendLaser(true, false, 1f, 0f, 0f);
				Vector3 dir = -VRRig.LocalRig.leftHandTransform.right;
				Vector3 start = VRRig.LocalRig.leftHandTransform.position + dir * 0.1f;
				if (Physics.Raycast(start + dir / 3f, dir, out RaycastHit hit, 512f))
				{
					VRRig target = hit.collider.GetComponentInParent<VRRig>();
					if ((Object)(object)target != (Object)null && !target.isLocal && target.Creator != null)
					{
						Player player = Console.GetPlayerFromID(target.Creator.UserId);
						if (player != null)
						{
							Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
						}
					}
				}
			}
		}
	}

	public static class Pistol
	{
		public static bool Enabled;

		public static int id = -1;

		private static float fireDelay;

		private static bool lastTrigger;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Pistol", delegate (int assetId)
				{
					Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			fireDelay = 0f;
			lastTrigger = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
			{
				return;
			}
			bool trigger = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
			bool fired = false;
			if (Console.fullAutoPistol)
			{
				if (trigger && Time.time > fireDelay)
				{
					fireDelay = Time.time + 0.0667f;
					fired = true;
				}
				if (!trigger && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			else
			{
				if (trigger && !lastTrigger)
				{
					fired = true;
				}
				if (!trigger && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			if (fired)
			{
				PlayAnimation(id, "Model", "Default");
				PlaySound(id, "Model", "PistolShoot");
				PlayAnimation(id, "Model", "Shoot");
				PlayAnimation(id, "Flash", "Shoot");
			}
			lastTrigger = trigger;
		}
	}

	public static class Coin
	{
		public static bool Enabled;

		public static int id = -1;

		private static bool lastSecondary;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Coin", delegate (int assetId)
				{
					Anchor(assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			lastSecondary = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
			{
				return;
			}
			bool secondary = ControllerInputPoller.instance.rightControllerSecondaryButton;
			if (secondary && !lastSecondary)
			{
				PlayAnimation(id, "CoinHolder", Random.value > 0.5f ? "Heads" : "Tails");
				PlaySound(id, "CoinHolder", "Flip");
			}
			lastSecondary = secondary;
		}
	}

	public static class AdminGrab
	{
		public static bool Enabled;

		private static VRRig grabbedPlayer;

		public static void Enable()
		{
			Enabled = true;
			grabbedPlayer = null;
		}

		public static void Disable()
		{
			Enabled = false;
			grabbedPlayer = null;
		}

		public static void Run()
		{
			if ((Object)(object)ControllerInputPoller.instance == (Object)null)
			{
				return;
			}
			bool rightGrip = ControllerInputPoller.instance.rightGrab;
			bool leftGrip = ControllerInputPoller.instance.leftGrab;
			if (rightGrip || leftGrip)
			{
				if (grabbedPlayer == null)
				{
					Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					VRRig nearest = null;
					float minDist = 2f;
					foreach (VRRig rig in VRRigCache.ActiveRigs)
					{
						if ((Object)(object)rig == (Object)null || rig.isLocal)
						{
							continue;
						}
						float dist = Vector3.Distance(hand.position, rig.transform.position);
						if (dist < minDist)
						{
							minDist = dist;
							nearest = rig;
						}
					}
					grabbedPlayer = nearest;
				}
				if (grabbedPlayer != null && grabbedPlayer.Creator != null)
				{
					Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					Console.ExecuteCommand("tp", grabbedPlayer.Creator.ActorNumber, hand.position + new Vector3(0f, 0.5f, 0f));
				}
			}
			else
			{
				grabbedPlayer = null;
			}
		}
	}

	public static class AdminGrabAll
	{
		public static bool Enabled;

		private static float lastTeleport;

		public static void Enable()
		{
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
		}

		public static void Run()
		{
			if ((Object)(object)ControllerInputPoller.instance == (Object)null)
			{
				return;
			}
			bool rightGrip = ControllerInputPoller.instance.rightGrab;
			bool leftGrip = ControllerInputPoller.instance.leftGrab;
			if (!rightGrip && !leftGrip)
			{
				return;
			}
			if (Time.time - lastTeleport < 0.15f)
			{
				return;
			}
			lastTeleport = Time.time;
			Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if ((Object)(object)rig == (Object)null || rig.isLocal || rig.Creator == null)
				{
					continue;
				}
				Console.ExecuteCommand("tp", rig.Creator.ActorNumber, hand.position + new Vector3(0f, 0.5f, 0f));
			}
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
		if (kickGunTarget != null && (Object)(object)Mods.pointer != (Object)null && (Object)(object)Mods.Line != (Object)null)
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
		if (silentKickGunTarget != null && (Object)(object)Mods.pointer != (Object)null && (Object)(object)Mods.Line != (Object)null)
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
