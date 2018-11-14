using UnityEngine;

namespace kTools.Portals
{
	public static class PortalDebugUtils
	{
		public static void DrawDebugCube(Vector3 position, Quaternion rotation, Vector3 scale, PortalDebugColor color)
		{
			Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
			Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
			Gizmos.matrix = Gizmos.matrix * cubeTransform;
			Gizmos.color = color.fill;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = color.wire;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = oldGizmosMatrix;
		}
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
