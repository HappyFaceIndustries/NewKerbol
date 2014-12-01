using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Kerbin:
	 * Larger polar ice caps
	 * farther from sun
	 * not much else
	*/

	[CelestialBodyTarget("Kerbin")]
	public class KerbinMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 19500000000;
		}
	}
}

