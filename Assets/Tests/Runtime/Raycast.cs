using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/Raycast")]
	public class Raycast : MonoBehaviour, IBake
	{	
        [SerializeField] private int m_RayDensity = 10;
        [SerializeField] private SerializableVolume[] m_SerializableVolumes;
        [SerializeField] private MeshRenderer[] m_PassedRenderers;
        [SerializeField] private RayDebug[] m_Rays;

		[SerializeField] private BakeState m_BakeState;
		public BakeState bakeState
		{
			get { return m_BakeState; }
		}

		[SerializeField] private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
		}

#if UNITY_EDITOR
		public void OnClickBake()
		{
            var passedRenderers = new List<MeshRenderer>();
            var rays =  new List<RayDebug>();
			m_Completion = 0.0f;
			m_BakeState = BakeState.Volumes;
            var staticRenderers = PortalPrepareUtil.GetStaticOccludeeRenderers();
			m_SerializableVolumes = PortalPrepareUtil.GetVolumeData(VolumeMode.Auto, 0);

            m_BakeState = BakeState.Visibility;
            var rayCount = PortalVisibilityUtil.CalculateRayCount(m_RayDensity, m_SerializableVolumes.Length);
            for(int r = 0; r < rayCount; r++)
            {
                var rayPosition = PortalVisibilityUtil.RandomPointWithinVolume(m_SerializableVolumes[0]);
                var rayDirection = PortalVisibilityUtil.RandomSphericalDistributionVector();
                rays.Add(new RayDebug(rayPosition, rayDirection));

                foreach(MeshRenderer renderer in staticRenderers)
                {
                    if(PortalVisibilityUtil.CheckAABBIntersection(rayPosition, rayDirection, renderer.bounds))
                        passedRenderers.AddIfUnique(renderer);
                }
            }
            m_PassedRenderers = passedRenderers.ToArray();
            m_Rays = rays.ToArray();
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		public void OnClickCancel()
		{
			m_SerializableVolumes = null;
            m_PassedRenderers = null;
			m_BakeState = BakeState.Empty;
			m_Completion = 0.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

        private void OnDrawGizmos()
        {
			if(m_SerializableVolumes == null || m_PassedRenderers == null || m_Rays == null)
				return;

            foreach(MeshRenderer renderer in m_PassedRenderers)
				PortalDebugUtil.DrawMesh(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale,
					renderer.GetComponent<MeshFilter>().sharedMesh, PortalDebugColors.white);

			foreach(SerializableVolume volume in PortalPrepareUtil.FilterVolumeDataNoChildren(m_SerializableVolumes))
				PortalDebugUtil.DrawCube(volume.positionWS, volume.rotationWS, volume.scaleWS, PortalDebugColors.volume);

            foreach(RayDebug ray in m_Rays)
            {
                PortalDebugUtil.DrawRay(ray.position, ray.direction, 10, PortalDebugColors.raycast);
                PortalDebugUtil.DrawSphere(ray.position, 0.1f, PortalDebugColors.raycast);
            }
        }

        [Serializable]
        public class RayDebug
        {
            public RayDebug(Vector3 position, Vector3 direction)
            {
                this.position = position;
                this.direction = direction;
            }

            public Vector3 position;
            public Vector3 direction;
        }
#endif
	}
}
