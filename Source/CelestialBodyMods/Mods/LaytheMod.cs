using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Laythe:
	 * Mountianous terrain covering roughly 30% of the surface
	 * Oxygen atmosphere
	 * Closer to the sun than Kerbin, but farther that Eve
	 * Patches of very green grass and patches of grey-green grass seperated by sand strips
	 * Bioluminescent algae covers the oceans
	 * Grass glows green when pressure is put on it
	 * The algae releases zombifying spores into the atmosphere, zombifying any exposed kerbals in a matter of seconds
	*/

	[CelestialBodyTarget("Laythe")]
	public class LaytheMod : CelestialBodyMod
	{
		protected override void SetupPQS (PQS pqs)
		{
			//new heightmap
			var height = pqs.GetPQSMod<PQSMod_VertexHeightMap> ();
			height.heightMap = MapSO.CreateInstance<MapSO> ();
			var heightMap = Utils.LoadTexture ("laythe_height.png");
			height.heightMap.CreateMap (MapSO.MapDepth.Greyscale, heightMap);
			GameObject.Destroy (heightMap);

			//all that cool stuff
			SetupLaythe (pqs);

			//rebuild sphere
			pqs.RebuildSphere ();
		}
		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material = new Material (Shader.Find ("Terrain/Scaled Planet (Simple)"));
			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Laythe_color.png");
			//mr.material.SetTexture ("_rimColorRamp", Utils.LoadTexture ("Scaled/Rims/Rim_lime.png"));

			var mats = scaled.renderer.materials;
			foreach (var m in mats)
			{
				Log (m.name);
			}
			var newMats = new List<Material> ();

			var emissiveScaledShader = Utils.LoadShader ("Shaders/Compiled-EmissiveScaled.shader");
			var mat = new Material (emissiveScaledShader);
			mat.SetTexture ("_EmissiveMap", Utils.LoadTexture ("Scaled/LaytheOcean_color.png"));
			mat.SetColor ("_Color", Utils.Color (56, 181, 69));
			mat.SetFloat ("_Brightness", 0.75f);
			mat.SetFloat ("_Transparency", 0.4f);

			newMats.Add (mat);
			newMats.AddRange (mats);

			scaled.renderer.materials = newMats.ToArray ();

			var afg = scaled.transform.FindChild ("Atmosphere").gameObject.GetComponent<AtmosphereFromGround> ();
			afg.waveLength = Utils.Color(183, 157, 196);

			UpdateAFG (afg);

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 10000000;
			body.orbit.eccentricity = 0;
			body.orbit.inclination = 0;
			body.orbitDriver.orbitColor = Utils.Color (90, 220, 15);

			var heat = AeroEffectController.GetNewReentryEffect ();
			var mach = AeroEffectController.GetNewMachEffect ();

			heat.color.max = Utils.Color (137, 255, 0);
			//keep min same so that it fades to orange

			mach.color.max = Utils.Color (186, 238, 128);
			//keep min same so it fades in from white

			//light up the thing with bioluminescent stuff!
			mach.lightPower.max = 2;
			mach.lightPower.min = 0.75f;

			AeroEffectController.Add (body, heat, mach);
		}
		
		void SetupLaythe(PQS pqs)
		{
			var land = pqs.GetPQSMod<PQSLandControl> ();
			land.createScatter = false;

			var kerbinPQS = Utils.GetCelestialBody ("Kerbin").pqsController;

			//for grass and stuff
			pqs.surfaceMaterial = new Material (kerbinPQS.surfaceMaterial);

			var newlcs = new List<PQSLandControl.LandClass> (land.landClasses);
			foreach(var lc in land.landClasses)
			{
				if (lc.landClassName == "Mud")
				{
					lc.color = Utils.Color (128, 115, 65);
					lc.noiseColor = Utils.Color (128, 115, 65);
				}
				if (lc.landClassName == "BaseLand")
				{
					lc.color = Utils.Color(51, 92, 51);
					lc.noiseColor = Utils.Color(51, 102, 51);
				}
				if (lc.landClassName == "SeaFloor")
				{
					lc.color = Utils.Color(0, 90, 0);
					lc.noiseColor = Utils.Color(0, 115, 0);
				}
				if (lc.landClassName == "Beach")
				{
					lc.color = Utils.Color(202, 189, 139);
					lc.noiseColor = Utils.Color(202, 189, 139);
				}
				if (lc.landClassName == "IceCaps" || lc.landClassName == "Snow")  //die! >:C
				{
					newlcs.Remove (lc);
				}
			}
			land.landClasses = newlcs.ToArray ();
			land.OnSetup ();

			PQS ocean = null;
			foreach (var child in pqs.ChildSpheres)
			{
				Log (child.gameObject.name);
				if (child.gameObject.name == "LaytheOcean")
					ocean = child;
			}
			if (ocean != null)
			{
				Log ("LaytheOcean found!");

				var OceanFX = ocean.transform.FindChild ("OceanFX").gameObject;

				var fx = ocean.GetPQSMod<PQSMod_OceanFX> ();
				var textures = fx.watermain;

				Shader emissiveQuadShader = Utils.LoadShader ("Shaders/Compiled-EmissiveQuad.shader");
				var emissiveMaterial = new Material (emissiveQuadShader);
				
				ocean.surfaceMaterial = emissiveMaterial;
				ocean.surfaceMaterial.SetTextureScale ("_EmissiveMap", new Vector2 (0.2f, 0.2f));

				var emissiveFx = OceanFX.AddComponent<PQSMod_EmissiveOceanFX> ();
				emissiveFx.order = 101;
				emissiveFx.modEnabled = true;
				emissiveFx.sphere = ocean;

				emissiveFx.color = Utils.Color (56, 181, 69, 255);
				emissiveFx.textures = textures;
				emissiveFx.brightness = 2f;
				emissiveFx.alpha = 0.15f;

				emissiveFx.OnSetup ();
			}
			else
				LogError ("LaytheOcean not found... :C");  //:'C
		}
	}
}

