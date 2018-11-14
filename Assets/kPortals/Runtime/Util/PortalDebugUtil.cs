using UnityEngine;

namespace kTools.Portals
{
	public static class PortalDebugUtil
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

#if UNITY_EDITOR
		/// <summary>
        /// Draw a cube gizmo with a specified transformation. Editor only.
        /// </summary>
        /// <param name="positionWS">Gizmo position in world space.</param>
		/// <param name="rotationWS">Gizmo rotation in world space.</param>
		/// <param name="scaleWS">Gizmo space in world space.</param>
		/// <param name="color">Gizmo colors.</param>
		public static void DrawCube(Vector3 positionWS, Quaternion rotationWS, Vector3 scaleWS, PortalDebugColor color)
		{
			Matrix4x4 cubeTransform = Matrix4x4.TRS(positionWS, rotationWS, scaleWS);
			Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
			Gizmos.matrix = Gizmos.matrix * cubeTransform;
			Gizmos.color = color.fill;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = color.wire;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = oldGizmosMatrix;
		}

		/// <summary>
        /// Draw a gizmo with a given mesh and specified transformation. Editor only.
        /// </summary>
        /// <param name="positionWS">Gizmo position in world space.</param>
		/// <param name="rotationWS">Gizmo rotation in world space.</param>
		/// <param name="scaleWS">Gizmo space in world space.</param>
		/// <param name="mesh">Gizmo mesh.</param>
		/// <param name="color">Gizmo colors.</param>
		/// <param name="submeshIndex">Index for submesh to use.</param>
		public static void DrawMesh(Vector3 positionWS, Quaternion rotationWS, Vector3 scaleWS, Mesh mesh, PortalDebugColor color, int submeshIndex = 0)
		{
			Gizmos.color = color.fill;
			Gizmos.DrawMesh(mesh, submeshIndex, positionWS, rotationWS, scaleWS);
			Gizmos.color = color.wire;
			Gizmos.DrawWireMesh(mesh, submeshIndex, positionWS, rotationWS, scaleWS);
		}
#endif
	}

	public static class PortalDebugColors
    {
        public static PortalDebugColor volume = new PortalDebugColor(new Vector4(0.43f, 0.81f, 0.96f, 1.0f), new Vector4(0.43f, 0.81f, 0.96f, 0.5f));
        public static PortalDebugColor occluder = new PortalDebugColor(new Vector4(0.99f, 0.82f, 0.64f, 1.0f), new Vector4(0.99f, 0.82f, 0.64f, 0.5f));
    }

    public struct PortalDebugColor
    {
        public PortalDebugColor(Vector4 wire, Vector4 fill)
        {
            this.wire = wire;
            this.fill = fill;
        }

        public Vector4 wire;
        public Vector4 fill;
    }
}
