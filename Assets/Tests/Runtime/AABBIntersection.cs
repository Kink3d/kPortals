using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/AABBIntersection")]
	public class AABBIntersection : MonoBehaviour 
	{	
		public Transform raySource;

        private MeshRenderer[] m_PassedRenderers;

		private void Update()
		{
            if(raySource == null)
                return;

            var staticRenderers = PortalPrepareUtil.GetStaticOccludeeRenderers();
            List<MeshRenderer> renderers = new List<MeshRenderer>();
			for(int i = 0; i < staticRenderers.Length; i++)
			{
                if (PortalVisibilityUtil.CheckAABBIntersection(raySource.position, raySource.forward, staticRenderers[i].bounds))
                    renderers.Add(staticRenderers[i]);
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
