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
		if (MenuManager.CurrentCategoryName == "Enabled Mods")
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
		Canvas val2 = canvasObj.AddComponent<Canvas>();
		CanvasScaler val3 = canvasObj.AddComponent<CanvasScaler>();
		canvasObj.AddComponent<GraphicRaycaster>();
		val2.renderMode = (RenderMode)2;
		val3.dynamicPixelsPerUnit = 1900f;
		val3.referencePixelsPerUnit = 100f;
		GameObject val4 = new GameObject();
		val4.transform.parent = canvasObj.transform;
		Text val5 = (titiel = val4.AddComponent<Text>());
		val5.font = MenuFont;
		val5.text = MenuTitle;
		val5.fontSize = 200;
		((Graphic)val5).color = MenuTitleColor;
		val5.fontStyle = (FontStyle)2;
		val5.alignment = (TextAnchor)4;
		val5.resizeTextForBestFit = true;
		val5.resizeTextMinSize = 0;
		val5.resizeTextMaxSize = 200;
		RectTransform component2 = ((Component)val5).GetComponent<RectTransform>();
		((Transform)component2).localPosition = Vector3.zero;
		component2.sizeDelta = new Vector2(0.28f, 0.05f);
		((Transform)component2).position = new Vector3(0.06f, 0f, 0.175f);
		((Transform)component2).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val6 = new GameObject();
		val6.transform.parent = canvasObj.transform;
		fpsText = val6.AddComponent<Text>();
		fpsText.font = MenuFont;
		fpsText.text = bottomBarStr;
		fpsText.fontSize = 200;
		((Graphic)fpsText).color = ToolTipColor;
		fpsText.fontStyle = (FontStyle)2;
		fpsText.alignment = (TextAnchor)4;
		fpsText.resizeTextForBestFit = true;
		fpsText.resizeTextMinSize = 0;
		fpsText.resizeTextMaxSize = 200;
		RectTransform component3 = ((Component)fpsText).GetComponent<RectTransform>();
		((Transform)component3).localPosition = Vector3.zero;
		component3.sizeDelta = new Vector2(0.28f, 0.02f);
		((Transform)component3).position = new Vector3(0.06f, 0f, 0.135f);
		((Transform)component3).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		List<ButtonInfo> currentButtons = MenuManager.CurrentButtons;
		GameObject val7 = MakeCylinderButton();
		val7.transform.parent = menu.transform;
		val7.transform.rotation = Quaternion.identity;
		val7.transform.localScale = new Vector3(BUTTON_CYLINDER_SCALE_X, BUTTON_CYLINDER_SCALE_Y, BUTTON_CYLINDER_SCALE_Z);
		val7.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
		Color dcTop = DisconnectButtonColor * 0.35f;
		Color dcBot = DisconnectButtonColor;
		val7.GetComponent<Renderer>().material = MakeGradientMat(dcTop, dcBot);
		val7.AddComponent<BtnCollider>().relatedText = "DisconnectingButton";
		RoundGameObject(val7, "DisconnectingButton", dcTop, dcBot);
		GameObject val8 = new GameObject();
		val8.transform.parent = canvasObj.transform;
		Text val9 = val8.AddComponent<Text>();
		val9.font = MenuFont;
		val9.text = "Disconnect";
		val9.fontSize = 200;
		val9.supportRichText = true;
		((Graphic)val9).color = DisconnectTextColor;
		val9.alignment = (TextAnchor)4;
		val9.resizeTextForBestFit = true;
		val9.resizeTextMinSize = 0;
		val9.resizeTextMaxSize = 200;
		val9.fontStyle = (FontStyle)2;
		RectTransform component4 = ((Component)val9).GetComponent<RectTransform>();
		((Transform)component4).localPosition = Vector3.zero;
		component4.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component4).localPosition = new Vector3(0.064f, 0f, 0.111f - (0.28f - 0.6f) / 2.6f);
		((Transform)component4).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val10 = MakeCylinderButton();
		val10.transform.parent = menu.transform;
		val10.transform.rotation = Quaternion.identity;
		val10.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val10.transform.localPosition = new Vector3(0.56f, 0.65f, 0f);
		Color npTop = NextPrevButtonColor * 0.35f;
		Color npBot = NextPrevButtonColor;
		val10.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		val10.AddComponent<BtnCollider>().relatedText = "PreviousPage";
		RoundGameObject(val10, "PreviousPage", npTop, npBot);
		GameObject val11 = new GameObject();
		val11.transform.parent = canvasObj.transform;
		Text val12 = val11.AddComponent<Text>();
		val12.font = MenuFont;
		val12.text = "<";
		val12.fontSize = 200;
		((Graphic)val12).color = NextPrevTextColor;
		val12.fontStyle = (FontStyle)2;
		val12.alignment = (TextAnchor)4;
		val12.resizeTextForBestFit = true;
		val12.resizeTextMinSize = 0;
		val12.resizeTextMaxSize = 200;
		RectTransform component5 = ((Component)val12).GetComponent<RectTransform>();
		((Transform)component5).localPosition = Vector3.zero;
		component5.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component5).localPosition = new Vector3(0.064f, 0.195f, 0f);
		((Transform)component5).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		GameObject val13 = MakeCylinderButton();
		val13.transform.parent = menu.transform;
		val13.transform.rotation = Quaternion.identity;
		val13.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
		val13.transform.localPosition = new Vector3(0.56f, -0.65f, 0f);
		val13.GetComponent<Renderer>().material = MakeGradientMat(npTop, npBot);
		val13.AddComponent<BtnCollider>().relatedText = "NextPage";
		RoundGameObject(val13, "NextPage", npTop, npBot);
		GameObject val14 = new GameObject();
		val14.transform.parent = canvasObj.transform;
		Text val15 = val14.AddComponent<Text>();
		val15.font = MenuFont;
		val15.text = ">";
		val15.fontSize = 200;
		((Graphic)val15).color = NextPrevTextColor;
		val15.fontStyle = (FontStyle)2;
		val15.alignment = (TextAnchor)4;
		val15.resizeTextForBestFit = true;
		val15.resizeTextMinSize = 0;
		val15.resizeTextMaxSize = 200;
		RectTransform component6 = ((Component)val15).GetComponent<RectTransform>();
		((Transform)component6).localPosition = Vector3.zero;
		component6.sizeDelta = new Vector2(0.2f, 0.03f);
		((Transform)component6).localPosition = new Vector3(0.064f, -0.195f, 0f);
		((Transform)component6).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		if (currentButtons != null)
		{
			string[] array = (from b in currentButtons.Skip(pageNumber * pageSize).Take(pageSize)
				select b.buttonText).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				float num2 = (float)num * ((pageSize == 7) ? 0.116f : 0.1f);
				GameObject val16 = MakeCylinderButton();
				val16.transform.parent = menu.transform;
				val16.transform.rotation = Quaternion.identity;
				val16.transform.localScale = new Vector3(BUTTON_CYLINDER_SCALE_X, BUTTON_CYLINDER_SCALE_Y, BUTTON_CYLINDER_SCALE_Z);
				val16.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - num2);
				val16.AddComponent<BtnCollider>().relatedText = array[num];
				int num3 = -1;
				for (int num4 = 0; num4 < currentButtons.Count; num4++)
				{
					if (array[num] == currentButtons[num4].buttonText)
					{
						num3 = num4;
						break;
					}
				}
				bool? flag = null;
				if (num3 >= 0 && num3 < currentButtons.Count)
				{
					flag = currentButtons[num3].enabled;
				}
				Color btnBase = (flag == true) ? ButtonColorEnabled : ButtonColorDisable;
				Color btnTop = btnBase * 0.35f;
				Color btnBot = btnBase;
				val16.GetComponent<Renderer>().material = MakeGradientMat(btnTop, btnBot);
				RoundGameObject(val16, array[num], btnTop, btnBot);
				GameObject val17 = new GameObject();
				val17.transform.parent = canvasObj.transform;
				Text val18 = val17.AddComponent<Text>();
				val18.font = MenuFont;
				val18.text = array[num];
				val18.fontSize = 200;
				val18.supportRichText = true;
				((Graphic)val18).color = ((flag == true) ? EnableTextColor : DisableTextColor);
				val18.fontStyle = (FontStyle)2;
				val18.alignment = (TextAnchor)4;
				val18.resizeTextForBestFit = true;
				val18.resizeTextMinSize = 0;
				val18.resizeTextMaxSize = 200;
				RectTransform component7 = ((Component)val18).GetComponent<RectTransform>();
				((Transform)component7).localPosition = Vector3.zero;
				component7.sizeDelta = new Vector2(0.2f, 0.03f);
				((Transform)component7).localPosition = new Vector3(0.064f, 0f, 0.111f - num2 / 2.6f);
				((Transform)component7).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			}
		}
		menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * 0.88f * (_menuCameraAnchored ? 1f : ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f));
		try
		{
			foreach (Transform t in menu.GetComponentsInChildren<Transform>(true))
				t.gameObject.layer = 2;
			menu.layer = 2;
		}
		catch { }
	}
}