using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	public static class PQSExtensions
	{
		public static T GetPQSMod<T>(this PQS pqs) where T : PQSMod
		{
			foreach (var mod in pqs.gameObject.GetComponentsInChildren<T>())
			{
				return mod;
			}
			Utils.Log ("Returning null!");
			return null;
		}

		public static T[] GetPQSMods<T>(this PQS pqs) where T : PQSMod
		{
			List<T> mods = new List<T> ();
			foreach (var mod in pqs.GetComponentsInChildren<T>())
			{
				mods.Add (mod);
			}
			return mods.ToArray ();
		}
	}
}

