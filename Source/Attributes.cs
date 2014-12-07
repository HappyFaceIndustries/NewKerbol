using System;
using UnityEngine;

namespace NewKerbol
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CelestialBodyTarget : Attribute
	{
		public string target;

		public CelestialBodyTarget(string target)
		{
			this.target = target;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CelestialBodyNewName : Attribute
	{
		public string name;

		public CelestialBodyNewName(string name)
		{
			this.name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class EffectControllerScenes : Attribute
	{
		public bool flight = true;
		public bool trackingStation = true;
		public bool spaceCenter = true;

		public EffectControllerScenes(bool flight, bool trackingStation, bool spaceCenter)
		{
			this.flight = flight;
			this.trackingStation = trackingStation;
			this.spaceCenter = spaceCenter;
		}
	}
}

