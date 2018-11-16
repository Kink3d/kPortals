using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/Occlusion")]
	public class Occlusion : MonoBehaviour 
	{	
		public Transform raySource;

        [SerializeField] private MeshRenderer[] m_PassedRenderers;
        [SerializeField] private SerializableOccluder m_Occluder;
        [SerializeField] private MeshCollider[] m_OccluderProxies;

		private void Update()
		{
            if(raySource == null)
                return;

            if(m_OccluderProxies == null || m_OccluderProxies.Length == 0)
            {
                var occluders = PortalPrepareUtil.GetOccluderData();
			    m_OccluderProxies = PortalPrepareUtil.GetOccluderProxies(occluders);
                foreach(Collider col in m_OccluderProxies)
                    col.transform.SetParent(transform);
            }

            var staticRenderers = PortalPrepareUtil.GetStaticOccludeeRenderers();
            List<MeshRenderer> renderers = new List<MeshRenderer>();
			for(int i = 0; i < staticRenderers.Length; i++)
			{
                if (PortalVisibilityUtil.CheckAABBIntersection(raySource.position, raySource.forward, staticRenderers[i].bounds))
                {
                    if(PortalVisibilityUtil.CheckOcclusion(m_OccluderProxies, staticRenderers[i], raySource.position, raySource.forward))
                        renderers.AddIfUnique(staticRenderers[i]);
                }
			}
            m_PassedRenderers = renderers.ToArray();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			if(raySource == null || m_PassedRenderers == null)
                return;

            PortalDebugUtil.DrawRay(raySource.position, raySource.forward, 10, PortalDebugColors.raycast);
			PortalDebugUtil.DrawSphere(raySource.position, 0.25f, PortalDebugColors.raycast);

			foreach(MeshRenderer renderer in m_PassedRenderers)
				PortalDebugUtil.DrawMesh(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale,
					renderer.GetComponent<MeshFilter>().sharedMesh, PortalDebugColors.white);
        }
#endif
	}
}
