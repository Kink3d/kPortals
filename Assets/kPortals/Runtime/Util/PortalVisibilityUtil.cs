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
		/// <summary>
        /// Calculate the amount of rays that should be used for a Volume given its scale and a user defined density. Editor only.
        /// </summary>
        /// <param name="density">User defined density value. Equivalent to rays per cubic unit.</param>
		/// <param name="volumeScale">Volume scale in world space.</param>
		public static int CalculateRayCount(int density, Vector3 volumeScale)
		{
			int rayCount = (int)(density * volumeScale.x * volumeScale.y * volumeScale.z);
			return rayCount;
		}
		
		/// <summary>
        /// Get a random position within the bounds of a Volume in world space. Editor only.
        /// </summary>
		/// <param name="volume">Volume to use for bounds.</param>
		public static Vector3 GetRandomPointWithinVolume(SerializableVolume volume)
        {
            var x = UnityEngine.Random.Range(volume.positionWS.x - (volume.scaleWS.x / 2), volume.positionWS.x + (volume.scaleWS.x / 2));
            var y = UnityEngine.Random.Range(volume.positionWS.y - (volume.scaleWS.y / 2), volume.positionWS.y + (volume.scaleWS.y / 2));
            var z = UnityEngine.Random.Range(volume.positionWS.z - (volume.scaleWS.z / 2), volume.positionWS.z + (volume.scaleWS.z / 2));
            return new Vector3(x, y, z);
        }

		/// <summary>
        /// Get a random direction vector using spherical distribution. Editor only.
        /// </summary>
		public static Vector3 RandomSphericalDistributionVector()
        {
            var theta = UnityEngine.Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            var phi = UnityEngine.Random.Range(0, Mathf.PI * 2);
            var x = Mathf.Cos(theta) * Mathf.Cos(phi);
            var y = Mathf.Cos(theta) * Mathf.Sin(phi);
            var z = Mathf.Sin(theta);
            return new Vector3(x, y, z);
        }

		/// <summary>
        /// Filter an array of MeshRenderers by whether their bounds center is within a cone. Editor only.
        /// </summary>
        /// <param name="input">MeshRenderer array to filter.</param>
		/// <param name="conePositionWS">Position of the cone source in world space.</param>
		/// <param name="coneDirectionWS">Direction of the cone in world space.</param>
		/// <param name="coneAngle">Angle of the cone.</param>
		public static MeshRenderer[] FilterRenderersByConeAngle(MeshRenderer[] input, Vector3 conePositionWS, Vector3 coneDirectionWS, float coneAngle)
		{
			Vector3 rayEnd = conePositionWS + coneDirectionWS; 
			return input.Where( s => AngleBetweenThreePoints(conePositionWS, rayEnd, s.bounds.center) < DegreesToRadians(coneAngle)).ToArray();
		}

		/// <summary>
        /// Check whether a ray interesects with an axis aligned bounding box. Editor only.
        /// </summary>
        /// <param name="positionWS">Ray source position in world space.</param>
		/// <param name="directionWS">Direction of the ray in world space.</param>
		/// <param name="bounds">AABB to test.</param>
		public static bool CheckAABBIntersection(Vector3 positionWS, Vector3 directionWS, Bounds bounds)
		{ 
			var minMax = new Vector3[2] { bounds.min, bounds.max };

			var inverseDirection = new Vector3(1 / directionWS.x, 1 / directionWS.y, 1 / directionWS.z);
			var signX = (inverseDirection.x < 0 ? 1 : 0);
			var signY = (inverseDirection.y < 0 ? 1 : 0);
			var signZ = (inverseDirection.z < 0 ? 1 : 0);

			var tmin = (minMax[signX].x - positionWS.x) * inverseDirection.x; 
			var tmax = (minMax[1 - signX].x - positionWS.x) * inverseDirection.x; 
			var tymin = (minMax[signY].y - positionWS.y) * inverseDirection.y; 
			var tymax = (minMax[1 - signY].y - positionWS.y) * inverseDirection.y; 

			if ((tmin > tymax) || (tymin > tmax)) 
				return false; 
			if (tymin > tmin) 
				tmin = tymin; 
			if (tymax < tmax) 
				tmax = tymax; 

			var tzmin = (minMax[signZ].z - positionWS.z) * inverseDirection.z; 
			var tzmax = (minMax[1 - signZ].z - positionWS.z) * inverseDirection.z; 

			if ((tmin > tzmax) || (tzmin > tmax)) 
				return false; 
			if (tzmin > tmin) 
				tmin = tzmin; 
			if (tzmax < tmax) 
				tmax = tzmax;

			return true; 
		}

		/// <summary>
        /// Check whether a ray that interesects with an Occludee collides with an Occluder first. Editor only.
        /// </summary>
		/// <param name="occluders">Occluders to test.</param>
		/// <param name="occludee">Occludee to test against for distance.</param>
        /// <param name="positionWS">Ray source position in world space.</param>
		/// <param name="directionWS">Direction of the ray in world space.</param>
		/// <param name="occluderHit">Position of ray hit on occluder if there was one.</param>
		public static bool CheckOcclusion(MeshCollider[] occluders, MeshRenderer occludee, Vector3 positionWS, Vector3 directionWS, out Vector3 occluderHit)
		{
			// Initialize hit position
			occluderHit = Vector3.zero;

			// If no occluders always return true
			if(occluders == null || occluders.Length == 0)
				return true;

			// Raycast through all objects, return true if no collisions
			var allHits = Physics.RaycastAll(positionWS, directionWS);
			if(allHits.Length == 0)
				return true;
			
			// Get all occluders that intersected the ray, return true if no intersections
			var intersectingOccluders = new Dictionary<MeshCollider, Vector3>();
			if(!TryGetIntersectingOccluders(occluders, allHits, out intersectingOccluders))
				return true;
			
			// Filter by distance from ray source
			var orderedOccluders = intersectingOccluders.OrderBy(s => Vector3.Distance(positionWS, s.Value));
			var closestOccluder = orderedOccluders.ElementAt(0);

			// If it is the same object as the occludee always return true
			if(AreOccludeeeAndOccluderEqual(closestOccluder.Key, occludee))
				return true;

			// Assign hit position to the location of hit on nearest occluder
			occluderHit = closestOccluder.Value;

			// TODO
			// - Need intersection point with AABB
			// Return true if the occluder is closer to the ray source than the occludee
			return Vector3.Distance(positionWS, closestOccluder.Value) > Vector3.Distance(positionWS, occludee.bounds.center);
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
