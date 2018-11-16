using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace kTools.Portals
{
	public static class PortalVisibilityUtil
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

#if UNITY_EDITOR
		public static int CalculateRayCount(int density, Vector3 volumeScale)
		{
			int rayCount = (int)(density * volumeScale.x * volumeScale.y * volumeScale.z);
			return rayCount;
		}

		public static Vector3 RandomPointWithinVolume(SerializableVolume volume)
        {
            var x = UnityEngine.Random.Range(volume.positionWS.x - (volume.scaleWS.x / 2), volume.positionWS.x + (volume.scaleWS.x / 2));
            var y = UnityEngine.Random.Range(volume.positionWS.y - (volume.scaleWS.y / 2), volume.positionWS.y + (volume.scaleWS.y / 2));
            var z = UnityEngine.Random.Range(volume.positionWS.z - (volume.scaleWS.z / 2), volume.positionWS.z + (volume.scaleWS.z / 2));
            return new Vector3(x, y, z);
        }

		public static Vector3 RandomSphericalDistributionVector()
        {
            var theta = UnityEngine.Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            var phi = UnityEngine.Random.Range(0, Mathf.PI * 2);
            var x = Mathf.Cos(theta) * Mathf.Cos(phi);
            var y = Mathf.Cos(theta) * Mathf.Sin(phi);
            var z = Mathf.Sin(theta);
            return new Vector3(x, y, z);
        }

		public static MeshRenderer[] FilterRenderersByConeAngle(MeshRenderer[] input, Vector3 conePosition, Vector3 coneDirection, float coneAngle)
		{
			Vector3 rayEnd = conePosition + coneDirection; 
			return input.Where( s => AngleBetweenThreePoints(conePosition, rayEnd, s.bounds.center) < DegreesToRadians(coneAngle)).ToArray();
		}

		public static bool CheckAABBIntersection(Vector3 position, Vector3 direction, Bounds bounds)
		{ 
			var minMax = new Vector3[2] { bounds.min, bounds.max };

			var inverseDirection = new Vector3(1 / direction.x, 1 / direction.y, 1 / direction.z);
			var signX = (inverseDirection.x < 0 ? 1 : 0);
			var signY = (inverseDirection.y < 0 ? 1 : 0);
			var signZ = (inverseDirection.z < 0 ? 1 : 0);

			var tmin = (minMax[signX].x - position.x) * inverseDirection.x; 
			var tmax = (minMax[1 - signX].x - position.x) * inverseDirection.x; 
			var tymin = (minMax[signY].y - position.y) * inverseDirection.y; 
			var tymax = (minMax[1 - signY].y - position.y) * inverseDirection.y; 

			if ((tmin > tymax) || (tymin > tmax)) 
				return false; 
			if (tymin > tmin) 
				tmin = tymin; 
			if (tymax < tmax) 
				tmax = tymax; 

			var tzmin = (minMax[signZ].z - position.z) * inverseDirection.z; 
			var tzmax = (minMax[1 - signZ].z - position.z) * inverseDirection.z; 

			if ((tmin > tzmax) || (tzmin > tmax)) 
				return false; 
			if (tzmin > tmin) 
				tmin = tzmin; 
			if (tzmax < tmax) 
				tmax = tzmax;

			return true; 
		}

		public static bool CheckOcclusion(MeshCollider[] occluders, MeshRenderer occludee, Vector3 position, Vector3 direction, out Vector3 occluderHit)
		{
			// Initialize hit position
			occluderHit = Vector3.zero;

			// If no occluders always return true
			if(occluders == null || occluders.Length == 0)
				return true;

			// Raycast through all objects, return true if no collisions
			var allHits = Physics.RaycastAll(position, direction);
			if(allHits.Length == 0)
				return true;
			
			// Get all occluders that intersected the ray, return true if no intersections
			var intersectingOccluders = new Dictionary<MeshCollider, Vector3>();
			if(!TryGetIntersectingOccluders(occluders, allHits, out intersectingOccluders))
				return true;
			
			// Filter by distance from ray source
			var orderedOccluders = intersectingOccluders.OrderBy(s => Vector3.Distance(position, s.Value));
			var closestOccluder = orderedOccluders.ElementAt(0);

			// If it is the same object as the occludee always return true
			if(AreOccludeeeAndOccluderEqual(closestOccluder.Key, occludee))
				return true;

			// Assign hit position to the location of hit on nearest occluder
			occluderHit = closestOccluder.Value;

			// TODO
			// - Need intersection point with AABB
			// Return true if the occluder is closer to the ray source than the occludee
			return Vector3.Distance(position, closestOccluder.Value) > Vector3.Distance(position, occludee.bounds.center);
		}
#endif

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

#if UNITY_EDITOR
		// --------------------------------------------------
        // GEOMETRY

		private static float AngleBetweenThreePoints(Vector3 center, Vector3 pointA, Vector3 pointB)
		{
			// Create vectors from points
			var v1 = pointA - center;
			var v2 = pointB - pointA;

			// Get angle between vectors
			var cross = Vector3.Cross(v1, v2);
			var dot = Vector3.Dot(v1, v2);
			var angle = Mathf.Atan2(cross.magnitude, dot);
			return (float) angle;
		}

		private static double DegreesToRadians(double angle)
		{
			// Convert an angle from degrees to radians
			return (Math.PI / 180) * angle;
		}

		// --------------------------------------------------
        // OCCLUSION

		private static bool TryGetIntersectingOccluders(MeshCollider[] occluders, RaycastHit[] hits, out Dictionary<MeshCollider, Vector3> occluderHits)
		{
			// Iterate hits and occluders
			// If occluders contains hit track the hit values
			occluderHits = new Dictionary<MeshCollider, Vector3>();
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

		private static bool AreOccludeeeAndOccluderEqual(MeshCollider occluder, MeshRenderer occludee)
		{
			// Return true if occluder and occludee are based on the same object
			var positionsEqual = occludee.transform.position == occluder.transform.position;
			var meshesEqual = occludee.GetComponent<MeshFilter>().sharedMesh == occluder.sharedMesh;
			return positionsEqual && meshesEqual;
		} 
#endif
	}
}
