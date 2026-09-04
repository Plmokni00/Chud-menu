using Chud.Backend;
using Chud.UI;
using ExitGames.Client.Photon;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;
using static Chud.PluginInfo;

namespace Chud;

public static class Bootstrapper
{
	private static bool patched;

	private static Harmony _harmony;
	public static void Patch()
	{
		if (patched && _harmony != null)
			return;
		try
		{
			Harmony val = new Harmony(GUID);
			_harmony = val;
			val.PatchAll();
			MethodInfo opRaiseEvent = typeof(LoadBalancingClient).GetMethod("OpRaiseEvent", BindingFlags.Public | BindingFlags.Instance, null,
				new[] { typeof(byte), typeof(object), typeof(RaiseEventOptions), typeof(SendOptions) }, null);
			MethodInfo prefix = typeof(RPCProtection).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
			if (opRaiseEvent != null && prefix != null)
				val.Patch(opRaiseEvent, new HarmonyMethod(prefix));
			patched = true;
		}
		catch (Exception e) { UnityEngine.Debug.LogError("[Chud] Bootstrapper Patch failed: " + e); }
	}

	public static void Initialize()
	{
		if ((UnityEngine.Object)(object)GameObject.Find("Chud_Init") != (UnityEngine.Object)null)
			return;
		GameObject go = new GameObject("Chud_Init");
		go.AddComponent<WristMenu>();
		go.AddComponent<Chud.Backend.Mods>();
		go.AddComponent<NetworkManager>();
		go.AddComponent<NotifiLib>();
		go.AddComponent<CustomPropSetter>();
		go.AddComponent<Chud.Backend.Console>();
		UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)go);
	}
}
