using System;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public struct SerializableVolume
	{
		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //
		
		public Vector3 positionWS;
		public Quaternion rotationWS;
		public Vector3 scaleWS;
		public int volumeID;
		public int parentID;
		public int[] childIDs;
	}
}
