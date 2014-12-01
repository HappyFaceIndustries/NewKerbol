using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class LaytheEffectController : MonoBehaviour
	{
		static Shader EmissiveQuadShader = null;

		public static Material KerbalEyes = null;
		public static Material KerbalPupils = null;
		public static Material KerbalHead = null;

		public static Material GlowKerbalEyes = null;
		public static Material GlowKerbalPupils = null;
		public static Material GlowKerbalHead = null;

		static Texture2D GreenVeins = null;
		static Texture2D GreenHead = null;

		void Start()
		{
			if (!NewKerbolConfig.ModEnabled)
			{
				DestroyImmediate (this);
				return;
			}

			NewKerbolConfig.Settings.TryGetValue ("ZombificationRate", ref ModuleEVALaytheZombie.ZombificationRate);

			foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
			{
				if (material.name == "mat_eyes01" && KerbalEyes == null)
				{
					KerbalEyes = material;
				}
				if (material.name == "mat_pupils01" && KerbalPupils == null)
				{
					KerbalPupils = material;
				}
				if (material.name == "kerbalHead" && KerbalHead == null)
				{
					KerbalHead = material;
				}
			}
			//reuse the laythe textures, they seem to work well enough
			if (GreenVeins == null)
				GreenVeins = Utils.LoadTexture ("laythe_height.png");
			if (GreenHead == null)
				GreenHead = Utils.LoadTexture ("Scaled/Laythe_bump.png");

			if (EmissiveQuadShader == null)
				EmissiveQuadShader = Utils.LoadShader ("Shaders/Compiled-EmissiveQuad.shader");

			if (GlowKerbalEyes == null)
			{
				GlowKerbalEyes = new Material (EmissiveQuadShader);
				GlowKerbalEyes.SetTexture ("_EmissiveMap", GreenVeins);
				GlowKerbalEyes.SetColor ("_Color", Utils.Color (56, 181, 69));
				GlowKerbalEyes.SetFloat ("_Transparency", 0.0f);
				GlowKerbalEyes.SetFloat ("_Brightness", 1.5f);
			}
			if (GlowKerbalPupils == null)
			{
				GlowKerbalPupils = new Material (EmissiveQuadShader);

				var tex = new Texture2D (1, 1, TextureFormat.ARGB32, false);
				tex.SetPixel (0, 0, Color.white);
				tex.Apply (false, false);

				GlowKerbalPupils.SetTexture ("_EmissiveMap", tex);
				GlowKerbalPupils.SetColor ("_Color", Color.red);
				GlowKerbalPupils.SetFloat ("_Transparency", 0.0f);
				GlowKerbalPupils.SetFloat ("_Brightness", 1.0f);
			}
			if (GlowKerbalHead == null)
			{
				GlowKerbalHead = new Material (EmissiveQuadShader);
				GlowKerbalHead.SetTexture ("_EmissiveMap", GreenHead);
				GlowKerbalHead.SetColor ("_Color", Color.red);
				GlowKerbalHead.SetFloat ("_Transparency", 0.25f);
				GlowKerbalHead.SetFloat ("_Brightness", 1.5f);
			}
		}

		//debug gui for emissive ocean effects
		void Update()
		{
			if (Input.GetKey (KeyCode.LeftAlt) && Input.GetKey (KeyCode.L) && Input.GetKeyDown (KeyCode.E))
			{
				open = !open;
			}
		}
		bool open = false;
		void OnGUI()
		{
			if (open)
			{
				GUI.Label (new Rect (300f, 250f, 300f, 30f), "Laythe Ocean FX Speed: ");
				PQSMod_EmissiveOceanFX.framesPerFrame = (int)GUI.HorizontalSlider (new Rect (300f, 300f, 300f, 30f), (float)PQSMod_EmissiveOceanFX.framesPerFrame, 1f, 100f);
			}
		}

		//thanks to TextureReplacer for some of this code
		public static void ChangeKerbalMaterials(Component component, Material eyeMaterial, Material pupilMaterial, Material headMaterial)
		{
			foreach (var rend in component.GetComponentsInChildren<Renderer>(true))
			{
				SkinnedMeshRenderer smr;
				try
				{
					smr = rend as SkinnedMeshRenderer;
				}
				catch
				{
					continue;
				}

				if (smr == null)
					continue;

				switch (smr.name)
				{
				case "eyeballLeft":
				case "eyeballRight":
					smr.material = eyeMaterial;
					break;
				case "pupilLeft":
				case "pupilRight":
					smr.material = pupilMaterial;
					break;
				case "headMesh01":
					smr.materials = new Material[]{ headMaterial, smr.material };
					break;
				}
			}
		}
	}

	public class ModuleEVALaytheZombie : PartModule
	{
		public string KerbalName;
		public ProtoCrewMember crew;

		kerbalExpressionSystem exSystem = null;

		//yes capital letter
		public static float ZombificationRate = 1f;

		[KSPField(guiActive = true, guiName = "Zombification", guiUnits = "%", isPersistant = true)]
		public float ZombificationProgress = 0f;

		public override void OnStart (StartState state)
		{
			if (vessel.evaController == null)
			{
				Utils.LogError ("null evaController! ABORT");
				this.isEnabled = false;
			}

			foreach (var crew in vessel.GetVesselCrew())
			{
				KerbalName = crew.name;
				this.crew = crew;
			}
			foreach (var system in Resources.FindObjectsOfTypeAll<kerbalExpressionSystem>())
			{
				if (system.protoCrewMember == crew)
				{
					exSystem = system;
					Utils.Log ("Found expressionSystem");
				}
			}
		}

		//no capital letter
		float zombificationRate = 1f;
		bool isZombie = false;
		
		public void LateUpdate()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (vessel.mainBody.bodyName == "Laythe" && vessel.altitude < 15000.0)
				{
					//set rate to go twice as fast on the ground
					if (part.GroundContact)
					{
						zombificationRate = 2f * ZombificationRate;
					}
					else
						zombificationRate = 1f * ZombificationRate;

					//apply effets of zombification
					if (ZombificationProgress >= 100f)
					{
						ZombificationProgress = 100f;
						if(!isZombie)
						{
							ScreenMessages.PostScreenMessage (new ScreenMessage("<color=red>" + KerbalName + " has is now a zombie!</color>", 10f, ScreenMessageStyle.UPPER_CENTER));
							FlightLogger.eventLog.Add ("[" + Utils.FormatTime(vessel.missionTime) + "]: " + KerbalName + " was zombified while on EVA.");
							LaytheEffectController.ChangeKerbalMaterials (part, LaytheEffectController.GlowKerbalEyes, LaytheEffectController.GlowKerbalPupils, LaytheEffectController.GlowKerbalHead);
							isZombie = true;
						}
					}
					else
						ZombificationProgress += ZombificationRate * TimeWarp.deltaTime;
				}

				if (isZombie && exSystem != null)
				{
					exSystem.wheeLevel = 100f;
					exSystem.panicLevel = 0f;
					exSystem.fearFactor = 0f;
				}
				else if(exSystem != null)
				{
					exSystem.wheeLevel = 0f;
					exSystem.panicLevel = 100f;
					exSystem.fearFactor = 100f;
				}
			}
		}
	}

	public class ModuleLaytheLight : PartModule
	{
		public static GameObject GlowPrefab;

		Light glow;
		GameObject glowObj;

		float glowIntensity = 0.45f;
		float glowSpeed = 0.5f;

		public override void OnStart (StartState state)
		{
			if (GlowPrefab == null)
			{
				GlowPrefab = new GameObject ("Glow", typeof(Light));
			}

			glowObj = (GameObject)Instantiate (GlowPrefab, part.transform.position, part.transform.rotation);
			glowObj.transform.parent = part.transform.parent;

			glow = glowObj.gameObject.light;
			glow.type = LightType.Point;
			glow.intensity = 0f;
			glow.range = 20f;
			glow.color = Utils.Color (56, 181, 69, 255);
		}

		public override void OnUpdate ()
		{
			if (part.GroundContact && vessel.mainBody.bodyName == "Laythe")
			{
				var dirD = vessel.mainBody.position - vessel.GetWorldPos3D();
				var dir = new Vector3 ((float)dirD.x, (float)dirD.y, (float)dirD.z);
				var ray = new Ray (part.transform.position, dir);
				//only PQS stuff
				var layerMask = 1 << 15;
				var rayOut = new RaycastHit ();
				Physics.Raycast (ray.origin, ray.direction, out rayOut, 10f, layerMask);

				glowObj.transform.position = rayOut.point;

				if (glow.intensity < glowIntensity)
					glow.intensity = glow.intensity + glowSpeed * Time.deltaTime;
				else
					glow.intensity = glowIntensity;
			}
			else
			{
				if (glow.intensity > 0f)
					glow.intensity = glow.intensity - glowSpeed * Time.deltaTime;
				else
					glow.intensity = 0f;
			}
		}
	}
}

