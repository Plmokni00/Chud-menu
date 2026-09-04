using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using GTAG_NotificationLib;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu : MonoBehaviour
{
	public static string MenuTitle = "Chud Menu";

	public static Font MenuFont;

	public static AudioClip customButtonClick;

	

	private static AudioSource buttonClickAudioSource;

	private static bool MenuFontInitialized = false;

	private static bool customAudioLoaded = false;

	public static string[] CustomBoardTexts = new string[4] { "CHUD MENU USERS, LOOK HERE", "CHUD MENU", "Monkeys can climb. Crickets can leap. Horses can race. Owls can seek. Cheetahs can run. Eagles can fly. People can try. But that's about it.", "if u get banned with this, its on u, not me" };

	public static string FolderName = "Chud Menu";

	public static bool Close = false;

	public static bool animationsEnabled = false;

	private const float MENU_CYLINDER_RADIUS = 0.1f;
	private const float MENU_CYLINDER_HEIGHT = 0.3f;
	private const float MENU_CYLINDER_DEPTH = 0.4f;
	private const float BUTTON_CYLINDER_SCALE_X = 0.09f;
	private const float BUTTON_CYLINDER_SCALE_Y = 0.9f;
	private const float BUTTON_CYLINDER_SCALE_Z = 0.08f;

	public static Color NormalColor = new Color(0.25f, 0.25f, 0.25f);

	public static Color ButtonColorDisable = new Color(0.4f, 0.4f, 0.4f);

	public static Color ButtonColorEnabled = new Color(0.7f, 0.7f, 0.7f);

	public static Color EnableTextColor = Color.white;

	public static Color DisableTextColor = new Color(0.85f, 0.85f, 0.85f);

	public static Color MenuTitleColor = Color.white;

	public static Color ToolTipColor = new Color(0.88f, 0.88f, 0.88f);

	public static Color DisconnectButtonColor = new Color(0.7f, 0f, 0f);

	public static Color DisconnectTextColor = Color.white;

	public static Color NextPrevButtonColor = new Color(0.22f, 0.22f, 0.22f);

	public static Color NextPrevTextColor = Color.white;

	

	private static Mesh _cylinderMesh;
	private static Mesh CylinderMesh
	{
		get
		{
			if (_cylinderMesh == null)
			{
				GameObject temp = GameObject.CreatePrimitive((PrimitiveType)3);
				_cylinderMesh = temp.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(temp);
			}
			return _cylinderMesh;
		}
	}

	internal static List<Material> gradientMaterials = new List<Material>();

	private static Dictionary<string, List<Renderer>> roundedRenderers = new Dictionary<string, List<Renderer>>();

	private const float bevelWidth = 0.02f;

	public static Vector3 PointerScale = new Vector3(0.015f, 0.015f, 0.015f);

	public static Vector3 PointerPos = new Vector3(0f, -0.1f, 0f);

	public static bool gripDownR;

	public static bool triggerDownR;

	public static bool abuttonDown;

	public static bool bbuttonDown;

	public static bool xbuttonDown;

	public static bool ybuttonDown;

	public static bool gripDownL;

	public static bool triggerDownL;

	public static Vector2 joy;

	public static Vector2 joyL;

	public static GameObject menu = null;

	private static float lastButtonPressTime = -1f;

	public static GameObject canvasObj = null;

	public static GameObject reference = null;

	private static GameObject _menuAnchor = null;

	private static bool _menuCameraAnchored = false;

	private static bool _menuAnchorIsRightHand = false;

	private static Transform _menuFollowHand = null;

	public static int pageNumber = 0;

	public static WristMenu instance;

	public static GameObject menuObj;

	public static Text fpsText;

	private static DateTime sessionStartTime = DateTime.Now;

	private static string bottomBarStr = "FPS: 0 | 12:00 AM | 0:00";

	private static float fpsAccumulator;

	private static int fpsFrameCount;

	private static int cachedFPS = 0;

	public static bool leftTriggerLocked = false;

	public static bool rightTriggerLocked = false;

	public static int pageSize = 4;

	public static int ClickCooldown = 10;

	public static bool customBoardsEnabled = true;
	public static bool customBoardsApplied = false;
	private static string[] originalBoardTexts = new string[4];
	private static GameObject[] cachedBoardObjects = new GameObject[4];
	private static TMP_Text[] cachedBoardTexts = new TMP_Text[4];
	private static readonly string[] BoardPaths = new string[]
	{
		"Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData",
		"Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText"
	};

	private static int _frameCounter;

	private static bool _adminInitialized;

	private static bool _mouseWasPressed;

	private static Camera _tpc;

	public static bool showFPS = false;

		public static bool showSessionTime = false;

	public static bool toggleMenu = false;
	private static bool _prevToggleButton = false;
	private static bool _menuStickyOpen = false;

	public static Text titiel;

	

	

	
		
		
		

	

	

	

	

	

	

	

	


	

	

	

	// Destroys all gradient materials + their textures and the reference sphere's material.
	

	

	}
