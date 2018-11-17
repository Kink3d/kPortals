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

		// Parameters
		[SerializeField] private VolumeMode m_VolumeMode;
		[SerializeField] private int m_Subdivisions;
		[SerializeField] private int m_RayDensity = 10;
		[SerializeField] private float m_ConeAngle = 45.0f;
		[SerializeField] private bool m_DrawOccluders = false;
		[SerializeField] private bool m_DrawVolumes = false;
		[SerializeField] private bool m_DrawVisibility = false;

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

		// Data
		[SerializeField] private SerializablePortalData m_PortalData;

		// Runtime
		private bool isInitialized = false;
		private Dictionary<int, MeshRenderer> m_VisibleRenderers = new Dictionary<int, MeshRenderer>();
		private List<PortalAgent> m_ActiveAgents = new List<PortalAgent>();

		// Statistics
		private float m_BakeStartTime;
		[SerializeField] private float m_BakeTime;
		public float bakeTime
		{
			get { return m_BakeTime; }
		} 

		[SerializeField] private Vector3 m_RayStatistics;
		public Vector3 rayStatistics
		{
			get { return m_RayStatistics; }
		}

		[SerializeField] private Vector2 m_OccludeeStatistics;
		public Vector2 occludeeStatistics
		{
			get { return m_OccludeeStatistics; }
		}

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
			// Abort if in play mode as save actions arent valid
			if(Application.isPlaying)
			{
				Debug.LogWarning("Cannot modifiy Portal data while in Play mode. Aborting.");
				return;
			}

			// Start bake
			EditorCoroutines.StartCoroutine(BakePortalData(), this);
		}
		
		/// <summary>
        /// Cancel any active bake and clear all data. Editor only.
        /// </summary>
		public void OnClickCancel()
		{
			// Abort if in play mode as save actions arent valid
			if(Application.isPlaying)
			{
				Debug.LogWarning("Cannot modifiy Portal data while in Play mode. Aborting.");
				return;
			}

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

		// --------------------------------------------------
        // RUNTIME

		private void OnEnable()
		{
			// Runtime only
			if(!Application.isPlaying)
				return;

			// If no active data dont calculate visibility at runtime
			if(m_BakeState != BakeState.Active)
				return;

			// Set renderers from all Volumes as not visible
            m_VisibleRenderers = m_PortalData.GetAllRenderers();
			foreach(MeshRenderer renderer in m_VisibleRenderers.Values)
				renderer.enabled = false;

			// Init per frame visibility calculations
			isInitialized = true;
		}

		private void Update()
		{
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
			// Update active volumeID for each agent
			var previousVolumeIDs = m_ActiveAgents.Select(i => i.activeVolumeID).ToArray();
			for(int i = 0; i < m_ActiveAgents.Count; i++)
			{
				foreach(SerializableVolume volume in m_PortalData.volumes)
				{
					if(new Bounds(volume.positionWS, volume.scaleWS).Contains(m_ActiveAgents[i].transform.position))
						m_ActiveAgents[i].activeVolumeID = volume.volumeID;
				}
			}

			// If active volume has changed for any agent update occlusion
			var currentVolumeIDs = m_ActiveAgents.Select(i => i.activeVolumeID).ToArray();
			if(!previousVolumeIDs.SequenceEqual(currentVolumeIDs))
				UpdateOcclusion();
		}

		private void UpdateOcclusion()
		{
			// Get an array of all unique active volumes
			var activeVolumes = new SerializableVolume[m_ActiveAgents.Count];
			for(int i = 0; i < activeVolumes.Length; i++)
				activeVolumes[i] = m_PortalData.volumes[m_ActiveAgents[i].activeVolumeID];
			var uniqueActiveVolumes = activeVolumes.Select(s => s.volumeID).Where(s => s != -1).Distinct().ToArray();

			// Get all Visibility data from active volumes
			var activeVisibility = new List<VisbilityData>();
			foreach(VisbilityData visibility in m_PortalData.visibilityTable)
			{
				if(uniqueActiveVolumes.Contains(visibility.volume.volumeID))
					activeVisibility.Add(visibility);
			}

			// Disable all visible renderers not in this active Volume
			var visibleRenderers = m_VisibleRenderers.Values.ToArray();
			var allVisibilityRenderers = activeVisibility.SelectMany(i => i.renderers).Distinct().ToArray();
			foreach(MeshRenderer renderer in visibleRenderers)
			{
				if (!allVisibilityRenderers.Contains(renderer))
					renderer.enabled = false;
			}

			// Enable all Renderers in the active Volumes that arent current visible
			MeshRenderer disposablerenderer;
			m_VisibleRenderers.Clear();
			int[] IDs = new int[allVisibilityRenderers.Length];
			for (int i = 0; i < allVisibilityRenderers.Length; i++)
			{
				IDs[i] = allVisibilityRenderers[i].GetInstanceID();
				if (!m_VisibleRenderers.TryGetValue(allVisibilityRenderers[i].gameObject.GetInstanceID(), out disposablerenderer))
					allVisibilityRenderers[i].enabled = true;
			}

			// Update visible renderers to match active Volumes
			for (int i = 0; i < allVisibilityRenderers.Length; i++)
				m_VisibleRenderers.Add(IDs[i], allVisibilityRenderers[i]);
		}

		// --------------------------------------------------
        // BAKE

#if UNITY_EDITOR
		private IEnumerator BakePortalData()
		{
			// Generate Portal data
			m_Completion = 0.0f;
			m_BakeTime = 0.0f;
			m_BakeStartTime = Time.realtimeSinceStartup;
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
			m_BakeTime = Time.realtimeSinceStartup - m_BakeStartTime;
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
			int totalRays = 0;
			int passedRays = 0;
			int occludedRays = 0;

			// Get Occluder proxies, volumes and occludees
			var occluderProxies = PortalPrepareUtil.GetOccluderProxies(occluders);
			var lowestSubdivisionVolumes = PortalPrepareUtil.FilterVolumeDataNoChildren(volumes);
			var occludees = PortalPrepareUtil.GetStaticOccludeeRenderers();

			// Build Visibility for Volumes
			for(int v = 0; v < lowestSubdivisionVolumes.Length; v++)
			{
				// Setup
				m_Completion = (float)(v + 1) / (float)lowestSubdivisionVolumes.Length;
				var volume = lowestSubdivisionVolumes[v];
				var passedObjects = new List<GameObject>();
				var debugData = new List<VisbilityData.Debug>();

				// Iterate random rays based on volume density
				var rayCount = PortalVisibilityUtil.CalculateRayCount(m_RayDensity, volume.scaleWS);
				totalRays += rayCount;
				for(int r = 0; r < rayCount; r++)
				{
					// Get a random ray and a list of cone filtered renderers to test
					var rayPosition = PortalVisibilityUtil.GetRandomPointWithinVolume(volume);
					var rayDirection = PortalVisibilityUtil.RandomSphericalDistributionVector();
					var filteredOccludees = PortalVisibilityUtil.FilterRenderersByConeAngle(occludees, rayPosition, rayDirection, m_ConeAngle);
					for(int f = 0; f < filteredOccludees.Length; f++)
					{
						// Test ray against renderer AABB
						if(PortalVisibilityUtil.CheckAABBIntersection(rayPosition, rayDirection, filteredOccludees[f].bounds))
						{
							// Test ray against occluders
							Vector3 hitPos;
                            bool passed = PortalVisibilityUtil.CheckOcclusion(occluderProxies, filteredOccludees[f], rayPosition, rayDirection, out hitPos);
							switch(passed)
							{
								case true:
									passedObjects.AddIfUnique(filteredOccludees[f].gameObject);
									passedRays++;
									break;
								case false:
									occludedRays++;	
									break;
							}

							// Track visibility debug data
                            debugData.Add(new VisbilityData.Debug()
                            {
                                source = rayPosition,
                                hit = hitPos,
                                direction = rayDirection,
                                distance = Vector3.Distance(rayPosition, filteredOccludees[f].transform.position),
                                isHit = !passed,
                            });
						}
					}
				}

				// Add to VisibilityTable
				visibilityTable.Add(new VisbilityData(volume, passedObjects.ToArray(), debugData.ToArray()));
				yield return null;
			}

			// Clear Occluder proxies
			for(int i = 0; i < occluderProxies.Length; i++)
				PortalCoreUtil.Destroy(occluderProxies[i].gameObject);

			// Save statistics
			var occludeeCount = PortalCoreUtil.GetUniqueOccludeeCount(visibilityTable);
			m_OccludeeStatistics = new Vector2(occludeeCount, occludees.Length);
			m_RayStatistics = new Vector3(passedRays, occludedRays, totalRays);

			// Finalize
			result(visibilityTable);
		}

		// -------------------------------------------------- //
        //                       GIZMOS                       //
        // -------------------------------------------------- //

        private void OnDrawGizmos()
        {
			// Draw Icon
			Gizmos.DrawIcon(transform.position, "kTools/Portals/PortalSystem icon.png", true);

			// If no Portal data dont draw gizmos
			if(m_BakeState != BakeState.Active)
				return;

			// Conditional draw of gizmo types
			DrawOccluderGizmos();
			DrawVolumeGizmos();
			DrawVisibilityGizmos();
        }

		private void DrawOccluderGizmos()
		{
			// Check draw toggle
			if(m_DrawOccluders == false)
				return;

			// Draw gizmos for all Occluders
			foreach(SerializableOccluder occluder in m_PortalData.occluders)
				PortalDebugUtil.DrawMesh(occluder.positionWS, occluder.rotationWS, occluder.scaleWS, 
					occluder.mesh, PortalDebugColors.occluder);
		}

		private void DrawVolumeGizmos()
		{
			// Check draw toggle
			if(m_DrawVolumes == false)
				return;

			// Draw gizmos for all Volumes
			foreach(SerializableVolume volume in PortalPrepareUtil.FilterVolumeDataNoChildren(m_PortalData.volumes))
				PortalDebugUtil.DrawCube(volume.positionWS, volume.rotationWS, volume.scaleWS, PortalDebugColors.volume);
		}

		private void DrawVisibilityGizmos()
		{
			// Check draw toggle
			if(m_DrawVisibility == false)
				return;

			// Iterate all debug data in Visibility table
			foreach(VisbilityData visibilityData in m_PortalData.visibilityTable)
            {
				foreach(VisbilityData.Debug debug in visibilityData.debugData)
				{
					if(debug.isHit)
					{
						// Draw line between ray source and Occluder hit point
						PortalDebugUtil.DrawSphere(debug.source, 0.1f, PortalDebugColors.black);
						PortalDebugUtil.DrawSphere(debug.hit, 0.1f, PortalDebugColors.black);
						PortalDebugUtil.DrawLine(debug.source, debug.hit, PortalDebugColors.black);
					}
					else
					{
						// Draw line between ray source and Occludee (rough)
						PortalDebugUtil.DrawSphere(debug.source, 0.1f, PortalDebugColors.white);
						PortalDebugUtil.DrawRay(debug.source, debug.direction, debug.distance, PortalDebugColors.white);
					}
				}
            }
		}
#endif
	}
}
