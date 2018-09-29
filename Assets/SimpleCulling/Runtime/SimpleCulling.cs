using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling
{
	public class SimpleCulling : MonoBehaviour 
	{
		public enum DebugMode { None, Occluders, Volumes }

		public enum BakeState { Empty, Occluders, Volumes, Occlusion, Active }

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
                for (int i = 0; i < m_ActiveVolume.renderers.Length; i++)
                {
                    if (!m_VisibleRenderers.Contains(m_ActiveVolume.renderers[i]))
                        m_ActiveVolume.renderers[i].enabled = true;
                }

                for (int i = 0; i < m_VisibleRenderers.Length; i++)
                {
                    if(!m_ActiveVolume.renderers.Contains(m_VisibleRenderers[i]))
                        m_VisibleRenderers[i].enabled = false;
                }

				m_VisibleRenderers = m_ActiveVolume.renderers;
			}
		}

        // ----------------------------------------------------------------------------------------------------//
        //                                              EDITOR                                                 //
        // ----------------------------------------------------------------------------------------------------//

#if UNITY_EDITOR

        // --------------------------------------------------
        // Editor State

        [SerializeField]
        private BakeState m_BakeState;
		public BakeState bakeState { get { return m_BakeState; } }
		
        [SerializeField]
		private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
			set { m_Completion = value; }
		}

        // --------------------------------------------------
        // Editor Data

        private MeshRenderer[] m_StaticRenderers;

		// --------------------------------------------------
		// Interface

		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
			EditorCoroutines.StopAllCoroutines(this);
			EditorCoroutines.StartCoroutine(Generate(), this);
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
			EditorCoroutines.StopAllCoroutines(this);
            ClearOccluderProxyGeometry();
			ClearHierarchicalVolumeGrid();
			m_BakeState = BakeState.Empty;	
			m_Completion = 0;
        }

		private IEnumerator Generate()
		{
            m_StaticRenderers = Utils.GetStaticRenderers();

			// Generate Occluder Proxy Geometry
			m_BakeState = BakeState.Occluders;
			ClearOccluderProxyGeometry();
            yield return EditorCoroutines.StartCoroutine(Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this, m_OccluderTag), this);
			
			// Generate Hierarchical Volume Grid
			m_BakeState = BakeState.Volumes;
			ClearHierarchicalVolumeGrid();
			Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
			yield return EditorCoroutines.StartCoroutine(Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, value => m_VolumeData = value, this), this);
            
			// Generate Occlusion Data
			m_BakeState = BakeState.Occlusion;
			VolumeData[] smallestVolumes = Utils.GetLowestSubdivisionVolumes(m_VolumeData, m_VolumeDensity);
			for(int i = 0; i < smallestVolumes.Length; i++)
			{
				m_Completion = (float)(i + 1) / (float)smallestVolumes.Length;
				m_ActiveVolume = smallestVolumes[i];
				yield return EditorCoroutines.StartCoroutine(Utils.BuildOcclusionForVolume(smallestVolumes[i].bounds, m_RayDensity, m_StaticRenderers, m_Occluders, value => smallestVolumes[i].renderers = value, this, m_FilterAngle), this);
			}
			
			m_BakeState = BakeState.Active;
			m_ActiveVolume = null;
			yield return null;
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
				DebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData, m_ActiveVolume);
            }
        }

#endif

    }
}