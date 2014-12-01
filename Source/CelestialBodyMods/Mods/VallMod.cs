using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Vall:
	 * 
	*/

	[CelestialBodyTarget("Vall")]
	public class VallMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 150000000000;
			body.orbit.eccentricity = 0.15;
			body.orbit.inclination = -10;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Sun");
		}
	}
}

