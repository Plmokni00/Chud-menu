using Chud.Backend;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Chud.Menu
{
    public static class OptionPages
    {
        public static void RegisterAll(MenuRegistry registry)
        {
            Add(registry, "Fly Speed", "Settings",
                new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                Mods.SetFlySpeed, "Set fly speed to {0}");

            Add(registry, "WASD Sense", "Settings",
                new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2", "2.25", "2.5", "2.75", "3" },
                Mods.SetWASDFlyMouseSense, "Set WASD fly sensitivity to {0}");

            Add(registry, "Speed Boost Settings", "Settings",
                Mods.SpeedBoostNames, Mods.SetSpeedBoostAmount, "Set speed boost to {0}");

            Add(registry, "Pull Power", "Settings",
                Mods.PullPowerNames, Mods.SetPullModPower, "Set pull mod strength to {0}");

            Add(registry, "Notification Time", "Settings",
                new[] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" },
                Mods.SetNotificationTime, "Notifications stay {0}");

            Add(registry, "Tag Aura Range", "Settings",
                new[] { "Off", "0.5m", "1m", "1.5m", "2m", "2.5m", "3m", "4m", "5m" },
                Mods.SetTagAuraRange, "Set tag aura range to {0}");

            Add(registry, "Anti Report Range", "Settings",
                new[] { "0.25m", "0.35m", "0.5m", "0.7m", "1m", "1.25m", "1.5m", "2m" },
                Mods.SetAntiReportRange, "Set anti-report detection range to {0}");

            Add(registry, "Water Splash Speed", "Settings",
                Mods.WaterSplashNames, Mods.SetWaterSplashSpeed, "Set water splash cooldown to {0}");

            Add(registry, "Controller Predictions Settings", "Settings",
                Mods.ControllerPredNames, Mods.SetControllerPrediction, "Set controller predictions to {0}");

            Add(registry, "FPS Spoofer Settings", "Settings",
                Mods.FPSSpoofValues.Select(v => v.ToString()).ToArray(), Mods.SetFPSSpoof, "Spoof {0} fps");

            MenuCategory fpsSpoofPage = registry.Find("FPS Spoofer Settings");
            if (fpsSpoofPage != null)
            {
                fpsSpoofPage.Buttons.Add(Button.Toggle("Random FPS",
                    "Roll a random value from 0 to 255 every " + Mods.FpsSpoofRandomInterval + " seconds",
                    Mods.EnableFPSSpoofRandom, Mods.DisableFPSSpoofRandom));
            }

            Add(registry, "Button Click Sound", "Settings",
                new[] { "Default button click", "Clicker trainer", "DDLC", "Minecraft Lever", "Skype" },
                Mods.SetButtonClickSound, "Use {0} for button clicks");

            Add(registry, "Menu Layout", "Settings",
                new[] { "Normal layout", "Modern layout" }, Mods.SetMenuLayout, "Use {0} for the menu");
        }

        private static void Add(MenuRegistry registry, string pageName, string exitTarget,
            string[] options, Action<int> setter, string tipFormat)
        {
            var buttons = new List<ButtonInfo>(options.Length + 1)
            {
                Button.Action("Exit " + pageName, "Returns to the settings page",
                    () => UI.WristMenu.NavigateTo(exitTarget))
            };

            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                string label = options[i];
                string tip;
                try
                {
                    tip = string.Format(tipFormat, label);
                }
                catch (FormatException)
                {
                    tip = tipFormat;
                }

                buttons.Add(Button.Action(label, tip, () => setter(index)));
            }

            registry.AddCategory(pageName, buttons);
        }
    }
}
