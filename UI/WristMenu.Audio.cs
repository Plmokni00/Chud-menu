using System.Collections;
using Chud.Backend;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI
{
    internal static class Audio
    {
        public static readonly string[] ButtonClickUrls =
        {
            "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click.mp3",
            "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click%202.mp3",
            "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/button%20click%203.mp3",
            "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/Button%20click%204.mp3",
            "https://raw.githubusercontent.com/Plmokni00/Chud-menu-files/main/Button%20click%205.mp3"
        };

        public static readonly string[] ButtonClickNames =
        {
            "Default button click",
            "Clicker trainer",
            "DDLC",
            "Minecraft Lever",
            "Skype"
        };

        public const float Volume = 0.5f;

        public static AudioClip[] Clips = new AudioClip[ButtonClickUrls.Length];

        public static int Index;

        private static bool _loading;
        private static bool _loadRequested;
        private static AudioSource _source;

        public static void Apply(int index)
        {
            if (index < 0 || index >= ButtonClickUrls.Length)
            {
                return;
            }

            Index = index;
            RequestLoad();
            Adopt();
        }

        private static void RequestLoad()
        {
            if (_loadRequested || _loading || WristMenu.Instance == null)
            {
                return;
            }

            _loadRequested = true;
            WristMenu.Instance.StartCoroutine(LoadButtonClickSounds());
        }

        public static IEnumerator LoadButtonClickSounds()
        {
            _loading = true;

            for (int i = 0; i < ButtonClickUrls.Length; i++)
            {
                int slot = i;
                yield return SoundCache.GetClip(ButtonClickUrls[slot], clip => Clips[slot] = clip);
            }

            Adopt();
            _loading = false;
        }

        private static void Adopt()
        {
            if (Index < 0 || Index >= Clips.Length)
            {
                Index = 0;
            }

            AudioClip chosen = Clips[Index];

            if (chosen == null)
            {
                chosen = Clips[0];
                if (chosen != null)
                {
                    Index = 0;
                }
            }

            WristMenu.CustomButtonClick = chosen;
        }

        public static void PlayButtonClick(bool rightHand)
        {
            AudioClip clip = WristMenu.CustomButtonClick;

            if (clip == null)
            {
                RequestLoad();
                return;
            }

            if (_source == (Object)null)
            {
                var go = new GameObject("ChudButtonAudio");
                _source = go.AddComponent<AudioSource>();
                _source.spatialBlend = 0f;
                _source.playOnAwake = false;
                Object.DontDestroyOnLoad(go);
            }

            _source.PlayOneShot(clip, Volume);
        }
    }
}
