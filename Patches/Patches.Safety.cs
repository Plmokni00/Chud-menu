using Chud.Backend;
using Chud.Diagnostics;
using Chud.Runtime;
using GorillaNetworking;
using HarmonyLib;
using PlayFab.CloudScriptModels;
using PlayFab.Json;
using PlayFab;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine.Networking;
using UnityEngine;
namespace Chud.Patches
{
    [HarmonyPatch(typeof(GorillaComputer), "CheckAutoBanListForName")]
    internal static class AntiBanListPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!BanPatchState.Enabled)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaServer), "CheckForBadName")]
    internal static class AntiBanPlayfabPatch
    {
        private static bool Prefix(Action<ExecuteFunctionResult> successCallback)
        {
            if (!BanPatchState.Enabled)
            {
                return true;
            }

            if (successCallback == null)
            {
                return false;
            }

            try
            {
                var payload = new JsonObject();
                payload.Add("result", 0);
                successCallback(new ExecuteFunctionResult { FunctionResult = payload });
            }
            catch (Exception ex)
            {
                Log.Warn("name-ban bypass callback failed", ex);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(VRRig), "SetNameTagText")]
    internal static class UnsanitizedChestPatch
    {
        private const int MaxNameLength = 32;

        private static void Postfix(VRRig __instance)
        {
            if (__instance == null || __instance.playerText1 == null)
            {
                return;
            }

            NetworkSystem network = GameContext.Network;
            if (network == null)
            {
                return;
            }

            try
            {
                NetPlayer netPlayer = __instance.Creator;
                string rawName = netPlayer != null ? netPlayer.NickName : network.GetMyNickName();
                if (!string.IsNullOrEmpty(rawName))
                {
                    __instance.playerText1.text = Sanitize(rawName);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("name tag sanitisation failed", ex);
            }
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            string result = value.Length > MaxNameLength ? value.Substring(0, MaxNameLength) : value;
            return result.Replace("<size", "<SIZE").Replace("size=", "SIZE=");
        }
    }

    [HarmonyPatch(typeof(VRRig), "SerializeReadShared")]
    internal static class ChestColorPatch
    {
        private static void Postfix(VRRig __instance)
        {
            RigColorUtil.ApplyTo(__instance);
        }
    }

    [HarmonyPatch(typeof(VRRig), "OnSubscriptionData")]
    internal static class ChestColorSubPatch
    {
        private static void Postfix(VRRig __instance)
        {
            RigColorUtil.ApplyTo(__instance);
        }
    }

    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), "UpdatePlayerText")]
    internal static class UnsanitizedBoardPatch
    {
        private static bool Prefix(GorillaPlayerScoreboardLine __instance)
        {
            if (__instance == null)
            {
                return false;
            }

            if (__instance.playerName == null)
            {
                return false;
            }

            return __instance.linePlayer != null;
        }
    }

    public static class RigColorUtil
    {
        private const float NearBlack = 4f / 255f;

        public static Color PlayerColor(VRRig rig)
        {
            if (rig == null)
            {
                return Color.white;
            }

            Color c = rig.playerColor;
            if (c.r < NearBlack && c.g < NearBlack && c.b < NearBlack)
            {
                return Color.white;
            }

            return c;
        }

        public static void ApplyTo(VRRig rig)
        {
            if (rig == null || rig.playerText1 == null)
            {
                return;
            }

            try
            {
                rig.playerText1.color = PlayerColor(rig);
            }
            catch (Exception ex)
            {
                Log.Warn("rig name tag colour failed", ex);
            }
        }
    }
}