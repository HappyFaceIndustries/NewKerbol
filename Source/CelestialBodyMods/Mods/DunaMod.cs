using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Duna:
	 * Red planet with snow covering ~70% of the surface.
	 * Orbits about as far as Dres, but slightly closer
	*/

	[CelestialBodyTarget("Duna")]
	public class DunaMod : CelestialBodyMod
	{
		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Duna_color.png");

			var afg = scaled.transform.FindChild ("Atmosphere").gameObject.GetComponent<AtmosphereFromGround> ();
			UpdateAFG (afg);

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupBody (CelestialBody body)
		{
			body.Radius = 250000;
			body.orbit.semiMajorAxis = 61600000000;
			body.orbitDriver.orbitColor = Utils.Color (201, 68, 11);
			body.atmosphereMultiplier = 0.15f;
			body.maxAtmosphereAltitude = 25000f;
			body.atmosphereScaleHeight = 2.5;
			body.useLegacyAtmosphere = true;
			body.GeeASL = 0.29;
		}

		protected override void SetupPQS(PQS pqs)
		{
			//disable easter eggs and color map
			DisableUnneededObjects (pqs);

			//change out heightmap for new one
			var height = pqs.GetPQSMod<PQSMod_VertexHeightMap> ();
			height.heightMap = MapSO.CreateInstance<MapSO> ();

			var heightMap = Utils.LoadTexture ("duna_height.png");
			height.heightMap.CreateMap (MapSO.MapDepth.Greyscale, heightMap);
			height.scaleDeformityByRadius = false;
			height.heightMapDeformity = 8000.0;
			GameObject.Destroy (heightMap);

			//new colormap
			var colorMapBlend = pqs.GetPQSMod<PQSMod_VertexColorMapBlend> ();
			colorMapBlend.modEnabled = false;
			var colorNoise = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
			colorNoise.modEnabled = false;

			var land = pqs.GetPQSMod<PQSLandControl> ();
			land.createScatter = false;
			land.createColors = false;

			var _LandClass = pqs.transform.FindChild ("_LandClass").gameObject;
			var heightColor = _LandClass.AddComponent<PQSMod_HeightColorRamp> ();
			heightColor.modEnabled = true;
			//just before the original color map, which is now disabled
			heightColor.order = 9999990;
			heightColor.sphere = pqs;

			//new color ramp
			var ramp = new PQSMod_HeightColorRamp.ColorRamp();
			ramp.Add(Utils.Color(149, 85, 58), Utils.Color(122, 58, 30), -100f);				//lowlands
			ramp.Add(Utils.Color(149, 85, 58), Utils.Color(122, 58, 30), 200f);					//end of lowlands
			ramp.Add (Utils.Color(192, 119, 87), Utils.Color(178, 79, 37), 1000f);				//fade into midlands
			ramp.Add (Utils.Color(192, 119, 87), Utils.Color(178, 79, 37), 2100f);				//end of midlands
			ramp.Add (Utils.Color(215, 170, 150), Utils.Color(220, 150, 120), 3000f);			//fade into highlands
			ramp.Add (new Color(0.9f, 0.9f, 0.9f), new Color(0.75f, 0.75f, 0.75f), 3200f);		//sharp start of snowlands
			ramp.Add (new Color(0.9f, 0.9f, 0.9f), new Color(0.75f, 0.75f, 0.75f), 5750f);		//snowlands
			ramp.Add(new Color(0.87f, 0.87f, 0.87f), new Color(0.8f, 0.8f, 0.85f), 6000f);		//sharp start of veryhighlands
			ramp.Add(new Color(0.87f, 0.87f, 0.87f), new Color(0.8f, 0.8f, 0.85f), 100000f);	//veryhighlands go to the top

			heightColor.Ramp = ramp;
			heightColor.simplex = new Simplex (555, 5, 0.5, 5);
			heightColor.BaseColorBias = 0.2f;
			heightColor.OnSetup ();

			//rebuild sphere
			pqs.RebuildSphere ();
		}

		void DisableUnneededObjects(PQS pqs)
		{
			//disable the kerbal face easter egg
			var Face = pqs.transform.FindChild ("Face").gameObject;
			foreach (var mod in Face.GetComponents<PQSMod>())
			{
				Log ("Face disabled");
				mod.modEnabled = false;
			}
			Face.SetActive (false);

			//disable the Curiosity Rover (MSL) easter egg
			var MSL = pqs.transform.FindChild ("MSL").gameObject;
			foreach (var mod in Face.GetComponents<PQSMod>())
			{
				Log ("Mars Science Laboratory disabled");
				mod.modEnabled = false;
			}
			MSL.SetActive (false);

			//disable the pyramid easter egg
			var Pyramid = pqs.transform.FindChild ("Pyramid").gameObject;
			foreach (var mod in Face.GetComponents<PQSMod>())
			{
				Log ("Pyramid disabled");
				mod.modEnabled = false;
			}
			Pyramid.SetActive (false);
		}
	}
}

