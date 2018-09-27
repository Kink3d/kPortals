using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class Occlusion : MonoBehaviour 
	{
		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		public Transform raySource;
		public float maxDisatnce = 10;

        // ----------------------------------------------------------------------------------------------------//
        //                                               TEST                                                  //
        // ----------------------------------------------------------------------------------------------------//

        // --------------------------------------------------
        // Runtime Data

        [SerializeField]
        private MeshRenderer[] m_StaticRenderers;

        [SerializeField]
		private MeshRenderer[] m_PassedRenderers;

        [SerializeField]
        private OccluderData[] m_Occluders;

        // --------------------------------------------------
        // Test Execution

        private void Update()
		{
            m_StaticRenderers = Utils.GetStaticRenderers();
            List<MeshRenderer> renderers = new List<MeshRenderer>();

            if (m_Occluders == null)
                m_Occluders = Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers);

            for (int i = 0; i < m_StaticRenderers.Length; i++)
			{
				if(Utils.CheckAABBIntersection(raySource.position, raySource.forward, m_StaticRenderers[i].bounds))
                {
                    if(Utils.CheckOcclusion(m_Occluders, m_StaticRenderers[i], raySource.position, raySource.forward))
                        renderers.Add(m_StaticRenderers[i]);
                }
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
