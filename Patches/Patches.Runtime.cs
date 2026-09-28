using Chud.Backend;
using Chud.Runtime;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Object = UnityEngine.Object;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
namespace Chud.Patches
{
    [HarmonyPatch(typeof(VRRig), "PostTick")]
    public static class PostTickPatch
    {
        private static bool Prefix(VRRig __instance)
        {
            if (!__instance.isLocal)
            {
                return true;
            }

            return !Mods.LocalRigOverrideActive;
        }
    }

    [HarmonyPatch(typeof(VRRig), "PostTick")]
    internal static class RigVisualPostTickPatch
    {
        private static void Postfix(VRRig __instance)
        {
            if (__instance == null || !__instance.isLocal)
            {
                return;
            }

            if (Chud.Runtime.GameContext.Poller == null)
            {
                return;
            }

            if (Mods.LocalRigOverrideActive)
            {
                return;
            }

            Mods.ApplyRigVisuals();
        }
    }

    [HarmonyPatch(typeof(GTPlayer), "ApplyClampedKnockback")]
    internal static class GuardianClampedKnockbackPatch
    {
        private static bool Prefix() => !GuardianGuard.ClampedKnockback;
    }

    [HarmonyPatch(typeof(GTPlayer), "ApplyKnockback")]
    internal static class GuardianKnockbackPatch
    {
        private static bool Prefix() => !GuardianGuard.KnockedBack;
    }

    [HarmonyPatch(typeof(GTPlayer), "DoLaunch")]
    internal static class GuardianLaunchPatch
    {
        private static bool Prefix() => !GuardianGuard.Launched;
    }

    [HarmonyPatch(typeof(VRRig), "ApplyLocalTrajectoryOverride")]
    internal static class GuardianTrajectoryPatch
    {
        private static bool Prefix() => !GuardianGuard.TrajectoryOverridden;
    }

    [HarmonyPatch(typeof(VRRig), "GrabbedByPlayer")]
    internal static class GuardianGrabbedByPatch
    {
        private static bool Prefix() => !GuardianGuard.GrabbedBy;
    }

    public static class GuardianGuard
    {
        public static bool ClampedKnockback;
        public static bool KnockedBack;
        public static bool Launched;
        public static bool TrajectoryOverridden;
        public static bool GrabbedBy;

        public static void EnableAll()
        {
            ClampedKnockback = true;
            KnockedBack = true;
            Launched = true;
            TrajectoryOverridden = true;
            GrabbedBy = true;
        }

        public static void DisableAll()
        {
            ClampedKnockback = false;
            KnockedBack = false;
            Launched = false;
            TrajectoryOverridden = false;
            GrabbedBy = false;
        }
    }

    [HarmonyPatch(typeof(GorillaQuitBox), "OnBoxTriggered")]
    internal static class QuitBoxPatch
    {
        public static bool Enabled = true;

        private static bool Prefix() => Enabled;
    }

    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger), "OnBoxTriggered")]
    internal static class NetworkTriggerPatch
    {
        public static bool Enabled;

        private static bool Prefix() => !Enabled;
    }

    [HarmonyPatch(typeof(VRRig), "PlayHandTapLocal")]
    internal static class JmanSoundPatch
    {
        public static bool Enabled;

        private const int BlockedLow = 336;
        private const int BlockedHigh = 338;

        private static bool Prefix(VRRig __instance, int audioClipIndex)
        {
            if (!Enabled)
            {
                return true;
            }

            if (__instance == (Object)VRRig.LocalRig)
            {
                return true;
            }

            return audioClipIndex < BlockedLow || audioClipIndex > BlockedHigh;
        }
    }

    [HarmonyPatch(typeof(VRRig), "IsItemAllowed")]
    internal static class UnlockAllCosmeticsPatch
    {
        public static bool Enabled;

        private static void Postfix(ref bool __result)
        {
            if (Enabled)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(VRRig), "PackCompetitiveData")]
    internal static class FPSSpoofPatch
    {
        private static void Postfix(VRRig __instance, ref short __result)
        {
            if (!Mods.FpsSpoofActive || !__instance.isLocal)
            {
                return;
            }

            __result = (short)((__result & 0xFF00) | (Mods.FpsSpoofValue & 0xFF));
        }
    }

    [HarmonyPatch(typeof(GTPlayerStats), "GetPackedValues")]
    internal static class FPSSpoofStatsPatch
    {
        private const int FpsShift = 16;

        private static void Postfix(ref long __result)
        {
            if (!Mods.FpsSpoofActive)
            {
                return;
            }

            long mask = ~(0xFFFFL << FpsShift);
            __result = (__result & mask) | ((long)(Mods.FpsSpoofValue & 0xFFFF) << FpsShift);
        }
    }

    [HarmonyPatch(typeof(VRRig), "InitializeNoobMaterial")]
    internal static class NoobMaterialPatch
    {
        private static bool Prefix(VRRig __instance, float red, float green, float blue, PhotonMessageInfoWrapped info)
        {
            NetworkSystem network = Chud.Runtime.GameContext.Network;
            if (network == null || network.LocalPlayer == null)
            {
                return true;
            }

            if (info.senderID != network.LocalPlayer.ActorNumber)
            {
                return true;
            }

            if (float.IsNaN(red) || float.IsNaN(green) || float.IsNaN(blue))
            {
                return false;
            }

            red = Mathf.Clamp01(red);
            green = Mathf.Clamp01(green);
            blue = Mathf.Clamp01(blue);
            __instance.InitializeNoobMaterialLocal(red, green, blue);
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaGameManager), "OnPlayerPropertiesUpdate")]
    internal static class ModFixNameSync
    {
        private static void Postfix(Player targetPlayer)
        {
            if (targetPlayer == null)
            {
                return;
            }

            int actorNumber = targetPlayer.ActorNumber;

            foreach (RigContainer container in VRRigCache.ActiveRigContainers)
            {
                if (container == null)
                {
                    continue;
                }

                NetPlayer creator = container.Creator;
                VRRig rig = container.Rig;
                if (creator != null && creator.ActorNumber == actorNumber && rig != null)
                {
                    rig.UpdateName();
                }
            }
        }
    }
}