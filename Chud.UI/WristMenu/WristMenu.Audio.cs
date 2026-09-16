using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static IEnumerator LoadCustomButtonClickAudio()
	{
		if (customAudioLoaded)
		{
			yield break;
		}
		string url = "https://raw.githubusercontent.com/vhghfhnfgvbngv/Idfk-bro/main/button%20click.mp3";
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, (AudioType)13);
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result == 1)
			{
				customButtonClick = DownloadHandlerAudioClip.GetContent(req);
				customAudioLoaded = true;
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static void PlayButtonClickSound(bool rightHand)
	{
		if ((Object)(object)customButtonClick != (Object)null)
		{
			if ((Object)(object)buttonClickAudioSource == (Object)null)
			{
				GameObject val = new GameObject("ChudButtonAudio");
				buttonClickAudioSource = val.AddComponent<AudioSource>();
				buttonClickAudioSource.spatialBlend = 0f;
				buttonClickAudioSource.playOnAwake = false;
				Object.DontDestroyOnLoad((Object)(object)val);
			}
			buttonClickAudioSource.PlayOneShot(customButtonClick, 0.5f);
		}
	}
}