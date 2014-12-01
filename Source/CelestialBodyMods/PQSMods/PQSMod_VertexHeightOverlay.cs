using System;

namespace NewKerbol
{
	public class PQSMod_VertexHeightOverlay : PQSMod
	{
		public MapSO overlay;
		public double Deformity;

		public override void OnSetup ()
		{
			this.requirements = PQS.ModiferRequirements.VertexMapCoords | PQS.ModiferRequirements.MeshCustomNormals;
		}

		public override double GetVertexMinHeight ()
		{
			return 0.0;
		}
		public override double GetVertexMaxHeight ()
		{
			return Deformity;
		}
		public override void OnVertexBuildHeight (PQS.VertexBuildData data)
		{
			var color = overlay.GetPixelColor (data.u, data.v);

			var height = (double)color.grayscale * Deformity;

			data.vertHeight += height;
		}
	}
}

