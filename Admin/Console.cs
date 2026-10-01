using Chud.Diagnostics;
using Chud.Rendering;
using Chud.Runtime;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GTAG_NotificationLib;
using HarmonyLib;
using Object = UnityEngine.Object;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine;
namespace Chud.Backend;

public class Console : MonoBehaviour
{
	#region Nested Types

	public class AssetCollisionHandler : MonoBehaviour
	{
		public int id;

		public string assetName;

		public string bundleName;

		private float lastCollisionTime;

		private void OnCollisionEnter(Collision collision)
		{
			Player local = PhotonNetwork.LocalPlayer;
			if (local == null || !ServerData.IsAdmin(local.UserId) || Time.time - lastCollisionTime < 0.5f)
			{
				return;
			}
			lastCollisionTime = Time.time;
			VRRig rig = collision.collider.GetComponentInParent<VRRig>();
			if (rig == (Object)null || rig.Creator == null || rig.isLocal || rig.Creator.UserId == local.UserId)
			{
				return;
			}
			Player target = GetPlayerFromID(rig.Creator.UserId);
			if (target == null)
			{
				return;
			}
			ExecuteCommand("silkick", ReceiverGroup.Others, target.UserId);
			if (assetName == "BanHammer")
			{
				ExecuteCommand("asset-playoneshot", ReceiverGroup.All, id, "KillSFX", "HammerHit");
			}
			else if (assetName == "Sword" && bundleName == "rbsword")
			{
				string[] clips = new string[2] { "Slash1", "Slash2" };
				ExecuteCommand("asset-playoneshot", ReceiverGroup.All, id, "SFX", clips[Random.Range(0, clips.Length)]);
			}
		}
	}

	public class ConsoleAsset
	{
		public int id;

		public GameObject obj;

		public string assetName;

		public string bundleName;

		public int ownerActor = -1;

		public ConsoleAsset(int id, GameObject obj, string assetName, string bundleName)
		{
			this.id = id;
			this.obj = obj;
			this.assetName = assetName;
			this.bundleName = bundleName;
		}

		public void DestroyObject()
		{
			if (obj != (Object)null)
			{
				Object.Destroy(obj);
			}
		}

		public void SetPosition(Vector3 position)
		{
			if (obj != (Object)null)
			{
				obj.transform.position = position;
			}
		}

		public void SetRotation(Quaternion rotation)
		{
			if (obj != (Object)null)
			{
				obj.transform.rotation = rotation;
			}
		}

		public void SetLocalPosition(Vector3 position)
		{
			if (obj != (Object)null)
			{
				obj.transform.localPosition = position;
			}
		}

		public void SetLocalRotation(Quaternion rotation)
		{
			if (obj != (Object)null)
			{
				obj.transform.localRotation = rotation;
			}
		}

		public void SetScale(Vector3 scale)
		{
			if (obj != (Object)null)
			{
				obj.transform.localScale = scale;
			}
		}

		private Transform FindTarget(string childName)
		{
			if (obj == (Object)null)
			{
				return null;
			}
			return string.IsNullOrEmpty(childName) ? obj.transform : obj.transform.Find(childName);
		}

		public void SetColor(string objectName, Color color)
		{
			Transform target = FindTarget(objectName);
			if (target == (Object)null)
			{
				return;
			}
			Renderer renderer = target.GetComponent<Renderer>();
			if (renderer != (Object)null)
			{
				renderer.material.color = color;
			}
		}

		public void PlayAudioSource(string audioSourceName)
		{
			Transform target = FindTarget(audioSourceName);
			if (target == (Object)null)
			{
				return;
			}
			AudioSource source = target.GetComponent<AudioSource>();
			if (source != (Object)null)
			{
				source.Play();
			}
		}

		public void StopAudioSource(string audioSourceName)
		{
			Transform target = FindTarget(audioSourceName);
			if (target == (Object)null)
			{
				return;
			}
			AudioSource source = target.GetComponent<AudioSource>();
			if (source != (Object)null)
			{
				source.Stop();
			}
		}

		public void ChangeAudioVolume(string volumeName, float volume)
		{
			Transform target = FindTarget(volumeName);
			if (target == (Object)null)
			{
				return;
			}
			AudioSource source = target.GetComponent<AudioSource>();
			if (source != (Object)null)
			{
				source.volume = Mathf.Clamp(volume, 0f, 1f);
			}
			VideoPlayer video = target.GetComponent<VideoPlayer>();
			if (video != (Object)null)
			{
				video.SetDirectAudioVolume((ushort)0, Mathf.Clamp(volume, 0f, 1f));
			}
		}

		public void PlayAnimation(string objectName, string animationName)
		{
			Transform target = FindTarget(objectName);
			if (target == (Object)null)
			{
				return;
			}
			Animator animator = target.GetComponent<Animator>();
			if (animator != (Object)null)
			{
				animator.Play(animationName);
			}
		}
	}

	public static Console instance;

	public const string ConsoleVersion = "3.0.8";

	public const string MenuName = "Chud Menu";

	public static readonly string ConsoleResourceLocation = "Console";

	public static string MenuVersion = "1.8.7";

	public const string SpoofMenuName = "gay furry femboy menu";

	public const string SpoofVersion = "69";

	public const float SANITIZE_INTERVAL = 2f;

	public static bool allowKickSelf;

	public static bool allowTpSelf = true;

	public static bool disableFlingSelf;

	public static bool adminIsScaling;

	public static float adminScale = 1f;

	public static VRRig adminRigTarget;

	public static readonly List<Player> excludedCones = new List<Player>();

	public static readonly Dictionary<VRRig, GameObject> conePool = new Dictionary<VRRig, GameObject>();

	public static bool IsMasterConsole;

	public static readonly Dictionary<string, (string, string)> userDictionary = new Dictionary<string, (string, string)>();

	public static readonly Dictionary<VRRig, GameObject> consoleUserIndicators = new Dictionary<VRRig, GameObject>();

	public static Coroutine smoothTeleportCoroutine;

	public static Coroutine shakeCoroutine;

	public static bool laserEnabled;

	public static readonly Dictionary<int, ConsoleAsset> ConsoleAssets = new Dictionary<int, ConsoleAsset>();

	public static readonly Dictionary<string, string> CustomBundleURLs = new Dictionary<string, string>();

	public static readonly string[] AssetServerURLs = new string[2] { "https://raw.githubusercontent.com/hamburbur-org/Public-Assets/refs/heads/main", "https://raw.githubusercontent.com/Seralyth/Console/refs/heads/master/ServerData" };

	public static float indicatorDelay;

	public static bool AutoDetectConsoleUsers;

	public static bool consoleLogging;

	public static bool fullAutoPistol;

	public static float lastRecheckTime = -5f;

	public static bool consoleSpoofEnabled;

	public static Shader CachedUberShader => ShaderLibrary.Uber;

	public static Shader CachedGuiTextShader => ShaderLibrary.Fallback;

	public static bool HasConsoleIndicator(VRRig rig) => consoleUserIndicators.ContainsKey(rig);

	private float dataLoadTime = -1f;

	private float reloadTime = -1f;

	private int loadAttempts;

	private bool isLoadingData;

	private readonly Dictionary<int, float> confirmUsingDelay = new Dictionary<int, float>();

	private readonly Dictionary<Player, Coroutine> laserCoroutineLeft = new Dictionary<Player, Coroutine>();

	private readonly Dictionary<Player, Coroutine> laserCoroutineRight = new Dictionary<Player, Coroutine>();

	private readonly Dictionary<string, AssetBundle> AssetBundlePool = new Dictionary<string, AssetBundle>();

	private readonly Dictionary<int, List<Tuple<Player, object[], string>>> PendingAssetCommands = new Dictionary<int, List<Tuple<Player, object[], string>>>();

	private bool pendingDelayedScan;

	private float _nextSanitize;

	private readonly List<int> _destroyPlayerAssetsKeys = new List<int>();

	private float _nextSpoofBroadcast;

	private List<VRRig> _adminCleanupList = new List<VRRig>();

	private List<VRRig> _userCleanupList = new List<VRRig>();

	private List<int> _sanitizeRemoveKeys = new List<int>();

	private readonly List<int> _pendingKeyCleanup = new List<int>();

	private bool consoleInitialized;

	private bool networkHandlersSubscribed;

	public static void EnableConsoleSpoof()
	{
		consoleSpoofEnabled = true;
		if (instance != null)
		{
			instance._nextSpoofBroadcast = 0f;
		}
	}

	public static void DisableConsoleSpoof()
	{
		consoleSpoofEnabled = false;
	}

	#endregion

	#region Lifecycle

	public void Awake()
	{
		instance = this;
		dataLoadTime = Time.time + 5f;
		if (!Directory.Exists(ConsoleResourceLocation))
		{
			Directory.CreateDirectory(ConsoleResourceLocation);
		}
		this.StartCoroutine(ServerData.DownloadAdminTextures());
		this.StartCoroutine(ServerData.LoadGithubAdmins());
		this.StartCoroutine(ServerData.LoadServerData());
		this.StartCoroutine(ServerData.LoadGithubSuperAdmins());
	}

	public void Start()
	{
		if (consoleInitialized)
		{
			return;
		}
		consoleInitialized = true;
		PlayerGameEvents.OnMiscEvent += NoOverlapEvents;
		PlayerGameEvents.OnMiscEvent += ConsoleAssetCommunication;
		GorillaTagger.OnPlayerSpawned(delegate
		{
			if (networkHandlersSubscribed)
			{
				return;
			}
			NetworkSystem obj = NetworkSystem.Instance;
			if (obj == null)
			{
				return;
			}
			networkHandlersSubscribed = true;
			obj.OnReturnedToSinglePlayer = (DelegateListProcessorPlusMinus<DelegateListProcessor, Action>)(object)obj.OnReturnedToSinglePlayer + (Action)ClearConsoleAssets;
			obj.OnReturnedToSinglePlayer = (DelegateListProcessorPlusMinus<DelegateListProcessor, Action>)(object)obj.OnReturnedToSinglePlayer + (Action)ClearCones;
			obj.OnPlayerJoined = (DelegateListProcessorPlusMinus<DelegateListProcessor<NetPlayer>, Action<NetPlayer>>)(object)obj.OnPlayerJoined + (Action<NetPlayer>)SyncConsoleAssets;
			obj.OnPlayerLeft = (DelegateListProcessorPlusMinus<DelegateListProcessor<NetPlayer>, Action<NetPlayer>>)(object)obj.OnPlayerLeft + (Action<NetPlayer>)SyncConsoleUsers;
		});
	}

	public void Update()
	{
		TickServerDataReload();
		if (IsMasterConsole)
		{
			return;
		}
		TickAdminScale();
		TickAssetSanitize();
		TickSpoofBroadcast();
	}

	private void TickServerDataReload()
	{
		if (isLoadingData)
		{
			return;
		}
		if (dataLoadTime > 0f && Time.time > dataLoadTime)
		{
			dataLoadTime = Time.time + 5f;
			loadAttempts++;
			if (loadAttempts >= 3)
			{
				dataLoadTime = -1f;
			}
			else
			{
				isLoadingData = true;
				this.StartCoroutine(RunLoadServerData());
				this.StartCoroutine(ServerData.LoadGithubAdmins());
				this.StartCoroutine(ServerData.LoadGithubSuperAdmins());
			}
		}
		if (reloadTime > 0f && Time.time > reloadTime)
		{
			reloadTime = Time.time + 60f;
			isLoadingData = true;
			this.StartCoroutine(RunLoadServerData());
			this.StartCoroutine(ServerData.LoadGithubAdmins());
			this.StartCoroutine(ServerData.LoadGithubSuperAdmins());
		}
		else if (reloadTime <= 0f)
		{
			reloadTime = Time.time + 10f;
		}
	}

	private static void TickAdminScale()
	{
		if (!adminIsScaling || adminRigTarget == (Object)null)
		{
			return;
		}
		adminRigTarget.NativeScale = adminScale;
		if (Mathf.Approximately(adminScale, 1f))
		{
			adminIsScaling = false;
		}
	}

	private void TickAssetSanitize()
	{
		if (Time.time < _nextSanitize)
		{
			return;
		}
		_nextSanitize = Time.time + SANITIZE_INTERVAL;
		SanitizeConsoleAssets();
	}

	private void TickSpoofBroadcast()
	{
		if (!consoleSpoofEnabled || !PhotonNetwork.InRoom || Time.time < _nextSpoofBroadcast)
		{
			return;
		}
		_nextSpoofBroadcast = Time.time + 5f;
		ExecuteCommand("confirmusing", ReceiverGroup.All, SpoofVersion, SpoofMenuName);
	}

	private IEnumerator RunLoadServerData()
	{
		yield return ServerData.LoadServerData();
		dataLoadTime = -1f;
		isLoadingData = false;
	}

	public static void LoadConsole()
	{
		GorillaTagger.OnPlayerSpawned(delegate
		{
			LoadConsoleImmediately();
		});
	}

	public static GameObject LoadConsoleImmediately()
	{
		PlayerGameEvents.MiscEvent("%<CONSOLE>%LoadVersion", ServerData.VersionToNumber(ConsoleVersion));
		string text = "goldentrophy_Console";
		GameObject existing = GameObject.Find(text);
		GameObject val = existing != null ? existing : new GameObject(text);
		val.AddComponent<Console>();
		return val;
	}

	public static void ScheduleConsoleUserScan()
	{
		if (instance == null)
		{
			return;
		}
		lastRecheckTime = -5f;
		ScanForConsoleUsers();
		if (!instance.pendingDelayedScan)
		{
			instance.pendingDelayedScan = true;
			instance.StartCoroutine(instance.DelayedConsoleScan());
		}
	}

	private IEnumerator DelayedConsoleScan()
	{
		yield return (object)new WaitForSeconds(3f);
		pendingDelayedScan = false;
		ScanForConsoleUsers();
	}

	#endregion

	#region Player And World Utilities

	public static Vector3 World2Player(Vector3 world)
	{
		GorillaTagger tagger = GorillaTagger.Instance;
		if (tagger == (Object)null || tagger.bodyCollider == (Object)null)
		{
			return world;
		}

		return world - tagger.bodyCollider.transform.position + tagger.transform.position;
	}

	public const float DefaultTeleportTime = 0.1f;

	public static void TeleportPlayer(Vector3 position)
	{
		GTPlayer player = GTPlayer.Instance;
		VRRig localRig = VRRig.LocalRig;
		if (player == (Object)null || localRig == (Object)null)
		{
			return;
		}

		player.TeleportTo(World2Player(position), player.transform.rotation, true, false);
		localRig.transform.position = position;
	}

	public static void ConfirmUsing(string id, string version, string menuName)
	{
		NotifiLib.SendNotification(id + " uses " + menuName + " v" + version);
	}

	public static VRRig GetVRRigFromPlayer(Player p)
	{
		if (p == null)
		{
			return null;
		}
		NetPlayer np = NetworkSystem.Instance != null ? NetworkSystem.Instance.GetNetPlayerByID(p.ActorNumber) : null;
		if (np != null)
		{
			return GorillaGameManager.StaticFindRigForPlayer(np);
		}
		return null;
	}

	public static Player GetPlayerFromID(string id)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].UserId == id)
			{
				return playerList[i];
			}
		}
		return null;
	}

	public static void ApplyCosmeticToRig(VRRig rig, string cosmeticId)
	{
		if (rig == (Object)null || string.IsNullOrEmpty(cosmeticId))
		{
			return;
		}
		MethodInfo method = AccessTools.Method(rig.GetType(), "AddCosmetic");
		if (method == null)
		{
			return;
		}
		ParameterInfo[] parameters = method.GetParameters();
		object[] args = new object[parameters.Length];
		args[0] = cosmeticId;
		for (int i = 1; i < args.Length; i++)
		{
			args[i] = Type.Missing;
		}
		method.Invoke(rig, args);
		rig.RefreshCosmetics();
	}

	public static void LightningStrike(Vector3 position)
	{
		Color cyan = Color.cyan;
		GameObject outer = new GameObject("LightningOuter");
		LineRenderer outerLine = outer.AddComponent<LineRenderer>();
		outerLine.startColor = cyan;
		outerLine.endColor = cyan;
		outerLine.startWidth = 0.25f;
		outerLine.endWidth = 0.25f;
		outerLine.positionCount = 5;
		outerLine.useWorldSpace = true;
		Vector3 point = position;
		for (int i = 0; i < 5; i++)
		{
			VRRig.LocalRig.PlayHandTapLocal(68, false, 0.25f);
			VRRig.LocalRig.PlayHandTapLocal(68, true, 0.25f);
			outerLine.SetPosition(i, point);
			point += new Vector3(Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f));
		}
		outerLine.material = new Material(CachedUberShader);
		outerLine.material.color = cyan;
		Object.Destroy(outer, 2f);
		GameObject inner = new GameObject("LightningInner");
		LineRenderer innerLine = inner.AddComponent<LineRenderer>();
		innerLine.startColor = Color.white;
		innerLine.endColor = Color.white;
		innerLine.startWidth = 0.1f;
		innerLine.endWidth = 0.1f;
		innerLine.positionCount = 5;
		innerLine.useWorldSpace = true;
		for (int j = 0; j < 5; j++)
		{
			innerLine.SetPosition(j, outerLine.GetPosition(j));
		}
		innerLine.material = new Material(CachedUberShader);
		innerLine.material.color = Color.white;
		Object.Destroy(inner, 2f);
	}

	public static IEnumerator SmoothTeleport(Vector3 position, float time)
	{
		GorillaTagger tagger = GorillaTagger.Instance;
		if (tagger == (Object)null || tagger.bodyCollider == (Object)null || tagger.rigidbody == (Object)null)
		{
			smoothTeleportCoroutine = null;
			yield break;
		}

		float startTime = Time.time;
		Vector3 startPosition = tagger.bodyCollider.transform.position;
		while (Time.time < startTime + time)
		{
			TeleportPlayer(Vector3.Lerp(startPosition, position, (Time.time - startTime) / time));
			if (tagger.rigidbody != (Object)null)
			{
				tagger.rigidbody.linearVelocity = Vector3.zero;
			}
			yield return null;
		}
		smoothTeleportCoroutine = null;
	}

	public static IEnumerator Shake(float strength, float time, bool constant)
	{
		GorillaTagger tagger = GorillaTagger.Instance;
		if (tagger == (Object)null || tagger.bodyCollider == (Object)null)
		{
			shakeCoroutine = null;
			yield break;
		}

		float startTime = Time.time;
		while (Time.time < startTime + time)
		{
			float shakePower = constant ? strength : strength * (1f - (Time.time - startTime) / time);
			TeleportPlayer(tagger.bodyCollider.transform.position + new Vector3(Random.Range(-shakePower, shakePower), Random.Range(-shakePower, shakePower), Random.Range(-shakePower, shakePower)));
			yield return null;
		}
		shakeCoroutine = null;
	}

	public static IEnumerator ControllerPress(string button, float value, float duration)
	{
		float stop = Time.time + duration;
		while (Time.time < stop)
		{
			switch (button)
			{
				case "lGrip":
					ControllerInputPoller.instance.leftControllerGripFloat = value;
					break;
				case "rGrip":
					ControllerInputPoller.instance.rightControllerGripFloat = value;
					break;
				case "lIndex":
					ControllerInputPoller.instance.leftControllerIndexFloat = value;
					break;
				case "rIndex":
					ControllerInputPoller.instance.rightControllerIndexFloat = value;
					break;
				case "lPrimary":
					ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = value > 0.33f;
					ControllerInputPoller.instance.leftControllerPrimaryButton = value > 0.66f;
					break;
				case "lSecondary":
					ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = value > 0.33f;
					ControllerInputPoller.instance.leftControllerSecondaryButton = value > 0.66f;
					break;
				case "rPrimary":
					ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = value > 0.33f;
					ControllerInputPoller.instance.rightControllerPrimaryButton = value > 0.66f;
					break;
				case "rSecondary":
					ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = value > 0.33f;
					ControllerInputPoller.instance.rightControllerSecondaryButton = value > 0.66f;
					break;
			}
			yield return null;
		}
	}

	#endregion

	#region Command Entry

	public static void ExecuteCommand(string command, RaiseEventOptions options, params object[] parameters)
	{
		if (consoleLogging && command != "isusing" && command != "confirmusing")
		{
			NotifiLib.SendNotification(command + " (self)");
		}
		NetworkManager.SendConsoleCommand(command, options, parameters);
	}

	public static void ExecuteCommand(string command, int target, params object[] parameters)
	{
		RaiseEventOptions val = new RaiseEventOptions();
		val.TargetActors = new int[1] { target };
		ExecuteCommand(command, val, parameters);
	}

	public static void ExecuteCommand(string command, ReceiverGroup target, params object[] parameters)
	{
		Console.ExecuteCommand(command, new RaiseEventOptions
		{
			Receivers = target
		}, parameters);
	}

	private static bool IsAdministrator(string userId)
	{
		lock (ServerData.AdminLock)
		{
			return !string.IsNullOrEmpty(userId) && ServerData.Administrators.ContainsKey(userId);
		}
	}

	private static bool IsSuperAdministrator(string name)
	{
		lock (ServerData.AdminLock)
		{
			return ServerData.SuperAdministrators.Contains(name);
		}
	}

	public static void HandleConsoleEvent(Player sender, object[] args, string command)
	{
		if (consoleLogging && command != "isusing" && command != "confirmusing" && sender != PhotonNetwork.LocalPlayer)
		{
			string senderName;
			try
			{
				VRRig rig = GetVRRigFromPlayer(sender);
				senderName = rig != null ? rig.Creator.NickName : sender.UserId;
			}
			catch
			{
				senderName = sender.UserId;
			}
			NotifiLib.SendNotification(command + " from " + senderName);
		}
		if (command == "isusing")
		{
			HandleIsUsing(sender);
			return;
		}
		if (command == "confirmusing")
		{
			if (instance != null)
			{
				instance.HandleConfirmUsing(sender, args);
			}
			return;
		}
		if (instance == null)
		{
			return;
		}
		if (!IsAdministrator(sender.UserId))
		{
			return;
		}
		string adminName;
		lock (ServerData.AdminLock)
		{
			ServerData.Administrators.TryGetValue(sender.UserId, out adminName);
		}

		try
		{
			instance.HandleAdminCommand(sender, args, command, IsSuperAdministrator(adminName));
		}
		catch (Exception ex)
		{
			Log.Warn("command '" + command + "' failed", ex);
			return;
		}

		if (command.StartsWith("asset-"))
		{
			try
			{
				instance.HandleAssetEvent(sender, args, command);
			}
			catch (Exception ex)
			{
				Log.Warn("command '" + command + "' failed", ex);
			}
		}
	}

	private static void HandleIsUsing(Player sender)
	{
		if (consoleSpoofEnabled)
		{
			ExecuteCommand("confirmusing", sender.ActorNumber, SpoofVersion, SpoofMenuName);
			return;
		}
		if (!IsAdministrator(sender.UserId))
		{
			return;
		}
		ExecuteCommand("confirmusing", sender.ActorNumber, MenuVersion, MenuName);
	}

	private void HandleConfirmUsing(Player sender, object[] args)
	{
		if (confirmUsingDelay.TryGetValue(sender.ActorNumber, out float delay) && Time.time < delay)
		{
			return;
		}
		confirmUsingDelay[sender.ActorNumber] = Time.time + 5f;
		VRRig rig = GetVRRigFromPlayer(sender);
		string displayName = rig != (Object)null ? rig.Creator.NickName : sender.UserId;
		string uid = sender.UserId;
		bool known = !string.IsNullOrEmpty(uid) && userDictionary.ContainsKey(uid);
		if (!string.IsNullOrEmpty(uid))
		{
			userDictionary[uid] = ((string)args[2], (string)args[1]);
		}
		if (!known && indicatorDelay > Time.time)
		{
			NotifiLib.SendNotification(displayName + " has <color=yellow>" + args[2]?.ToString() + "</color> v" + args[1]);
		}
		if (AutoDetectConsoleUsers && rig != (Object)null)
		{
			AddConsoleUserIndicator(rig, (string)args[2], (string)args[1]);
		}
	}

	#endregion

	#region Admin Command Handling

	private void HandleAdminCommand(Player sender, object[] args, string command, bool isSuper)
	{
		switch (command)
		{
			case "kick":
				ApplyKick((string)args[1], isSuper);
				break;
			case "silkick":
				ApplySilentKick((string)args[1], isSuper);
				break;
			case "kickall":
				ApplyKickAll();
				break;
			case "vibrate":
				ApplyVibrate((int)args[1], Mathf.Clamp((float)args[2], 0f, 10f));
				break;
			case "tp":
				ApplyTeleport(args, isSuper);
				break;
			case "vel":
				ApplyVelocity(args, isSuper);
				break;
			case "controller":
				ApplyControllerPress(args);
				break;
			case "tpsmooth":
			case "smoothtp":
				ApplySmoothTeleport((Vector3)args[1], (float)args[2]);
				break;
			case "shake":
				ApplyShake((float)args[1], (float)args[2], (bool)args[3]);
				break;
			case "tpnv":
				ApplyTeleportNoVelocity(args, isSuper);
				break;
			case "notify":
				ApplyNotify(args);
				break;
			case "strike":
				ApplyStrike(args);
				break;
			case "lr":
				ApplyLine(args);
				break;
			case "platf":
				ApplyPlatform(args);
				break;
			case "muteall":
				ApplyMuteAll(true);
				break;
			case "unmuteall":
				ApplyMuteAll(false);
				break;
			case "mute":
				ApplyMute((string)args[1], true);
				break;
			case "unmute":
				ApplyMute((string)args[1], false);
				break;
			case "scale":
				ApplyScale(sender, args);
				break;
			case "time":
				ApplyTimeOfDay(args);
				break;
			case "weather":
				ApplyWeather((bool)args[1]);
				break;
			case "setmaterial":
				ApplySetMaterial((int)args[1], (int)args[2]);
				break;
			case "laser":
				ApplyLaser(sender, args);
				break;
			case "sb":
				ApplySoundThroughMic(args, isSuper);
				break;
			case "spatial":
				ApplySpatial(GetVRRigFromPlayer(sender), (bool)args[1]);
				break;
			case "nocone":
				ApplyNoCone(sender, args);
				break;
			case "rigposition":
				ApplyRigPosition((bool)args[1], (object[])args[2], (object[])args[3], (object[])args[4]);
				break;
			case "setfog":
				ApplySetFog(args);
				break;
			case "resetfog":
				ApplyResetFog();
				break;
			case "game-setposition":
				ApplyGameSetPosition(args, isSuper);
				break;
			case "game-setrotation":
				ApplyGameSetRotation(args, isSuper);
				break;
			case "game-clone":
				ApplyGameClone(args, isSuper);
				break;
			case "cosmetic":
				ApplyCosmetic(sender, (string)args[1]);
				break;
			case "cosmetics":
				ApplyCosmetics(sender, (string[])args[1]);
				break;
		}
	}

	private static bool CanSelfTeleport(bool isSuper)
	{
		return (!disableFlingSelf || isSuper) && (allowTpSelf || isSuper);
	}

	private static void ApplyTeleport(object[] args, bool isSuper)
	{
		if (!CanSelfTeleport(isSuper))
		{
			return;
		}
		ApplySmoothTeleport((Vector3)args[1], DefaultTeleportTime);
	}

	private static void ApplyTeleportNoVelocity(object[] args, bool isSuper)
	{
		if (!CanSelfTeleport(isSuper))
		{
			return;
		}
		ApplySmoothTeleport((Vector3)args[1], DefaultTeleportTime);
		GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
	}

	private static void ApplyVelocity(object[] args, bool isSuper)
	{
		if (!CanSelfTeleport(isSuper))
		{
			return;
		}
		GorillaTagger.Instance.rigidbody.linearVelocity = (Vector3)args[1];
	}

	private static void ApplyControllerPress(object[] args)
	{
		instance.StartCoroutine(ControllerPress((string)args[1], (float)args[2], (float)args[3]));
	}

	private static void ApplyNotify(object[] args)
	{
		NotifiLib.SendNotification((string)args[1]);
	}

	private static void ApplyStrike(object[] args)
	{
		LightningStrike((Vector3)args[1]);
	}

	private static void ApplyScale(Player sender, object[] args)
	{
		adminIsScaling = true;
		adminRigTarget = GetVRRigFromPlayer(sender);
		adminScale = (float)args[1];
	}

	private static void ApplyTimeOfDay(object[] args)
	{
		if (BetterDayNightManager.instance is BetterDayNightManager timeManager)
		{
			timeManager.SetTimeOfDay((int)args[1]);
		}
	}

	private static void ApplySoundThroughMic(object[] args, bool isSuper)
	{
		if (!isSuper)
		{
			return;
		}
		try
		{
			instance.StartCoroutine(PlaySoundThroughMic((string)args[1]));
		}
		catch
		{
		}
	}

	private static void ApplyNoCone(Player sender, object[] args)
	{
		if ((bool)args[1])
		{
			excludedCones.Add(sender);
		}
		else
		{
			excludedCones.Remove(sender);
		}
	}

	private static void ApplyGameSetPosition(object[] args, bool isSuper)
	{
		if (!isSuper)
		{
			return;
		}
		GameObject moveTarget = GameObject.Find((string)args[1]);
		if (moveTarget != (Object)null)
		{
			moveTarget.transform.position = (Vector3)args[2];
		}
	}

	private static void ApplyGameSetRotation(object[] args, bool isSuper)
	{
		if (!isSuper)
		{
			return;
		}
		GameObject rotateTarget = GameObject.Find((string)args[1]);
		if (rotateTarget != (Object)null)
		{
			rotateTarget.transform.rotation = (Quaternion)args[2];
		}
	}

	private static void ApplyGameClone(object[] args, bool isSuper)
	{
		if (!isSuper)
		{
			return;
		}
		GameObject cloneSource = GameObject.Find((string)args[1]);
		if (cloneSource != (Object)null)
		{
			Object.Instantiate(cloneSource, cloneSource.transform.position, cloneSource.transform.rotation, cloneSource.transform.parent).name = (string)args[2];
		}
	}

	#endregion

	#region Command Implementations

	private static void ApplyKick(string targetUserId, bool isSuper)
	{
		Player target = GetPlayerFromID(targetUserId);
		if (target != null)
		{
			VRRig rig = GetVRRigFromPlayer(target);
			if (rig != (Object)null)
			{
				LightningStrike(rig.headMesh.transform.position);
			}
		}
		if (allowKickSelf || !IsAdministrator(targetUserId) || isSuper)
		{
			if (targetUserId == PhotonNetwork.LocalPlayer.UserId)
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
		}
	}

	private static void ApplySilentKick(string targetUserId, bool isSuper)
	{
		if (allowKickSelf || !IsAdministrator(targetUserId) || isSuper)
		{
			if (targetUserId == PhotonNetwork.LocalPlayer.UserId)
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
		}
	}

	private static void ApplyKickAll()
	{
		Player[] targets = IsAdministrator(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList;
		foreach (Player target in targets)
		{
			try
			{
				NetPlayer netPlayer = NetworkSystem.Instance != null ? NetworkSystem.Instance.GetNetPlayerByID(target.ActorNumber) : null;
				if (netPlayer == null)
				{
					continue;
				}
				VRRig rig = GorillaGameManager.StaticFindRigForPlayer(netPlayer);
				if (rig != (Object)null)
				{
					LightningStrike(rig.headMesh.transform.position);
				}
			}
			catch
			{
			}
		}
		if (!IsAdministrator(PhotonNetwork.LocalPlayer.UserId))
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
		}
	}

	private static void ApplyVibrate(int mode, float duration)
	{
		if (mode == 1 || mode == 3)
		{
			GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength, duration);
		}
		if (mode == 2 || mode == 3)
		{
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength, duration);
		}
	}

	private static void ApplySmoothTeleport(Vector3 position, float time)
	{
		if (smoothTeleportCoroutine != null)
		{
			instance.StopCoroutine(smoothTeleportCoroutine);
			smoothTeleportCoroutine = null;
		}
		if (instance == (Object)null || time <= 0f)
		{
			TeleportPlayer(position);
			return;
		}
		smoothTeleportCoroutine = instance.StartCoroutine(SmoothTeleport(position, time));
	}

	private static void ApplyShake(float strength, float time, bool constant)
	{
		if (shakeCoroutine != null)
		{
			instance.StopCoroutine(shakeCoroutine);
		}
		shakeCoroutine = instance.StartCoroutine(Shake(strength, time, constant));
	}

	private static void ApplyLine(object[] args)
	{
		GameObject line = new GameObject("Line");
		LineRenderer renderer = line.AddComponent<LineRenderer>();
		Color color = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
		renderer.startColor = color;
		renderer.endColor = color;
		renderer.startWidth = (float)args[5];
		renderer.endWidth = (float)args[5];
		renderer.positionCount = 2;
		renderer.useWorldSpace = true;
		renderer.SetPosition(0, (Vector3)args[6]);
		renderer.SetPosition(1, (Vector3)args[7]);
		renderer.material = new Material(CachedUberShader);
		renderer.material.color = color;
		Object.Destroy(line, (float)args[8]);
	}

	private static void ApplyPlatform(object[] args)
	{
		GameObject platform = GameObject.CreatePrimitive((PrimitiveType.Cube));
		Object.Destroy(platform, args.Length > 8 ? (float)args[8] : 60f);
		if (args.Length > 4)
		{
			if ((float)args[7] == 0f)
			{
				Object.Destroy(platform.GetComponent<Renderer>());
			}
			else
			{
				platform.GetComponent<Renderer>().material.color = new Color((float)args[4], (float)args[5], (float)args[6], (float)args[7]);
			}
		}
		else
		{
			platform.GetComponent<Renderer>().material.color = Color.black;
		}
		platform.transform.position = (Vector3)args[1];
		platform.transform.rotation = args.Length > 3 ? Quaternion.Euler((Vector3)args[3]) : Quaternion.identity;
		platform.transform.localScale = args.Length > 2 ? (Vector3)args[2] : new Vector3(1f, 0.1f, 1f);
	}

	private static void ApplyMuteAll(bool mute)
	{
		foreach (GorillaPlayerScoreboardLine line in GameLists.ScoreboardLines())
		{
			if (line.playerVRRig == null || line.linePlayer == null)
			{
				continue;
			}

			if (mute && !line.playerVRRig.muted && !IsAdministrator(line.linePlayer.UserId))
			{
				PressScoreboard(line, true);
			}
			else if (!mute && line.playerVRRig.muted)
			{
				PressScoreboard(line, false);
			}
		}
	}

	private static void PressScoreboard(GorillaPlayerScoreboardLine line, bool isOn)
	{
		try
		{
			line.PressButton(isOn, GorillaPlayerLineButton.ButtonType.Mute);
		}
		catch (Exception ex)
		{
			Log.Warn("scoreboard mute press failed", ex);
		}
	}

	private static void ApplyMute(string targetUserId, bool mute)
	{
		foreach (GorillaPlayerScoreboardLine line in GameLists.ScoreboardLines())
		{
			if (line.playerVRRig == null || line.linePlayer == null)
			{
				continue;
			}

			if (line.playerVRRig.Creator.UserId != targetUserId)
			{
				continue;
			}
			if (mute && !line.playerVRRig.muted && !IsAdministrator(line.linePlayer.UserId))
			{
				PressScoreboard(line, true);
			}
			else if (!mute && line.playerVRRig.muted)
			{
				PressScoreboard(line, false);
			}
		}
	}

	private static void ApplyWeather(bool raining)
	{
		if (BetterDayNightManager.instance is BetterDayNightManager manager)
		{
			for (int i = 0; i < manager.weatherCycle.Length; i++)
			{
				manager.weatherCycle[i] = (BetterDayNightManager.WeatherType)(raining ? 1 : 0);
			}
		}
	}

	private static void ApplySetMaterial(int actorNumber, int materialIndex)
	{
		VRRig rig = GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(actorNumber, false));
		if (rig != (Object)null)
		{
			rig.ChangeMaterialLocal(materialIndex);
		}
	}

	private static readonly Color FixedLaserColor = new Color(1f, 0f, 0f);

	private void ApplyLaser(Player sender, object[] args)
	{
		bool enabled = (bool)args[1];
		bool rightHand = (bool)args[2];
		Dictionary<Player, Coroutine> pool = rightHand ? laserCoroutineRight : laserCoroutineLeft;
		if (pool.TryGetValue(sender, out Coroutine running))
		{
			instance.StopCoroutine(running);
			pool.Remove(sender);
		}
		if (enabled)
		{
			pool[sender] = instance.StartCoroutine(RenderLaser(rightHand, GetVRRigFromPlayer(sender), FixedLaserColor));
		}
	}

	private static void ApplySpatial(VRRig rig, bool distant)
	{
		try
		{
			if (rig == (Object)null)
			{
				return;
			}
			AudioSource source = rig.GetComponentInChildren<AudioSource>();
			if (source != (Object)null)
			{
				source.spatialBlend = distant ? 1f : 0.9f;
				source.maxDistance = distant ? float.MaxValue : 500f;
			}
		}
		catch
		{
		}
	}

	private static void ApplyRigPosition(bool enabled, object[] head, object[] left, object[] right)
	{
		VRRig.LocalRig.enabled = enabled;
		if (head != null)
		{
			VRRig.LocalRig.transform.position = (Vector3)head[0];
			VRRig.LocalRig.transform.rotation = (Quaternion)head[1];
			VRRig.LocalRig.head.rigTarget.transform.rotation = (Quaternion)head[2];
		}
		if (left != null)
		{
			VRRig.LocalRig.leftHand.rigTarget.transform.position = (Vector3)left[0];
			VRRig.LocalRig.leftHand.rigTarget.transform.rotation = (Quaternion)left[1];
		}
		if (right != null)
		{
			VRRig.LocalRig.rightHand.rigTarget.transform.position = (Vector3)right[0];
			VRRig.LocalRig.rightHand.rigTarget.transform.rotation = (Quaternion)right[1];
		}
	}

	private static void ApplySetFog(object[] args)
	{
		try
		{
			Color color = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
			Type type = Type.GetType("ZoneShaderSettings, Assembly-CSharp");
			if (type == null)
			{
				return;
			}
			object active = type.GetProperty("activeInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
			if (active == null)
			{
				return;
			}
			type.GetMethod("SetGroundFogValue")?.Invoke(active, new object[4] { color, (float)args[5], (float)args[6], (float)args[7] });
		}
		catch
		{
		}
	}

	private static void ApplyResetFog()
	{
		try
		{
			Type type = Type.GetType("ZoneShaderSettings, Assembly-CSharp");
			if (type == null)
			{
				return;
			}
			object active = type.GetProperty("activeInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
			object defaults = type.GetProperty("defaultsInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
			if (active != null && defaults != null)
			{
				type.GetMethod("CopySettings")?.Invoke(active, new object[1] { defaults });
			}
		}
		catch
		{
		}
	}

	private static void ApplyCosmetic(Player sender, string cosmeticId)
	{
		VRRig rig = GetVRRigFromPlayer(sender);
		if (rig != (Object)null)
		{
			ApplyCosmeticToRig(rig, cosmeticId);
		}
	}

	private static void ApplyCosmetics(Player sender, string[] cosmeticIds)
	{
		VRRig rig = GetVRRigFromPlayer(sender);
		if (rig == (Object)null)
		{
			return;
		}
		foreach (string cosmeticId in cosmeticIds)
		{
			ApplyCosmeticToRig(rig, cosmeticId);
		}
	}

	public static IEnumerator RenderLaser(bool rightHand, VRRig rigTarget, Color laserColor)
	{
		if (rigTarget == (Object)null)
		{
			yield break;
		}
		float startTime = Time.time;
		RaycastHit ray = default(RaycastHit);
		while (!(rigTarget == (Object)null) && !(Time.time - startTime > 0.1f))
		{
			rigTarget.PlayHandTapLocal(18, !rightHand, 99999f);
			GameObject outer = new GameObject("LaserOuter");
			outer.hideFlags = HideFlags.HideAndDontSave;
			LineRenderer outerLine = outer.AddComponent<LineRenderer>();
			outerLine.startColor = laserColor;
			outerLine.endColor = laserColor;
			outerLine.startWidth = 0.15f + Mathf.Sin(Time.time * 5f) * 0.01f;
			outerLine.endWidth = outerLine.startWidth;
			outerLine.positionCount = 2;
			outerLine.useWorldSpace = true;
			Vector3 startPos = (rightHand ? rigTarget.rightHandTransform.position : rigTarget.leftHandTransform.position) + (rightHand ? rigTarget.rightHandTransform.up : rigTarget.leftHandTransform.up) * 0.1f;
			Vector3 dir = rightHand ? rigTarget.rightHandTransform.right : -rigTarget.leftHandTransform.right;
			Vector3 endPos = !Physics.Raycast(startPos + dir / 3f, dir, out ray, 512f) ? startPos + dir * 512f : ray.point;
			outerLine.SetPosition(0, startPos + dir * 0.1f);
			outerLine.SetPosition(1, endPos);
			outerLine.material = new Material(CachedUberShader);
			outerLine.material.color = laserColor;
			outerLine.material.hideFlags = HideFlags.HideAndDontSave;
			Object.Destroy(outer, Time.deltaTime);
			GameObject inner = new GameObject("LaserInner");
			inner.hideFlags = HideFlags.HideAndDontSave;
			LineRenderer innerLine = inner.AddComponent<LineRenderer>();
			innerLine.startColor = Color.white;
			innerLine.endColor = Color.white;
			innerLine.startWidth = 0.1f;
			innerLine.endWidth = 0.1f;
			innerLine.positionCount = 2;
			innerLine.useWorldSpace = true;
			innerLine.SetPosition(0, startPos + dir * 0.1f);
			innerLine.SetPosition(1, endPos);
			innerLine.material = new Material(CachedUberShader);
			innerLine.material.color = Color.white;
			innerLine.material.renderQueue = outerLine.material.renderQueue + 1;
			innerLine.material.hideFlags = HideFlags.HideAndDontSave;
			Object.Destroy(inner, Time.deltaTime);
			GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			spark.hideFlags = HideFlags.HideAndDontSave;
			Object.Destroy(spark, 2f);
			Object.Destroy(spark.GetComponent<Collider>());
			spark.GetComponent<Renderer>().material.color = Color.yellow;
			spark.GetComponent<Renderer>().material.hideFlags = HideFlags.HideAndDontSave;
			spark.AddComponent<Rigidbody>().linearVelocity = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(0f, 7.5f), Random.Range(-7.5f, 7.5f));
			spark.transform.position = endPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
			spark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			yield return null;
		}
	}

	#endregion

	#region Asset Lifecycle

	public static void NoOverlapEvents(string eventName, int id)
	{
		if (eventName == "%<CONSOLE>%LoadVersion" || ServerData.VersionToNumber(ConsoleVersion) > id)
		{
			return;
		}
		IsMasterConsole = true;
	}

	public static void ConsoleAssetCommunication(string eventName, int id)
	{
		if (!eventName.StartsWith("%<CONSOLE>%SyncAssets"))
		{
			return;
		}
		string[] parts = eventName.Split(new string[1] { "||" }, StringSplitOptions.None);
		switch (parts[0])
		{
			case "spawn":
				instance.StartCoroutine(LinkConsoleAsset(id, parts[3], parts[1], parts[2], bool.Parse(parts[4])));
				break;
			case "destroy":
				ConsoleAssets.Remove(id);
				break;
			case "confirmusing":
				ConfirmUsing(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(id, false).UserId, parts[1], parts[2]);
				break;
		}
	}

	public static IEnumerator LinkConsoleAsset(int id, string linkObjectName, string assetName, string assetBundle, bool addGorillaSurfaceOverride)
	{
		if (!PhotonNetwork.InRoom)
		{
			yield break;
		}
		if (GameObject.Find(linkObjectName) == (Object)null)
		{
			float timeoutTime = Time.time + 10f;
			while (Time.time < timeoutTime && GameObject.Find(linkObjectName) == (Object)null)
			{
				yield return null;
			}
		}
		GameObject finalLink = GameObject.Find(linkObjectName);
		if (finalLink == (Object)null || !PhotonNetwork.InRoom)
		{
			yield break;
		}
		ConsoleAssets.Add(id, new ConsoleAsset(id, finalLink.transform.parent.gameObject, assetName, assetBundle));
	}

	public static Player GetMasterAdministrator()
	{
		Player result = null;
		int lowestActor = int.MaxValue;
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (IsAdministrator(playerList[i].UserId) && playerList[i].ActorNumber < lowestActor)
			{
				result = playerList[i];
				lowestActor = playerList[i].ActorNumber;
			}
		}
		return result;
	}

	public static void DestroyColliders(GameObject obj)
	{
		Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
		foreach (Collider collider in colliders)
		{
			Object.Destroy(collider);
		}
	}

	public void SanitizeConsoleAssets()
	{
		PrunePendingAssetCommands();

		if (ConsoleAssets.Count == 0)
		{
			return;
		}
		_sanitizeRemoveKeys.Clear();
		foreach (KeyValuePair<int, ConsoleAsset> entry in ConsoleAssets)
		{
			if (entry.Value.obj == (Object)null || !entry.Value.obj.activeSelf)
			{
				_sanitizeRemoveKeys.Add(entry.Key);
			}
		}
		for (int i = 0; i < _sanitizeRemoveKeys.Count; i++)
		{
			ConsoleAssets[_sanitizeRemoveKeys[i]].DestroyObject();
			ConsoleAssets.Remove(_sanitizeRemoveKeys[i]);
		}
	}

	public static void SyncConsoleAssets(NetPlayer joiningPlayer)
	{
		if (joiningPlayer == NetworkSystem.Instance.LocalPlayer || ConsoleAssets.Count <= 0)
		{
			return;
		}
		Player master = GetMasterAdministrator();
		if (master == null || PhotonNetwork.LocalPlayer != master)
		{
			return;
		}
		foreach (ConsoleAsset asset in ConsoleAssets.Values)
		{
			ExecuteCommand("asset-spawn", joiningPlayer.GetPlayerRef().ActorNumber, asset.bundleName, asset.assetName, asset.id);
			if (asset.obj != (Object)null)
			{
				ExecuteCommand("asset-setposition", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.position);
				ExecuteCommand("asset-setrotation", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.rotation);
				ExecuteCommand("asset-setscale", joiningPlayer.GetPlayerRef().ActorNumber, asset.id, asset.obj.transform.localScale);
			}
		}
		PhotonNetwork.SendAllOutgoingCommands();
	}

	public static void SyncConsoleUsers(NetPlayer player)
	{
		if (instance == null)
		{
			return;
		}
		Player playerRef = player.GetPlayerRef();
		if (player != null && !string.IsNullOrEmpty(player.UserId))
		{
			userDictionary.Remove(player.UserId);
		}
		if (playerRef != null)
		{
			instance.DestroyPlayerAssets(playerRef.ActorNumber);
			instance.StopLaserCoroutines(playerRef);
		}
		else
		{
			instance.DestroyPlayerAssets(-1);
		}
	}

	private void StopLaserCoroutines(Player player)
	{
		if (laserCoroutineLeft.TryGetValue(player, out Coroutine left))
		{
			if (instance != (Object)null)
			{
				instance.StopCoroutine(left);
			}
			laserCoroutineLeft.Remove(player);
		}
		if (laserCoroutineRight.TryGetValue(player, out Coroutine right))
		{
			if (instance != (Object)null)
			{
				instance.StopCoroutine(right);
			}
			laserCoroutineRight.Remove(player);
		}
	}

	public void DestroyPlayerAssets(int actorNumber)
	{
		_destroyPlayerAssetsKeys.Clear();
		foreach (KeyValuePair<int, ConsoleAsset> entry in ConsoleAssets)
		{
			if (entry.Value.ownerActor == actorNumber)
			{
				_destroyPlayerAssetsKeys.Add(entry.Key);
			}
		}
		for (int i = 0; i < _destroyPlayerAssetsKeys.Count; i++)
		{
			int id = _destroyPlayerAssetsKeys[i];
			if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
			{
				asset.DestroyObject();
				ConsoleAssets.Remove(id);
			}
			PendingAssetCommands.Remove(id);
		}
	}

	private static void ScanForConsoleUsers()
	{
		if (!PhotonNetwork.InRoom || !(Time.time - lastRecheckTime > 3f))
		{
			return;
		}
		lastRecheckTime = Time.time;
		indicatorDelay = Time.time + 5f;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (string.IsNullOrEmpty(player.UserId) || userDictionary.ContainsKey(player.UserId))
			{
				continue;
			}
			RaiseEventOptions options = new RaiseEventOptions();
			options.TargetActors = new int[1] { player.ActorNumber };
			ExecuteCommand("isusing", options);
		}
	}

	public static int GetFreeAssetID()
	{
		int id;
		int attempts = 0;
		do
		{
			id = Random.Range(0, int.MaxValue);
			if (++attempts > 1000)
			{
				break;
			}
		}
		while (ConsoleAssets.ContainsKey(id));

		if (!ConsoleAssets.ContainsKey(id))
		{
			return id;
		}

		int candidate = ConsoleAssets.Count == 0 ? 1 : ConsoleAssets.Keys.Max() + 1;
		while (ConsoleAssets.ContainsKey(candidate))
		{
			candidate++;
		}

		return candidate;
	}

	public IEnumerator LoadAssetBundle(string bundleName)
	{
		if (AssetBundlePool.ContainsKey(bundleName))
		{
			yield break;
		}
		foreach (string serverUrl in AssetServerURLs)
		{
			UnityWebRequest req = UnityWebRequest.Get(serverUrl + "/" + bundleName);
			try
			{
				yield return req.SendWebRequest();
				if (req.result == UnityWebRequest.Result.Success)
				{
					AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
					yield return bundleReq;
					if (bundleReq.assetBundle != (Object)null)
					{
						AssetBundlePool[bundleName] = bundleReq.assetBundle;
						break;
					}
				}
			}
			finally
			{
				req?.Dispose();
			}
		}
	}

	public IEnumerator LoadAssetBundleFromURL(string bundleName, string url)
	{
		if (AssetBundlePool.ContainsKey(bundleName))
		{
			yield break;
		}
		CustomBundleURLs[bundleName] = url;
		UnityWebRequest req = UnityWebRequest.Get(url);
		try
		{
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
				yield return bundleReq;
				if (bundleReq.assetBundle != (Object)null)
				{
					AssetBundlePool[bundleName] = bundleReq.assetBundle;
				}
			}
		}
		finally
		{
			req?.Dispose();
		}
	}

	public IEnumerator SpawnConsoleAsset(string bundleName, string assetName, int id, bool addSurfaceOverride = false)
	{
		if (ConsoleAssets.ContainsKey(id))
		{
			ConsoleAssets[id].DestroyObject();
			ConsoleAssets.Remove(id);
		}
		if (!AssetBundlePool.ContainsKey(bundleName))
		{
			yield return instance.StartCoroutine(CustomBundleURLs.ContainsKey(bundleName) ? LoadAssetBundleFromURL(bundleName, CustomBundleURLs[bundleName]) : LoadAssetBundle(bundleName));
		}
		if (!AssetBundlePool.ContainsKey(bundleName))
		{
			yield break;
		}
		AssetBundleRequest assetReq = AssetBundlePool[bundleName].LoadAssetAsync<GameObject>(assetName);
		yield return assetReq;
		if (assetReq.asset == (Object)null)
		{
			yield break;
		}
		GameObject obj = Object.Instantiate<GameObject>((GameObject)assetReq.asset);
		Animator[] animators = obj.GetComponentsInChildren<Animator>(true);
		foreach (Animator animator in animators)
		{
			animator.enabled = true;
		}
		AudioSource[] sources = obj.GetComponentsInChildren<AudioSource>(true);
		foreach (AudioSource source in sources)
		{
			if (source.clip != (Object)null && source.playOnAwake)
			{
				source.Play();
			}
		}
		if (addSurfaceOverride)
		{
			Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
			foreach (Collider collider in colliders)
			{
				if (collider.GetComponent<GorillaSurfaceOverride>() == (Object)null)
				{
					collider.gameObject.AddComponent<GorillaSurfaceOverride>();
				}
			}
		}
		ConsoleAssets[id] = new ConsoleAsset(id, obj, assetName, bundleName);
		if (assetName == "BanHammer" || (assetName == "Sword" && bundleName == "rbsword"))
		{
			Collider[] hitColliders = obj.GetComponentsInChildren<Collider>(true);
			foreach (Collider collider in hitColliders)
			{
				AssetCollisionHandler handler = collider.gameObject.AddComponent<AssetCollisionHandler>();
				handler.id = id;
				handler.assetName = assetName;
				handler.bundleName = bundleName;
			}
		}
		if (!PendingAssetCommands.TryGetValue(id, out List<Tuple<Player, object[], string>> pending))
		{
			yield break;
		}
		foreach (Tuple<Player, object[], string> command in pending)
		{
			HandleAssetEvent(command.Item1, command.Item2, command.Item3);
		}
		PendingAssetCommands.Remove(id);
	}

	#endregion

	#region Admin Indicators

	public static void UpdateAdminIndicators()
	{
		if (instance == null)
		{
			return;
		}
		if (!PhotonNetwork.InRoom)
		{
			ClearConePool();
			return;
		}
		try
		{
			instance.PruneConePool();
			string localName;
			bool localIsSuper;
			lock (ServerData.AdminLock)
			{
				localIsSuper = ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out localName) && ServerData.SuperAdministrators.Contains(localName);
			}
			foreach (Player player in PhotonNetwork.PlayerListOthers)
			{
				ApplyCrownTo(player, localIsSuper);
			}

			if (SeeOwnCrown && PhotonNetwork.LocalPlayer != null)
			{
				ApplyCrownTo(PhotonNetwork.LocalPlayer, true);
			}
		}
		catch
		{
			return;
		}
	}

	public static bool SeeOwnCrown;

	private static void ApplyCrownTo(Player player, bool localIsSuper)
	{
		string adminName;
		bool isAdmin;
		lock (ServerData.AdminLock)
		{
			isAdmin = ServerData.Administrators.TryGetValue(player.UserId, out adminName);
		}
		if (!isAdmin || (!localIsSuper && excludedCones.Contains(player)))
		{
			return;
		}
		VRRig rig = GetVRRigFromPlayer(player);
		if (rig == (Object)null)
		{
			return;
		}
		if (!conePool.TryGetValue(rig, out GameObject cone))
		{
			cone = CreateAdminCone();
			Renderer created = cone.GetComponent<Renderer>();
			Material resolved = ResolveAdminMaterial(adminName);
			if (created != (Object)null && resolved != (Object)null)
			{
				created.material = resolved;
				created.material.color = Color.white;
			}
			conePool.Add(rig, cone);
		}
		cone.transform.localScale = new Vector3(0.35f, 0.35f, 0.02f) * rig.scaleFactor;
		cone.transform.position = Mods.GetTagPosition(rig, Mods.TagStackCrown);
		Camera main = Mods.MainCamera();
		if (main != (Object)null)
		{
			cone.transform.LookAt(main.transform);
		}
	}

	private void PruneConePool()
	{
		_adminCleanupList.Clear();
		VRRig ownCrownRig = SeeOwnCrown ? null : GetVRRigFromPlayer(PhotonNetwork.LocalPlayer);
		foreach (KeyValuePair<VRRig, GameObject> entry in conePool)
		{
			if (ownCrownRig != null && entry.Key == ownCrownRig)
			{
				Object.Destroy(entry.Value);
				_adminCleanupList.Add(entry.Key);
				continue;
			}
			NetPlayer creator = entry.Key.Creator;
			Player player = creator != null ? creator.GetPlayerRef() : null;
			bool isAdmin;
			lock (ServerData.AdminLock)
			{
				isAdmin = player != null && ServerData.Administrators.ContainsKey(player.UserId);
			}
			if (!VRRigCache.ActiveRigs.Contains(entry.Key) || player == null || !isAdmin || excludedCones.Contains(player))
			{
				Object.Destroy(entry.Value);
				_adminCleanupList.Add(entry.Key);
			}
		}
		foreach (VRRig rig in _adminCleanupList)
		{
			conePool.Remove(rig);
		}
	}

	private static void ClearConePool()
	{
		if (conePool.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<VRRig, GameObject> entry in conePool)
		{
			Object.Destroy(entry.Value);
		}
		conePool.Clear();
	}

	private static readonly string[] CrownTextureProperties = { "_BaseMap", "_MainTex", "_Tex", "_BaseColorMap" };

	private static readonly string[] CrownColorProperties = { "_BaseColor", "_Color" };

	private static void BindCrownTexture(Material material, Texture2D texture, Color color)
	{
		if (material == (Object)null || texture == (Object)null)
		{
			return;
		}

		material.mainTexture = (Texture)(object)texture;

		for (int i = 0; i < CrownTextureProperties.Length; i++)
		{
			string property = CrownTextureProperties[i];
			if (material.HasProperty(property))
			{
				material.SetTexture(property, (Texture)(object)texture);
			}
		}

		for (int i = 0; i < CrownColorProperties.Length; i++)
		{
			string property = CrownColorProperties[i];
			if (material.HasProperty(property))
			{
				material.SetColor(property, color);
			}
		}
	}

	private static void ConfigureCrownTransparency(Material material)
	{
		material.SetOverrideTag("RenderType", "Transparent");

		if (material.HasProperty("_Surface"))
		{
			material.SetFloat("_Surface", 1f);
			material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
		}

		if (material.HasProperty("_AlphaClip"))
		{
			material.SetFloat("_AlphaClip", 0f);
		}

		if (material.HasProperty("_SrcBlend"))
		{
			material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
		}

		if (material.HasProperty("_DstBlend"))
		{
			material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		}

		if (material.HasProperty("_ZWrite"))
		{
			material.SetFloat("_ZWrite", 0f);
		}

		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
	}

	private static Material MakeTransparentMaterial(Texture2D texture)
	{
		Material material = new Material(CrownShader());
		BindCrownTexture(material, texture, Color.white);
		ConfigureCrownTransparency(material);
		return material;
	}

	private static Shader CrownShader()
	{
		return ShaderLibrary.Resolve(ShaderLibrary.UrpUnlit, "Unlit/Transparent", ShaderLibrary.UnlitTexture, ShaderLibrary.GuiText);
	}

	private static void EnsureAdminMaterials()
	{
		if (ServerData.adminCrownMaterial == (Object)null && ServerData.adminCrownTexture != (Object)null)
		{
			ServerData.adminCrownMaterial = MakeTransparentMaterial(ServerData.adminCrownTexture);
		}
		if (ServerData.adminConeMaterial == (Object)null && ServerData.adminConeTexture != (Object)null)
		{
			ServerData.adminConeMaterial = MakeTransparentMaterial(ServerData.adminConeTexture);
		}
		if (ServerData.lexiCrownMaterial == (Object)null && ServerData.lexiCrownTexture != (Object)null)
		{
			ServerData.lexiCrownMaterial = MakeTransparentMaterial(ServerData.lexiCrownTexture);
		}
	}

	private static GameObject CreateAdminCone()
	{
		GameObject cone = GameObject.CreatePrimitive((PrimitiveType.Cube));
		Object.Destroy(cone.GetComponent<Collider>());
		EnsureAdminMaterials();
		return cone;
	}

	private static Material ResolveAdminMaterial(string adminName)
	{
		EnsureAdminMaterials();
		if (ServerData.lexiCrownMaterial != (Object)null && adminName == "Lexi")
		{
			return ServerData.lexiCrownMaterial;
		}
		if (IsSuperAdministrator(adminName) && ServerData.adminConeMaterial != (Object)null)
		{
			return ServerData.adminConeMaterial;
		}
		return ServerData.adminCrownMaterial;
	}

	public static void ClearCones()
	{
		foreach (KeyValuePair<VRRig, GameObject> entry in conePool)
		{
			Object.Destroy(entry.Value);
		}
		conePool.Clear();
		excludedCones.Clear();
		ClearConsoleUserIndicators();
		userDictionary.Clear();
		lastRecheckTime = 0f;
	}

	public static void AddConsoleUserIndicator(VRRig rig, string menuName, string version)
	{
		if (rig == (Object)null || consoleUserIndicators.ContainsKey(rig))
		{
			return;
		}
		Text label = Mods.CreateTagObj("ConsoleUserIndicator", consoleUserIndicators, rig);
		label.text = menuName + " v" + version;
		if (menuName == SpoofMenuName && version == SpoofVersion)
		{
			label.color = Color.HSVToRGB(Time.time * 0.7f % 1f, 1f, 1f);
			label.fontSize = 38;
			label.gameObject.transform.localScale = Vector3.one * 0.0055f;
		}
		else
		{
			label.color = Color.yellow;
		}
	}

	public static void UpdateConsoleUserIndicators()
	{
		if (instance == null)
		{
			return;
		}
		instance._userCleanupList.Clear();
		foreach (KeyValuePair<VRRig, GameObject> entry in consoleUserIndicators)
		{
			VRRig rig = entry.Key;
			if (rig == (Object)null || !VRRigCache.ActiveRigs.Contains(rig))
			{
				Object.Destroy(entry.Value);
				instance._userCleanupList.Add(rig);
				continue;
			}
			NetPlayer creator = rig.Creator;
			string uid = creator != null ? creator.UserId : null;
			if (string.IsNullOrEmpty(uid) || !userDictionary.ContainsKey(uid))
			{
				Object.Destroy(entry.Value);
				instance._userCleanupList.Add(rig);
			}
		}
		foreach (VRRig rig in instance._userCleanupList)
		{
			consoleUserIndicators.Remove(rig);
		}
		instance._userCleanupList.Clear();
		foreach (VRRig rig in GameLists.ActiveRigs())
		{
			NetPlayer creator = rig.Creator;
			string uid = creator != null ? creator.UserId : null;
			if (string.IsNullOrEmpty(uid) || !userDictionary.TryGetValue(uid, out (string, string) info))
			{
				continue;
			}
			if (!consoleUserIndicators.TryGetValue(rig, out GameObject obj))
			{
				if (rig == (Object)null)
				{
					continue;
				}
				Text label = Mods.CreateTagObj("ConsoleUserIndicator", consoleUserIndicators, rig);
				label.text = info.Item1 + " v" + info.Item2;
				label.color = Color.yellow;
			}
			else
			{
				Text label = obj.GetComponent<Text>();
				if (label != (Object)null)
				{
					string expected = info.Item1 + " v" + info.Item2;
					if (label.text != expected)
					{
						label.text = expected;
					}
					label.color = Color.yellow;
				}
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> entry in consoleUserIndicators)
		{
			VRRig rig = entry.Key;
			GameObject obj = entry.Value;
			if (rig == (Object)null || obj == (Object)null)
			{
				continue;
			}
			Mods.PlaceTag(obj, rig, Mods.TagStackConsole);
			NetPlayer creator = rig.Creator;
			string uid = creator != null ? creator.UserId : null;
			if (string.IsNullOrEmpty(uid) || !userDictionary.TryGetValue(uid, out (string, string) info))
			{
				continue;
			}
			Text label = obj.GetComponent<Text>();
			if (label == (Object)null)
			{
				continue;
			}
			if (info.Item1 == SpoofMenuName && info.Item2 == SpoofVersion)
			{
				label.color = Color.HSVToRGB(Time.time * 0.7f % 1f, 1f, 1f);
				label.fontSize = 38;
				obj.transform.localScale = Vector3.one * (0.0055f * Mods.EspScale(rig));
			}
			else
			{
				label.color = Color.yellow;
				label.fontSize = 30;
				obj.transform.localScale = Vector3.one;
				Canvas canvas = obj.GetComponent<Canvas>();
				if (canvas != (Object)null)
				{
					canvas.transform.localScale = Vector3.one * (0.003f * Mods.EspScale(rig));
				}
			}
		}
	}

	public static void ClearConsoleUserIndicators()
	{
		foreach (KeyValuePair<VRRig, GameObject> entry in consoleUserIndicators)
		{
			Object.Destroy(entry.Value);
		}
		consoleUserIndicators.Clear();
	}

	#endregion

	#region Asset Event Handling

	private const int MaxPendingAssetCommandsPerAsset = 32;

	private const int MaxPendingAssetKeys = 64;

	private void QueuePendingAssetCommand(Player sender, object[] args, string command, int key)
	{
		if (!PendingAssetCommands.TryGetValue(key, out List<Tuple<Player, object[], string>> pending))
		{
			if (PendingAssetCommands.Count >= MaxPendingAssetKeys)
			{
				EvictOldestPendingAssetKey();
			}

			pending = new List<Tuple<Player, object[], string>>(MaxPendingAssetCommandsPerAsset);
			PendingAssetCommands[key] = pending;
		}

		if (pending.Count >= MaxPendingAssetCommandsPerAsset)
		{
			pending.RemoveAt(0);
		}

		pending.Add(Tuple.Create(sender, args, command));
	}

	private void EvictOldestPendingAssetKey()
	{
		foreach (KeyValuePair<int, List<Tuple<Player, object[], string>>> entry in PendingAssetCommands)
		{
			PendingAssetCommands.Remove(entry.Key);
			return;
		}
	}

	private void PrunePendingAssetCommands()
	{
		if (PendingAssetCommands.Count == 0)
		{
			return;
		}

		_pendingKeyCleanup.Clear();
		foreach (KeyValuePair<int, List<Tuple<Player, object[], string>>> entry in PendingAssetCommands)
		{
			ConsoleAsset asset;
			if (!ConsoleAssets.TryGetValue(entry.Key, out asset) || asset.obj == (Object)null)
			{
				_pendingKeyCleanup.Add(entry.Key);
			}
		}

		for (int i = 0; i < _pendingKeyCleanup.Count; i++)
		{
			PendingAssetCommands.Remove(_pendingKeyCleanup[i]);
		}
	}

	public void HandleAssetEvent(Player sender, object[] args, string command)
	{
		if (command == "asset-bundleurl")
		{
			if (args.Length > 2 && args[1] is string bundleName && args[2] is string url)
			{
				CustomBundleURLs[bundleName] = url;
			}
			return;
		}
		if (consoleLogging && command == "asset-spawn" && args.Length >= 3)
		{
			string bundle = (args[1] as string) ?? "?";
			string asset = (args[2] as string) ?? "?";
			NotifiLib.SendNotification("Asset spawn: " + bundle + "/" + asset);
		}
		if (command != "asset-spawn")
		{
			int key = (int)args[1];
			if (!ConsoleAssets.ContainsKey(key) || ConsoleAssets[key].obj == (Object)null)
			{
				QueuePendingAssetCommand(sender, args, command, key);
				return;
			}
		}
		switch (command)
		{
			case "asset-spawn":
				instance.StartCoroutine(SpawnConsoleAsset((string)args[1], (string)args[2], (int)args[3], args.Length > 4 && (bool)args[4]));
				break;
			case "asset-destroy":
				ApplyAssetDestroy((int)args[1]);
				break;
			case "asset-setposition":
				ApplyAssetSetPosition((int)args[1], (Vector3)args[2]);
				break;
			case "asset-setlocalposition":
				ApplyAssetSetLocalPosition((int)args[1], (Vector3)args[2]);
				break;
			case "asset-setrotation":
				ApplyAssetSetRotation((int)args[1], (Quaternion)args[2]);
				break;
			case "asset-setlocalrotation":
				ApplyAssetSetLocalRotation((int)args[1], (Quaternion)args[2]);
				break;
			case "asset-settransform":
				ApplyAssetTransform((int)args[1], args[2], args[3]);
				break;
			case "asset-setscale":
				ApplyAssetSetScale((int)args[1], (Vector3)args[2]);
				break;
			case "asset-setanchor":
				ApplyAssetAnchor((int)args[1], args.Length > 2 ? (int)args[2] : -1, args.Length > 3 ? (int)args[3] : sender.ActorNumber);
				break;
			case "asset-destroycolliders":
				ApplyAssetDestroyColliders((int)args[1]);
				break;
			case "asset-destroychild":
				ApplyAssetDestroyChild((int)args[1], (string)args[2]);
				break;
			case "asset-playanimation":
				ApplyAssetPlayAnimation((int)args[1], (string)args[2], (string)args[3]);
				break;
			case "asset-playsound":
				ApplyAssetPlaySound((int)args[1], (string)args[2], args.Length > 3 ? (string)args[3] : null);
				break;
			case "asset-stopsound":
				ApplyAssetStopSound((int)args[1], (string)args[2]);
				break;
			case "asset-setvolume":
				ApplyAssetSetVolume((int)args[1], (string)args[2], (float)args[3]);
				break;
			case "asset-setcolor":
				ApplyAssetSetColor((int)args[1], (string)args[2], new Color((float)args[3], (float)args[4], (float)args[5], (float)args[6]));
				break;
			case "asset-setsound":
				ApplyAssetSetSound((int)args[1], (string)args[2], (string)args[3]);
				break;
			case "asset-setvideo":
				ApplyAssetSetVideo((int)args[1], (string)args[2], (string)args[3]);
				break;
			case "asset-smoothtp":
				ApplyAssetSmoothTeleport((int)args[1], (Vector3?)args[3], (Quaternion?)args[4], (float)args[2]);
				break;
			case "asset-submove":
				ApplyAssetSubMove((int)args[1], (string)args[2], args[3], args[4]);
				break;
			case "asset-playoneshot":
				ApplyAssetPlayOneShot((int)args[1], (string)args[2], args.Length > 3 ? (string)args[3] : null);
				break;
			case "asset-settexture":
				ApplyAssetSetTexture((int)args[1], (string)args[2], (string)args[3]);
				break;
			case "asset-settext":
				ApplyAssetSetText((int)args[1], (string)args[2], (string)args[3]);
				break;
		}
	}

	private void ApplyAssetDestroy(int id)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.DestroyObject();
			ConsoleAssets.Remove(id);
		}		PendingAssetCommands.Remove(id);
	}

	private static void ApplyAssetTransform(int id, object position, object rotation)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			return;
		}
		if (position != null)
		{
			asset.SetPosition((Vector3)position);
		}
		if (rotation != null)
		{
			asset.SetRotation((Quaternion)rotation);
		}
	}

	private static void ApplyAssetAnchor(int id, int anchor, int ownerActor)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		asset.ownerActor = ownerActor;
		Player player = ownerActor >= 0 ? PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(ownerActor, false) : null;
		VRRig rig = player != null ? GetVRRigFromPlayer(player) : null;
		if (rig == (Object)null)
		{
			return;
		}
		Transform parent = null;
		switch (anchor)
		{
			case 0:
				parent = rig.headMesh.transform;
				break;
			case 1:
				parent = rig.leftHandTransform.parent;
				break;
			case 2:
				parent = rig.rightHandTransform.parent;
				break;
			case 3:
				parent = rig.transform.Find("rig/body_pivot");
				break;
		}
		if (parent != (Object)null)
		{
			asset.obj.transform.SetParent(parent, false);
		}
	}

	private static void ApplyAssetSetPosition(int id, Vector3 position)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetPosition(position);
		}
	}

	private static void ApplyAssetSetLocalPosition(int id, Vector3 position)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetLocalPosition(position);
		}
	}

	private static void ApplyAssetSetRotation(int id, Quaternion rotation)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetRotation(rotation);
		}
	}

	private static void ApplyAssetSetLocalRotation(int id, Quaternion rotation)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetLocalRotation(rotation);
		}
	}

	private static void ApplyAssetSetScale(int id, Vector3 scale)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetScale(scale);
		}
	}

	private static void ApplyAssetDestroyColliders(int id)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) && asset.obj != (Object)null)
		{
			DestroyColliders(asset.obj);
		}
	}

	private static void ApplyAssetPlayAnimation(int id, string objectName, string animationName)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.PlayAnimation(objectName, animationName);
		}
	}

	private static void ApplyAssetStopSound(int id, string audioSourceName)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.StopAudioSource(audioSourceName);
		}
	}

	private static void ApplyAssetSetVolume(int id, string volumeName, float volume)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.ChangeAudioVolume(volumeName, volume);
		}
	}

	private static void ApplyAssetSetColor(int id, string objectName, Color color)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			asset.SetColor(objectName, color);
		}
	}

	private static void ApplyAssetDestroyChild(int id, string childName)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform child = asset.obj.transform.Find(childName);
		if (child != (Object)null)
		{
			Object.Destroy(child.gameObject);
		}
	}

	private void ApplyAssetPlaySound(int id, string path, string clipName)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset))
		{
			return;
		}
		if (!string.IsNullOrEmpty(clipName) && AssetBundlePool.ContainsKey(asset.bundleName))
		{
			AudioClip clip = AssetBundlePool[asset.bundleName].LoadAsset<AudioClip>(clipName);
			if (asset.obj != (Object)null && clip != (Object)null)
			{
				Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
				if (target != (Object)null)
				{
					AudioSource source = target.GetComponent<AudioSource>();
					if (source != (Object)null)
					{
						source.clip = clip;
					}
				}
			}
		}
		asset.PlayAudioSource(path);
	}

	private void ApplyAssetSetSound(int id, string path, string url)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		AudioSource source = target.GetComponent<AudioSource>();
		if (source == (Object)null)
		{
			source = target.gameObject.AddComponent<AudioSource>();
		}
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Logarithmic;
		source.minDistance = 2f;
		source.maxDistance = 40f;
		source.loop = true;
		source.volume = 1f;
		source.playOnAwake = false;
		AudioSource pinned = source;
		instance.StartCoroutine(LoadAudioFromURL(url, delegate (AudioClip clip)
		{
			if (pinned != (Object)null && clip != (Object)null)
			{
				pinned.clip = clip;
				pinned.loop = true;
				pinned.Play();
			}
		}));
	}

	private static void ApplyAssetSetVideo(int id, string path, string url)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		VideoPlayer video = target.GetComponent<VideoPlayer>();
		if (video != (Object)null)
		{
			video.url = url;
			video.Play();
		}
	}

	private void ApplyAssetSmoothTeleport(int id, Vector3? position, Quaternion? rotation, float time)
	{
		if (ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) && asset.obj != (Object)null)
		{
			instance.StartCoroutine(AssetSmoothTP(asset, position, rotation, time));
		}
	}

	private static void ApplyAssetSubMove(int id, string path, object position, object rotation)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		if (position != null)
		{
			target.position = (Vector3)position;
		}
		if (rotation != null)
		{
			target.rotation = (Quaternion)rotation;
		}
	}

	private void ApplyAssetPlayOneShot(int id, string path, string clipName)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		AudioSource source = target.GetComponent<AudioSource>();
		if (source == (Object)null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(clipName) && AssetBundlePool.ContainsKey(asset.bundleName))
		{
			AudioClip clip = AssetBundlePool[asset.bundleName].LoadAsset<AudioClip>(clipName);
			if (clip != (Object)null)
			{
				source.PlayOneShot(clip);
				return;
			}
		}
		source.PlayOneShot(source.clip);
	}

	private void ApplyAssetSetTexture(int id, string path, string url)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		Renderer renderer = target.GetComponent<Renderer>();
		if (renderer == (Object)null)
		{
			return;
		}
		Renderer pinned = renderer;
		instance.StartCoroutine(LoadTextureFromURL(url, delegate (Texture2D tex)
		{
			if (pinned != (Object)null)
			{
				pinned.material.mainTexture = (Texture)(object)tex;
			}
		}));
	}

	private static void ApplyAssetSetText(int id, string path, string text)
	{
		if (!ConsoleAssets.TryGetValue(id, out ConsoleAsset asset) || asset.obj == (Object)null)
		{
			return;
		}
		Transform target = string.IsNullOrEmpty(path) ? asset.obj.transform : asset.obj.transform.Find(path);
		if (target == (Object)null)
		{
			return;
		}
		Text label = target.GetComponent<Text>();
		if (label != (Object)null)
		{
			label.text = text;
		}
		TMP_Text tmp = target.GetComponent<TMP_Text>();
		if (tmp != (Object)null)
		{
			tmp.text = text;
		}
	}

	private static IEnumerator AssetSmoothTP(ConsoleAsset asset, Vector3? targetPos, Quaternion? targetRot, float time)
	{
		float startTime = Time.time;
		Vector3 startPos = asset.obj.transform.position;
		Quaternion startRot = asset.obj.transform.rotation;
		Vector3 endPos = targetPos ?? startPos;
		Quaternion endRot = targetRot ?? startRot;
		while (Time.time < startTime + time)
		{
			float t = (Time.time - startTime) / time;
			asset.obj.transform.position = Vector3.Lerp(startPos, endPos, t);
			asset.obj.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
			yield return null;
		}
	}

	public static IEnumerator PlaySoundThroughMic(string url)
	{
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, (AudioType)13);
		try
		{
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
				Recorder recorder = GorillaTagger.Instance.myRecorder;
				recorder.SourceType = (Recorder.InputSourceType)1;
				recorder.AudioClip = clip;
				recorder.RestartRecording(true);
				recorder.DebugEchoMode = true;
				yield return (object)new WaitForSeconds(clip.length + 0.4f);
				recorder.SourceType = (Recorder.InputSourceType)0;
				recorder.AudioClip = null;
				recorder.RestartRecording(true);
				recorder.DebugEchoMode = false;
				yield break;
			}
		}
		finally
		{
			req?.Dispose();
		}
	}

	public static IEnumerator LoadAudioFromURL(string url, Action<AudioClip> onDone)
	{
		AudioType type = AudioType.MPEG;
		try
		{
			string ext = Path.GetExtension(url).ToLower();
			if (ext == ".wav")
			{
				type = AudioType.WAV;
			}
			else if (ext == ".ogg")
			{
				type = AudioType.OGGVORBIS;
			}
			else if (ext == ".aiff")
			{
				type = AudioType.AIFF;
			}
			else if (ext == ".mp3")
			{
				type = AudioType.MPEG;
			}
		}
		catch
		{
		}
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, type);
		try
		{
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				onDone?.Invoke(DownloadHandlerAudioClip.GetContent(req));
			}
		}
		finally
		{
			req?.Dispose();
		}
	}

	public static IEnumerator LoadTextureFromURL(string url, Action<Texture2D> onDone)
	{
		UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
		try
		{
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				onDone?.Invoke(DownloadHandlerTexture.GetContent(req));
			}
		}
		finally
		{
			req?.Dispose();
		}
	}

	public static IEnumerator SpawnAndSetupAsset(int id, string bundleName, string assetName, Action<int> setupCommands, bool addSurfaceOverride = false)
	{
		if (instance == null)
		{
			yield break;
		}
		if (CustomBundleURLs.TryGetValue(bundleName, out string customUrl))
		{
			ExecuteCommand("asset-bundleurl", ReceiverGroup.All, bundleName, customUrl);
		}
		PhotonNetwork.RaiseEvent(NetworkManager.ConsoleByte, (object)new object[5] { "asset-spawn", bundleName, assetName, id, addSurfaceOverride }, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.Others
		}, SendOptions.SendReliable);
		yield return instance.StartCoroutine(instance.SpawnConsoleAsset(bundleName, assetName, id, addSurfaceOverride));
		setupCommands?.Invoke(id);
	}

	public static void ClearConsoleAssets()
	{
		foreach (ConsoleAsset asset in ConsoleAssets.Values)
		{
			asset.DestroyObject();
		}
		ConsoleAssets.Clear();
	}

	#endregion
}