using BepInEx;
using UnityEngine;
using static Chud.PluginInfo;

namespace Chud;

[BepInPlugin(GUID, Name, Version)]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
		Bootstrapper.Patch();
	}

	private void Start()
	{
		Bootstrapper.Initialize();
	}
}
