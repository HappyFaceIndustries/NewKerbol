using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetaryExpansion
{
	[KSPScenario(ScenarioCreationOptions.AddToNewCareerGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.SPH, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
	public class ObservablePlanets : ScenarioModule
	{
//		public static ObservablePlanet Duna;
//
//		public void Start()
//		{
//			if (Duna == null)
//			{
//				Duna = ObservablePlanet.CreateInstance ("Duna");
//			}
//		}
//
//		public void OnGUI()
//		{
//			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
//			{
//				if (GUI.Button (new Rect (220, 200, 100, 30), "Discover"))
//				{
//					Duna.Discover ();
//				}
//				if (GUI.Button (new Rect (220, 245, 100, 30), "Undiscover"))
//				{
//					Duna.Undiscover ();
//				}
//			}
//		}
	}

	public class ObservablePlanet
	{
		//values
		public CelestialBody Body;
		public OrbitDriver OrbitDriver;
		public OrbitRenderer Renderer;
		public GameObject Scaled;
		public MapObject Map;

		public string Name;
		
		public bool Discovered;

		//methods
		public void Discover()
		{
			Utils.Log ("Discovering " + Name);

			ReactivateOrbit ();

			//add map target
			if (PlanetariumCamera.fetch != null)
				PlanetariumCamera.fetch.AddTarget (Map);
			else
				Utils.LogWarning ("PlaneteriumCamera.fetch is null, cannot add map target!");

			Discovered = true;
		}
		public void Undiscover()
		{
			Utils.Log ("Undiscovering " + Name);

			DeactivateOrbit ();

			//remove map target
			if (PlanetariumCamera.fetch != null)
				PlanetariumCamera.fetch.RemoveTarget (Map);
			else
				Utils.LogWarning ("PlaneteriumCamera.fetch is null, cannot remove map target!");

			Discovered = false;
		}

		void DeactivateOrbit()
		{
			Renderer.drawMode = OrbitRenderer.DrawMode.OFF;
			Renderer.drawIcons = OrbitRenderer.DrawIcons.NONE;
		}
		void ReactivateOrbit()
		{
			Renderer.drawMode = OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE;
			Renderer.drawIcons = OrbitRenderer.DrawIcons.OBJ;
		}

		//create them
		public static ObservablePlanet CreateInstance(string planetName)
		{
			ObservablePlanet planet = new ObservablePlanet();

			planet.Name = planetName;

			foreach (var pbody in Resources.FindObjectsOfTypeAll<PSystemBody>())
			{
				if (pbody.celestialBody.bodyName == planetName)
				{
					planet.Body = pbody.celestialBody;
					planet.OrbitDriver = pbody.orbitDriver;
					planet.Scaled = pbody.scaledVersion;
				}
			}
			foreach (var map in PlanetariumCamera.fetch.targets)
			{
				if (map.celestialBody != null && map.celestialBody.bodyName == planetName)
					planet.Map = map;
			}
			foreach (var orb in Resources.FindObjectsOfTypeAll<OrbitRenderer>())
			{
				if (orb.gameObject.name == planetName)
				{
					planet.Renderer = orb;
				}
			}

			if (planet.Body == null || planet.Scaled == null || planet.Map == null)
				Utils.LogWarning ("Cannot create ObservablePlanet instance with the name: " + planetName);
			else
			{
				Utils.Log ("ObservablePlanet '" + planetName + "' created: " + planet.Body.bodyName + ", " + planet.Scaled.name + ", " + planet.Map.GetName());
				return planet;
			}

			planet = null;
			return null;
		}
	}
}

