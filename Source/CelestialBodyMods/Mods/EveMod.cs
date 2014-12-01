using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[CelestialBodyTarget("Eve")]
	public class EveMod : CelestialBodyMod
	{
		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material = new Material (Shader.Find ("Terrain/Scaled Planet (Simple)"));
			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Eve_color.png");
			//mr.material.SetTexture ("_rimColorRamp", Utils.LoadTexture ("Scaled/Rims/Rim_yellow.png"));

			var afg = scaled.transform.FindChild ("Atmosphere").gameObject.GetComponent<AtmosphereFromGround> ();
			afg.waveLength = Utils.Color(108, 140, 196);

			UpdateAFG (afg);

			RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupBody (CelestialBody body)
		{
			body.Radius = 440000;

			body.useLegacyAtmosphere = true;
			body.maxAtmosphereAltitude = 100000;
			body.atmosphereMultiplier = 1.8f;
			body.atmosphereScaleHeight = 6;
			body.atmosphericAmbientColor = Utils.Color (54, 50, 41);

			body.orbitDriver.orbitColor = Utils.Color (235, 171, 74);
			body.orbit.semiMajorAxis = 12500000000;
			body.orbit.eccentricity = 0.04;
			body.orbit.inclination = 0.005;

			body.GeeASL = 1.1;

			//new aeroFX: longer tail, brighter, more wobble
			var mach = AeroEffectController.GetNewMachEffect ();
			mach.wobble.max = 2f;
			mach.wobble.min = 0.5f;
			mach.length.max *= 3f;
			mach.length.min *= 3f;
			mach.intensity.max *= 2f;
			mach.airspeedNoiseVolume.max *= 2f;
			mach.airspeedNoiseVolume.min *= 2f;

			AeroEffectController.Add (body, null, mach);
		}

		protected override void SetupPQS (PQS pqs)
		{
			var dunaPQS = Utils.GetCelestialBody ("Duna").pqsController;
			pqs.surfaceMaterial = new Material (dunaPQS.surfaceMaterial);

			var _LandClass = pqs.transform.FindChild ("_LandClass").gameObject;

			//disable current colormap
			var noiseColor = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
			noiseColor.modEnabled = false;
			var colorBlend = pqs.GetPQSMod<PQSMod_VertexColorMapBlend> ();
			colorBlend.modEnabled = false;
			var land = pqs.GetPQSMod<PQSLandControl> ();
			land.modEnabled = false;

			//add new colorRamp
			var colorRamp = _LandClass.AddComponent<PQSMod_HeightColorRamp> ();
			colorRamp.modEnabled = true;
			colorRamp.order = 9999990;
			colorRamp.sphere = pqs;

			var ramp = new PQSMod_HeightColorRamp.ColorRamp ();

			ramp.Add(Utils.Color (235, 171, 74), Utils.Color (235, 171, 74), -5000f);		//oceans
			ramp.Add(Utils.Color (235, 171, 74),Utils.Color (235, 171, 74), -50f);			//end of oceans
			ramp.Add(Utils.Color(155, 135, 88), Utils.Color(175, 149, 87), -10f);			//start of beaches
			ramp.Add(Utils.Color(155, 135, 88), Utils.Color(175, 149, 87), 100f);			//end of beaches
			ramp.Add (Utils.Color (212, 171, 75), Utils.Color (189, 158, 86), 200f);		//start of lowlands
			ramp.Add (Utils.Color (189, 158, 86), Utils.Color (155, 120, 60), 2000f);		//fade into midlands
			ramp.Add (Utils.Color (155, 120, 60), Utils.Color (172, 139, 78), 5000f);		//fade into highlands
			ramp.Add (Utils.Color (223, 189, 129), Utils.Color (220, 194, 149), 5500f);		//quickly fade out to mountain tops
			ramp.Add (Utils.Color (220, 194, 149), Utils.Color (186, 184, 180), 20000f);	//top of the curve

			colorRamp.Ramp = ramp;
			colorRamp.BaseColorBias = 0.5f;
			colorRamp.simplex = new Simplex (654, 6, 0.35, 4);
			colorRamp.OnSetup ();

			//new heightmap
			var height = pqs.GetPQSMod<PQSMod_VertexHeightMap> ();
			height.scaleDeformityByRadius = false;
			height.heightMapOffset = -500.0;
			height.heightMapDeformity = 7000.0;

			height.heightMap = MapSO.CreateInstance<MapSO> ();

			var heightMap = Utils.LoadTexture ("eve_height.png");
			height.heightMap.CreateMap (MapSO.MapDepth.Greyscale, heightMap);
			GameObject.Destroy (heightMap);

			//setup ocean
			PQS ocean = null;
			foreach (var child in pqs.ChildSpheres)
			{
				Log (child.gameObject.name);
				if (child.gameObject.name == "EveOcean")
					ocean = child;
			}
			if (ocean != null)
			{
				Log ("EveOcean found!");

				var OceanFX = ocean.transform.FindChild ("OceanFX").gameObject;

				ocean.surfaceMaterial.color = Utils.Color (235, 171, 74);
				ocean.surfaceMaterial.SetColor ("_ColorFromSpace", Utils.Color (235, 171, 74));
			}
		}
	}

	public class CloudRotation : MonoBehaviour
	{
		public float rotationTime = 60f;
		float rotationSpeed;

		void Start()
		{
			rotationSpeed = 360 / rotationTime;
		}

		void LateUpdate()
		{
			var rot = transform.localRotation.eulerAngles;
			if (rot.y >= 360f)
				rot.y = 0f;
			rot.y += rotationSpeed * TimeWarp.deltaTime;
			transform.localRotation = Quaternion.Euler (rot);
		}
	}
}

