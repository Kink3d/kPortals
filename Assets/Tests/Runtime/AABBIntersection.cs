using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/AABBIntersection")]
	public class AABBIntersection : MonoBehaviour 
	{
        public Transform raySource;

        private MeshRenderer[] m_StaticRenderers;
        private MeshRenderer[] m_PassedRenderers;

		private void Update()
		{
            if(raySource == null)
                return;

            m_StaticRenderers = Utils.GetStaticRenderers();
            List<MeshRenderer> renderers = new List<MeshRenderer>();
			for(int i = 0; i < m_StaticRenderers.Length; i++)
			{
                if (Utils.CheckAABBIntersection(raySource.position, raySource.forward, m_StaticRenderers[i].bounds))
                    renderers.Add(m_StaticRenderers[i]);
			}
            m_PassedRenderers = renderers.ToArray();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(raySource == null || m_StaticRenderers == null || m_PassedRenderers == null)
                return;

            DebugUtils.DrawRay(raySource.position, raySource.forward);
            DebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);
			DebugUtils.DrawSphere(raySource.position, 0.25f);
        }
#endif

    }
}
