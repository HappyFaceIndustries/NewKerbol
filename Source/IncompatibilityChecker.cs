using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


//////////////////////////////////////////
/////// Substitute Your Namespace ////////
//////////////////////////////////////////
namespace NewKerbol
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class IncompatibilityChecker : MonoBehaviour
	{
		//////////////////////////////////////////
		////////////// Edit here /////////////////
		//////////////////////////////////////////

		// return true to remove the mod
		// return false to skip
		// set WARNING to a message that warns the player about any glitches or incompatibilities that may arise
		public static bool Check(AssemblyLoader.LoadedAssembly assembly)
		{
			//only show warnings if the mod is enabled
			if(NewKerbolConfig.ModEnabled)
			{
				if (assembly.name == "Clouds")
				{
					WARNING = "Enviromental Visual Enhancements is not comaptible with New Kerbol. It causes visual glitches with some planets, and may lead to an instable game. It would be best if you removed one. ";
					return true;
				}
				if (assembly.name == "PlanetFactory")
				{
					WARNING = "Planet Factory is not compatible with New Kerbol. Together, they may cause bugs or crashes, and it would be best if you removed one.";
					return true;
				}
				if (assembly.name == "RealSolarSystem")
				{
					WARNING = "Real Solar System is not compatible with New Kerbol. It may cause game-breaking issues, or even crashes. You should not run both mods at the same time. It would be best if you removed one.";
					return true;
				}
				if (assembly.name == "AlternisKerbol")
				{
					WARNING = "Alternis Kerbol is not compatible with New Kerbol. They both seek to accomplish the same thing (change the stock system) and really should not be run together. It would be best if you removed one.";
					return true;
				}
			}
			return false;
		}


		//////////////////////////////////////////
		//////// DO NOT EDIT BELOW HERE //////////
		//////////////////////////////////////////


		static int VERSION = 1;
		static string WARNING = null;

		public static bool FINISHED = false;

		private void Start()
		{
			var fields = GetAllCheckers ()
				.Select (t => t.GetField ("VERSION", BindingFlags.NonPublic | BindingFlags.Static))
				.Where (f => f != null)
				.Where (f => f.FieldType == typeof(int))
				.ToArray ();

			if (VERSION != fields.Max (f => (int)f.GetValue (null)))
			{
				return;
			}
			VERSION = int.MaxValue;

			List<IncompatibilityWarning> warnings = new List<IncompatibilityWarning> ();

			foreach (var field in fields)
			{
				Type type = field.DeclaringType;
				string typeAssemblyName = type.Assembly.GetName ().Name;

				var method = type.GetMethod ("Check", new Type[]{ typeof(AssemblyLoader.LoadedAssembly) });
				if (method.IsStatic && method.ReturnType == typeof(bool))
				{
					for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; i++)
					{
						var assembly = AssemblyLoader.loadedAssemblies [i];

						try
						{
							if ((bool)method.Invoke (null, new object[]{ assembly }))
							{
								string warning = null;
								var w = type.GetField ("WARNING", BindingFlags.Static | BindingFlags.NonPublic);
								if (w.FieldType == typeof(string))
								{
									warning = (string)w.GetValue (null);
								}

								warnings.Add(new IncompatibilityWarning(assembly.name, typeAssemblyName, warning));
							}
						}
						catch (Exception e)
						{
							Debug.LogWarning("[IncompatibilityChecker]: Exception encountered while running check for " + typeAssemblyName + ": \n" + e.ToString());
						}
					}
				}
			}

			//show the gui, but only if there are warnings
			if (warnings.Count > 0)
			{
				GameObject obj = new GameObject ("");
				obj = (GameObject)Instantiate (obj);
				obj.name = "IncompatibilityPopup";

				var popup = obj.AddComponent<IncompatibilityPopup> ();
				popup.warnings = warnings.ToArray();
			}

			//set all of the values to show that we're finished
			foreach (var field in fields)
			{
				var type = field.DeclaringType;

				var b = type.GetField ("FINISHED", BindingFlags.Static | BindingFlags.Public);
				if (b.FieldType == typeof(bool))
				{
					b.SetValue (null, true);
				}
			}
		}


		Type[] GetAllCheckers()
		{
			List<Type> types = new List<Type> ();

			foreach (var assembly in AssemblyLoader.loadedAssemblies)
			{
				foreach (var type in assembly.assembly.GetTypes())
				{
					if (type.Name == "IncompatibilityChecker" && type.BaseType == typeof(MonoBehaviour))
					{
						types.Add (type);
					}
				}
			}

			return types.ToArray ();
		}

		public class IncompatibilityWarning
		{
			public string incompatibilty;
			public string mod;
			public string warning;

			public IncompatibilityWarning(string mod, string incompat, string warning = null)
			{
				this.mod = mod;
				this.incompatibilty = incompat;
				this.warning = warning;
			}
		}

		public class IncompatibilityPopup : MonoBehaviour
		{
			public IncompatibilityWarning[] warnings;

			Vector2 scroll = Vector2.zero;
			Rect rect = GetCenteredRect(500f, 400f);
			GUISkin skin = HighLogic.Skin;

			static GUIStyle DescriptionStyle;
			static GUIStyle LabelStyle;
			static GUIStyle NoteBoldStyle;
			static GUIStyle NoteNotBoldStyle;
			static GUIStyle LabelBoldStyle;

			void Start()
			{
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

				skin.label = NewKerbolConfig.LabelStyle;

				InputLockManager.SetControlLock (ControlTypes.MAIN_MENU, "INCOMPATIBILITY_CHECKER_MENU_LOCK");
			}
			void OnDestroy()
			{
				InputLockManager.RemoveControlLock ("INCOMPATIBILITY_CHECKER_MENU_LOCK");
			}

			void OnGUI()
			{
				GUI.skin = skin;
				GUI.Window (105036, rect, Window, "Incompatibilities Detected");
			}

			void Window(int id)
			{
				GUILayout.BeginVertical ();

				scroll = GUILayout.BeginScrollView (scroll);

				for(int i = 0; i < warnings.Length; i++)
				{
					var incomp = warnings [i];
					GUILayout.BeginVertical (skin.box);

					GUILayout.Label (incomp.mod + " is incompatible with " + incomp.incompatibilty + ".");
					if (incomp.warning != null)
					{
							GUILayout.Label (incomp.warning, DescriptionStyle);
							GUILayout.Space (10f);
					}

					GUILayout.EndVertical ();
				}

				GUILayout.EndScrollView ();

				if (GUILayout.Button ("Okay")) 
				{
					DestroyImmediate (this);
				}

				GUILayout.EndVertical ();
				GUI.DragWindow ();
			}

			static Vector2 ScreenCenter
			{get{return new Vector2 (Screen.width / 2, Screen.height / 2);}}

			static Rect GetCenteredRect(float width, float height)
			{
				return new Rect (ScreenCenter.x - (width / 2), ScreenCenter.y - (height / 2), width, height);
			}
		}
	}
}

