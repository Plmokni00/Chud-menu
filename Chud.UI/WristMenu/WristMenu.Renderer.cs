using System.Collections.Generic;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

namespace Chud.UI;

internal partial class WristMenu
{
	private static int _pageCount = 1;

	public void Draw()
	{
		if (menuLayout == 0)
		{
			MenuLayout2.Draw2();
			return;
		}
		DrawLayout1();
	}

	private void DrawLayout1()
	{
		if (MenuManager.Instance.CurrentCategoryName == "Enabled Mods")
		{
			RebuildEnabledMods();
		}
		pageSize = 7;
		menu = new GameObject();
		menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH * 0.95625f);
		InitTextLayer();

		BuildPanel();
		BuildHeader();
		BuildDisconnect();
		BuildPage();
		BuildNavRow();
		BuildFooter();

		menu.transform.localScale = new Vector3(MENU_CYLINDER_RADIUS, MENU_CYLINDER_HEIGHT, MENU_CYLINDER_DEPTH) * MenuScaleFactor * (_menuCameraAnchored ? 1f : ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f));
		try
		{
			foreach (Transform t in menu.GetComponentsInChildren<Transform>(true))
				t.gameObject.layer = 2;
			menu.layer = 2;
		}
		catch { }
	}

	private void BuildPanel()
	{
		menuObj = MakeCylinder();
		menuObj.transform.parent = menu.transform;
		menuObj.transform.rotation = Quaternion.identity;
		menuObj.transform.localPosition = new Vector3(PanelX, 0f, 0f);
		menuObj.transform.localScale = new Vector3(PanelThickness, PanelWidth, PanelHeight);
		Color bgTop = NormalColor * 0.35f;
		Color bgBot = NormalColor;
		menuObj.GetComponent<Renderer>().material = MakeGradientMat(bgTop, bgBot);
		RoundGameObject(menuObj, "__background__", bgTop, bgBot);
	}

	private void BuildHeader()
	{
		bool noStatus = string.IsNullOrEmpty(bottomBarStr);
		float titleZ = noStatus ? TitleZ : TitleZ - TitleStatusShift;
		MakeMenuText("title", MenuTitle, new Vector3(LabelX, 0f, titleZ), TitleWidth, noStatus ? TitleHeightBig : TitleHeight, MenuTitleColor);
	}

	private void BuildDisconnect()
	{
		GameObject disconnectBtn = MakeCylinderButton();
		disconnectBtn.transform.parent = menu.transform;
		disconnectBtn.transform.rotation = Quaternion.identity;
		disconnectBtn.transform.localScale = new Vector3(DisconnectDepth, DisconnectWidth, DisconnectHeight);
		disconnectBtn.transform.localPosition = new Vector3(PanelFrontX, 0f, DisconnectZ);
		Color dcTop = DisconnectButtonColor * 0.35f;
		Color dcBot = DisconnectButtonColor;
		disconnectBtn.GetComponent<Renderer>().material = MakeGradientMat(dcTop, dcBot);
		BtnCollider disconnectCollider = disconnectBtn.AddComponent<BtnCollider>();
		disconnectCollider.buttonId = "DisconnectingButton";
		disconnectCollider.displayText = "Disconnect";
		RoundGameObject(disconnectBtn, "DisconnectingButton", dcTop, dcBot);

		MakeMenuTextStretched("DisconnectingButton", "Disconnect", new Vector3(LabelX, 0f, DisconnectZ + LabelShiftZ), DisconnectWidth * ButtonTextWidthRatio, DisconnectHeight * ButtonTextHeightRatio, DisconnectTextColor);
	}

	private void BuildPage()
	{
		List<ButtonInfo> currentButtons = MenuManager.Instance.CurrentButtons;
		if (currentButtons == null)
		{
			return;
		}
		int total = currentButtons.Count;
		int pageCount = PageCountFor(total);
		pageNumber = ClampPage(pageNumber, pageCount);
		_pageCount = pageCount;

		int start = pageNumber * pageSize;
		int end = Mathf.Min(start + pageSize, total);
		for (int i = start; i < end; i++)
		{
			ButtonInfo info = currentButtons[i];
			if (info == null)
			{
				continue;
			}
			int slot = i - start;
			string id = string.IsNullOrEmpty(info.id) ? info.buttonText : info.id;
			bool? isEnabled = info.enabled;
			Color baseColor = (isEnabled == true) ? ButtonColorEnabled : ButtonColorDisable;
			Color topColor = baseColor * 0.35f;
			float z = ButtonZForSlot(slot);

			GameObject buttonObj = MakeCylinderButton();
			buttonObj.transform.parent = menu.transform;
			buttonObj.transform.rotation = Quaternion.identity;
			buttonObj.transform.localScale = new Vector3(ButtonDepth, ButtonWidth, ButtonHeight);
			buttonObj.transform.localPosition = new Vector3(PanelFrontX, 0f, z);
			buttonObj.GetComponent<Renderer>().material = MakeGradientMat(topColor, baseColor);
			BtnCollider pageCollider = buttonObj.AddComponent<BtnCollider>();
			pageCollider.buttonId = id;
			pageCollider.displayText = info.buttonText;
			RoundGameObject(buttonObj, id, topColor, baseColor);

			Color accent = isEnabled == true
				? new Color(ButtonColorEnabled.r, ButtonColorEnabled.g, ButtonColorEnabled.b, 0.75f)
				: new Color(ButtonColorDisable.r, ButtonColorDisable.g, ButtonColorDisable.b, 0.4f);

			MakeOutline(
				"frame_" + id,
				menu.transform,
				new Vector3(PanelFrontX + 0.016f, 0f, z),
				new Vector3(0.01f, ButtonWidth, ButtonHeight),
				ButtonRadius,
				ButtonCornerSegments,
				0.018f,
				accent);

			MakeMenuText(
				id,
				TruncateLabel(info.buttonText),
				new Vector3(LabelX, LabelShiftY, z + LabelShiftZ),
				LabelMaxWidth,
				LabelMaxHeight,
				isEnabled == true ? EnableTextColor : DisableTextColor);
		}
	}

	private void BuildNavRow()
	{
		Color npTop = NextPrevButtonColor * 0.35f;
		Color npBot = NextPrevButtonColor;
		BuildNavButton("PreviousPage", "<", NavOffsetY, npTop, npBot);
		BuildNavButton("NextPage", ">", -NavOffsetY, npTop, npBot);
	}

	private void BuildNavButton(string id, string glyph, float y, Color top, Color bot)
	{
		GameObject navBtn = MakeCylinderButton();
		navBtn.transform.parent = menu.transform;
		navBtn.transform.rotation = Quaternion.identity;
		navBtn.transform.localScale = new Vector3(NavDepth, NavWidth, NavHeight);
		navBtn.transform.localPosition = new Vector3(PanelFrontX, y, NavZ);
		navBtn.GetComponent<Renderer>().material = MakeGradientMat(top, bot);
		BtnCollider navCollider = navBtn.AddComponent<BtnCollider>();
		navCollider.buttonId = id;
		navCollider.displayText = glyph;
		RoundGameObject(navBtn, id, top, bot);

		MakeOutline(
			"frame_" + id,
			menu.transform,
			new Vector3(PanelFrontX + 0.016f, y, NavZ),
			new Vector3(0.01f, NavWidth, NavHeight),
			0.22f,
			ButtonCornerSegments,
			0.018f,
			new Color(NextPrevButtonColor.r * 2.2f, NextPrevButtonColor.g * 2.2f, NextPrevButtonColor.b * 2.2f, 0.55f));

		MakeMenuText(id, glyph, new Vector3(LabelX, y, NavZ + LabelShiftZ), NavWidth * NavTextWidthRatio, NavHeight * NavTextHeightRatio, NextPrevTextColor);
	}

	private void BuildFooter()
	{
		fpsText = null;
		Text status = MakeMenuText("status", bottomBarStr, new Vector3(LabelX, 0f, StatusZ), StatusWidth, StatusHeight, ToolTipColor);
		if (status != null)
		{
			fpsText = status;
		}
	}
}
