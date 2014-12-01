using System;
using UnityEngine;

namespace NewKerbol
{
	public class CustomParticleEmitter : KSPParticleEmitter
	{
		[Obsolete("Dont touch this!")]
		public new bool doesAnimateColor = false;

		[Obsolete("Dont touch this!")]
		public new Color[] colorAnimation = new Color[1];

		public new void SetupProperties()
		{
			this.pe.useWorldSpace = this.useWorldSpace;
			this.pe.rndVelocity = this.rndVelocity;
			this.pe.emitterVelocityScale = this.emitterVelocityScale;
			this.pe.angularVelocity = this.angularVelocity;
			this.pe.rndAngularVelocity = this.rndAngularVelocity;
			this.pe.rndRotation = this.rndRotation;
			this.pa.worldRotationAxis = this.worldRotationAxis;
			this.pa.localRotationAxis = this.localRotationAxis;
			this.pa.sizeGrow = this.sizeGrow;
			this.pa.rndForce = this.rndForce;
			this.pa.force = this.force;
			this.pa.damping = this.damping;
			this.pr.castShadows = this.castShadows;
			this.pr.receiveShadows = this.recieveShadows;
			this.pr.lengthScale = this.lengthScale;
			this.pr.velocityScale = this.velocityScale;
			this.pr.maxParticleSize = this.maxParticleSize;
			this.pr.maxPartileSize = this.maxParticleSize;
			this.pr.particleRenderMode = this.particleRenderMode;
			this.pr.uvAnimationXTile = this.uvAnimationXTile;
			this.pr.uvAnimationYTile = this.uvAnimationYTile;
			this.pr.uvAnimationCycles = (float)this.uvAnimationCycles;
			this.pr.material = this.material;

			//removed the animate color stuff
			base.doesAnimateColor = false;
		}
		void Start()
		{
			Utils.Log ("CustomParticleEmitter says hello! :)");
		}

		void Update()
		{
			if (this.emit)
			{
				SetupProperties ();

				if (this.minEmission < 0)
					this.minEmission = 0;

				if (this.maxEmission > 0)
				{
					int rnd = UnityEngine.Random.Range (this.minEmission, this.maxEmission);
					for (int i = 0; i < rnd; i++)
					{
						EmitParticle ();
					}
				}
			}
		}
	}
}

