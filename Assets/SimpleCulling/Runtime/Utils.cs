using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	public static class Utils
	{
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

		public static MeshRenderer[] GetStaticRenderers()
		{
			return UnityEngine.Object.FindObjectsOfType<MeshRenderer>().Where(s => s.gameObject.isStatic).ToArray();
		}

		public static MeshRenderer[] FilterRenderersByConeAngle(MeshRenderer[] input, Vector3 conePosition, Vector3 coneDirection, float coneAngle)
		{
			Vector3 rayEnd = conePosition + coneDirection; 
			return input.Where( s => Utils.AngleBetweenThreePoints(conePosition, rayEnd, s.bounds.center) < Utils.DegreesToRadians(coneAngle)).ToArray();
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

			//var test = Vector3.Dot(Vector3.up, cross);
			//if (test < 0.0) 
			//	angle = -angle;
			return (float) angle;
		}

		public static double DegreesToRadians(double angle)
		{
			return (Math.PI / 180) * angle;
		}

		// --------------------------------------------------
		// Hirarchical Volume Grid

		public static void IterateHierarchicalVolumeGrid(int count, int density, ref VolumeData data)
		{
			count++;
			VolumeData[] childData = new VolumeData[8];
			Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
			for(int i = 0; i < childData.Length; i++)
			{
				Vector3 size = new Vector3(data.bounds.size.x * 0.5f, data.bounds.size.y * 0.5f, data.bounds.size.z * 0.5f);
				float signX = (float)(i + 1) % 2 == 0 ? 1 : -1;  
				float signY = i == 2 || i == 3 || i == 6 || i == 7 ? 1 : -1; // TODO - Maths
				float signZ = i == 4 || i == 5 || i == 6 || i == 7 ? 1 : -1; //(float)(i + 1) * 0.5f > 4 ? 1 : -1; // TODO - Maths
				Vector3 position = data.bounds.center + new Vector3(signX * size.x * 0.5f, signY * size.y * 0.5f, signZ * size.z * 0.5f);
				Bounds bounds = new Bounds(position, size);
				childData[i] = new VolumeData(bounds, null, null);

				if(count < density)
				{
					IterateHierarchicalVolumeGrid(count, density, ref childData[i]);
				}
			}
			data.children = childData;
		}

		public static void IterateHierarchicalVolumeDebug(VolumeData data)
		{
			if(data.children != null && data.children.Length > 0)
			{
				for(int i = 0; i < data.children.Length; i++)
					IterateHierarchicalVolumeDebug(data.children[i]);
			}
			else
			{
				Gizmos.color = EditorColors.volumeFill;
				Gizmos.DrawCube(data.bounds.center, data.bounds.size);
				Gizmos.color = EditorColors.volumeWire;
				Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
			}
		}
	}

	// --------------------------------------------------
	// Data Structures

	[Serializable]
	public class OccluderData
	{
		public OccluderData(MeshCollider collider)
		{
			this.collider = collider;
		}

		public MeshCollider collider;
	}

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

	// --------------------------------------------------
	// Editor Constants

	public static class EditorColors
	{
		public static Color occluderWire = new Color(0f, 1f, 1f, 0.5f);
		public static Color occluderFill = new Color(0f, 1f, 1f, 1f);
		public static Color volumeWire = new Color(1f, 1f, 1f, 0.5f);
		public static Color volumeFill = new Color(1f, 1f, 1f, 0.1f);

		// Debug
		public static Color debugWhiteWire = new Color(1f, 1f, 1f, 1.0f);
		public static Color debugBlackWire =new Color(0f, 0f, 0f, 1.0f);
		public static Color debugBlueWire = new Color(0f, 1f, 1f, 1.0f);
		public static Color debugBlackFill =new Color(0f, 0f, 0f, 0.5f);
		public static Color debugBlueFill = new Color(1f, 1f, 1f, 0.5f);
	}
}
