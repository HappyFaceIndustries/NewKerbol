using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[EffectControllerScenes(true, false, false)]
	public class AeroEffectController : EffectController
	{
		//static things
		static AeroFXState defaultReentryState;
		static AeroFXState defaultMachState;
		static Dictionary<CelestialBody, AeroFXState> newReentryStates = new Dictionary<CelestialBody, AeroFXState> ();
		static Dictionary<CelestialBody, AeroFXState> newMachStates = new Dictionary<CelestialBody, AeroFXState> ();

		public static void Add(CelestialBody body, AeroFXState reentry, AeroFXState mach)
		{
			if (reentry != null)
				newReentryStates.Add (body, reentry);
			if (mach != null)
				newMachStates.Add (body, mach);
		}

		public static AeroFXState GetNewReentryEffect()
		{
			var fx = new AeroFXState ();

			//set to default values, gotten from the DumpAeroFXState() method below
			fx.airspeedNoisePitch = MinMaxFloat (0.5f, 2f);
			fx.airspeedNoiseVolume = MinMaxFloat (0, 1);
			//same color, but min's alpha is zero
			fx.color = MinMaxColor (new Color(1, 0.294f, 0.114f, 0f), new Color(1f, 0.294f, 0.114f, 1f));
			fx.edgeFade = MinMaxFloat (0, 0.3f);
			fx.falloff1 = MinMaxFloat (0.9f, 0.9f);
			fx.falloff2 = MinMaxFloat (1, 1);
			fx.falloff3 = MinMaxFloat (2, 1);
			fx.intensity = MinMaxFloat (0, 0.11f);
			//tail length
			fx.length = MinMaxFloat (5, 15);
			fx.lightPower = MinMaxFloat (3, 8);
			fx.wobble = MinMaxFloat (1, 1);

			return fx;
		}
		public static AeroFXState GetNewMachEffect()
		{
			var fx = new AeroFXState ();

			//set to default values, gotten from the DumpAeroFXState() method below
			fx.airspeedNoisePitch = MinMaxFloat (0.3f, 1f);
			fx.airspeedNoiseVolume = MinMaxFloat (0, 0.5f);
			//same as reentry: same color, but min's alpha is zero
			fx.color = MinMaxColor (new Color(0.22f, 0.22f, 0.22f, 0f), new Color(0.22f, 0.22f, 0.22f, 1f));
			fx.edgeFade = MinMaxFloat (0, 0);
			fx.falloff1 = MinMaxFloat (0.9f, 0.9f);
			fx.falloff2 = MinMaxFloat (0.6f, 0.6f);
			fx.falloff3 = MinMaxFloat (0.5f, 0.5f);
			fx.intensity = MinMaxFloat (0, 0.11f);
			//tail length
			fx.length = MinMaxFloat (2, 3.5f);
			fx.lightPower = MinMaxFloat (0f, 0f);
			fx.wobble = MinMaxFloat (0, 0.2f);

			return fx;
		}
		static AeroFXState.MinMaxFloat MinMaxFloat(float min, float max)
		{
			var mmf = new AeroFXState.MinMaxFloat ();
			mmf.min = min;
			mmf.max = max;
			return mmf;
		}
		static AeroFXState.MinMaxColor MinMaxColor(Color min, Color max)
		{
			var mmc = new AeroFXState.MinMaxColor ();
			mmc.min = min;
			mmc.max = max;
			return mmc;
		}

		//per-instance things
		AerodynamicsFX aeroFX;

		void Awake()
		{
			if (!NewKerbolConfig.ModEnabled)
			{
				DestroyImmediate (this);
				return;
			}

			aeroFX = aeroFX = Resources.FindObjectsOfTypeAll<AerodynamicsFX> ()[0];

			if (defaultReentryState == null)
				defaultReentryState = aeroFX.ReentryHeat;
			if (defaultMachState == null)
				defaultMachState = aeroFX.Condensation;

			//DumpAeroFXState (defaultReentryState, "Reentry");
			//DumpAeroFXState (defaultMachState, "Mach");

			GameEvents.onDominantBodyChange.Add (OnDominantBodyChange);
		}
		void OnDestroy()
		{
			GameEvents.onDominantBodyChange.Remove (OnDominantBodyChange);
		}

		void OnDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> fromto)
		{
			bool useDefaultMach = true;
			bool useDefaultReentry = true;

			foreach (var state in newReentryStates)
			{
				if (fromto.to == state.Key)
				{
					aeroFX.ReentryHeat = state.Value;
					useDefaultReentry = false;
					Utils.Log ("Changed reentry effect to " + state.Key.bodyName + " preset");
				}
			}
			foreach (var state in newMachStates)
			{
				if (fromto.to == state.Key)
				{
					aeroFX.Condensation = state.Value;
					useDefaultMach = false;
					Utils.Log ("Changed mach effect to " + state.Key.bodyName + " preset");
				}
			}

			if (useDefaultReentry)
			{
				aeroFX.ReentryHeat = defaultReentryState;
				Utils.Log ("Changed reentry effect to default preset");
			}
			if (useDefaultMach)
			{
				aeroFX.Condensation = defaultMachState;
				Utils.Log ("Changed mach effect to default preset");
			}
		}

		//debug
		public void DumpAeroFXState(AeroFXState state, string name)
		{
			Utils.Log ("Dumping AeroFXState: " + name);
			DumpMinMax (state.airspeedNoisePitch, "airSpeedNoisePitch");
			DumpMinMax (state.airspeedNoiseVolume, "airSpeedNoiseVolume");
			DumpMinMax (state.color, "color");
			DumpMinMax (state.edgeFade, "edgeFade");
			DumpMinMax (state.falloff1, "falloff1");
			DumpMinMax (state.falloff2, "falloff2");
			DumpMinMax (state.falloff3, "falloff3");
			DumpMinMax (state.intensity, "intensity");
			DumpMinMax (state.length, "length");
			DumpMinMax (state.lightPower, "lightPower");
			DumpMinMax (state.wobble, "wobble");
			Utils.Log ("Dump finished");
		}
		void DumpMinMax(AeroFXState.MinMaxFloat f, string name)
		{
			Utils.Log (name + ": " + f.min + " -> " + f.max);
		}
		void DumpMinMax(AeroFXState.MinMaxColor c, string name)
		{
			Utils.Log (name + ": " + c.min + " -> " + c.max);
		}
	}
}

