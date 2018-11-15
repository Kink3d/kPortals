using System;
using System.Collections.Generic;
using System.Linq;
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

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Get all renderers from the VisibilityTable
        /// </summary>
		public Dictionary<int, MeshRenderer> GetAllRenderers()
		{
			// Get lowest subdivision volumes
			var lowestSubdivisionVolumes = PortalPrepareUtil.FilterVolumeDataNoChildren(volumes);
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			foreach (SerializableVolume volume in lowestSubdivisionVolumes)
			{
				// Iterate all volumes
				GameObject[] volumeObjects;
				if(!visibilityTable.TryGetObjects(volume, out volumeObjects))
				{
					// Get objects from VisbilityTable and collect renderers
					foreach(GameObject go in volumeObjects)
						renderers.Add(go.GetComponent<MeshRenderer>());
				}
			}
			// Store as dictionary with instanceID
            return renderers.Distinct().ToDictionary(s => s.GetInstanceID());
		}
	}
}
