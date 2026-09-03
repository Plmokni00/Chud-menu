using System;
using GorillaNetworking;
using HarmonyLib;
using PlayFab;
using PlayFab.CloudScriptModels;
using PlayFab.Json;

namespace Chud.Backend;

[HarmonyPatch(typeof(GorillaServer), "CheckForBadName")]
internal class AntiBanPlayfabPatch
{
	public static bool Prefix(CheckForBadNameRequest request, Action<ExecuteFunctionResult> successCallback, Action<PlayFabError> errorCallback)
	{
		if (!BanPatchState.enabled)
		{
			return true;
		}
		if (successCallback != null)
		{
			ExecuteFunctionResult val = new ExecuteFunctionResult();
			JsonObject val2 = new JsonObject();
			val2.Add("result", (object)0);
			val.FunctionResult = (object)val2;
			successCallback(val);
		}
		return false;
	}
}
