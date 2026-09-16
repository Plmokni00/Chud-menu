using System.Collections;
using Chud.Backend;
using Chud.UI;
using UnityEngine;

internal class BtnCollider : MonoBehaviour
{
	public static int framePressCooldown;

	public string relatedText;

	private void OnTriggerEnter(Collider collider)
	{
		if (string.IsNullOrEmpty(relatedText)) return;
		if ((Object)(object)WristMenu.instance == (Object)null) return;
		if ((Object)(object)GorillaTagger.Instance == (Object)null) return;
		if (Time.frameCount >= framePressCooldown + WristMenu.ClickCooldown && collider != null && collider.name == "buttonPresser")
		{
			GorillaTagger.Instance.StartVibration(Mods.isRightHanded, 0.01f, 0.001f);
			if (WristMenu.animationsEnabled)
			{
				this.StartCoroutine(PressAni());
			}
			WristMenu.Toggle(relatedText);
			framePressCooldown = Time.frameCount;
		}
	}

	private IEnumerator PressAni()
	{
		Vector3 original = transform.localScale;
		Vector3 pressed = original * 0.85f;
		float dur = 0.05f;
		float elapsed = 0f;
		while (elapsed < dur)
		{
			transform.localScale = Vector3.Lerp(original, pressed, elapsed / dur);
			elapsed += Time.deltaTime;
			yield return null;
		}
		transform.localScale = pressed;
		elapsed = 0f;
		while (elapsed < dur)
		{
			transform.localScale = Vector3.Lerp(pressed, original, elapsed / dur);
			elapsed += Time.deltaTime;
			yield return null;
		}
		transform.localScale = original;
	}
}
