using System;
using System.Collections;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace Chud.Backend;

public static partial class ConsoleMods
{
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
				GameObject crosshair = GameObject.CreatePrimitive((PrimitiveType.Sphere));
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
				crosshair = GameObject.CreatePrimitive((PrimitiveType.Sphere));
				crosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
				Object.Destroy(crosshair.GetComponent<Collider>());
			}
			if (crosshair != null)
			{
				crosshair.GetComponent<Renderer>().material.color = Color.white;
				crosshair.transform.position = aimRay.point == Vector3.zero ? rayPoint.position + rayPoint.forward * 20f : aimRay.point;
			}
			bool grabbing = ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.rightGrab;
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
					if (target != (Object)null && !target.isLocal && target.Creator != null)
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
					if (target != (Object)null && !target.isLocal && target.Creator != null)
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
			if (ControllerInputPoller.instance == (Object)null)
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
						if (rig == (Object)null || rig.isLocal)
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
			if (ControllerInputPoller.instance == (Object)null)
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
				if (rig == (Object)null || rig.isLocal || rig.Creator == null)
				{
					continue;
				}
				Console.ExecuteCommand("tp", rig.Creator.ActorNumber, hand.position + new Vector3(0f, 0.5f, 0f));
			}
		}
	}
}
