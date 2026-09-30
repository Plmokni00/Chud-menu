using Chud.Backend;
using Chud.Patches;
using Chud.Runtime;
using GTAG_NotificationLib;
using Mods = Chud.Backend.Mods;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.Menu
{
    public static class ButtonCatalog
    {
        public sealed class Page
        {
            public Page(string name, List<ButtonInfo> buttons)
            {
                Name = name;
                Buttons = buttons;
            }

            public string Name { get; }

            public List<ButtonInfo> Buttons { get; }
        }

        public static IReadOnlyList<Page> Pages { get; } = Build();

        private static IReadOnlyList<Page> Build()
        {
            return new List<Page>(15)
            {
                MainPage(),
                SettingsPage(),
                ColorsPage(),
                EnabledModsPage(),
                MovementPage(),
                VisualPage(),
                MiscPage(),
                RoomPage(),
                FunPage(),
                RigPage(),
                InfectionPage(),
                MasterPage(),
                ConsoleModsPage(),
                ConsoleSettingsPage(),
                CreditsPage()
            };
        }

        public static void RegisterAll()
        {
            MenuRegistry registry = MenuRegistry.Instance;

            foreach (Page page in Pages)
            {
                registry.AddCategory(page.Name, page.Buttons);
            }

            registry.AddCategory("Soundboard", Mods.BuildSoundboardCategory());

            OptionPages.RegisterAll(registry);

            NormaliseActionState(registry);
        }

        private static void NormaliseActionState(MenuRegistry registry)
        {
            registry.ForEachButton((_, button) =>
            {
                if (button.type == ButtonType.Action && !button.enabled.HasValue)
                {
                    button.enabled = false;
                }
            });
        }

        #region Pages

        private static Page MainPage()
        {
            return new Page(MenuRegistry.MainCategory, new List<ButtonInfo>
            {
                Button.Action("main_discord", "Join Discord", "Join the Chud Menu Discord",
                    () => Application.OpenURL("https://discord.gg/3kzkDTFbH7")),
                Nav("main_settings", "Settings", "Opens the settings tab", "Settings"),
                Nav("main_enabled", "Enabled Mods", "Shows your enabled mods", "Enabled Mods"),
                Nav("main_movement", "Movement Mods", "Opens the movement mods", "Movement Mods"),
                Nav("main_visual", "Visual Mods", "Opens the visual mods", "Visual Mods"),
                Nav("main_fun", "Fun Mods", "Opens the fun mods", "Fun Mods"),
                Nav("main_misc", "Misc Mods", "Opens the misc mods", "Misc Mods"),
                Nav("main_rig", "Rig Mods", "Opens the rig mods", "Rig Mods"),
                Nav("main_infection", "Infection Mods", "Opens the infection mods", "Infection Mods"),
                Nav("main_room", "Room Mods", "Opens the room mods", "Room Mods"),
                Nav("main_master", "Master Mods", "Opens the master mods", "Master Mods"),
                Nav("main_soundboard", "Soundboard", "Opens the soundboard", "Soundboard"),
                Nav("main_credits", "Credits", "Opens the credits", "Credits")
            });
        }

        private static Page SettingsPage()
        {
            return new Page("Settings", new List<ButtonInfo>
            {
                Nav("settings_exit", "Exit Settings", "Returns to the main page", "Settings"),
                Nav("settings_colors", "Menu Colors", "Opens the menu colors", "Menu Colors"),
                Nav("settings_fly_speed", "Fly Speed", "Opens the fly speed settings", "Fly Speed"),
                Nav("settings_wasd_sense", "WASD Sense", "Opens the WASD fly sensitivity settings", "WASD Sense"),
                Nav("settings_speedboost", "Speed Boost Settings", "Opens the speed boost settings", "Speed Boost Settings"),
                Nav("settings_pull_power", "Pull Power", "Opens the pull mod power settings", "Pull Power"),
                Nav("settings_notif_time", "Notification Time", "Opens the notification time settings", "Notification Time"),
                Nav("settings_tag_aura", "Tag Aura Range", "Opens the tag aura range settings", "Tag Aura Range"),
                Nav("settings_antireport", "Anti Report Range", "Opens the anti-report range settings", "Anti Report Range"),
                Nav("settings_splash_speed", "Water Splash Speed", "Opens the water splash speed settings", "Water Splash Speed"),
                Nav("settings_controller_pred", "Controller Predictions Settings", "Opens the controller predictions settings", "Controller Predictions Settings"),
                Nav("settings_fps_spoof", "FPS Spoofer Settings", "Opens the FPS spoofer settings", "FPS Spoofer Settings"),
                Nav("settings_click_sound", "Button Click Sound", "Opens the button click sound settings", "Button Click Sound"),
                Nav("settings_menu_layout", "Menu Layout", "Opens the menu layout settings", "Menu Layout"),

                Button.Toggle("settings_animations", "Menu Animations",
                    "Toggle menu open/close and button press animations",
                    () => Chud.UI.WristMenu.AnimationsEnabled = true,
                    () => Chud.UI.WristMenu.AnimationsEnabled = false),
                Button.Toggle("settings_toggle_menu", "Toggle Menu",
                    "Press button once to open, press again to close",
                    () => Chud.UI.WristMenu.ToggleMenu = true,
                    () => Chud.UI.WristMenu.ToggleMenu = false),
                Button.Toggle("settings_right_hand", "Right Hand", "Move menu to right hand",
                    Mods.EnableRightHand, Mods.DisableRightHand),
                Button.Toggle("settings_show_fps", "Show FPS", "Show FPS counter",
                    () => Chud.UI.WristMenu.ShowFPS = true,
                    () => Chud.UI.WristMenu.ShowFPS = false),
                Button.Toggle("settings_show_session", "Show Session Time", "Show session duration",
                    () => Chud.UI.WristMenu.ShowSessionTime = true,
                    () => Chud.UI.WristMenu.ShowSessionTime = false),
                Button.Toggle("settings_no_mouse_lock", "No Mouse Lock",
                    "Prevent WASD fly from locking mouse on right click",
                    () => Mods.SetWASDFlyNoMouseLock(true),
                    () => Mods.SetWASDFlyNoMouseLock(false)),
                Button.Toggle("settings_pc_guns", "PC Guns", "Use guns with mouse",
                    Mods.EnablePCGuns, Mods.DisablePCGuns),
                Button.Toggle("settings_pc_click", "PC Button Click", "Click buttons with mouse",
                    Mods.EnablePCButtonClick, Mods.DisablePCButtonClick),
                Button.Toggle("settings_toggle_notifs", "Toggle Notifications", "Show/hide notifications",
                    Mods.ToggleNotifications, Mods.DisableNotifications),
                Button.Action("settings_clear_notifs", "Clear Notifications",
                    "Remove all on-screen notifications", Mods.ClearNotifications),

                Button.Toggle("settings_custom_boards", "Custom Boards",
                    "Replace in-game message boards with custom text",
                    () =>
                    {
                        Chud.UI.WristMenu.CustomBoardsEnabled = true;
                        Chud.UI.WristMenu.CustomBoardsApplied = false;
                    },
                    () =>
                    {
                        Chud.UI.WristMenu.CustomBoardsEnabled = false;
                        Chud.UI.WristMenu.CustomBoardsApplied = false;
                        if (Chud.UI.WristMenu.HasInstance)
                        {
                            Chud.UI.WristMenu.Instance.RestoreOriginalBoardText();
                        }
                    }),

                Button.Toggle("settings_see_reports", "see anti cheat reports", "Show anti-cheat reports",
                    Mods.EnableSeeAntiCheatReports, Mods.DisableSeeAntiCheatReports)
            });
        }

        private static Page ColorsPage()
        {
            string[] names = Mods.MenuColorNames;
            var buttons = new List<ButtonInfo>(names.Length + 1)
            {
                Nav("colors_exit", "Exit Menu Colors", "Returns to the settings page", "Menu Colors")
            };

            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                buttons.Add(Button.Action(
                    "colors_" + names[i].ToLowerInvariant(),
                    names[i],
                    "Set menu color to " + names[i].ToLowerInvariant(),
                    () => Mods.SetMenuColor(index)));
            }

            return new Page("Menu Colors", buttons);
        }

        private static Page EnabledModsPage()
        {
            return new Page(MenuRegistry.EnabledModsCategory, new List<ButtonInfo>());
        }

        private static Page MovementPage()
        {
            return new Page("Movement Mods", new List<ButtonInfo>
            {
                Nav("movement_exit", "Exit Movement Mods", "Returns to the main page", "Movement Mods"),
                Button.Frame("movement_fly", "Fly", "Hold B", Mods.EnableFly, Mods.DisableFly),
                Button.Frame("movement_joystick_fly", "Joystick Fly", "Fly with joystick", Mods.JoystickFly, Mods.DisableJoystickFly),
                Button.Frame("movement_wasd_fly", "WASD Fly", "Fly with WASD keys", Mods.EnableWASDFly, Mods.DisableWASDFly),
                Button.Frame("movement_speed_boost", "Speed Boost", "Hold grip to run fast", Mods.SpeedBoost, Mods.DisableSpeedBoost),
                Button.Frame("movement_no_gravity", "No Gravity", "Disable gravity", Mods.NoGravity, Mods.DisableNoGravity),
                Button.Frame("movement_noclip", "Noclip", "Walk through walls", Mods.Noclip, Mods.NoclipOff),
                Button.Frame("movement_platforms", "Platforms", "Place platforms", Mods.Platforms),
                Button.Frame("movement_sticky_platforms", "Sticky Platforms", "Sticky ver of plats", Mods.StickyPlatforms),
                Button.Frame("movement_pull_mod", "Pull Mod", "Pull forward while gripping", Mods.PullMod),
                Button.Gun("movement_tp_gun", "TP Gun", "Shoot to teleport", Mods.TPGun, Mods.CleanupGun),
                Button.Action("movement_tp_stump", "Teleport to Stump", "Teleport to the forest stump", Mods.TeleportToSpawn),
                Button.Frame("movement_minos", "Minos Prime", "Right B to jump, then Right A to slam", Mods.MinosPrime, Mods.DisableMinosPrime),
                Button.Toggle("movement_spider", "Spider monke", "Walk on any surface you touch", Mods.EnableSpiderMonkey, Mods.DisableSpiderMonkey),
                Button.Toggle("movement_controller_pred", "Controller Predictions",
                    "Amplify your hand movement, everyone sees it", Mods.EnableControllerPredictions, Mods.DisableControllerPredictions)
            });
        }

        private static Page VisualPage()
        {
            return new Page("Visual Mods", new List<ButtonInfo>
            {
                Nav("visual_exit", "Exit Visual Mods", "Returns to the main page", "Visual Mods"),
                Button.Frame("visual_cosmetic_tags", "Cosmetic Name Tags", "Show cosmetics above heads", Mods.CosmeticNameTags, Mods.DisableCosmeticNameTags),
                Button.Frame("visual_id_tags", "ID Name Tags", "Show IDs above heads", Mods.IDTags, Mods.DisableIDTags),
                Button.Frame("visual_platform_tags", "Platform Name Tags", "Show platform above heads", Mods.PlatformTags, Mods.DisablePlatformTags),
                Button.Frame("visual_name_tags", "Name Tags", "Show names above heads", Mods.NameTags, Mods.DisableNameTags),
                Button.Frame("visual_fps_tags", "FPS Name Tags", "Show FPS above heads", Mods.FPSTags, Mods.DisableFPSTags),
                Button.Frame("visual_ars_tags", "ARS Nametags", "Show people on ARS", Mods.EnableARSNameTags, Mods.DisableARSNameTags),
                Button.Frame("visual_tracers", "Tracers", "Lines towards everyone", Mods.Tracers, Mods.DisableTracers),
                Button.Frame("visual_box_esp", "2D Box ESP", "Boxes around players", Mods.BoxEspRender, Mods.DisableBoxEsp),
                Button.Frame("visual_skeleton_esp", "Skeleton ESP", "Draw skeleton lines on players", Mods.SkeletonEsp, Mods.DisableSkeletonEsp),
                Button.Frame("visual_third_person", "3rd Person", "Third person view -- X to toggle", Mods.EnableThirdPerson, Mods.DisableThirdPerson),
                Button.Frame("visual_cosmetic_notifier", "Cosmetic Notifier",
                    "The notis show who has a special cosmetics", Mods.CosmeticNotifier, Mods.DisableCosmeticNotifier)
            });
        }

        private static Page MiscPage()
        {
            return new Page("Misc Mods", new List<ButtonInfo>
            {
                Nav("misc_exit", "Exit Misc Mods", "Returns to the main page", "Misc Mods"),
                Button.Toggle("misc_anti_name_ban", "Anti Name Ban", "Prevent name bans", Mods.AntiNameBan, Mods.DisableAntiNameBan),
                Button.Toggle("misc_anti_afk", "Anti AFK", "Prevent AFK kick", Mods.AntiAFK, Mods.DisableAntiAFK),
                Button.Toggle("misc_anti_guardian_grab", "Anti Guardian Grab", "Block guardian grab", Mods.AntiGuardianGrab, Mods.DisableAntiGuardianGrab),
                Button.Toggle("misc_disable_quit_box", "Disable Quit Box", "Disable quit box", Mods.DisableQuitBox, Mods.EnableQuitBox),
                Button.Toggle("misc_disable_net_triggers", "Disable Network Triggers", "Change maps without leaving", Mods.DisableNetworkTriggers, Mods.EnableNetworkTriggers),
                Button.Toggle("misc_block_jman", "Block jman sounds", "Block jman sounds", Mods.BlockJmanSounds, Mods.DisableBlockJmanSounds),
                Button.Toggle("misc_anti_block_crash", "Anti Block Crash", "doesn't load the blocks in monkey blocks", Mods.AntiBlockCrash, Mods.DisableAntiBlockCrash),
                Button.Gun("misc_mute_gun", "Mute Gun", "Shoot to mute/unmute", Mods.MuteGun, Mods.CleanupGun),
                Button.Toggle("misc_ars", "ARS", "Auto-report system", Mods.EnableARS, Mods.DisableARS),
                Button.Toggle("misc_anti_report", "Anti Report", "Disconnect if someone nears your report button", Mods.EnableAntiReport, Mods.DisableAntiReport)
            });
        }

        private static Page RoomPage()
        {
            return new Page("Room Mods", new List<ButtonInfo>
            {
                Nav("room_exit", "Exit Room Mods", "Returns to the main page", "Room Mods"),
                Button.Action("room_join_random", "Join Random Public", "Join a random public lobby", Mods.JoinRandomPublic),
                JoinCodeButton("room_join_mods", "Join Code MODS", "Join MODS room", "MODS"),
                JoinCodeButton("room_join_mod", "Join Code MOD", "Join MOD room", "MOD"),
                JoinCodeButton("room_join_chud", "Join Code chud", "Join chud room", "chud"),
                JoinCodeButton("room_join_pixel", "Join Code PIXEL", "Join PIXEL room", "PIXEL"),
                JoinCodeButton("room_join_mbeachy", "Join Code MBEACHY", "Join MBEACHY room", "MBEACHY"),
                JoinCodeButton("room_join_content", "Join Code CONTENT", "Join CONTENT room", "CONTENT"),
                JoinCodeButton("room_join_creator", "Join Code CREATOR", "Join CREATOR room", "CREATOR"),
                JoinCodeButton("room_join_foggy", "Join Code FOGGY", "Join FOGGY room", "FOGGY"),
                JoinCodeButton("room_join_lucio", "Join Code LUCIO", "Join LUCIO room", "LUCIO"),
                JoinCodeButton("room_join_pbbv", "Join Code PBBV", "Join PBBV room", "PBBV"),
                JoinCodeButton("room_join_echo", "Join Code ECHO", "Join ECHO room", "ECHO"),
                JoinCodeButton("room_join_run", "Join Code RUN", "Join RUN room", "RUN")
            });
        }

        private static Page FunPage()
        {
            return new Page("Fun Mods", new List<ButtonInfo>
            {
                Nav("fun_exit", "Exit Fun Mods", "Returns to the main page", "Fun Mods"),
                Button.Toggle("fun_unlock_vim", "Unlock VIM/Subscription", "Unlock VIM features", Mods.UnlockVim, Mods.DisableUnlockVim),

                Button.Action("fun_unlock_cosmetics", "Unlock All Cosmetics",
                    "Unlocks all cosmetics and lets you see others' Cosmetx cosmetics",
                    () =>
                    {
                        Mods.UnlockAllCosmetics();
                        Chud.Patches.UnlockAllCosmeticsPatch.Enabled = true;
                    }),

                Button.Toggle("fun_tryon_all", "SS tryon all cosmetics (Mirror)",
                    "Fills worn slots with Tree Pin and cycles every other cosmetic in the mirror one at a time",
                    Mods.EnableTryOnAll, Mods.DisableTryOnAll),
                Button.Toggle("fun_remove_all", "Remove all cosmetics (Mirror)",
                    "Put on every worn cosmetic so they turn off",
                    Mods.EnableRemoveAllCosmetics, Mods.DisableRemoveAllCosmetics),
                Button.Toggle("fun_bitcrunch", "Bitcrunch Mic", "Makes ur mic sound bad", Mods.BitcrunchMic, Mods.DisableBitcrunchMic),
                Button.Frame("fun_boop", "Boop", "Play's a noise when booping someone", Mods.Boop, Mods.DisableBoop),
                Button.Gun("fun_getid_gun", "GetPlayerID Gun", "Shoot to copy ID", Mods.GetPlayerIDGun, Mods.CleanupGun),
                Button.Gun("fun_lag_gun", "Lag Gun", "Lags whoever u shoot, not very good only works on quest", Mods.LagGun, Mods.StopLagGunFull),
                Button.Gun("fun_orbit_gun", "Orbit Gun", "T-pose orbit around player", Mods.OrbitGun, Mods.StopOrbitFull),
                Button.Toggle("fun_paintbrawl_aimbot", "Paintbrawl Aimbot",
                    "Redirects your slingshot to the closest player",
                    () => Chud.Patches.SlingshotAimbotPatch.Enabled = true,
                    () => Chud.Patches.SlingshotAimbotPatch.Enabled = false),
                Button.Frame("fun_color_spaz", "Random Color Spaz", "Change colors fast", Mods.RandomColorSpaz, Mods.DisableRandomColorSpaz),
                Button.Frame("fun_water_splash", "Water Splash", "Splash water from your hand", Mods.WaterSplash, Mods.DisableWaterSplash),
                Button.Action("fun_group_kick", "Group kick all (Stump)",
                    "Kick everyone in stump you will get kicked too but it will auto rejoin, only works in privates",
                    Mods.GroupKickAll),
                Button.Action("fun_get_id_self", "Get ID Self", "Copy your ID", Mods.GetIDSelf),
                Button.Frame("fun_grab_bugs_all", "Grab All Bugs", "Grab all bugs with your hand -- Grab them first", Mods.GrabAllBugs, Mods.DisableGrabAllBugs),
                Button.Frame("fun_grab_bug_green", "Grab Green Bug", "Grab Green Doug with grip from anywhere -- Grab them first", Mods.GrabGreenBug, Mods.DisableGrabGreenBug),
                Button.Frame("fun_grab_bug_doug", "Grab Doug the Bug", "Grab Doug with grip from anywhere -- Grab them first", Mods.GrabDougBug, Mods.DisableGrabDougBug),
                Button.Frame("fun_spaz_bugs", "Spaz Bugs", "Spaz the bugs between your hands -- Grab them first", Mods.SpazBugs, Mods.DisableSpazBugs),
                Button.Action("fun_lowercase_name", "lowercase name", "Make ur name lowercase", Mods.MakeNameLowercase),
                Button.Toggle("fun_fps_spoof", "FPS Spoofer", "Spoof your fps to other players", Mods.EnableFPSSpoof, Mods.DisableFPSSpoof),
                Button.Action("fun_altcase_name", "Random Capital Name", "make ur name alternating case", Mods.MakeNameAlternatingCase)
            });
        }

        private static Page RigPage()
        {
            return new Page("Rig Mods", new List<ButtonInfo>
            {
                Nav("rig_exit", "Exit Rig Mods", "Returns to the main page", "Rig Mods"),
                Button.Frame("rig_ghost", "Ghost Monke", "Press B to freeze your rig", Mods.GhostMonke, Mods.DisableGhostMonke),
                Button.Frame("rig_invis", "Invis Monke", "Press A to be invisible", Mods.InvisMonke, Mods.DisableInvisMonke),
                Button.Toggle("rig_backflip", "Backflip", "Press B", Mods.EnableBackflip, Mods.DisableBackflip),
                Button.Toggle("rig_frontflip", "Frontflip", "Press B", Mods.EnableFrontflip, Mods.DisableFrontflip),
                Button.Toggle("rig_spinning_torso", "Spinning Torso", "Makes your torso spin around", Mods.EnableSpinningTorso, Mods.DisableSpinningTorso),
                Button.Toggle("rig_fake_fbt", "Fake FBT", "Fake Full Body Tracking", Mods.EnableFakeFBT, Mods.DisableFakeFBT),
                Button.Toggle("rig_dinnerbone", "Dinnerbone", "Flip yourself upside down", Mods.EnableDinnerbone, Mods.DisableDinnerbone),
                Button.Toggle("rig_natsuki", "Natsuki Neck", "Snap your neck to the right", Mods.EnableNatsukiNeck, Mods.DisableNatsukiNeck),
                Button.Frame("rig_grab_rig", "Grab Rig", "Hold grip to grab your rig", Mods.GrabRig, Mods.DisableGrabRig),
                Button.Gun("movement_copy_gun", "Copy Movement Gun", "Lock onto player and copy their movements", Mods.CopyMovementGun, Mods.StopCopyMovementGunFull),
                Button.Gun("rig_look_at_gun", "Look At Gun", "Makes your rigs stare at whoever Your gun is Shooting", Mods.LookAtGun, Mods.StopLookAtGunFull)
            });
        }

        private static Page InfectionPage()
        {
            return new Page("Infection Mods", new List<ButtonInfo>
            {
                Nav("infection_exit", "Exit Infection Mods", "Returns to the main page", "Infection Mods"),
                Button.Gun("infection_tag_gun", "Tag Gun", "Its tag gun", Mods.TagGun, Mods.CleanupGun),
                Button.Frame("infection_tag_all", "Tag All", "Tags everyone", Mods.TagAll, Mods.DisableTagAll),
                Button.Frame("infection_tag_aura", "Tag Aura", "Auto-tag players around you", Mods.TagAura, Mods.DisableTagAura),
                Button.Frame("infection_tag_aura_visual", "Tag Aura Visual", "Show aura range visual", Mods.TagAuraVisual, Mods.DisableTagAuraVisual)
            });
        }

        private static Page MasterPage()
        {
            return new Page("Master Mods", new List<ButtonInfo>
            {
                Nav("master_exit", "Exit Master Mods", "Returns to the main page", "Master Mods"),
                Button.Action("master_status", "Not master client", "Your current master client status", null),

                Button.Toggle("master_spaz_self", "Spaz Self", "Tag and untag urself", Mods.SpazSelf, Mods.DisableSpazSelf)
                    .RequiringMode("Infection"),
                Button.Action("master_untag_self", "Untag Self", "untag urself", Mods.UntagSelf)
                    .RequiringMode("Infection"),
                Button.Toggle("master_spaz_all", "Spaz All", "Tag and untag everyone", Mods.SpazAll, Mods.DisableSpazAll)
                    .RequiringMode("Infection"),
                Button.Gun("master_untag_gun", "Untag Gun", "Shoot infected players to untag them", Mods.UntagGun, Mods.CleanupGun)
                    .RequiringMode("Infection"),

                Button.Action("master_paintbrawl_kill_all", "Paint Brawl Kill All", "Kill everyone in paintbrawl", Mods.PaintBrawlKillAll)
                    .RequiringMode("Paintbrawl"),
                Button.Gun("master_paintbrawl_kill_gun", "Paint Brawl Kill Gun", "Shoot a player to kill them in paintbrawl", Mods.PaintBrawlKillGun, Mods.CleanupGun)
                    .RequiringMode("Paintbrawl")
            });
        }

        private static Page ConsoleModsPage()
        {
            return new Page(MenuRegistry.ConsoleModsCategory, new List<ButtonInfo>
            {
                Nav("console_exit", "Exit Console Mods", "Returns to the main page", "Console Mods"),

                Button.Gun("console_kick_gun", "Kick Gun", "Shoot a player to kick them", ConsoleMods.KickGun, Mods.CleanupGun),
                Button.Gun("console_silent_kick_gun", "Silent Kick Gun", "Shoot a player to silently kick them", ConsoleMods.SilentKickGun, Mods.CleanupGun),
                Button.Gun("console_fling_gun", "Fling Gun", "Shoot a player to fling them", ConsoleMods.FlingGun, Mods.CleanupGun),
                Button.Gun("console_vibrate_gun", "Vibrate Gun", "Shoot a player to vibrate their controllers", ConsoleMods.VibrateGun, Mods.CleanupGun),
                Button.Gun("console_lightning_gun", "Lightning Gun", "Shoot to strike lightning", ConsoleMods.LightningGun, Mods.CleanupGun),
                Button.Gun("console_jail_gun", "Jail Gun", "Trap players in a jail cell", ConsoleMods.JailGun, ConsoleMods.JailGunOff),
                Button.Gun("console_tpall_gun", "TP All Gun", "Teleport everyone to your aim point", ConsoleMods.TPAllGun, Mods.CleanupGun),
                Button.Gun("console_freeze_gun", "Freeze Gun", "Hit to freeze/unfreeze players", ConsoleMods.FreezeGun.Fire, ConsoleMods.FreezeGun.Disable),

                Button.Toggle("console_scale_self", "Scale Self", "Right trigger bigger, left trigger smaller", ConsoleMods.ScaleSelf.Enable, ConsoleMods.ScaleSelf.Disable),
                Button.Toggle("console_admin_grab", "Admin Grab", "Grab players with your hand", ConsoleMods.AdminGrab.Enable, ConsoleMods.AdminGrab.Disable),
                Button.Toggle("console_admin_grab_all", "Admin Grab All", "Grab all players at once no matter distance", ConsoleMods.AdminGrabAll.Enable, ConsoleMods.AdminGrabAll.Disable),
                Button.Toggle("console_laser", "Laser", "Toggle lasers from your hands", ConsoleMods.Laser.Enable, ConsoleMods.Laser.Disable),
                Button.Action("console_kick_all", "Kick All", "Kick everyone from lobby", ConsoleMods.KickAll),

                AssetToggle("console_karambit", "Karambit", ConsoleMods.Karambit.Enable, ConsoleMods.Karambit.Disable),
                AssetToggle("console_rblx_carpet", "Rblx Carpet", ConsoleMods.RblxCarpet.Enable, ConsoleMods.RblxCarpet.Disable),
                AssetToggle("console_mc_sword", "MC Sword", ConsoleMods.McSword.Enable, ConsoleMods.McSword.Disable),
                AssetToggle("console_ban_hammer", "Ban Hammer", ConsoleMods.BanHammer.Enable, ConsoleMods.BanHammer.Disable),
                AssetToggle("console_roblox_sword", "Roblox Sword", ConsoleMods.RobloxSword.Enable, ConsoleMods.RobloxSword.Disable),
                AssetToggle("console_rainbow_sword", "Rainbow Sword", ConsoleMods.RainbowSword.Enable, ConsoleMods.RainbowSword.Disable),
                AssetToggle("console_ender_sword", "Weird Ender Sword", ConsoleMods.WeirdEnderSword.Enable, ConsoleMods.WeirdEnderSword.Disable),
                AssetToggle("console_pistol", "Pistol", ConsoleMods.Pistol.Enable, ConsoleMods.Pistol.Disable),
                AssetToggle("console_physics_gun", "Physics Gun", ConsoleMods.PhysicsGun.Enable, ConsoleMods.PhysicsGun.Disable),
                AssetToggle("console_noli_star", "Noli Star", ConsoleMods.NoliStar.Enable, ConsoleMods.NoliStar.Disable),
                AssetToggle("console_bag", "Bag", ConsoleMods.Bag.Enable, ConsoleMods.Bag.Disable),
                AssetToggle("console_kormakur", "Kormakur", ConsoleMods.Kormakur.Enable, ConsoleMods.Kormakur.Disable),
                AssetToggle("console_coin", "Coin", ConsoleMods.Coin.Enable, ConsoleMods.Coin.Disable),
                AssetToggle("console_minos_plush", "Minos Prime Plush", ConsoleMods.MinosPrime.Enable, ConsoleMods.MinosPrime.Disable),
                AssetToggle("console_boombox", "Boombox", ConsoleMods.Boombox.Enable, ConsoleMods.Boombox.Disable),
                AssetToggle("console_samsung", "Samsung", ConsoleMods.Samsung.Enable, ConsoleMods.Samsung.Disable),
                AssetToggle("console_tv", "TV", ConsoleMods.TV.Enable, ConsoleMods.TV.Disable),
                AssetToggle("console_travis", "Travis", ConsoleMods.Travis.Enable, ConsoleMods.Travis.Disable),
                AssetToggle("console_travis_beach", "Travis (Beach)", ConsoleMods.TravisBeach.Enable, ConsoleMods.TravisBeach.Disable),
                AssetToggle("console_travis_critters", "Travis (Critters)", ConsoleMods.TravisCritters.Enable, ConsoleMods.TravisCritters.Disable),
                AssetToggle("console_travis_city", "Travis (City)", ConsoleMods.TravisCity.Enable, ConsoleMods.TravisCity.Disable),
                AssetToggle("console_shreksophone", "Shreksophone", ConsoleMods.Shreksophone.Enable, ConsoleMods.Shreksophone.Disable),
                AssetToggle("console_carti", "Carti", ConsoleMods.Carti.Enable, ConsoleMods.Carti.Disable),
                AssetToggle("console_cherry_bomb", "Cherry Bomb", ConsoleMods.CherryBomb.Enable, ConsoleMods.CherryBomb.Disable),
                Button.Toggle("console_spoof", "Console Spoof", "Spoof as gay furry femboy menu v69", Chud.Backend.Console.EnableConsoleSpoof, Chud.Backend.Console.DisableConsoleSpoof),
                Button.Action("console_destroy_assets", "Destroy All Assets", "Remove all spawned assets", ConsoleMods.DestroyAllAssets),
                Nav("console_open_settings", "Console Settings", "Opens console settings", "Console Settings")
            });
        }

        private static Page ConsoleSettingsPage()
        {
            return new Page(MenuRegistry.ConsoleSettingsCategory, new List<ButtonInfo>
            {
                Nav("console_settings_exit", "Exit Console Settings", "Returns to the console page", "Console Settings"),
                Button.Toggle("console_settings_kick_self", "Allow Kick Self",
                    "Allow other admins to kick/tp/fling you", ConsoleMods.AllowKickSelf.Enable, ConsoleMods.AllowKickSelf.Disable),
                Button.Toggle("console_settings_tp_self", "Allow Teleport Self",
                    "Allow other admins to teleport you", ConsoleMods.AllowTpSelf.Enable, ConsoleMods.AllowTpSelf.Disable),
                Button.Toggle("console_settings_detect", "Detect Console Users",
                    "Auto detect who has console", ConsoleMods.DetectConsoleUsers.Enable, ConsoleMods.DetectConsoleUsers.Disable),
                Button.Toggle("console_settings_logging", "Console Logging",
                    "Log console commands, asset spawns, and errors to BepInEx + notification",
                    ConsoleMods.ConsoleLogging.Enable, ConsoleMods.ConsoleLogging.Disable),
                Button.Toggle("console_settings_no_indicator", "No Admin Indicator", "Hide your admin crown",
                    ConsoleMods.NoAdminIndicator.Enable, ConsoleMods.NoAdminIndicator.Disable),
                Button.Toggle("console_settings_fullauto", "Full Auto Pistol", "Toggle full auto mode for pistol",
                    ConsoleMods.FullAutoPistol.Enable, ConsoleMods.FullAutoPistol.Disable),
                Button.Toggle("console_settings_see_crown", "See Crown", "Show your own crown above your head",
                    () => Chud.Backend.Console.SeeOwnCrown = true,
                    () => Chud.Backend.Console.SeeOwnCrown = false)
            });
        }

        private static Page CreditsPage()
        {
            return new Page("Credits", new List<ButtonInfo>
            {
                Nav("credits_exit", "Exit Credits", "Returns to the main page", "Credits"),
                Button.Action("credits_jolyne", "Jolyne/Sayori", "Owners Github",
                    () => Application.OpenURL("https://github.com/Plmokni00")),
                Button.Action("credits_muse", "Muse Spark 1.3 Free", "The AI i use",
                    () => NotifiLib.SendNotification("Muse Spark 1.3 Free: The AI i use", 2)),
                Button.Action("credits_spacebunny", "Space Bunny Free", "The AI i use",
                    () => NotifiLib.SendNotification("Space Bunny Free: The AI i use", 2)),
                Button.Action("credits_industry", "Industry", "ARS system by Industry",
                    () => NotifiLib.SendNotification("Industry: ARS system by Industry", 2))
            });
        }

        #endregion

        #region Helpers

        private static ButtonInfo Nav(string id, string text, string tip, string target)
        {
            return Button.Action(id, text, tip, () => UI.WristMenu.NavigateTo(target));
        }

        private static ButtonInfo JoinCodeButton(string id, string text, string tip, string code)
        {
            return Button.Action(id, text, tip, () => Mods.JoinCode(code));
        }

        private static ButtonInfo AssetToggle(string id, string name, Action enable, Action disable)
        {
            return Button.Toggle(id, name, "This is " + name, enable, disable);
        }

        #endregion
    }
}