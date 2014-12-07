using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewKerbol
{
	[EffectControllerScenes(true, false, true)]
	public class PlanetariumHandler : EffectController
	{
		void Start()
		{
			Utils.Log ("Beginning reordering of map targets...");

			List<MapObject> bodyTargets = new List<MapObject>();
			List<MapObject> nonBodyTargets = new List<MapObject>();

			foreach (var target in PlanetariumCamera.fetch.targets)
			{
				if (target.type == MapObject.MapObjectType.CELESTIALBODY && target.celestialBody != null)
				{
					bodyTargets.Add (target);
				}
				else
				{
					nonBodyTargets.Add (target);
				}
			}

			MapObject[] finalBodyTargets = new MapObject[17];
			finalBodyTargets [0] = bodyTargets.Find (t => t.celestialBody.flightGlobalsIndex == 0); //sun
			finalBodyTargets [1] = bodyTargets.Find (t => t.celestialBody.bodyName == "Moho");
			finalBodyTargets [2] = bodyTargets.Find (t => t.celestialBody.bodyName == "Jool");
			finalBodyTargets [3] = bodyTargets.Find (t => t.celestialBody.bodyName == "Laythe");
			finalBodyTargets [4] = bodyTargets.Find (t => t.celestialBody.bodyName == "Tylo");
			finalBodyTargets [5] = bodyTargets.Find (t => t.celestialBody.bodyName == "Dres");
			finalBodyTargets [6] = bodyTargets.Find (t => t.celestialBody.bodyName == "Ike");
			finalBodyTargets [7] = bodyTargets.Find (t => t.celestialBody.bodyName == "Eve");
			finalBodyTargets [8] = bodyTargets.Find (t => t.celestialBody.bodyName == "Pol");
			finalBodyTargets [9] = bodyTargets.Find (t => t.celestialBody.bodyName == "Kerbin");
			finalBodyTargets [10] = bodyTargets.Find (t => t.celestialBody.bodyName == "Mun");
			finalBodyTargets [11] = bodyTargets.Find (t => t.celestialBody.bodyName == "Gilly");
			finalBodyTargets [12] = bodyTargets.Find (t => t.celestialBody.bodyName == "Bop");
			finalBodyTargets [13] = bodyTargets.Find (t => t.celestialBody.bodyName == "Duna");
			finalBodyTargets [14] = bodyTargets.Find (t => t.celestialBody.bodyName == "Eeloo");
			finalBodyTargets [15] = bodyTargets.Find (t => t.celestialBody.bodyName == "Vall");
			finalBodyTargets [16] = bodyTargets.Find (t => t.celestialBody.bodyName == "Minmus");

			var finalList = new List<MapObject> (finalBodyTargets);
			finalList.AddRange (nonBodyTargets);

			PlanetariumCamera.fetch.targets = finalList;

			Utils.Log ("Reordering of map targets complete!");
		}
	}
}

