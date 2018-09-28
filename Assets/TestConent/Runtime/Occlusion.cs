using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class Occlusion : MonoBehaviour 
	{

#if (UNITY_EDITOR)

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
                EditorCoroutines.StartCoroutine(Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this), this);

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
            DebugUtils.DrawRay(raySource.position, raySource.forward);
            DebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);
			DebugUtils.DrawSphere(raySource.position, 0.25f);
        }

#endif

    }
}
