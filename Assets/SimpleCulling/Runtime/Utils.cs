using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	public static class Utils
	{
		// ----------------------------------------------------------------------------------------------------//
		//                                              RUNTIME                                                 //
		// ----------------------------------------------------------------------------------------------------//

		public static bool GetActiveVolumeAtPosition(VolumeData volumeData, Vector3 position, out VolumeData activeVolume)
		{
			if(volumeData.bounds.Contains(position))
			{
				// Continue to traverse down the volume hierarchy
				if(volumeData.children.Length > 0)
				{
					for(int i = 0; i < volumeData.children.Length; i++)
						GetActiveVolumeAtPosition(volumeData.children[i], position, out activeVolume);
				}
				// Active volume at subdivision of volume hierarchy
				else
				{
					activeVolume = volumeData;
					return true;
				}
			}

			activeVolume = null;
			return false;
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                               BAKE                                                  //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Create Objects

		public static GameObject NewObject(string name, Transform parent = null, 
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
        // Scene Data

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
			return input.Where( s => Utils.AngleBetweenThreePoints(conePosition, rayEnd, s.bounds.center) < Utils.DegreesToRadians(coneAngle)).ToArray();
		}

		public static OccluderHit[] FilterHitsByOccluders(OccluderData[] occluders, RaycastHit[] hits)
		{
			if(occluders.Length == 0 || hits.Length == 0)
				return null;
			
			Dictionary<Collider, OccluderData> occluderDictionary = occluders.ToDictionary(s => s.collider as Collider);
			List<OccluderHit> occluderHits = new List<OccluderHit>(); // TD - Remove GC
			foreach(RaycastHit hit in hits)
			{
				OccluderData hitOccluder;
				if(occluderDictionary.TryGetValue(hit.collider, out hitOccluder))
					occluderHits.Add(new OccluderHit(hit.point, hitOccluder));
			}
			
			return occluderHits.ToArray();
		}

        // --------------------------------------------------
        // Geometry

        public static float AngleBetweenThreePoints(Vector3 center, Vector3 pointA, Vector3 pointB)
		{
			var v1 = pointA - center;
			var v2 = pointB - pointA;

			var cross = Vector3.Cross(v1, v2);
			var dot = Vector3.Dot(v1, v2);
			var angle = Mathf.Atan2(cross.magnitude, dot);
			return (float) angle;
		}
        
        public static double DegreesToRadians(double angle)
		{
			return (Math.PI / 180) * angle;
		}

        public static Vector3 RandomPointWithinBounds(Bounds bounds)
        {
            float x = UnityEngine.Random.Range(bounds.center.x - (bounds.size.x / 2), bounds.center.x + (bounds.size.x / 2));
            float y = UnityEngine.Random.Range(bounds.center.y - (bounds.size.y / 2), bounds.center.y + (bounds.size.y / 2));
            float z = UnityEngine.Random.Range(bounds.center.z - (bounds.size.z / 2), bounds.center.z + (bounds.size.z / 2));
            return new Vector3(x, y, z);
        }

        public static Vector3 RandomSphericalDistributionVector()
        {
            float theta = UnityEngine.Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            float phi = UnityEngine.Random.Range(0, Mathf.PI * 2);

            float x = Mathf.Cos(theta) * Mathf.Cos(phi);
            float y = Mathf.Cos(theta) * Mathf.Sin(phi);
            float z = Mathf.Sin(theta);

            return new Vector3(x, y, z);
        }

        // --------------------------------------------------
        // Intersection

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

			return Vector3.Distance(position, occluderHits[0].point) > Vector3.Distance(position, occludee.bounds.center); // TD - Need intersection point with AABB\
		}

        // --------------------------------------------------
        // Visibility

		public static MeshRenderer[] BuildOcclusionForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, OccluderData[] occluders, int coneAngle = 45)
        {
            VolumeDebugData debugData = new VolumeDebugData();
            return BuildOcclusionForVolume(bounds, rayDensity, staticRenderers, occluders, out debugData, coneAngle);
        }

		public static MeshRenderer[] BuildOcclusionForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, OccluderData[] occluders, out VolumeDebugData debugData, int coneAngle = 45)
        {
            return BuildVisibilityAndOcclusionForVolume(bounds, rayDensity, staticRenderers, out debugData, coneAngle, occluders);
        }

        public static MeshRenderer[] BuildVisibilityForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, int coneAngle = 45)
        {
            VolumeDebugData debugData = new VolumeDebugData();
            return BuildVisibilityForVolume(bounds, rayDensity, staticRenderers, out debugData, coneAngle);
        }

		public static MeshRenderer[] BuildVisibilityForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, out VolumeDebugData debugData, int coneAngle = 45)
        {
            return BuildVisibilityAndOcclusionForVolume(bounds, rayDensity, staticRenderers, out debugData, coneAngle);
        }

        private static MeshRenderer[] BuildVisibilityAndOcclusionForVolume(Bounds bounds, int rayDensity, MeshRenderer[] staticRenderers, out VolumeDebugData debugData, int coneAngle = 45, OccluderData[] occluders = null)
        {
            List<MeshRenderer> passedRenderers = new List<MeshRenderer>();
            int rayCount = (int)(rayDensity * bounds.size.x * bounds.size.y * bounds.size.z);

            debugData = new VolumeDebugData();
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
                rayDebugData[r] = new RayDebugData(new Vector3[2] { rayPosition, rayEnd }, rayHasHit);
            }

            debugData = new VolumeDebugData(rayDebugData, staticRenderers.Length, passedRenderers.Count);

            return passedRenderers.ToArray();
        }

        // --------------------------------------------------
        // Occluder Proxy Geometry

        public static string occluderContainerName = "OccluderProxies";

        public static OccluderData[] BuildOccluderProxyGeometry(Transform parent, MeshRenderer[] staticRenderers, string tag = "Occluder")
        {
            Transform container = Utils.NewObject(occluderContainerName, parent).transform;
            List<MeshRenderer> occluderRenderers = staticRenderers.Where(s => s.gameObject.tag == tag).ToList();
            OccluderData[] occluders = new OccluderData[occluderRenderers.Count];
            for (int i = 0; i < occluders.Length; i++)
            {
                GameObject occluderObj = occluderRenderers[i].gameObject;
                Transform occluderTransform = occluderObj.transform;
                GameObject proxyObj = NewObject(occluderObj.name, container, occluderTransform.position, occluderTransform.rotation, occluderTransform.lossyScale);
                MeshCollider proxyCollider = proxyObj.AddComponent<MeshCollider>();
                proxyCollider.sharedMesh = occluderRenderers[i].GetComponent<MeshFilter>().sharedMesh;
                occluders[i] = new OccluderData(proxyCollider, occluderRenderers[i]);
            }
            return occluders;
        }

        // --------------------------------------------------
        // Hirarchical Volume Grid
        
        public static VolumeData BuildHierarchicalVolumeGrid(Bounds bounds, int density)
        {
            VolumeData data = new VolumeData(bounds, null, null);
            int count = 0;
            if (count < density)
            {
                BuildHierarchicalVolumeGridRecursive(count, density, ref data);
            }
            return data;
        }
        
        private static void BuildHierarchicalVolumeGridRecursive(int count, int density, ref VolumeData data)
		{
			count++;
			VolumeData[] childData = new VolumeData[8];
			for(int i = 0; i < childData.Length; i++)
			{
				Vector3 size = new Vector3(data.bounds.size.x * 0.5f, data.bounds.size.y * 0.5f, data.bounds.size.z * 0.5f);
				float signX = (float)(i + 1) % 2 == 0 ? 1 : -1;  
				float signY = i == 2 || i == 3 || i == 6 || i == 7 ? 1 : -1; // TD - Maths
				float signZ = i == 4 || i == 5 || i == 6 || i == 7 ? 1 : -1; //(float)(i + 1) * 0.5f > 4 ? 1 : -1; // TD - Maths
				Vector3 position = data.bounds.center + new Vector3(signX * size.x * 0.5f, signY * size.y * 0.5f, signZ * size.z * 0.5f);
				Bounds bounds = new Bounds(position, size);
				childData[i] = new VolumeData(bounds, null, null);

				if(count < density)
				{
					BuildHierarchicalVolumeGridRecursive(count, density, ref childData[i]);
				}
			}
			data.children = childData;
		}

		public static VolumeData[] GetLowestSubdivisionVolumes(VolumeData data, int density)
		{
			int finalSubdivisionVolumeCount = (int)Mathf.Pow(density, 8);
			List<VolumeData> dataCollection = new List<VolumeData>();

            int count = 0;
            if (count < density)
            {
				for(int i = 0; i < data.children.Length; i++)
                	GetLowestSubdivisionVolumesRecursive(count, density, data.children[i], ref dataCollection);
            }
			else
				dataCollection.Add(data);
			return dataCollection.ToArray();
		}

		private static void GetLowestSubdivisionVolumesRecursive(int count, int density, VolumeData volume, ref List<VolumeData> dataCollection)
		{
			count++;
			if(count < density)
			{
				for(int i = 0; i < volume.children.Length; i++)
                	GetLowestSubdivisionVolumesRecursive(count, density, volume.children[i], ref dataCollection);
			}
			else
				dataCollection.Add(volume);
		}
	}

	// --------------------------------------------------
	// Data Structures

	[Serializable]
	public class VolumeData
	{
		public VolumeData(Bounds bounds, VolumeData[] children, Renderer[] renderers)
		{
			this.bounds = bounds;
			this.children = children;
			this.renderers = renderers;
		}

		public Bounds bounds;
		public VolumeData[] children;
		public Renderer[] renderers;
	}

	[Serializable]
	public class OccluderData
	{
		public OccluderData(MeshCollider collider, MeshRenderer renderer)
		{
			this.collider = collider;
			this.renderer = renderer;
		}

		public MeshCollider collider;
		public MeshRenderer renderer; // TD - Replace this with instance ID of object
	}

    [Serializable]
    public struct OccluderHit
	{
		public OccluderHit(Vector3 point, OccluderData data)
		{
			this.point = point;
			this.data = data;
		}

		public Vector3 point;
		public OccluderData data;
	}

    [Serializable]
    public struct VolumeDebugData
    {
        public VolumeDebugData(RayDebugData[] rays, int totalRenderers, int visibleRenderers)
        {
            this.rays = rays;
            this.totalRenderers = totalRenderers;
            this.visibleRenderers = visibleRenderers;
        }

        public RayDebugData[] rays;
        public int totalRenderers;
        public int visibleRenderers;
    }

    [Serializable]
    public struct RayDebugData
    {
        public RayDebugData(Vector3[] points, bool pass)
        {
            this.points = points;
            this.pass = pass;
        }

        public Vector3[] points;
        public bool pass;
    }

    // --------------------------------------------------
    // Editor Constants

    public static class DebugUtils
    {
        public static void DrawRendererDebug(MeshRenderer[] allRenderers, MeshRenderer[] passedRenderers)
        {
            if (allRenderers == null || passedRenderers == null)
                return;

            for (int i = 0; i < allRenderers.Length; i++)
            {
                bool isPassed = passedRenderers.Contains(allRenderers[i]);
                Transform transform = allRenderers[i].transform;
                Mesh mesh = allRenderers[i].GetComponent<MeshFilter>().sharedMesh;

                Gizmos.color = isPassed ? EditorColors.occludeePass[0] : EditorColors.occludeeFail[0];
                Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
                Gizmos.color = isPassed ? EditorColors.occludeePass[1] : EditorColors.occludeeFail[1];
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }

        public static void DrawRayDebug(Vector3 position, Vector3 direction)
        {
            Gizmos.color = EditorColors.ray[0];
            Gizmos.DrawLine(position, position + Vector3.Scale(direction, new Vector3(10, 10, 10)));
        }

        public static void DrawConeDebug(Vector3 position, Vector3 direction, float angle)
        {
            DebugExtension.DebugCone(position, Vector3.Scale(direction, new Vector3(10, 10, 10)), EditorColors.visualiser[0], angle, 0);
        }
    }

	public static class EditorColors
	{
        public static Color[] ray = new Color[2] { new Color(0f, 1f, 0f, 1f), new Color(0f, 1f, 0f, 0.1f) };
        public static Color[] visualiser = new Color[2] { new Color(0.5f, 0.5f, 0.5f, 1f), new Color(0.5f, 0.5f, 0.5f, 0.5f) };
        public static Color[] occludeePass = new Color[2] { new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.5f) };
        public static Color[] occludeeFail = new Color[2] { new Color(0f, 0f, 0f, 1f), new Color(0f, 0f, 0f, 0.5f) };

        // OLD
        public static Color occluderWire = new Color(0f, 1f, 1f, 0.5f);
		public static Color occluderFill = new Color(0f, 1f, 1f, 1f);
		public static Color volumeWire = new Color(1f, 1f, 1f, 0.5f);
		public static Color volumeFill = new Color(1f, 1f, 1f, 0.1f);
		public static Color debugWhiteWire = new Color(1f, 1f, 1f, 1.0f);
		public static Color debugBlackWire =new Color(0f, 0f, 0f, 1.0f);
		public static Color debugBlueWire = new Color(0f, 1f, 1f, 1.0f);
		public static Color debugBlackFill =new Color(0f, 0f, 0f, 0.5f);
		public static Color debugBlueFill = new Color(1f, 1f, 1f, 0.5f);
	}
}
