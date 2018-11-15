using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/GetOccluderProxies")]
	public class GetOccluderProxies : MonoBehaviour, IBake 
	{	
		[SerializeField] private SerializableOccluder[] m_SerializableOccluders;
		[SerializeField] private Collider[] m_OccluderProxies;

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
			ClearOccluderProxies();
			m_Completion = 0.0f;
			m_BakeState = BakeState.Occluders;
			m_SerializableOccluders = PortalPrepareUtil.GetOccluderData();
			m_OccluderProxies = PortalPrepareUtil.GetOccluderProxies(m_SerializableOccluders);
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		public void OnClickCancel()
		{
			ClearOccluderProxies();
			m_SerializableOccluders = null;
			m_BakeState = BakeState.Empty;
			m_Completion = 0.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		private void ClearOccluderProxies()
		{
			if(m_OccluderProxies != null)
			{
				for(int i = 0; i < m_OccluderProxies.Length; i++)
					PortalCoreUtil.Destroy(m_OccluderProxies[i].gameObject);
				m_OccluderProxies = null;
			}
		}

        private void OnDrawGizmos()
        {
			if(m_OccluderProxies == null)
				return;

			foreach(MeshCollider proxy in m_OccluderProxies)
			{
				var transform = proxy.transform;
				PortalDebugUtil.DrawMesh(transform.position, transform.rotation, transform.lossyScale, 
					proxy.sharedMesh, PortalDebugColors.occluder);
			}
        }
#endif
	}
}
