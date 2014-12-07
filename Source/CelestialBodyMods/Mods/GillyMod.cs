using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Gilly:
	 * 
	*/

	[CelestialBodyTarget("Gilly")]
	public class GillyMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 38000000000;
			body.orbit.eccentricity = 0.04;
			body.orbit.inclination = -0.4;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Sun");
			body.orbitDriver.orbitColor = Utils.Color (130, 130, 130);

			body.Radius = 35000;
			body.UpdateGeeASL (0.04);
		}

		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Gilly_color.png");

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupPQS (PQS pqs)
		{
			var color = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
			color.modEnabled = false;

			var land = pqs.GetPQSMod<PQSLandControl> ();
			land.modEnabled = false;

			var simplex = pqs.GetPQSMod<PQSMod_VertexSimplexHeightAbsolute> ();
			simplex.deformity = 1000;
			simplex.frequency = 4;
			simplex.octaves = 4;
			simplex.persistence = 0.9;
			simplex.seed = 45276;
			simplex.OnSetup ();

			var heightNoise = pqs.GetPQSMod<PQSMod_VertexHeightNoise> ();
			heightNoise.modEnabled = false;

			var _Height = pqs.transform.FindChild ("_Height").gameObject;
			var _Color = pqs.transform.FindChild ("_Color").gameObject;

			var noise = _Height.AddComponent<PQSMod_VertexSimplexHeightAbsolute>();
			noise.modEnabled = true;
			noise.order = 8;
			noise.sphere = pqs;
			noise.deformity = 20000;
			noise.frequency = 0.65;
			noise.octaves = 4;
			noise.persistence = 0.55;
			noise.seed = 45276;

			var noise2 = _Height.AddComponent<PQSMod_VertexSimplexHeightAbsolute>();
			noise2.modEnabled = true;
			noise2.order = 9;
			noise2.sphere = pqs;
			noise2.deformity = 4000;
			noise2.frequency = 3;
			noise2.octaves = 4;
			noise2.persistence = 0.65;
			noise2.seed = 45276;

			noise.OnSetup ();

			var flatten = _Height.AddComponent<PQSMod_FlattenOcean> ();
			flatten.modEnabled = true;
			flatten.order = 12;
			flatten.sphere = pqs;
			flatten.oceanRadius = 0.0;
			flatten.OnSetup ();

			var colorRamp = _Color.AddComponent<PQSMod_HeightColorRamp> ();
			colorRamp.modEnabled = true;
			colorRamp.order = 110;
			colorRamp.sphere = pqs;

			var ramp = new PQSMod_HeightColorRamp.ColorRamp ();
			ramp.Add (Utils.Color (30, 30, 30), Utils.Color (45, 45, 45), -100f);
			ramp.Add (Utils.Color (150, 150, 150), Utils.Color (200, 200, 200), 20000);
			ramp.Add (Utils.Color (150, 150, 150), Utils.Color (200, 200, 200), 60000);

			colorRamp.Ramp = ramp;
			colorRamp.simplex = new Simplex (45267, 3, 0.5, 8);
			colorRamp.BaseColorBias = 0.45f;

			pqs.RebuildSphere ();
		}
	}
}

