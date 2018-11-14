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

			m_StaticRenderers = Utils.GetStaticRenderers();
			m_PassedRenderers = Utils.FilterRenderersByConeAngle(m_StaticRenderers, raySource.position, raySource.forward, maxAngle);
		}

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			if(raySource == null || m_StaticRenderers == null || m_PassedRenderers == null)
                return;

			DrawVectorDebug();
			DebugUtils.DrawRay(raySource.position, raySource.forward);
            DebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);
            DebugUtils.DrawCone(raySource.position, raySource.forward, maxAngle);
			DebugUtils.DrawSphere(raySource.position, 0.25f);
        }

		private void DrawVectorDebug()
        {
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
