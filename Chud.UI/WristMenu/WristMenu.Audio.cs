using System.Collections;
using Chud.Backend;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static readonly string[] ButtonClickUrls = new string[]
	{
		"https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click.mp3",
		"https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click%202.mp3",
		"https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click%203.mp3",
		"https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/Button%20click%204.mp3",
		"https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/Button%20click%205.mp3"
	};

	public static readonly string[] ButtonClickNames = new string[]
	{
		"Default button click",
		"Clicker trainer",
		"DDLC",
		"Minecraft Lever",
		"Skype"
	};

	public static AudioClip[] buttonClickClips = new AudioClip[5];

	public static int buttonClickIndex = 0;

	private static bool buttonClickLoading = false;

	public static void ApplyButtonClickSound(int index)
	{
		if (index < 0 || index >= ButtonClickUrls.Length) return;
		buttonClickIndex = index;
		EnsureButtonClickLoaded();
		if (buttonClickClips[index] != null)
		{
			customButtonClick = buttonClickClips[index];
		}
	}

	public static void EnsureButtonClickLoaded()
	{
		if (buttonClickLoading || instance == null) return;
		buttonClickLoading = true;
		instance.StartCoroutine(LoadCustomButtonClickAudio());
	}

	public static IEnumerator LoadCustomButtonClickAudio()
	{
		for (int i = 0; i < ButtonClickUrls.Length; i++)
		{
			int slot = i;
			yield return SoundCache.GetClip(ButtonClickUrls[slot], c => buttonClickClips[slot] = c);
		}
		if (buttonClickIndex < 0 || buttonClickIndex >= buttonClickClips.Length)
			buttonClickIndex = 0;
		if (buttonClickClips[buttonClickIndex] != null)
		{
			customButtonClick = buttonClickClips[buttonClickIndex];
		}
		else if (buttonClickClips[0] != null)
		{
			buttonClickIndex = 0;
			customButtonClick = buttonClickClips[0];
		}
		buttonClickLoading = false;
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
			return;
		}
		try
		{
			if (VRRig.LocalRig != null)
				VRRig.LocalRig.PlayHandTapLocal(Mods.ButtonSound, rightHand, 0.5f);
		}
		catch { }
		EnsureButtonClickLoaded();
	}
}
