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
			new ButtonInfo { buttonText = "Join Discord", method = () => Application.OpenURL("https://discord.gg/dshwtjYVUr"), isTogglable = false, type = ButtonType.Action, toolTip = "Join the Chud Menu Discord" },
			new ButtonInfo { buttonText = "Settings", method = () => MenuManager.ToggleCategory("Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the settings tab" },
			new ButtonInfo { buttonText = "Enabled Mods", method = () => MenuManager.ToggleCategory("Enabled Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Shows your enabled mods" },
			new ButtonInfo { buttonText = "Movement Mods", method = () => MenuManager.ToggleCategory("Movement Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the movement mods" },
			new ButtonInfo { buttonText = "Visual Mods", method = () => MenuManager.ToggleCategory("Visual Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the visual mods" },
			new ButtonInfo { buttonText = "Fun Mods", method = () => MenuManager.ToggleCategory("Fun Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the fun mods" },
			new ButtonInfo { buttonText = "Misc Mods", method = () => MenuManager.ToggleCategory("Misc Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the misc mods" },
			new ButtonInfo { buttonText = "Rig Mods", method = () => MenuManager.ToggleCategory("Rig Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the rig mods" },
			new ButtonInfo { buttonText = "Infection Mods", method = () => MenuManager.ToggleCategory("Infection Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the infection mods" },
			new ButtonInfo { buttonText = "Room Mods", method = () => MenuManager.ToggleCategory("Room Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the room mods" },
			new ButtonInfo { buttonText = "Master Mods", method = () => MenuManager.ToggleCategory("Master Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the master mods" },
			new ButtonInfo { buttonText = "Soundboard", method = () => MenuManager.ToggleCategory("Soundboard"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the soundboard" },
			new ButtonInfo { buttonText = "Credits", method = () => MenuManager.ToggleCategory("Credits"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the credits" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Settings", method = () => MenuManager.ToggleCategory("Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Menu Colors", method = () => MenuManager.ToggleCategory("Menu Colors"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the menu colors" },
			new ButtonInfo { buttonText = "Fly Speed", method = () => MenuManager.ToggleCategory("Fly Speed"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the fly speed settings" },
			new ButtonInfo { buttonText = "WASD Sense", method = () => MenuManager.ToggleCategory("WASD Sense"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the WASD fly sensitivity settings" },
			new ButtonInfo { buttonText = "Speed Boost Settings", method = () => MenuManager.ToggleCategory("Speed Boost Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the speed boost settings" },
			new ButtonInfo { buttonText = "Pull Power", method = () => MenuManager.ToggleCategory("Pull Power"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the pull mod power settings" },
			new ButtonInfo { buttonText = "Notification Time", method = () => MenuManager.ToggleCategory("Notification Time"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the notification time settings" },
			new ButtonInfo { buttonText = "Tag Aura Range", method = () => MenuManager.ToggleCategory("Tag Aura Range"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the tag aura range settings" },
			new ButtonInfo { buttonText = "Anti Report Range", method = () => MenuManager.ToggleCategory("Anti Report Range"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the anti-report range settings" },
			new ButtonInfo { buttonText = "Water Splash Speed", method = () => MenuManager.ToggleCategory("Water Splash Speed"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the water splash speed settings" },
			new ButtonInfo { buttonText = "Controller Predictions Settings", method = () => MenuManager.ToggleCategory("Controller Predictions Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the controller predictions settings" },
			new ButtonInfo { buttonText = "FPS Spoofer Settings", method = () => MenuManager.ToggleCategory("FPS Spoofer Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens the FPS spoofer settings" },
			new ButtonInfo { buttonText = "Menu Animations", enableMethod = () => WristMenu.animationsEnabled = true, disableMethod = () => WristMenu.animationsEnabled = false, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Toggle menu open/close and button press animations" },
			new ButtonInfo { buttonText = "Toggle Menu", enableMethod = () => WristMenu.toggleMenu = true, disableMethod = () => WristMenu.toggleMenu = false, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Press button once to open, press again to close" },
			new ButtonInfo { buttonText = "Right Hand", enableMethod = Mods.EnableRightHand, disableMethod = Mods.DisableRightHand, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Move menu to right hand" },
			new ButtonInfo { buttonText = "Show FPS", enableMethod = () => WristMenu.showFPS = true, disableMethod = () => WristMenu.showFPS = false, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Show FPS counter" },
			new ButtonInfo { buttonText = "Show Session Time", enableMethod = () => WristMenu.showSessionTime = true, disableMethod = () => WristMenu.showSessionTime = false, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Show session duration" },
			new ButtonInfo { buttonText = "No Mouse Lock", enableMethod = () => Mods.SetWASDFlyNoMouseLock(true), disableMethod = () => Mods.SetWASDFlyNoMouseLock(false), enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Prevent WASD fly from locking mouse on right click" },
			new ButtonInfo { buttonText = "PC Guns", enableMethod = Mods.EnablePCGuns, disableMethod = Mods.DisablePCGuns, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Use guns with mouse" },
			new ButtonInfo { buttonText = "PC Button Click", enableMethod = Mods.EnablePCButtonClick, disableMethod = Mods.DisablePCButtonClick, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Click buttons with mouse" },
			new ButtonInfo { buttonText = "Toggle Notifications", enableMethod = Mods.ToggleNotifications, disableMethod = Mods.DisableNotifications, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Show/hide notifications" },
			new ButtonInfo { buttonText = "Clear Notifications", method = Mods.ClearNotifications, isTogglable = false, type = ButtonType.Action, toolTip = "Remove all on-screen notifications" },
			new ButtonInfo { buttonText = "Custom Boards", enableMethod = () => { WristMenu.customBoardsEnabled = true; WristMenu.customBoardsApplied = false; }, disableMethod = () => { WristMenu.customBoardsEnabled = false; WristMenu.customBoardsApplied = false; if (WristMenu.instance != null) WristMenu.instance.RestoreOriginalBoardText(); }, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Replace in-game message boards with custom text" },
			new ButtonInfo { buttonText = "see anti cheat reports", enableMethod = Mods.EnableSeeAntiCheatReports, disableMethod = Mods.DisableSeeAntiCheatReports, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Show anti-cheat reports" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Menu Colors", method = () => MenuManager.ToggleCategory("Menu Colors"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the settings page" },
			new ButtonInfo { buttonText = "Gray", method = () => Mods.SetMenuColor(0), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to gray" },
			new ButtonInfo { buttonText = "Blue", method = () => Mods.SetMenuColor(1), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to blue" },
			new ButtonInfo { buttonText = "Red", method = () => Mods.SetMenuColor(2), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to red" },
			new ButtonInfo { buttonText = "Orange", method = () => Mods.SetMenuColor(3), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to orange" },
			new ButtonInfo { buttonText = "Green", method = () => Mods.SetMenuColor(4), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to green" },
			new ButtonInfo { buttonText = "Cyan", method = () => Mods.SetMenuColor(5), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to cyan" },
			new ButtonInfo { buttonText = "Purple", method = () => Mods.SetMenuColor(6), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to purple" },
			new ButtonInfo { buttonText = "Magenta", method = () => Mods.SetMenuColor(7), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to magenta" },
			new ButtonInfo { buttonText = "Pink", method = () => Mods.SetMenuColor(8), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to pink" },
			new ButtonInfo { buttonText = "Brown", method = () => Mods.SetMenuColor(9), isTogglable = false, type = ButtonType.Action, toolTip = "Set menu color to brown" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Enabled Mods", method = () => MenuManager.ToggleCategory("Enabled Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Movement Mods", method = () => MenuManager.ToggleCategory("Movement Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Fly", enableMethod = Mods.EnableFly, disableMethod = Mods.DisableFly, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Hold B" },
			new ButtonInfo { buttonText = "Joystick Fly", enableMethod = Mods.JoystickFly, disableMethod = Mods.DisableJoystickFly, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Fly with joystick" },
			new ButtonInfo { buttonText = "WASD Fly", enableMethod = Mods.EnableWASDFly, disableMethod = Mods.DisableWASDFly, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Fly with WASD keys" },
			new ButtonInfo { buttonText = "Speed Boost", method = Mods.SpeedBoost, disableMethod = Mods.DisableSpeedBoost, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Hold grip to run fast" },
			new ButtonInfo { buttonText = "No Gravity", method = Mods.NoGravity, disableMethod = Mods.DisableNoGravity, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Disable gravity" },
			new ButtonInfo { buttonText = "Noclip", method = Mods.Noclip, disableMethod = Mods.NoclipOff, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Walk through walls" },
			new ButtonInfo { buttonText = "Platforms", method = Mods.Platforms, type = ButtonType.FrameToggle, enabled = false, isTogglable = true, toolTip = "Place platforms" },
			new ButtonInfo { buttonText = "Sticky Platforms", method = Mods.StickyPlatforms, type = ButtonType.FrameToggle, enabled = false, isTogglable = true, toolTip = "Sticky ver of plats" },
			new ButtonInfo { buttonText = "Pull Mod", method = Mods.PullMod, type = ButtonType.FrameToggle, enabled = false, isTogglable = true, toolTip = "Pull forward while gripping" },
			new ButtonInfo { buttonText = "TP Gun", method = Mods.TPGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot to teleport" },
			new ButtonInfo { buttonText = "Copy Movement Gun", method = Mods.CopyMovementGun, disableMethod = Mods.StopCopyMovementGunFull, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Lock onto player and copy their movements" },
			new ButtonInfo { buttonText = "Teleport to Stump", method = Mods.TeleportToSpawn, isTogglable = false, type = ButtonType.Action, toolTip = "Teleport to the forest stump" },
			new ButtonInfo { buttonText = "Minos Prime", method = Mods.MinosPrime, disableMethod = Mods.DisableMinosPrime, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Right B to jump, then Right A to slam" },
			new ButtonInfo { buttonText = "Spider monke", enableMethod = Mods.EnableSpiderMonkey, disableMethod = Mods.DisableSpiderMonkey, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Walk on any surface you touch" },
			new ButtonInfo { buttonText = "Controller Predictions", enableMethod = Mods.EnableControllerPredictions, disableMethod = Mods.DisableControllerPredictions, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Amplify your hand movement, everyone sees it" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Visual Mods", method = () => MenuManager.ToggleCategory("Visual Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Cosmetic Name Tags", method = Mods.CosmeticNameTags, disableMethod = Mods.DisableCosmeticNameTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show cosmetics above heads" },
			new ButtonInfo { buttonText = "ID Name Tags", method = Mods.IDTags, disableMethod = Mods.DisableIDTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show IDs above heads" },
			new ButtonInfo { buttonText = "Platform Name Tags", method = Mods.PlatformTags, disableMethod = Mods.DisablePlatformTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show platform above heads" },
			new ButtonInfo { buttonText = "Name Tags", method = Mods.NameTags, disableMethod = Mods.DisableNameTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show names above heads" },
			new ButtonInfo { buttonText = "FPS Name Tags", method = Mods.FPSTags, disableMethod = Mods.DisableFPSTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show FPS above heads" },
			new ButtonInfo { buttonText = "ARS Nametags", method = Mods.EnableARSNameTags, disableMethod = Mods.DisableARSNameTags, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show people on ARS" },
			new ButtonInfo { buttonText = "Tracers", method = Mods.Tracers, disableMethod = Mods.DisableTracers, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Lines towards everyone" },
			new ButtonInfo { buttonText = "2D Box ESP", method = Mods.BoxEspRender, disableMethod = Mods.DisableBoxEsp, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Boxes around players" },
			new ButtonInfo { buttonText = "Skeleton ESP", method = Mods.SkeletonEsp, disableMethod = Mods.DisableSkeletonEsp, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Draw skeleton lines on players" },
			new ButtonInfo { buttonText = "3rd Person", method = Mods.EnableThirdPerson, disableMethod = Mods.DisableThirdPerson, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Third person view -- X to toggle" },
			new ButtonInfo { buttonText = "Cosmetic Notifier", method = Mods.CosmeticNotifier, disableMethod = Mods.DisableCosmeticNotifier, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "The notis show who has a special cosmetics" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Misc Mods", method = () => MenuManager.ToggleCategory("Misc Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Anti Name Ban", enableMethod = Mods.AntiNameBan, disableMethod = Mods.DisableAntiNameBan, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Prevent name bans" },
			new ButtonInfo { buttonText = "Anti AFK", enableMethod = Mods.AntiAFK, disableMethod = Mods.DisableAntiAFK, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Prevent AFK kick" },
			new ButtonInfo { buttonText = "Anti Guardian Grab", enableMethod = Mods.AntiGuardianGrab, disableMethod = Mods.DisableAntiGuardianGrab, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Block guardian grab" },
			new ButtonInfo { buttonText = "Disable Quit Box", enableMethod = Mods.DisableQuitBox, disableMethod = Mods.EnableQuitBox, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Disable quit box" },
			new ButtonInfo { buttonText = "Disable Network Triggers", enableMethod = Mods.DisableNetworkTriggers, disableMethod = Mods.EnableNetworkTriggers, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Change maps without leaving" },
			new ButtonInfo { buttonText = "Block jman sounds", enableMethod = Mods.BlockJmanSounds, disableMethod = Mods.DisableBlockJmanSounds, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Block jman sounds" },
			new ButtonInfo { buttonText = "Anti Block Crash", enableMethod = Mods.AntiBlockCrash, disableMethod = Mods.DisableAntiBlockCrash, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "doesn't load the blocks in monkey blocks" },
			new ButtonInfo { buttonText = "Mute Gun", method = Mods.MuteGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot to mute/unmute" },
			new ButtonInfo { buttonText = "ARS", enableMethod = Mods.EnableARS, disableMethod = Mods.DisableARS, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Auto-report system" },
			new ButtonInfo { buttonText = "Anti Report", enableMethod = Mods.EnableAntiReport, disableMethod = Mods.DisableAntiReport, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Disconnect if someone nears your report button" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Room Mods", method = () => MenuManager.ToggleCategory("Room Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Join Random Public", method = Mods.JoinRandomPublic, isTogglable = false, type = ButtonType.Action, toolTip = "Join a random public lobby" },
			new ButtonInfo { buttonText = "Join Code MODS", method = () => Mods.JoinCode("MODS"), isTogglable = false, type = ButtonType.Action, toolTip = "Join MODS room" },
			new ButtonInfo { buttonText = "Join Code MOD", method = () => Mods.JoinCode("MOD"), isTogglable = false, type = ButtonType.Action, toolTip = "Join MOD room" },
			new ButtonInfo { buttonText = "Join Code chud", method = () => Mods.JoinCode("chud"), isTogglable = false, type = ButtonType.Action, toolTip = "Join chud room" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Fun Mods", method = () => MenuManager.ToggleCategory("Fun Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Unlock VIM/Subscription", enableMethod = Mods.UnlockVim, disableMethod = Mods.DisableUnlockVim, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Unlock VIM features" },
			new ButtonInfo { buttonText = "Unlock All Cosmetics", method = () => { Mods.UnlockAllCosmetics(); Chud.Backend.UnlockAllCosmeticsPatch.enabled = true; }, isTogglable = false, type = ButtonType.Action, toolTip = "Unlocks all cosmetics and lets you see others' Cosmetx cosmetics" },
			new ButtonInfo { buttonText = "SS tryon all cosmetics (Mirror)", enableMethod = Mods.EnableTryOnAll, disableMethod = Mods.DisableTryOnAll, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Fills worn slots with Tree Pin and cycles every other cosmetic in the mirror one at a time" },
			new ButtonInfo { buttonText = "Remove all cosmetics (Mirror)", enableMethod = Mods.EnableRemoveAllCosmetics, disableMethod = Mods.DisableRemoveAllCosmetics, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Put on every worn cosmetic so they turn off" },
			new ButtonInfo { buttonText = "Bitcrunch Mic", enableMethod = Mods.BitcrunchMic, disableMethod = Mods.DisableBitcrunchMic, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Makes ur mic sound bad" },
			new ButtonInfo { buttonText = "Boop", method = Mods.Boop, disableMethod = Mods.DisableBoop, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Play's a noise when booping someone" },
			new ButtonInfo { buttonText = "GetPlayerID Gun", method = Mods.GetPlayerIDGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot to copy ID" },
			new ButtonInfo { buttonText = "Lag Gun", method = Mods.LagGun, disableMethod = Mods.StopLagGunFull, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Lags whoever u shoot, not very good only works on quest" },
			new ButtonInfo { buttonText = "Orbit Gun", method = Mods.OrbitGun, disableMethod = Mods.StopOrbitFull, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "T-pose orbit around player" },
			new ButtonInfo { buttonText = "Paintbrawl Aimbot", enableMethod = () => Chud.Backend.GetLaunchPatch.enabled = true, disableMethod = () => Chud.Backend.GetLaunchPatch.enabled = false, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Redirects your slingshot to the closest player" },
			new ButtonInfo { buttonText = "Random Color Spaz", method = Mods.RandomColorSpaz, disableMethod = Mods.DisableRandomColorSpaz, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Change colors fast" },
			new ButtonInfo { buttonText = "Water Splash", method = Mods.WaterSplash, disableMethod = Mods.DisableWaterSplash, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Splash water from your hand" },
			new ButtonInfo { buttonText = "Group kick all (Stump)", method = Mods.GroupKickAll, isTogglable = false, type = ButtonType.Action, toolTip = "Kick everyone in stump you will get kicked too but it will auto rejoin, only works in privates" },
			new ButtonInfo { buttonText = "Get ID Self", method = Mods.GetIDSelf, isTogglable = false, type = ButtonType.Action, toolTip = "Copy your ID" },
			new ButtonInfo { buttonText = "Grab All Bugs", method = Mods.GrabAllBugs, disableMethod = Mods.DisableGrabAllBugs, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Grab all bugs with your hand -- Grab them first" },
			new ButtonInfo { buttonText = "Grab Green Bug", method = Mods.GrabGreenBug, disableMethod = Mods.DisableGrabGreenBug, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Grab Green Doug with grip from anywhere -- Grab them first" },
			new ButtonInfo { buttonText = "Grab Doug the Bug", method = Mods.GrabDougBug, disableMethod = Mods.DisableGrabDougBug, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Grab Doug with grip from anywhere -- Grab them first" },
			new ButtonInfo { buttonText = "Spawn Hoverboard", method = Mods.SpawnHoverboard, disableMethod = Mods.DisableSpawnHoverboard, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Press grip to spawn hoverboard" },
			new ButtonInfo { buttonText = "Spaz Bugs", method = Mods.SpazBugs, disableMethod = Mods.DisableSpazBugs, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Spaz the bugs between your hands -- Grab them first" },
			new ButtonInfo { buttonText = "lowercase name", method = () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", "").ToLower(); PhotonNetwork.LocalPlayer.NickName = n; if (VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, isTogglable = false, type = ButtonType.Action, toolTip = "Make ur name lowercase" },
			new ButtonInfo { buttonText = "FPS Spoofer", enableMethod = Mods.EnableFPSSpoof, disableMethod = Mods.DisableFPSSpoof, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Spoof your fps to other players" },
			new ButtonInfo { buttonText = "Random Capital Name", method = () => { if (PhotonNetwork.LocalPlayer != null) { string n = System.Text.RegularExpressions.Regex.Replace(PhotonNetwork.LocalPlayer.NickName, "<color[^>]*>", ""); n = n.Replace("</color>", ""); char[] c = n.ToCharArray(); for (int i = 0; i < c.Length; i++) c[i] = (i % 2 == 0) ? char.ToUpper(c[i]) : char.ToLower(c[i]); PhotonNetwork.LocalPlayer.NickName = new string(c); if (VRRig.LocalRig != null) VRRig.LocalRig.UpdateName(); } }, isTogglable = false, type = ButtonType.Action, toolTip = "make ur name alternating case" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Rig Mods", method = () => MenuManager.ToggleCategory("Rig Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Ghost Monke", method = Mods.GhostMonke, disableMethod = Mods.DisableGhostMonke, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Press B to freeze your rig" },
			new ButtonInfo { buttonText = "Invis Monke", method = Mods.InvisMonke, disableMethod = Mods.DisableInvisMonke, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Press A to be invisible" },
			new ButtonInfo { buttonText = "Backflip", enableMethod = Mods.EnableBackflip, disableMethod = Mods.DisableBackflip, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Press B" },
			new ButtonInfo { buttonText = "Frontflip", enableMethod = Mods.EnableFrontflip, disableMethod = Mods.DisableFrontflip, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Press B" },
			new ButtonInfo { buttonText = "Spinning Torso", enableMethod = Mods.EnableSpinningTorso, disableMethod = Mods.DisableSpinningTorso, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Makes your torso spin around" },
			new ButtonInfo { buttonText = "Fake FBT", enableMethod = Mods.EnableFakeFBT, disableMethod = Mods.DisableFakeFBT, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Fake Full Body Tracking" },
			new ButtonInfo { buttonText = "Dinnerbone", enableMethod = Mods.EnableDinnerbone, disableMethod = Mods.DisableDinnerbone, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Flip yourself upside down" },
			new ButtonInfo { buttonText = "Grab Rig", method = Mods.GrabRig, disableMethod = Mods.DisableGrabRig, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Hold grip to grab your rig" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Infection Mods", method = () => MenuManager.ToggleCategory("Infection Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Tag Gun", method = Mods.TagGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Its tag gun" },
			new ButtonInfo { buttonText = "Tag All", method = Mods.TagAll, disableMethod = Mods.DisableTagAll, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Tags everyone" },
			new ButtonInfo { buttonText = "Tag Aura", method = Mods.TagAura, disableMethod = Mods.DisableTagAura, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Auto-tag players around you" },
			new ButtonInfo { buttonText = "Tag Aura Visual", method = Mods.TagAuraVisual, disableMethod = Mods.DisableTagAuraVisual, enabled = false, isTogglable = true, type = ButtonType.FrameToggle, toolTip = "Show aura range visual" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Master Mods", method = () => MenuManager.ToggleCategory("Master Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Not master client", method = null, enabled = false, isTogglable = false, type = ButtonType.Action, toolTip = "Your current master client status" },
			new ButtonInfo { buttonText = "Spaz Self", method = Mods.SpazSelf, disableMethod = Mods.DisableSpazSelf, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Tag and untag urself", requiredGameMode = "Infection" },
			new ButtonInfo { buttonText = "Untag Self", method = Mods.UntagSelf, isTogglable = false, type = ButtonType.Action, toolTip = "untag urself", requiredGameMode = "Infection" },
			new ButtonInfo { buttonText = "Spaz All", method = Mods.SpazAll, disableMethod = Mods.DisableSpazAll, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Tag and untag everyone", requiredGameMode = "Infection" },
			new ButtonInfo { buttonText = "Untag Gun", method = Mods.UntagGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot infected players to untag them", requiredGameMode = "Infection" },
			new ButtonInfo { buttonText = "Break Guardian", method = Mods.BreakGuardian, disableMethod = Mods.DisableBreakGuardian, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "No Guardian??", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "Guardian Self", method = Mods.GuardianSelf, isTogglable = false, type = ButtonType.Action, toolTip = "Make yourself guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "UnGuardian Self", method = Mods.UnguardianSelf, isTogglable = false, type = ButtonType.Action, toolTip = "Remove yourself from guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "Guardian Gun", method = Mods.GuardianGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to make them guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "Guardian Spaz Gun", method = Mods.GuardianSpazGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Lock onto a player to spaz their guardian state", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "Unguardian Gun", method = Mods.UnguardianGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to remove their guardian", requiredGameMode = "Guardian" },
			new ButtonInfo { buttonText = "Paint Brawl Kill All", method = Mods.PaintBrawlKillAll, isTogglable = false, type = ButtonType.Action, toolTip = "Kill everyone in paintbrawl", requiredGameMode = "Paintbrawl" },
			new ButtonInfo { buttonText = "Paint Brawl Kill Gun", method = Mods.PaintBrawlKillGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to kill them in paintbrawl", requiredGameMode = "Paintbrawl" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Console Mods", method = () => MenuManager.ToggleCategory("Console Mods"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Kick Gun", method = ConsoleMods.KickGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to kick them" },
			new ButtonInfo { buttonText = "Silent Kick Gun", method = ConsoleMods.SilentKickGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to silently kick them" },
			new ButtonInfo { buttonText = "Fling Gun", method = ConsoleMods.FlingGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to fling them" },
			new ButtonInfo { buttonText = "Vibrate Gun", method = ConsoleMods.VibrateGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot a player to vibrate their controllers" },
			new ButtonInfo { buttonText = "Lightning Gun", method = ConsoleMods.LightningGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Shoot to strike lightning" },
			new ButtonInfo { buttonText = "Jail Gun", method = ConsoleMods.JailGun, disableMethod = ConsoleMods.JailGunOff, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Trap players in a jail cell" },
			new ButtonInfo { buttonText = "TP All Gun", method = ConsoleMods.TPAllGun, disableMethod = Mods.CleanupGun, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Teleport everyone to your aim point" },
			new ButtonInfo { buttonText = "Freeze Gun", method = ConsoleMods.FreezeGun.Fire, disableMethod = ConsoleMods.FreezeGun.Disable, enabled = false, isTogglable = true, type = ButtonType.Gun, toolTip = "Hit to freeze/unfreeze players" },
			new ButtonInfo { buttonText = "Scale Self", enableMethod = ConsoleMods.ScaleSelf.Enable, disableMethod = ConsoleMods.ScaleSelf.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Right trigger bigger, left trigger smaller" },
			new ButtonInfo { buttonText = "Admin Grab", enableMethod = ConsoleMods.AdminGrab.Enable, disableMethod = ConsoleMods.AdminGrab.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Grab players with your hand" },
			new ButtonInfo { buttonText = "Admin Grab All", enableMethod = ConsoleMods.AdminGrabAll.Enable, disableMethod = ConsoleMods.AdminGrabAll.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Grab all players at once no matter distance" },
			new ButtonInfo { buttonText = "Laser", enableMethod = ConsoleMods.Laser.Enable, disableMethod = ConsoleMods.Laser.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Toggle lasers from your hands" },
			new ButtonInfo { buttonText = "Kick All", method = ConsoleMods.KickAll, isTogglable = false, type = ButtonType.Action, toolTip = "Kick everyone from lobby" },
			new ButtonInfo { buttonText = "Karambit", enableMethod = ConsoleMods.Karambit.Enable, disableMethod = ConsoleMods.Karambit.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Karambit" },
			new ButtonInfo { buttonText = "Knife", enableMethod = ConsoleMods.Knife.Enable, disableMethod = ConsoleMods.Knife.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Knife" },
			new ButtonInfo { buttonText = "Rblx Carpet", enableMethod = ConsoleMods.RblxCarpet.Enable, disableMethod = ConsoleMods.RblxCarpet.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Rblx Carpet" },
			new ButtonInfo { buttonText = "MC Sword", enableMethod = ConsoleMods.McSword.Enable, disableMethod = ConsoleMods.McSword.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is MC Sword" },
			new ButtonInfo { buttonText = "Ban Hammer", enableMethod = ConsoleMods.BanHammer.Enable, disableMethod = ConsoleMods.BanHammer.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Ban Hammer" },
			new ButtonInfo { buttonText = "Roblox Sword", enableMethod = ConsoleMods.RobloxSword.Enable, disableMethod = ConsoleMods.RobloxSword.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Roblox Sword" },
			new ButtonInfo { buttonText = "Rainbow Sword", enableMethod = ConsoleMods.RainbowSword.Enable, disableMethod = ConsoleMods.RainbowSword.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Rainbow Sword" },
			new ButtonInfo { buttonText = "Weird Ender Sword", enableMethod = ConsoleMods.WeirdEnderSword.Enable, disableMethod = ConsoleMods.WeirdEnderSword.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Weird Ender Sword" },
			new ButtonInfo { buttonText = "Pistol", enableMethod = ConsoleMods.Pistol.Enable, disableMethod = ConsoleMods.Pistol.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Pistol" },
			new ButtonInfo { buttonText = "Physics Gun", enableMethod = ConsoleMods.PhysicsGun.Enable, disableMethod = ConsoleMods.PhysicsGun.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Physics Gun" },
			new ButtonInfo { buttonText = "Noli Star", enableMethod = ConsoleMods.NoliStar.Enable, disableMethod = ConsoleMods.NoliStar.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Noli Star" },
			new ButtonInfo { buttonText = "Bag", enableMethod = ConsoleMods.Bag.Enable, disableMethod = ConsoleMods.Bag.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Bag" },
			new ButtonInfo { buttonText = "Kormakur", enableMethod = ConsoleMods.Kormakur.Enable, disableMethod = ConsoleMods.Kormakur.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Kormakur" },
			new ButtonInfo { buttonText = "Coin", enableMethod = ConsoleMods.Coin.Enable, disableMethod = ConsoleMods.Coin.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Coin" },
			new ButtonInfo { buttonText = "Minos Prime Plush", enableMethod = ConsoleMods.MinosPrime.Enable, disableMethod = ConsoleMods.MinosPrime.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Minos Prime Plush" },
			new ButtonInfo { buttonText = "Boombox", enableMethod = ConsoleMods.Boombox.Enable, disableMethod = ConsoleMods.Boombox.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Boombox" },
			new ButtonInfo { buttonText = "Samsung", enableMethod = ConsoleMods.Samsung.Enable, disableMethod = ConsoleMods.Samsung.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Samsung" },
			new ButtonInfo { buttonText = "TV", enableMethod = ConsoleMods.TV.Enable, disableMethod = ConsoleMods.TV.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is TV" },
			new ButtonInfo { buttonText = "Travis", enableMethod = ConsoleMods.Travis.Enable, disableMethod = ConsoleMods.Travis.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Travis" },
			new ButtonInfo { buttonText = "Travis (Beach)", enableMethod = ConsoleMods.TravisBeach.Enable, disableMethod = ConsoleMods.TravisBeach.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Travis (Beach)" },
			new ButtonInfo { buttonText = "Travis (Critters)", enableMethod = ConsoleMods.TravisCritters.Enable, disableMethod = ConsoleMods.TravisCritters.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Travis (Critters)" },
			new ButtonInfo { buttonText = "Travis (City)", enableMethod = ConsoleMods.TravisCity.Enable, disableMethod = ConsoleMods.TravisCity.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Travis (City)" },
			new ButtonInfo { buttonText = "Shreksophone", enableMethod = ConsoleMods.Shreksophone.Enable, disableMethod = ConsoleMods.Shreksophone.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Shreksophone" },
			new ButtonInfo { buttonText = "Carti", enableMethod = ConsoleMods.Carti.Enable, disableMethod = ConsoleMods.Carti.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Carti" },
			new ButtonInfo { buttonText = "Cherry Bomb", enableMethod = ConsoleMods.CherryBomb.Enable, disableMethod = ConsoleMods.CherryBomb.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "This is Cherry Bomb" },
			new ButtonInfo { buttonText = "Console Spoof", enableMethod = Chud.Backend.Console.EnableConsoleSpoof, disableMethod = Chud.Backend.Console.DisableConsoleSpoof, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Spoof as Gay ass fucker menu v67" },
			new ButtonInfo { buttonText = "Destroy All Assets", method = ConsoleMods.DestroyAllAssets, isTogglable = false, type = ButtonType.Action, toolTip = "Remove all spawned assets" },
			new ButtonInfo { buttonText = "Console Settings", method = () => MenuManager.ToggleCategory("Console Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens console settings" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Console Settings", method = () => MenuManager.ToggleCategory("Console Settings"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the console page" },
			new ButtonInfo { buttonText = "Allow Kick Self", enableMethod = ConsoleMods.AllowKickSelf.Enable, disableMethod = ConsoleMods.AllowKickSelf.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Allow other admins to kick/tp/fling you" },
			new ButtonInfo { buttonText = "Allow Teleport Self", enableMethod = ConsoleMods.AllowTpSelf.Enable, disableMethod = ConsoleMods.AllowTpSelf.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Allow other admins to teleport you" },
			new ButtonInfo { buttonText = "Detect Console Users", enableMethod = ConsoleMods.DetectConsoleUsers.Enable, disableMethod = ConsoleMods.DetectConsoleUsers.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Auto detect who has console" },
			new ButtonInfo { buttonText = "Console Logging", enableMethod = ConsoleMods.ConsoleLogging.Enable, disableMethod = ConsoleMods.ConsoleLogging.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Log console commands, asset spawns, and errors to BepInEx + notification" },
			new ButtonInfo { buttonText = "No Admin Indicator", enableMethod = ConsoleMods.NoAdminIndicator.Enable, disableMethod = ConsoleMods.NoAdminIndicator.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Hide your admin crown" },
			new ButtonInfo { buttonText = "Full Auto Pistol", enableMethod = ConsoleMods.FullAutoPistol.Enable, disableMethod = ConsoleMods.FullAutoPistol.Disable, enabled = false, isTogglable = true, type = ButtonType.Toggle, toolTip = "Toggle full auto mode for pistol" },
			new ButtonInfo { buttonText = "Sound", method = () => MenuManager.ToggleCategory("Sound"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens sound" },
			new ButtonInfo { buttonText = "Video", method = () => MenuManager.ToggleCategory("Video"), isTogglable = false, type = ButtonType.Action, toolTip = "Opens video" }
		},

		new[] {
			new ButtonInfo { buttonText = "Exit Credits", method = () => MenuManager.ToggleCategory("Credits"), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the main page" },
			new ButtonInfo { buttonText = "Jolyne (Only menu owner/Maker)", method = () => NotifiLib.SendNotification("Jolyne: Menu owner", 2), isTogglable = false, type = ButtonType.Action, toolTip = "Menu owner" },
			new ButtonInfo { buttonText = "DeepSeek V4", method = () => NotifiLib.SendNotification("DeepSeek V4: Made most of the mods on the menu", 2), isTogglable = false, type = ButtonType.Action, toolTip = "Made most of the mods on the menu" },
			new ButtonInfo { buttonText = "Seralyth", method = () => NotifiLib.SendNotification("Seralyth: has skidded code from Seralyth", 2), isTogglable = false, type = ButtonType.Action, toolTip = "has skidded code from Seralyth" },
			new ButtonInfo { buttonText = "Industry", method = () => NotifiLib.SendNotification("Industry: ARS system by Industry", 2), isTogglable = false, type = ButtonType.Action, toolTip = "ARS system by Industry" }
		}
	};

	public static string CurrentCategoryName
	{
		get => MenuManager.CurrentCategoryName;
		set => MenuManager.CurrentCategoryName = value;
	}

	public static int CurrentCategoryIndex => System.Array.IndexOf(categoryNames, CurrentCategoryName);

	public static int buttonPages => buttons.Length;

	public static ButtonInfo GetIndex(string name)
	{
		foreach (ButtonInfo[] array in buttons)
		{
			foreach (ButtonInfo button in array)
			{
				if (button.buttonText == name)
					return button;
				if (button.aliases != null && System.Array.IndexOf(button.aliases, name) >= 0)
					return button;
			}
		}
		return null;
	}

	public static void Register()
	{
		for (int i = 0; i < categoryNames.Length; i++)
			MenuManager.AddCategory(categoryNames[i], buttons[i].ToList());

		AddSettingPage("Fly Speed", "Settings", new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" }, Mods.SetFlySpeed, "Set fly speed to {0}");
		AddSettingPage("WASD Sense", "Settings", new[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2", "2.25", "2.5", "2.75", "3" }, Mods.SetWASDFlyMouseSense, "Set WASD fly sensitivity to {0}");
		AddSettingPage("Speed Boost Settings", "Settings", Mods.SpeedBoostNames, Mods.SetSpeedBoostAmount, "Set speed boost to {0}");
		AddSettingPage("Pull Power", "Settings", Mods.PullPowerNames, Mods.SetPullModPower, "Set pull mod strength to {0}");
		AddSettingPage("Notification Time", "Settings", new[] { "1s", "1.5s", "2s", "2.5s", "3s", "4s", "5s", "6s", "8s", "10s" }, Mods.SetNotificationTime, "Notifications stay {0}");
		AddSettingPage("Tag Aura Range", "Settings", new[] { "Off", "0.5m", "1m", "1.5m", "2m", "2.5m", "3m", "4m", "5m" }, Mods.SetTagAuraRange, "Set tag aura range to {0}");
		AddSettingPage("Anti Report Range", "Settings", new[] { "0.25m", "0.35m", "0.5m", "0.7m", "1m", "1.25m", "1.5m", "2m" }, Mods.SetAntiReportRange, "Set anti-report detection range to {0}");
		AddSettingPage("Water Splash Speed", "Settings", Mods.WaterSplashNames, Mods.SetWaterSplashSpeed, "Set water splash cooldown to {0}");
		AddSettingPage("Controller Predictions Settings", "Settings", Mods.ControllerPredNames, Mods.SetControllerPrediction, "Set controller predictions");
		AddSettingPage("FPS Spoofer Settings", "Settings", Mods.FPSSpoofValues.Select(v => v.ToString()).ToArray(), Mods.SetFPSSpoof, "Spoof {0} fps");

		ConsoleMediaConfig.LoadConfig();
		MenuManager.AddCategory("Sound", ConsoleMods.BuildSoundCategory());
		MenuManager.AddCategory("Video", ConsoleMods.BuildVideoCategory());
		MenuManager.AddCategory("Soundboard", Mods.BuildSoundboardCategory());

		foreach (MenuCategory category in MenuManager.Categories)
			foreach (ButtonInfo button in category.Buttons)
				if (button.type == ButtonType.Action && !button.enabled.HasValue)
					button.enabled = false;
	}

	private static void AddSettingPage(string category, string exitTarget, string[] options, Action<int> setter, string tipFormat = null, string[] tips = null)
	{
		var list = new List<ButtonInfo>
		{
			new ButtonInfo { buttonText = "Exit " + category, method = () => MenuManager.ToggleCategory(exitTarget), isTogglable = false, type = ButtonType.Action, toolTip = "Returns to the settings page" }
		};
		for (int i = 0; i < options.Length; i++)
		{
			int idx = i;
			string label = options[i];
			string tip = tips != null ? tips[i] : string.Format(tipFormat, label);
			list.Add(new ButtonInfo { buttonText = label, method = () => setter(idx), isTogglable = false, type = ButtonType.Action, toolTip = tip });
		}
		MenuManager.AddCategory(category, list);
	}
}
