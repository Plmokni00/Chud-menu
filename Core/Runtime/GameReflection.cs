using Chud.Diagnostics;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine;
namespace Chud.Runtime
{
    public static class GameReflection
    {
        private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AllStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static FieldInfo _vrrigFps;
        private static bool _vrrigFpsResolved;

        private static FieldInfo _gtPlayerLastHitInfoHand;
        private static bool _gtPlayerLastHitInfoHandResolved;

        private static FieldInfo _playerOwnedCosmetics;
        private static bool _playerOwnedCosmeticsResolved;

        private static MethodInfo _addCosmetic;
        private static ParameterInfo[] _addCosmeticParameters;
        private static bool _addCosmeticResolved;

        private static Type _subscriptionManager;
        private static bool _subscriptionManagerResolved;

        private static MethodInfo _isLocalSubscribed;
        private static bool _isLocalSubscribedResolved;

        public static void ReportAvailability()
        {
            var results = new (string Name, bool Ok)[]
            {
                ("VRRig.fps", VrrigFps() != null),
                ("GTPlayer.lastHitInfoHand", GtPlayerLastHitInfoHand() != null),
                ("VRRig._playerOwnedCosmetics", PlayerOwnedCosmetics() != null),
                ("VRRig.AddCosmetic", AddCosmeticMethod() != null),
                ("SubscriptionManager.IsLocalSubscribed", IsLocalSubscribedMethod() != null)
            };

            foreach ((string name, bool ok) in results)
            {
                if (ok)
                {
                    continue;
                }

                Log.Warn(
                    "private game member '" + name + "' is unavailable; features that depend on it " +
                    "will be disabled (the game may have been updated)");            }
        }

        public static FieldInfo VrrigFps()
        {
            if (!_vrrigFpsResolved)
            {
                _vrrigFpsResolved = true;
                _vrrigFps = AccessTools.Field(typeof(VRRig), "fps");
            }

            return _vrrigFps;
        }

        public static int ReadLocalFps()
        {
            FieldInfo field = VrrigFps();
            VRRig rig = GameContext.LocalRig;
            if (field == null || rig == null)
            {
                return -1;
            }

            return Log.Guard("GameReflection.ReadLocalFps", () =>
            {
                object value = field.GetValue(rig);
                return value is int fps ? fps : -1;
            }, -1);
        }

        public static FieldInfo GtPlayerLastHitInfoHand()
        {
            if (!_gtPlayerLastHitInfoHandResolved)
            {
                _gtPlayerLastHitInfoHandResolved = true;
                _gtPlayerLastHitInfoHand =
                    AccessTools.Field(typeof(GorillaLocomotion.GTPlayer), "lastHitInfoHand");
            }

            return _gtPlayerLastHitInfoHand;
        }

        public static FieldInfo PlayerOwnedCosmetics()
        {
            if (!_playerOwnedCosmeticsResolved)
            {
                _playerOwnedCosmeticsResolved = true;
                _playerOwnedCosmetics = AccessTools.Field(typeof(VRRig), "_playerOwnedCosmetics");
            }

            return _playerOwnedCosmetics;
        }

        public static MethodInfo AddCosmeticMethod()
        {
            if (!_addCosmeticResolved)
            {
                _addCosmeticResolved = true;
                _addCosmetic = AccessTools.Method(typeof(VRRig), "AddCosmetic");
                _addCosmeticParameters = _addCosmetic?.GetParameters();
            }

            return _addCosmetic;
        }

        public static bool TryAddCosmetic(VRRig rig, string itemName)
        {
            if (rig == null || string.IsNullOrEmpty(itemName))
            {
                return false;
            }

            MethodInfo method = AddCosmeticMethod();
            ParameterInfo[] parameters = _addCosmeticParameters;
            if (method == null || parameters == null || parameters.Length == 0)
            {
                Log.Warn("VRRig.AddCosmetic unavailable; cannot grant cosmetic '" + itemName + "'");
                return false;
            }

            return Log.Guard("GameReflection.TryAddCosmetic", () =>
            {
                object[] args = new object[parameters.Length];
                args[0] = itemName;
                for (int i = 1; i < args.Length; i++)
                {
                    args[i] = Type.Missing;
                }

                method.Invoke(rig, args);
                return true;
            }, false);
        }

        public static Type SubscriptionManagerType()
        {
            if (_subscriptionManagerResolved)
            {
                return _subscriptionManager;
            }

            _subscriptionManagerResolved = true;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    Type found = assemblies[i].GetType("GorillaTagScripts.SubscriptionManager", false);
                    if (found != null)
                    {
                        _subscriptionManager = found;
                        return _subscriptionManager;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("assembly scan failed for SubscriptionManager: " + assemblies[i].FullName, ex);
                }
            }

            return null;
        }

        public static MethodInfo IsLocalSubscribedMethod()
        {
            if (_isLocalSubscribedResolved)
            {
                return _isLocalSubscribed;
            }

            _isLocalSubscribedResolved = true;
            Type type = SubscriptionManagerType();
            if (type != null)
            {
                _isLocalSubscribed = type.GetMethod("IsLocalSubscribed", AllStatic);
            }

            return _isLocalSubscribed;
        }

        public static Type FindType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    Type found = assemblies[i].GetType(fullName, false);
                    if (found != null)
                    {
                        return found;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("type lookup failed for " + fullName, ex);
                }
            }

            return null;
        }

        public static bool TrySetField(object target, FieldInfo field, object value)
        {
            if (target == null || field == null)
            {
                return false;
            }

            return Log.Guard("GameReflection.TrySetField(" + field.Name + ")", () =>
            {
                field.SetValue(target, value);
                return true;
            }, false);
        }

        public static T TryGetField<T>(object target, FieldInfo field, T fallback = default(T))
        {
            if (target == null || field == null)
            {
                return fallback;
            }

            return Log.Guard("GameReflection.TryGetField(" + field.Name + ")", () =>
            {
                object value = field.GetValue(target);
                return value is T typed ? typed : fallback;
            }, fallback);
        }
    }
}