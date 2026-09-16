using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Chud.Backend;

[HarmonyPatch(typeof(PlayerColoredCosmetic), "Awake")]
internal static class PlayerColoredCosmeticPatch
{
	public static bool Prefix(PlayerColoredCosmetic __instance)
	{
		if (Mods.cloningGhostRig)
			return false;

		FieldInfo rulesField = AccessTools.Field(typeof(PlayerColoredCosmetic), "coloringRules");
		if (rulesField == null)
			return true;

		Array rules = rulesField.GetValue(__instance) as Array;
		if (rules == null || rules.Length == 0)
			return true;

		FieldInfo rendererField = AccessTools.Field(rules.GetType().GetElementType(), "meshRenderer");
		if (rendererField == null)
			return true;

		List<object> valid = new List<object>(rules.Length);
		foreach (object rule in rules)
		{
			if (rendererField.GetValue(rule) != null)
				valid.Add(rule);
		}

		if (valid.Count == rules.Length)
			return true;

		Array validArray = Array.CreateInstance(rules.GetType().GetElementType(), valid.Count);
		for (int i = 0; i < valid.Count; i++)
		{
			validArray.SetValue(valid[i], i);
		}
		rulesField.SetValue(__instance, validArray);
		return true;
	}
}