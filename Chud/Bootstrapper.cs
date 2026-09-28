using Console = Chud.Backend.Console;
using Chud.Backend;
using Chud.Diagnostics;
using Chud.Patches;
using Chud.Rendering;
using Chud.Runtime;
using ExitGames.Client.Photon;
using HarmonyLib;
using Object = UnityEngine.Object;
using Photon.Realtime;
using static Chud.PluginInfo;
using System.Reflection;
using System;
using UnityEngine;
namespace Chud
{
    public static class Bootstrapper
    {
        private static bool _patched;
        private static Harmony _harmony;

        public static bool Patched => _patched;

        public static void Patch()
        {
            if (_patched && _harmony != null)
            {
                return;
            }

            try
            {
                _harmony = new Harmony(GUID);
            }
            catch (Exception ex)
            {
                Log.Error("could not create the Harmony instance; no patches were applied", ex);
                return;
            }

            int failed = ApplyAttributePatches();
            ApplyRpcRateLimit();
            ReportReflectionAvailability();

            _patched = true;

            if (failed == 0)
            {
                Log.Info(AppliedPatchCount + " patch classes applied");
            }
            else
            {
                Log.Error(
                    AppliedPatchCount + " patch classes applied, " + failed +
                    " failed (see the errors above)");
            }
        }

        public static int FailedPatchCount { get; private set; }

        public static int AppliedPatchCount { get; private set; }

        private static int ApplyAttributePatches()
        {
            Type[] types;
            try
            {
                types = typeof(Bootstrapper).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = Array.FindAll(ex.Types, t => t != null);
            }

            int applied = 0;
            int failed = 0;

            foreach (Type type in types)
            {
                if (type == null || !HasPatchAttribute(type))
                {
                    continue;
                }

                try
                {
                    _harmony.PatchAll(type);
                    applied++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Log.Error("patch class '" + type.FullName + "' failed and was skipped", ex);
                }
            }

            FailedPatchCount = failed;
            AppliedPatchCount = applied;
            return failed;
        }

        private static bool HasPatchAttribute(Type type)
        {
            object[] attributes;
            try
            {
                attributes = type.GetCustomAttributes(typeof(HarmonyPatch), false);
            }
            catch (Exception)
            {
                return false;
            }

            return attributes.Length > 0;
        }

        private static void ApplyRpcRateLimit()
        {
            Log.Guard("Bootstrapper.ApplyRpcRateLimit", () =>
            {
                MethodInfo target = typeof(LoadBalancingClient).GetMethod(
                    "OpRaiseEvent",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) },
                    null);

                MethodInfo prefix = typeof(RpcRateLimiter).GetMethod(
                    "Prefix", BindingFlags.Static | BindingFlags.Public);

                if (target == null)
                {
                    Log.Warn("LoadBalancingClient.OpRaiseEvent not found; RPC rate limiting is inactive");
                    return;
                }

                if (prefix == null)
                {
                    Log.Error("RpcRateLimiter.Prefix not found; RPC rate limiting is inactive");
                    return;
                }

                _harmony.Patch(target, new HarmonyMethod(prefix));
            });
        }

        private static void ReportReflectionAvailability()
        {
            Log.Guard("Bootstrapper.ReportReflectionAvailability", GameReflection.ReportAvailability);
        }

        public static void Initialize()
        {
            Log.Guard("Bootstrapper.Initialize", () =>
            {
                if (GameObject.Find(InitObjectName) != (Object)null)
                {
                    return;
                }

                var host = new GameObject(InitObjectName);

                if (!AddComponent<Chud.UI.WristMenu>(host))
                {
                    Object.Destroy(host);
                    return;
                }

                AddComponent<Mods>(host);
                AddComponent<NetworkManager>(host);
                AddComponent<GTAG_NotificationLib.NotifiLib>(host);
                AddComponent<CustomPropSetter>(host);
                AddComponent<Console>(host);

                Object.DontDestroyOnLoad(host);
            });
        }

        private const string InitObjectName = "Chud_Init";

        private static bool AddComponent<T>(GameObject host) where T : Component
        {
            if (host.GetComponent<T>() != null)
            {
                return true;
            }

            return Log.Guard("Bootstrapper.AddComponent<" + typeof(T).Name + ">",
                () => host.AddComponent<T>() != null, false);
        }
    }
}