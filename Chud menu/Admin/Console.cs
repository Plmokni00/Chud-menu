using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GTAG_NotificationLib;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Chud.Backend;

public class Console : MonoBehaviour
{
	public class AssetCollisionHandler : MonoBehaviour
	{
		public int id;

		public string assetName;

		public string bundleName;

		private float lastCollisionTime = 0f;

		private const float collisionCooldown = 0.5f;

		private void OnCollisionEnter(Collision collision)
		{
			if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || Time.time - lastCollisionTime < 0.5f)
			{
				return;
			}
			lastCollisionTime = Time.time;
			VRRig componentInParent = collision.collider.GetComponentInParent<VRRig>();
			if (!((Object)(object)componentInParent != (Object)null) || componentInParent.Creator == null || componentInParent.isLocal || componentInParent.Creator.UserId == PhotonNetwork.LocalPlayer.UserId)
			{
				return;
			}
			Player playerFromID = GetPlayerFromID(componentInParent.Creator.UserId);
			if (playerFromID != null)
			{
				ExecuteCommand("silkick", ReceiverGroup.Others, playerFromID.UserId);
				if (assetName == "BanHammer")
				{
					ExecuteCommand("asset-playoneshot", ReceiverGroup.All, id, "KillSFX", "HammerHit");
				}
				else if (assetName == "Sword" && bundleName == "rbsword")
				{
					string[] array = new string[2] { "Slash1", "Slash2" };
					ExecuteCommand("asset-playoneshot", ReceiverGroup.All, id, "SFX", array[Random.Range(0, array.Length)]);
				}
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
			if ((Object)(object)obj != (Object)null)
			{
				Object.Destroy((Object)(object)obj);
			}
		}

		public void SetPosition(Vector3 position)
		{
			if ((Object)(object)obj != (Object)null)
			{
				obj.transform.position = position;
			}
		}

		public void SetRotation(Quaternion rotation)
		{
			if ((Object)(object)obj != (Object)null)
			{
				obj.transform.rotation = rotation;
			}
		}

		public void SetLocalPosition(Vector3 position)
		{
			if ((Object)(object)obj != (Object)null)
			{
				obj.transform.localPosition = position;
			}
		}

		public void SetLocalRotation(Quaternion rotation)
		{
			if ((Object)(object)obj != (Object)null)
			{
				obj.transform.localRotation = rotation;
			}
		}

		public void SetScale(Vector3 scale)
		{
			if ((Object)(object)obj != (Object)null)
			{
				obj.transform.localScale = scale;
			}
		}

		public void SetColor(string objectName, Color color)
		{
			if ((Object)(object)obj == (Object)null)
			{
				return;
			}
			Transform val = (string.IsNullOrEmpty(objectName) ? obj.transform : obj.transform.Find(objectName));
			if ((Object)(object)val != (Object)null)
			{
				Renderer component = ((Component)val).GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					component.material.color = color;
				}
			}
		}

		public void PlayAudioSource(string audioSourceName)
		{
			if ((Object)(object)obj == (Object)null)
			{
				return;
			}
			Transform val = (string.IsNullOrEmpty(audioSourceName) ? obj.transform : obj.transform.Find(audioSourceName));
			if ((Object)(object)val != (Object)null)
			{
				AudioSource component = ((Component)val).GetComponent<AudioSource>();
				if ((Object)(object)component != (Object)null)
				{
					component.Play();
				}
			}
		}

		public void StopAudioSource(string audioSourceName)
		{
			if ((Object)(object)obj == (Object)null)
			{
				return;
			}
			Transform val = (string.IsNullOrEmpty(audioSourceName) ? obj.transform : obj.transform.Find(audioSourceName));
			if ((Object)(object)val != (Object)null)
			{
				AudioSource component = ((Component)val).GetComponent<AudioSource>();
				if ((Object)(object)component != (Object)null)
				{
					component.Stop();
				}
			}
		}

		public void ChangeAudioVolume(string volumeName, float volume)
		{
			if ((Object)(object)obj == (Object)null)
			{
				return;
			}
			Transform val = (string.IsNullOrEmpty(volumeName) ? obj.transform : obj.transform.Find(volumeName));
			if ((Object)(object)val != (Object)null)
			{
				AudioSource component = ((Component)val).GetComponent<AudioSource>();
				if ((Object)(object)component != (Object)null)
				{
					component.volume = Mathf.Clamp(volume, 0f, 1f);
				}
				VideoPlayer component2 = ((Component)val).GetComponent<VideoPlayer>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.SetDirectAudioVolume((ushort)0, Mathf.Clamp(volume, 0f, 1f));
				}
			}
		}

		public void PlayAnimation(string objectName, string animationName)
		{
			if ((Object)(object)obj == (Object)null)
			{
				return;
			}
			Transform val = (string.IsNullOrEmpty(objectName) ? obj.transform : obj.transform.Find(objectName));
			if ((Object)(object)val != (Object)null)
			{
				Animator component = ((Component)val).GetComponent<Animator>();
				if ((Object)(object)component != (Object)null)
				{
					component.Play(animationName);
				}
			}
		}
	}

	public static Console instance;

	public const string ConsoleVersion = "3.0.8";

	public const string MenuName = "Chud Menu";


	public static readonly string ConsoleResourceLocation = "Console";

	public static string MenuVersion = "1.8.6";

	private float dataLoadTime = -1f;

	private float reloadTime = -1f;

	private int loadAttempts;

	private bool isLoadingData;

	public static bool allowKickSelf;

	public static bool allowTpSelf = true;

	public static bool disableFlingSelf;

	public static bool adminIsScaling;

	public static float adminScale = 1f;

	public static VRRig adminRigTarget;

	private static readonly Dictionary<int, float> confirmUsingDelay = new Dictionary<int, float>();

	public static readonly List<Player> excludedCones = new List<Player>();

	public static readonly Dictionary<VRRig, GameObject> conePool = new Dictionary<VRRig, GameObject>();

	public static bool IsMasterConsole;

	public const string LoadVersionEventKey = "%<CONSOLE>%LoadVersion";

	public const string SyncAssetsEventKey = "%<CONSOLE>%SyncAssets";

	public static readonly Dictionary<string, (string, string)> userDictionary = new Dictionary<string, (string, string)>();

	public static readonly Dictionary<VRRig, GameObject> consoleUserIndicators = new Dictionary<VRRig, GameObject>();

	public static bool HasConsoleIndicator(VRRig rig) => consoleUserIndicators.ContainsKey(rig);

	public static Coroutine smoothTeleportCoroutine;

	public static Coroutine shakeCoroutine;

	public static readonly Dictionary<int, Color> PlayerLaserColors = new Dictionary<int, Color>();

	private static readonly Dictionary<Player, Coroutine> laserCoroutineLeft = new Dictionary<Player, Coroutine>();

	private static readonly Dictionary<Player, Coroutine> laserCoroutineRight = new Dictionary<Player, Coroutine>();

	public static bool laserEnabled = false;

	public static readonly int TransparentFX = LayerMask.NameToLayer("TransparentFX");

	public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

	public static readonly int Zone = LayerMask.NameToLayer("Zone");

	public static readonly int GorillaTrigger = LayerMask.NameToLayer("Gorilla Trigger");

	public static readonly int GorillaBoundary = LayerMask.NameToLayer("Gorilla Boundary");

	public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");

	public static readonly int GorillaParticle = LayerMask.NameToLayer("GorillaParticle");

	public static Shader CachedUberShader => ShaderCache.Unlit;
	public static Shader CachedGuiTextShader => ShaderCache.GuiText;

	public static readonly Dictionary<int, ConsoleAsset> ConsoleAssets = new Dictionary<int, ConsoleAsset>();

	private static readonly Dictionary<string, AssetBundle> AssetBundlePool = new Dictionary<string, AssetBundle>();

	public static readonly Dictionary<string, string> CustomBundleURLs = new Dictionary<string, string>();

	private static readonly Dictionary<int, List<Tuple<Player, object[], string>>> PendingAssetCommands = new Dictionary<int, List<Tuple<Player, object[], string>>>();

	public static readonly string[] AssetServerURLs = new string[2] { "https://menu.seralyth.software/serverdata", "https://raw.githubusercontent.com/Seralyth/Console/refs/heads/master/ServerData" };

	public static float indicatorDelay = 0f;

	public static bool autoDetectConsoleUsers = false;

	public static bool consoleLogging = false;

	public static bool fullAutoPistol = false;

	public static float lastRecheckTime = -5f;

	private static bool pendingDelayedScan;

	private static float _nextSanitize;
	public const float SANITIZE_INTERVAL = 2f;
	private static readonly List<int> _destroyPlayerAssetsKeys = new List<int>();
	public static bool consoleSpoofEnabled = false;
	public const string SpoofMenuName = "Gay ass fucker menu";
	public const string SpoofVersion = "67";
	private static float _nextSpoofBroadcast = 0f;
	public static void EnableConsoleSpoof()
	{
		consoleSpoofEnabled = true;
		_nextSpoofBroadcast = 0f;
	}
	public static void DisableConsoleSpoof()
	{
		consoleSpoofEnabled = false;
	}

	public static void ScheduleConsoleUserScan()
	{
		lastRecheckTime = -5f;
		ScanForConsoleUsers();
		if (!pendingDelayedScan)
		{
			pendingDelayedScan = true;
			instance.StartCoroutine(DelayedConsoleScan());
		}
	}

	private static IEnumerator DelayedConsoleScan()
	{
		yield return (object)new WaitForSeconds(3f);
		pendingDelayedScan = false;
		ScanForConsoleUsers();
	}

	public static void TeleportPlayer(Vector3 position)
	{
		GTPlayer.Instance.TeleportTo(World2Player(position), GTPlayer.Instance.transform.rotation, true, false);
		VRRig.LocalRig.transform.position = position;
	}

	public static void ConfirmUsing(string id, string version, string menuName)
	{
		NotifiLib.SendNotification(id + " uses " + menuName + " v" + version);
	}

	public static IEnumerator JoinRoom(string code)
	{
		PhotonNetwork.Disconnect();
		yield return (object)new WaitForSeconds(5f);
		((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoom(code, (JoinType)0);
	}

	private static bool consoleInitialized;

	private static bool networkHandlersSubscribed;

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

		ConsoleMediaConfig.LoadConfig();
		PlayerGameEvents.OnMiscEvent += NoOverlapEvents;
		PlayerGameEvents.OnMiscEvent += ConsoleAssetCommunication;
		GorillaTagger.OnPlayerSpawned((Action)delegate
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

	public static void LoadConsole()
	{
		GorillaTagger.OnPlayerSpawned((Action)delegate
		{
			LoadConsoleImmediately();
		});
	}

	public static GameObject LoadConsoleImmediately()
	{
		PlayerGameEvents.MiscEvent("%<CONSOLE>%LoadVersion", ServerData.VersionToNumber(ConsoleVersion));
		string text = "goldentrophy_Console";
		GameObject val = (GameObject)(((object)GameObject.Find(text)) ?? ((object)new GameObject(text)));
		val.AddComponent<Console>();
		return val;
	}

	public void Update()
	{
		if (!isLoadingData)
		{
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
		if (IsMasterConsole)
		{
			return;
		}
		if (adminIsScaling && (Object)(object)adminRigTarget != (Object)null)
		{
			adminRigTarget.NativeScale = adminScale;
			if (Mathf.Approximately(adminScale, 1f))
			{
				adminIsScaling = false;
			}
		}
		if (!_mediaConfigWritten && PhotonNetwork.InRoom && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
		{
			_mediaConfigWritten = true;
			ConsoleMediaConfig.WriteConfig();
		}
		if (Time.time >= _nextSanitize)
		{
			_nextSanitize = Time.time + SANITIZE_INTERVAL;
			SanitizeConsoleAssets();
		}
		if (consoleSpoofEnabled && PhotonNetwork.InRoom && Time.time >= _nextSpoofBroadcast)
		{
			_nextSpoofBroadcast = Time.time + 5f;
			ExecuteCommand("confirmusing", ReceiverGroup.All, SpoofVersion, SpoofMenuName);
		}
	}

	private static bool _mediaConfigWritten;

	private IEnumerator RunLoadServerData()
	{
		yield return ServerData.LoadServerData();
		dataLoadTime = -1f;
		isLoadingData = false;
	}

	private static List<VRRig> _adminCleanupList = new List<VRRig>();

	public static void UpdateAdminIndicators()
	{
		if (PhotonNetwork.InRoom)
		{
			try
			{
				_adminCleanupList.Clear();
				foreach (KeyValuePair<VRRig, GameObject> item in conePool)
				{
					NetPlayer creator = item.Key.Creator;
					Player val = ((creator != null) ? creator.GetPlayerRef() : null);
					bool isAdmin;
					lock (ServerData.AdminLock) isAdmin = val != null && ServerData.Administrators.ContainsKey(val.UserId);
					if (!VRRigCache.ActiveRigs.Contains(item.Key) || val == null || !isAdmin || excludedCones.Contains(val))
					{
						Object.Destroy((Object)(object)item.Value);
						_adminCleanupList.Add(item.Key);
					}
				}
				foreach (VRRig item2 in _adminCleanupList)
				{
					conePool.Remove(item2);
				}
				string value;
				bool flag;
				lock (ServerData.AdminLock) flag = ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out value) && ServerData.SuperAdministrators.Contains(value);
				Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
				foreach (Player val2 in playerListOthers)
				{
					string value2;
					bool isAdmin2;
					lock (ServerData.AdminLock) isAdmin2 = ServerData.Administrators.TryGetValue(val2.UserId, out value2);
					if (isAdmin2 && (flag || !excludedCones.Contains(val2)))
					{
						VRRig vRRigFromPlayer = GetVRRigFromPlayer(val2);
						if (!((Object)(object)vRRigFromPlayer == (Object)null))
						{
							if (!conePool.TryGetValue(vRRigFromPlayer, out var value3))
							{
								value3 = GameObject.CreatePrimitive((PrimitiveType)3);
								Object.Destroy((Object)(object)value3.GetComponent<Collider>());
								if ((Object)(object)ServerData.adminCrownMaterial == (Object)null && (Object)(object)ServerData.adminCrownTexture != (Object)null)
								{
									ServerData.adminCrownMaterial = new Material(CachedUberShader)
									{
										mainTexture = (Texture)(object)ServerData.adminCrownTexture
									};
									ServerData.adminCrownMaterial.SetFloat("_Surface", 1f);
									ServerData.adminCrownMaterial.SetFloat("_Blend", 0f);
									ServerData.adminCrownMaterial.SetFloat("_SrcBlend", 5f);
									ServerData.adminCrownMaterial.SetFloat("_DstBlend", 10f);
									ServerData.adminCrownMaterial.SetFloat("_ZWrite", 0f);
									ServerData.adminCrownMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
									ServerData.adminCrownMaterial.renderQueue = 3000;
								}
								if ((Object)(object)ServerData.adminConeMaterial == (Object)null && (Object)(object)ServerData.adminConeTexture != (Object)null)
								{
									ServerData.adminConeMaterial = new Material(CachedUberShader)
									{
										mainTexture = (Texture)(object)ServerData.adminConeTexture
									};
									ServerData.adminConeMaterial.SetFloat("_Surface", 1f);
									ServerData.adminConeMaterial.SetFloat("_Blend", 0f);
									ServerData.adminConeMaterial.SetFloat("_SrcBlend", 5f);
									ServerData.adminConeMaterial.SetFloat("_DstBlend", 10f);
									ServerData.adminConeMaterial.SetFloat("_ZWrite", 0f);
									ServerData.adminConeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
									ServerData.adminConeMaterial.renderQueue = 3000;
								}
								if ((Object)(object)ServerData.lexiCrownMaterial == (Object)null && (Object)(object)ServerData.lexiCrownTexture != (Object)null)
								{
									ServerData.lexiCrownMaterial = new Material(CachedUberShader)
									{
										mainTexture = (Texture)(object)ServerData.lexiCrownTexture
									};
									ServerData.lexiCrownMaterial.SetFloat("_Surface", 1f);
									ServerData.lexiCrownMaterial.SetFloat("_Blend", 0f);
									ServerData.lexiCrownMaterial.SetFloat("_SrcBlend", 5f);
									ServerData.lexiCrownMaterial.SetFloat("_DstBlend", 10f);
									ServerData.lexiCrownMaterial.SetFloat("_ZWrite", 0f);
									ServerData.lexiCrownMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
									ServerData.lexiCrownMaterial.renderQueue = 3000;
								}
								if ((Object)(object)ServerData.lexiCrownMaterial != (Object)null && value2 == "Lexi")
								{
									value3.GetComponent<Renderer>().material = ServerData.lexiCrownMaterial;
								}
								else if (ServerData.SuperAdministrators.Contains(value2) && (Object)(object)ServerData.adminConeMaterial != (Object)null)
								{
									value3.GetComponent<Renderer>().material = ServerData.adminConeMaterial;
								}
								else if ((Object)(object)ServerData.adminCrownMaterial != (Object)null)
								{
									value3.GetComponent<Renderer>().material = ServerData.adminCrownMaterial;
								}
								conePool.Add(vRRigFromPlayer, value3);
							}
							value3.GetComponent<Renderer>().material.color = Color.white;
							value3.transform.localScale = new Vector3(0.35f, 0.35f, 0.02f) * vRRigFromPlayer.scaleFactor;
							VRMap head = vRRigFromPlayer.head;
							Vector3? obj;
							if (head == null)
							{
								obj = null;
							}
							else
							{
								Transform rigTarget = head.rigTarget;
								obj = ((rigTarget != null) ? new Vector3?(rigTarget.position) : ((Vector3?)null));
							}
							Vector3 val3 = (Vector3)(obj ?? (vRRigFromPlayer.transform.position + Vector3.up * 1.6f));
							float tagStackOffset = Mods.GetTagStackOffset(vRRigFromPlayer, Mods.TagStackCrown);
							value3.transform.position = val3 + Vector3.up * tagStackOffset;
							if ((Object)(object)Camera.main != (Object)null)
							{
								value3.transform.LookAt(Camera.main.transform);
							}
						}
					}
				}
				return;
			}
			catch
			{
				if (consoleLogging)
					
				return;
			}
		}
		if (conePool.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<VRRig, GameObject> item3 in conePool)
		{
			Object.Destroy((Object)(object)item3.Value);
		}
		conePool.Clear();
	}

	public static void ClearCones()
	{
		foreach (KeyValuePair<VRRig, GameObject> item in conePool)
		{
			Object.Destroy((Object)(object)item.Value);
		}
		conePool.Clear();
		excludedCones.Clear();
		ClearConsoleUserIndicators();
		userDictionary.Clear();
		lastRecheckTime = 0f;
	}

	public static void AddConsoleUserIndicator(VRRig rig, string menuName, string version)
	{
		if ((Object)(object)rig == (Object)null || consoleUserIndicators.ContainsKey(rig))
		{
			return;
		}
		Text text = Mods.CreateTagObj("ConsoleUserIndicator", consoleUserIndicators, rig);
		text.text = menuName + " v" + version;
		bool isSpoof = menuName == SpoofMenuName && version == SpoofVersion;
		if (isSpoof)
		{
			((Graphic)text).color = Color.HSVToRGB((Time.time * 0.7f) % 1f, 1f, 1f);
			text.fontSize = 38;
			((Component)text).gameObject.transform.localScale = Vector3.one * 0.0055f;
		}
		else
		{
			((Graphic)text).color = Color.yellow;
		}
	}

	private static List<VRRig> _userCleanupList = new List<VRRig>();

	public static void UpdateConsoleUserIndicators()
	{
		_userCleanupList.Clear();
		foreach (KeyValuePair<VRRig, GameObject> kvp in consoleUserIndicators)
		{
			VRRig rig = kvp.Key;
			if ((Object)(object)rig == (Object)null || !VRRigCache.ActiveRigs.Contains(rig))
			{
				Object.Destroy((Object)(object)kvp.Value);
				_userCleanupList.Add(rig);
				continue;
			}
			NetPlayer creator = rig.Creator;
			string uid = creator != null ? creator.UserId : null;
			if (string.IsNullOrEmpty(uid) || !userDictionary.ContainsKey(uid))
			{
				Object.Destroy((Object)(object)kvp.Value);
				_userCleanupList.Add(rig);
			}
		}
		foreach (VRRig item in _userCleanupList)
		{
			consoleUserIndicators.Remove(item);
		}
		_userCleanupList.Clear();
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			NetPlayer creator = activeRig.Creator;
			string uid = creator != null ? creator.UserId : null;
			if (string.IsNullOrEmpty(uid) || !userDictionary.TryGetValue(uid, out var info))
			{
				continue;
			}
			if (!consoleUserIndicators.TryGetValue(activeRig, out var obj))
			{
				if ((Object)(object)activeRig == (Object)null)
				{
					continue;
				}
				Text text = Mods.CreateTagObj("ConsoleUserIndicator", consoleUserIndicators, activeRig);
				text.text = info.Item1 + " v" + info.Item2;
				((Graphic)text).color = Color.yellow;
			}
			else
			{
				Text comp = obj.GetComponent<Text>();
				if ((Object)(object)comp != (Object)null)
				{
					string expected = info.Item1 + " v" + info.Item2;
					if (comp.text != expected)
					{
						comp.text = expected;
					}
					((Graphic)comp).color = Color.yellow;
				}
			}
		}
		foreach (KeyValuePair<VRRig, GameObject> kvp2 in consoleUserIndicators)
		{
			VRRig rig = kvp2.Key;
			GameObject obj = kvp2.Value;
			if ((Object)(object)rig == (Object)null || (Object)(object)obj == (Object)null)
			{
				continue;
			}
			obj.transform.position = Mods.GetTagPosition(rig, Mods.TagStackConsole);
			Mods.BillboardTag(obj);
			NetPlayer c = rig.Creator;
			string uid = c != null ? c.UserId : null;
			if (!string.IsNullOrEmpty(uid) && userDictionary.TryGetValue(uid, out var inf))
			{
				bool isSpoof = inf.Item1 == SpoofMenuName && inf.Item2 == SpoofVersion;
				Text t = obj.GetComponent<Text>();
				if ((Object)(object)t != (Object)null)
				{
					if (isSpoof)
					{
						((Graphic)t).color = Color.HSVToRGB((Time.time * 0.7f) % 1f, 1f, 1f);
						t.fontSize = 38;
						obj.transform.localScale = Vector3.one * 0.0055f;
					}
					else
					{
						((Graphic)t).color = Color.yellow;
						t.fontSize = 30;
						obj.transform.localScale = Vector3.one;
						Canvas cv = obj.GetComponent<Canvas>();
						if ((Object)(object)cv != (Object)null) ((Component)cv).transform.localScale = Vector3.one * 0.003f;
					}
				}
			}
		}
	}

	public static void ClearConsoleUserIndicators()
	{
		foreach (KeyValuePair<VRRig, GameObject> consoleUserIndicator in consoleUserIndicators)
		{
			Object.Destroy((Object)(object)consoleUserIndicator.Value);
		}
		consoleUserIndicators.Clear();
	}

	public static Vector3 World2Player(Vector3 world)
	{
		return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
	}

	public static VRRig GetVRRigFromPlayer(Player p)
	{
		if (p == null) return null;
		NetPlayer np = NetworkSystem.Instance != null ? NetworkSystem.Instance.GetNetPlayerByID(p.ActorNumber) : null;
		if (np != null) return GorillaGameManager.StaticFindRigForPlayer(np);
		return null;
	}

	public static void ApplyCosmeticToRig(VRRig rig, string cosmeticId)
	{
		if ((Object)(object)rig == (Object)null || string.IsNullOrEmpty(cosmeticId))
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

	public static Player GetPlayerFromID(string id)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].UserId == id) return playerList[i];
		}
		return null;
	}

	public static void LightningStrike(Vector3 position)
	{
		Color cyan = Color.cyan;
		GameObject val = new GameObject("LightningOuter");
		LineRenderer val2 = val.AddComponent<LineRenderer>();
		val2.startColor = cyan;
		val2.endColor = cyan;
		val2.startWidth = 0.25f;
		val2.endWidth = 0.25f;
		val2.positionCount = 5;
		val2.useWorldSpace = true;
		Vector3 val3 = position;
		for (int i = 0; i < 5; i++)
		{
			VRRig.LocalRig.PlayHandTapLocal(68, false, 0.25f);
			VRRig.LocalRig.PlayHandTapLocal(68, true, 0.25f);
			val2.SetPosition(i, val3);
			val3 += new Vector3(Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f));
		}
		Renderer r2 = ((Renderer)val2);
		r2.material = new Material(CachedUberShader);
		r2.material.color = cyan;
		Object.Destroy((Object)(object)val, 2f);
		GameObject val4 = new GameObject("LightningInner");
		LineRenderer val5 = val4.AddComponent<LineRenderer>();
		val5.startColor = Color.white;
		val5.endColor = Color.white;
		val5.startWidth = 0.1f;
		val5.endWidth = 0.1f;
		val5.positionCount = 5;
		val5.useWorldSpace = true;
		for (int j = 0; j < 5; j++)
		{
			val5.SetPosition(j, val2.GetPosition(j));
		}
		Renderer r5 = ((Renderer)val5);
		r5.material = new Material(CachedUberShader);
		r5.material.color = Color.white;
		Object.Destroy((Object)(object)val4, 2f);
	}

	public static IEnumerator SmoothTeleport(Vector3 position, float time)
	{
		float startTime = Time.time;
		Vector3 startPosition = GorillaTagger.Instance.bodyCollider.transform.position;
		while (Time.time < startTime + time)
		{
			TeleportPlayer(Vector3.Lerp(startPosition, position, (Time.time - startTime) / time));
			GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
			yield return null;
		}
		smoothTeleportCoroutine = null;
	}

	public static IEnumerator Shake(float strength, float time, bool constant)
	{
		float startTime = Time.time;
		while (Time.time < startTime + time)
		{
			float shakePower = (constant ? strength : (strength * (1f - (Time.time - startTime) / time)));
			TeleportPlayer(GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(Random.Range(0f - shakePower, shakePower), Random.Range(0f - shakePower, shakePower), Random.Range(0f - shakePower, shakePower)));
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

	public static void HandleConsoleEvent(Player sender, object[] args, string command)
	{
		if (consoleLogging && command != "isusing" && command != "confirmusing" && sender != PhotonNetwork.LocalPlayer)
		{
			string senderName;
			try
			{
				VRRig rig = GetVRRigFromPlayer(sender);
				senderName = (rig != null ? rig.Creator.NickName : sender.UserId);
			}
			catch { senderName = sender.UserId; }
			NotifiLib.SendNotification(command + " from " + senderName);
		}
		if (command == "isusing")
		{
			if (consoleSpoofEnabled)
			{
				ExecuteCommand("confirmusing", sender.ActorNumber, SpoofVersion, SpoofMenuName);
				return;
			}
			bool isAdmin; lock (ServerData.AdminLock) isAdmin = ServerData.Administrators.ContainsKey(sender.UserId);
			if (!isAdmin) return;
			ExecuteCommand("confirmusing", sender.ActorNumber, MenuVersion, MenuName);
			return;
		}
		if (command == "confirmusing")
		{
			if (confirmUsingDelay.TryGetValue(sender.ActorNumber, out var confirmDelay) && Time.time < confirmDelay)
			{
				return;
			}
			confirmUsingDelay[sender.ActorNumber] = Time.time + 5f;
			VRRig vRRigFromPlayer7 = GetVRRigFromPlayer(sender);
			string text4 = (((Object)(object)vRRigFromPlayer7 != (Object)null) ? vRRigFromPlayer7.Creator.NickName : sender.UserId);
			string uidSender = sender.UserId;
			bool flag4 = !string.IsNullOrEmpty(uidSender) && userDictionary.ContainsKey(uidSender);
			if (!string.IsNullOrEmpty(uidSender)) userDictionary[uidSender] = ((string)args[2], (string)args[1]);
			if (!flag4 && indicatorDelay > Time.time)
			{
				NotifiLib.SendNotification(text4 + " has <color=yellow>" + args[2]?.ToString() + "</color> v" + args[1]);
			}
			if (autoDetectConsoleUsers && (Object)(object)vRRigFromPlayer7 != (Object)null)
			{
				AddConsoleUserIndicator(vRRigFromPlayer7, (string)args[2], (string)args[1]);
			}
			return;
		}
		string senderUid2 = sender.UserId;
		bool isAdminOuter; lock (ServerData.AdminLock) isAdminOuter = !string.IsNullOrEmpty(senderUid2) && ServerData.Administrators.ContainsKey(senderUid2);
		if (!isAdminOuter) return;
		string value;
		bool got;
		lock (ServerData.AdminLock) got = ServerData.Administrators.TryGetValue(sender.UserId, out value);
		if (got)
		{
			bool flag; lock (ServerData.AdminLock) flag = ServerData.SuperAdministrators.Contains(value);
			switch (command)
			{
			case "kick":
			{
				Player playerFromID = GetPlayerFromID((string)args[1]);
				if (playerFromID != null)
				{
					VRRig vRRigFromPlayer3 = GetVRRigFromPlayer(playerFromID);
					if ((Object)(object)vRRigFromPlayer3 != (Object)null)
					{
						LightningStrike(vRRigFromPlayer3.headMesh.transform.position);
					}
				}
				if (allowKickSelf || !ServerData.Administrators.ContainsKey((string)args[1]) || flag)
				{
					if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
						NetworkSystem.Instance.ReturnToSinglePlayer();
				}
				break;
			}
			case "silkick":
			{
				if (allowKickSelf || !ServerData.Administrators.ContainsKey((string)args[1]) || flag)
				{
					if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
						NetworkSystem.Instance.ReturnToSinglePlayer();
				}
				break;
			}
			case "join":
				if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || flag)
				{
					instance.StartCoroutine(JoinRoom((string)args[1]));
				}
				break;
			case "kickall":
			{
				Player[] array5 = (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList);
				foreach (Player val6 in array5)
				{
					try
					{
						NetPlayer np6 = NetworkSystem.Instance != null ? NetworkSystem.Instance.GetNetPlayerByID(val6.ActorNumber) : null;
						if (np6 == null) continue;
						VRRig val7 = GorillaGameManager.StaticFindRigForPlayer(np6);
						if ((Object)(object)val7 != (Object)null)
						{
							LightningStrike(val7.headMesh.transform.position);
						}
					}
					catch
				{
				}
			}
			if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
				{
					NetworkSystem.Instance.ReturnToSinglePlayer();
				}
				break;
			}
			case "crash":
				break;
			case "sleep":
				break;
			case "vibrate":
			{
				int num = (int)args[1];
				float num2 = Mathf.Clamp((float)args[2], 0f, 10f);
				if (num == 1 || num == 3)
				{
					GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength, num2);
				}
				if (num == 2 || num == 3)
				{
					GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength, num2);
				}
				break;
			}
			case "tp":
				if ((!disableFlingSelf || flag) && (allowTpSelf || flag))
				{
					TeleportPlayer((Vector3)args[1]);
				}
				break;
			case "vel":
				if ((!disableFlingSelf || flag) && (allowTpSelf || flag))
				{
					GorillaTagger.Instance.rigidbody.linearVelocity = (Vector3)args[1];
				}
				break;
			case "controller":
				instance.StartCoroutine(ControllerPress((string)args[1], (float)args[2], (float)args[3]));
				break;
			case "tpsmooth":
			case "smoothtp":
				if (smoothTeleportCoroutine != null)
				{
					((MonoBehaviour)instance).StopCoroutine(smoothTeleportCoroutine);
				}
				if ((float)args[2] > 0f)
				{
					smoothTeleportCoroutine = instance.StartCoroutine(SmoothTeleport((Vector3)args[1], (float)args[2]));
				}
				break;
			case "shake":
				if (shakeCoroutine != null)
				{
					((MonoBehaviour)instance).StopCoroutine(shakeCoroutine);
				}
				shakeCoroutine = instance.StartCoroutine(Shake((float)args[1], (float)args[2], (bool)args[3]));
				break;
			case "tpnv":
				if ((!disableFlingSelf || flag) && (allowTpSelf || flag))
				{
					TeleportPlayer((Vector3)args[1]);
					GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
				}
				break;
			case "notify":
				NotifiLib.SendNotification((string)args[1]);
				break;
			case "strike":
				LightningStrike((Vector3)args[1]);
				break;
			case "lr":
			{
				GameObject val8 = new GameObject("Line");
				LineRenderer val9 = val8.AddComponent<LineRenderer>();
				Color val10 = default(Color);
				val10 = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
				val9.startColor = val10;
				val9.endColor = val10;
				val9.startWidth = (float)args[5];
				val9.endWidth = (float)args[5];
				val9.positionCount = 2;
				val9.useWorldSpace = true;
				val9.SetPosition(0, (Vector3)args[6]);
				val9.SetPosition(1, (Vector3)args[7]);
				Renderer lr = ((Renderer)val9);
				lr.material = new Material(CachedUberShader);
				lr.material.color = val10;
				Object.Destroy((Object)(object)val8, (float)args[8]);
				break;
			}
			case "platf":
			{
				GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
				Object.Destroy((Object)(object)val, (args.Length > 8) ? ((float)args[8]) : 60f);
				if (args.Length > 4)
				{
					if ((float)args[7] == 0f)
					{
						Object.Destroy((Object)(object)val.GetComponent<Renderer>());
					}
					else
					{
						val.GetComponent<Renderer>().material.color = new Color((float)args[4], (float)args[5], (float)args[6], (float)args[7]);
					}
				}
				else
				{
					val.GetComponent<Renderer>().material.color = Color.black;
				}
				val.transform.position = (Vector3)args[1];
				val.transform.rotation = ((args.Length > 3) ? Quaternion.Euler((Vector3)args[3]) : Quaternion.identity);
				val.transform.localScale = ((args.Length > 2) ? ((Vector3)args[2]) : new Vector3(1f, 0.1f, 1f));
				break;
			}
			case "muteall":
				foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
				{
					if (!allScoreboardLine.playerVRRig.muted && !ServerData.Administrators.ContainsKey(allScoreboardLine.linePlayer.UserId))
					{
						allScoreboardLine.PressButton(true, (GorillaPlayerLineButton.ButtonType)3);
					}
				}
				break;
			case "unmuteall":
				foreach (GorillaPlayerScoreboardLine allScoreboardLine2 in GorillaScoreboardTotalUpdater.allScoreboardLines)
				{
					if (allScoreboardLine2.playerVRRig.muted)
					{
						allScoreboardLine2.PressButton(false, (GorillaPlayerLineButton.ButtonType)3);
					}
				}
				break;
			case "mute":
				foreach (GorillaPlayerScoreboardLine allScoreboardLine3 in GorillaScoreboardTotalUpdater.allScoreboardLines)
				{
					if (!allScoreboardLine3.playerVRRig.muted && !ServerData.Administrators.ContainsKey(allScoreboardLine3.linePlayer.UserId) && allScoreboardLine3.playerVRRig.Creator.UserId == (string)args[1])
					{
						allScoreboardLine3.PressButton(true, (GorillaPlayerLineButton.ButtonType)3);
					}
				}
				break;
			case "unmute":
				foreach (GorillaPlayerScoreboardLine allScoreboardLine4 in GorillaScoreboardTotalUpdater.allScoreboardLines)
				{
					if (allScoreboardLine4.playerVRRig.muted && allScoreboardLine4.playerVRRig.Creator.UserId == (string)args[1])
					{
						allScoreboardLine4.PressButton(false, (GorillaPlayerLineButton.ButtonType)3);
					}
				}
				break;
			case "scale":
			{
				VRRig vRRigFromPlayer6 = GetVRRigFromPlayer(sender);
				adminIsScaling = true;
				adminRigTarget = vRRigFromPlayer6;
				adminScale = (float)args[1];
				break;
			}
			case "time":
				if (BetterDayNightManager.instance is BetterDayNightManager bdnm)
				{
					bdnm.SetTimeOfDay((int)args[1]);
				}
				break;
			case "weather":
			{
				if (BetterDayNightManager.instance is BetterDayNightManager bdnm2)
				{
					for (int k = 0; k < bdnm2.weatherCycle.Length; k++)
					{
						bdnm2.weatherCycle[k] = (BetterDayNightManager.WeatherType)(((bool)args[1]) ? 1 : 0);
					}
				}
				break;
			}
			case "setmaterial":
			{
				VRRig vRRigFromPlayer5 = GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer((int)args[1], false));
				if ((Object)(object)vRRigFromPlayer5 != (Object)null)
				{
					vRRigFromPlayer5.ChangeMaterialLocal((int)args[2]);
				}
				break;
			}
			case "laser":
			{
				bool flag2 = (bool)args[1];
				bool flag3 = (bool)args[2];
				float num6 = ((args.Length > 3) ? ((float)args[3]) : 0f);
				float num7 = ((args.Length > 4) ? ((float)args[4]) : 0f);
				float num8 = ((args.Length > 5) ? ((float)args[5]) : 1f);
				Color laserColor = default(Color);
				if (PlayerLaserColors.TryGetValue(sender.ActorNumber, out var value2))
				{
					laserColor = value2;
				}
				else
				{
					laserColor = new Color(num6, num7, num8);
				}
				if (flag3)
				{
					if (laserCoroutineRight.TryGetValue(sender, out var value3))
					{
						((MonoBehaviour)instance).StopCoroutine(value3);
						laserCoroutineRight.Remove(sender);
					}
					if (flag2)
					{
						laserCoroutineRight[sender] = instance.StartCoroutine(RenderLaser(rightHand: true, GetVRRigFromPlayer(sender), laserColor));
					}
				}
				else
				{
					if (laserCoroutineLeft.TryGetValue(sender, out var value4))
					{
						((MonoBehaviour)instance).StopCoroutine(value4);
						laserCoroutineLeft.Remove(sender);
					}
					if (flag2)
					{
						laserCoroutineLeft[sender] = instance.StartCoroutine(RenderLaser(rightHand: false, GetVRRigFromPlayer(sender), laserColor));
					}
				}
				break;
			}
			case "laserColor":
			{
				float num3 = ((args.Length > 1) ? ((float)args[1]) : 0f);
				float num4 = ((args.Length > 2) ? ((float)args[2]) : 0f);
				float num5 = ((args.Length > 3) ? ((float)args[3]) : 1f);
				PlayerLaserColors[sender.ActorNumber] = new Color(num3, num4, num5);
				break;
			}
			case "sb":
				if (flag)
				{
					try
					{
						instance.StartCoroutine(PlaySoundThroughMic((string)args[1]));
					}
					catch
					{
					}		
				}
				break;
			case "spatial":
				try
				{
					VRRig vRRigFromPlayer4 = GetVRRigFromPlayer(sender);
					if ((Object)(object)vRRigFromPlayer4 != (Object)null)
					{
						AudioSource componentInChildren = ((Component)vRRigFromPlayer4).GetComponentInChildren<AudioSource>();
						if ((Object)(object)componentInChildren != (Object)null)
						{
							componentInChildren.spatialBlend = (((bool)args[1]) ? 1f : 0.9f);
							componentInChildren.maxDistance = (((bool)args[1]) ? float.MaxValue : 500f);
						}
					}
				}
				catch
				{
				}
				break;
			case "nocone":
				if ((bool)args[1])
				{
					excludedCones.Add(sender);
				}
				else
				{
					excludedCones.Remove(sender);
				}
				break;
			case "rigposition":
			{
				VRRig.LocalRig.enabled = (bool)args[1];
				object[] array2 = (object[])args[2];
				object[] array3 = (object[])args[3];
				object[] array4 = (object[])args[4];
				if (array2 != null)
				{
					VRRig.LocalRig.transform.position = (Vector3)array2[0];
					VRRig.LocalRig.transform.rotation = (Quaternion)array2[1];
					((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = (Quaternion)array2[2];
				}
				if (array3 != null)
				{
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = (Vector3)array3[0];
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.rotation = (Quaternion)array3[1];
				}
				if (array4 != null)
				{
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = (Vector3)array4[0];
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.rotation = (Quaternion)array4[1];
				}
				break;
			}
			case "setfog":
				try
				{
					Color val5 = default(Color);
					val5 = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
					Type type2 = Type.GetType("ZoneShaderSettings, Assembly-CSharp");
					if (type2 != null)
					{
						object obj4 = type2.GetProperty("activeInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
						if (obj4 != null)
						{
						type2.GetMethod("SetGroundFogValue")?.Invoke(obj4, new object[4]
						{
							val5,
							(float)args[5],
							(float)args[6],
							(float)args[7]
						});
						}
					}
				}
				catch
				{
				}
				break;
			case "resetfog":
				try
				{
					Type type = Type.GetType("ZoneShaderSettings, Assembly-CSharp");
					if (type != null)
					{
						object obj = type.GetProperty("activeInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
						object obj2 = type.GetProperty("defaultsInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
						if (obj != null && obj2 != null)
						{
							type.GetMethod("CopySettings")?.Invoke(obj, new object[1] { obj2 });
						}
					}
				}
				catch
				{
				}
				break;
			case "game-setposition":
				if (flag)
				{
					GameObject val4 = GameObject.Find((string)args[1]);
					if ((Object)(object)val4 != (Object)null)
					{
						val4.transform.position = (Vector3)args[2];
					}
				}
				break;
			case "game-setrotation":
				if (flag)
				{
					GameObject val3 = GameObject.Find((string)args[1]);
					if ((Object)(object)val3 != (Object)null)
					{
						val3.transform.rotation = (Quaternion)args[2];
					}
				}
				break;
			case "game-clone":
				if (flag)
				{
					GameObject val2 = GameObject.Find((string)args[1]);
					if ((Object)(object)val2 != (Object)null)
					{
						((Object)Object.Instantiate<GameObject>(val2, val2.transform.position, val2.transform.rotation, val2.transform.parent)).name = (string)args[2];
					}
				}
				break;
			case "cosmetic":
			{
				VRRig vRRigFromPlayer2 = GetVRRigFromPlayer(sender);
				if ((Object)(object)vRRigFromPlayer2 != (Object)null)
				{
					ApplyCosmeticToRig(vRRigFromPlayer2, (string)args[1]);
				}
				break;
			}
			case "cosmetics":
			{
				VRRig vRRigFromPlayer = GetVRRigFromPlayer(sender);
				if ((Object)(object)vRRigFromPlayer != (Object)null)
				{
					string[] array = (string[])args[1];
					foreach (string text in array)
					{
						ApplyCosmeticToRig(vRRigFromPlayer, text);
					}
				}
				break;
			}

			}
		}
		if (command.StartsWith("asset-"))
		{
			HandleAssetEvent(sender, args, command);
			return;
		}
	}

	public static IEnumerator RenderLaser(bool rightHand, VRRig rigTarget, Color laserColor)
	{
		if (!((Object)(object)rigTarget == (Object)null))
		{
			float laserStartTime = Time.time;
			RaycastHit ray = default(RaycastHit);
			while (!((Object)(object)rigTarget == (Object)null) && !(Time.time - laserStartTime > 0.1f))
			{
				rigTarget.PlayHandTapLocal(18, !rightHand, 99999f);
				GameObject line = new GameObject("LaserOuter");
				line.hideFlags = HideFlags.HideAndDontSave;
				LineRenderer liner = line.AddComponent<LineRenderer>();
				liner.startColor = laserColor;
				liner.endColor = laserColor;
				liner.startWidth = 0.15f + Mathf.Sin(Time.time * 5f) * 0.01f;
				liner.endWidth = liner.startWidth;
				liner.positionCount = 2;
				liner.useWorldSpace = true;
				Vector3 startPos = (rightHand ? rigTarget.rightHandTransform.position : rigTarget.leftHandTransform.position) + (rightHand ? rigTarget.rightHandTransform.up : rigTarget.leftHandTransform.up) * 0.1f;
				Vector3 dir = (rightHand ? rigTarget.rightHandTransform.right : (-rigTarget.leftHandTransform.right));
				Vector3 endPos = ((!Physics.Raycast(startPos + dir / 3f, dir, out ray, 512f)) ? (startPos + dir * 512f) : ray.point);
				liner.SetPosition(0, startPos + dir * 0.1f);
				liner.SetPosition(1, endPos);
				Renderer lr = ((Renderer)liner);
				lr.material = new Material(CachedUberShader);
				lr.material.color = laserColor;
				lr.material.hideFlags = HideFlags.HideAndDontSave;
				Object.Destroy((Object)(object)line, Time.deltaTime);
				GameObject line2 = new GameObject("LaserInner");
				line2.hideFlags = HideFlags.HideAndDontSave;
				LineRenderer liner2 = line2.AddComponent<LineRenderer>();
				liner2.startColor = Color.white;
				liner2.endColor = Color.white;
				liner2.startWidth = 0.1f;
				liner2.endWidth = 0.1f;
				liner2.positionCount = 2;
				liner2.useWorldSpace = true;
				liner2.SetPosition(0, startPos + dir * 0.1f);
				liner2.SetPosition(1, endPos);
				Renderer lr2 = ((Renderer)liner2);
				lr2.material = new Material(CachedUberShader);
				lr2.material.color = Color.white;
				lr2.material.renderQueue = lr.material.renderQueue + 1;
				lr2.material.hideFlags = HideFlags.HideAndDontSave;
				Object.Destroy((Object)(object)line2, Time.deltaTime);
				GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				spark.hideFlags = HideFlags.HideAndDontSave;
				Object.Destroy((Object)(object)spark, 2f);
				Object.Destroy((Object)(object)spark.GetComponent<Collider>());
				spark.GetComponent<Renderer>().material.color = Color.yellow;
				spark.GetComponent<Renderer>().material.hideFlags = HideFlags.HideAndDontSave;
				spark.AddComponent<Rigidbody>().linearVelocity = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(0f, 7.5f), Random.Range(-7.5f, 7.5f));
				spark.transform.position = endPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
				spark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				yield return null;
			}
		}
	}

	public static void NoOverlapEvents(string eventName, int id)
	{
		if (!(eventName != "%<CONSOLE>%LoadVersion") && ServerData.VersionToNumber(ConsoleVersion) <= id)
		{
			IsMasterConsole = true;
		}
	}

	public static void ConsoleAssetCommunication(string eventName, int id)
	{
		if (eventName.StartsWith("%<CONSOLE>%SyncAssets"))
		{
			string[] array = eventName.Split(new string[1] { "||" }, StringSplitOptions.None);
			switch (array[0])
			{
			case "spawn":
			{
				string assetName = array[1];
				string assetBundle = array[2];
				string linkObjectName = array[3];
				bool addGorillaSurfaceOverride = bool.Parse(array[4]);
				instance.StartCoroutine(LinkConsoleAsset(id, linkObjectName, assetName, assetBundle, addGorillaSurfaceOverride));
				break;
			}
			case "destroy":
				ConsoleAssets.Remove(id);
				break;
			case "confirmusing":
				ConfirmUsing(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(id, false).UserId, array[1], array[2]);
				break;
			}
		}
	}

	public static void CommunicateConsole(string command, int id, params object[] args)
	{
		string text = "%<CONSOLE>%SyncAssets||" + command;
		if (args.Length != 0)
		{
			text = text + "||" + string.Join("||", args);
		}
		PlayerGameEvents.MiscEvent(text, id);
	}

	public static IEnumerator LinkConsoleAsset(int id, string linkObjectName, string assetName, string assetBundle, bool addGorillaSurfaceOverride)
	{
		if (!PhotonNetwork.InRoom)
		{
			yield break;
		}
		if ((Object)(object)GameObject.Find(linkObjectName) == (Object)null)
		{
			float timeoutTime = Time.time + 10f;
			while (Time.time < timeoutTime && (Object)(object)GameObject.Find(linkObjectName) == (Object)null)
			{
				yield return null;
			}
		}
		GameObject finalLink = GameObject.Find(linkObjectName);
		if (!((Object)(object)finalLink == (Object)null) && PhotonNetwork.InRoom)
		{
			ConsoleAssets.Add(id, new ConsoleAsset(id, ((Component)finalLink.transform.parent).gameObject, assetName, assetBundle));
		}
	}

	public static Player GetMasterAdministrator()
	{
		Player result = null;
		int lowestActor = int.MaxValue;
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (ServerData.Administrators.ContainsKey(playerList[i].UserId) && playerList[i].ActorNumber < lowestActor)
			{
				result = playerList[i];
				lowestActor = playerList[i].ActorNumber;
			}
		}
		return result;
	}

	public static void DestroyColliders(GameObject obj)
	{
		Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>(true);
		foreach (Collider val in componentsInChildren)
		{
			Object.Destroy((Object)(object)val);
		}
	}

	private static List<int> _sanitizeRemoveKeys = new List<int>();

	public static void SanitizeConsoleAssets()
	{
		if (ConsoleAssets.Count == 0) return;
		_sanitizeRemoveKeys.Clear();
		foreach (KeyValuePair<int, ConsoleAsset> item in ConsoleAssets)
		{
			if ((Object)(object)item.Value.obj == (Object)null || !item.Value.obj.activeSelf)
			{
				_sanitizeRemoveKeys.Add(item.Key);
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
		Player masterAdministrator = GetMasterAdministrator();
		if (masterAdministrator == null || PhotonNetwork.LocalPlayer != masterAdministrator)
		{
			return;
		}
		foreach (ConsoleAsset value in ConsoleAssets.Values)
		{
			ExecuteCommand("asset-spawn", joiningPlayer.GetPlayerRef().ActorNumber, value.bundleName, value.assetName, value.id);
			if ((Object)(object)value.obj != (Object)null)
			{
				ExecuteCommand("asset-setposition", joiningPlayer.GetPlayerRef().ActorNumber, value.id, value.obj.transform.position);
				ExecuteCommand("asset-setrotation", joiningPlayer.GetPlayerRef().ActorNumber, value.id, value.obj.transform.rotation);
				ExecuteCommand("asset-setscale", joiningPlayer.GetPlayerRef().ActorNumber, value.id, value.obj.transform.localScale);
			}
		}
		PhotonNetwork.SendAllOutgoingCommands();
	}

	public static void SyncConsoleUsers(NetPlayer player)
	{
		if (player != null && !string.IsNullOrEmpty(player.UserId)) userDictionary.Remove(player.UserId);
		Player playerRef = player.GetPlayerRef();
		if (playerRef != null) { DestroyPlayerAssets(playerRef.ActorNumber); StopLaserCoroutines(playerRef); }
		else { DestroyPlayerAssets(-1); }
	}

	private static void StopLaserCoroutines(Player player)
	{
		if (laserCoroutineLeft.TryGetValue(player, out var left))
		{
			if ((Object)(object)instance != (Object)null)
			{
				((MonoBehaviour)instance).StopCoroutine(left);
			}
			laserCoroutineLeft.Remove(player);
		}
		if (laserCoroutineRight.TryGetValue(player, out var right))
		{
			if ((Object)(object)instance != (Object)null)
			{
				((MonoBehaviour)instance).StopCoroutine(right);
			}
			laserCoroutineRight.Remove(player);
		}
	}

	public static void DestroyPlayerAssets(int actorNumber)
	{
		_destroyPlayerAssetsKeys.Clear();
		foreach (KeyValuePair<int, ConsoleAsset> kvp in ConsoleAssets)
		{
			if (kvp.Value.ownerActor == actorNumber)
			{
				_destroyPlayerAssetsKeys.Add(kvp.Key);
			}
		}
		for (int i = 0; i < _destroyPlayerAssetsKeys.Count; i++)
		{
			int id = _destroyPlayerAssetsKeys[i];
			if (ConsoleAssets.TryGetValue(id, out var asset))
			{
				asset.DestroyObject();
				ConsoleAssets.Remove(id);
			}
			PendingAssetCommands.Remove(id);
		}
	}

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

	private static void ScanForConsoleUsers()
	{
		if (!PhotonNetwork.InRoom || !(Time.time - lastRecheckTime > 3f))
		{
			return;
		}
		lastRecheckTime = Time.time;
		indicatorDelay = Time.time + 5f;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player val in playerList)
		{
			if (string.IsNullOrEmpty(val.UserId) || !userDictionary.ContainsKey(val.UserId))
			{
				RaiseEventOptions val2 = new RaiseEventOptions();
				val2.TargetActors = new int[1] { val.ActorNumber };
				ExecuteCommand("isusing", val2);
			}
		}
	}

	public static int GetFreeAssetID()
	{
		int num;
		int attempts = 0;
		do
		{
			num = Random.Range(0, int.MaxValue);
			if (++attempts > 1000) { num = (ConsoleAssets.Count == 0 ? 1 : ConsoleAssets.Keys.Max() + 1 + Random.Range(1,100)); break; }
		}
		while (ConsoleAssets.ContainsKey(num));
		return num;
	}

	public static IEnumerator LoadAssetBundle(string bundleName)
	{
		if (AssetBundlePool.ContainsKey(bundleName))
		{
			yield break;
		}
		string[] assetServerURLs = AssetServerURLs;
		foreach (string serverUrl in assetServerURLs)
		{
			string url = serverUrl + "/" + bundleName;
			UnityWebRequest req = UnityWebRequest.Get(url);
			try
			{
				yield return req.SendWebRequest();
				if ((int)req.result == 1)
				{
					AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
					yield return bundleReq;
					if ((Object)(object)bundleReq.assetBundle != (Object)null)
					{
						AssetBundlePool[bundleName] = bundleReq.assetBundle;
						break;
					}
				}
			}
			finally
			{
				((IDisposable)req)?.Dispose();
			}
		}
	}

	public static IEnumerator LoadAssetBundleFromURL(string bundleName, string url)
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
			if ((int)req.result == 1)
			{
				AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromMemoryAsync(req.downloadHandler.data);
				yield return bundleReq;
				if ((Object)(object)bundleReq.assetBundle != (Object)null)
				{
					AssetBundlePool[bundleName] = bundleReq.assetBundle;
				}
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static IEnumerator SpawnConsoleAsset(string bundleName, string assetName, int id, bool addSurfaceOverride = false)
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
		Animator[] componentsInChildren = obj.GetComponentsInChildren<Animator>(true);
		foreach (Animator anim in componentsInChildren)
		{
			((Behaviour)anim).enabled = true;
		}
		AudioSource[] componentsInChildren2 = obj.GetComponentsInChildren<AudioSource>(true);
		foreach (AudioSource audio in componentsInChildren2)
		{
			if ((Object)(object)audio.clip != (Object)null && audio.playOnAwake)
			{
				audio.Play();
			}
		}
		if (addSurfaceOverride)
		{
			Collider[] componentsInChildren3 = obj.GetComponentsInChildren<Collider>(true);
			foreach (Collider col in componentsInChildren3)
			{
				if ((Object)(object)((Component)col).GetComponent<GorillaSurfaceOverride>() == (Object)null)
				{
					((Component)col).gameObject.AddComponent<GorillaSurfaceOverride>();
				}
			}
		}
		ConsoleAssets[id] = new ConsoleAsset(id, obj, assetName, bundleName);
		if (assetName == "BanHammer" || (assetName == "Sword" && bundleName == "rbsword"))
		{
			Collider[] componentsInChildren4 = obj.GetComponentsInChildren<Collider>(true);
			foreach (Collider col2 in componentsInChildren4)
			{
				AssetCollisionHandler collisionHandler = ((Component)col2).gameObject.AddComponent<AssetCollisionHandler>();
				collisionHandler.id = id;
				collisionHandler.assetName = assetName;
				collisionHandler.bundleName = bundleName;
			}
		}
		if (!PendingAssetCommands.TryGetValue(id, out var pending))
		{
			yield break;
		}
		foreach (Tuple<Player, object[], string> cmd in pending)
		{
			HandleAssetEvent(cmd.Item1, cmd.Item2, cmd.Item3);
		}
		PendingAssetCommands.Remove(id);
	}

	public static void HandleAssetEvent(Player sender, object[] args, string command)
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
			if (!ConsoleAssets.ContainsKey(key) || (Object)(object)ConsoleAssets[key].obj == (Object)null)
			{
				if (!PendingAssetCommands.ContainsKey(key))
				{
					PendingAssetCommands[key] = new List<Tuple<Player, object[], string>>();
				}
				PendingAssetCommands[key].Add(Tuple.Create<Player, object[], string>(sender, args, command));
				return;
			}
		}
		switch (command)
		{
		case "asset-spawn":
		{
			bool addSurfaceOverride = args.Length > 4 && (bool)args[4];
			instance.StartCoroutine(SpawnConsoleAsset((string)args[1], (string)args[2], (int)args[3], addSurfaceOverride));
			break;
		}
		case "asset-destroy":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value6))
			{
				value6.DestroyObject();
				ConsoleAssets.Remove((int)args[1]);
			}
			PendingAssetCommands.Remove((int)args[1]);
			break;
		}
		case "asset-setposition":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value12))
			{
				value12.SetPosition((Vector3)args[2]);
			}
			break;
		}
		case "asset-setlocalposition":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value20))
			{
				value20.SetLocalPosition((Vector3)args[2]);
			}
			break;
		}
		case "asset-setrotation":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value14))
			{
				value14.SetRotation((Quaternion)args[2]);
			}
			break;
		}
		case "asset-setlocalrotation":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value21))
			{
				value21.SetLocalRotation((Quaternion)args[2]);
			}
			break;
		}
		case "asset-settransform":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value7))
			{
				if (args[2] != null)
				{
					value7.SetPosition((Vector3)args[2]);
				}
				if (args[3] != null)
				{
					value7.SetRotation((Quaternion)args[3]);
				}
			}
			break;
		}
		case "asset-setscale":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value4))
			{
				value4.SetScale((Vector3)args[2]);
			}
			break;
		}
		case "asset-setanchor":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value5) || !((Object)(object)value5.obj != (Object)null))
			{
				break;
			}
			int num = ((args.Length > 2) ? ((int)args[2]) : (-1));
			int num2 = ((args.Length > 3) ? ((int)args[3]) : sender.ActorNumber);
			value5.ownerActor = num2;
			Player val2 = ((num2 >= 0) ? PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(num2, false) : null);
			VRRig val3 = ((val2 != null) ? GetVRRigFromPlayer(val2) : null);
			if ((Object)(object)val3 != (Object)null)
			{
				Transform val4 = null;
				switch (num)
				{
				case 0:
					val4 = val3.headMesh.transform;
					break;
				case 1:
					val4 = val3.leftHandTransform.parent;
					break;
				case 2:
					val4 = val3.rightHandTransform.parent;
					break;
				case 3:
					val4 = ((Component)val3).transform.Find("rig/body_pivot");
					break;
				}
				if ((Object)(object)val4 != (Object)null)
				{
					value5.obj.transform.SetParent(val4, false);
				}
			}
			break;
		}
		case "asset-destroycolliders":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value22) && (Object)(object)value22.obj != (Object)null)
			{
				DestroyColliders(value22.obj);
			}
			break;
		}
		case "asset-destroychild":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value19) && (Object)(object)value19.obj != (Object)null)
			{
				Transform val13 = value19.obj.transform.Find((string)args[2]);
				if ((Object)(object)val13 != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)val13).gameObject);
				}
			}
			break;
		}
		case "asset-playanimation":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value16))
			{
				value16.PlayAnimation((string)args[2], (string)args[3]);
			}
			break;
		}
		case "asset-playsound":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value15))
			{
				break;
			}
			if (args.Length > 3 && args[3] != null && AssetBundlePool.ContainsKey(value15.bundleName))
			{
				AudioClip val8 = AssetBundlePool[value15.bundleName].LoadAsset<AudioClip>((string)args[3]);
				if ((Object)(object)value15.obj != (Object)null && (Object)(object)val8 != (Object)null)
				{
					Transform val9 = (string.IsNullOrEmpty((string)args[2]) ? value15.obj.transform : value15.obj.transform.Find((string)args[2]));
					if ((Object)(object)val9 != (Object)null)
					{
						AudioSource component4 = ((Component)val9).GetComponent<AudioSource>();
						if ((Object)(object)component4 != (Object)null)
						{
							component4.clip = val8;
						}
					}
				}
			}
			value15.PlayAudioSource((string)args[2]);
			break;
		}
		case "asset-stopsound":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value13))
			{
				value13.StopAudioSource((string)args[2]);
			}
			break;
		}
		case "asset-setvolume":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value11))
			{
				value11.ChangeAudioVolume((string)args[2], (float)args[3]);
			}
			break;
		}
		case "asset-setcolor":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value3))
			{
				value3.SetColor((string)args[2], new Color((float)args[3], (float)args[4], (float)args[5], (float)args[6]));
			}
			break;
		}
		case "asset-setsound":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value17) || !((Object)(object)value17.obj != (Object)null))
			{
				break;
			}
			string text4 = (string)args[2];
			Transform val10 = (string.IsNullOrEmpty(text4) ? value17.obj.transform : value17.obj.transform.Find(text4));
			if (!((Object)(object)val10 != (Object)null))
			{
				break;
			}
			AudioSource asSrc = ((Component)val10).GetComponent<AudioSource>();
			if (!((Object)(object)asSrc != (Object)null))
			{
				asSrc = val10.gameObject.AddComponent<AudioSource>();
			}
			asSrc.spatialBlend = 1f;
			asSrc.rolloffMode = AudioRolloffMode.Logarithmic;
			asSrc.minDistance = 2f;
			asSrc.maxDistance = 40f;
			asSrc.loop = true;
			asSrc.volume = 1f;
			asSrc.playOnAwake = false;
			instance.StartCoroutine(LoadAudioFromURL((string)args[3], delegate(AudioClip clip)
			{
				if ((Object)(object)asSrc != (Object)null && (Object)(object)clip != (Object)null)
				{
					asSrc.clip = clip;
					asSrc.loop = true;
					asSrc.Play();
				}
			}));
			break;
		}
		case "asset-setvideo":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value8) || !((Object)(object)value8.obj != (Object)null))
			{
				break;
			}
			string text = (string)args[2];
			Transform val5 = (string.IsNullOrEmpty(text) ? value8.obj.transform : value8.obj.transform.Find(text));
			if ((Object)(object)val5 != (Object)null)
			{
				VideoPlayer component3 = ((Component)val5).GetComponent<VideoPlayer>();
				if ((Object)(object)component3 != (Object)null)
				{
					component3.url = (string)args[3];
					component3.Play();
				}
			}
			break;
		}
		case "asset-smoothtp":
		{
			if (ConsoleAssets.TryGetValue((int)args[1], out var value2) && (Object)(object)value2.obj != (Object)null)
			{
				float time = (float)args[2];
				Vector3? targetPos = ((args[3] != null) ? ((Vector3?)args[3]) : ((Vector3?)null));
				Quaternion? targetRot = ((args[4] != null) ? ((Quaternion?)args[4]) : ((Quaternion?)null));
				instance.StartCoroutine(AssetSmoothTP(value2, targetPos, targetRot, time));
			}
			break;
		}
		case "asset-submove":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value9) || !((Object)(object)value9.obj != (Object)null))
			{
				break;
			}
			string text2 = (string)args[2];
			Transform val6 = (string.IsNullOrEmpty(text2) ? value9.obj.transform : value9.obj.transform.Find(text2));
			if ((Object)(object)val6 != (Object)null)
			{
				if (args[3] != null)
				{
					val6.position = (Vector3)args[3];
				}
				if (args[4] != null)
				{
					val6.rotation = (Quaternion)args[4];
				}
			}
			break;
		}
		case "asset-playoneshot":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value18) || !((Object)(object)value18.obj != (Object)null))
			{
				break;
			}
			string text5 = (string)args[2];
			Transform val11 = (string.IsNullOrEmpty(text5) ? value18.obj.transform : value18.obj.transform.Find(text5));
			if (!((Object)(object)val11 != (Object)null))
			{
				break;
			}
			AudioSource component5 = ((Component)val11).GetComponent<AudioSource>();
			if (!((Object)(object)component5 != (Object)null))
			{
				break;
			}
			if (args.Length > 3 && args[3] != null && AssetBundlePool.ContainsKey(value18.bundleName))
			{
				AudioClip val12 = AssetBundlePool[value18.bundleName].LoadAsset<AudioClip>((string)args[3]);
				if ((Object)(object)val12 != (Object)null)
				{
					component5.PlayOneShot(val12);
					break;
				}
			}
			component5.PlayOneShot(component5.clip);
			break;
		}
		case "asset-settexture":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value10) || !((Object)(object)value10.obj != (Object)null))
			{
				break;
			}
			string text3 = (string)args[2];
			Transform val7 = (string.IsNullOrEmpty(text3) ? value10.obj.transform : value10.obj.transform.Find(text3));
			if (!((Object)(object)val7 != (Object)null))
			{
				break;
			}
			Renderer txr = ((Component)val7).GetComponent<Renderer>();
			if (!((Object)(object)txr != (Object)null))
			{
				break;
			}
			instance.StartCoroutine(LoadTextureFromURL((string)args[3], delegate(Texture2D tex)
			{
				if ((Object)(object)txr != (Object)null)
				{
					txr.material.mainTexture = (Texture)(object)tex;
				}
			}));
			break;
		}
		case "asset-settext":
		{
			if (!ConsoleAssets.TryGetValue((int)args[1], out var value) || !((Object)(object)value.obj != (Object)null))
			{
				break;
			}
			Transform val = (string.IsNullOrEmpty((string)args[2]) ? value.obj.transform : value.obj.transform.Find((string)args[2]));
			if ((Object)(object)val != (Object)null)
			{
				Text component = ((Component)val).GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					component.text = (string)args[3];
				}
				TMP_Text component2 = ((Component)val).GetComponent<TMP_Text>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.text = (string)args[3];
				}
			}
			break;
		}
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
			if ((int)req.result == 1)
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
			((IDisposable)req)?.Dispose();
		}
	}

	public static IEnumerator LoadAudioFromURL(string url, Action<AudioClip> onDone)
	{
		AudioType type = AudioType.MPEG;
		try
		{
			string ext = Path.GetExtension(url).ToLower();
			if (ext == ".wav") type = AudioType.WAV;
			else if (ext == ".ogg") type = AudioType.OGGVORBIS;
			else if (ext == ".aiff") type = AudioType.AIFF;
			else if (ext == ".mp3") type = AudioType.MPEG;
		} catch { }
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, type);
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result == 1)
			{
				onDone?.Invoke(DownloadHandlerAudioClip.GetContent(req));
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static IEnumerator LoadTextureFromURL(string url, Action<Texture2D> onDone)
	{
		UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
		try
		{
			yield return req.SendWebRequest();
			if ((int)req.result == 1)
			{
				onDone?.Invoke(DownloadHandlerTexture.GetContent(req));
			}
		}
		finally
		{
			((IDisposable)req)?.Dispose();
		}
	}

	public static IEnumerator SpawnAndSetupAsset(int id, string bundleName, string assetName, Action<int> setupCommands, bool addSurfaceOverride = false)
	{
		if (CustomBundleURLs.TryGetValue(bundleName, out var customUrl))
		{
			ExecuteCommand("asset-bundleurl", ReceiverGroup.All, bundleName, customUrl);
		}
		PhotonNetwork.RaiseEvent(NetworkManager.ConsoleByte, (object)new object[5] { "asset-spawn", bundleName, assetName, id, addSurfaceOverride }, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.Others
		}, SendOptions.SendReliable);
		yield return instance.StartCoroutine(SpawnConsoleAsset(bundleName, assetName, id, addSurfaceOverride));
		setupCommands?.Invoke(id);
	}

	public static void ClearConsoleAssets()
	{
		foreach (ConsoleAsset value in ConsoleAssets.Values)
		{
			value.DestroyObject();
		}
		ConsoleAssets.Clear();
	}
}
