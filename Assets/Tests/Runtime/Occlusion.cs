using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/Occlusion")]
	public class Occlusion : MonoBehaviour 
	{
        public Transform raySource;
		public float maxDisatnce = 10;
        
        private MeshRenderer[] m_StaticRenderers;
		private MeshRenderer[] m_PassedRenderers;
        private OccluderData[] m_Occluders;

        private void Update()
		{
            if(raySource == null)
                return;

            m_StaticRenderers = PortalUtils.GetStaticRenderers();
            List<MeshRenderer> renderers = new List<MeshRenderer>();

            if (m_Occluders == null)
                EditorCoroutines.StartCoroutine(PortalUtils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this), this);

            for (int i = 0; i < m_StaticRenderers.Length; i++)
			{
				if(PortalUtils.CheckAABBIntersection(raySource.position, raySource.forward, m_StaticRenderers[i].bounds))
                {
                    if(PortalUtils.CheckOcclusion(m_Occluders, m_StaticRenderers[i], raySource.position, raySource.forward))
                        renderers.Add(m_StaticRenderers[i]);
                }
			}
            m_PassedRenderers = renderers.ToArray();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(raySource == null || m_StaticRenderers == null || m_PassedRenderers == null)
                return;

            PortalDebugUtils.DrawRay(raySource.position, raySource.forward);
            PortalDebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);
			PortalDebugUtils.DrawSphere(raySource.position, 0.25f);
        }
#endif

    }
}
