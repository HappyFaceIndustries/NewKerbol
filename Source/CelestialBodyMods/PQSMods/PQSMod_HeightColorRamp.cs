using System;
using UnityEngine;

namespace NewKerbol
{
	public class PQSMod_HeightColorRamp : PQSMod
	{
		public ColorRamp Ramp;
		public Simplex simplex;
		public float BaseColorBias = 0.2f;

		public override void OnSetup ()
		{
			this.requirements = PQS.ModiferRequirements.MeshColorChannel;
		}

		public override void OnVertexBuild (PQS.VertexBuildData data)
		{
			float height = (float)(data.vertHeight - sphere.radius);

			var colors = Ramp.Evaluate (height);
			var color = colors [0];
			var noiseColor = colors [1];

			double noise = simplex.noise (data.directionFromCenter);
			float blend = Mathf.Clamp01 (Mathf.Abs ((float)noise) + BaseColorBias);

			data.vertColor = Color.Lerp (color, noiseColor, blend);
		}

		public class ColorRamp
		{
			FloatCurve r;
			FloatCurve g;
			FloatCurve b;

			FloatCurve rn;
			FloatCurve gn;
			FloatCurve bn;

			public ColorRamp()
			{
				r = new FloatCurve();
				g = new FloatCurve();
				b = new FloatCurve();

				rn = new FloatCurve();
				gn = new FloatCurve();
				bn = new FloatCurve();
			}

			public Color[] Evaluate(float height)
			{
				var color = new Color (r.Evaluate (height), g.Evaluate (height), b.Evaluate (height));
				var noise = new Color (rn.Evaluate (height), gn.Evaluate (height), bn.Evaluate (height));

				return new Color[]{ color, noise };
			}

			public void Add(Color color, Color noiseColor, float height)
			{
				r.Add (height, color.r);
				g.Add (height, color.g);
				b.Add (height, color.b);

				rn.Add (height, noiseColor.r);
				gn.Add (height, noiseColor.g);
				bn.Add (height, noiseColor.b);
			}
		}
	}
}

