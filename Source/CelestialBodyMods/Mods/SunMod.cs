using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[CelestialBodyTarget("Sun")]
	public class SunMod : CelestialBodyMod
	{
		double SunRadius = 340100000;

		protected override void SetupScaled (GameObject scaled)
		{
			var mr = scaled.GetComponent<MeshRenderer> ();

			//set sun color to red
			if (NewKerbolConfig.UseRedSun)
			{
				mr.material.SetColor ("_EmitColor0", Utils.Color (255, 112, 107));
				mr.material.SetColor ("_EmitColor1", Utils.Color (255, 255, 255));
				mr.material.SetColor ("_SunspotColor", Utils.Color (255, 255, 255));
				mr.material.SetFloat ("_SunspotPower", 1.0f);
				mr.material.SetColor ("_RimColor", Utils.Color (235, 102, 87));
				mr.material.SetFloat ("_RimPower", 0.27f);

				//remove corona
				foreach (var corona in Resources.FindObjectsOfTypeAll<SunCoronas> ())
				{
					corona.gameObject.SetActive (false);
				}
			}

			//rescale scaled mesh to new radius
			var mf = scaled.GetComponent<MeshFilter> ();
			this.RescaleMesh (mf.mesh);

		}
		protected override void SetupBody (CelestialBody body)
		{
			body.Radius = SunRadius;
		}
	}
}

