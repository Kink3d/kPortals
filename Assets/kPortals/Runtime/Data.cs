using System;
using UnityEngine;

namespace SimpleTools.Culling
{
	[Serializable]
	public class VolumeData
	{
		public VolumeData() {}

		public VolumeData(Bounds bounds, VolumeData[] children, MeshRenderer[] renderers)
		{
			this.bounds = bounds;
			this.children = children;
			this.renderers = renderers;
		}

		public Bounds bounds;
		public VolumeData[] children;
		public MeshRenderer[] renderers;
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
		public MeshRenderer renderer;
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
        public RayDebugData(Vector3 position, Vector3 direction, bool pass)
        {
            this.position = position;
			this.direction = direction;
            this.pass = pass;
        }

        public Vector3 position;
		public Vector3 direction;
        public bool pass;
    }
}
