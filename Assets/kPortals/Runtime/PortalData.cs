using System;
using UnityEngine;

namespace kTools.PortalsOld
{
	// TODO
	// - This should be a struct
	[Serializable]
	public class VolumeData
	{
		// -------------------------------------------------- //
        //                     CONSTRUCTORS                   //
        // -------------------------------------------------- //

		// TODO
		// - There shouldnt be a constructor without parameters
		public VolumeData() {}

		public VolumeData(Bounds bounds, VolumeData[] children, MeshRenderer[] renderers)
		{
			this.bounds = bounds;
			this.children = children;
			this.renderers = renderers;
		}

		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		// TODO
		// - These shouldnt be public
		public Bounds bounds;
		public VolumeData[] children;
		public MeshRenderer[] renderers;
	}

	// TODO
	// - This should be a struct
	[Serializable]
	public class OccluderData
	{
		// -------------------------------------------------- //
        //                     CONSTRUCTORS                   //
        // -------------------------------------------------- //

		public OccluderData(MeshCollider collider, MeshRenderer renderer)
		{
			this.collider = collider;
			this.renderer = renderer;
		}

		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		// TODO
		// - These shouldnt be public
		public MeshCollider collider;
		public MeshRenderer renderer;
	}

    [Serializable]
    public struct OccluderHit
	{
		// -------------------------------------------------- //
        //                     CONSTRUCTORS                   //
        // -------------------------------------------------- //

		public OccluderHit(Vector3 point, OccluderData data)
		{
			this.point = point;
			this.data = data;
		}

		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		// TODO
		// - These shouldnt be public
		public Vector3 point;
		public OccluderData data;
	}

    [Serializable]
    public struct VolumeDebugData
    {
		// -------------------------------------------------- //
        //                     CONSTRUCTORS                   //
        // -------------------------------------------------- //

        public VolumeDebugData(RayDebugData[] rays, int totalRenderers, int visibleRenderers)
        {
            this.rays = rays;
            this.totalRenderers = totalRenderers;
            this.visibleRenderers = visibleRenderers;
        }

		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		// TODO
		// - These shouldnt be public
        public RayDebugData[] rays;
        public int totalRenderers;
        public int visibleRenderers;
    }

    [Serializable]
    public struct RayDebugData
    {
		// -------------------------------------------------- //
        //                     CONSTRUCTORS                   //
        // -------------------------------------------------- //

        public RayDebugData(Vector3 position, Vector3 direction, bool pass)
        {
            this.position = position;
			this.direction = direction;
            this.pass = pass;
        }

		// -------------------------------------------------- //
        //                    PUBLIC FIELDS                   //
        // -------------------------------------------------- //

		// TODO
		// - These shouldnt be public
        public Vector3 position;
		public Vector3 direction;
        public bool pass;
    }
}
