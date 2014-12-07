using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Ike:
	 * 
	*/

	[CelestialBodyTarget("Ike")]
	public class IkeMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 110000000;
			body.orbit.eccentricity = 0.3;
			body.orbit.inclination = 10.1;
			//test
			body.orbitDriver.reverse = !body.orbitDriver.reverse;

			body.orbit.referenceBody = Utils.GetCelestialBody ("Jool");
		}
	}
}

