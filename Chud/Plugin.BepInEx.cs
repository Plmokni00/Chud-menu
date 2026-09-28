using BepInEx;
using static Chud.PluginInfo;
using UnityEngine;
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