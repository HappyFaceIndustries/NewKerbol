using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Moho:
	 * 
	*/

	//TODO: Moho
	//TODO: MohoEffectController
	[CelestialBodyTarget("Moho")]
	public class MohoMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 6850000000;
			body.orbit.eccentricity = 0.005;
			body.orbit.inclination = -1;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Sun");
			body.orbitDriver.orbitColor = Utils.Color (118, 40, 25);

			body.tidallyLocked = true;

			//TODO: make the red face the sun
		}

		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Moho_color.png");

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupPQS (PQS pqs)
		{
			//new heightmap
			var height = pqs.GetPQSMod<PQSMod_VertexHeightMap> ();
			height.heightMap = MapSO.CreateInstance<MapSO> ();
			height.heightMapDeformity = 20000;
			var heightMap = Utils.LoadTexture ("moho_height.png");
			height.heightMap.CreateMap (MapSO.MapDepth.Greyscale, heightMap);
			GameObject.Destroy (heightMap);

			//setup fine details
			var simplexAbsolute = pqs.GetPQSMod<PQSMod_VertexSimplexHeightAbsolute> ();
			simplexAbsolute.deformity = 100;
			var simplex = pqs.GetPQSMod<PQSMod_VertexSimplexHeight> ();
			simplex.modEnabled = false;


			//remove old colormap
			var noiseColor = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
			noiseColor.modEnabled = false;
			var heightColor = pqs.GetPQSMod<PQSMod_HeightColorMap> ();
			heightColor.modEnabled = false;

			var _Color = pqs.transform.FindChild ("_Color").gameObject;
			var colorRamp = _Color.AddComponent<PQSMod_HeightColorRamp> ();

			var ramp = new PQSMod_HeightColorRamp.ColorRamp();
			ramp.Add (Utils.Color (101, 48, 37), Utils.Color (104, 65, 58), -100f);
			ramp.Add (Utils.Color (118, 40, 25), Utils.Color (129, 64, 50), 3900f);
			ramp.Add (Utils.Color (155, 123, 105), Utils.Color (121, 102, 91), 13000f);
			ramp.Add (Utils.Color (90, 69, 57), Utils.Color (95, 79, 70), 17000f);
			ramp.Add (Utils.Color (115, 105, 100), Utils.Color (152, 148, 145), 20000f);
			ramp.Add (Utils.Color (115, 105, 100), Utils.Color (152, 148, 145), 100000f);

			//TODO: make ramp

			colorRamp.Ramp = ramp;
			colorRamp.simplex = new Simplex(666, 6, 0.6, 6); //>:D
			colorRamp.BaseColorBias = 0.1f;
			colorRamp.modEnabled = true;
			colorRamp.order = 202;
			colorRamp.sphere = pqs;

			pqs.RebuildSphere ();
		}
	}
}

