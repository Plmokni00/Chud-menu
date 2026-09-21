using System.Collections.Generic;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

namespace Chud.UI;

internal partial class WristMenu
{
	public void Draw()
	{
		if (MenuManager.Instance.CurrentCategoryName == "Enabled Mods")
		{
			RebuildEnabledMods();
		}
		pageSize = 7;
		menu = new GameObject();
		menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH * 0.95625f);
		menuObj = MakeCylinder();
		menuObj.transform.parent = menu.transform;
		menuObj.transform.rotation = Quaternion.identity;
		menuObj.transform.localScale = new Vector3(0.1f, 1f, 1f);
		Renderer bgRenderer = menuObj.GetComponent<Renderer>();
		Color bgTop = NormalColor * 0.35f;
		Color bgBot = NormalColor;
		bgRenderer.material = MakeGradientMat(bgTop, bgBot);
		menuObj.transform.position = new Vector3(0.05f, 0f, 0f);
		RoundGameObject(menuObj, "__background__", bgTop, bgBot);
		canvasObj = new GameObject();
		canvasObj.transform.parent = menu.transform;
		Canvas canvas = canvasObj.AddComponent<Canvas>();
		CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
		canvasObj.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.WorldSpace;
		scaler.dynamicPixelsPerUnit = 1900f;
		scaler.referencePixelsPerUnit = 100f;
		GameObject titleObj = new GameObject();
		titleObj.transform.parent = canvasObj.transform;
		Text titleText = titleObj.AddComponent<Text>();
		titleText.font = MenuFont;
		titleText.text = MenuTitle;
		titleText.fontSize = 200;
		((Graphic)titleText).color = MenuTitleColor;
		titleText.fontStyle = FontStyle.Bold;
		titleText.alignment = TextAnchor.MiddleCenter;
		titleText.resizeTextForBestFit = true;
		titleText.resizeTextMinSize = 0;
		titleText.resizeTextMaxSize = 200;
		RectTransform titleRect = ((Component)titleText).GetComponent<RectTransform>();
		((Transform)titleRect).localPosition = Vector3.zero;
		titleRect.sizeDelta = new Vector2(0.28f, 0.05f);
		((Transform)titleRect).position = new Vector3(0.06f, 0f, 0.175f);
		((Transform)titleRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject bottomBarObj = new GameObject();
		bottomBarObj.transform.parent = canvasObj.transform;
		fpsText = bottomBarObj.AddComponent<Text>();
		fpsText.font = MenuFont;
		fpsText.text = bottomBarStr;
		fpsText.fontSize = 200;
		((Graphic)fpsText).color = ToolTipColor;
		fpsText.fontStyle = FontStyle.Bold;
		fpsText.alignment = TextAnchor.MiddleCenter;
		fpsText.resizeTextForBestFit = true;
		fpsText.resizeTextMinSize = 0;
		fpsText.resizeTextMaxSize = 200;
		RectTransform bottomBarRect = ((Component)fpsText).GetComponent<RectTransform>();
		((Transform)bottomBarRect).localPosition = Vector3.zero;
		bottomBarRect.sizeDelta = new Vector2(0.28f, 0.02f);
		((Transform)bottomBarRect).position = new Vector3(0.06f, 0f, 0.135f);
		((Transform)bottomBarRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		List<ButtonInfo> currentButtons = MenuManager.Instance.CurrentButtons;
		GameObject disconnectBtn = MakeCylinderButton();
		disconnectBtn.transform.parent = menu.transform;
		disconnectBtn.transform.rotation = Quaternion.identity;
		disconnectBtn.transform.localScale = new Vector3(BUTTON_CYLINDER_SCALE_X, BUTTON_CYLINDER_SCALE_Y, BUTTON_CYLINDER_SCALE_Z);
		disconnectBtn.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
		Color dcTop = DisconnectButtonColor * 0.35f;
		Color dcBot = DisconnectButtonColor;
		disconnectBtn.GetComponent<Renderer>().material = MakeGradientMat(dcTop, dcBot);
		BtnCollider disconnectCollider = disconnectBtn.AddComponent<BtnCollider>();
		disconnectCollider.buttonId = "DisconnectingButton";
		disconnectCollider.displayText = "Disconnect";
		RoundGameObject(disconnectBtn, "DisconnectingButton", dcTop, dcBot);
		GameObject disconnectLabelObj = new GameObject();
		disconnectLabelObj.transform.parent = canvasObj.transform;
		Text disconnectLabel = disconnectLabelObj.AddComponent<Text>();
		disconnectLabel.font = MenuFont;
		disconnectLabel.text = "Disconnect";
		disconnectLabel.fontSize = 200;
		disconnectLabel.supportRichText = true;
		((Graphic)disconnectLabel).color = DisconnectTextColor;
		disconnectLabel.alignment = TextAnchor.MiddleCenter;
		disconnectLabel.resizeTextForBestFit = true;
		disconnectLabel.resizeTextMinSize = 0;
		disconnectLabel.resizeTextMaxSize = 200;
		disconnectLabel.fontStyle = FontStyle.Bold;
		RectTransform disconnectRect = ((Component)disconnectLabel).GetComponent<RectTransform>();
		((Transform)disconnectRect).localPosition = Vector3.zero;
		disconnectRect.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)disconnectRect).localPosition = new Vector3(0.064f, 0f, 0.111f - (0.28f - 0.6f) / 2.6f);
		((Transform)disconnectRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject prevBtn = MakeCylinderButton();
		prevBtn.transform.parent = menu.transform;
		prevBtn.transform.rotation = Quaternion.identity;
		prevBtn.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		prevBtn.transform.localPosition = new Vector3(0.56f, 0.65f, 0f);
		Color npTop = NextPrevButtonColor * 0.35f;
		Color npBot = NextPrevButtonColor;
		prevBtn.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		BtnCollider prevCollider = prevBtn.AddComponent<BtnCollider>();
		prevCollider.buttonId = "PreviousPage";
		prevCollider.displayText = "<";
		RoundGameObject(prevBtn, "PreviousPage", npTop, npBot);
		GameObject prevLabelObj = new GameObject();
		prevLabelObj.transform.parent = canvasObj.transform;
		Text prevLabel = prevLabelObj.AddComponent<Text>();
		prevLabel.font = MenuFont;
		prevLabel.text = "<";
		prevLabel.fontSize = 200;
		((Graphic)prevLabel).color = NextPrevTextColor;
		prevLabel.fontStyle = FontStyle.Bold;
		prevLabel.alignment = TextAnchor.MiddleCenter;
		prevLabel.resizeTextForBestFit = true;
		prevLabel.resizeTextMinSize = 0;
		prevLabel.resizeTextMaxSize = 200;
		RectTransform prevRect = ((Component)prevLabel).GetComponent<RectTransform>();
		((Transform)prevRect).localPosition = Vector3.zero;
		prevRect.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)prevRect).localPosition = new Vector3(0.064f, 0.195f, 0f);
		((Transform)prevRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject nextBtn = MakeCylinderButton();
		nextBtn.transform.parent = menu.transform;
		nextBtn.transform.rotation = Quaternion.identity;
		nextBtn.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		nextBtn.transform.localPosition = new Vector3(0.56f, -0.65f, 0f);
		nextBtn.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		BtnCollider nextCollider = nextBtn.AddComponent<BtnCollider>();
		nextCollider.buttonId = "NextPage";
		nextCollider.displayText = ">";
		RoundGameObject(nextBtn, "NextPage", npTop, npBot);
		GameObject nextLabelObj = new GameObject();
		nextLabelObj.transform.parent = canvasObj.transform;
		Text nextLabel = nextLabelObj.AddComponent<Text>();
		nextLabel.font = MenuFont;
		nextLabel.text = ">";
		nextLabel.fontSize = 200;
		((Graphic)nextLabel).color = NextPrevTextColor;
		nextLabel.fontStyle = FontStyle.Bold;
		nextLabel.alignment = TextAnchor.MiddleCenter;
		nextLabel.resizeTextForBestFit = true;
		nextLabel.resizeTextMinSize = 0;
		nextLabel.resizeTextMaxSize = 200;
		RectTransform nextRect = ((Component)nextLabel).GetComponent<RectTransform>();
		((Transform)nextRect).localPosition = Vector3.zero;
		nextRect.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)nextRect).localPosition = new Vector3(0.064f, -0.195f, 0f);
		((Transform)nextRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		if (currentButtons != null)
		{
			ButtonInfo[] page = currentButtons.Skip(pageNumber * pageSize).Take(pageSize).ToArray();
			string[] labels = (from b in page
				select b.buttonText).ToArray();
			string[] ids = (from b in page
				select b.id ?? b.buttonText).ToArray();
			for (int slot = 0; slot < labels.Length; slot++)
			{
				float slotOffset = (float)slot * ((pageSize == 7) ? 0.116f : 0.1f);
				GameObject buttonObj = MakeCylinderButton();
				buttonObj.transform.parent = menu.transform;
				buttonObj.transform.rotation = Quaternion.identity;
				buttonObj.transform.localScale = new Vector3(BUTTON_CYLINDER_SCALE_X, BUTTON_CYLINDER_SCALE_Y, BUTTON_CYLINDER_SCALE_Z);
				buttonObj.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - slotOffset);
				BtnCollider pageCollider = buttonObj.AddComponent<BtnCollider>();
				pageCollider.buttonId = ids[slot];
				pageCollider.displayText = labels[slot];
				int buttonIndex = -1;
				for (int scanIndex = 0; scanIndex < currentButtons.Count; scanIndex++)
				{
					if (ids[slot] == currentButtons[scanIndex].id)
					{
						buttonIndex = scanIndex;
						break;
					}
				}
				bool? isEnabled = null;
				if (buttonIndex >= 0 && buttonIndex < currentButtons.Count)
				{
					isEnabled = currentButtons[buttonIndex].enabled;
				}
				Color baseColor = (isEnabled == true) ? ButtonColorEnabled : ButtonColorDisable;
				Color topColor = baseColor * 0.35f;
				Color bottomColor = baseColor;
				buttonObj.GetComponent<Renderer>().material = MakeGradientMat(topColor, bottomColor);
				RoundGameObject(buttonObj, ids[slot], topColor, bottomColor);
				GameObject labelObj = new GameObject();
				labelObj.transform.parent = canvasObj.transform;
				Text label = labelObj.AddComponent<Text>();
				label.font = MenuFont;
				label.text = labels[slot];
				label.fontSize = 200;
				label.supportRichText = true;
				((Graphic)label).color = ((isEnabled == true) ? EnableTextColor : DisableTextColor);
				label.fontStyle = FontStyle.Bold;
				label.alignment = TextAnchor.MiddleCenter;
				label.resizeTextForBestFit = true;
				label.resizeTextMinSize = 0;
				label.resizeTextMaxSize = 200;
				RectTransform labelRect = ((Component)label).GetComponent<RectTransform>();
				((Transform)labelRect).localPosition = Vector3.zero;
				labelRect.sizeDelta = new Vector2(0.2f, 0.03f);
				((Transform)labelRect).localPosition = new Vector3(0.064f, 0f, 0.111f - slotOffset / 2.6f);
				((Transform)labelRect).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			}
		}
		menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * 0.8f * (_menuCameraAnchored ? 1f : ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f));
		try
		{
			foreach (Transform t in menu.GetComponentsInChildren<Transform>(true))
				t.gameObject.layer = 2;
			menu.layer = 2;
		}
		catch { }
	}
}
