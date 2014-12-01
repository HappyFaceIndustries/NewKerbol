using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Mun:
	 * 
	*/

	[CelestialBodyTarget("Mun")]
	public class MunMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis *= 2;
		}
	}
}

