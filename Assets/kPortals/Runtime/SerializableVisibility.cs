using System;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public class VisbilityData
	{
		// -------------------------------------------------- //
		//                     CONSTRUCTOR                    //
		// -------------------------------------------------- //

		public VisbilityData(SerializableVolume volume, GameObject[] objects)
		{
			this.volume = volume;
			this.objects = objects;
		}

		// -------------------------------------------------- //
		//                    PUBLIC FIELDS                   //
		// -------------------------------------------------- //

		public SerializableVolume volume;
		public GameObject[] objects;
	}
}
