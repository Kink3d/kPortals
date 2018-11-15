using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/Portal System")]
	public class PortalSystem : MonoBehaviour, IBake
	{
		// -------------------------------------------------- //
        //                     SINGELTON                      //
        // -------------------------------------------------- //

        private static PortalSystem _Instance;
        public static PortalSystem Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = FindObjectOfType<PortalSystem>();
                return _Instance;
            }
        }

		// -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //

		[SerializeField] private VolumeMode m_VolumeMode;
		[SerializeField] private int m_Subdivisions;
		[SerializeField] private int m_RayDensity = 10;
		[SerializeField] private float m_ConeAngle = 45.0f;
		[SerializeField] private SerializablePortalData m_PortalData;

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

		private bool isInitialized = false;
		private Dictionary<int, MeshRenderer> m_VisibleRenderers = new Dictionary<int, MeshRenderer>();
		private List<PortalAgent> m_ActiveAgents = new List<PortalAgent>();

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Register a PortalAgent for calculating visibility.
        /// </summary>
        /// <param name="agent">PortalAgent to add.</param>
		public void RegisterAgent(PortalAgent agent)
		{
			if(!m_ActiveAgents.Contains(agent))
				m_ActiveAgents.Add(agent);
		}

		/// <summary>
        /// Unregister a PortalAgent from calculating visibility.
        /// </summary>
        /// <param name="agent">PortalAgent to remove.</param>
		public void UnregisterAgent(PortalAgent agent)
		{
			if(m_ActiveAgents.Contains(agent))
				m_ActiveAgents.Remove(agent);
		}

#if UNITY_EDITOR
		/// <summary>
        /// Start the bake process. Editor only.
        /// </summary>
		public void OnClickBake()
		{
			EditorCoroutines.StartCoroutine(BakePortalData(), this);
		}
		
		/// <summary>
        /// Cancel any active bake and clear all data. Editor only.
        /// </summary>
		public void OnClickCancel()
		{
			// Reset all
			EditorCoroutines.StopAllCoroutines(this);
			m_BakeState = BakeState.Empty;	
			m_PortalData = null;
			m_Completion = 0;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}
#endif

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

		private void OnEnable()
		{
			// TODO
			// - Rewrite this section

			// Runtime only
			if(!Application.isPlaying)
				return;

			// If not active data dont calculate visibility at runtime
			if(m_BakeState != BakeState.Active)
				return;

			// Set renderers from all Volumes as visible
            m_VisibleRenderers = m_PortalData.GetAllRenderers();

			// Init per frame visibility calculations
			isInitialized = true;
		}

		private void Update()
		{
			// TODO
			// - Rewrite this section
			
			// Runtime only
			if(!Application.isPlaying)
				return;

			// If not initialized wait
			if(!isInitialized)
				return;

			// Try to update visibility
			UpdateActiveVolume();
		}

		private void UpdateActiveVolume()
		{
			// TODO
			// - Rewrite this section

			// Get active volume
			int volumeIndex = -1;
			for(int i = 0; i < m_PortalData.volumes.Length; i++)
			{
				var volume =m_PortalData.volumes[i];
				var bounds = new Bounds(volume.positionWS, volume.scaleWS);
				if(bounds.Contains(m_ActiveAgents[0].transform.position))
					volumeIndex = i;
			}

			// If active volume has changed update occlusion
			if(volumeIndex != m_ActiveAgents[0].activeVolumeID)
			{
				m_ActiveAgents[0].activeVolumeID = volumeIndex;
				UpdateOcclusion();
			}
		}

		private void UpdateOcclusion()
		{
			// TODO
			// - Rewrite this section for optimisation
			if(m_ActiveAgents[0].activeVolumeID == -1)
				return;

			// Get renderers from active volume
			SerializableVolume activeVolume = m_PortalData.volumes[m_ActiveAgents[0].activeVolumeID];
			MeshRenderer[] activeVolumeRenderers;
			m_PortalData.visibilityTable.TryGetRemderers(activeVolume, out activeVolumeRenderers);

			// Disable all visible renderers not in this active Volume
			List<MeshRenderer> visibleRenderers = m_VisibleRenderers.Values.ToList();
			for (int i = 0; i < visibleRenderers.Count; i++)
			{
				if (!activeVolumeRenderers.Contains(visibleRenderers[i]))
					visibleRenderers[i].enabled = false;
			}

			// Enable all Volumes in this active Volume that arent current visible
			MeshRenderer renderer;
			int[] IDs = new int[activeVolumeRenderers.Length];
			for (int i = 0; i < activeVolumeRenderers.Length; i++)
			{
				IDs[i] = activeVolumeRenderers[i].GetInstanceID();
				if (!m_VisibleRenderers.TryGetValue(activeVolumeRenderers[i].gameObject.GetInstanceID(), out renderer))
					activeVolumeRenderers[i].enabled = true;
			}

			// Update visible renderers to match active Volume
			m_VisibleRenderers.Clear();
			for (int i = 0; i < activeVolumeRenderers.Length; i++)
				m_VisibleRenderers.Add(IDs[i], activeVolumeRenderers[i]);
		}

#if UNITY_EDITOR
		private IEnumerator BakePortalData()
		{
			// Generate Portal data
			m_Completion = 0.0f;
			m_BakeState = BakeState.Occluders;
			var occluders = PortalPrepareUtil.GetOccluderData();
			m_BakeState = BakeState.Volumes;
			var volumes = PortalPrepareUtil.GetVolumeData(m_VolumeMode, m_Subdivisions);
			m_BakeState = BakeState.Visibility;
			List<VisbilityData> visibilityTable = null;
			yield return EditorCoroutines.StartCoroutine(GenerateVisibilityTable(occluders, volumes, value => visibilityTable = value), this);

			// Serialize
			m_PortalData = new SerializablePortalData()
			{
				occluders = occluders,
				volumes = volumes,
				visibilityTable = visibilityTable
			};

			// Finalize
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		private IEnumerator GenerateVisibilityTable(SerializableOccluder[] occluders, SerializableVolume[] volumes, Action<List<VisbilityData>> result)
		{
			// Abort bake if input data is invalid
			if(occluders == null || volumes == null)
			{
				Debug.LogError("Bake prepare data is invalid. Check bake output and retry.");
				m_BakeState = BakeState.Empty;	
				m_Completion = 0;
				yield return null;
			}

			// Setup
			m_BakeState = BakeState.Visibility;
			var visibilityTable = new List<VisbilityData>();

			// Get Occluder proxies
			var occluderProxies = PortalPrepareUtil.GetOccluderProxies(occluders);

			// Get lowest subdivision volumes
			var lowestSubdivisionVolumes = PortalPrepareUtil.FilterVolumeDataNoChildren(volumes);

			// Get occludees
			var occludees = PortalPrepareUtil.GetStaticOccludeeRenderers();

			// Build Visibility for Volumes
			for(int v = 0; v < lowestSubdivisionVolumes.Length; v++)
			{
				// Setup
				m_Completion = (float)(v + 1) / (float)lowestSubdivisionVolumes.Length;
				var volume = lowestSubdivisionVolumes[v];
				var passedObjects = new List<GameObject>();

				// Iterate random rays based on volume density
				var rayCount = PortalVisibilityUtil.CalculateRayCount(m_RayDensity, lowestSubdivisionVolumes.Length);
				for(int r = 0; r < rayCount; r++)
				{
					// Get a random ray and a list of cone filtered renderers to test
					var rayPosition = PortalVisibilityUtil.RandomPointWithinVolume(volume);
					var rayDirection = PortalVisibilityUtil.RandomSphericalDistributionVector();
					var filteredOccludees = PortalVisibilityUtil.FilterRenderersByConeAngle(occludees, rayPosition, rayDirection, m_ConeAngle);
					for(int f = 0; f < filteredOccludees.Length; f++)
					{
						// Test ray against renderer AABB and occluders
						if(PortalVisibilityUtil.CheckAABBIntersection(rayPosition, rayDirection, filteredOccludees[f].bounds))
						{
							if(PortalVisibilityUtil.CheckOcclusion(occluderProxies, filteredOccludees[f], rayPosition, rayDirection))
								passedObjects.AddIfUnique(filteredOccludees[f].gameObject);
						}
					}
				}

				// Add to VisibilityTable
				visibilityTable.Add(new VisbilityData(volume, passedObjects.ToArray()));
				yield return null;
			}

			// Clear Occluder proxies
			for(int i = 0; i < occluderProxies.Length; i++)
				PortalCoreUtil.Destroy(occluderProxies[i].gameObject);

			// Finalize
			result(visibilityTable);
		}

        private void OnDrawGizmos()
        {
			if(m_BakeState != BakeState.Active)
				return;

			foreach(SerializableOccluder occluder in m_PortalData.occluders)
				PortalDebugUtil.DrawMesh(occluder.positionWS, occluder.rotationWS, occluder.scaleWS, 
					occluder.mesh, PortalDebugColors.occluder);

			foreach(SerializableVolume volume in PortalPrepareUtil.FilterVolumeDataNoChildren(m_PortalData.volumes))
				PortalDebugUtil.DrawCube(volume.positionWS, volume.rotationWS, volume.scaleWS, PortalDebugColors.volume);
        }
#endif
	}
}
