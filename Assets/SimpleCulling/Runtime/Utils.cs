using System;
using System.Collections;
using System.Collections.Generic;
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
		public MeshCollider collider;

		public OccluderData(MeshCollider collider)
		{
			this.collider = collider;
		}
	}

	[Serializable]
	public class VolumeData
	{
		public Bounds bounds;
		public VolumeData[] children;
		public Renderer[] renderers;

		public VolumeData(Bounds bounds, VolumeData[] children, Renderer[] renderers)
		{
			this.bounds = bounds;
			this.children = children;
			this.renderers = renderers;
		}
	}

	// --------------------------------------------------
	// Editor Constants

	public static class EditorColors
	{
		public static Color occluderWire = new Color(0f, 1f, 1f, 0.5f);
		public static Color occluderFill = new Color(0f, 1f, 1f, 1f);
		public static Color volumeWire = new Color(1f, 1f, 1f, 0.5f);
		public static Color volumeFill = new Color(1f, 1f, 1f, 0.1f);
	}
}
