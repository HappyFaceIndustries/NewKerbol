using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	public class PQSMod_EmissiveOceanFX : PQSMod
	{
		public Color color;
		public float alpha = 0.2f;
		public float brightness = 1.5f;
		public Texture2D[] textures;
		public int texIndex = 0;

		Material EmissiveMaterial;
		bool willWork = true;

		public override void OnSetup ()
		{
			EmissiveMaterial = sphere.surfaceMaterial;

			if (EmissiveMaterial != null && textures != null && textures.Length > 0)
				willWork = true;

			Utils.Log ("[LaytheOcean]: willWork: " + willWork.ToString());

			if (willWork)
			{
				EmissiveMaterial.SetTexture ("_EmissiveMap", textures [0]);
				EmissiveMaterial.SetColor ("_Color", color);
				EmissiveMaterial.SetFloat ("_Brightness", brightness);
				EmissiveMaterial.SetFloat ("_Transparency", alpha);
			}
		}

		int counter = 0;
		public static int framesPerFrame = 0;
		public override void OnUpdateFinished ()
		{
			if (willWork && framesPerFrame > 0) 
			{
				if (counter <= 0)
				{
					texIndex++;
					if (texIndex >= textures.Length)
						texIndex = 0;
					EmissiveMaterial.SetTexture ("_EmissiveMap", textures [texIndex]);

					EmissiveMaterial.SetColor ("_Color", color);
					EmissiveMaterial.SetFloat ("_Brightness", brightness);
					EmissiveMaterial.SetFloat ("_Transparency", alpha);

					counter = framesPerFrame;
				}
				else
					counter--;
			}
		}
	}
}

