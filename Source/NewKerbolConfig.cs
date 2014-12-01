using System;
using UnityEngine;

namespace NewKerbol
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public sealed class NewKerbolConfig : MonoBehaviour
	{
		//settings node
		public static ConfigNode Settings {get; private set;}

		void Start()
		{
			//load settings
			Settings = Utils.LoadConfig("Settings.cfg");
		}

		GUISkin skin = HighLogic.Skin;

		Rect windowRect = new Rect(100f, 100f, 300f, 500f);
		void OnGUI()
		{
			GUI.skin = skin;

			windowRect = GUI.Window (42567, windowRect, Window, "New Kerbol Settings");
		}
		void OnDestroy()
		{
			Utils.SaveConfig (Settings, "Settings.cfg");

			if (!Settings.TryGetValue ("modEnabled", ref modEnabled))
			{
				Utils.LogWarning ("The config value 'enabled' was not found, or could not be parsed, so it will remain true.");
			}
		}

		Vector2 scroll = Vector2.zero;
		public void Window(int id)
		{
			GUILayout.BeginVertical ();

			if (Settings != null)
			{
				scroll = GUILayout.BeginScrollView (scroll);

				foreach (ConfigNode.Value value in Settings.values)
				{
					if (IsBool (value))
					{
						bool b = bool.Parse (value.value);
						b = GUILayout.Toggle (b, value.name);
						Settings.SetValue (value.name, b.ToString ());
					}
					else if (IsFloat(value))
					{
						GUILayout.BeginHorizontal ();

						float f = 0f;
						GUILayout.Label (value.name + ": ");
						f = float.Parse (GUILayout.TextField (value.value));
						Settings.SetValue (value.name, f.ToString ());

						GUILayout.EndHorizontal ();
					}
					else
					{
						GUILayout.BeginHorizontal ();

						GUILayout.Label (value.name + ": ");
						Settings.SetValue (value.name, GUILayout.TextField (value.value));

						GUILayout.EndHorizontal ();
					}
				}

				GUILayout.EndScrollView ();
			}
			else
			{
				GUILayout.Label ("Error showing settings GUI");
			}
			GUILayout.EndVertical ();

			GUI.DragWindow ();
		}

		bool IsBool(ConfigNode.Value value)
		{
			bool output = false;
			return bool.TryParse (value.value, out output);
		}
		bool IsFloat(ConfigNode.Value value)
		{
			float output = 0f;
			return float.TryParse (value.value, out output);
		}

		//whether or not the mod is enabled
		public static bool ModEnabled
		{
			get
			{
				return modEnabled;
			}
		}
		static bool modEnabled = true;
	}
}

