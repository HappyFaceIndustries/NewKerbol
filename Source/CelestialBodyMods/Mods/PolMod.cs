using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Pol:
	 * 
	*/

	//TODO: Pol
	[CelestialBodyTarget("Pol")]
	public class PolMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 15000000;
			body.orbit.eccentricity = 0.47;
			body.orbit.inclination = 85;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Eve");
		}
	}
}

