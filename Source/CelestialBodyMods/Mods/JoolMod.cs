using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Jool:
	 * Yellow-pink gas giant with 2 large stripes
	 * Orbits between Moho and Eve
	 * Moons: Ike, Tylo, Dres, Laythe
	*/

	[CelestialBodyTarget("Jool")]
	public class JoolMod : CelestialBodyMod
	{
		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			mr.material = new Material (Shader.Find ("Terrain/Scaled Planet (Simple)"));

			mr.material.mainTexture = Utils.LoadTexture ("Scaled/Jool_color.png");

			var afg = scaled.transform.FindChild ("Atmosphere").gameObject.GetComponent<AtmosphereFromGround> ();
			afg.waveLength = Utils.Color (113, 136, 250);

			Log ("Scale: " + scaled.transform.localScale.ToString ());

			UpdateAFG (afg);
		}
		protected override void SetupBody (CelestialBody body)
		{
			body.atmosphericAmbientColor = Utils.Color (255, 215, 11);
			body.orbit.semiMajorAxis = 10500000000;
			body.orbit.eccentricity = 0.005;
			body.orbit.inclination = 2;
			body.orbitDriver.orbitColor = Utils.Color (255, 215, 11);
		}
	}
}

