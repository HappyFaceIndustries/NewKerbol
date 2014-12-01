using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NewKerbol
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	internal class CelestialBodyModInit : MonoBehaviour
	{
		//equations
		double GetNewPeriod(CelestialBody body)
		{
            return 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.Mass + body.referenceBody.Mass));
        }
        double GetNewSOI(CelestialBody body)
        {
            return body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
        }
        double GetNewHillSphere(CelestialBody body)
        {
            return body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1 / 3);
        }

		static bool finished = false;
		void Start()
		{
			if (!NewKerbolConfig.ModEnabled)
			{
				DestroyImmediate (this);
				return;
			}

			if (!finished)
			{
				Init ();
				finished = true;
			}
		}

		internal static void AddModule(string partName, string name)
		{
			var part = PartLoader.LoadedPartsList.Find(p => p.name == partName);
			try
			{
				part.partPrefab.AddModule (name);
			}
			catch {}
		}

		List<CelestialBodyMod> Mods = new List<CelestialBodyMod> ();

		private void Init()
		{
			Utils.Log ("Init starting...");

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

			//disable scatter: temporary until I can write a custom scatter pqsmod
			GameSettings.PLANET_SCATTER = false;

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

			foreach (var mod in Mods)
			{
				mod.Start ();
			}

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

			Utils.Log ("Init finished!");
		}
	}

	public class CelestialBodyMod
	{ 
		public CelestialBody Target = null;

		public static Mesh JoolMesh;

		public double origRadius;
		public float origScale;
		public float newScale;

		public void Start()
		{
			OnStart ();

			origRadius = Target.Radius;

			SetupBody (Target);
			Target.CBUpdate ();
			if (Target.pqsController != null)
			{
				Target.pqsController.radius = Target.Radius;
				foreach (var child in Target.pqsController.ChildSpheres)
					child.radius = Target.Radius;

				SetupPQS (Target.pqsController);
			}
			foreach (var t in ScaledSpace.Instance.scaledSpaceTransforms) 
			{
				if (t.name == Target.name)
				{
					origScale = t.localScale.x;
					newScale = (float)(Target.Radius / origRadius);
					SetupScaled (t.gameObject);
				}
			}
		}
		protected virtual void OnStart() {}
		
		protected virtual void SetupBody (CelestialBody body) {}
		protected virtual void SetupPQS (PQS pqs) {}
		protected virtual void SetupScaled(GameObject scaled) {}

		public void UpdateAFG(AtmosphereFromGround ag)
		{
			Log ("Updating atmosphere visuals");

			var body = Target;
			ag.outerRadius = ((float)body.Radius * 1.025f) * ScaledSpace.InverseScaleFactor;
			ag.innerRadius = ag.outerRadius * 0.975f;
			ag.outerRadius2 = ag.outerRadius * ag.outerRadius;
			ag.innerRadius2 = ag.innerRadius * ag.innerRadius;
			ag.scale = 1f / (ag.outerRadius - ag.innerRadius);
			ag.scaleDepth = -0.25f;
			ag.scaleOverScaleDepth = ag.scale / ag.scaleDepth;
			ag.invWaveLength = new Color(1f / Mathf.Pow(ag.waveLength[0], 4), 1f / Mathf.Pow(ag.waveLength[1], 4), 1f / Mathf.Pow(ag.waveLength[2], 4), 0.5f);
			ag.transform.localScale = Vector3.one * ((float)(body.Radius + body.maxAtmosphereAltitude) / (float)body.Radius);
			ag.doScale = false;
			//stupid private methods
			MethodInfo start = ag.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
			start.Invoke(ag, new object[]{});

			Log ("Atmosphere visual update complete!");
		}

		public void RecalculateScaledMesh(GameObject scaled, PQS pqs, CelestialBody body)
		{
			Log ("Recalculating scaled mesh");
			var mf = scaled.GetComponent<MeshFilter> ();
			var mesh = new Mesh ();
			Utils.CopyMesh (JoolMesh, mesh);
			float scaleFactor = (float)(origRadius / (1000 * 6000 * (double)origScale));

			Utils.MatchVerts(mesh, body.pqsController, body.ocean ? body.Radius : 0.0, scaleFactor);
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();

			mf.mesh = mesh;

			scaled.transform.localScale = (Vector3.one * newScale) * origScale;
			Log ("Recalculation of mesh complete!");
		}

		//Logging
		protected void Log(object message)
		{
			Utils.Log ("[" + this.Target.bodyName + "]: " + message);
		}
		protected void LogError(object message)
		{
			Utils.LogError ("[" + this.Target.bodyName + "]: " + message);
		}
		protected void LogWarning(object message)
		{
			Utils.LogWarning ("[" + this.Target.bodyName + "]: " + message);
		}
	}
}

