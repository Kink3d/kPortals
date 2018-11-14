using System;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public struct SerializableVolume
	{
		public Vector3 positionWS;
		public Quaternion rotationWS;
		public Vector3 scaleWS;
		public int parentID;
		public int[] childIDs;
	}
}
