using Chud.Backend;
using HarmonyLib;
using Object = UnityEngine.Object;
using UnityEngine;
namespace Chud.Patches
{
    public static class GhostRigNames
    {
        public const string Rig = "Chud_GhostRig";
        public const string Holder = "Chud_GhostRigHolder";

        public static bool IsGhost(VRRig rig)
        {
            if (rig == (Object)null || rig.gameObject == null)
            {
                return false;
            }

            string name = rig.gameObject.name;
            return name == Rig || name == Holder;
        }
    }

    [HarmonyPatch(typeof(VRRig), "OnDisable")]
    internal static class GhostRigOnDisablePatch
    {
        private static bool Prefix(VRRig __instance)
        {
            return !GhostRigNames.IsGhost(__instance);
        }
    }

    [HarmonyPatch(typeof(VRRig), "OnEnable")]
    internal static class GhostRigOnEnablePatch
    {
        private static bool Prefix(VRRig __instance)
        {
            return !GhostRigNames.IsGhost(__instance);
        }
    }

    [HarmonyPatch(typeof(VRRig), "Awake")]
    internal static class RigAwakePatch
    {
        private static bool Prefix(VRRig __instance)
        {
            if (__instance == (Object)null)
            {
                return true;
            }

            if (Mods.CloningGhostRig)
            {
                return false;
            }

            return !GhostRigNames.IsGhost(__instance);
        }
    }

    [HarmonyPatch(typeof(BodyDockPositions), "RefreshTransferrableItems")]
    internal static class GhostBodyDockPatch
    {
        private static bool Prefix(BodyDockPositions __instance)
        {
            if (__instance == null)
            {
                return true;
            }

            return !GhostRigNames.IsGhost(__instance.GetComponentInParent<VRRig>());
        }
    }

    [HarmonyPatch(typeof(VRRigCollection), "OnRigTriggerEnter")]
    internal static class GhostVRRigCollectionPatch
    {
        private static bool Prefix(Collider other)
        {
            if (other == null)
            {
                return true;
            }

            if (GhostRigNames.IsGhost(other.GetComponentInParent<VRRig>()))
            {
                return false;
            }

            RigContainer container = other.GetComponentInParent<RigContainer>();
            return !(container != null && GhostRigNames.IsGhost(container.Rig));
        }
    }

    [HarmonyPatch(typeof(VRRigCollection), "OnRigTriggerExit")]
    internal static class GhostVRRigCollectionExitPatch
    {
        private static bool Prefix(Collider other)
        {
            return other == null || !GhostRigNames.IsGhost(other.GetComponentInParent<VRRig>());
        }
    }

    [HarmonyPatch(typeof(PlayerColoredCosmetic), "Awake")]
    internal static class PlayerColoredCosmeticPatch
    {
        private static readonly System.Reflection.FieldInfo ColoringRulesField =
            HarmonyLib.AccessTools.Field(typeof(PlayerColoredCosmetic), "coloringRules");

        private static System.Reflection.FieldInfo RendererField;
        private static bool RendererFieldResolved;

        private static bool Prefix(PlayerColoredCosmetic __instance)
        {
            if (Mods.CloningGhostRig)
            {
                return false;
            }

            if (ColoringRulesField == null)
            {
                return true;
            }

            var rules = ColoringRulesField.GetValue(__instance) as System.Array;
            if (rules == null || rules.Length == 0)
            {
                return true;
            }

            if (!RendererFieldResolved)
            {
                RendererFieldResolved = true;
                System.Type element = rules.GetType().GetElementType();
                RendererField = element == null
                    ? null
                    : HarmonyLib.AccessTools.Field(element, "meshRenderer");
            }

            if (RendererField == null)
            {
                return true;
            }

            var valid = new System.Collections.Generic.List<object>(rules.Length);
            foreach (object rule in rules)
            {
                if (RendererField.GetValue(rule) != null)
                {
                    valid.Add(rule);
                }
            }

            if (valid.Count == rules.Length)
            {
                return true;
            }

            System.Type ruleType = rules.GetType().GetElementType();
            System.Array replacement = System.Array.CreateInstance(ruleType, valid.Count);
            for (int i = 0; i < valid.Count; i++)
            {
                replacement.SetValue(valid[i], i);
            }

            ColoringRulesField.SetValue(__instance, replacement);
            return true;
        }
    }
}