using System;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public class SerializablePortalData
	{
		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		public SerializableVolume[] volumes;
		public SerializableOccluder[] occluders;
		public List<VisbilityData> visibilityTable;
	}
}
