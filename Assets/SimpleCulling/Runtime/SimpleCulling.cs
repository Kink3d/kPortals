using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	public class SimpleCulling : MonoBehaviour 
	{
		public enum DebugMode { None, Occluders, Volumes }

		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		private string m_OccluderTag = "Occluder"; // TD - Expose and add option for layer

		[SerializeField]
        private int m_VolumeDensity = 4;

		[SerializeField]
        private int m_RayDensity = 1;

		[SerializeField]
        private int m_FilterAngle = 45;

		[SerializeField]
        private DebugMode m_DebugMode = DebugMode.None;

		// ----------------------------------------------------------------------------------------------------//
		//                                              RUNTIME                                                //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		[SerializeField]
		private VolumeData m_VolumeData;

		[SerializeField]
		private OccluderData[] m_Occluders;

		private MeshRenderer[] m_VisibleRenderers;

		private VolumeData m_ActiveVolume = new VolumeData();

		private void OnEnable()
		{
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			VolumeData[] volumes = Utils.GetLowestSubdivisionVolumes(m_VolumeData, m_VolumeDensity);
			foreach (VolumeData data in volumes)
                renderers.AddRange(data.renderers);
            m_VisibleRenderers = renderers.Distinct().ToArray();
		}

		private void Update()
		{
			if(m_VolumeData == null)
				return;
			
			if(Utils.GetActiveVolumeAtPosition(m_VolumeData, Camera.main.transform.position, out m_ActiveVolume))
			{
				foreach (Renderer renderer in m_VisibleRenderers)
                renderer.enabled = false;

				m_VisibleRenderers = m_ActiveVolume.renderers;
				foreach (Renderer renderer in m_VisibleRenderers)
					renderer.enabled = true;
			}
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              EDITOR                                                 //
		// ----------------------------------------------------------------------------------------------------//

#if UNITY_EDITOR

		// --------------------------------------------------
		// Editor State

		private bool m_IsGenerating;
		public bool isGenerating { get { return m_IsGenerating; } }

		// --------------------------------------------------
		// Editor Data
        
		private MeshRenderer[] m_StaticRenderers;

		// --------------------------------------------------
		// Interface

		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
            m_IsGenerating = true;
            m_StaticRenderers = Utils.GetStaticRenderers();

			// Generate Occluder Proxy Geometry
			ClearOccluderProxyGeometry();
            m_Occluders = Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, m_OccluderTag);
			
			// Generate Hierarchical Volume Grid
			ClearHierarchicalVolumeGrid();
			Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
			m_VolumeData = Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity);

			// Generate Occlusion Data
			VolumeData[] smallestVolumes = Utils.GetLowestSubdivisionVolumes(m_VolumeData, m_VolumeDensity);
			foreach(VolumeData volume in smallestVolumes)
				volume.renderers = Utils.BuildOcclusionForVolume(volume.bounds, m_RayDensity, m_StaticRenderers, m_Occluders, m_FilterAngle);

            m_IsGenerating = false;
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            ClearOccluderProxyGeometry();
			ClearHierarchicalVolumeGrid();
        }

		// --------------------------------------------------
		// Occluder Proxy Geometry

		[ExecuteInEditMode]
		private void ClearOccluderProxyGeometry()
		{
			m_Occluders = null;
			Transform container = transform.Find(Utils.occluderContainerName);
			if(container != null)
			{
				DestroyImmediate(container.gameObject);
			}
		}

		// --------------------------------------------------
		// Hirarchical Volume Grid

		[ExecuteInEditMode]
		private void ClearHierarchicalVolumeGrid()
		{
			m_VolumeData = null;
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            if (m_DebugMode == DebugMode.Occluders)
            {
				DebugUtils.DrawOccluders(m_Occluders);
            }
			if (m_DebugMode == DebugMode.Volumes)
            {
				DebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData);
            }
        }

#endif

    }
}