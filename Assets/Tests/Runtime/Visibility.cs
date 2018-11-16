using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/Visibility")]
	public class Visibility : MonoBehaviour, IBake 
	{	
		// -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //

		[SerializeField] private VolumeMode m_VolumeMode;
		[SerializeField] private int m_Subdivisions;
		[SerializeField] private int m_RayCount = 10;
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

        [SerializeField] private List<VisibilityDebug> m_Debugs;

		private bool isInitialized = false;
		private Dictionary<int, MeshRenderer> m_VisibleRenderers = new Dictionary<int, MeshRenderer>();
		private List<PortalAgent> m_ActiveAgents = new List<PortalAgent>();

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
            m_Debugs = new List<VisibilityDebug>();

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
				var rayCount = m_RayCount;
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
                            Vector3 hitPos;
                            bool hit = !CheckOcclusion(occluderProxies, filteredOccludees[f], rayPosition, rayDirection, out hitPos);
							if(hit)
								passedObjects.AddIfUnique(filteredOccludees[f].gameObject);

                            m_Debugs.Add(new VisibilityDebug()
                            {
                                source = rayPosition,
                                hit = hitPos,
                                direction = rayDirection,
                                distance = Vector3.Distance(rayPosition, filteredOccludees[f].transform.position),
                                isHit = hit,
                            });
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

        public static bool CheckOcclusion(Collider[] occluders, MeshRenderer occludee, Vector3 position, Vector3 direction, out Vector3 hitPos)
		{
            hitPos = Vector3.zero;

			// If no occluders always return true
			if(occluders == null || occluders.Length == 0)
				return true;

			// Raycast through all objects, return true if no collisions
			var allHits = Physics.RaycastAll(position, direction);
			if(allHits.Length == 0)
				return true;
			
			// Get all occluders that intersected the ray, return true if no intersections
			var intersectingOccluders = new Dictionary<Collider, Vector3>();
			if(!TryGetIntersectingOccluders(occluders, allHits, out intersectingOccluders))
				return true;
			
			// Filter by distance from ray source
			var orderedHits = intersectingOccluders.Values.OrderBy(s => Vector3.Distance(position, s)).ToArray();
			//var closestOccluder = intersectingOccluders.ElementAt(0);

			// If it is the same object as the occludee always return true
			//if(closestOccluder.Key.gameObject == occludee.gameObject)
			//	return true;

            hitPos = orderedHits[0];

			// TODO
			// - Need intersection point with AABB
			// Return true if the occluder is closer to the ray source than the occludee
			return Vector3.Distance(position, orderedHits[0]) > Vector3.Distance(position, occludee.bounds.center);
		}

        private static bool TryGetIntersectingOccluders(Collider[] occluders, RaycastHit[] hits, out Dictionary<Collider, Vector3> occluderHits)
		{
			// Iterate hits and occluders
			// If occluders contains hit track the hit values
			occluderHits = new Dictionary<Collider, Vector3>();
			for(int h = 0; h < hits.Length; h++)
			{
				for(int o = 0; o < occluders.Length; o++)
				{
					if(hits[h].collider == occluders[o])
						occluderHits.Add(occluders[o], hits[h].point);
				}
			}

			// True if any occluders were hit
			return occluderHits.Count > 0;
		}

        private void OnDrawGizmos()
        {
			if(m_BakeState != BakeState.Active || m_Debugs == null)
				return;

            foreach(VisibilityDebug debug in m_Debugs)
            {
                if(debug.isHit)
                {
                    PortalDebugUtil.DrawSphere(debug.source, 0.1f, PortalDebugColors.black);
                    PortalDebugUtil.DrawSphere(debug.hit, 0.1f, PortalDebugColors.black);
                    PortalDebugUtil.DrawLine(debug.source, debug.hit, PortalDebugColors.black);
                }
                else
                {
                    PortalDebugUtil.DrawSphere(debug.source, 0.1f, PortalDebugColors.white);
                    PortalDebugUtil.DrawRay(debug.source, debug.direction, debug.distance, PortalDebugColors.white);
                }
            }

			foreach(SerializableVolume volume in PortalPrepareUtil.FilterVolumeDataNoChildren(m_PortalData.volumes))
				PortalDebugUtil.DrawCube(volume.positionWS, volume.rotationWS, volume.scaleWS, PortalDebugColors.volume);
        }

        [Serializable]
        public struct VisibilityDebug
        {
            public Vector3 source;
            public Vector3 hit;
            public Vector3 direction;
            public float distance;
            public bool isHit;
        }
#endif
	}
}
