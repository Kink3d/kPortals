using System;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public class SerializablePortalData
	{
		public SerializableVolume[] volumes;
		public SerializableOccluder[] occluders;
		public List<SerializableKeyValuePair<SerializableVolume, GameObject[]>> visibilityTable;
	}

	[Serializable]
	public class SerializableKeyValuePair<TKey, TValue>
    {
		public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}
