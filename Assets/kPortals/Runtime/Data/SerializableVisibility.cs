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
	}
}
