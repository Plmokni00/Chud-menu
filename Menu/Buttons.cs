using System;
using System.Collections.Generic;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using Chud.UI;
using GTAG_NotificationLib;
using Photon.Pun;
using UnityEngine;
using Mods = Chud.Backend.Mods;

namespace Chud.Menu;

public static class Buttons
{
	public static string[] categoryNames =
	{
		"Main",
		"Settings",
		"Menu Colors",
		"Enabled Mods",
		"Movement Mods",
		"Visual Mods",
		"Misc Mods",
		"Room Mods",
		"Fun Mods",
		"Rig Mods",
		"Infection Mods",
		"Master Mods",
		"Console Mods",
		"Console Settings",
		"Credits"
	};

	public static ButtonInfo[][] buttons =
	{
		new[] {
			new ButtonInfo { id = "main_discord", buttonText = "Join Discord", method = () => Application.OpenURL("https://discord.gg/3kzkDTFbH7"), type = ButtonType.Action, toolTip = "Join the Chud Menu Discord" },
			new ButtonInfo { id = "main_settings", buttonText = "Settings", method = () => MenuManager.Instance.ToggleCategory("Settings"), type = ButtonType.Action, toolTip = "Opens the settings tab" },
			new ButtonInfo { id = "main_enabled", buttonText = "Enabled Mods", method = () => MenuManager.Instance.ToggleCategory("Enabled Mods"), type = ButtonType.Action, toolTip = "Shows your enabled mods" },
			new ButtonInfo { id = "main_movement", buttonText = "Movement Mods", method = () => MenuManager.Instance.ToggleCategory("Movement Mods"), type = ButtonType.Action, toolTip = "Opens the movement mods" },
			new ButtonInfo { id = "main_visual", buttonText = "Visual Mods", method = () => MenuManager.Instance.ToggleCategory("Visual Mods"), type = ButtonType.Action, toolTip = "Opens the visual mods" },
			new ButtonInfo { id = "main_fun", buttonText = "Fun Mods", method = () => MenuManager.Instance.ToggleCategory("Fun Mods"), type = ButtonType.Action, toolTip = "Opens the fun mods" },
			new ButtonInfo { id = "main_misc", buttonText = "Misc Mods", method = () => MenuManager.Instance.ToggleCategory("Misc Mods"), type = ButtonType.Action, toolTip = "Opens the misc mods" },
			new ButtonInfo { id = "main_rig", buttonText = "Rig Mods", method = () => MenuManager.Instance.ToggleCategory("Rig Mods"), type = ButtonType.Action, toolTip = "Opens the rig mods" },
			new ButtonInfo { id = "main_infection", buttonText = "Infection Mods", method = () => MenuManager.Instance.ToggleCategory("Infection Mods"), type = ButtonType.Action, toolTip = "Opens the infection mods" },
			new ButtonInfo { id = "main_room", buttonText = "Room Mods", method = () => MenuManager.Instance.ToggleCategory("Room Mods"), type = ButtonType.Action, toolTip = "Opens the room mods" },
			new ButtonInfo { id = "main_master", buttonText = "Master Mods", method = () => MenuManager.Instance.ToggleCategory("Master Mods"), type = ButtonType.Action, toolTip = "Opens the master mods" },
			new ButtonInfo { id = "main_soundboard", buttonText = "Soundboard", method = () => MenuManager.Instance.ToggleCategory("Soundboard"), type = ButtonType.Action, toolTip = "Opens the soundboard" },
			new ButtonInfo { id = "main_credits", buttonText = "Credits", method = () => MenuManager.Instance.ToggleCategory("Credits"), type = ButtonType.Action, toolTip = "Opens the credits" }
		},

		new[] {
			new ButtonInfo { id = "settings_exit", buttonText = "Exit Settings", method = () => MenuManager.Instance.ToggleCategory("Settings"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "settings_colors", buttonText = "Menu Colors", method = () => MenuManager.Instance.ToggleCategory("Menu Colors"), type = ButtonType.Action, toolTip = "Opens the menu colors" },
			new ButtonInfo { id = "settings_fly_speed", buttonText = "Fly Speed", method = () => MenuManager.Instance.ToggleCategory("Fly Speed"), type = ButtonType.Action, toolTip = "Opens the fly speed settings" },
			new ButtonInfo { id = "settings_wasd_sense", buttonText = "WASD Sense", method = () => MenuManager.Instance.ToggleCategory("WASD Sense"), type = ButtonType.Action, toolTip = "Opens the WASD fly sensitivity settings" },
			new ButtonInfo { id = "settings_speedboost", buttonText = "Speed Boost Settings", method = () => MenuManager.Instance.ToggleCategory("Speed Boost Settings"), type = ButtonType.Action, toolTip = "Opens the speed boost settings" },
			new ButtonInfo { id = "settings_pull_power", buttonText = "Pull Power", method = () => MenuManager.Instance.ToggleCategory("Pull Power"), type = ButtonType.Action, toolTip = "Opens the pull mod power settings" },
			new ButtonInfo { id = "settings_notif_time", buttonText = "Notification Time", method = () => MenuManager.Instance.ToggleCategory("Notification Time"), type = ButtonType.Action, toolTip = "Opens the notification time settings" },
			new ButtonInfo { id = "settings_tag_aura", buttonText = "Tag Aura Range", method = () => MenuManager.Instance.ToggleCategory("Tag Aura Range"), type = ButtonType.Action, toolTip = "Opens the tag aura range settings" },
			new ButtonInfo { id = "settings_antireport", buttonText = "Anti Report Range", method = () => MenuManager.Instance.ToggleCategory("Anti Report Range"), type = ButtonType.Action, toolTip = "Opens the anti-report range settings" },
			new ButtonInfo { id = "settings_splash_speed", buttonText = "Water Splash Speed", method = () => MenuManager.Instance.ToggleCategory("Water Splash Speed"), type = ButtonType.Action, toolTip = "Opens the water splash speed settings" },
			new ButtonInfo { id = "settings_controller_pred", buttonText = "Controller Predictions Settings", method = () => MenuManager.Instance.ToggleCategory("Controller Predictions Settings"), type = ButtonType.Action, toolTip = "Opens the controller predictions settings" },
			new ButtonInfo { id = "settings_fps_spoof", buttonText = "FPS Spoofer Settings", method = () => MenuManager.Instance.ToggleCategory("FPS Spoofer Settings"), type = ButtonType.Action, toolTip = "Opens the FPS spoofer settings" },
			new ButtonInfo { id = "settings_click_sound", buttonText = "Button Click Sound", method = () => MenuManager.Instance.ToggleCategory("Button Click Sound"), type = ButtonType.Action, toolTip = "Opens the button click sound settings" },
			new ButtonInfo { id = "settings_animations", buttonText = "Menu Animations", enableMethod = () => WristMenu.animationsEnabled = true, disableMethod = () => WristMenu.animationsEnabled = false, enabled = false, type = ButtonType.Toggle, toolTip = "Toggle menu open/close and button press animations" },
			new ButtonInfo { id = "settings_toggle_menu", buttonText = "Toggle Menu", enableMethod = () => WristMenu.toggleMenu = true, disableMethod = () => WristMenu.toggleMenu = false, enabled = false, type = ButtonType.Toggle, toolTip = "Press button once to open, press again to close" },
			new ButtonInfo { id = "settings_right_hand", buttonText = "Right Hand", enableMethod = Mods.EnableRightHand, disableMethod = Mods.DisableRightHand, enabled = false, type = ButtonType.Toggle, toolTip = "Move menu to right hand" },
			new ButtonInfo { id = "settings_show_fps", buttonText = "Show FPS", enableMethod = () => WristMenu.showFPS = true, disableMethod = () => WristMenu.showFPS = false, enabled = false, type = ButtonType.Toggle, toolTip = "Show FPS counter" },
			new ButtonInfo { id = "settings_show_session", buttonText = "Show Session Time", enableMethod = () => WristMenu.showSessionTime = true, disableMethod = () => WristMenu.showSessionTime = false, enabled = false, type = ButtonType.Toggle, toolTip = "Show session duration" },
			new ButtonInfo { id = "settings_no_mouse_lock", buttonText = "No Mouse Lock", enableMethod = () => Mods.SetWASDFlyNoMouseLock(true), disableMethod = () => Mods.SetWASDFlyNoMouseLock(false), enabled = false, type = ButtonType.Toggle, toolTip = "Prevent WASD fly from locking mouse on right click" },
			new ButtonInfo { id = "settings_pc_guns", buttonText = "PC Guns", enableMethod = Mods.EnablePCGuns, disableMethod = Mods.DisablePCGuns, enabled = false, type = ButtonType.Toggle, toolTip = "Use guns with mouse" },
			new ButtonInfo { id = "settings_pc_click", buttonText = "PC Button Click", enableMethod = Mods.EnablePCButtonClick, disableMethod = Mods.DisablePCButtonClick, enabled = false, type = ButtonType.Toggle, toolTip = "Click buttons with mouse" },
			new ButtonInfo { id = "settings_toggle_notifs", buttonText = "Toggle Notifications", enableMethod = Mods.ToggleNotifications, disableMethod = Mods.DisableNotifications, enabled = false, type = ButtonType.Toggle, toolTip = "Show/hide notifications" },
			new ButtonInfo { id = "settings_clear_notifs", buttonText = "Clear Notifications", method = Mods.ClearNotifications, type = ButtonType.Action, toolTip = "Remove all on-screen notifications" },
			new ButtonInfo { id = "settings_custom_boards", buttonText = "Custom Boards", enableMethod = () => { WristMenu.customBoardsEnabled = true; WristMenu.customBoardsApplied = false; }, disableMethod = () => { WristMenu.customBoardsEnabled = false; WristMenu.customBoardsApplied = false; if (WristMenu.instance != null) WristMenu.instance.RestoreOriginalBoardText(); }, enabled = false, type = ButtonType.Toggle, toolTip = "Replace in-game message boards with custom text" },
			new ButtonInfo { id = "settings_see_reports", buttonText = "see anti cheat reports", enableMethod = Mods.EnableSeeAntiCheatReports, disableMethod = Mods.DisableSeeAntiCheatReports, enabled = false, type = ButtonType.Toggle, toolTip = "Show anti-cheat reports" }
		},

		new[] {
			new ButtonInfo { id = "colors_exit", buttonText = "Exit Menu Colors", method = () => MenuManager.Instance.ToggleCategory("Menu Colors"), type = ButtonType.Action, toolTip = "Returns to the settings page" },
			new ButtonInfo { id = "colors_gray", buttonText = "Gray", method = () => Mods.SetMenuColor(0), type = ButtonType.Action, toolTip = "Set menu color to gray" },
			new ButtonInfo { id = "colors_brown", buttonText = "Brown", method = () => Mods.SetMenuColor(1), type = ButtonType.Action, toolTip = "Set menu color to brown" },
			new ButtonInfo { id = "colors_red", buttonText = "Red", method = () => Mods.SetMenuColor(2), type = ButtonType.Action, toolTip = "Set menu color to red" },
			new ButtonInfo { id = "colors_orange", buttonText = "Orange", method = () => Mods.SetMenuColor(3), type = ButtonType.Action, toolTip = "Set menu color to orange" },
			new ButtonInfo { id = "colors_yellow", buttonText = "Yellow", method = () => Mods.SetMenuColor(4), type = ButtonType.Action, toolTip = "Set menu color to yellow" },
			new ButtonInfo { id = "colors_pink", buttonText = "Pink", method = () => Mods.SetMenuColor(5), type = ButtonType.Action, toolTip = "Set menu color to pink" },
			new ButtonInfo { id = "colors_purple", buttonText = "Purple", method = () => Mods.SetMenuColor(6), type = ButtonType.Action, toolTip = "Set menu color to purple" },
			new ButtonInfo { id = "colors_blue", buttonText = "Blue", method = () => Mods.SetMenuColor(7), type = ButtonType.Action, toolTip = "Set menu color to blue" },
			new ButtonInfo { id = "colors_cyan", buttonText = "Cyan", method = () => Mods.SetMenuColor(8), type = ButtonType.Action, toolTip = "Set menu color to cyan" },
			new ButtonInfo { id = "colors_green", buttonText = "Green", method = () => Mods.SetMenuColor(9), type = ButtonType.Action, toolTip = "Set menu color to green" }
		},

		new[] {
			new ButtonInfo { id = "enabled_exit", buttonText = "Exit Enabled Mods", method = () => MenuManager.Instance.ToggleCategory("Enabled Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" }
		},

		new[] {
			new ButtonInfo { id = "movement_exit", buttonText = "Exit Movement Mods", method = () => MenuManager.Instance.ToggleCategory("Movement Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "movement_fly", buttonText = "Fly", enableMethod = Mods.EnableFly, disableMethod = Mods.DisableFly, enabled = false, type = ButtonType.Toggle, toolTip = "Hold B" },
			new ButtonInfo { id = "movement_joystick_fly", buttonText = "Joystick Fly", enableMethod = Mods.JoystickFly, disableMethod = Mods.DisableJoystickFly, enabled = false, type = ButtonType.Toggle, toolTip = "Fly with joystick" },
			new ButtonInfo { id = "movement_wasd_fly", buttonText = "WASD Fly", enableMethod = Mods.EnableWASDFly, disableMethod = Mods.DisableWASDFly, enabled = false, type = ButtonType.Toggle, toolTip = "Fly with WASD keys" },
			new ButtonInfo { id = "movement_speed_boost", buttonText = "Speed Boost", method = Mods.SpeedBoost, disableMethod = Mods.DisableSpeedBoost, enabled = false, type = ButtonType.FrameToggle, toolTip = "Hold grip to run fast" },
			new ButtonInfo { id = "movement_no_gravity", buttonText = "No Gravity", method = Mods.NoGravity, disableMethod = Mods.DisableNoGravity, enabled = false, type = ButtonType.FrameToggle, toolTip = "Disable gravity" },
			new ButtonInfo { id = "movement_noclip", buttonText = "Noclip", method = Mods.Noclip, disableMethod = Mods.NoclipOff, enabled = false, type = ButtonType.FrameToggle, toolTip = "Walk through walls" },
			new ButtonInfo { id = "movement_platforms", buttonText = "Platforms", method = Mods.Platforms, type = ButtonType.FrameToggle, enabled = false, toolTip = "Place platforms" },
			new ButtonInfo { id = "movement_sticky_platforms", buttonText = "Sticky Platforms", method = Mods.StickyPlatforms, type = ButtonType.FrameToggle, enabled = false, toolTip = "Sticky ver of plats" },
			new ButtonInfo { id = "movement_pull_mod", buttonText = "Pull Mod", method = Mods.PullMod, type = ButtonType.FrameToggle, enabled = false, toolTip = "Pull forward while gripping" },
			new ButtonInfo { id = "movement_tp_gun", buttonText = "TP Gun", method = Mods.TPGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot to teleport" },
			new ButtonInfo { id = "movement_tp_stump", buttonText = "Teleport to Stump", method = Mods.TeleportToSpawn, type = ButtonType.Action, toolTip = "Teleport to the forest stump" },
			new ButtonInfo { id = "movement_minos", buttonText = "Minos Prime", method = Mods.MinosPrime, disableMethod = Mods.DisableMinosPrime, enabled = false, type = ButtonType.FrameToggle, toolTip = "Right B to jump, then Right A to slam" },
			new ButtonInfo { id = "movement_spider", buttonText = "Spider monke", enableMethod = Mods.EnableSpiderMonkey, disableMethod = Mods.DisableSpiderMonkey, enabled = false, type = ButtonType.Toggle, toolTip = "Walk on any surface you touch" },
			new ButtonInfo { id = "movement_controller_pred", buttonText = "Controller Predictions", enableMethod = Mods.EnableControllerPredictions, disableMethod = Mods.DisableControllerPredictions, enabled = false, type = ButtonType.Toggle, toolTip = "Amplify your hand movement, everyone sees it" }
		},

		new[] {
			new ButtonInfo { id = "visual_exit", buttonText = "Exit Visual Mods", method = () => MenuManager.Instance.ToggleCategory("Visual Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "visual_cosmetic_tags", buttonText = "Cosmetic Name Tags", method = Mods.CosmeticNameTags, disableMethod = Mods.DisableCosmeticNameTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show cosmetics above heads" },
			new ButtonInfo { id = "visual_id_tags", buttonText = "ID Name Tags", method = Mods.IDTags, disableMethod = Mods.DisableIDTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show IDs above heads" },
			new ButtonInfo { id = "visual_platform_tags", buttonText = "Platform Name Tags", method = Mods.PlatformTags, disableMethod = Mods.DisablePlatformTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show platform above heads" },
			new ButtonInfo { id = "visual_name_tags", buttonText = "Name Tags", method = Mods.NameTags, disableMethod = Mods.DisableNameTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show names above heads" },
			new ButtonInfo { id = "visual_fps_tags", buttonText = "FPS Name Tags", method = Mods.FPSTags, disableMethod = Mods.DisableFPSTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show FPS above heads" },
			new ButtonInfo { id = "visual_ars_tags", buttonText = "ARS Nametags", method = Mods.EnableARSNameTags, disableMethod = Mods.DisableARSNameTags, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show people on ARS" },
			new ButtonInfo { id = "visual_tracers", buttonText = "Tracers", method = Mods.Tracers, disableMethod = Mods.DisableTracers, enabled = false, type = ButtonType.FrameToggle, toolTip = "Lines towards everyone" },
			new ButtonInfo { id = "visual_box_esp", buttonText = "2D Box ESP", method = Mods.BoxEspRender, disableMethod = Mods.DisableBoxEsp, enabled = false, type = ButtonType.FrameToggle, toolTip = "Boxes around players" },
			new ButtonInfo { id = "visual_skeleton_esp", buttonText = "Skeleton ESP", method = Mods.SkeletonEsp, disableMethod = Mods.DisableSkeletonEsp, enabled = false, type = ButtonType.FrameToggle, toolTip = "Draw skeleton lines on players" },
			new ButtonInfo { id = "visual_third_person", buttonText = "3rd Person", method = Mods.EnableThirdPerson, disableMethod = Mods.DisableThirdPerson, enabled = false, type = ButtonType.FrameToggle, toolTip = "Third person view -- X to toggle" },
			new ButtonInfo { id = "visual_cosmetic_notifier", buttonText = "Cosmetic Notifier", method = Mods.CosmeticNotifier, disableMethod = Mods.DisableCosmeticNotifier, enabled = false, type = ButtonType.FrameToggle, toolTip = "The notis show who has a special cosmetics" }
		},

		new[] {
			new ButtonInfo { id = "misc_exit", buttonText = "Exit Misc Mods", method = () => MenuManager.Instance.ToggleCategory("Misc Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "misc_anti_name_ban", buttonText = "Anti Name Ban", enableMethod = Mods.AntiNameBan, disableMethod = Mods.DisableAntiNameBan, enabled = false, type = ButtonType.Toggle, toolTip = "Prevent name bans" },
			new ButtonInfo { id = "misc_anti_afk", buttonText = "Anti AFK", enableMethod = Mods.AntiAFK, disableMethod = Mods.DisableAntiAFK, enabled = false, type = ButtonType.Toggle, toolTip = "Prevent AFK kick" },
			new ButtonInfo { id = "misc_anti_guardian_grab", buttonText = "Anti Guardian Grab", enableMethod = Mods.AntiGuardianGrab, disableMethod = Mods.DisableAntiGuardianGrab, enabled = false, type = ButtonType.Toggle, toolTip = "Block guardian grab" },
			new ButtonInfo { id = "misc_disable_quit_box", buttonText = "Disable Quit Box", enableMethod = Mods.DisableQuitBox, disableMethod = Mods.EnableQuitBox, enabled = false, type = ButtonType.Toggle, toolTip = "Disable quit box" },
			new ButtonInfo { id = "misc_disable_net_triggers", buttonText = "Disable Network Triggers", enableMethod = Mods.DisableNetworkTriggers, disableMethod = Mods.EnableNetworkTriggers, enabled = false, type = ButtonType.Toggle, toolTip = "Change maps without leaving" },
			new ButtonInfo { id = "misc_block_jman", buttonText = "Block jman sounds", enableMethod = Mods.BlockJmanSounds, disableMethod = Mods.DisableBlockJmanSounds, enabled = false, type = ButtonType.Toggle, toolTip = "Block jman sounds" },
			new ButtonInfo { id = "misc_anti_block_crash", buttonText = "Anti Block Crash", enableMethod = Mods.AntiBlockCrash, disableMethod = Mods.DisableAntiBlockCrash, enabled = false, type = ButtonType.Toggle, toolTip = "doesn't load the blocks in monkey blocks" },
			new ButtonInfo { id = "misc_mute_gun", buttonText = "Mute Gun", method = Mods.MuteGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot to mute/unmute" },
			new ButtonInfo { id = "misc_ars", buttonText = "ARS", enableMethod = Mods.EnableARS, disableMethod = Mods.DisableARS, enabled = false, type = ButtonType.Toggle, toolTip = "Auto-report system" },
			new ButtonInfo { id = "misc_anti_report", buttonText = "Anti Report", enableMethod = Mods.EnableAntiReport, disableMethod = Mods.DisableAntiReport, enabled = false, type = ButtonType.Toggle, toolTip = "Disconnect if someone nears your report button" }
		},

		new[] {
			new ButtonInfo { id = "room_exit", buttonText = "Exit Room Mods", method = () => MenuManager.Instance.ToggleCategory("Room Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "room_join_random", buttonText = "Join Random Public", method = Mods.JoinRandomPublic, type = ButtonType.Action, toolTip = "Join a random public lobby" },
			new ButtonInfo { id = "room_join_mods", buttonText = "Join Code MODS", method = () => Mods.JoinCode("MODS"), type = ButtonType.Action, toolTip = "Join MODS room" },
			new ButtonInfo { id = "room_join_mod", buttonText = "Join Code MOD", method = () => Mods.JoinCode("MOD"), type = ButtonType.Action, toolTip = "Join MOD room" },
			new ButtonInfo { id = "room_join_chud", buttonText = "Join Code chud", method = () => Mods.JoinCode("chud"), type = ButtonType.Action, toolTip = "Join chud room" }
		},

		new[] {
			new ButtonInfo { id = "fun_exit", buttonText = "Exit Fun Mods", method = () => MenuManager.Instance.ToggleCategory("Fun Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "fun_unlock_vim", buttonText = "Unlock VIM/Subscription", enableMethod = Mods.UnlockVim, disableMethod = Mods.DisableUnlockVim, enabled = false, type = ButtonType.Toggle, toolTip = "Unlock VIM features" },
			new ButtonInfo { id = "fun_unlock_cosmetics", buttonText = "Unlock All Cosmetics", method = () => { Mods.UnlockAllCosmetics(); Chud.Backend.UnlockAllCosmeticsPatch.enabled = true; }, type = ButtonType.Action, toolTip = "Unlocks all cosmetics and lets you see others' Cosmetx cosmetics" },
			new ButtonInfo { id = "fun_tryon_all", buttonText = "SS tryon all cosmetics (Mirror)", enableMethod = Mods.EnableTryOnAll, disableMethod = Mods.DisableTryOnAll, enabled = false, type = ButtonType.Toggle, toolTip = "Fills worn slots with Tree Pin and cycles every other cosmetic in the mirror one at a time" },
			new ButtonInfo { id = "fun_remove_all", buttonText = "Remove all cosmetics (Mirror)", enableMethod = Mods.EnableRemoveAllCosmetics, disableMethod = Mods.DisableRemoveAllCosmetics, enabled = false, type = ButtonType.Toggle, toolTip = "Put on every worn cosmetic so they turn off" },
			new ButtonInfo { id = "fun_bitcrunch", buttonText = "Bitcrunch Mic", enableMethod = Mods.BitcrunchMic, disableMethod = Mods.DisableBitcrunchMic, enabled = false, type = ButtonType.Toggle, toolTip = "Makes ur mic sound bad" },
			new ButtonInfo { id = "fun_boop", buttonText = "Boop", method = Mods.Boop, disableMethod = Mods.DisableBoop, enabled = false, type = ButtonType.FrameToggle, toolTip = "Play's a noise when booping someone" },
			new ButtonInfo { id = "fun_getid_gun", buttonText = "GetPlayerID Gun", method = Mods.GetPlayerIDGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot to copy ID" },
			new ButtonInfo { id = "fun_lag_gun", buttonText = "Lag Gun", method = Mods.LagGun, disableMethod = Mods.StopLagGunFull, enabled = false, type = ButtonType.Gun, toolTip = "Lags whoever u shoot, not very good only works on quest" },
			new ButtonInfo { id = "fun_orbit_gun", buttonText = "Orbit Gun", method = Mods.OrbitGun, disableMethod = Mods.StopOrbitFull, enabled = false, type = ButtonType.Gun, toolTip = "T-pose orbit around player" },
			new ButtonInfo { id = "fun_paintbrawl_aimbot", buttonText = "Paintbrawl Aimbot", enableMethod = () => Chud.Backend.GetLaunchPatch.enabled = true, disableMethod = () => Chud.Backend.GetLaunchPatch.enabled = false, enabled = false, type = ButtonType.Toggle, toolTip = "Redirects your slingshot to the closest player" },
			new ButtonInfo { id = "fun_color_spaz", buttonText = "Random Color Spaz", method = Mods.RandomColorSpaz, disableMethod = Mods.DisableRandomColorSpaz, enabled = false, type = ButtonType.FrameToggle, toolTip = "Change colors fast" },
			new ButtonInfo { id = "fun_water_splash", buttonText = "Water Splash", method = Mods.WaterSplash, disableMethod = Mods.DisableWaterSplash, enabled = false, type = ButtonType.FrameToggle, toolTip = "Splash water from your hand" },
			new ButtonInfo { id = "fun_group_kick", buttonText = "Group kick all (Stump)", method = Mods.GroupKickAll, type = ButtonType.Action, toolTip = "Kick everyone in stump you will get kicked too but it will auto rejoin, only works in privates" },
			new ButtonInfo { id = "fun_get_id_self", buttonText = "Get ID Self", method = Mods.GetIDSelf, type = ButtonType.Action, toolTip = "Copy your ID" },
			new ButtonInfo { id = "fun_grab_bugs_all", buttonText = "Grab All Bugs", method = Mods.GrabAllBugs, disableMethod = Mods.DisableGrabAllBugs, enabled = false, type = ButtonType.FrameToggle, toolTip = "Grab all bugs with your hand -- Grab them first" },
			new ButtonInfo { id = "fun_grab_bug_green", buttonText = "Grab Green Bug", method = Mods.GrabGreenBug, disableMethod = Mods.DisableGrabGreenBug, enabled = false, type = ButtonType.FrameToggle, toolTip = "Grab Green Doug with grip from anywhere -- Grab them first" },
			new ButtonInfo { id = "fun_grab_bug_doug", buttonText = "Grab Doug the Bug", method = Mods.GrabDougBug, disableMethod = Mods.DisableGrabDougBug, enabled = false, type = ButtonType.FrameToggle, toolTip = "Grab Doug with grip from anywhere -- Grab them first" },
			new ButtonInfo { id = "fun_spaz_bugs", buttonText = "Spaz Bugs", method = Mods.SpazBugs, disableMethod = Mods.DisableSpazBugs, enabled = false, type = ButtonType.FrameToggle, toolTip = "Spaz the bugs between your hands -- Grab them first" },
			new ButtonInfo { id = "fun_lowercase_name", buttonText = "lowercase name", method = () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", "").ToLower(); PhotonNetwork.LocalPlayer.NickName = n; if (VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, type = ButtonType.Action, toolTip = "Make ur name lowercase" },
			new ButtonInfo { id = "fun_fps_spoof", buttonText = "FPS Spoofer", enableMethod = Mods.EnableFPSSpoof, disableMethod = Mods.DisableFPSSpoof, enabled = false, type = ButtonType.Toggle, toolTip = "Spoof your fps to other players" },
			new ButtonInfo { id = "fun_altcase_name", buttonText = "Random Capital Name", method = () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", ""); char[] c = n.ToCharArray(); for (int i = 0; i < c.Length; i++) c[i] = (i % 2 == 0) ? char.ToUpper(c[i]) : char.ToLower(c[i]); PhotonNetwork.LocalPlayer.NickName = new string(c); if (VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, type = ButtonType.Action, toolTip = "make ur name alternating case" }
		},

		new[] {
			new ButtonInfo { id = "rig_exit", buttonText = "Exit Rig Mods", method = () => MenuManager.Instance.ToggleCategory("Rig Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "rig_ghost", buttonText = "Ghost Monke", method = Mods.GhostMonke, disableMethod = Mods.DisableGhostMonke, enabled = false, type = ButtonType.FrameToggle, toolTip = "Press B to freeze your rig" },
			new ButtonInfo { id = "rig_invis", buttonText = "Invis Monke", method = Mods.InvisMonke, disableMethod = Mods.DisableInvisMonke, enabled = false, type = ButtonType.FrameToggle, toolTip = "Press A to be invisible" },
			new ButtonInfo { id = "rig_backflip", buttonText = "Backflip", enableMethod = Mods.EnableBackflip, disableMethod = Mods.DisableBackflip, enabled = false, type = ButtonType.Toggle, toolTip = "Press B" },
			new ButtonInfo { id = "rig_frontflip", buttonText = "Frontflip", enableMethod = Mods.EnableFrontflip, disableMethod = Mods.DisableFrontflip, enabled = false, type = ButtonType.Toggle, toolTip = "Press B" },
			new ButtonInfo { id = "rig_spinning_torso", buttonText = "Spinning Torso", enableMethod = Mods.EnableSpinningTorso, disableMethod = Mods.DisableSpinningTorso, enabled = false, type = ButtonType.Toggle, toolTip = "Makes your torso spin around" },
			new ButtonInfo { id = "rig_fake_fbt", buttonText = "Fake FBT", enableMethod = Mods.EnableFakeFBT, disableMethod = Mods.DisableFakeFBT, enabled = false, type = ButtonType.Toggle, toolTip = "Fake Full Body Tracking" },
			new ButtonInfo { id = "rig_dinnerbone", buttonText = "Dinnerbone", enableMethod = Mods.EnableDinnerbone, disableMethod = Mods.DisableDinnerbone, enabled = false, type = ButtonType.Toggle, toolTip = "Flip yourself upside down" },
			new ButtonInfo { id = "rig_natsuki", buttonText = "Natsuki Neck", enableMethod = Mods.EnableNatsukiNeck, disableMethod = Mods.DisableNatsukiNeck, enabled = false, type = ButtonType.Toggle, toolTip = "Snap your neck to the right" },
			new ButtonInfo { id = "rig_grab_rig", buttonText = "Grab Rig", method = Mods.GrabRig, disableMethod = Mods.DisableGrabRig, enabled = false, type = ButtonType.FrameToggle, toolTip = "Hold grip to grab your rig" },
			new ButtonInfo { id = "movement_copy_gun", buttonText = "Copy Movement Gun", method = Mods.CopyMovementGun, disableMethod = Mods.StopCopyMovementGunFull, enabled = false, type = ButtonType.Gun, toolTip = "Lock onto player and copy their movements" },
			new ButtonInfo { id = "rig_look_at_gun", buttonText = "Look At Gun", method = Mods.LookAtGun, disableMethod = Mods.StopLookAtGunFull, enabled = false, type = ButtonType.Gun, toolTip = "Makes your rigs stare at whoever Your gun is Shooting" }
		},

		new[] {
			new ButtonInfo { id = "infection_exit", buttonText = "Exit Infection Mods", method = () => MenuManager.Instance.ToggleCategory("Infection Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "infection_tag_gun", buttonText = "Tag Gun", method = Mods.TagGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Its tag gun" },
			new ButtonInfo { id = "infection_tag_all", buttonText = "Tag All", method = Mods.TagAll, disableMethod = Mods.DisableTagAll, enabled = false, type = ButtonType.FrameToggle, toolTip = "Tags everyone" },
			new ButtonInfo { id = "infection_tag_aura", buttonText = "Tag Aura", method = Mods.TagAura, disableMethod = Mods.DisableTagAura, enabled = false, type = ButtonType.FrameToggle, toolTip = "Auto-tag players around you" },
			new ButtonInfo { id = "infection_tag_aura_visual", buttonText = "Tag Aura Visual", method = Mods.TagAuraVisual, disableMethod = Mods.DisableTagAuraVisual, enabled = false, type = ButtonType.FrameToggle, toolTip = "Show aura range visual" }
		},

		new[] {
			new ButtonInfo { id = "master_exit", buttonText = "Exit Master Mods", method = () => MenuManager.Instance.ToggleCategory("Master Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "master_status", buttonText = "Not master client", method = null, enabled = false, type = ButtonType.Action, toolTip = "Your current master client status" },
			new ButtonInfo { id = "master_spaz_self", buttonText = "Spaz Self", method = Mods.SpazSelf, disableMethod = Mods.DisableSpazSelf, enabled = false, type = ButtonType.Toggle, toolTip = "Tag and untag urself", requiredGameMode = "Infection" },
			new ButtonInfo { id = "master_untag_self", buttonText = "Untag Self", method = Mods.UntagSelf, type = ButtonType.Action, toolTip = "untag urself", requiredGameMode = "Infection" },
			new ButtonInfo { id = "master_spaz_all", buttonText = "Spaz All", method = Mods.SpazAll, disableMethod = Mods.DisableSpazAll, enabled = false, type = ButtonType.Toggle, toolTip = "Tag and untag everyone", requiredGameMode = "Infection" },
			new ButtonInfo { id = "master_untag_gun", buttonText = "Untag Gun", method = Mods.UntagGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot infected players to untag them", requiredGameMode = "Infection" },
			new ButtonInfo { id = "master_break_guardian", buttonText = "Break Guardian", method = Mods.BreakGuardian, disableMethod = Mods.DisableBreakGuardian, enabled = false, type = ButtonType.Toggle, toolTip = "No Guardian??", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_guardian_self", buttonText = "Guardian Self", method = Mods.GuardianSelf, type = ButtonType.Action, toolTip = "Make yourself guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_unguardian_self", buttonText = "UnGuardian Self", method = Mods.UnguardianSelf, type = ButtonType.Action, toolTip = "Remove yourself from guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_guardian_gun", buttonText = "Guardian Gun", method = Mods.GuardianGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to make them guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_guardian_spaz_gun", buttonText = "Guardian Spaz Gun", method = Mods.GuardianSpazGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Lock onto a player to spaz their guardian state", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_unguardian_gun", buttonText = "Unguardian Gun", method = Mods.UnguardianGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to remove their guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { id = "master_paintbrawl_kill_all", buttonText = "Paint Brawl Kill All", method = Mods.PaintBrawlKillAll, type = ButtonType.Action, toolTip = "Kill everyone in paintbrawl", requiredGameMode = "Paintbrawl" },
			new ButtonInfo { id = "master_paintbrawl_kill_gun", buttonText = "Paint Brawl Kill Gun", method = Mods.PaintBrawlKillGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to kill them in paintbrawl", requiredGameMode = "Paintbrawl" }
		},

		new[] {
			new ButtonInfo { id = "console_exit", buttonText = "Exit Console Mods", method = () => MenuManager.Instance.ToggleCategory("Console Mods"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "console_kick_gun", buttonText = "Kick Gun", method = ConsoleMods.KickGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to kick them" },
			new ButtonInfo { id = "console_silent_kick_gun", buttonText = "Silent Kick Gun", method = ConsoleMods.SilentKickGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to silently kick them" },
			new ButtonInfo { id = "console_fling_gun", buttonText = "Fling Gun", method = ConsoleMods.FlingGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to fling them" },
			new ButtonInfo { id = "console_vibrate_gun", buttonText = "Vibrate Gun", method = ConsoleMods.VibrateGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot a player to vibrate their controllers" },
			new ButtonInfo { id = "console_lightning_gun", buttonText = "Lightning Gun", method = ConsoleMods.LightningGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Shoot to strike lightning" },
			new ButtonInfo { id = "console_jail_gun", buttonText = "Jail Gun", method = ConsoleMods.JailGun, disableMethod = ConsoleMods.JailGunOff, enabled = false, type = ButtonType.Gun, toolTip = "Trap players in a jail cell" },
			new ButtonInfo { id = "console_tpall_gun", buttonText = "TP All Gun", method = ConsoleMods.TPAllGun, disableMethod = Mods.CleanupGun, enabled = false, type = ButtonType.Gun, toolTip = "Teleport everyone to your aim point" },
			new ButtonInfo { id = "console_freeze_gun", buttonText = "Freeze Gun", method = ConsoleMods.FreezeGun.Fire, disableMethod = ConsoleMods.FreezeGun.Disable, enabled = false, type = ButtonType.Gun, toolTip = "Hit to freeze/unfreeze players" },
			new ButtonInfo { id = "console_scale_self", buttonText = "Scale Self", enableMethod = ConsoleMods.ScaleSelf.Enable, disableMethod = ConsoleMods.ScaleSelf.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Right trigger bigger, left trigger smaller" },
			new ButtonInfo { id = "console_admin_grab", buttonText = "Admin Grab", enableMethod = ConsoleMods.AdminGrab.Enable, disableMethod = ConsoleMods.AdminGrab.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Grab players with your hand" },
			new ButtonInfo { id = "console_admin_grab_all", buttonText = "Admin Grab All", enableMethod = ConsoleMods.AdminGrabAll.Enable, disableMethod = ConsoleMods.AdminGrabAll.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Grab all players at once no matter distance" },
			new ButtonInfo { id = "console_laser", buttonText = "Laser", enableMethod = ConsoleMods.Laser.Enable, disableMethod = ConsoleMods.Laser.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Toggle lasers from your hands" },
			new ButtonInfo { id = "console_kick_all", buttonText = "Kick All", method = ConsoleMods.KickAll, type = ButtonType.Action, toolTip = "Kick everyone from lobby" },
			new ButtonInfo { id = "console_karambit", buttonText = "Karambit", enableMethod = ConsoleMods.Karambit.Enable, disableMethod = ConsoleMods.Karambit.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Karambit" },
			new ButtonInfo { id = "console_rblx_carpet", buttonText = "Rblx Carpet", enableMethod = ConsoleMods.RblxCarpet.Enable, disableMethod = ConsoleMods.RblxCarpet.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Rblx Carpet" },
			new ButtonInfo { id = "console_mc_sword", buttonText = "MC Sword", enableMethod = ConsoleMods.McSword.Enable, disableMethod = ConsoleMods.McSword.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is MC Sword" },
			new ButtonInfo { id = "console_ban_hammer", buttonText = "Ban Hammer", enableMethod = ConsoleMods.BanHammer.Enable, disableMethod = ConsoleMods.BanHammer.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Ban Hammer" },
			new ButtonInfo { id = "console_roblox_sword", buttonText = "Roblox Sword", enableMethod = ConsoleMods.RobloxSword.Enable, disableMethod = ConsoleMods.RobloxSword.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Roblox Sword" },
			new ButtonInfo { id = "console_rainbow_sword", buttonText = "Rainbow Sword", enableMethod = ConsoleMods.RainbowSword.Enable, disableMethod = ConsoleMods.RainbowSword.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Rainbow Sword" },
			new ButtonInfo { id = "console_ender_sword", buttonText = "Weird Ender Sword", enableMethod = ConsoleMods.WeirdEnderSword.Enable, disableMethod = ConsoleMods.WeirdEnderSword.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Weird Ender Sword" },
			new ButtonInfo { id = "console_pistol", buttonText = "Pistol", enableMethod = ConsoleMods.Pistol.Enable, disableMethod = ConsoleMods.Pistol.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Pistol" },
			new ButtonInfo { id = "console_physics_gun", buttonText = "Physics Gun", enableMethod = ConsoleMods.PhysicsGun.Enable, disableMethod = ConsoleMods.PhysicsGun.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Physics Gun" },
			new ButtonInfo { id = "console_noli_star", buttonText = "Noli Star", enableMethod = ConsoleMods.NoliStar.Enable, disableMethod = ConsoleMods.NoliStar.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Noli Star" },
			new ButtonInfo { id = "console_bag", buttonText = "Bag", enableMethod = ConsoleMods.Bag.Enable, disableMethod = ConsoleMods.Bag.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Bag" },
			new ButtonInfo { id = "console_kormakur", buttonText = "Kormakur", enableMethod = ConsoleMods.Kormakur.Enable, disableMethod = ConsoleMods.Kormakur.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Kormakur" },
			new ButtonInfo { id = "console_coin", buttonText = "Coin", enableMethod = ConsoleMods.Coin.Enable, disableMethod = ConsoleMods.Coin.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Coin" },
			new ButtonInfo { id = "console_minos_plush", buttonText = "Minos Prime Plush", enableMethod = ConsoleMods.MinosPrime.Enable, disableMethod = ConsoleMods.MinosPrime.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Minos Prime Plush" },
			new ButtonInfo { id = "console_boombox", buttonText = "Boombox", enableMethod = ConsoleMods.Boombox.Enable, disableMethod = ConsoleMods.Boombox.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Boombox" },
			new ButtonInfo { id = "console_samsung", buttonText = "Samsung", enableMethod = ConsoleMods.Samsung.Enable, disableMethod = ConsoleMods.Samsung.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Samsung" },
			new ButtonInfo { id = "console_tv", buttonText = "TV", enableMethod = ConsoleMods.TV.Enable, disableMethod = ConsoleMods.TV.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is TV" },
			new ButtonInfo { id = "console_travis", buttonText = "Travis", enableMethod = ConsoleMods.Travis.Enable, disableMethod = ConsoleMods.Travis.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Travis" },
			new ButtonInfo { id = "console_travis_beach", buttonText = "Travis (Beach)", enableMethod = ConsoleMods.TravisBeach.Enable, disableMethod = ConsoleMods.TravisBeach.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Travis (Beach)" },
			new ButtonInfo { id = "console_travis_critters", buttonText = "Travis (Critters)", enableMethod = ConsoleMods.TravisCritters.Enable, disableMethod = ConsoleMods.TravisCritters.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Travis (Critters)" },
			new ButtonInfo { id = "console_travis_city", buttonText = "Travis (City)", enableMethod = ConsoleMods.TravisCity.Enable, disableMethod = ConsoleMods.TravisCity.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Travis (City)" },
			new ButtonInfo { id = "console_shreksophone", buttonText = "Shreksophone", enableMethod = ConsoleMods.Shreksophone.Enable, disableMethod = ConsoleMods.Shreksophone.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Shreksophone" },
			new ButtonInfo { id = "console_carti", buttonText = "Carti", enableMethod = ConsoleMods.Carti.Enable, disableMethod = ConsoleMods.Carti.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Carti" },
			new ButtonInfo { id = "console_cherry_bomb", buttonText = "Cherry Bomb", enableMethod = ConsoleMods.CherryBomb.Enable, disableMethod = ConsoleMods.CherryBomb.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "This is Cherry Bomb" },
			new ButtonInfo { id = "console_spoof", buttonText = "Console Spoof", enableMethod = Chud.Backend.Console.EnableConsoleSpoof, disableMethod = Chud.Backend.Console.DisableConsoleSpoof, enabled = false, type = ButtonType.Toggle, toolTip = "Spoof as gay furry femboy menu v69" },
			new ButtonInfo { id = "console_destroy_assets", buttonText = "Destroy All Assets", method = ConsoleMods.DestroyAllAssets, type = ButtonType.Action, toolTip = "Remove all spawned assets" },
			new ButtonInfo { id = "console_open_settings", buttonText = "Console Settings", method = () => MenuManager.Instance.ToggleCategory("Console Settings"), type = ButtonType.Action, toolTip = "Opens console settings" }
		},

		new[] {
			new ButtonInfo { id = "console_settings_exit", buttonText = "Exit Console Settings", method = () => MenuManager.Instance.ToggleCategory("Console Settings"), type = ButtonType.Action, toolTip = "Returns to the console page" },
			new ButtonInfo { id = "console_settings_kick_self", buttonText = "Allow Kick Self", enableMethod = ConsoleMods.AllowKickSelf.Enable, disableMethod = ConsoleMods.AllowKickSelf.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Allow other admins to kick/tp/fling you" },
			new ButtonInfo { id = "console_settings_tp_self", buttonText = "Allow Teleport Self", enableMethod = ConsoleMods.AllowTpSelf.Enable, disableMethod = ConsoleMods.AllowTpSelf.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Allow other admins to teleport you" },
			new ButtonInfo { id = "console_settings_detect", buttonText = "Detect Console Users", enableMethod = ConsoleMods.DetectConsoleUsers.Enable, disableMethod = ConsoleMods.DetectConsoleUsers.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Auto detect who has console" },
			new ButtonInfo { id = "console_settings_logging", buttonText = "Console Logging", enableMethod = ConsoleMods.ConsoleLogging.Enable, disableMethod = ConsoleMods.ConsoleLogging.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Log console commands, asset spawns, and errors to BepInEx + notification" },
			new ButtonInfo { id = "console_settings_no_indicator", buttonText = "No Admin Indicator", enableMethod = ConsoleMods.NoAdminIndicator.Enable, disableMethod = ConsoleMods.NoAdminIndicator.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Hide your admin crown" },
			new ButtonInfo { id = "console_settings_fullauto", buttonText = "Full Auto Pistol", enableMethod = ConsoleMods.FullAutoPistol.Enable, disableMethod = ConsoleMods.FullAutoPistol.Disable, enabled = false, type = ButtonType.Toggle, toolTip = "Toggle full auto mode for pistol" }
		},

		new[] {
			new ButtonInfo { id = "credits_exit", buttonText = "Exit Credits", method = () => MenuManager.Instance.ToggleCategory("Credits"), type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { id = "credits_jolyne", buttonText = "Jolyne/Sayori", method = () => Application.OpenURL("https://github.com/Plmokni00"), type = ButtonType.Action, toolTip = "Owners Github" },
			new ButtonInfo { id = "credits_ling", buttonText = "Ling-3.0-Flash-Fin-Free", method = () => NotifiLib.SendNotification("Ling-3.0-Flash-Fin-Free: Made this mod", 2), type = ButtonType.Action, toolTip = "Made this mod" },
			new ButtonInfo { id = "credits_industry", buttonText = "Industry", method = () => NotifiLib.SendNotification("Industry: ARS system by Industry", 2), type = ButtonType.Action, toolTip = "ARS system by Industry" }
		}
	};

	public static string CurrentCategoryName
	{
		get => MenuManager.Instance.CurrentCategoryName;
		set => MenuManager.Instance.CurrentCategoryName = value;
	}

	public static void Register()
	{
		for (int i = 0; i < categoryNames.Length; i++)
			MenuManager.Instance.AddCategory(categoryNames[i], buttons[i].ToList());

		AddSettingPage("Fly Speed", "fly_speed", "Settings", new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" }, Mods.SetFlySpeed, "Set fly speed to {0}");
		AddSettingPage("WASD Sense", "wasd_sense", "Settings", new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2", "2.25", "2.5", "2.75", "3" }, Mods.SetWASDFlyMouseSense, "Set WASD fly sensitivity to {0}");
		AddSettingPage("Speed Boost Settings", "speed_boost_opt", "Settings", Mods.SpeedBoostNames, Mods.SetSpeedBoostAmount, "Set speed boost to {0}");
		AddSettingPage("Pull Power", "pull_power_opt", "Settings", Mods.PullPowerNames, Mods.SetPullModPower, "Set pull mod strength to {0}");
		AddSettingPage("Notification Time", "notif_time", "Settings", new[] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" }, Mods.SetNotificationTime, "Notifications stay {0}");
		AddSettingPage("Tag Aura Range", "tag_aura_range", "Settings", new[] { "Off", "0.5m", "1m", "1.5m", "2m", "2.5m", "3m", "4m", "5m" }, Mods.SetTagAuraRange, "Set tag aura range to {0}");
		AddSettingPage("Anti Report Range", "antireport_range", "Settings", new[] { "0.25m", "0.35m", "0.5m", "0.7m", "1m", "1.25m", "1.5m", "2m" }, Mods.SetAntiReportRange, "Set anti-report detection range to {0}");
		AddSettingPage("Water Splash Speed", "splash_speed", "Settings", Mods.WaterSplashNames, Mods.SetWaterSplashSpeed, "Set water splash cooldown to {0}");
		AddSettingPage("Controller Predictions Settings", "controller_pred", "Settings", Mods.ControllerPredNames, Mods.SetControllerPrediction, "Set controller predictions");
		AddSettingPage("FPS Spoofer Settings", "fps_spoof_opt", "Settings", Mods.FPSSpoofValues.Select(v => v.ToString()).ToArray(), Mods.SetFPSSpoof, "Spoof {0} fps");
		AddSettingPage("Button Click Sound", "click_sound", "Settings", new[] { "Default button click", "Clicker trainer", "DDLC", "Minecraft Lever", "Skype" }, Mods.SetButtonClickSound, "Use {0} for button clicks");

		MenuManager.Instance.AddCategory("Soundboard", Mods.BuildSoundboardCategory());

		foreach (MenuCategory category in MenuManager.Instance.Categories)
			foreach (ButtonInfo button in category.Buttons)
				if (button.type == ButtonType.Action && !button.enabled.HasValue)
					button.enabled = false;
	}

	private static void AddSettingPage(string category, string idPrefix, string exitTarget, string[] options, Action<int> setter, string tipFormat = null, string[] tips = null)
	{
		var list = new List<ButtonInfo>
		{
			new ButtonInfo { id = idPrefix + "_exit", buttonText = "Exit " + category, method = () => MenuManager.Instance.ToggleCategory(exitTarget), type = ButtonType.Action, toolTip = "Returns to the settings page" }
		};
		for (int i = 0; i < options.Length; i++)
		{
			int idx = i;
			string label = options[i];
			string tip = tips != null ? tips[i] : string.Format(tipFormat, label);
			list.Add(new ButtonInfo { id = idPrefix + "_" + idx, buttonText = label, method = () => setter(idx), type = ButtonType.Action, toolTip = tip });
		}
		MenuManager.Instance.AddCategory(category, list);
	}
}
