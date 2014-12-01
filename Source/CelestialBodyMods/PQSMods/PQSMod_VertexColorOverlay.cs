using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	public class PQSMod_VertexColorOverlay : PQSMod
	{
		public MapSO overlay;

		public override void OnSetup ()
		{
			this.requirements = PQS.ModiferRequirements.MeshColorChannel;
		}

		public override void OnVertexBuild (PQS.VertexBuildData data)
		{
			var color = overlay.GetPixelColor (data.u, data.v);

			//if the alpha is 0, use current color. if the alpha is 1, use new color
			data.vertColor = Color.Lerp (data.vertColor, color, color.a);
		}
	}
}

