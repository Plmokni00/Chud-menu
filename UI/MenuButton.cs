using Chud.Backend;
using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using UnityEngine;
namespace Chud.UI
{
    public class MenuButton : MonoBehaviour
    {
        public static int FramePressCooldown;

        public string ButtonId;

        public string DisplayText;

        private bool _pressing;

        private void OnTriggerEnter(Collider collider)
        {
            if (string.IsNullOrEmpty(ButtonId)) return;
            if (WristMenu.Instance == (Object)null) return;
            if (GorillaTagger.Instance == (Object)null) return;
            if (collider == null || collider.name != "buttonPresser") return;
            if (Time.frameCount < FramePressCooldown + WristMenu.ClickCooldown) return;

            FramePressCooldown = Time.frameCount;

            try
            {
                GorillaTagger.Instance.StartVibration(Mods.IsRightHanded, 0.01f, 0.001f);

                if (WristMenu.AnimationsEnabled)
                {
                    _pressing = true;
                    StartCoroutine(PressAnimation());
                }

                WristMenu.Press(ButtonId);
            }
            catch (Exception ex)
            {
                Log.Error("button press '" + ButtonId + "' failed", ex);
            }
        }

        private IEnumerator PressAnimation()
        {
            Vector3 original = transform.localScale;
            Vector3 pressed = original * 0.85f;
            const float duration = 0.05f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!_pressing)
                {
                    yield break;
                }

                transform.localScale = Vector3.Lerp(original, pressed, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = pressed;
            elapsed = 0f;

            while (elapsed < duration)
            {
                if (!_pressing)
                {
                    yield break;
                }

                transform.localScale = Vector3.Lerp(pressed, original, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _pressing = false;
            transform.localScale = original;
        }

        private void OnDisable()
        {
            _pressing = false;
        }
    }
}