using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chud.Classes;
using Chud.UI;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using GTAG_NotificationLib;

namespace Chud.Backend;

public static class ConsoleMods
{
	public static int selectedSoundIndex = 0;
	public static int selectedVideoIndex = 0;

	public static string customSoundUrl = "";
	public static string customVideoUrl = "";

	public static int previousSoundIndex = 0;
	public static int previousVideoIndex = 0;

	private static readonly string oldSoundBase = "https://github.com/vhghfhnfgvbngv/plmokni/raw/refs/heads/main/";
	private static readonly string stupidBase = "https://github.com/OMalley1112/StupidTemplate/raw/master/";

	private static readonly string[] soundFiles = new string[]
	{
		"BIRDBRAIN.mp3",
		"JUST MONIKA_A DDLC Song.mp3",
		"minos-prime-talking.mp3",
		"Graboid vs Anaconda #graboid #tremors #vsedit #vsbattle #wis #wisedit #anaconda #whoisstronger - CityofEdits.mp3",
		"nastelbom-background-video-495891.mp3",
		"idk yo 6767.mp3"
	};
	public static readonly string[] soundNames = new string[]
	{
		"Birdbrain",
		"Just Monika",
		"Minos Prime",
		"Graboid vs Anaconda",
		"Nastelbom",
		"idk yo 6767",
		"Faster Stronger Better Gaben",
		"Gabe the halls",
		"Gaben (Baby)",
		"Du Bist Gut Genug",
		"Im a chud",
		"APT. COVER Sri Lanka",
		"Terranova"
	};

	public static string GetSoundUrl(int index)
	{
		if (index < 0) return customSoundUrl;
		if (index < soundFiles.Length)
			return oldSoundBase + Uri.EscapeDataString(soundFiles[index]);
		return soundDirectUrls[index - soundFiles.Length];
	}

	private static readonly string[] soundDirectUrls = new string[]
	{
		"https://github.com/Burty56/Console-Sounds/raw/main/Faster%20Stronger%20Better%20Gaben.wav",
		"https://github.com/Burty56/Console-Sounds/raw/main/Gabe%20the%20halls.wav",
		"https://github.com/Burty56/Console-Sounds/raw/main/Gaben%20(Baby).wav",
		"https://github.com/Burty56/Console-Sounds/raw/main/KITSCHKRIEG%20feat.%20BLUMENGARTEN%20%26%20SHIRIN%20DAVID%20-%20GUT%20GENUG%20-%20KITSCHKRIEG.mp3",
		"https://github.com/Burty56/Console-Sounds/raw/main/im%20a%20chud..%20im%20a%20chud%20-%20SomIlicito.mp3",
		"https://github.com/Burty56/Console-Sounds/raw/main/APT.%20COVER%20Sri%20Lanka🇱🇰%20-%20Pawthographyics.mp3",
		"https://github.com/vhghfhnfgvbngv/plmokni/raw/refs/heads/main/Terranova%20(Original%20Radio%20Mix).mp3"
	};

	private static readonly string[] videoFiles = new string[]
	{
		"505 💔😼‼️ - molo (720p, h264).mp4",
		"Baby ft. Barking Cat - DeksyTheCat (144p, h264).mp4",
		"Bonnie Tyler - Holding Out For A Hero (Lyrics).mp4",
		"Bro was aura farming…💀 Spinosaurus edit Jurassic park 3 also subscribe - PopularLBPZ (360p, h264).mp4",
		"Dinosaurs That Are Pure Evil, Good And Broken Jurassic World Edit #shorts #shortsfeed - UNIsH_Edits (144p, h264).mp4",
		"Don Toliver - Body [Official Visualizer].mp4",
		"Download (1).mp4",
		"Download (2).mp4",
		"Download (3).mp4",
		"Download (4).mp4",
		"Download (5).mp4",
		"Download.mp4",
		"El Reno tornado 2013 🌪️🔥 #edit #tornado #elreno #naturaldisasters - 🔥plackyDONS_Editz🔥 (720p, h264).mp4",
		"I Bought a House on Amazon - Unspeakable (144p, h264).mp4",
		"I Exposed My Viewers CRAZIEST Screentimes - Uncltrd Live (144p, h264).mp4",
		"I Survived 24 Hours In 2 Story Bus - Unspeakable (144p, h264).mp4",
		"I Survived 50 Hours Driving My School Bus - Unspeakable (144p, h264) (1).mp4",
		"I Survived 50 Hours Driving My School Bus - Unspeakable (144p, h264).mp4",
		"IM SENDING YOU STRAIGHT TO HELL! #edit #rdr2 #reddeadredemption - 𝐖𝐢𝐥𝐝𝐜𝐚𝐫𝐝 (720p, h264).mp4",
		"It can camouflage! Indominus Rex (Jurassic World) - DinoEdits15 (720p, h264).mp4",
		"JP The Lost World Edit Jurassic Sam #jurassicpark #jurassicworld #edit #4k #fyp - Jurassic Sam (144p, h264).mp4",
		"JP The Lost World Edit Jurassic Sam #jurassicpark #jurassicworld #edit #4k #fyp - Jurassic Sam (720p, h264).mp4",
		"JURASSIC WORLD REBIRTH EXPLAINED BY AN ASIAN - Korean Comic (144p, h264).mp4",
		"Jurassic world 2015 edit #jurassicworld #shorts #dinosaur #edit #jurassicpark - Scotty_Editzz (720p, h264).mp4",
		"LAST TO LEAVE BATHROOM WINS 10,000 CASH! - Unspeakable (144p, h264).mp4",
		"Ozzy Osbourne - Crazy Train [High Quality] - killler man (720p, h264).mp4",
		"Rexy Edit Jurassic Sam #rexy #jurassicpark #jurassicworldedit #edit #fyp #4k - Jurassic Sam (720p, h264).mp4",
		"Sinister Cat Edit #invincible #edit #shorts #popular - InvincibEdit (720p, h264).mp4",
		"Spinosaurus Ate Him... Jurassic Park Edit (4K) - HYPER EDITS (720p, h264).mp4",
		"Su-33 EDIT 🔥 #warthunder #gaijin #military #edit #viral - 𝐖𝐓𝐆𝐀𝐌𝐈𝐍𝐆 (360p, h264).mp4",
		"That One Dinosaur Edit - Okamiro (144p, h264) (1).mp4",
		"That One Dinosaur Edit - Okamiro (144p, h264).mp4",
		"There is no way the Eurofighter Typhoon actually did this! - Javiation24 (720p, h264).mp4",
		"Top 3 Strongest Raptors in Jurassic Park_World - GojiEdits15 (720p, h264).mp4",
		"Top 8 strongest dinos in JW Jurassic Sam #fyp #jurassicworld #jurassicpark #edit - Jurassic Sam (720p, h264).mp4",
		"Tremors edit #tremors #kevinbacon - TS-Bopittwist (144p, h264).mp4",
		"What if ☠️ #jurassicworld #jurassicpark #jurassic #edit #shorts #whatif #spinosaurus #indominusrex - Jurassic Matheo (720p, h264).mp4",
		"When Pennywise Got JUMPED For SMOKING Georgie - Crispy Boy (144p, h264).mp4",
		"Where Is My... Spinosaurus Trend #jurassicworld #trend #shorts #edit #dinosaur #fyp #viral - 𝐍𝐀𝐂𝐇𝐎 𝐄𝐃𝐈𝐓𝐒 ⚔️ (720p, h264).mp4",
		"Who has The most aura Jurassic Sam #jurasicworld #edit #4k #jurasicpark #fyp - Jurassic Sam (720p, h264).mp4",
		"tremors edit - Jace studios 299 (720p, h264).mp4",
		"welcome to Jurassic Park Jurassic Sam #edit #fyp #jurassicpark #jurassicworld - Jurassic Sam (144p, h264).mp4",
		"𝐌𝐎𝐍𝐓𝐀𝐆𝐄𝐌 𝐂𝐇𝐀𝐓𝐄𝐀𝐃𝐎 ( 𝐮𝐥𝐭𝐫𝐚 𝐬𝐥𝐨𝐰𝐞𝐝 + 𝐫𝐞𝐯𝐞𝐫𝐛 ) 𝙭 𝐂𝐚𝐭 𝐄𝐝𝐢𝐭 - 𝕴𝖈𝖍𝖎𝖌𝖔⚡︎ (144p, h264).mp4",
		"𝚃𝙷𝙰𝚃 𝚃𝙷𝙸𝙽𝙶'𝚂 𝙿𝙰𝚁𝚃 𝚁𝙰𝙿𝚃𝙾𝚁..🦖 𝙹𝚄𝚁𝙰𝚂𝚂𝙸𝙲 𝚆𝙾𝚁𝙻𝙳 𝙳𝙾𝙽'𝚃 𝚂𝚃𝙾𝙿 - 𝙶𝙻𝚇𝚇𝙼𝚂𝚃𝚁𝙸𝙳𝙴𝚁 (𝚂𝙻𝙾𝚆𝙴𝙳) #edit #shorts - A R Editz (720p, h264).mp4",
		"AM THE BEST CAT EDIT 🐈😻 #fypシ゚viral #fyp #cat #edit #editor #catedit #Didicook #sigmaedit - MoozyBoom137 (144p, h264).mp4",
		"CAT MEMES FAMILY ROAD TRIP STORY (THAILAND) - NO1 CAT MEMES (144p, h264) (1).mp4",
		"NOTION 💀🔥‼️‼️ - molo (720p, h264).mp4",
		"☠️DOGS VS CATS☠️ (THE ETERNAL BATTLE)#edit #aura #trollfaceedit #dog #cat #funny #anime #dogs #cats - 5-FEET-9 (720p, h264).mp4",
		"The Best Cat #cat #edit #rigby #best #fyp #fypシ゚viral #viral - 👑 8ur93r M4n 👑 (720p, h264).mp4",
		"Go Kitty Go ! - jai castellanos (144p, h264).mp4",
		"I LOVE MY CAT 🧡 #art #animationart #digitalart #cartoon #meme #cat - CHLOE SPARKLE (720p, h264).mp4"
	};
	public static readonly string[] videoNames = new string[]
	{
		"505 - molo",
		"Baby ft. Barking Cat",
		"Bonnie Tyler - Hero",
		"Bro was aura farming Spino",
		"Dinosaurs Pure Evil",
		"Don Toliver - Body",
		"Download (1)",
		"Download (2)",
		"Download (3)",
		"Download (4)",
		"Download (5)",
		"Download",
		"El Reno tornado 2013",
		"I Bought a House on Amazon",
		"I Exposed CRAZIEST Screentimes",
		"I Survived 24 Hours 2 Story Bus",
		"I Survived 50 Hours Bus (1)",
		"I Survived 50 Hours Bus",
		"SENDING YOU TO HELL! RDR2",
		"It can camouflage! I-Rex",
		"JP The Lost World (144p)",
		"JP The Lost World (720p)",
		"JW REBIRTH EXPLAINED",
		"Jurassic world 2015 edit",
		"LAST TO LEAVE BATHROOM",
		"Ozzy Osbourne - Crazy Train",
		"Rexy Edit Jurassic Sam",
		"Sinister Cat Edit",
		"Spinosaurus Ate Him",
		"Su-33 EDIT War Thunder",
		"That One Dinosaur Edit (1)",
		"That One Dinosaur Edit",
		"Eurofighter Typhoon",
		"Top 3 Raptors JW",
		"Top 8 strongest dinos JW",
		"Tremors edit",
		"What if JW",
		"Pennywise Got JUMPED",
		"Where Is My Spinosaurus",
		"Who has The most aura",
		"tremors edit - Jace",
		"welcome to Jurassic Park",
		"MONTAGEM CHATEADO",
		"THAT THING'S PART RAPTOR",
		"AM THE BEST CAT EDIT",
		"CAT MEMES ROAD TRIP",
		"NOTION",
		"DOGS VS CATS",
		"The Best Cat",
		"Go Kitty Go",
		"I Love My Cat",
		"Elliot Likes Femboys",
		"Dancing Monkeys",
		"Sky - Carti",
		"Over - Carti",
		"Rendezvous - Don Toliver",
		"wokeuplikethis* - Carti",
		"GPT Mod Menu - SoupVR",
		"Did you pray today?",
		"Zimble Mod Checker",
		"Crazy Russian Guy",
		"Tom Holland Moment",
		"Im a Korean",
		"ShibaGT Gold Rat",
		"USA Rat",
		"Press Option 1 Now",
		"Zimble Bad Boy",
		"Caramell Dansen",
		"Protect Your Shopping Trolley",
		"Theo Does Snacks",
		"ZlothY Locura",
		"Skidding is a Crime",
		"Rizz",
		"Shimmy Shimmy ya",
		"You got me jumping like",
		"Guardians of the Galaxy Vol 2",
		"Five Nights at Freddy's 2",
		"ep 1 rickandmorty",
		"The Amazing Spider-Man",
		"South Park",
		"Cat Memes Family",
		"Hachimi Funk",
		"GUT GENUG",
		"Thick Of It",
		"Miles Morales Edit",
		"MemeCompilation",
		"WhyYallPutCheeseOnMyCheeseburger",
		"bosnov see bunny",
		"plmokni video"
	};

	private static readonly string[] videoDirectUrls = new string[]
	{
		"https://files.hamburbur.org/ElliotLikesFemboys.mp4",
		"https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/monkeys_dancing.mp4",
		"https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/Playboi%20Cart%20-%20Sky.mp4",
		"https://files.hamburbur.org/Over-PlayboiCarti.mp4",
		"https://files.hamburbur.org/Rendezvous-DonToliver.mp4",
		"https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/REmZhFKmOmo.mp4",
		"https://files.hamburbur.org/gptmodmenu-soupvr.mp4",
		"https://files.hamburbur.org/didyoupraytoday.mp4",
		"https://files.hamburbur.org/zimblemodchecker.mov",
		"https://files.hamburbur.org/crazyrussianguy.mp4",
		"https://files.hamburbur.org/tomhollandmoment.mp4",
		"https://files.hamburbur.org/imakorean.mov",
		"https://files.hamburbur.org/shibagoldrat.mov",
		"https://files.hamburbur.org/usamenu.mp4",
		"https://files.hamburbur.org/gorilla-tag-gorilla.mp4",
		"https://files.hamburbur.org/zimblebadboy.mp4",
		"https://files.hamburbur.org/caramelldansen.mp4",
		"https://files.hamburbur.org/How%20to%20Protect%20Your%20Shopping%20Trolley%20From%20Improvised%20Explosives.mp4",
		"https://files.hamburbur.org/TheoDoesSnacks.mov",
		"https://files.hamburbur.org/ZlothYLocura.mov",
		"https://files.hamburbur.org/SkiddingIsACrime.mp4",
		"https://files.hamburbur.org/rizz.mp4",
		"https://files.hamburbur.org/shimmy%20shimmy%20ya%20but%20high%20quality%20(full).mp4",
		"https://files.hamburbur.org/YouGotMeJumpingLike.mov",
		"https://files.hamburbur.org/Guardians%20of%20the%20Galaxy%20Vol.%202%20(2017)%20(Awafim.tv).mp4",
		"https://files.hamburbur.org/FNaF2_UnityReady.mp4",
		"https://fmovs.online/Items/f91ca0b70d444ed017fe0a86cae12986/Download?api_key=d3da2a6ef25e4bf9953b50c818e1a669",
		"https://fmovs.online/Items/9732a76ae9cee1cfdedab3f5c9701b41/Download?api_key=586f5aad06d24392a2f24e6976287b5b",
		"https://fmovs.online/Items/e40d4c2e1dfbc062d14ca8588acaf4be/Download?api_key=586f5aad06d24392a2f24e6976287b5b",
		"https://github.com/Burty56/Console-Sounds/raw/main/%F0%9D%99%83%F0%9D%98%BC%F0%9D%98%BE%F0%9D%99%83%F0%9D%99%84%F0%9D%99%88%F0%9D%99%84%20%F0%9D%99%88%F0%9D%98%BC%F0%9D%99%88%F0%9D%98%BD%F0%9D%99%8A%20%F0%9D%99%81%F0%9D%99%90%F0%9D%99%89%F0%9D%99%86%20%20%F0%9D%99%86%F0%9D%99%84%F0%9D%99%89%F0%9D%99%82%F0%9D%98%BF%F0%9D%99%8A%F0%9D%99%88%20%F0%9D%99%AD%20%F0%9D%99%8E%F0%9D%99%91%F0%9D%99%89%F0%9D%99%8A%20-%20%F0%9D%98%BE%F0%9D%98%BC%F0%9D%99%8F%20%F0%9D%99%88%F0%9D%99%80%F0%9D%99%88%F0%9D%99%80%20%F0%9D%99%80%F0%9D%98%BF%F0%9D%99%84%F0%9D%99%8F.mp4",
		"https://github.com/Burty56/Console-Sounds/raw/main/KITSCHKRIEG%20feat.%20BLUMENGARTEN%20%26%20SHIRIN%20DAVID%20-%20GUT%20GENUG%20-%20KITSCHKRIEG.mp4",
		"https://github.com/Burty56/Console-Sounds/raw/main/KSI%20-%20Thick%20Of%20It%20(feat.%20Trippie%20Redd).mp4",
		"https://github.com/Burty56/Console-Sounds/raw/main/Miles%20Morales%20Edit.mp4",
		"https://github.com/Burty56/Menu-Stuff/raw/main/MemeCompilation.mp4",
		"https://github.com/Burty56/Menu-Stuff/raw/main/WhyYallPutCheeseOnMyCheeseburger.mp4",
		"https://github.com/Burty56/Menu-Stuff/raw/main/bosnov%20see%20bunny.mp4",
		"https://github.com/Burty56/Menu-Stuff/raw/main/relaxingAsmrs.mp4",
		"https://github.com/vhghfhnfgvbngv/plmokni/raw/refs/heads/main/v24044gl0000d8s5eevog65ur9io1k1g.mp4"
	};

	public static string GetVideoUrl(int index)
	{
		if (index < 0) return customVideoUrl;
		if (index < videoFiles.Length)
			return stupidBase + Uri.EscapeDataString(videoFiles[index]);
		return videoDirectUrls[index - videoFiles.Length];
	}

	// ====== Helpers that play locally once + sync to others (avoids double-handle from ReceiverGroup.All) ======
	private static void PlaySound(int id, string path, string clipName)
	{
		Console.ExecuteCommand("asset-playsound", ReceiverGroup.Others, id, path, clipName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playsound", id, path, clipName }, "asset-playsound");
	}

	private static void PlayAnimation(int id, string objectName, string animationName)
	{
		Console.ExecuteCommand("asset-playanimation", ReceiverGroup.Others, id, objectName, animationName);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "asset-playanimation", id, objectName, animationName }, "asset-playanimation");
	}

	private static void SendLaser(bool enabled, bool rightHand, float r, float g, float b)
	{
		Console.ExecuteCommand("laser", ReceiverGroup.Others, enabled, rightHand, r, g, b);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "laser", enabled, rightHand, r, g, b }, "laser");
	}

	public static void TPAllGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			Console.ExecuteCommand("tp", ReceiverGroup.Others, Mods.pointer.transform.position);
		});
	}

	private static void SendLaserColor(float r, float g, float b)
	{
		Console.ExecuteCommand("laserColor", ReceiverGroup.Others, r, g, b);
		Console.HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { "laserColor", r, g, b }, "laserColor");
	}

	// ====== Run Method (called every frame from UpdateActiveMods) ======
	public static void Run()
	{
		if (NoliStar.Enabled) NoliStar.Run();
		if (BanHammer.Enabled) BanHammer.Run();
		if (RainbowSword.Enabled) RainbowSword.Run();

		if (PhysicsGun.Enabled) PhysicsGun.Run();
		if (Laser.Enabled) Laser.Run();
		if (AdminGrab.Enabled) AdminGrab.Run();
		if (AdminGrabAll.Enabled) AdminGrabAll.Run();
		if (Pistol.Enabled) Pistol.Run();
		if (Coin.Enabled) Coin.Run();
		if (CherryBomb.Enabled) CherryBomb.Run();
		if (FreezeGun.Enabled) FreezeGun.Run();
		if (ScaleSelf.Enabled) ScaleSelf.Run();
		if (Knife.Enabled) Knife.Run();
	}

	// ====== Helpers ======
	public static void DestroyAsset(ref int id)
	{
		if (id >= 0)
		{
			Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, id);
			if (Console.ConsoleAssets.TryGetValue(id, out var asset))
			{
				asset.DestroyObject();
				Console.ConsoleAssets.Remove(id);
			}
			id = -1;
		}
	}

	// ====== NoliStar ======
	public static class NoliStar
	{
		public static bool Enabled;
		private static int id = -1;
		private static int musicId = -1;
		private static float updateDelay;
		private static float respawnTime;
		private static bool holdingTrigger;
		private static Vector3 throwDirection;
		private static Vector3 networkedPos;
		private static Quaternion networkedRot;
		private static int state; // 0=Default, 1=Throwing, 2=Respawning

		public static void Enable()
		{
			if (Enabled) return;
			Enabled = true;
			id = -1;
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			if (musicId >= 0)
			{
				Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, musicId);
				musicId = -1;
			}
			state = 0;
			holdingTrigger = false;
			updateDelay = 0f;
			respawnTime = 0f;
		}

		public static void Run()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Star", delegate(int assetId)
				{
					PlaySound(assetId, "Model", "StarSpawn");
				}));
			}
			if (!Console.ConsoleAssets.TryGetValue(id, out var starAsset) || starAsset.obj == null)
				return;
			GameObject starObj = starAsset.obj;
			ControllerInputPoller poller = ControllerInputPoller.instance;
			float noliTrigger = poller.rightControllerIndexFloat;
			if (noliTrigger > 0.5f && state == 0)
			{
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				GameObject noliCrosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				noliCrosshair.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				noliCrosshair.transform.position = (noliRay.point == Vector3.zero) ? (noliRay.transform.position + noliRay.transform.forward * 20f) : noliRay.point;
				noliCrosshair.GetComponent<Renderer>().material.color = Color.white;
				Object.Destroy(noliCrosshair, Time.deltaTime);
				Object.Destroy(noliCrosshair.GetComponent<Collider>());
			}
			if (noliTrigger < 0.5f && holdingTrigger && state == 0)
			{
				state = 1;
				PlayAnimation(id, "Model", "Throw");
				PlaySound(id, "Model", "ThrowStar");
				Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out RaycastHit noliDirRay, 512f, GTPlayer.Instance.locomotionEnabledLayers);
				throwDirection = (noliDirRay.point - starObj.transform.position).normalized;
			}
			holdingTrigger = noliTrigger > 0.5f;
			switch (state)
			{
			case 0:
				starObj.transform.position = GorillaTagger.Instance.rightHandTransform.position + Vector3.up * 0.2f;
				starObj.transform.rotation = Quaternion.Euler(Time.time * 32f, Time.time * 10f, Time.time * 47f);
				break;
			case 1:
			{
				Physics.Raycast(starObj.transform.position, throwDirection, out RaycastHit noliHitRay, 0.5f, GTPlayer.Instance.locomotionEnabledLayers);
				if (noliHitRay.point == Vector3.zero)
				{
					starObj.transform.position += throwDirection * (Time.deltaTime * 15f);
					starObj.transform.rotation = Quaternion.Euler(Time.time * 239f, Time.time * 201f, Time.time * 170f);
				}
				else
				{
					PlayAnimation(id, "Model", "Explode");
					bool noliKill = false;
					foreach (VRRig nRig in VRRigCache.ActiveRigs)
					{
						if (!nRig.isLocal && Vector3.Distance(starObj.transform.position, nRig.transform.position) < 2.32775f && nRig.Creator != null)
						{
							Player nPlayer = Console.GetPlayerFromID(nRig.Creator.UserId);
							if (nPlayer != null) Console.ExecuteCommand("silkick", nPlayer.ActorNumber, nPlayer.UserId);
							noliKill = true;
						}
					}
					PlaySound(id, "Model", noliKill ? "KillStar" : "BreakStar");
					state = 2;
					respawnTime = Time.time + 3f;
				}
				break;
			}
			case 2:
				if (Time.time > respawnTime)
				{
					PlayAnimation(id, "Model", "Default");
					PlaySound(id, "Model", "StarSpawn");
					state = 0;
				}
				break;
			}
			if (Time.time > updateDelay && (networkedRot != starObj.transform.rotation || networkedPos != starObj.transform.position))
			{
				updateDelay = Time.time + 0.05f;
				networkedPos = starObj.transform.position;
				networkedRot = starObj.transform.rotation;
			Console.ExecuteCommand("asset-setposition", ReceiverGroup.Others, id, starObj.transform.position);
			Console.ExecuteCommand("asset-setrotation", ReceiverGroup.Others, id, starObj.transform.rotation);
			}
		}
	}

	// ====== BanHammer ======
	public static class BanHammer
	{
		public static bool Enabled;
		public static int id = -1;
		private static float slashDelayBH;
		private static float pauseSfxBH;
		private static bool lastVelTooHighBH;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "banhammer", "BanHammer", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}, addSurfaceOverride: true));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelayBH = 0f;
			pauseSfxBH = 0f;
			lastVelTooHighBH = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var bhAsset) || bhAsset.obj == null)
				return;
			Transform bhRayPoint = bhAsset.obj.transform.Find("Model/HitBox");
			if (bhRayPoint == null)
				return;
			if (!bhRayPoint.TryGetComponent(out MeshCollider _))
				bhRayPoint.gameObject.AddComponent<MeshCollider>();
			Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhRay, 0.4f, Mods.GetNoInvisLayerMask());
			Physics.SphereCast(bhRayPoint.position, 0.2f, bhRayPoint.forward, out RaycastHit bhCRay, 0.4f, GTPlayer.Instance.locomotionEnabledLayers);
			Vector3 bhHandVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
			Vector3 bhBodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
			bool bhVelTooHigh = (bhHandVel - bhBodyVel).magnitude > 10f;
			if (Time.time > slashDelayBH)
			{
				if (bhRay.collider != null)
				{
					VRRig bhTarget = bhRay.collider.GetComponentInParent<VRRig>();
					if (bhTarget != null && !bhTarget.isLocal)
					{
						slashDelayBH = Time.time + 1f;
						pauseSfxBH = Time.time + 1f;
						Console.instance.StartCoroutine(KillFX());
						NetPlayer bhPlayer = bhTarget.Creator;
						Console.ExecuteCommand("silkick", bhPlayer.ActorNumber, bhPlayer.UserId);
					}
				}
				if (bhCRay.collider != null)
				{
					slashDelayBH = Time.time + 0.3f;
					pauseSfxBH = Time.time + 0.5f;
					float bhTotalVel = bhHandVel.magnitude + bhBodyVel.magnitude;
					GorillaTagger.Instance.rigidbody.linearVelocity += bhCRay.normal * Mathf.Clamp(bhTotalVel, 1f, 14f);
					Console.instance.StartCoroutine(HitFX());
				}
			}
			if (bhVelTooHigh && !lastVelTooHighBH && Time.time > pauseSfxBH)
			{
				pauseSfxBH = Time.time + 0.3f;
				PlaySound(id, "Model/SwingSFX", "Swing");
			}
			lastVelTooHighBH = bhVelTooHigh;
		}

		private static IEnumerator KillFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/KillSFX", "HammerKill");
			PlayAnimation(id, "Model", "HitPlayer");
		}

		private static IEnumerator HitFX()
		{
			PlayAnimation(id, "Model", "Default");
			yield return null;
			yield return null;
			PlaySound(id, "Model/SwingSFX", "HammerHit");
			PlayAnimation(id, "Model", "HitGround");
			foreach (VRRig rig in VRRigCache.ActiveRigs)
			{
				if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.transform.position) < 2f)
					Console.ExecuteCommand("vel", rig.Creator.ActorNumber, (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 5f);
			}
		}
	}

	// ====== RainbowSword ======
	public static class RainbowSword
	{
		public static bool Enabled;
		public static int id = -1;
		private static float slashDelayRS;
		private static float pauseSfxRS;
		private static bool lastVelTooHighRS;

		public static void Enable()
		{
			if (id >= 0) return;
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "rbsword", "Sword", delegate(int assetId)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}, addSurfaceOverride: true));
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			slashDelayRS = 0f;
			pauseSfxRS = 0f;
			lastVelTooHighRS = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var rsAsset) || rsAsset.obj == null)
				return;
			Transform rsRayPoint = rsAsset.obj.transform.Find("Sword/HitBox");
			if (rsRayPoint == null)
				return;
			Physics.SphereCast(rsRayPoint.position, 0.1f, rsRayPoint.forward, out RaycastHit rsRay, 0.7f, Mods.GetNoInvisLayerMask());
			if (Time.time > slashDelayRS && rsRay.collider != null)
			{
				try
				{
					VRRig rsTarget = rsRay.collider.GetComponentInParent<VRRig>();
					if (rsTarget != null && !rsTarget.isLocal && rsTarget.Creator != null)
					{
						slashDelayRS = Time.time + 0.5f;
						pauseSfxRS = Time.time + 1f;
						PlaySound(id, "Sword/SFX", "Slash" + Random.Range(1, 3));
						PlayAnimation(id, "Sword", "Particles");
						Player rsPlayer = Console.GetPlayerFromID(rsTarget.Creator.UserId);
						if (rsPlayer != null) Console.ExecuteCommand("silkick", rsPlayer.ActorNumber, rsPlayer.UserId);
					}
				}
				catch
				{
				}
			}
			Vector3 rsHandVel = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
			Vector3 rsBodyVel = GorillaTagger.Instance.rigidbody.linearVelocity;
			bool rsVelTooHigh = (rsHandVel - rsBodyVel).magnitude > 10f;
			if (rsVelTooHigh && !lastVelTooHighRS && Time.time > pauseSfxRS)
			{
				pauseSfxRS = Time.time + 0.3f;
				PlaySound(id, "Sword/SFX", "Swing" + Random.Range(1, 3));
			}
			lastVelTooHighRS = rsVelTooHigh;
		}
	}

	// ====== WeirdEnderSword ======
	public static class WeirdEnderSword
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable()
		{
			if (id >= 0) return;
			Console.CustomBundleURLs["rgbendersword"] = "https://github.com/Seralyth/Console/raw/refs/heads/master/ServerData/rgbendersword";
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "rgbendersword", "sword", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
			Enabled = true;
		}
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== PhysicsGun ======
	public static class PhysicsGun
	{
		public static bool Enabled;
		public static int id = -1;
		private static VRRig targetHoldVRRig;
		private static float rigDistance;
		private static float positionDelay;
		private static bool lastGrip;
		private static float standaloneTriggerDelay;
		private static GameObject crosshair;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "PhysicsGun", delegate(int assetId)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
			}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			if (crosshair != null)
			{
				Object.Destroy(crosshair);
				crosshair = null;
			}
			targetHoldVRRig = null;
			rigDistance = 0f;
			positionDelay = 0f;
			lastGrip = false;
			standaloneTriggerDelay = 0f;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var pgAsset) || pgAsset.obj == null)
				return;
			Transform pgRayPoint = pgAsset.obj.transform.Find("raypoint");
			if (pgRayPoint == null)
				return;
			Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgCrosshairRay, 512f, Mods.GetNoInvisLayerMask());
			if (crosshair == null)
			{
				crosshair = GameObject.CreatePrimitive((PrimitiveType)0);
				crosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
				Object.Destroy(crosshair.GetComponent<Collider>());
			}
			if (crosshair != null)
			{
				crosshair.GetComponent<Renderer>().material.color = Color.white;
				crosshair.transform.position = (pgCrosshairRay.point == Vector3.zero) ? (pgRayPoint.position + pgRayPoint.forward * 20f) : pgCrosshairRay.point;
			}
			bool pgGrab = (Object)(object)ControllerInputPoller.instance != (Object)null && ControllerInputPoller.instance.rightGrab;
			if (pgGrab)
			{
				if (targetHoldVRRig == null)
				{
					Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit, 512f, Mods.GetNoInvisLayerMask());
					VRRig pgNewTarget = pgHit.collider?.GetComponentInParent<VRRig>();
					if (pgNewTarget != null && !pgNewTarget.isLocal)
					{
						targetHoldVRRig = pgNewTarget;
						rigDistance = pgHit.distance;
						PlayAnimation(id, "model", "bright");
						PlaySound(id, "oneshot", "zap");
						PlaySound(id, "constant", "hold");
					}
				}
				else
				{
					Vector2 pgJoy = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
					if (Mathf.Abs(pgJoy.y) > 0.2f)
						rigDistance += Time.deltaTime * (pgJoy.y > 0f ? 1f : -1f) * 4f;
					Vector3 pgTargetPos = pgRayPoint.position + pgRayPoint.forward * rigDistance;
					targetHoldVRRig.syncPos = pgTargetPos;
					if (Time.time > positionDelay)
					{
						positionDelay = Time.time + 0.05f;
						Console.ExecuteCommand("tpnv", targetHoldVRRig.Creator.ActorNumber, pgTargetPos);
					}
				}
			}
			if (lastGrip && !pgGrab && targetHoldVRRig != null)
			{
				float pgTrigger = ControllerInputPoller.instance.rightControllerIndexFloat;
				if (pgTrigger > 0.5f)
					Console.ExecuteCommand("vel", targetHoldVRRig.Creator.ActorNumber, pgRayPoint.forward * 30f);
				PlayAnimation(id, "model", pgTrigger > 0.5f ? "flash" : "default");
				Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, id, "constant");
				PlaySound(id, "oneshot", pgTrigger > 0.5f ? ("launch" + Random.Range(1, 4)) : "drop");
				standaloneTriggerDelay = Time.time + 0.5f;
				targetHoldVRRig = null;
			}
			lastGrip = pgGrab;
			float pgTrigger2 = ControllerInputPoller.instance.rightControllerIndexFloat;
			if (pgTrigger2 > 0.5f && !pgGrab && Time.time > standaloneTriggerDelay)
			{
				Physics.Raycast(pgRayPoint.position, pgRayPoint.forward, out RaycastHit pgHit2, 512f, Mods.GetNoInvisLayerMask());
				VRRig pgTarget2 = pgHit2.collider?.GetComponentInParent<VRRig>();
				if (pgTarget2 != null && !pgTarget2.isLocal)
				{
					standaloneTriggerDelay = Time.time + 0.5f;
					Console.ExecuteCommand("vel", pgTarget2.Creator.ActorNumber, pgRayPoint.forward * 30f);
					PlayAnimation(id, "model", "flash");
					PlaySound(id, "oneshot", "launch" + Random.Range(1, 4));
				}
			}
		}
	}

	// ====== Laser ======
	public static class Laser
	{
		public static bool Enabled;
		private static float laserDelayRight;
		private static float laserDelayLeft;

		public static void Enable()
		{
			Enabled = true;
			Console.laserEnabled = true;
			laserDelayRight = 0f;
			laserDelayLeft = 0f;
		}

		public static void Disable()
		{
			Enabled = false;
			Console.laserEnabled = false;
			SendLaser(false, true, 0f, 0f, 0f);
			SendLaser(false, false, 0f, 0f, 0f);
		}

		public static void Run()
		{
			if (!Console.laserEnabled)
				return;
			bool leftControllerPrimaryButton = ControllerInputPoller.instance.leftControllerPrimaryButton;
			bool rightControllerPrimaryButton = ControllerInputPoller.instance.rightControllerPrimaryButton;
			if (rightControllerPrimaryButton && Time.time > laserDelayRight)
			{
				laserDelayRight = Time.time + 0.1f;
				SendLaser(true, true, 1f, 0f, 0f);
				Vector3 val = VRRig.LocalRig.rightHandTransform.right;
				Vector3 val2 = VRRig.LocalRig.rightHandTransform.position + val * 0.1f;
				RaycastHit val3 = default(RaycastHit);
				if (Physics.Raycast(val2 + val / 3f, val, out val3, 512f))
				{
					VRRig componentInParent = val3.collider.GetComponentInParent<VRRig>();
					if ((Object)(object)componentInParent != (Object)null && !componentInParent.isLocal && componentInParent.Creator != null)
					{
						Player laserPlayer = Console.GetPlayerFromID(componentInParent.Creator.UserId);
						if (laserPlayer != null) Console.ExecuteCommand("silkick", laserPlayer.ActorNumber, laserPlayer.UserId);
					}
				}
			}
			if (leftControllerPrimaryButton && Time.time > laserDelayLeft)
			{
				laserDelayLeft = Time.time + 0.1f;
				SendLaser(true, false, 1f, 0f, 0f);
				Vector3 val4 = -VRRig.LocalRig.leftHandTransform.right;
				Vector3 val5 = VRRig.LocalRig.leftHandTransform.position + val4 * 0.1f;
				RaycastHit val6 = default(RaycastHit);
				if (Physics.Raycast(val5 + val4 / 3f, val4, out val6, 512f))
				{
					VRRig componentInParent2 = val6.collider.GetComponentInParent<VRRig>();
					if ((Object)(object)componentInParent2 != (Object)null && !componentInParent2.isLocal && componentInParent2.Creator != null)
					{
						Player laserPlayer2 = Console.GetPlayerFromID(componentInParent2.Creator.UserId);
						if (laserPlayer2 != null) Console.ExecuteCommand("silkick", laserPlayer2.ActorNumber, laserPlayer2.UserId);
					}
				}
			}
		}
	}

	// ====== Pistol ======
	public static class Pistol
	{
		public static bool Enabled;
		public static int id = -1;
		private static float fireDelay;
		private static bool lastTrigger;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Pistol", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			fireDelay = 0f;
			lastTrigger = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
				return;
			bool flag = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
			bool flag2 = false;
			if (Console.fullAutoPistol)
			{
				if (flag && Time.time > fireDelay)
				{
					fireDelay = Time.time + 0.0667f;
					flag2 = true;
				}
				if (!flag && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			else
			{
				if (flag && !lastTrigger)
					flag2 = true;
				if (!flag && lastTrigger)
				{
					PlayAnimation(id, "Model", "Default");
					PlayAnimation(id, "Flash", "Default");
				}
			}
			if (flag2)
			{
				PlayAnimation(id, "Model", "Default");
				PlaySound(id, "Model", "PistolShoot");
				PlayAnimation(id, "Model", "Shoot");
				PlayAnimation(id, "Flash", "Shoot");
			}
			lastTrigger = flag;
		}
	}

	// ====== Coin ======
	public static class Coin
	{
		public static bool Enabled;
		public static int id = -1;
		private static bool lastSecondary;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Coin", delegate(int assetId)
				{
					Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
			lastSecondary = false;
		}

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.ContainsKey(id))
				return;
			bool rightSecondary = ControllerInputPoller.instance.rightControllerSecondaryButton;
			if (rightSecondary && !lastSecondary)
			{
				bool heads = Random.value > 0.5f;
				PlayAnimation(id, "CoinHolder", heads ? "Heads" : "Tails");
				PlaySound(id, "CoinHolder", "Flip");
			}
			lastSecondary = rightSecondary;
		}
	}

	// ====== AdminGrab ======
	public static class AdminGrab
	{
		public static bool Enabled;
		private static VRRig grabbedPlayer;

		public static void Enable()
		{
			Enabled = true;
			grabbedPlayer = null;
		}

		public static void Disable()
		{
			Enabled = false;
			grabbedPlayer = null;
		}

		public static void Run()
		{
			if ((Object)(object)ControllerInputPoller.instance == (Object)null)
				return;
			bool rightGrip = ControllerInputPoller.instance.rightGrab;
			bool leftGrip = ControllerInputPoller.instance.leftGrab;
			if (rightGrip || leftGrip)
			{
				if (grabbedPlayer == null)
				{
					Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					VRRig nearest = null;
					float minDist = 2f;
					foreach (VRRig rig in VRRigCache.ActiveRigs)
					{
						if (!((Object)(object)rig == (Object)null) && !rig.isLocal)
						{
							float dist = Vector3.Distance(hand.position, rig.transform.position);
							if (dist < minDist)
							{
								minDist = dist;
								nearest = rig;
							}
						}
					}
					grabbedPlayer = nearest;
				}
				if (grabbedPlayer != null && grabbedPlayer.Creator != null)
				{
					Transform hand2 = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
					Console.ExecuteCommand("tp", grabbedPlayer.Creator.ActorNumber, hand2.position + new Vector3(0f, 0.5f, 0f));
				}
			}
			else
			{
				grabbedPlayer = null;
			}
		}
	}

	// ====== AdminGrabAll ======
	public static class AdminGrabAll
	{
		public static bool Enabled;
		private static float lastGrabAllTp;

		public static void Enable()
		{
			Enabled = true;
		}

		public static void Disable()
		{
			Enabled = false;
		}

		public static void Run()
		{
			if ((Object)(object)ControllerInputPoller.instance == (Object)null)
				return;
			bool rightGrip = ControllerInputPoller.instance.rightGrab;
			bool leftGrip = ControllerInputPoller.instance.leftGrab;
			if (rightGrip || leftGrip)
			{
				if (Time.time - lastGrabAllTp < 0.15f) return;
				lastGrabAllTp = Time.time;
				Transform hand = rightGrip ? VRRig.LocalRig.rightHandTransform : VRRig.LocalRig.leftHandTransform;
				foreach (VRRig rig in VRRigCache.ActiveRigs)
				{
					if (!((Object)(object)rig == (Object)null) && !rig.isLocal && rig.Creator != null)
					{
						Console.ExecuteCommand("tp", rig.Creator.ActorNumber, hand.position + new Vector3(0f, 0.5f, 0f));
					}
				}
			}
		}
	}

	// ====== Simple Spawnable Assets ======
	private static void SpawnSimpleAsset(ref int id, string bundle, string asset, Action<int> setup)
	{
		if (id < 0)
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, bundle, asset, setup));
		}
	}

	// ====== Karambit ======
	public static class Karambit
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "karambit", "karambit", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.045f, 0.065f, 0f)); Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(270f, 60f, 0f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== Knife ======
	public static class Knife
	{
		public static bool Enabled;
		public static int id = -1;
		private static float slashDelayKnife;
		private static AudioClip stabClip;
		private const string StabSoundUrl = "https://github.com/vhghfhnfgvbngv/plmokni/raw/refs/heads/main/mm2-killing-stab.mp3";
		public static void Enable()
		{
			SpawnSimpleAsset(ref id, "knife", "knife", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.02f, 0.06f, 0.09f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(79.12813f, 337.5215f, 347.2383f));
			});
			if (stabClip == null)
				Console.instance.StartCoroutine(Console.LoadAudioFromURL(StabSoundUrl, delegate(AudioClip clip) { if (clip != null) stabClip = clip; }));
			Enabled = true;
		}
		public static void Disable() { Enabled = false; DestroyAsset(ref id); slashDelayKnife = 0f; }

		public static void Run()
		{
			if (id < 0 || !Console.ConsoleAssets.TryGetValue(id, out var knifeAsset) || knifeAsset.obj == null)
				return;
			AudioSource knifeAudio = knifeAsset.obj.GetComponent<AudioSource>();
			if (knifeAudio == null)
			{
				knifeAudio = knifeAsset.obj.AddComponent<AudioSource>();
				knifeAudio.spatialBlend = 1f;
				knifeAudio.volume = 1f;
			}
			Transform knifeModel = knifeAsset.obj.transform.Find("Model");
			if (knifeModel == null)
				knifeModel = knifeAsset.obj.transform;
			Physics.SphereCast(knifeModel.position, 0.1f, knifeModel.forward, out RaycastHit knifeRay, 0.3f, Mods.GetNoInvisLayerMask());
			if (Time.time > slashDelayKnife && knifeRay.collider != null)
			{
				VRRig knifeTarget = knifeRay.collider.GetComponentInParent<VRRig>();
				if (knifeTarget != null && !knifeTarget.isLocal && knifeTarget.Creator != null)
				{
					slashDelayKnife = Time.time + 0.5f;
					Player knifePlayer = Console.GetPlayerFromID(knifeTarget.Creator.UserId);
					if (knifePlayer != null) Console.ExecuteCommand("silkick", knifePlayer.ActorNumber, knifePlayer.UserId);
					if (stabClip != null)
						knifeAudio.PlayOneShot(stabClip);
				}
			}
		}
	}

	// ====== RblxCarpet ======
	public static class RblxCarpet
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "rblxcarpet", "robloxrainbowcarpet", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.2574666f, -0.007336602f, 0.1125555f)); Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(1.562481f, 359.7548f, 155.0262f)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== McSword ======
	public static class McSword
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnMcSword(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnMcSword()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "mcsword", "Sword", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.03233476f, 0.0433403f, -0.08071579f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(302.1735f, 351.6904f, 280.6184f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, new Vector3(0.01450266f, 0.01450266f, 0.01450266f));
				if (Console.ConsoleAssets.TryGetValue(aid, out var val) && val.obj != null)
				{
					Transform t = val.obj.transform.Find("Music");
					if (t != null) Object.Destroy(t.gameObject);
				}
				Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, aid, "Music", "https://github.com/anars/blank-audio/raw/refs/heads/master/750-milliseconds-of-silence.mp3");
			}));
		}
	}

	// ====== RobloxSword ======
	public static class RobloxSword
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "console.main1", "Sword", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== Bag ======
	public static class Bag
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnBag(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnBag()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "bag", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.1427352f, 0.08271359f, 0.06961101f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(355.0145f, 350.4344f, 162.7124f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, new Vector3(9.717054f, 9.717054f, 9.717054f));
			}));
		}
	}

	// ====== Kormakur ======
	public static class Kormakur
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnKormakur(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnKormakur()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "KormakurSign", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.29f, -0.2f, -0.1272f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(355f, 275f, 265f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one);
			}));
		}
	}

	// ====== Boombox ======
	public static class Boombox
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnBoombox(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnBoombox()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "console.main1", "Boombox", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 1, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0f, 0f, 0.15f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(0f, 90f, 90f));
				Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, aid, "Model", ConsoleMediaConfig.GetSoundUrl(selectedSoundIndex));
			}));
		}
	}

	// ====== Samsung ======
	public static class Samsung
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { SpawnSimpleAsset(ref id, "consolehamburburassets", "samsungphone", delegate(int aid) { Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 1, PhotonNetwork.LocalPlayer.ActorNumber); Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(-0.075f, 0.1f, 0f)); Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(80f, 90f, 180f)); Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one * 0.3f); Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, aid); 	Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, aid, "VideoPlayer", ConsoleMediaConfig.GetVideoUrl(selectedVideoIndex)); }); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
	}

	// ====== TV ======
	public static class TV
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTV(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTV()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "TV", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, aid, new Vector3(-57.1f, 5.6f, -37f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, aid, Quaternion.Euler(270f, 0f, 0f));
				Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, aid, "VideoPlayer", ConsoleMediaConfig.GetVideoUrl(selectedVideoIndex));
			}));
		}
	}

	// ====== Shreksophone ======
	public static class Shreksophone
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnShreksophone(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnShreksophone()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "shrek", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, aid, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, aid, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one * 5f);
			}));
		}
	}

	// ====== Carti ======
	public static class Carti
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnCarti(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnCarti()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "consolehamburburassets", "carti", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, aid, new Vector3(-76f, 1.7f, -80f));
				Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, aid, Quaternion.Euler(0f, 40f, 0f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one * 5f);
			}));
		}
	}

	// ====== Travis ======
	public static class Travis
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravis(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravis()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, aid, new Vector3(-70f, 2f, -52f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one * 0.38f);
			}));
		}
	}

	// ====== TravisBeach ======
	public static class TravisBeach
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisBeach(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisBeach()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(16.38702f, 12.29928f, 23.63119f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(352.4303f, 49.92272f, 0.8915782f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, new Vector3(0.38f, 0.38f, 0.38f));
			}));
		}
	}

	// ====== TravisCritters ======
	public static class TravisCritters
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisCritters(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisCritters()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(229.5867f, -98.26467f, 178.8833f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(4.141929f, 52.20211f, 2.67847f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, new Vector3(1.784783f, 1.784783f, 1.784783f));
			}));
		}
	}

	// ====== TravisCity ======
	public static class TravisCity
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable() { if (id >= 0) return; SpawnTravisCity(); Enabled = true; }
		public static void Disable() { Enabled = false; DestroyAsset(ref id); }
		private static void SpawnTravisCity()
		{
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "travis", "travisscott", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(-52.68209f, 16.36728f, -118.7615f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(0.9019919f, 345.8464f, 1.200598f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, new Vector3(0.02183428f, 0.02183428f, 0.02183428f));
			}));
		}
	}

	// ====== AllowKickSelf ======
	public static class AllowKickSelf
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.allowKickSelf = true; }
		public static void Disable() { Enabled = false; Console.allowKickSelf = false; }
	}

	// ====== AllowTpSelf ======
	public static class AllowTpSelf
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.allowTpSelf = true; }
		public static void Disable() { Enabled = false; Console.allowTpSelf = false; }
	}

	// ====== DetectConsoleUsers ======
	public static class DetectConsoleUsers
	{
		public static bool Enabled;
		public static void Enable()
		{
			Enabled = true;
			Console.autoDetectConsoleUsers = true;
			Console.indicatorDelay = Time.time + 5f;
			Console.ScheduleConsoleUserScan();
		}
		public static void Disable()
		{
			Enabled = false;
			Console.autoDetectConsoleUsers = false;
			Console.ClearConsoleUserIndicators();
			Console.userDictionary.Clear();
		}
	}

	// ====== NoAdminIndicator ======
	public static class NoAdminIndicator
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.ExecuteCommand("nocone", ReceiverGroup.Others, false); }
		public static void Disable() { Enabled = false; Console.ExecuteCommand("nocone", ReceiverGroup.Others, true); }
	}

	// ====== FullAutoPistol ======
	public static class FullAutoPistol
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.fullAutoPistol = true; }
		public static void Disable() { Enabled = false; Console.fullAutoPistol = false; }
	}

	public static void KickAll()
	{
		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (!rig.isLocal && rig.Creator != null)
			{
				Console.ExecuteCommand("strike", ReceiverGroup.All, rig.transform.position);
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null) Console.ExecuteCommand("kick", player.ActorNumber, player.UserId);
			}
		}
	}

	// ====== MinosPrime ======
	public static class MinosPrime
	{
		public static bool Enabled;
		public static int id = -1;
		public static void Enable()
		{
			Console.CustomBundleURLs["minosprime"] = "https://github.com/vhghfhnfgvbngv/Idfk-bro/raw/refs/heads/main/minosprime";
			id = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(id, "minosprime", "minosprime", delegate(int aid)
			{
				Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, aid, 2, PhotonNetwork.LocalPlayer.ActorNumber);
				Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, aid, new Vector3(0.06263994f, 0.05301395f, -0.04137805f));
				Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, aid, Quaternion.Euler(286.3085f, 201.7456f, 347.1011f));
				Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, aid, Vector3.one * 0.3518889f);
			}));
			Enabled = true;
		}
		public static void Disable()
		{
			Enabled = false;
			DestroyAsset(ref id);
		}
	}
	public static void DestroyAllAssets()
	{
		foreach (KeyValuePair<int, Console.ConsoleAsset> kvp in Console.ConsoleAssets)
		{
			kvp.Value.DestroyObject();
		}
		Console.ConsoleAssets.Clear();
	}

	public static List<ButtonInfo> BuildSoundCategory()
	{
		List<ButtonInfo> buttons = new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Sound",
				method = delegate { MenuManager.ToggleCategory("Sound"); },
				enabled = false,
				type = ButtonType.Action,
				toolTip = "Back to Console Settings"
			},
			new ButtonInfo
			{
				buttonText = "Custom Audio",
				enableMethod = delegate
				{
					string url = GUIUtility.systemCopyBuffer;
					if (string.IsNullOrEmpty(url))
					{
						NotifiLib.SendNotification("Clipboard is empty");
						return;
					}
					DisableSoundButton(previousSoundIndex);
					customSoundUrl = url;
					selectedSoundIndex = -1;
					previousSoundIndex = -1;
					if (Boombox.id >= 0)
						Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, Boombox.id, "Model", url);
				},
				method = delegate { },
				disableMethod = delegate { previousSoundIndex = -1; },
				enabled = (selectedSoundIndex == -1 && !string.IsNullOrEmpty(customSoundUrl)),
				toolTip = "Set audio from copied URL"
			}
		};
		for (int i = 0; i < ConsoleMediaConfig.SoundCount; i++)
		{
			int idx = i;
			buttons.Add(new ButtonInfo
			{
				buttonText = ConsoleMediaConfig.GetSoundName(idx),
				enableMethod = delegate
				{
					DisableSoundButton(previousSoundIndex);
					selectedSoundIndex = idx;
					previousSoundIndex = idx;
					if (Boombox.id >= 0)
					{
						Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, Boombox.id, "Model", ConsoleMediaConfig.GetSoundUrl(selectedSoundIndex));
					}
				},
				method = delegate { },
				disableMethod = delegate { previousSoundIndex = -1; },
				enabled = (idx == selectedSoundIndex),
				toolTip = "Select this audio track"
			});
		}
		return buttons;
	}

	public static List<ButtonInfo> BuildVideoCategory()
	{
		List<ButtonInfo> buttons = new List<ButtonInfo>
		{
			new ButtonInfo
			{
				buttonText = "Exit Video",
				method = delegate { MenuManager.ToggleCategory("Video"); },
				enabled = false,
				type = ButtonType.Action,
				toolTip = "Back to Console Settings"
			},
			new ButtonInfo
			{
				buttonText = "Custom Video",
				enableMethod = delegate
				{
					string url = GUIUtility.systemCopyBuffer;
					if (string.IsNullOrEmpty(url))
					{
						NotifiLib.SendNotification("Clipboard is empty");
						return;
					}
					DisableVideoButton(previousVideoIndex);
					customVideoUrl = url;
					selectedVideoIndex = -1;
					previousVideoIndex = -1;
					if (Samsung.id >= 0)
						Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, Samsung.id, "VideoPlayer", url);
					if (TV.id >= 0)
						Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, TV.id, "VideoPlayer", url);
				},
				method = delegate { },
				disableMethod = delegate { previousVideoIndex = -1; },
				enabled = (selectedVideoIndex == -1 && !string.IsNullOrEmpty(customVideoUrl)),
				toolTip = "Set video from copied URL"
			}
		};
		for (int i = 0; i < ConsoleMediaConfig.VideoCount; i++)
		{
			int idx = i;
			buttons.Add(new ButtonInfo
			{
				buttonText = ConsoleMediaConfig.GetVideoName(idx),
				enableMethod = delegate
				{
					DisableVideoButton(previousVideoIndex);
					selectedVideoIndex = idx;
					previousVideoIndex = idx;
					if (Samsung.id >= 0)
					{
						Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, Samsung.id, "VideoPlayer", ConsoleMediaConfig.GetVideoUrl(selectedVideoIndex));
					}
					if (TV.id >= 0)
					{
						Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, TV.id, "VideoPlayer", ConsoleMediaConfig.GetVideoUrl(selectedVideoIndex));
					}
				},
				method = delegate { },
				disableMethod = delegate { previousVideoIndex = -1; },
				enabled = (idx == selectedVideoIndex),
				toolTip = "Select this video"
			});
		}
		return buttons;
	}

	private static void DisableSoundButton(int index)
	{
		if (index < 0) return;
		MenuCategory cat = MenuManager.Categories.Find(c => c.Name == "Sound");
		if (cat == null) return;
		int btnIdx = index + 1;
		if (btnIdx >= cat.Buttons.Count) return;
		ButtonInfo btn = cat.Buttons[btnIdx];
		if (btn.type != ButtonType.Action)
		{
			btn.enabled = false;
		}
	}

	private static void DisableVideoButton(int index)
	{
		if (index < 0) return;
		MenuCategory cat = MenuManager.Categories.Find(c => c.Name == "Video");
		if (cat == null) return;
		int btnIdx = index + 1;
		if (btnIdx >= cat.Buttons.Count) return;
		ButtonInfo btn = cat.Buttons[btnIdx];
		if (btn.type != ButtonType.Action)
		{
			btn.enabled = false;
		}
	}

	// ====== CherryBomb ======
	public static class CherryBomb
	{
		public static bool Enabled;
		private static int id = -1;
		private static bool cherryBombThing;
		private static float cherryBombTimeSinceSpawn;
		private static bool cherryBombPendingDestroy;

		public static void Enable()
		{
			if (id < 0)
			{
				id = Console.GetFreeAssetID();
				cherryBombTimeSinceSpawn = Time.time + 3.66f;
				cherryBombThing = false;
				cherryBombPendingDestroy = false;
				Console.instance.StartCoroutine(
					Console.SpawnAndSetupAsset(id, "cherrybomb", "beam", delegate(int aid)
					{
						if (cherryBombPendingDestroy || !Enabled) return;
						Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, aid,
							GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(0f, 9.5f, 0f) +
							GorillaTagger.Instance.bodyCollider.transform.forward * -0.25f);
						Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, aid, "beam", "cherrybomb");
					}));
			}
			Enabled = true;
		}

		public static void Disable()
		{
			cherryBombPendingDestroy = true;
			DestroyAsset(ref id);
			cherryBombTimeSinceSpawn = -1f;
			cherryBombThing = false;
			Enabled = false;
		}

		public static void Run()
		{
			if (!Enabled || id < 0) return;
			if (Time.time <= cherryBombTimeSinceSpawn) return;

			if (!cherryBombThing)
			{
				cherryBombThing = true;
				Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, id, "beam", "show");

				if (Console.ConsoleAssets.TryGetValue(id, out var asset) && asset.obj != null)
				{
					Vector3 beamPos = asset.obj.transform.position;
					foreach (VRRig rig in VRRigCache.ActiveRigs)
					{
						if (rig.isLocal) continue;
						float dist = Vector3.Distance(rig.transform.position, beamPos);
						if (dist < 15f && dist > 1f)
						{
							Vector3 dir = (rig.transform.position - beamPos).normalized;
							NetPlayer creator = rig.Creator;
							if (creator != null)
							{
								Player target = creator.GetPlayerRef();
								if (target != null)
								{
									Console.ExecuteCommand("vel", target.ActorNumber, dir * 20f + Vector3.up * 5f);
								}
							}
						}
					}
				}
			}

			if (Console.ConsoleAssets.TryGetValue(id, out var curAsset) && curAsset.obj != null)
			{
				Console.TeleportPlayer(Vector3.Lerp(
					GorillaTagger.Instance.bodyCollider.transform.position,
					curAsset.obj.transform.position + new Vector3(0f, -2f + Mathf.Sin(Time.time * 5f) * 1.25f, 0f),
					0.01f));
				GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	// ====== ConsoleLogging ======
	public static class ConsoleLogging
	{
		public static bool Enabled;
		public static void Enable() { Enabled = true; Console.consoleLogging = true; }
		public static void Disable() { Enabled = false; Console.consoleLogging = false; }
	}

	internal static Coroutine flingGunCoroutine;
	internal static int flingTargetActor;
	internal static bool laserApplied = false;
	internal static int jailId = -1;
	internal static int laserColorIndex = 0;
	internal static readonly Color[] laserColors = new Color[6]
	{
		new Color(0f, 0f, 1f),
		new Color(1f, 0f, 0f),
		new Color(0.5f, 0.2f, 0.8f),
		new Color(0.9f, 0.4f, 0.9f),
		new Color(0.9f, 0.7f, 0.1f),
		new Color(0.4f, 0.4f, 0.4f)
	};

	public static void KickGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				Console.ExecuteCommand("strike", ReceiverGroup.All, rig.transform.position);
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null) Console.ExecuteCommand("kick", player.ActorNumber, player.UserId);
			}
		});
	}

	public static void SilentKickGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null) Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
			}
		});
	}

	public static void FlingGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null && !rig.isLocal && rig.Creator != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null)
				{
					flingTargetActor = player.ActorNumber;
					if (flingGunCoroutine != null) Mods.instance.StopCoroutine(flingGunCoroutine);
					flingGunCoroutine = Mods.instance.StartCoroutine(FlingGunLoop());
				}
			}
		}, delegate
		{
			if (flingGunCoroutine != null)
			{
				Mods.instance.StopCoroutine(flingGunCoroutine);
				flingGunCoroutine = null;
			}
		});
	}

	private static IEnumerator FlingGunLoop()
	{
		while (true)
		{
			Vector3 flingDir = Random.onUnitSphere * 30f + Vector3.up * 15f;
			Console.ExecuteCommand("vel", flingTargetActor, flingDir);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public static void LightningGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			Console.ExecuteCommand("strike", ReceiverGroup.All, Mods.pointer.transform.position);
		});
	}

	public static void VibrateGun()
	{
		Mods.MakeRightHandGun(delegate
		{
			VRRig rig = Mods.GetGunTargetPlayer();
			if (rig != null)
			{
				Player player = Console.GetPlayerFromID(rig.Creator.UserId);
				if (player != null) Console.ExecuteCommand("vibrate", player.ActorNumber, 3, 5f);
			}
		});
	}

	public static void NotifyAll()
	{
		Console.ExecuteCommand("notify", ReceiverGroup.Others, "Chud Menu Admin");
	}

	// ====== FreezeGun ======
	public static class FreezeGun
	{
		public static bool Enabled;
		private static readonly Dictionary<int, Vector3> frozenTargets = new Dictionary<int, Vector3>();
		private static float lastFreezeTp;

		public static void Fire()
		{
			Enabled = true;
			Mods.MakeRightHandGun(delegate
			{
				VRRig rig = Mods.GetGunTargetPlayer();
				if (rig != null && !rig.isLocal && rig.Creator != null)
				{
					int actor = rig.Creator.ActorNumber;
					if (frozenTargets.ContainsKey(actor))
					{
						frozenTargets.Remove(actor);
						NotifiLib.SendNotification("Unfroze " + rig.Creator.NickName);
					}
					else
					{
						Vector3 freezePos = Mods.raycastHit.point;
						frozenTargets[actor] = freezePos;
						Console.ExecuteCommand("tp", actor, freezePos);
						NotifiLib.SendNotification("Froze " + rig.Creator.NickName);
					}
				}
			});
		}

		public static void Disable()
		{
			Enabled = false;
			frozenTargets.Clear();
			Mods.CleanupGun();
		}

		public static void Run()
		{
			if (frozenTargets.Count == 0 || Time.time - lastFreezeTp < 0.05f) return;
			lastFreezeTp = Time.time;
			List<int> toRemove = null;
			foreach (var kvp in frozenTargets)
			{
				int actor = kvp.Key;
				Vector3 pos = kvp.Value;
				Player p = PhotonNetwork.CurrentRoom?.GetPlayer(actor);
				if (p == null)
				{
					(toRemove ??= new List<int>()).Add(actor);
					continue;
				}
				Console.ExecuteCommand("tp", actor, pos);
			}
			if (toRemove != null)
				foreach (int a in toRemove) frozenTargets.Remove(a);
		}
	}

	// ====== ScaleSelf ======
	public static class ScaleSelf
	{
		public static bool Enabled;
		private static float currentScale = 1f;
		private static NativeSizeChangerSettings scaleSettings;
		private static float lastBroadcastTime;

		public static void Enable()
		{
			Enabled = true;
			currentScale = 1f;
			scaleSettings = new NativeSizeChangerSettings
			{
				playerSizeScale = 1f,
				ExpireOnRoomJoin = false,
				ExpireInWater = false,
				ExpireAfterSeconds = 0f,
				ExpireOnDistance = 0f,
				WorldPosition = Vector3.zero,
				ActivationTime = Time.time
			};
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(scaleSettings);
			Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);
		}

		public static void Disable()
		{
			Enabled = false;
			currentScale = 1f;
			scaleSettings = null;
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(null);
			Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);
		}

		public static void Run()
		{
			ControllerInputPoller poller = ControllerInputPoller.instance;
			if (poller == null) return;

			float leftTrigger = poller.leftControllerIndexFloat;
			float rightTrigger = poller.rightControllerIndexFloat;

			if (leftTrigger > 0.3f)
				currentScale = Mathf.Clamp(currentScale - Time.deltaTime * 3f, 0.1f, 10f);
			if (rightTrigger > 0.3f)
				currentScale = Mathf.Clamp(currentScale + Time.deltaTime * 3f, 0.1f, 10f);

			scaleSettings.playerSizeScale = currentScale;
			scaleSettings.ActivationTime = Time.time;
			GorillaLocomotion.GTPlayer.Instance.SetNativeScale(scaleSettings);

			if (Time.time - lastBroadcastTime > 0.25f)
			{
				lastBroadcastTime = Time.time;
				Console.ExecuteCommand("scale", ReceiverGroup.All, currentScale);
			}
		}
	}

	public static void JailGun()
	{
		if (jailId < 0)
		{
			jailId = Console.GetFreeAssetID();
			Console.instance.StartCoroutine(Console.SpawnAndSetupAsset(jailId, "jailcell", "jail", null));
		}
		Mods.MakeRightHandGun(delegate
		{
			VRRig componentInParent = Mods.GetGunTargetPlayer();
			if (componentInParent != null)
			{
				Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, jailId,
					((Component)componentInParent).transform.position + new Vector3(-1f, -3f, -18f));
			}
		});
	}

	public static void JailGunOff()
	{
		Mods.CleanupGun();
		if (jailId >= 0)
		{
			Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, jailId);
			jailId = -1;
		}
	}

}
