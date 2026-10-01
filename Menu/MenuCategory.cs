using System.Collections.Generic;
using System.Linq;
namespace Chud.Menu
{
    public class MenuCategory
    {
        public string Name { get; }

        public List<ButtonInfo> Buttons { get; }

        public MenuCategory(string name, List<ButtonInfo> buttons = null)
        {
            Name = name ?? string.Empty;
            Buttons = buttons ?? new List<ButtonInfo>();
        }

        public ButtonInfo Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (int i = 0; i < Buttons.Count; i++)
            {
                ButtonInfo button = Buttons[i];
                if (button != null && button.buttonText == name)
                {
                    return button;
                }
            }

            return null;
        }

        public int RemoveWhere(System.Func<ButtonInfo, bool> predicate)
        {
            return predicate == null ? 0 : Buttons.RemoveAll(b => b != null && predicate(b));
        }
    }
}