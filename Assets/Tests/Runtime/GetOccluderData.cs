using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/GetOccluderData")]
	public class GetOccluderData : MonoBehaviour, IBake 
	{	
		[SerializeField] private SerializableOccluder[] m_SerializableOccluders;

		[SerializeField] private BakeState m_BakeState;
		public BakeState bakeState
		{
			get { return m_BakeState; }
		}

		[SerializeField] private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
		}

#if UNITY_EDITOR
		public void OnClickBake()
		{
			m_Completion = 0.0f;
			m_BakeState = BakeState.Occluders;
			m_SerializableOccluders = PortalPrepareUtil.GetOccluderData();
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		public void OnClickCancel()
		{
			m_SerializableOccluders = null;
			m_BakeState = BakeState.Empty;
			m_Completion = 0.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

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
