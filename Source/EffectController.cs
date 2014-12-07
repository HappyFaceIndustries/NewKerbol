using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NewKerbol
{
	//just a class to handle instantiating the effect controllers in a clean way, and disable them if needed
	//all effect controllers extend this class
	public class EffectController : MonoBehaviour
	{
		private void Awake()
		{
			if (!NewKerbolConfig.ModEnabled)
			{
				DestroyImmediate (this);
				return;
			}
			OnAwake ();
		}
		public virtual void OnAwake()
		{
		}
	}

	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class EffectControllerSpawner : MonoBehaviour
	{
		List<Spawnable> types = new List<Spawnable>();

		void Start()
		{
			DontDestroyOnLoad (this);

			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (type.BaseType == typeof(EffectController))
				{
					EffectControllerScenes sceneAttribute = null;

					var attributes = Attribute.GetCustomAttributes (type);
					foreach (var attr in attributes)
					{
						if (attr is EffectControllerScenes)
						{
							sceneAttribute = (EffectControllerScenes)attr;
						}
					}

					if (sceneAttribute != null)
					{
						var spawnable = new Spawnable ();
						spawnable.flight = sceneAttribute.flight;
						spawnable.trackingStation = sceneAttribute.trackingStation;
						spawnable.spaceCenter = sceneAttribute.spaceCenter;
						spawnable.type = type;

						types.Add (spawnable);
					}
					else
					{
						Utils.LogWarning ("EffectControllerSpawner " + type.Name + " has no scene attribute");
					}
				}
			}
		}

		void OnLevelWasLoaded(int level)
		{
			bool flight = ((GameScenes)level) == GameScenes.FLIGHT;
			bool trackingStation = ((GameScenes)level) == GameScenes.TRACKSTATION;
			bool spaceCenter = ((GameScenes)level) == GameScenes.SPACECENTER;

			Spawn (flight, trackingStation, spaceCenter);
		}

		void Spawn (bool flight, bool trackingStation, bool spaceCenter)
		{
			GameObject prefab = new GameObject ("");
			foreach (var type in types)
			{
				if (flight && type.flight)
				{
					var obj = (GameObject)Instantiate (prefab);
					obj.name = type.Name;
					obj.AddComponent (type.type);
					Utils.Log ("[EffectControllerSpawner]: Spawned controller " + type.Name);
				}
				if (trackingStation && type.trackingStation)
				{
					var obj = (GameObject)Instantiate (prefab);
					obj.name = type.Name;
					obj.AddComponent (type.type);
					Utils.Log ("[EffectControllerSpawner]: Spawned controller " + type.Name);
				}
				if (spaceCenter && type.spaceCenter)
				{
					var obj = (GameObject)Instantiate (prefab);
					obj.name = type.Name;
					obj.AddComponent (type.type);
					Utils.Log ("[EffectControllerSpawner]: Spawned controller " + type.Name);
				}
			}
		}

		public class Spawnable
		{
			public bool flight = true;
			public bool trackingStation = false;
			public bool spaceCenter = false;
			public Type type;

			public string Name
			{
				get
				{
					if (type != null)
						return type.Name;
					else
						return "no-name";
				}
			}
		}
	}
}

