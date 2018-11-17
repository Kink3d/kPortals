using System.Linq;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/ConeAngle")]
	public class ConeAngle : MonoBehaviour 
	{	
		public Transform raySource;
        public float maxAngle = 45;

        private MeshRenderer[] m_StaticRenderers;
        private MeshRenderer[] m_PassedRenderers;

		private void Update()
		{
            if(raySource == null)
                return;

            m_StaticRenderers = PortalPrepareUtil.GetStaticOccludeeRenderers();
            m_PassedRenderers = PortalVisibilityUtil.FilterRenderersByConeAngle(m_StaticRenderers, raySource.position, raySource.forward, maxAngle);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			if(raySource == null || m_PassedRenderers == null)
                return;

            PortalDebugUtil.DrawRay(raySource.position, raySource.forward, 10, PortalDebugColors.raycast);
			PortalDebugUtil.DrawSphere(raySource.position, 0.25f, PortalDebugColors.raycast);
            PortalDebugUtil.DrawCone(raySource.position, raySource.forward, 10, maxAngle, PortalDebugColors.black);

            foreach(MeshRenderer renderer in m_StaticRenderers)
            {
                bool isPassed = m_PassedRenderers.Contains(renderer);
                PortalDebugUtil.DrawLine(raySource.position, renderer.bounds.center, isPassed ? PortalDebugColors.white : PortalDebugColors.black);
            }

			foreach(MeshRenderer renderer in m_PassedRenderers)
				PortalDebugUtil.DrawMesh(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale,
					renderer.GetComponent<MeshFilter>().sharedMesh, PortalDebugColors.white);
        }
#endif
	}
}
