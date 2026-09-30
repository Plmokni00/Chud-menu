using Console = Chud.Backend.Console;
using Chud.Backend;
using Chud.Diagnostics;
using Chud.Runtime;
using GorillaLocomotion;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using System;
using UnityEngine;
namespace Chud.Patches
{
    public static class SlingshotAimbotPatch
    {
        public static bool Enabled;

        private const float RoughSpeed = 20f;
        private const float SpeedMultiplier = 2.5f;
        private const float MinSpeed = 5f;

        internal static bool TryCompute(Slingshot slingshot, out Vector3 velocity)
        {
            velocity = Vector3.zero;

            GorillaTagger tagger = GameContext.Tagger;
            if (tagger == null || tagger.headCollider == null || slingshot.center == null)
            {
                return false;
            }

            VRRig target = FindClosestPlayer(tagger);
            if (target == null || target.headMesh == null)
            {
                return false;
            }

            velocity = CalcVelocity(target, slingshot.center.transform.position);
            return true;
        }

        private static VRRig FindClosestPlayer(GorillaTagger tagger)
        {
            VRRig best = null;
            float bestScore = float.MaxValue;
            Transform head = tagger.headCollider.transform;

            foreach (VRRig rig in GameLists.ActiveRigs())
            {
                if (rig == null || rig.isLocal || rig.Creator == null)
                {
                    continue;
                }

                Vector3 toRig = (rig.transform.position - head.position).normalized;
                float distance = Vector3.Distance(head.position, rig.transform.position);
                float score = Vector3.Angle(head.forward, toRig) + distance * 0.1f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = rig;
                }
            }

            return best;
        }

        private static Vector3 CalcVelocity(VRRig target, Vector3 origin)
        {
            Vector3 targetPos = target.headMesh.transform.position;
            Vector3 targetVel = target.LatestVelocity();
            targetVel.y /= 3f;

            Vector3 displacement = targetPos - origin;
            float x = FlatMagnitude(displacement);
            float time = x / RoughSpeed;

            displacement = targetPos + targetVel * time - origin;
            x = FlatMagnitude(displacement);
            float y = displacement.y;

            float g = -Physics.gravity.y;
            float minSpeed = Mathf.Sqrt(g * (y + Mathf.Sqrt(x * x + y * y)));
            float launchSpeed = Mathf.Max(minSpeed * SpeedMultiplier, MinSpeed);

            return VelocityForAngle(displacement, launchSpeed);
        }

        private static float FlatMagnitude(Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z).magnitude;
        }

        private static Vector3 VelocityForAngle(Vector3 displacement, float speed)
        {
            Vector3 flat = new Vector3(displacement.x, 0f, displacement.z);
            float x = flat.magnitude;
            float y = displacement.y;
            float g = -Physics.gravity.y;
            float v2 = speed * speed;

            float underSqrt = v2 * v2 - g * (g * x * x + 2f * y * v2);
            if (underSqrt <= 0f)
            {
                return displacement.normalized * speed;
            }

            float root = Mathf.Sqrt(underSqrt);
            if (x < 0.001f)
            {
                return Vector3.up * speed;
            }

            float angle = Mathf.Atan((v2 - root) / (g * x));
            Vector3 dir = flat.normalized;

            return dir * Mathf.Cos(angle) * speed + Vector3.up * Mathf.Sin(angle) * speed;
        }
    }

    [HarmonyPatch(typeof(Slingshot), "GetLaunchVelocity")]
    internal static class GetLaunchPatch
    {
        private static void Postfix(Slingshot __instance, ref Vector3 __result)
        {
            if (!SlingshotAimbotPatch.Enabled)
            {
                return;
            }

            if (SlingshotAimbotPatch.TryCompute(__instance, out Vector3 velocity))
            {
                __result = velocity;
            }
        }
    }

    [HarmonyPatch(typeof(GTPlayer), "ApplyNativeScaleAdjustment")]
    internal static class ScaleSpeedPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref float __result, float adjustedMagnitude)
        {
            if (!ConsoleMods.ScaleSelf.Enabled)
            {
                return true;
            }

            __result = adjustedMagnitude;
            return false;
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
    internal static class RoomJoinPatch
    {
        private static void Prefix(Player newPlayer)
        {
            if (newPlayer == null)
            {
                return;
            }

            int count = PhotonNetwork.CurrentRoom == null ? 0 : PhotonNetwork.CurrentRoom.PlayerCount;
            NotifiLib.SendNotification(
                "<color=#88ff88>" + newPlayer.NickName + "</color> joined (<color=white>" + count + "</color> players)");

            Mods.ARSCheckPlayer(newPlayer);

            if (Console.AutoDetectConsoleUsers)
            {
                Console.ScheduleConsoleUserScan();
            }
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
    internal static class RoomLeavePatch
    {
        private static void Prefix(Player otherPlayer)
        {
            if (otherPlayer != null && otherPlayer != PhotonNetwork.LocalPlayer)
            {
                int count = PhotonNetwork.CurrentRoom == null ? 0 : PhotonNetwork.CurrentRoom.PlayerCount;
                NotifiLib.SendNotification(
                    "<color=#ff8888>" + otherPlayer.NickName + "</color> left (<color=white>" + count + "</color> players)");
            }

            if (Console.AutoDetectConsoleUsers)
            {
                Console.ScheduleConsoleUserScan();
            }
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnJoinedRoom")]
    internal static class RoomEnteredPatch
    {
        private static void Postfix()
        {
            int count = PhotonNetwork.CurrentRoom == null ? 0 : PhotonNetwork.CurrentRoom.PlayerCount;
            NotifiLib.SendNotification("You joined (<color=white>" + count + "</color> players)");
            Mods.ReapplyActiveMods();

            if (Console.AutoDetectConsoleUsers)
            {
                Console.ScheduleConsoleUserScan();
            }
        }
    }

    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnLeftRoom")]
    internal static class RoomLeftPatch
    {
        private static void Prefix()
        {
            Mods.DisableAntiReport();
        }
    }
}