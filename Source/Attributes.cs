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
}

