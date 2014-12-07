using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Bop:
	 * 
	*/

	[CelestialBodyTarget("Bop")]
	public class BopMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 37000000000;
			body.orbit.eccentricity = 0.09;
			body.orbit.inclination = 5.5;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Sun");

			if (SpaceKraken)
			{
				body.orbitDriver.orbitColor = Utils.Color (92, 92, 92);
			}
			else
			{
				body.orbitDriver.orbitColor = Utils.Color (195, 177, 171);
			}
		}

		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			if(SpaceKraken)
				mr.material.mainTexture = Utils.LoadTexture ("Scaled/Kraken_color.png");

			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		//if true, use the kraken
		bool SpaceKraken = false;
		protected override void OnStart ()
		{
			if (!NewKerbolConfig.Settings.TryGetValue ("UseSpaceKraken", ref SpaceKraken))
				SpaceKraken = false;
		}

		protected override void SetupPQS (PQS pqs)
		{
			//MUA HA HA HA...
			if (SpaceKraken)
			{
				//disable the special craters
				var decals = pqs.GetPQSMods<PQSMod_MapDecal> ();
				foreach (var decal in decals)
				{
					decal.modEnabled = false;
				}
				var flattens = pqs.GetPQSMods<PQSMod_MapDecal> ();
				foreach (var flatten in flattens)
				{
					flatten.modEnabled = false;
				}

				//disable the heightmap, scatter, and colormap
				var scatter = pqs.GetPQSMod<PQSLandControl> ();
				scatter.modEnabled = false;
				var heightNoise = pqs.GetPQSMod<PQSMod_VertexHeightNoise> ();
				heightNoise.modEnabled = false;

				//collect gameobjects
				var _Color = pqs.transform.FindChild ("_Color").gameObject;
				var _Height = pqs.transform.FindChild ("_Height").gameObject;


				var simplexColor = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
				var simplex = pqs.GetPQSMod<PQSMod_VertexSimplexHeightAbsolute> ();

				simplexColor.modEnabled = false;

				//the guy can't have perfectly flat skin, can he?
				simplex.deformity = 50;
				simplex.frequency = 4;
				simplex.octaves = 4;
				simplex.persistence = 0.4;
				simplex.seed = 4;
				simplex.modEnabled = true;
				simplex.order = 6;
				simplex.OnSetup ();

				var height = _Height.AddComponent<PQSMod_VertexHeightMap> ();
				height.heightMap = CreateMapSO (Utils.LoadTexture ("Height/Kraken_height.png"));
				height.heightMapDeformity = 25000;
				height.heightMapOffset = 50.0;
				height.scaleDeformityByRadius = false;
				height.modEnabled = true;
				height.order = 5;
				height.sphere = pqs;
				height.OnSetup ();

				var color = _Height.AddComponent<PQSMod_VertexColorMap> ();
				color.vertexColorMap = CreateColorMapSO (Utils.LoadTexture ("Scaled/Kraken_color.png"));
				color.modEnabled = true;
				color.order = 200;
				color.sphere = pqs;
				color.OnSetup ();

				Log ("THE KRAKEN HAS RISEN! >:D");
			}
			else
			{
				Log ("The kraken decided to sleep in today... :'(");
			}

			pqs.RebuildSphere ();
		}

		//used to generate the texture
		private void BuildKraken(PQS pqs)
		{
			var _Color = pqs.transform.FindChild ("_Color").gameObject;
			var _Height = pqs.transform.FindChild ("_Height").gameObject;

			//height stuff
			var skin = pqs.GetPQSMod<PQSMod_VertexSimplexHeightAbsolute> ();
			var tentacles = _Height.AddComponent<PQSMod_VertexHeightMap> ();
			var eyes = _Height.AddComponent<PQSMod_VertexHeightMap> ();

			//the guy can't have perfectly flat skin, can he?
			skin.deformity = 50;
			skin.frequency = 4;
			skin.octaves = 4;
			skin.persistence = 0.4;
			skin.seed = 4;
			skin.modEnabled = true;
			skin.order = 20;
			skin.OnSetup ();

			//extrude the tentacles from it
			tentacles.heightMap = CreateMapSO (Utils.LoadTexture ("Kraken/tentacles_height.png"));
			tentacles.heightMapDeformity = 25000;
			tentacles.scaleDeformityByRadius = false;
			tentacles.heightMapOffset = 0.0;
			tentacles.modEnabled = true;
			tentacles.order = 21;
			tentacles.sphere = pqs;
			tentacles.OnSetup ();

			//then extrude the eyes
			eyes.heightMap = CreateMapSO (Utils.LoadTexture ("Kraken/eyes_height.png"));
			eyes.heightMapDeformity = 2000;
			eyes.scaleDeformityByRadius = false;
			eyes.heightMapOffset = 0.0;
			eyes.modEnabled = true;
			eyes.order = 22;
			eyes.sphere = pqs;
			eyes.OnSetup ();

			//color stuff
			var skinColor = pqs.GetPQSMod<PQSMod_VertexSimplexNoiseColor> ();
			var tentaclesColor = _Color.AddComponent<PQSMod_VertexColorOverlay> ();
			var eyesColor = _Color.AddComponent<PQSMod_VertexColorOverlay> ();

			skinColor.blend = 0.4f;
			skinColor.frequency = 4;
			skinColor.octaves = 4;
			skinColor.persistence = 0.4;
			skinColor.seed = 4;
			skinColor.order = 200;
			skinColor.colorStart = Utils.Color (67, 99, 14);
			skinColor.colorEnd = Utils.Color (67, 109, 14);
			skinColor.OnSetup ();

			//color the tentacles
			tentaclesColor.overlay = CreateColorMapSO (Utils.LoadTexture ("Kraken/tentacles_color.png"));
			tentaclesColor.modEnabled = true;
			tentaclesColor.order = 201;
			tentaclesColor.sphere = pqs;
			tentaclesColor.OnSetup ();

			//color the eyes
			eyesColor.overlay = CreateColorMapSO (Utils.LoadTexture ("Kraken/eyes_color.png"));
			eyesColor.modEnabled = true;
			eyesColor.order = 202;
			eyesColor.sphere = pqs;
			eyesColor.OnSetup ();
		}

		MapSO CreateMapSO(Texture2D heightMap)
		{
			var map = MapSO.CreateInstance<MapSO> ();
			map.CreateMap (MapSO.MapDepth.Greyscale, heightMap);
			return map;
		}
		MapSO CreateColorMapSO(Texture2D colorMap)
		{
			var map = MapSO.CreateInstance<MapSO> ();
			map.CreateMap (MapSO.MapDepth.RGBA, colorMap);
			return map;
		}
	}
}

