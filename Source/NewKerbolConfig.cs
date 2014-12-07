using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class NewKerbolConfig : MonoBehaviour
	{
		//settings node
		public static ConfigNode Settings {get; private set;}
		
		static ConfigNode ScienceNode;

		public static GUIStyle DescriptionStyle;
		public static GUIStyle LabelStyle;
		public static GUIStyle NoteBoldStyle;
		public static GUIStyle NoteNotBoldStyle;
		public static GUIStyle LabelBoldStyle;
		
		void Start()
		{
			//load settings
			Settings = Utils.LoadConfig("Settings.cfg");

			ScienceNode = Utils.LoadConfig ("Science.cfg");

			DescriptionStyle = new GUIStyle (skin.label);
			DescriptionStyle.normal.textColor = new Color (0.6f, 0.6f, 0.6f);
			DescriptionStyle.active.textColor = new Color (0.6f, 0.6f, 0.6f);
			DescriptionStyle.focused.textColor = new Color (0.6f, 0.6f, 0.6f);

			LabelStyle = new GUIStyle (skin.label);
			LabelStyle.normal.textColor = Color.white;
			LabelStyle.active.textColor = Color.white;
			LabelStyle.focused.textColor = Color.white;

			LabelBoldStyle = new GUIStyle (LabelStyle);
			LabelBoldStyle.fontStyle = FontStyle.Bold;

			NoteBoldStyle = new GUIStyle (skin.label);
			NoteBoldStyle.normal.textColor = XKCDColors.KSPNotSoGoodOrange;
			NoteBoldStyle.active.textColor = XKCDColors.KSPNotSoGoodOrange;
			NoteBoldStyle.focused.textColor = XKCDColors.KSPNotSoGoodOrange;
			NoteBoldStyle.fontStyle = FontStyle.Bold;

			NoteNotBoldStyle = new GUIStyle (NoteBoldStyle);
			NoteNotBoldStyle.fontStyle = FontStyle.Normal;

			skin.label = LabelStyle;

			DontDestroyOnLoad (this);
		}

		bool switchWindow = false;

		GUISkin skin = HighLogic.Skin;

		Rect windowRect = new Rect(200f, 200f, 300f, 400f);
		void OnGUI()
		{
			GUI.skin = skin;

			if (!switchWindow)
				windowRect = GUI.Window (42567, windowRect, Window, "New Kerbol Settings");
			else
				windowRect = GUI.Window (42568, windowRect, ProgressWindow, "New Kerbol Progress");
		}
		void SaveSettings()
		{
			Utils.SaveConfig (Settings, "Settings.cfg");
		}

		//whether or not the mod is enabled
		public static bool ModEnabled = true;
		public static bool UseCustomScience = true;
		public static bool UseSpaceKraken = false;
		public static bool UseRedSun = true;
		public static float ZombificationRate = 1f;
		public static float MaxWindStrength = 10f;
		public static bool DoParachutesCut = true;

		Vector2 scroll = Vector2.zero;
		public void Window(int id)
		{
			GUILayout.BeginVertical ();

			if (Settings != null)
			{
				scroll = GUILayout.BeginScrollView (scroll);

				ModEnabled = DrawBoolField ("modEnabled", "Mod Enabled", "If unchecked, then this mod will not run");
				UseCustomScience = DrawBoolField ("UseCustomScience", "Custom Science", "Whether or not to use the custom science dialogs and values in Science.cfg.");
				DrawNote ("This will also affect contract payments and per-planet science multipliers as well");
				GUILayout.Space (7f);
				UseSpaceKraken = DrawBoolField ("UseSpaceKraken", "Kraken", "Transforms New Bop into the Almighty Space Kraken.");
				UseRedSun = DrawBoolField ("UseRedSun", "Red Dwarf Sun", "Turns the sun into a red dwarf star");
				GUILayout.Space (7f);
				ZombificationRate = DrawFloatField ("ZombificationRate", "Zombification Rate", "The rate at which kerbals on EVA zombify when on New Laythe in percent/second");
				DrawNote ("This rate is doubled when on land and tripled when swimming");
				MaxWindStrength = DrawFloatField ("MaxWindStrength", "Maximum Wind Strength", "The maximum wind strength on New Eve. Default is 10.0");
				DoParachutesCut = DrawBoolField ("DoParachutesCut", "Parachutes get torn off by winds", "If checked, then parachutes will get torn off by high winds on New Eve");

				GUILayout.EndScrollView ();
			}
			else
			{
				GUILayout.Label ("Error showing settings GUI", LabelBoldStyle);
			}

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				if (GUILayout.Button ("Apply settings and Run") && IncompatibilityChecker.FINISHED)
				{
					switchWindow = true;
					SaveSettings ();
				}
			}

			GUILayout.EndVertical ();

			GUI.DragWindow ();
		}

		Vector2 progressScroll = Vector2.zero;
		List<string> progressStrings = new List<string>();
		public void ProgressWindow(int id)
		{
			GUILayout.BeginVertical ();

			progressScroll = GUILayout.BeginScrollView (progressScroll);
			foreach (var progressString in progressStrings)
			{
				GUILayout.Label (progressString);
			}
			GUILayout.EndScrollView ();

			if (index > 999)
			{
				if (GUILayout.Button ("Close"))
				{
					DestroyImmediate (this);
				}
			}
			else
			{
				GUILayout.Label ("Building...", LabelBoldStyle);
			}

			GUILayout.EndVertical ();

			if (index > 999)
				GUI.DragWindow ();
		}

		int index = -1;
		void Update()
		{
			if (index < 999)
				InputLockManager.SetControlLock (ControlTypes.MAIN_MENU, "NewKerbol_MenuLock");
			else if (InputLockManager.IsLocked (ControlTypes.MAIN_MENU))
				InputLockManager.RemoveControlLock ("NewKerbol_MenuLock");

			if (HighLogic.LoadedScene == GameScenes.MAINMENU && switchWindow)
			{
				if (!ModEnabled && index < 999)
				{
					progressStrings.Add ("Canceled build");
					progressScroll.y = windowRect.yMax;
					index = 1000;
				}

				if (index < 0)
				{
					progressStrings.Add ("Building New Kerbol System...");
					progressStrings.Add ("Pre Build");
					progressScroll.y = windowRect.yMax;
					Initialization.PreBuild ();
					index = 0;
					return;
				}
				else if (index >= 0 && index < Initialization.Mods.Count)
				{
					var mod = Initialization.Mods [index];
					progressStrings.Add ("Building " + mod.Target.bodyName + "...");
					progressScroll.y = windowRect.yMax;
					mod.Start ();
					index++;
					return;
				}
				else if (index >= Initialization.Mods.Count && index < 499)
				{
					if (UseCustomScience && ScienceNode != null)
					{
						progressStrings.Add ("Applying custom science parameters...");
						progressScroll.y = windowRect.yMax;
						Initialization.SetScience (ScienceNode);
					}
					else
					{
						progressStrings.Add ("Custom science was disabled");
						progressScroll.y = windowRect.yMax;
					}
					index = 500;
				}
				else if (index > 499 && index < 999)
				{
					progressStrings.Add ("Post Build");
					progressScroll.y = windowRect.yMax;
					Initialization.PostBuild ();
					index = 1000;
					return;
				}
			}
		}
		void OnDestroy()
		{
			InputLockManager.RemoveControlLock ("NewKerbol_MenuLock");
		}

		public float DrawFloatField(string name, string displayName, string description)
		{
			GUILayout.BeginHorizontal ();

			string value = Settings.GetValue (name);
			float f = 0f;
			GUILayout.Label (displayName + ": ");
			f = float.Parse (GUILayout.TextField (value));
			Settings.SetValue (name, f.ToString ());

			GUILayout.EndHorizontal ();

			GUILayout.Label (description, DescriptionStyle);
			GUILayout.Space (2f);

			return f;
		}
		public bool DrawBoolField(string name, string displayName, string description)
		{
			GUILayout.BeginHorizontal ();

			string value = Settings.GetValue (name);
			bool b = bool.Parse (value);
			b = GUILayout.Toggle (b, displayName);
			Settings.SetValue (name, b.ToString ());

			GUILayout.EndHorizontal ();

			GUILayout.Label (description, DescriptionStyle);
			GUILayout.Space (2f);

			return b;
		}
		public void DrawNote(string note)
		{
			GUILayout.BeginHorizontal ();

			GUILayout.Label ("Note:", NoteBoldStyle, GUILayout.Width(35f));
			GUILayout.Space (2f);
			GUILayout.Label (note, NoteNotBoldStyle);

			GUILayout.EndHorizontal ();
		}
	}
}

