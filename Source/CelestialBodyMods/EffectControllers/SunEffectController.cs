using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[EffectControllerScenes(true, true, true)]
	public class SunEffectController : EffectController
	{
		void Start()
		{
			//only do stuff if it should use the orange sun
			if (NewKerbolConfig.UseRedSun)
			{
				//recolor flare
				Sun.Instance.sunFlare.color = Utils.Color (238, 102, 87);

				//sunlight change
				var sunLight = GameObject.Find ("SunLight");
				if (sunLight != null)
				{
					sunLight.light.intensity = 0.7f;
					sunLight.light.color = Utils.Color (255, 211, 206);
				}

				//iva sunlight change
				var ivaSun = GameObject.Find ("IVASun");
				if (ivaSun != null)
				{
					ivaSun.light.intensity = 0.7f;
					ivaSun.light.color = Utils.Color (255, 211, 206);
				}

				//scaledspace sunlight
				var scaledSunLight = GameObject.Find ("Scaledspace SunLight");
				if (scaledSunLight != null)
				{
					scaledSunLight.light.intensity = 0.7f;
					scaledSunLight.light.color = Utils.Color (255, 211, 206);
				}
			}

			//set ambient vacuum light to blackish
			var ambientLighting = FindObjectOfType<DynamicAmbientLight>();
			if (ambientLighting != null)
			{
				ambientLighting.vacuumAmbientColor = Utils.Color (25, 25, 25);
			}
		}
	}
}

