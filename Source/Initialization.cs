using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NewKerbol
{
	public class Initialization
	{
		//equations
		static double GetNewPeriod(CelestialBody body)
		{
			return 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.Mass + body.referenceBody.Mass));
		}
		static double GetNewSOI(CelestialBody body)
		{
			return body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
		}
		static double GetNewHillSphere(CelestialBody body)
		{
			return body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1 / 3);
		}

		public static void AddModule(string partName, string name)
		{
			var part = PartLoader.LoadedPartsList.Find(p => p.name == partName);
			try
			{
				part.partPrefab.AddModule (name);
			}
			catch {}
		}

		public static List<CelestialBodyMod> Mods = new List<CelestialBodyMod> ();

		public static void PreBuild()
		{
			foreach (var part in PartLoader.LoadedPartsList)
			{
				AddModule (part.name, "ModuleLaytheLight");
			}
			AddModule ("kerbalEVA", "ModuleEVALaytheZombie");

			foreach (var t in ScaledSpace.Instance.scaledSpaceTransforms) 
			{
				if (t.name == "Jool")
				{
					Utils.Log ("Found JoolMesh");
					CelestialBodyMod.JoolMesh = t.gameObject.GetComponent<MeshFilter> ().mesh;
				}
			}

			//goes though all types to setup
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (type.BaseType == typeof(CelestialBodyMod))
				{
					string targetString = "";
					CelestialBody target;

					bool targetFound = false;
					var attributes = Attribute.GetCustomAttributes (type);
					foreach (var attr in attributes)
					{
						if (attr is CelestialBodyTarget)
						{
							targetString = (attr as CelestialBodyTarget).target;
							targetFound = true;
						}
					}
					if (targetFound == false)
					{
						Utils.LogWarning ("Target attribute not found: " + type.Name);
						continue;
					}

					target = Utils.GetCelestialBody (targetString);
					if (target == null)
					{
						Utils.LogWarning ("Target attribute found, but the target did not exsist: " + type.Name);
						continue;
					}

					var mod = (CelestialBodyMod)Activator.CreateInstance (type);
					mod.Target = target;
					Mods.Add (mod);
				}
			}
		}

		public static void SetScience(ConfigNode scienceNode)
		{
			//science dialogs
			foreach(var definition in GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION"))
			{
				foreach(var node in scienceNode.GetNodes("EXPERIMENT_DEFINITION_EDIT"))
				{
					if (definition.GetValue ("id") == node.GetValue ("id"))
					{
						var results = definition.GetNode ("RESULTS");
						var newResults = node.GetNode ("RESULTS");

						//remove definintion's current science dialogs
						results.ClearValues ();

						//iterate over each new result and add it to the definition
						foreach (ConfigNode.Value value in newResults.values)
						{
							results.AddValue (value.name, value.value);
						}
					}
				}
			}

			//science multipliers
			if (scienceNode.HasNode ("SCIENCE_MULTIPLIERS"))
			{
				var mults = scienceNode.GetNode ("SCIENCE_MULTIPLIERS");

				foreach (ConfigNode node in mults.nodes)
				{
					foreach (var body in FlightGlobals.Bodies)
					{
						if (node.name == body.bodyName)
						{
							ParseScienceParams (body.scienceValues, node);
						}
					}
				}
			}
		}


		public static void PostBuild()
		{
			//export
			ConfigNode exportNode = Utils.LoadConfig ("PluginData/Export.cfg");
			foreach (var mod in Mods)
			{
				if(exportNode.HasNode(mod.Target.bodyName))
				{
					var node = exportNode.GetNode (mod.Target.bodyName);
					var pqs = mod.Target.pqsController;

					int width = 1024;
					double maxHeight = 10000.0;
					bool hasOcean = false;
					double oceanHeight = 0.0;
					Color oceanColor = Color.blue;

					node.TryGetValue ("width", ref width);
					node.TryGetValue ("maxHeight", ref maxHeight);
					node.TryGetValue ("hasOcean", ref hasOcean);
					node.TryGetValue ("oceanHeight", ref oceanHeight);
					node.TryGetValue ("oceanColor", ref oceanColor);

					var maps = pqs.CreateMaps (width, maxHeight, hasOcean, oceanHeight, oceanColor);

					Utils.SaveTexturePNG ("export/" + mod.Target.bodyName + "_color", maps [0]);
					Utils.SaveTexturePNG ("export/" + mod.Target.bodyName + "_bump", maps [1]);
				}
			}

			//orbit calcs
			foreach (var body in FlightGlobals.Bodies)
			{
				if (body.flightGlobalsIndex != 0)
				{
					body.sphereOfInfluence = GetNewSOI (body);
					body.hillSphere = GetNewHillSphere (body);
					body.orbit.period = GetNewPeriod (body);

					body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
					body.orbit.orbitPercent = body.orbit.meanAnomalyAtEpoch / (Math.PI * 2);
					body.orbit.ObTAtEpoch = body.orbit.orbitPercent * body.orbit.period;
				}
			}

			Utils.Log ("Rearranging orbiting bodies");
			foreach (var body in FlightGlobals.Bodies)
			{
				if (body.flightGlobalsIndex != 0)
				{
					if (!body.referenceBody.orbitingBodies.Contains (body))
						body.referenceBody.orbitingBodies.Add (body);
				}
			}
			foreach (var body in FlightGlobals.Bodies)
			{
				var orbitingBodies = new List<CelestialBody> (body.orbitingBodies);
				foreach (var orbiting in orbitingBodies)
				{
					if (orbiting.referenceBody != body)
						body.orbitingBodies.Remove (orbiting);
				}
			}

			foreach (var body in FlightGlobals.Bodies)
			{
				if (body.orbitDriver != null)
				{
					//final orbit update to make sure everything works
					body.orbitDriver.UpdateOrbit ();
				}
				if (body.flightGlobalsIndex == 0)
				{
					//change the name last so that nothing screws up
					body.bodyName = "New Kerbol";
				}
			}
		}
		
		static void ParseScienceParams(CelestialBodyScienceParams scienceParams, ConfigNode node)
		{
			node.TryGetValue ("flyingHighAltitudeThreshold", ref scienceParams.flyingAltitudeThreshold);
			node.TryGetValue ("FlyingHighDataValue", ref scienceParams.FlyingHighDataValue);
			node.TryGetValue ("FlyingLowDataValue", ref scienceParams.FlyingLowDataValue);
			node.TryGetValue ("InSpaceHighDataValue", ref scienceParams.InSpaceHighDataValue);
			node.TryGetValue ("InSpaceLowDataValue", ref scienceParams.InSpaceLowDataValue);
			node.TryGetValue ("LandedDataValue", ref scienceParams.LandedDataValue);
			node.TryGetValue ("RecoveryValue", ref scienceParams.RecoveryValue);
			node.TryGetValue ("spaceAltitudeThreshold", ref scienceParams.spaceAltitudeThreshold);
			node.TryGetValue ("SplashedDataValue", ref scienceParams.SplashedDataValue);
		}
	}
}

