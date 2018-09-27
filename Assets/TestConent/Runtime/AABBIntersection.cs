using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class AABBIntersection : MonoBehaviour 
	{
		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		public Transform raySource;

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
            List<MeshRenderer> renderers = new List<MeshRenderer>();

			for(int i = 0; i < m_StaticRenderers.Length; i++)
			{
                if (Utils.CheckAABBIntersection(raySource.position, raySource.forward, m_StaticRenderers[i].bounds))
                    renderers.Add(m_StaticRenderers[i]);
			}
            m_PassedRenderers = renderers.ToArray();
        }

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DebugUtils.DrawRayDebug(raySource.position, raySource.forward);
            DebugUtils.DrawRendererDebug(m_StaticRenderers, m_PassedRenderers);
        }
    }
}
