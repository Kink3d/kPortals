using System;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public struct SerializableOccluder
	{
		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //
		
		public Vector3 positionWS;
		public Quaternion rotationWS;
		public Vector3 scaleWS;
		public Mesh mesh;
	}
}
