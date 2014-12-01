using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Minmus:
	 * 
	*/

	//TODO: Minmus
	//TODO: MinmusEffectController: make it bouncy! :D
	[CelestialBodyTarget("Minmus")]
	public class MinmusMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 5000000;
			body.orbit.eccentricity = 0.01;
			body.orbit.inclination = 40;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Vall");
			body.Radius = 30000;

			body.orbitDriver.orbitColor = Utils.Color (100, 203, 173);
		}

		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Minmus_color.png");

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupPQS (PQS pqs)
		{
			var fractal = pqs.GetPQSMod<PQSMod_VertexPlanet> ();
			fractal.deformity = 3500;
			fractal.seed = 425364;
			foreach (var lc in fractal.landClasses)
			{
				if (lc.name == "AbyPl")
				{
					lc.baseColor = Utils.Color (100, 203, 173);
					lc.colorNoise = Utils.Color (92, 224, 185);
				}
				if (lc.name == "Beach")
				{
					lc.baseColor = Utils.Color (96, 196, 169);
					lc.colorNoise = Utils.Color (96, 196, 169);
				}
				if (lc.name == "Grass")
				{
					lc.baseColor = Utils.Color (44, 94, 84);
					lc.colorNoise = Utils.Color (41, 111, 91);
				}
				if (lc.name == "Snow")
				{
					lc.baseColor = Utils.Color (66, 104, 96);
					lc.colorNoise = Utils.Color (76, 124, 114);
				}
			}

			var land = pqs.GetPQSMod<PQSLandControl> ();
			land.createScatter = false;

			pqs.RebuildSphere ();
		}
	}
}

