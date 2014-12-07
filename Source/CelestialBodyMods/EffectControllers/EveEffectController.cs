using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewKerbol
{
	[EffectControllerScenes(true, false, false)]
	public class EveEffectController : EffectController
	{
		//lightning
		public static bool LightningEnabled = true;
		public float Lightning_MinTime = 10f;
		public float Lightning_MaxRandTime = 35f;
		public float Lightning_MinStrength = 0.25f;
		public float Lightning_MaxStrength = 1f;
		public float Lightning_MinSoundOffset = 0.1f;
		public float Lightning_MaxSoundOffset = 4f;
		public double Lightning_MinAltitude = 35000;
//		public static AudioClip Lightning_AudioClip1 = null;
//		public static AudioClip Lightning_AudioClip2 = null;
//		public static AudioClip Lightning_AudioClip3 = null;
		float nextStrikeTime;
		float nextStrikeStrength;
		float nextStrikeOffset;
		static GameObject dirLightPrefab;
		GameObject dirLight;
//		AudioSource source;

		//wind
		public float Wind_MaxForce = 10f;
		public float Wind_ChuteTearThreshold = 0.7f;
		public float Wind_TimeMultiplier = 30f;

		void Start()
		{
			if (!NewKerbolConfig.ModEnabled)
			{
				DestroyImmediate (this);
				return;
			}

			//lightning
			if (dirLightPrefab == null)
			{
				dirLightPrefab = new GameObject ("EveLightning", typeof(Light), typeof(AudioSource));
				var l = dirLightPrefab.light;
				l.intensity = 0f;
				l.type = LightType.Directional;
				l.color = new Color (0.85f, 0.9f, 1.0f);
			}
			dirLight = (GameObject)Instantiate (dirLightPrefab);
			dirLight.transform.position = Vector3.zero;

//			source = dirLight.audio;
//			if (Lightning_AudioClip1 == null)
//			{
//				Lightning_AudioClip1 = Utils.LoadAudio ("thunder1");
//			}
//			if (Lightning_AudioClip2 == null)
//			{
//				Lightning_AudioClip2 = Utils.LoadAudio ("thunder2");
//			}
//			if (Lightning_AudioClip3 == null)
//			{
//				Lightning_AudioClip3 = Utils.LoadAudio ("thunder3");
//			}
//			source.maxDistance = 50000f;
//			source.minDistance = 10000f;
//			source.rolloffMode = AudioRolloffMode.Linear;

			nextStrikeTime = this.NextLightningTime ();
			nextStrikeStrength = this.NextLightningStrength ();
			nextStrikeOffset = this.NextLightningOffset ();

			Wind_MaxForce = NewKerbolConfig.MaxWindStrength;
		}

		void LateUpdate()
		{
			if (FlightGlobals.ActiveVessel.mainBody.bodyName == "Eve")
			{
				//values
				var upAxis = FlightGlobals.getUpAxis (FlightGlobals.ActiveVessel.GetWorldPos3D ());
				var mainBody = FlightGlobals.ActiveVessel.mainBody;
				var vessel = FlightGlobals.ActiveVessel;

				//lightning
				dirLight.transform.rotation = Quaternion.Euler(upAxis * -1);
				//dirLight.transform.position = Vector3.zero;

				//wind
				float atmPressure = (float)FlightGlobals.getStaticPressure (vessel.altitude, mainBody);
				if (TimeWarp.WarpMode != TimeWarp.Modes.HIGH)
				{
					//sine/cos functions to simulate a circular wind pattern
					float longStrength = (float)(Math.Sin (Planetarium.GetUniversalTime () / (double)Wind_TimeMultiplier));
					float latStrength = (float)(Math.Cos (Planetarium.GetUniversalTime () / (double)Wind_TimeMultiplier));
					//take atmosphere pressure into account
					longStrength *= atmPressure;
					latStrength *= atmPressure;

					//calculate longitude wind direction
					var lattarget = mainBody.GetWorldSurfacePosition (vessel.latitude, vessel.longitude, 20000);
					var latorigin = mainBody.GetWorldSurfacePosition (vessel.latitude, (vessel.longitude + 1) >= 360 ? (vessel.longitude + 1) - 360 : (vessel.longitude + 1), 20000);
					var latdir = (lattarget - latorigin).normalized;

					//calculate the latitude wind direction
					var lontarget = mainBody.GetWorldSurfacePosition (vessel.latitude, vessel.longitude, 20000);
					var lonorigin = mainBody.GetWorldSurfacePosition ((vessel.latitude + 1) >= 90 ? (vessel.latitude + 1) - 180 : (vessel.latitude + 1), vessel.longitude, 20000);
					var londir = (lontarget - lonorigin).normalized;

					//apply a random element to it, and lerp them together
					var blend = Random.Range (0.4f, 0.6f);
					var dir = Vector3.Lerp (latdir, londir, blend);
					var strength = Mathf.Lerp (latStrength, longStrength, blend);

					//apply wind to parts with physics
					foreach (var vess in FlightGlobals.Vessels.FindAll(v => v.loaded || v == vessel))
					{
						foreach (var part in vessel.parts)
						{
							{
								if (part.physicalSignificance == Part.PhysicalSignificance.FULL && part.rb != null)
								{
									part.rb.AddForce (dir * strength * Wind_MaxForce);
								}

								//only cut chutes if the settings say so
								if(NewKerbolConfig.DoParachutesCut)
								{
									//cut chutes above a certain wind strength
									if (part.Modules.Contains ("ModuleParachute"))
									{
										var parachute = part.Modules ["ModuleParachute"] as ModuleParachute;
										if (strength >= Wind_ChuteTearThreshold && parachute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED)
										{
											float rand = UnityEngine.Random.Range (1f, 1000f);
											if (rand < 5f)
											{
												parachute.CutParachute ();

												FlightLogger.eventLog.Add ("[" + Utils.FormatTime (vessel.missionTime) + "]: The parachutes on " + part.partInfo.name + " were lost due to high wind speeds.");
											}
										}
										if (parachute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED)
										{
											float rand = UnityEngine.Random.Range (1f, 1000f);
											if (rand < 2f)
											{
												parachute.CutParachute ();
												FlightLogger.eventLog.Add ("[" + Utils.FormatTime (vessel.missionTime) + "]: The parachutes on " + part.partInfo.name + " were lost due to high wind speeds.");
											}
										}
									}

									//same(ish) as above, but for realchutes! :D
									if (part.Modules.Contains ("RealChuteModule"))
									{
										var module = part.Modules ["RealChuteModule"];
										var methodInfo = module.GetType ().GetMethod ("GUICut");

										float rand = UnityEngine.Random.Range (1f, 1000f);
										if (rand < 2f && vess.altitude < 4250)
										{
											methodInfo.Invoke (module, new object[]{ });
											FlightLogger.eventLog.Add ("[" + Utils.FormatTime (vessel.missionTime) + "]: The parachutes on " + part.partInfo.name + " were lost due to high wind speeds.");
										}
									}
								}
							}
						}
					}
				}

				if (vessel.altitude < Lightning_MinAltitude)
				{
					//lightning
					if (nextStrikeTime <= 0)
					{
						StartCoroutine (LightningRoutine ());
						nextStrikeTime = this.NextLightningTime ();
					}
					else
					{
						nextStrikeTime -= TimeWarp.deltaTime;
					}
				}
			}
		}
		
		float NextLightningTime()
		{
			if (FlightGlobals.ActiveVessel.mainBody.bodyName == "Eve" && FlightGlobals.ActiveVessel.altitude < Lightning_MinAltitude)
				return Lightning_MinTime + Random.Range (0f, Lightning_MaxRandTime);
			else
				return 100f;
		}
		float NextLightningStrength()
		{
			if (FlightGlobals.ActiveVessel.mainBody.bodyName == "Eve" && FlightGlobals.ActiveVessel.altitude < Lightning_MinAltitude)
				return Random.Range (Lightning_MinStrength, Lightning_MaxStrength);
			else
				return 0f;
		}
		float NextLightningOffset()
		{
			if (FlightGlobals.ActiveVessel.mainBody.bodyName == "Eve" && FlightGlobals.ActiveVessel.altitude < Lightning_MinAltitude)
				return Random.Range (Lightning_MinSoundOffset, Lightning_MaxSoundOffset);
			else
				return 0f;
		}
//		AudioClip NextAudioClip()
//		{
//			int rand = UnityEngine.Random.Range (1, 3);
//			switch (rand)
//			{
//			case 1:
//				return Lightning_AudioClip1;
//			case 2:
//				return Lightning_AudioClip2;
//			case 3:
//				return Lightning_AudioClip3;
//			}
//			return null;
//		}

		//main lightning coroutine
		private IEnumerator<YieldInstruction> LightningRoutine()
		{
			//only do lightning in physwarp or real time
			if (TimeWarp.WarpMode != TimeWarp.Modes.HIGH)
			{
				//strobe party!
				Utils.Log ("Lightning struck!");
				dirLight.light.intensity = nextStrikeStrength * 8f;
				yield return new WaitForSeconds (0.1f);
				dirLight.light.intensity = 0f;
				yield return new WaitForSeconds (0.1f);

				dirLight.light.intensity = nextStrikeStrength * 4f;
				yield return new WaitForSeconds (0.1f);
				dirLight.light.intensity = 0f;
				yield return new WaitForSeconds (0.1f);

				dirLight.light.intensity = nextStrikeStrength * 2f;
				yield return new WaitForSeconds (0.1f);
				dirLight.light.intensity = 0f;
//				yield return new WaitForSeconds (nextStrikeOffset);
//
//				Utils.Log ("You heard the lightning strike!");
//				source.volume = GameSettings.AMBIENCE_VOLUME * 2;
//				source.PlayOneShot (this.NextAudioClip ());

				nextStrikeStrength = this.NextLightningStrength ();
				nextStrikeOffset = this.NextLightningOffset ();
			}
			yield return null;
		}
	}
}

