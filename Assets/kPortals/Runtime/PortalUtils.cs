using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.Portals
{
	public static class PortalUtils
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		// --------------------------------------------------
        // RUNTIME

		public static bool GetActiveVolumeAtPosition(VolumeData volumeData, Vector3 position, out VolumeData activeVolume)
		{
			// If the Volume contains position
			if(volumeData.bounds.Contains(position))
			{
				if(volumeData.children != null && volumeData.children.Length > 0)
				{
					// Continue to traverse down the volume hierarchy
					for(int i = 0; i < volumeData.children.Length; i++)
					{
						if(GetActiveVolumeAtPosition(volumeData.children[i], position, out activeVolume))
							return true;
					}
				}
				else
				{
					// Active volume at final subdivision of volume hierarchy
					activeVolume = volumeData;
					return true;
				}
			}

			// Failed
			activeVolume = null;
			return false;
		}

        public static VolumeData[] GetLowestSubdivisionVolumes(VolumeData data, int density)
        {
			// Convert requested density to subdvision value
            int finalSubdivisionVolumeCount = (int)Mathf.Pow(density, 8);
            List<VolumeData> dataCollection = new List<VolumeData>();

            int count = 0;
            if (count < density)
            {
				// Traverse volume hierarchy
                for (int i = 0; i < data.children.Length; i++)
                    GetLowestSubdivisionVolumesRecursive(count, density, data.children[i], ref dataCollection);
            }
            else
                dataCollection.Add(data);
            return dataCollection.ToArray();
        }

#if UNITY_EDITOR
        // --------------------------------------------------
        // SCENE DATA

        public static Bounds GetSceneBounds(MeshRenderer[] staticRenderers)
        {
            Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int i = 0; i < staticRenderers.Length; i++)
            {
                sceneBounds.Encapsulate(staticRenderers[i].bounds);
            }
            float maxSize = Mathf.Max(Mathf.Max(sceneBounds.size.x, sceneBounds.size.y), sceneBounds.size.z);
            sceneBounds.size = new Vector3(maxSize, maxSize, maxSize);
            return sceneBounds;
        }

        public static MeshRenderer[] GetStaticRenderers()
		{
			return UnityEngine.Object.FindObjectsOfType<MeshRenderer>().Where(s => s.gameObject.isStatic).ToArray();
		}

		public static MeshRenderer[] FilterRenderersByConeAngle(MeshRenderer[] input, Vector3 conePosition, Vector3 coneDirection, float coneAngle)
		{
			Vector3 rayEnd = conePosition + coneDirection; 
			return input.Where( s => PortalUtils.AngleBetweenThreePoints(conePosition, rayEnd, s.bounds.center) < PortalUtils.DegreesToRadians(coneAngle)).ToArray();
		}

        // --------------------------------------------------
        // INTERSECTION

        public static bool CheckAABBIntersection(Vector3 position, Vector3 direction, Bounds bounds)
		{ 
			Vector3[] minMax = new Vector3[2] { bounds.min, bounds.max };

			Vector3 inverseDirection = new Vector3(1 / direction.x, 1 / direction.y, 1 / direction.z);
			int signX = (inverseDirection.x < 0 ? 1 : 0);
			int signY = (inverseDirection.y < 0 ? 1 : 0);
			int signZ = (inverseDirection.z < 0 ? 1 : 0);

			float tmin = (minMax[signX].x - position.x) * inverseDirection.x; 
			float tmax = (minMax[1 - signX].x - position.x) * inverseDirection.x; 
			float tymin = (minMax[signY].y - position.y) * inverseDirection.y; 
			float tymax = (minMax[1 - signY].y - position.y) * inverseDirection.y; 

			if ((tmin > tymax) || (tymin > tmax)) 
				return false; 
			if (tymin > tmin) 
				tmin = tymin; 
			if (tymax<tmax) 
				tmax = tymax; 

			float tzmin = (minMax[signZ].z - position.z) * inverseDirection.z; 
			float tzmax = (minMax[1 - signZ].z - position.z) * inverseDirection.z; 

			if ((tmin > tzmax) || (tzmin > tmax)) 
				return false; 
			if (tzmin > tmin) 
				tmin = tzmin; 
			if (tzmax<tmax) 
				tmax = tzmax; 

			return true; 
		}

		public static bool CheckOcclusion(OccluderData[] occluders, MeshRenderer occludee, Vector3 position, Vector3 direction)
		{
			if(occluders == null)
				return true;

			RaycastHit[] allHits = Physics.RaycastAll(position, direction);
			if(allHits.Length == 0)
				return true;
			
			OccluderHit[] occluderHits = FilterHitsByOccluders(occluders, allHits);
			if(occluderHits == null || occluderHits.Length == 0)
				return true;

			occluderHits.OrderBy(s => Vector3.Distance(position, s.point));
			if(occluderHits[0].data.renderer == occludee)
				return true;

			// TODO
			// - Need intersection point with AABB
			return Vector3.Distance(position, occluderHits[0].point) > Vector3.Distance(position, occludee.bounds.center);
		}

        // --------------------------------------------------
        // VISIBILITY

		public static IEnumerator BuildOcclusionForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, OccluderData[] occluders, Action<MeshRenderer[]> result, object obj, int coneAngle = 45)
        {
            VolumeDebugData debugData = new VolumeDebugData();
            yield return EditorCoroutines.StartCoroutine(BuildVisibilityAndOcclusionForVolume(bounds, rayDensity, staticRenderers, result,  value => debugData = value, coneAngle, occluders), obj);
        }

		public static IEnumerator BuildVisibilityForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, Action<MeshRenderer[]> result, Action<VolumeDebugData> debugData, object obj, int coneAngle = 45)
        {
            yield return EditorCoroutines.StartCoroutine(BuildVisibilityAndOcclusionForVolume(bounds, rayDensity, staticRenderers, result, debugData, coneAngle), obj);
        }

        // --------------------------------------------------
        // OCCLUDER PROXY GEOMETRY

        public static string occluderContainerName = "OccluderProxies";

        public static IEnumerator BuildOccluderProxyGeometry(Transform parent, MeshRenderer[] staticRenderers, Action<OccluderData[]> result, Component component, string tag = "Occluder")
        {
			PortalSystem culling = component as PortalSystem;

            Transform container = NewObject(occluderContainerName, parent).transform;
            List<MeshRenderer> occluderRenderers = staticRenderers.Where(s => s.gameObject.tag == tag).ToList();
            OccluderData[] occluders = new OccluderData[occluderRenderers.Count];
            for (int i = 0; i < occluders.Length; i++)
            {
                MeshFilter meshFilter = occluderRenderers[i].GetComponent<MeshFilter>();
                if (meshFilter == null)
                    continue;

                GameObject occluderObj = occluderRenderers[i].gameObject;
                Transform occluderTransform = occluderObj.transform;
                GameObject proxyObj = NewObject(occluderObj.name, container, occluderTransform.position, occluderTransform.rotation, occluderTransform.lossyScale);
                MeshCollider proxyCollider = proxyObj.AddComponent<MeshCollider>();
                proxyCollider.sharedMesh = meshFilter.sharedMesh;
                occluders[i] = new OccluderData(proxyCollider, occluderRenderers[i]);

                if (culling != null)
					culling.completion = (float)(i + 1) / (float)occluders.Length;
                UnityEditor.SceneView.RepaintAll();
                yield return null;
            }
            result(occluders);
        }

		// --------------------------------------------------
        // VOLUME GRID

		public static IEnumerator BuildManualVolumeGrid(BoxCollider[] manualVolumes, Action<VolumeData> result)
        {
			VolumeData data = null;
			if(manualVolumes.Length == 0)
				result(data);

			Bounds bounds = new Bounds();
			for(int i = 0; i < manualVolumes.Length; i++)
				bounds.Encapsulate(manualVolumes[i].bounds);
            
            List<VolumeData> volumes = new List<VolumeData>();
			for(int i = 0; i < manualVolumes.Length; i++)
				volumes.Add(new VolumeData(manualVolumes[i].bounds, null, null));
			
			data = new VolumeData(bounds, volumes.ToArray(), null);
			yield return null;
			result(data);
        }
        
        public static IEnumerator BuildHierarchicalVolumeGrid(Bounds bounds, int density, Action<VolumeData> result, object obj)
        {
            VolumeData data = new VolumeData(bounds, null, null);
            int count = 0;
            if (count < density)
            {
                yield return EditorCoroutines.StartCoroutine(BuildHierarchicalVolumeGridRecursive(count, density, data, value => data = value, obj), obj);
            }
            result(data);
        }
#endif

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

		// --------------------------------------------------
        // RUNTIME

        private static void GetLowestSubdivisionVolumesRecursive(int count, int density, VolumeData volume, ref List<VolumeData> dataCollection)
        {
            count++;
            if (count < density)
            {
                for (int i = 0; i < volume.children.Length; i++)
                    GetLowestSubdivisionVolumesRecursive(count, density, volume.children[i], ref dataCollection);
            }
            else
                dataCollection.Add(volume);
        }

#if UNITY_EDITOR
		// --------------------------------------------------
        // SCENE DATA

		private static OccluderHit[] FilterHitsByOccluders(OccluderData[] occluders, RaycastHit[] hits)
		{
			if(occluders.Length == 0 || hits.Length == 0)
				return null;
			
			// TODO
			// - Remove GC
			Dictionary<Collider, OccluderData> occluderDictionary = occluders.ToDictionary(s => s.collider as Collider);
			List<OccluderHit> occluderHits = new List<OccluderHit>();
			foreach(RaycastHit hit in hits)
			{
				OccluderData hitOccluder;
				if(occluderDictionary.TryGetValue(hit.collider, out hitOccluder))
					occluderHits.Add(new OccluderHit(hit.point, hitOccluder));
			}
			
			return occluderHits.ToArray();
		}

		// --------------------------------------------------
        // OBJECT

        private static GameObject NewObject(string name, Transform parent = null, 
			Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Vector3 scale = default(Vector3))
		{
			if(position == default(Vector3))
				position = Vector3.zero;
			
			if(rotation == default(Quaternion))
				rotation = Quaternion.identity;

			if(scale == default(Vector3))
				scale = Vector3.one;

			GameObject obj = new GameObject(name);
			Transform transform = obj.transform;
			transform.SetParent(parent);
			transform.position = position;
			transform.localScale = transform.InverseTransformVector(scale);
			transform.rotation = rotation;
			return obj;
		}

		// --------------------------------------------------
        // GEOMETRY

        private static float AngleBetweenThreePoints(Vector3 center, Vector3 pointA, Vector3 pointB)
		{
			var v1 = pointA - center;
			var v2 = pointB - pointA;

			var cross = Vector3.Cross(v1, v2);
			var dot = Vector3.Dot(v1, v2);
			var angle = Mathf.Atan2(cross.magnitude, dot);
			return (float) angle;
		}
        
        private static double DegreesToRadians(double angle)
		{
			return (Math.PI / 180) * angle;
		}

        private static Vector3 RandomPointWithinBounds(Bounds bounds)
        {
            float x = UnityEngine.Random.Range(bounds.center.x - (bounds.size.x / 2), bounds.center.x + (bounds.size.x / 2));
            float y = UnityEngine.Random.Range(bounds.center.y - (bounds.size.y / 2), bounds.center.y + (bounds.size.y / 2));
            float z = UnityEngine.Random.Range(bounds.center.z - (bounds.size.z / 2), bounds.center.z + (bounds.size.z / 2));
            return new Vector3(x, y, z);
        }

        private static Vector3 RandomSphericalDistributionVector()
        {
            float theta = UnityEngine.Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            float phi = UnityEngine.Random.Range(0, Mathf.PI * 2);

            float x = Mathf.Cos(theta) * Mathf.Cos(phi);
            float y = Mathf.Cos(theta) * Mathf.Sin(phi);
            float z = Mathf.Sin(theta);

            return new Vector3(x, y, z);
        }

		// --------------------------------------------------
        // VISIBILITY

        private static IEnumerator BuildVisibilityAndOcclusionForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, Action<MeshRenderer[]> result, Action<VolumeDebugData> debugData, int coneAngle = 45, OccluderData[] occluders = null)
        {
            List<MeshRenderer> passedRenderers = new List<MeshRenderer>();
            int rayCount = (int)(rayDensity * bounds.size.x * bounds.size.y * bounds.size.z);

            RayDebugData[] rayDebugData = new RayDebugData[rayCount];

            for (int r = 0; r < rayCount; r++)
            {
                bool rayHasHit = false;

                Vector3 rayPosition = RandomPointWithinBounds(bounds);
                Vector3 rayDirection = RandomSphericalDistributionVector();

                MeshRenderer[] filteredRenderers = FilterRenderersByConeAngle(staticRenderers, rayPosition, rayDirection, coneAngle);
                for (int i = 0; i < filteredRenderers.Length; i++)
                {
                    if (CheckAABBIntersection(rayPosition, rayDirection, filteredRenderers[i].bounds))
                    {
						if(occluders != null)
						{
							if(CheckOcclusion(occluders, filteredRenderers[i], rayPosition, rayDirection))
							{
								rayHasHit = true;
								if (!passedRenderers.Contains(filteredRenderers[i]))
									passedRenderers.Add(filteredRenderers[i]);
							}
						}
						else
						{
							rayHasHit = true;
							if (!passedRenderers.Contains(filteredRenderers[i]))
                            	passedRenderers.Add(filteredRenderers[i]);
						}
                    }
                }

                Vector3 rayEnd = Vector3.Scale(rayDirection, new Vector3(10, 10, 10));
                rayDebugData[r] = new RayDebugData(rayPosition, rayDirection, rayHasHit);
            }

			yield return null;
			debugData(new VolumeDebugData(rayDebugData, staticRenderers.Length, passedRenderers.Count));
            result(passedRenderers.ToArray());
        }

		// --------------------------------------------------
        // VOLUME GRID
        
        private static IEnumerator BuildHierarchicalVolumeGridRecursive(int count, int density, VolumeData data, Action<VolumeData> result, object obj)
		{
			count++;
			VolumeData[] childData = new VolumeData[8];
			for(int i = 0; i < childData.Length; i++)
			{
				Vector3 size = new Vector3(data.bounds.size.x * 0.5f, data.bounds.size.y * 0.5f, data.bounds.size.z * 0.5f);

				// TODO
				// - Math this
				float signX = (float)(i + 1) % 2 == 0 ? 1 : -1;  
				float signY = i == 2 || i == 3 || i == 6 || i == 7 ? 1 : -1;
				float signZ = i == 4 || i == 5 || i == 6 || i == 7 ? 1 : -1; //(float)(i + 1) * 0.5f > 4 ? 1 : -1;
				Vector3 position = data.bounds.center + new Vector3(signX * size.x * 0.5f, signY * size.y * 0.5f, signZ * size.z * 0.5f);
				Bounds bounds = new Bounds(position, size);
				childData[i] = new VolumeData(bounds, null, null);

				if(count < density)
				{
					VolumeData childResult = new VolumeData();
					yield return EditorCoroutines.StartCoroutine(BuildHierarchicalVolumeGridRecursive(count, density, childData[i], value => childResult = value, obj), obj);
					childData[i] = childResult;
				}
			}
			data.children = childData;
			result(data);
		}
#endif

    }
}
