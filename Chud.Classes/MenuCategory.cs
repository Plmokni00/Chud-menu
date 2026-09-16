using System.Collections.Generic;

namespace Chud.Classes;

public class MenuCategory
{
	public string Name;

	public List<ButtonInfo> Buttons = new List<ButtonInfo>();

	public MenuCategory(string name)
	{
		Name = name;
	}

	public MenuCategory(string name, List<ButtonInfo> buttons)
	{
		Name = name;
		Buttons = buttons;
	}
}
