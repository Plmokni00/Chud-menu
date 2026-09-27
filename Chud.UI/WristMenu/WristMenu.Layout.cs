using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	internal const float MenuScaleFactor = 0.95f;
	internal const float ButtonTextMaxStretch = 1.6f;
	internal const float PanelThickness = 0.08f;
	internal const float PanelFrontOffset = 0.533f;
	internal const float PanelX = PanelFrontOffset - PanelThickness * 0.425f;
	internal const float PanelWidth = 0.9f;
	internal const float PanelHeight = 1f;
	internal const float PanelFrontX = 0.56f;
	internal const float LabelX = PanelFrontX + 0.035f;

	internal const float TitleZ = 0.42f;
	internal const float TitleStatusShift = 0.015f;
	internal const float TitleHeight = 0.082f;
	internal const float TitleHeightBig = 0.1f;
	internal const float TitleWidth = 0.78f;

	internal const float StatusZ = 0.462f;
	internal const float StatusWidth = 0.72f;
	internal const float StatusHeight = 0.026f;

	internal const float DisconnectZ = 0.556f;
	internal const float DisconnectHeight = ButtonHeight;
	internal const float DisconnectWidth = ButtonWidth;
	internal const float DisconnectDepth = ButtonDepth;

	internal const float FirstButtonZ = 0.275f;
	internal const float ButtonPitch = 0.088f;
	internal const float ButtonHeight = 0.072f;
	internal const float ButtonWidth = 0.76f;
	internal const float ButtonDepth = 0.05f;
	internal const float ButtonRadius = 0.16f;
	internal const int ButtonCornerSegments = 6;

	internal const float LabelShiftY = 0f;
	internal const float LabelShiftZ = 0.008f;
	internal const float ButtonTextWidthRatio = 0.9f;
	internal const float ButtonTextHeightRatio = 0.73f;
	internal const float NavTextWidthRatio = 0.55f;
	internal const float NavTextHeightRatio = 0.55f;
	internal const float LabelMaxWidth = ButtonWidth * ButtonTextWidthRatio;
	internal const float LabelMaxHeight = ButtonHeight * ButtonTextHeightRatio;

	internal const float NavZ = -0.375f;
	internal const float NavHeight = 0.1f;
	internal const float NavWidth = 0.36f;
	internal const float NavOffsetY = 0.205f;
	internal const float NavDepth = 0.05f;

	internal static float ButtonZForSlot(int slot)
	{
		return FirstButtonZ - slot * ButtonPitch;
	}

	internal static int PageCountFor(int count)
	{
		if (pageSize < 1)
		{
			pageSize = 7;
		}
		if (count <= 0)
		{
			return 1;
		}
		int pages = (count + pageSize - 1) / pageSize;
		return pages < 1 ? 1 : pages;
	}

	internal static int ClampPage(int page, int pageCount)
	{
		if (page < 0)
		{
			return 0;
		}
		if (pageCount < 1)
		{
			pageCount = 1;
		}
		return page > pageCount - 1 ? pageCount - 1 : page;
	}

	internal static string TruncateLabel(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return " ";
		}
		return text.Length <= 40 ? text : text.Substring(0, 39) + "…";
	}

	private static GameObject MakePlate(string name, Transform parent, Vector3 localPosition, Vector3 localScale, float radius, int segments)
	{
		GameObject go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.transform.localPosition = localPosition;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = localScale;
		MeshFilter mf = go.AddComponent<MeshFilter>();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();
		mf.mesh = GenerateRoundedRectMesh(1f, 1f, radius, segments, 0.85f);
		mr.material = MakePlainMat(Color.white);
		return go;
	}

	private static GameObject MakeOutline(string name, Transform parent, Vector3 localPosition, Vector3 localScale, float radius, int segments, float border, Color color)
	{
		GameObject go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.transform.localPosition = localPosition;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = localScale;
		MeshFilter mf = go.AddComponent<MeshFilter>();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();
		mf.mesh = GenerateRoundedFrameMesh(radius, border, segments);
		mr.material = MakeOutlineMat(color);
		return go;
	}
}
