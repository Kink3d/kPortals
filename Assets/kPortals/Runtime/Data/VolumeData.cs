using System;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals
{
	public struct VolumeData
	{
		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		public Vector3 positionWS;
		public Vector3 scaleWS;
		public VolumeData[] children;

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Converts this VolumeData to a SerializableVolume array for storage.
        /// </summary>
		public SerializableVolume[] Serialize()
		{
			// Recursively serializable the VolumeData hierarchy
			List<SerializableVolume> serializableVolumes = new List<SerializableVolume>();
			var index = -1;
			SerializeRecursive(ref serializableVolumes, -1, out index);
			return serializableVolumes.ToArray();
		}

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

		private void SerializeRecursive(ref List<SerializableVolume> serializableVolumes, int parentID, out int volumeID)
		{
			// Serialize this volume
			volumeID = serializableVolumes.Count;
			SerializableVolume volume = new SerializableVolume()
			{
				positionWS = positionWS,
				rotationWS = Quaternion.identity,
				volumeID = volumeID,
				scaleWS = scaleWS,
				parentID = parentID
			};
			serializableVolumes.Add(volume);

			// If no children we are finished
			if(children == null)
				return;

			// Recursively serialize its children
			volume.childIDs = new int[children.Length];
			for(int i = 0; i < children.Length; i++)
			{
				var childIndex = -1;
				children[i].SerializeRecursive(ref serializableVolumes, volumeID, out childIndex);
				volume.childIDs[i] = childIndex;
			}
		}
	}
}
