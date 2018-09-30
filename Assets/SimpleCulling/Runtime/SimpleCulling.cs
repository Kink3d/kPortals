using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling
{
	public class SimpleCulling : MonoBehaviour 
	{
		public enum VolumeMode { Automatic, Manual }

		public enum DebugMode { None, Occluders, Volumes }

		public enum BakeState { Empty, Occluders, Volumes, Occlusion, Active }

		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		private string m_OccluderTag = "Occluder"; // TODO - Expose and add option for layer

		[SerializeField]
        private VolumeMode m_VolumeMode = VolumeMode.Automatic;

		[SerializeField]
        private BoxCollider[] m_ManualVolumes;

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

		private Dictionary<int, MeshRenderer> m_VisibleRenderers = new Dictionary<int, MeshRenderer>();

		private VolumeData m_ActiveVolume;
		private VolumeData m_PreviousVolume;

		private void OnEnable()
		{
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			int volumeDensity = m_VolumeMode == VolumeMode.Automatic ? m_VolumeDensity : 1;
			VolumeData[] volumes = Utils.GetLowestSubdivisionVolumes(m_VolumeData, volumeDensity);
			foreach (VolumeData data in volumes)
                renderers.AddRange(data.renderers);
            m_VisibleRenderers = renderers.Distinct().ToDictionary(s => s.GetInstanceID());

			Utils.GetActiveVolumeAtPosition(m_VolumeData, Camera.main.transform.position, out m_ActiveVolume);
		}

		private void Update()
		{
			if(m_VolumeData == null)
				return;
			
			if(Utils.GetActiveVolumeAtPosition(m_VolumeData, Camera.main.transform.position, out m_ActiveVolume))
			{
				if(m_ActiveVolume != m_PreviousVolume) // TODO - Disable for runtime optimisation
				{
					m_PreviousVolume = m_ActiveVolume;
					UpdateOcclusion();
				}	
			}
		}

		private void UpdateOcclusion()
		{
			if(Utils.GetActiveVolumeAtPosition(m_VolumeData, Camera.main.transform.position, out m_ActiveVolume))
			{
				List<MeshRenderer> visibleRenderers = m_VisibleRenderers.Values.ToList();
				for (int i = 0; i < visibleRenderers.Count; i++)
                {
                    if (!m_ActiveVolume.renderers.Contains(visibleRenderers[i])) // TODO - Optimise this
                        visibleRenderers[i].enabled = false;
                }

				MeshRenderer renderer;
				int[] IDs = new int[m_ActiveVolume.renderers.Length];
				for (int i = 0; i < m_ActiveVolume.renderers.Length; i++)
                {
					IDs[i] = m_ActiveVolume.renderers[i].GetInstanceID();
                    if (!m_VisibleRenderers.TryGetValue(m_ActiveVolume.renderers[i].gameObject.GetInstanceID(), out renderer))
                        m_ActiveVolume.renderers[i].enabled = true;
                }

				m_VisibleRenderers.Clear();
				for (int i = 0; i < m_ActiveVolume.renderers.Length; i++) // TODO - Optimise this
					m_VisibleRenderers.Add(IDs[i], m_ActiveVolume.renderers[i]);
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
			EditorCoroutines.StartCoroutine(GenerateOcclusion(), this);
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
			EditorCoroutines.StopAllCoroutines(this);
            ClearOccluderData();
			ClearVolumeData();
			m_BakeState = BakeState.Empty;	
			m_Completion = 0;
        }

		private IEnumerator GenerateOcclusion()
		{
            m_StaticRenderers = Utils.GetStaticRenderers();

			// Generate Occluder Data
			m_BakeState = BakeState.Occluders;
			ClearOccluderData();
            yield return EditorCoroutines.StartCoroutine(Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this, m_OccluderTag), this);
			
			// Generate Volume Data
			m_BakeState = BakeState.Volumes;
			ClearVolumeData();
			switch(m_VolumeMode)
			{
				case VolumeMode.Automatic:
					// Generate Hierarchical Volume Grid
					Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
					yield return EditorCoroutines.StartCoroutine(Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, value => m_VolumeData = value, this), this);
					break;
				case VolumeMode.Manual:
					// Generate Manual Volumes
					yield return EditorCoroutines.StartCoroutine(Utils.BuildManualVolumeGrid(m_ManualVolumes, value => m_VolumeData = value), this);
					break;
			}

			if(m_VolumeData != null)
			{
				// Generate Occlusion Data
				m_BakeState = BakeState.Occlusion;
				int volumeDensity = m_VolumeMode == VolumeMode.Automatic ? m_VolumeDensity : 1;
				VolumeData[] smallestVolumes = Utils.GetLowestSubdivisionVolumes(m_VolumeData, volumeDensity);
				for(int i = 0; i < smallestVolumes.Length; i++)
				{
					m_Completion = (float)(i + 1) / (float)smallestVolumes.Length;
					m_ActiveVolume = smallestVolumes[i];
					yield return EditorCoroutines.StartCoroutine(Utils.BuildOcclusionForVolume(smallestVolumes[i].bounds, m_RayDensity, m_StaticRenderers, m_Occluders, value => smallestVolumes[i].renderers = value, this, m_FilterAngle), this);
				}
				m_BakeState = BakeState.Active;
			}
			else
			{
				Debug.LogError("Occlusion Bake Failed. Check Settings.");
				m_BakeState = BakeState.Empty;	
				m_Completion = 0;
			}
			m_ActiveVolume = null;
			yield return null;
        }

        // --------------------------------------------------
        // Occluder Proxy Geometry

        [ExecuteInEditMode]
		private void ClearOccluderData()
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
		private void ClearVolumeData()
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