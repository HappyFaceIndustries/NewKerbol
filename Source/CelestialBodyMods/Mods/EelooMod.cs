using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	/* Eeloo:
	 * 
	*/

	//TODO: Eeloo
	//TODO: EelooEffectController: make it slippery
	[CelestialBodyTarget("Eeloo")]
	public class EelooMod : CelestialBodyMod
	{
		protected override void SetupBody (CelestialBody body)
		{
			body.orbit.semiMajorAxis = 12000000;
			body.orbit.eccentricity = 0.005;
			body.orbit.inclination = 0;
			body.orbit.referenceBody = Utils.GetCelestialBody ("Duna");
			body.Radius = 90000;
			body.GeeASL = 0.075;
		}

		protected override void SetupScaled (GameObject scaled)
		{
			this.RecalculateScaledMesh (scaled, Target.pqsController, Target);
		}

		protected override void SetupPQS (PQS pqs)
		{
			pqs.RebuildSphere ();
		}
	}
}

