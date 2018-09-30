using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class ConeAngle : MonoBehaviour 
	{

#if UNITY_EDITOR

        // ----------------------------------------------------------------------------------------------------//
        //                                           PUBLIC FIELDS                                             //
        // ----------------------------------------------------------------------------------------------------//

        public Transform raySource;
		public float maxAngle;

		// ----------------------------------------------------------------------------------------------------//
		//                                               TEST                                                  //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		[SerializeField]
		private MeshRenderer[] m_StaticRenderers;
		
		[SerializeField]
		private MeshRenderer[] m_PassedRenderers;

		// --------------------------------------------------
		// Test Execution

		private void Update()
		{
			m_StaticRenderers = Utils.GetStaticRenderers();
			m_PassedRenderers = Utils.FilterRenderersByConeAngle(m_StaticRenderers, raySource.position, raySource.forward, maxAngle);
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
			DrawVectorDebug();
			DebugUtils.DrawRay(raySource.position, raySource.forward);
            DebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);
            DebugUtils.DrawCone(raySource.position, raySource.forward, maxAngle);
			DebugUtils.DrawSphere(raySource.position, 0.25f);
        }

		private void DrawVectorDebug()
        {
            if (m_StaticRenderers == null || m_PassedRenderers == null)
                return;

            for (int i = 0; i < m_StaticRenderers.Length; i++)
            {
                bool isPassed = m_PassedRenderers.Contains(m_StaticRenderers[i]);
                Gizmos.color = isPassed ? DebugColors.occludeePass[0] : DebugColors.occludeeFail[0];
                Gizmos.DrawLine(raySource.position, m_StaticRenderers[i].bounds.center);
            }
		}

#endif

    }
}
