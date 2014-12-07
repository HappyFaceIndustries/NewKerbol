using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Dres:
	 * 
	*/

	[CelestialBodyTarget("Dres")]
	public class DresMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 90000000;
			body.orbit.eccentricity = 0.06;
			body.orbit.inclination = 0.33;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Jool");
		}
	}
}

