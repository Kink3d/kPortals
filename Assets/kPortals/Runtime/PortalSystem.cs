using System;
using System.Collections;
using System.Collections.Generic;
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
		private Dictionary<PortalAgent, int> m_ActiveAgents = new Dictionary<PortalAgent, int>();

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Register a PortalAgent for calculating visibility.
        /// </summary>
        /// <param name="agent">PortalAgent to add.</param>
		public void RegisterAgent(PortalAgent agent)
		{
			if(!m_ActiveAgents.ContainsKey(agent))
				m_ActiveAgents.Add(agent, -1);
		}

		/// <summary>
        /// Unregister a PortalAgent from calculating visibility.
        /// </summary>
        /// <param name="agent">PortalAgent to remove.</param>
		public void UnregisterAgent(PortalAgent agent)
		{
			if(m_ActiveAgents.ContainsKey(agent))
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
			// Runtime only
			if(!Application.isPlaying)
				return;
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
