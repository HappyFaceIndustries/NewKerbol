using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	//its waaaay better than Squad's stupid code! :D
	public class PQSMod_BetterHeightColorMap : PQSMod
	{
		public LandClass[] landClasses;
		public Color baseColor = Color.gray;
		public Simplex simplex;

		public override void OnSetup ()
		{
			this.requirements = PQS.ModiferRequirements.MeshColorChannel;
		}

		public override void OnVertexBuild (PQS.VertexBuildData data)
		{
			Color color;

			LandClass land = SelectLandClassByHeight (data.vertHeight);

			if (land == null)
			{
				color = baseColor;
				goto End;
			}
			else
				color = land.color;

			float noise = (float)simplex.noise (data.directionFromCenter);
			color = Color.Lerp (color, land.noiseColor, Mathf.Clamp (Mathf.Abs (noise), 0f, land.noiseThreshold));

			End:
			data.vertColor = color;
		}

		public LandClass SelectLandClassByHeight(double height)
		{
			height = height - sphere.radius;

			foreach (var lc in landClasses)
			{
				if (height <= lc.altEnd && height >= lc.altStart)
				{
					return lc;
				}
			}
			return null;
		}

		public class LandClass
		{
			public string landClassName;
			public Color color;
			public Color noiseColor;
			public float noiseThreshold;
			public double altStart;
			public double altEnd;
		
			public LandClass(string name, double altStart, double altEnd, Color color, Color noiseColor, float noiseThreshold)
			{
				this.landClassName = name;
				this.color = color;
				this.noiseColor = noiseColor;
				this.noiseThreshold = noiseThreshold;
				this.altStart = altStart;
				this.altEnd = altEnd;
			}
		}
	}
}

