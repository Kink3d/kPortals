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
