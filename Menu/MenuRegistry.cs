using Chud.Diagnostics;
using System.Collections.Generic;
using System;
namespace Chud.Menu
{
    public sealed class MenuRegistry
    {
        public const string MainCategory = "Main";
        public const string EnabledModsCategory = "Enabled Mods";
        public const string MasterModsCategory = "Master Mods";
        public const string ConsoleModsCategory = "Console Mods";
        public const string ConsoleSettingsCategory = "Console Settings";

        private static readonly MenuRegistry _instance = new MenuRegistry();

        private readonly List<MenuCategory> _categories = new List<MenuCategory>(32);
        private readonly HashSet<string> _categoryNames = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<MenuCategory> Categories => _categories;

        public static MenuRegistry Instance => _instance;

        public string CurrentCategoryName { get; set; } = MainCategory;

        public MenuCategory CurrentCategory
        {
            get
            {
                for (int i = 0; i < _categories.Count; i++)
                {
                    MenuCategory category = _categories[i];
                    if (category != null && category.Name == CurrentCategoryName)
                    {
                        return category;
                    }
                }

                return null;
            }
        }

        public IReadOnlyList<ButtonInfo> CurrentButtons
        {
            get
            {
                MenuCategory category = CurrentCategory;
                return category == null ? Array.Empty<ButtonInfo>() : category.Buttons;
            }
        }

        public MenuCategory Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (int i = 0; i < _categories.Count; i++)
            {
                MenuCategory category = _categories[i];
                if (category != null && category.Name == name)
                {
                    return category;
                }
            }

            return null;
        }

        public bool Contains(string name) => Find(name) != null;

        public MenuCategory AddCategory(string name, List<ButtonInfo> buttons = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("refusing to register a menu category with no name");
                return null;
            }

            if (!_categoryNames.Add(name))
            {
                Log.Warn("menu category '" + name + "' registered more than once; keeping the first");
                return Find(name);
            }

            var category = new MenuCategory(name, buttons);
            _categories.Add(category);
            return category;
        }

        public bool NavigateTo(string name)
        {
            string target = string.IsNullOrEmpty(name) || name == CurrentCategoryName
                ? MainCategory
                : name;

            if (Find(target) == null)
            {
                Log.Warn("navigation to unknown menu category '" + target + "' ignored");
                return false;
            }

            if (target == CurrentCategoryName)
            {
                return false;
            }

            CurrentCategoryName = target;
            return true;
        }

        public void ForEachButton(Action<MenuCategory, ButtonInfo> action)
        {
            if (action == null)
            {
                return;
            }

            for (int c = 0; c < _categories.Count; c++)
            {
                MenuCategory category = _categories[c];
                if (category == null || category.Name == EnabledModsCategory)
                {
                    continue;
                }

                List<ButtonInfo> buttons = category.Buttons;
                for (int b = 0; b < buttons.Count; b++)
                {
                    ButtonInfo button = buttons[b];
                    if (button != null)
                    {
                        action(category, button);
                    }
                }
            }
        }
    }
}