using UnityEngine;

namespace kTools.Portals
{
	public static class DebugColors
    {
        public static DebugColor volume = new DebugColor(new Vector4(0.43f, 0.81f, 0.96f, 1.0f), new Vector4(0.43f, 0.81f, 0.96f, 0.5f));
        public static DebugColor occluder = new DebugColor(new Vector4(0.99f, 0.82f, 0.64f, 1.0f), new Vector4(0.99f, 0.82f, 0.64f, 0.5f));
    }

    public struct DebugColor
    {
        public DebugColor(Vector4 wire, Vector4 fill)
        {
            this.wire = wire;
            this.fill = fill;
        }

        public Vector4 wire;
        public Vector4 fill;
    }
}
