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
            Add(registry, "Fly Speed", "fly_speed", "Settings",
                new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                Mods.SetFlySpeed, "Set fly speed to {0}");

            Add(registry, "WASD Sense", "wasd_sense", "Settings",
                new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2", "2.25", "2.5", "2.75", "3" },
                Mods.SetWASDFlyMouseSense, "Set WASD fly sensitivity to {0}");

            Add(registry, "Speed Boost Settings", "speed_boost_opt", "Settings",
                Mods.SpeedBoostNames, Mods.SetSpeedBoostAmount, "Set speed boost to {0}");

            Add(registry, "Pull Power", "pull_power_opt", "Settings",
                Mods.PullPowerNames, Mods.SetPullModPower, "Set pull mod strength to {0}");

            Add(registry, "Notification Time", "notif_time", "Settings",
                new[] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" },
                Mods.SetNotificationTime, "Notifications stay {0}");

            Add(registry, "Tag Aura Range", "tag_aura_range", "Settings",
                new[] { "Off", "0.5m", "1m", "1.5m", "2m", "2.5m", "3m", "4m", "5m" },
                Mods.SetTagAuraRange, "Set tag aura range to {0}");

            Add(registry, "Anti Report Range", "antireport_range", "Settings",
                new[] { "0.25m", "0.35m", "0.5m", "0.7m", "1m", "1.25m", "1.5m", "2m" },
                Mods.SetAntiReportRange, "Set anti-report detection range to {0}");

            Add(registry, "Water Splash Speed", "splash_speed", "Settings",
                Mods.WaterSplashNames, Mods.SetWaterSplashSpeed, "Set water splash cooldown to {0}");

            Add(registry, "Controller Predictions Settings", "controller_pred", "Settings",
                Mods.ControllerPredNames, Mods.SetControllerPrediction, "Set controller predictions");

            Add(registry, "FPS Spoofer Settings", "fps_spoof_opt", "Settings",
                Mods.FPSSpoofValues.Select(v => v.ToString()).ToArray(), Mods.SetFPSSpoof, "Spoof {0} fps");

            Add(registry, "Button Click Sound", "click_sound", "Settings",
                new[] { "Default button click", "Clicker trainer", "DDLC", "Minecraft Lever", "Skype" },
                Mods.SetButtonClickSound, "Use {0} for button clicks");

            Add(registry, "Menu Layout", "menu_layout", "Settings",
                new[] { "Normal layout", "Modern layout" }, Mods.SetMenuLayout, "Use {0} for the menu");
        }

        private static void Add(MenuRegistry registry, string pageName, string idPrefix, string exitTarget,
            string[] options, Action<int> setter, string tipFormat)
        {
            var buttons = new List<ButtonInfo>(options.Length + 1)
            {
                Button.Action(idPrefix + "_exit", "Exit " + pageName, "Returns to the settings page",
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

                buttons.Add(Button.Action(idPrefix + "_" + index, label, tip, () => setter(index)));
            }

            registry.AddCategory(pageName, buttons);
        }
    }
}