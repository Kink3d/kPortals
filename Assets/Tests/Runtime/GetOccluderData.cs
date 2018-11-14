using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/GetOccluderData")]
	public class GetOccluderData : MonoBehaviour, IBake 
	{	
		private SerializableOccluder[] m_SerializableOccluders;

		private BakeState m_BakeState;
		public BakeState bakeState
		{
			get { return m_BakeState; }
		}

		private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
		}

		public void OnClickBake()
		{
			m_Completion = 0.0f;
			m_BakeState = BakeState.Occluders;
			m_SerializableOccluders = PortalUtil.GetOccluderData();
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
		}

		public void OnClickCancel()
		{
			m_SerializableOccluders = null;
			m_BakeState = BakeState.Empty;
			m_Completion = 0.0f;
		}

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			if(m_SerializableOccluders == null)
				return;

			foreach(SerializableOccluder occluder in m_SerializableOccluders)
			{
				PortalDebugUtil.DrawMesh(occluder.positionWS, occluder.rotationWS, occluder.scaleWS, 
					occluder.mesh, PortalDebugColors.occluder);
			}
        }
#endif
	}
}
