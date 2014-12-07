using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Tylo:
	 * 
	*/

	//TODO: Tylo
	[CelestialBodyTarget("Tylo")]
	public class TyloMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 46000000;
			body.orbit.eccentricity = 0.0005;
			body.orbit.inclination = 4.3;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Jool");
		}

		protected override void SetupPQS (PQS pqs)
		{

		}
	}
}

