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
                Button.Action("Join Discord", "Join the Chud Menu Discord",
                    () => Application.OpenURL("https://discord.gg/3kzkDTFbH7")),
                Nav("Settings", "Opens the settings tab", "Settings"),
                Nav("Enabled Mods", "Shows your enabled mods", MenuRegistry.EnabledModsCategory),
                Nav("Movement Mods", "Opens the movement mods", "Movement Mods"),
                Nav("Visual Mods", "Opens the visual mods", "Visual Mods"),
                Nav("Fun Mods", "Opens the fun mods", "Fun Mods"),
                Nav("Misc Mods", "Opens the misc mods", "Misc Mods"),
                Nav("Rig Mods", "Opens the rig mods", "Rig Mods"),
                Nav("Infection Mods", "Opens the infection mods", "Infection Mods"),
                Nav("Room Mods", "Opens the room mods", "Room Mods"),
                Nav("Master Mods", "Opens the master mods", MenuRegistry.MasterModsCategory),
                Nav("Soundboard", "Opens the soundboard", "Soundboard"),
                Nav("Credits", "Opens the credits", "Credits")
            });
        }

        private static Page SettingsPage()
        {
            return new Page("Settings", new List<ButtonInfo>
            {
                Nav("Exit Settings", "Returns to the main page", "Settings"),
                Nav("Menu Colors", "Opens the menu colors", "Menu Colors"),
                Nav("Fly Speed", "Opens the fly speed settings", "Fly Speed"),
                Nav("WASD Sense", "Opens the WASD fly sensitivity settings", "WASD Sense"),
                Nav("Speed Boost Settings", "Opens the speed boost settings", "Speed Boost Settings"),
                Nav("Pull Power", "Opens the pull mod power settings", "Pull Power"),
                Nav("Notification Time", "Opens the notification time settings", "Notification Time"),
                Nav("Tag Aura Range", "Opens the tag aura range settings", "Tag Aura Range"),
                Nav("Anti Report Range", "Opens the anti-report range settings", "Anti Report Range"),
                Nav("Water Splash Speed", "Opens the water splash speed settings", "Water Splash Speed"),
                Nav("Controller Predictions Settings", "Opens the controller predictions settings", "Controller Predictions Settings"),
                Nav("FPS Spoofer Settings", "Opens the FPS spoofer settings", "FPS Spoofer Settings"),
                Nav("Button Click Sound", "Opens the button click sound settings", "Button Click Sound"),
                Nav("Menu Layout", "Opens the menu layout settings", "Menu Layout"),

                Button.Toggle("Menu Animations",
                    "Toggle menu open/close and button press animations",
                    () => Chud.UI.WristMenu.AnimationsEnabled = true,
                    () => Chud.UI.WristMenu.AnimationsEnabled = false),
                Button.Toggle("Toggle Menu",
                    "Press button once to open, press again to close",
                    () => Chud.UI.WristMenu.ToggleMenu = true,
                    () => Chud.UI.WristMenu.ToggleMenu = false),
                Button.Toggle("Right Hand", "Move menu to right hand",
                    Mods.EnableRightHand, Mods.DisableRightHand),
                Button.Toggle("Show FPS", "Show FPS counter",
                    () => Chud.UI.WristMenu.ShowFPS = true,
                    () => Chud.UI.WristMenu.ShowFPS = false),
                Button.Toggle("Show Session Time", "Show session duration",
                    () => Chud.UI.WristMenu.ShowSessionTime = true,
                    () => Chud.UI.WristMenu.ShowSessionTime = false),
                Button.Toggle("No Mouse Lock",
                    "Prevent WASD fly from locking mouse on right click",
                    () => Mods.SetWASDFlyNoMouseLock(true),
                    () => Mods.SetWASDFlyNoMouseLock(false)),
                Button.Toggle("PC Guns", "Use guns with mouse",
                    Mods.EnablePCGuns, Mods.DisablePCGuns),
                Button.Toggle("PC Button Click", "Click buttons with mouse",
                    Mods.EnablePCButtonClick, Mods.DisablePCButtonClick),
                Button.Toggle("Toggle Notifications", "Show/hide notifications",
                    Mods.ToggleNotifications, Mods.DisableNotifications),
                Button.Action("Clear Notifications",
                    "Remove all on-screen notifications", Mods.ClearNotifications),

                Button.Toggle("Custom Boards",
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

                Button.Toggle("see anti cheat reports", "Show anti-cheat reports",
                    Mods.EnableSeeAntiCheatReports, Mods.DisableSeeAntiCheatReports)
            });
        }

        private static Page ColorsPage()
        {
            string[] names = Mods.MenuColorNames;
            var buttons = new List<ButtonInfo>(names.Length + 1)
            {
                Nav("Exit Menu Colors", "Returns to the settings page", "Menu Colors")
            };

            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                buttons.Add(Button.Action(
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
                Nav("Exit Movement Mods", "Returns to the main page", "Movement Mods"),
                Button.Frame("Fly", "Hold B", Mods.EnableFly, Mods.DisableFly),
                Button.Frame("Joystick Fly", "Fly with joystick", Mods.JoystickFly, Mods.DisableJoystickFly),
                Button.Frame("WASD Fly", "Fly with WASD keys", Mods.EnableWASDFly, Mods.DisableWASDFly),
                Button.Frame("Speed Boost", "Hold grip to run fast", Mods.SpeedBoost, Mods.DisableSpeedBoost),
                Button.Frame("No Gravity", "Disable gravity", Mods.NoGravity, Mods.DisableNoGravity),
                Button.Frame("Noclip", "Walk through walls", Mods.Noclip, Mods.NoclipOff),
                Button.Frame("Platforms", "Place platforms", Mods.Platforms),
                Button.Frame("Sticky Platforms", "Sticky ver of plats", Mods.StickyPlatforms),
                Button.Frame("Pull Mod", "Pull forward while gripping", Mods.PullMod),
                Button.Gun("TP Gun", "Shoot to teleport", Mods.TPGun, Mods.CleanupGun),
                Button.Action("Teleport to Stump", "Teleport to the forest stump", Mods.TeleportToSpawn),
                Button.Frame("Minos Prime", "Right B to jump, then Right A to slam", Mods.MinosPrime, Mods.DisableMinosPrime),
                Button.Toggle("Spider monke", "Walk on any surface you touch", Mods.EnableSpiderMonkey, Mods.DisableSpiderMonkey),
                Button.Toggle("Controller Predictions",
                    "Amplify your hand movement, everyone sees it", Mods.EnableControllerPredictions, Mods.DisableControllerPredictions)
            });
        }

        private static Page VisualPage()
        {
            return new Page("Visual Mods", new List<ButtonInfo>
            {
                Nav("Exit Visual Mods", "Returns to the main page", "Visual Mods"),
                Button.Frame("Cosmetic Name Tags", "Show cosmetics above heads", Mods.CosmeticNameTags, Mods.DisableCosmeticNameTags),
                Button.Frame("ID Name Tags", "Show IDs above heads", Mods.IDTags, Mods.DisableIDTags),
                Button.Frame("Platform Name Tags", "Show platform above heads", Mods.PlatformTags, Mods.DisablePlatformTags),
                Button.Frame("Name Tags", "Show names above heads", Mods.NameTags, Mods.DisableNameTags),
                Button.Frame("FPS Name Tags", "Show FPS above heads", Mods.FPSTags, Mods.DisableFPSTags),
                Button.Frame("ARS Nametags", "Show people on ARS", Mods.EnableARSNameTags, Mods.DisableARSNameTags),
                Button.Frame("Tracers", "Lines towards everyone", Mods.Tracers, Mods.DisableTracers),
                Button.Frame("2D Box ESP", "Boxes around players", Mods.BoxEspRender, Mods.DisableBoxEsp),
                Button.Frame("Skeleton ESP", "Draw skeleton lines on players", Mods.SkeletonEsp, Mods.DisableSkeletonEsp),
                Button.Frame("3rd Person", "Third person view -- X to toggle", Mods.EnableThirdPerson, Mods.DisableThirdPerson),
                Button.Frame("Cosmetic Notifier",
                    "The notis show who has a special cosmetics", Mods.CosmeticNotifier, Mods.DisableCosmeticNotifier)
            });
        }

        private static Page MiscPage()
        {
            return new Page("Misc Mods", new List<ButtonInfo>
            {
                Nav("Exit Misc Mods", "Returns to the main page", "Misc Mods"),
                Button.Toggle("Anti Name Ban", "Prevent name bans", Mods.AntiNameBan, Mods.DisableAntiNameBan),
                Button.Toggle("Anti AFK", "Prevent AFK kick", Mods.AntiAFK, Mods.DisableAntiAFK),
                Button.Toggle("Anti Guardian Grab", "Block guardian grab", Mods.AntiGuardianGrab, Mods.DisableAntiGuardianGrab),
                Button.Toggle("Disable Quit Box", "Disable quit box", Mods.DisableQuitBox, Mods.EnableQuitBox),
                Button.Toggle("Disable Network Triggers", "Change maps without leaving", Mods.DisableNetworkTriggers, Mods.EnableNetworkTriggers),
                Button.Toggle("Block jman sounds", "Block jman sounds", Mods.BlockJmanSounds, Mods.DisableBlockJmanSounds),
                Button.Toggle("Anti Block Crash", "doesn't load the blocks in monkey blocks", Mods.AntiBlockCrash, Mods.DisableAntiBlockCrash),
                Button.Gun("Mute Gun", "Shoot to mute/unmute", Mods.MuteGun, Mods.CleanupGun),
                Button.Toggle("ARS", "Auto-report system", Mods.EnableARS, Mods.DisableARS),
                Button.Toggle("Anti Report", "Disconnect if someone nears your report button", Mods.EnableAntiReport, Mods.DisableAntiReport)
            });
        }

        private static Page RoomPage()
        {
            return new Page("Room Mods", new List<ButtonInfo>
            {
                Nav("Exit Room Mods", "Returns to the main page", "Room Mods"),
                Button.Action("Join Random Public", "Join a random public lobby", Mods.JoinRandomPublic),
                JoinCodeButton("Join Code MODS", "Join MODS room", "MODS"),
                JoinCodeButton("Join Code MOD", "Join MOD room", "MOD"),
                JoinCodeButton("Join Code chud", "Join chud room", "chud"),
                JoinCodeButton("Join Code PIXEL", "Join PIXEL room", "PIXEL"),
                JoinCodeButton("Join Code MBEACHY", "Join MBEACHY room", "MBEACHY"),
                JoinCodeButton("Join Code CONTENT", "Join CONTENT room", "CONTENT"),
                JoinCodeButton("Join Code CREATOR", "Join CREATOR room", "CREATOR"),
                JoinCodeButton("Join Code FOGGY", "Join FOGGY room", "FOGGY"),
                JoinCodeButton("Join Code LUCIO", "Join LUCIO room", "LUCIO"),
                JoinCodeButton("Join Code PBBV", "Join PBBV room", "PBBV"),
                JoinCodeButton("Join Code ECHO", "Join ECHO room", "ECHO"),
                JoinCodeButton("Join Code RUN", "Join RUN room", "RUN")
            });
        }

        private static Page FunPage()
        {
            return new Page("Fun Mods", new List<ButtonInfo>
            {
                Nav("Exit Fun Mods", "Returns to the main page", "Fun Mods"),
                Button.Toggle("Unlock VIM/Subscription", "Unlock VIM features", Mods.UnlockVim, Mods.DisableUnlockVim),

                Button.Action("Unlock All Cosmetics",
                    "Unlocks all cosmetics and lets you see others' Cosmetx cosmetics",
                    () =>
                    {
                        Mods.UnlockAllCosmetics();
                        Chud.Patches.UnlockAllCosmeticsPatch.Enabled = true;
                    }),

                Button.Toggle("SS tryon all cosmetics (Mirror)",
                    "Fills worn slots with Tree Pin and cycles every other cosmetic in the mirror one at a time",
                    Mods.EnableTryOnAll, Mods.DisableTryOnAll),
                Button.Toggle("Remove all cosmetics (Mirror)",
                    "Put on every worn cosmetic so they turn off",
                    Mods.EnableRemoveAllCosmetics, Mods.DisableRemoveAllCosmetics),
                Button.Toggle("Bitcrunch Mic", "Makes ur mic sound bad", Mods.BitcrunchMic, Mods.DisableBitcrunchMic),
                Button.Frame("Boop", "Play's a noise when booping someone", Mods.Boop, Mods.DisableBoop),
                Button.Gun("GetPlayerID Gun", "Shoot to copy ID", Mods.GetPlayerIDGun, Mods.CleanupGun),
                Button.Gun("Orbit Gun", "T-pose orbit around player", Mods.OrbitGun, Mods.StopOrbitFull),
                Button.Toggle("Paintbrawl Aimbot",
                    "Redirects your slingshot to the closest player",
                    () => Chud.Patches.SlingshotAimbotPatch.Enabled = true,
                    () => Chud.Patches.SlingshotAimbotPatch.Enabled = false),
                Button.Frame("Random Color Spaz", "Change colors fast", Mods.RandomColorSpaz, Mods.DisableRandomColorSpaz),
                Button.Frame("Water Splash", "Splash water from your hand", Mods.WaterSplash, Mods.DisableWaterSplash),
                Button.Action("Group kick all (Stump)",
                    "Kick everyone in stump you will get kicked too but it will auto rejoin, only works in privates",
                    Mods.GroupKickAll),
                Button.Action("Get ID Self", "Copy your ID", Mods.GetIDSelf),
                Button.Frame("Grab All Bugs", "Grab all bugs with your hand -- Grab them first", Mods.GrabAllBugs, Mods.DisableGrabAllBugs),
                Button.Frame("Grab Green Bug", "Grab Green Doug with grip from anywhere -- Grab them first", Mods.GrabGreenBug, Mods.DisableGrabGreenBug),
                Button.Frame("Grab Doug the Bug", "Grab Doug with grip from anywhere -- Grab them first", Mods.GrabDougBug, Mods.DisableGrabDougBug),
                Button.Frame("Spaz Bugs", "Spaz the bugs between your hands -- Grab them first", Mods.SpazBugs, Mods.DisableSpazBugs),
                Button.Action("lowercase name", "Make ur name lowercase", Mods.MakeNameLowercase),
                Button.Toggle("FPS Spoofer", "Spoof your fps to other players", Mods.EnableFPSSpoof, Mods.DisableFPSSpoof),
                Button.Action("Random Capital Name", "make ur name alternating case", Mods.MakeNameAlternatingCase)
            });
        }

        private static Page RigPage()
        {
            return new Page("Rig Mods", new List<ButtonInfo>
            {
                Nav("Exit Rig Mods", "Returns to the main page", "Rig Mods"),
                Button.Frame("Ghost Monke", "Press B to freeze your rig", Mods.GhostMonke, Mods.DisableGhostMonke),
                Button.Frame("Invis Monke", "Press A to be invisible", Mods.InvisMonke, Mods.DisableInvisMonke),
                Button.Toggle("Backflip", "Press B", Mods.EnableBackflip, Mods.DisableBackflip),
                Button.Toggle("Frontflip", "Press B", Mods.EnableFrontflip, Mods.DisableFrontflip),
                Button.Toggle("Spinning Torso", "Makes your torso spin around", Mods.EnableSpinningTorso, Mods.DisableSpinningTorso),
                Button.Toggle("Fake FBT", "Fake Full Body Tracking", Mods.EnableFakeFBT, Mods.DisableFakeFBT),
                Button.Toggle("Dinnerbone", "Flip yourself upside down", Mods.EnableDinnerbone, Mods.DisableDinnerbone),
                Button.Toggle("Natsuki Neck", "Snap your neck to the right", Mods.EnableNatsukiNeck, Mods.DisableNatsukiNeck),
                Button.Frame("Grab Rig", "Hold grip to grab your rig", Mods.GrabRig, Mods.DisableGrabRig),
                Button.Gun("Copy Movement Gun", "Lock onto player and copy their movements", Mods.CopyMovementGun, Mods.StopCopyMovementGunFull),
                Button.Gun("Look At Gun", "Makes your rigs stare at whoever Your gun is Shooting", Mods.LookAtGun, Mods.StopLookAtGunFull)
            });
        }

        private static Page InfectionPage()
        {
            return new Page("Infection Mods", new List<ButtonInfo>
            {
                Nav("Exit Infection Mods", "Returns to the main page", "Infection Mods"),
                Button.Gun("Tag Gun", "Its tag gun", Mods.TagGun, Mods.CleanupGun),
                Button.Frame("Tag All", "Tags everyone", Mods.TagAll, Mods.DisableTagAll),
                Button.Frame("Tag Aura", "Auto-tag players around you", Mods.TagAura, Mods.DisableTagAura),
                Button.Frame("Tag Aura Visual", "Show aura range visual", Mods.TagAuraVisual, Mods.DisableTagAuraVisual)
            });
        }

        private static Page MasterPage()
        {
            return new Page(MenuRegistry.MasterModsCategory, new List<ButtonInfo>
            {
                Nav("Exit Master Mods", "Returns to the main page", MenuRegistry.MasterModsCategory),
                Button.Action("Not master client", "Your current master client status", null),

                Button.Toggle("Spaz Self", "Tag and untag urself", Mods.SpazSelf, Mods.DisableSpazSelf)
                    .RequiringMode("Infection"),
                Button.Action("Untag Self", "untag urself", Mods.UntagSelf)
                    .RequiringMode("Infection"),
                Button.Toggle("Spaz All", "Tag and untag everyone", Mods.SpazAll, Mods.DisableSpazAll)
                    .RequiringMode("Infection"),
                Button.Gun("Untag Gun", "Shoot infected players to untag them", Mods.UntagGun, Mods.CleanupGun)
                    .RequiringMode("Infection"),

                Button.Action("Paint Brawl Kill All", "Kill everyone in paintbrawl", Mods.PaintBrawlKillAll)
                    .RequiringMode("Paintbrawl"),
                Button.Gun("Paint Brawl Kill Gun", "Shoot a player to kill them in paintbrawl", Mods.PaintBrawlKillGun, Mods.CleanupGun)
                    .RequiringMode("Paintbrawl")
            });
        }

        private static Page ConsoleModsPage()
        {
            return new Page(MenuRegistry.ConsoleModsCategory, new List<ButtonInfo>
            {
                Nav("Exit Console Mods", "Returns to the main page", "Console Mods"),

                Button.Gun("Kick Gun", "Shoot a player to kick them", ConsoleMods.KickGun, Mods.CleanupGun),
                Button.Gun("Silent Kick Gun", "Shoot a player to silently kick them", ConsoleMods.SilentKickGun, Mods.CleanupGun),
                Button.Gun("Fling Gun", "Shoot a player to fling them", ConsoleMods.FlingGun, Mods.CleanupGun),
                Button.Gun("Vibrate Gun", "Shoot a player to vibrate their controllers", ConsoleMods.VibrateGun, Mods.CleanupGun),
                Button.Gun("Lightning Gun", "Shoot to strike lightning", ConsoleMods.LightningGun, Mods.CleanupGun),
                Button.Gun("Jail Gun", "Trap players in a jail cell", ConsoleMods.JailGun, ConsoleMods.JailGunOff),
                Button.Gun("TP All Gun", "Teleport everyone to your aim point", ConsoleMods.TPAllGun, Mods.CleanupGun),
                Button.Gun("Freeze Gun", "Hit to freeze/unfreeze players", ConsoleMods.FreezeGun.Fire, ConsoleMods.FreezeGun.Disable),

                Button.Toggle("Scale Self", "Right trigger bigger, left trigger smaller", ConsoleMods.ScaleSelf.Enable, ConsoleMods.ScaleSelf.Disable),
                Button.Toggle("Admin Grab", "Grab players with your hand", ConsoleMods.AdminGrab.Enable, ConsoleMods.AdminGrab.Disable),
                Button.Toggle("Admin Grab All", "Grab all players at once no matter distance", ConsoleMods.AdminGrabAll.Enable, ConsoleMods.AdminGrabAll.Disable),
                Button.Toggle("Laser", "Toggle lasers from your hands", ConsoleMods.Laser.Enable, ConsoleMods.Laser.Disable),
                Button.Action("Kick All", "Kick everyone from lobby", ConsoleMods.KickAll),

                AssetToggle("Karambit", ConsoleMods.Karambit.Enable, ConsoleMods.Karambit.Disable),
                AssetToggle("Rblx Carpet", ConsoleMods.RblxCarpet.Enable, ConsoleMods.RblxCarpet.Disable),
                AssetToggle("MC Sword", ConsoleMods.McSword.Enable, ConsoleMods.McSword.Disable),
                AssetToggle("Ban Hammer", ConsoleMods.BanHammer.Enable, ConsoleMods.BanHammer.Disable),
                AssetToggle("Roblox Sword", ConsoleMods.RobloxSword.Enable, ConsoleMods.RobloxSword.Disable),
                AssetToggle("Rainbow Sword", ConsoleMods.RainbowSword.Enable, ConsoleMods.RainbowSword.Disable),
                AssetToggle("Weird Ender Sword", ConsoleMods.WeirdEnderSword.Enable, ConsoleMods.WeirdEnderSword.Disable),
                AssetToggle("Pistol", ConsoleMods.Pistol.Enable, ConsoleMods.Pistol.Disable),
                AssetToggle("Physics Gun", ConsoleMods.PhysicsGun.Enable, ConsoleMods.PhysicsGun.Disable),
                AssetToggle("Noli Star", ConsoleMods.NoliStar.Enable, ConsoleMods.NoliStar.Disable),
                AssetToggle("Bag", ConsoleMods.Bag.Enable, ConsoleMods.Bag.Disable),
                AssetToggle("Kormakur", ConsoleMods.Kormakur.Enable, ConsoleMods.Kormakur.Disable),
                AssetToggle("Coin", ConsoleMods.Coin.Enable, ConsoleMods.Coin.Disable),
                AssetToggle("Minos Prime Plush", ConsoleMods.MinosPrime.Enable, ConsoleMods.MinosPrime.Disable),
                AssetToggle("Boombox", ConsoleMods.Boombox.Enable, ConsoleMods.Boombox.Disable),
                AssetToggle("Samsung", ConsoleMods.Samsung.Enable, ConsoleMods.Samsung.Disable),
                AssetToggle("TV", ConsoleMods.TV.Enable, ConsoleMods.TV.Disable),
                AssetToggle("Travis", ConsoleMods.Travis.Enable, ConsoleMods.Travis.Disable),
                AssetToggle("Travis (Beach)", ConsoleMods.TravisBeach.Enable, ConsoleMods.TravisBeach.Disable),
                AssetToggle("Travis (Critters)", ConsoleMods.TravisCritters.Enable, ConsoleMods.TravisCritters.Disable),
                AssetToggle("Travis (City)", ConsoleMods.TravisCity.Enable, ConsoleMods.TravisCity.Disable),
                AssetToggle("Shreksophone", ConsoleMods.Shreksophone.Enable, ConsoleMods.Shreksophone.Disable),
                AssetToggle("Carti", ConsoleMods.Carti.Enable, ConsoleMods.Carti.Disable),
                AssetToggle("Cherry Bomb", ConsoleMods.CherryBomb.Enable, ConsoleMods.CherryBomb.Disable),
                Button.Toggle("Console Spoof", "Spoof as gay furry femboy menu v69", Chud.Backend.Console.EnableConsoleSpoof, Chud.Backend.Console.DisableConsoleSpoof),
                Button.Action("Destroy All Assets", "Remove all spawned assets", ConsoleMods.DestroyAllAssets),
                Nav("Console Settings", "Opens console settings", "Console Settings")
            });
        }

        private static Page ConsoleSettingsPage()
        {
            return new Page(MenuRegistry.ConsoleSettingsCategory, new List<ButtonInfo>
            {
                Nav("Exit Console Settings", "Returns to the console page", "Console Settings"),
                Button.Toggle("Allow Kick Self",
                    "Allow other admins to kick/tp/fling you", ConsoleMods.AllowKickSelf.Enable, ConsoleMods.AllowKickSelf.Disable),
                Button.Toggle("Allow Teleport Self",
                    "Allow other admins to teleport you", ConsoleMods.AllowTpSelf.Enable, ConsoleMods.AllowTpSelf.Disable),
                Button.Toggle("Detect Console Users",
                    "Auto detect who has console", ConsoleMods.DetectConsoleUsers.Enable, ConsoleMods.DetectConsoleUsers.Disable),
                Button.Toggle("Console Logging",
                    "Log console commands, asset spawns, and errors to BepInEx + notification",
                    ConsoleMods.ConsoleLogging.Enable, ConsoleMods.ConsoleLogging.Disable),
                Button.Toggle("No Admin Indicator", "Hide your admin crown",
                    ConsoleMods.NoAdminIndicator.Enable, ConsoleMods.NoAdminIndicator.Disable),
                Button.Toggle("Full Auto Pistol", "Toggle full auto mode for pistol",
                    ConsoleMods.FullAutoPistol.Enable, ConsoleMods.FullAutoPistol.Disable),
                Button.Toggle("See Crown", "Show your own crown above your head",
                    () => Chud.Backend.Console.SeeOwnCrown = true,
                    () => Chud.Backend.Console.SeeOwnCrown = false)
            });
        }

        private static Page CreditsPage()
        {
            return new Page("Credits", new List<ButtonInfo>
            {
                Nav("Exit Credits", "Returns to the main page", "Credits"),
                Button.Action("Jolyne/Sayori", "Owners Github",
                    () => Application.OpenURL("https://github.com/Plmokni00")),
                Button.Action("Muse Spark 1.3 Free", "The AI i use",
                    () => NotifiLib.SendNotification("Muse Spark 1.3 Free: The AI i use", 2)),
                Button.Action("Space Bunny Free", "The AI i use",
                    () => NotifiLib.SendNotification("Space Bunny Free: The AI i use", 2)),
                Button.Action("Industry", "ARS system by Industry",
                    () => NotifiLib.SendNotification("Industry: ARS system by Industry", 2))
            });
        }

        #endregion

        #region Helpers

        private static ButtonInfo Nav(string text, string tip, string target)
        {
            return Button.Action(text, tip, () => UI.WristMenu.NavigateTo(target));
        }

        private static ButtonInfo JoinCodeButton(string text, string tip, string code)
        {
            return Button.Action(text, tip, () => Mods.JoinCode(code));
        }

        private static ButtonInfo AssetToggle(string name, Action enable, Action disable)
        {
            return Button.Toggle(name, "This is " + name, enable, disable);
        }

        #endregion
    }
}
