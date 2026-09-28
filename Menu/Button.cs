using System;
namespace Chud.Menu
{
    public static class Button
    {
        public const string NoTooltip = "This button doesn't have a tooltip/tutorial";

        public static ButtonInfo Action(string id, string text, string tip, Action method)
        {
            return new ButtonInfo
            {
                id = id,
                buttonText = text,
                toolTip = tip,
                method = method,
                type = ButtonType.Action,
                enabled = false
            };
        }

        public static ButtonInfo Toggle(string id, string text, string tip, Action on, Action off)
        {
            return new ButtonInfo
            {
                id = id,
                buttonText = text,
                toolTip = tip,
                enableMethod = on,
                disableMethod = off,
                type = ButtonType.Toggle,
                enabled = false
            };
        }

        public static ButtonInfo Frame(string id, string text, string tip, Action onTick, Action off = null)
        {
            return new ButtonInfo
            {
                id = id,
                buttonText = text,
                toolTip = tip,
                method = onTick,
                disableMethod = off,
                type = ButtonType.FrameToggle,
                enabled = false
            };
        }

        public static ButtonInfo Gun(string id, string text, string tip, Action onTrigger, Action off = null)
        {
            return new ButtonInfo
            {
                id = id,
                buttonText = text,
                toolTip = tip,
                method = onTrigger,
                disableMethod = off,
                type = ButtonType.Gun,
                enabled = false
            };
        }

        public static ButtonInfo RequiringMode(this ButtonInfo button, string gameMode)
        {
            if (button != null)
            {
                button.requiredGameMode = gameMode;
            }

            return button;
        }

        public static ButtonInfo RequiringLobby(this ButtonInfo button)
        {
            if (button != null)
            {
                button.requiresLobby = true;
            }

            return button;
        }

        public static ButtonInfo Tip(this ButtonInfo button, string tip)
        {
            if (button != null)
            {
                button.toolTip = tip;
            }

            return button;
        }
    }
}