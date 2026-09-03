using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	private void UpdateCustomBoardText()
	{
		for (int i = 0; i < BoardPaths.Length; i++)
		{
			if ((Object)(object)cachedBoardObjects[i] == (Object)null)
			{
				cachedBoardObjects[i] = GameObject.Find(BoardPaths[i]);
				if ((Object)(object)cachedBoardObjects[i] != (Object)null)
					cachedBoardTexts[i] = cachedBoardObjects[i].GetComponent<TextMeshPro>();
			}
			if ((Object)(object)cachedBoardTexts[i] != (Object)null)
			{
				if (string.IsNullOrEmpty(originalBoardTexts[i]))
					originalBoardTexts[i] = cachedBoardTexts[i].text;
				cachedBoardTexts[i].text = CustomBoardTexts[i];
			}
		}
	}

	public void RestoreOriginalBoardText()
	{
		for (int i = 0; i < BoardPaths.Length; i++)
		{
			if (string.IsNullOrEmpty(originalBoardTexts[i])) continue;
			if ((Object)(object)cachedBoardObjects[i] == (Object)null)
			{
				cachedBoardObjects[i] = GameObject.Find(BoardPaths[i]);
				if ((Object)(object)cachedBoardObjects[i] != (Object)null)
					cachedBoardTexts[i] = cachedBoardObjects[i].GetComponent<TextMeshPro>();
			}
			if ((Object)(object)cachedBoardTexts[i] != (Object)null)
				cachedBoardTexts[i].text = originalBoardTexts[i];
		}
	}
}