using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using UnityEngine;

namespace Chud.Backend;

public sealed class MenuManager
{
	public static MenuManager Instance { get; } = new MenuManager();

	public List<MenuCategory> Categories = new List<MenuCategory>();

	public string CurrentCategoryName = "Main";

	public MenuCategory CurrentCategory => Categories.Find((MenuCategory c) => c.Name == CurrentCategoryName);

	public List<ButtonInfo> CurrentButtons => CurrentCategory?.Buttons;

	private MenuManager()
	{
	}

	public void AddCategory(string name, List<ButtonInfo> buttons = null)
	{
		if (!Categories.Any((MenuCategory c) => c.Name == name))
		{
			Categories.Add(new MenuCategory(name, buttons ?? new List<ButtonInfo>()));
		}
	}

	public void ToggleCategory(string name)
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
