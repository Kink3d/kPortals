using System;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals
{
	[Serializable]
	public class VisbilityData
	{
		// -------------------------------------------------- //
		//                     CONSTRUCTOR                    //
		// -------------------------------------------------- //

		public VisbilityData(SerializableVolume volume, GameObject[] objects, Debug[] debugData)
		{
			this.volume = volume;
			this.objects = objects;
			this.debugData = debugData;
		}

		// -------------------------------------------------- //
		//                    PUBLIC FIELDS                   //
		// -------------------------------------------------- //

		public SerializableVolume volume;
		public GameObject[] objects;
		public Debug[] debugData;

		private MeshRenderer[] m_Renderers;
		public MeshRenderer[] renderers
		{
			get
			{
				if(m_Renderers == null || m_Renderers.Length == 0)
				{
					m_Renderers = new MeshRenderer[objects.Length];
					for(int i = 0; i < m_Renderers.Length; i++)
						m_Renderers[i] = objects[i].GetComponent<MeshRenderer>();
				}
				return m_Renderers;
			}
		}

		// -------------------------------------------------- //
		//                    DATA CLASSES                    //
		// -------------------------------------------------- //

		[Serializable]
        public struct Debug
        {
            public Vector3 source;
            public Vector3 hit;
            public Vector3 direction;
            public float distance;
            public bool isHit;
        }
	}
}
