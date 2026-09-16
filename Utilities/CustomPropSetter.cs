using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Chud.Backend;

internal class CustomPropSetter : MonoBehaviour
{
	private IEnumerator Start()
	{
		while (PhotonNetwork.LocalPlayer == null || !PhotonNetwork.IsConnectedAndReady)
			yield return null;
		ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
		val[(object)"Chud menu"] = true;
		PhotonNetwork.LocalPlayer.SetCustomProperties(val, (ExitGames.Client.Photon.Hashtable)null, (WebFlags)null);
	}
}
