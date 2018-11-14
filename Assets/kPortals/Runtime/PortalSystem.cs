using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.PortalsOld
{
    [AddComponentMenu("kTools/Portals/Old/PortalSystem")]
	public class PortalSystem : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                        ENUM                        //
        // -------------------------------------------------- //

		public enum VolumeMode { Automatic, Manual }
		public enum DebugMode { None, Occluders, Volumes }
		public enum BakeState { Empty, Occluders, Volumes, Occlusion, Active }

		// -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //

		// System Parameters

		// TODO
		// - Revisit Volumes as scripted
		// - Change occluder tag > static flag
		[SerializeField] private string m_OccluderTag = "Occluder";
		[SerializeField] private VolumeMode m_VolumeMode = VolumeMode.Automatic;
		[SerializeField] private BoxCollider[] m_ManualVolumes;
		[SerializeField] private int m_VolumeDensity = 4;
		[SerializeField] private int m_RayDensity = 1;
		[SerializeField] private int m_FilterAngle = 45;
		[SerializeField] private DebugMode m_DebugMode = DebugMode.None;

		// Serialized Data

		// TODO
		// - Make VolumeData serializable
		// - Revisit OccluderData for hand placed Occluders
		[SerializeField] private VolumeData m_VolumeData;
		[SerializeField] private OccluderData[] m_Occluders;

		private Dictionary<int, MeshRenderer> m_VisibleRenderers = new Dictionary<int, MeshRenderer>();
		private VolumeData m_ActiveVolume;
		private VolumeData m_PreviousVolume;

		[SerializeField] private BakeState m_BakeState;
		public BakeState bakeState 
		{ 
			get { return m_BakeState; } 
		}
		
        [SerializeField] private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
			set { m_Completion = value; }
		}

		// -------------------------------------------------- //
        //                ENGINE LOOP METHODS                 //
        // -------------------------------------------------- //

		private void OnEnable()
		{
			// Get Volumes containing renderers
			int volumeDensity = m_VolumeMode == VolumeMode.Automatic ? m_VolumeDensity : 1;
			VolumeData[] volumes = PortalUtils.GetLowestSubdivisionVolumes(m_VolumeData, volumeDensity);
			
			// Set renderers from all Volumes as visible
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			foreach (VolumeData data in volumes)
                renderers.AddRange(data.renderers);
            m_VisibleRenderers = renderers.Distinct().ToDictionary(s => s.GetInstanceID());
		}

		private void Update()
		{
			// If Volume data isnt initialized correctly
			if(m_VolumeData == null)
				return;

			// Update active volume
			UpdateActiveVolume();			
		}

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

#if UNITY_EDITOR
		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
			// Start Occlusion generation
			EditorCoroutines.StopAllCoroutines(this);
			EditorCoroutines.StartCoroutine(GenerateOcclusion(), this);
        }

		[ExecuteInEditMode]
        public void OnClickCancel()
        {
			// Clear and reset all data
			EditorCoroutines.StopAllCoroutines(this);
			m_VolumeData = null;
			m_BakeState = BakeState.Empty;	
			m_Completion = 0;
			ClearOccluderData();
        }
#endif

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

		private void UpdateActiveVolume()
		{
			// TODO
			// - Add manual target definition
			
			// If active Volume has changed update Occlusion
			if(PortalUtils.GetActiveVolumeAtPosition(m_VolumeData, Camera.main.transform.position, out m_ActiveVolume))
			{
				if(m_ActiveVolume != m_PreviousVolume) 
				{
					m_PreviousVolume = m_ActiveVolume;
					UpdateOcclusion();
				}	
			}
		}

		private void UpdateOcclusion()
		{
			// TODO
			// - Rewrite this section for optimisation

			// Disable all visible renderers not in this active Volume
			List<MeshRenderer> visibleRenderers = m_VisibleRenderers.Values.ToList();
			for (int i = 0; i < visibleRenderers.Count; i++)
			{
				if (!m_ActiveVolume.renderers.Contains(visibleRenderers[i]))
					visibleRenderers[i].enabled = false;
			}

			// Enable all Volumes in this active Volume that arent current visible
			MeshRenderer renderer;
			int[] IDs = new int[m_ActiveVolume.renderers.Length];
			for (int i = 0; i < m_ActiveVolume.renderers.Length; i++)
			{
				IDs[i] = m_ActiveVolume.renderers[i].GetInstanceID();
				if (!m_VisibleRenderers.TryGetValue(m_ActiveVolume.renderers[i].gameObject.GetInstanceID(), out renderer))
					m_ActiveVolume.renderers[i].enabled = true;
			}

			// Update visible renderers to match active Volume
			m_VisibleRenderers.Clear();
			for (int i = 0; i < m_ActiveVolume.renderers.Length; i++)
				m_VisibleRenderers.Add(IDs[i], m_ActiveVolume.renderers[i]);
		}

#if UNITY_EDITOR
		[ExecuteInEditMode]
		private IEnumerator GenerateOcclusion()
		{
			// Get all static renderers
            var staticRenderers = PortalUtils.GetStaticRenderers();

			// Generate Occluder Data
			m_BakeState = BakeState.Occluders;
			ClearOccluderData();
            yield return EditorCoroutines.StartCoroutine(PortalUtils.BuildOccluderProxyGeometry(transform, staticRenderers, 
				value => m_Occluders = value, this, m_OccluderTag), this);
			
			// Generate Volume Data
			m_BakeState = BakeState.Volumes;
			m_VolumeData = null;
			switch(m_VolumeMode)
			{
				case VolumeMode.Automatic:
					// Generate Hierarchical Volume Grid
					Bounds bounds = PortalUtils.GetSceneBounds(staticRenderers);
					yield return EditorCoroutines.StartCoroutine(PortalUtils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, 
						value => m_VolumeData = value, this), this);
					break;
				case VolumeMode.Manual:
					// Generate Manual Volumes
					yield return EditorCoroutines.StartCoroutine(PortalUtils.BuildManualVolumeGrid(m_ManualVolumes, 
						value => m_VolumeData = value), this);
					break;
			}

			// If bake failed abort
			if(m_VolumeData == null)
			{
				Debug.LogError("Occlusion Bake Failed. Check Settings.");
				m_BakeState = BakeState.Empty;	
				m_Completion = 0;
				yield return null;
			}

			// Generate Occlusion Data
			m_BakeState = BakeState.Occlusion;
			int volumeDensity = m_VolumeMode == VolumeMode.Automatic ? m_VolumeDensity : 1;
			VolumeData[] smallestVolumes = PortalUtils.GetLowestSubdivisionVolumes(m_VolumeData, volumeDensity);
			for(int i = 0; i < smallestVolumes.Length; i++)
			{
				m_Completion = (float)(i + 1) / (float)smallestVolumes.Length;
				m_ActiveVolume = smallestVolumes[i];
				yield return EditorCoroutines.StartCoroutine(PortalUtils.BuildOcclusionForVolume(smallestVolumes[i].bounds, m_RayDensity, 
					staticRenderers, m_Occluders, value => smallestVolumes[i].renderers = value, this, m_FilterAngle), this);
			}

			// Finalize
			m_ActiveVolume = null;
			m_BakeState = BakeState.Active;
			yield return null;
        }

		[ExecuteInEditMode]
		private void ClearOccluderData()
		{
			// Clear Occluders
			m_Occluders = null;

			// Delete all Occluder proxy geometry
			var container = transform.Find(PortalUtils.occluderContainerName);
			if(container != null)
				DestroyImmediate(container.gameObject);
		}
#endif

		// -------------------------------------------------- //
        //                       GIZMOS                       //
        // -------------------------------------------------- //

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			if(m_Occluders == null || m_VolumeData == null)
				return;

            if (m_DebugMode == DebugMode.Occluders)
				PortalDebugUtils.DrawOccluders(m_Occluders);
			if (m_DebugMode == DebugMode.Volumes)
				PortalDebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData, m_ActiveVolume);
        }
#endif

    }
}