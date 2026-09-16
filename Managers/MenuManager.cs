using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using UnityEngine;

namespace Chud.Backend;

public static class MenuManager
{
	public static List<MenuCategory> Categories = new List<MenuCategory>();

	public static string CurrentCategoryName = "Main";

	public static MenuCategory CurrentCategory => Categories.Find((MenuCategory c) => c.Name == CurrentCategoryName);

	public static List<ButtonInfo> CurrentButtons => CurrentCategory?.Buttons;

	public static void AddCategory(string name, List<ButtonInfo> buttons = null)
	{
		if (!Categories.Any((MenuCategory c) => c.Name == name))
		{
			Categories.Add(new MenuCategory(name, buttons ?? new List<ButtonInfo>()));
		}
	}

	public static void ToggleCategory(string name)
	{
		if (CurrentCategoryName == name)
		{
			CurrentCategoryName = "Main";
		}
		else
		{
			CurrentCategoryName = name;
		}
		WristMenu.pageNumber = 0;
		if (WristMenu.toggleMenu)
		{
			WristMenu.RefreshMenu();
		}
		else
		{
			WristMenu.DestroyMenu();
			if ((Object)(object)WristMenu.instance != (Object)null)
			{
				WristMenu.instance.Draw();
			}
		}
	}
}
