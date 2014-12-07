using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NewKerbol
{
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

		public void RescaleMesh(Mesh mesh)
		{
			Log ("Rescaling mesh: " + mesh.name);

			double newRadius = Target.Radius;
			float scale = (float)(newRadius / origRadius);
			Utils.ScaleVerts (mesh, scale);

			Log ("Rescaling of mesh complete!");
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

